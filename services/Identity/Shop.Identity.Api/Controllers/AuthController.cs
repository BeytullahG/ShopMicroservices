using Shop.Identity.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Shop.Identity.Api.Entities;
using FluentValidation;
using Shop.Identity.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Shop.Shared.Constants;
using Shop.Identity.Api.Data;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Shop.Identity.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ITokenService tokenService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        AppDbContext db,
        IOptions<RefreshTokenOptions> refreshOptions) : ControllerBase
{

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var validationResult = await registerValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return ValidationProblem(ModelState);
        }

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        var roleResult = await userManager.AddToRoleAsync(user, Roles.Customer);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to assign role {Roles.Customer} : {errors}");
        }
        await transaction.CommitAsync(ct);


        return StatusCode(StatusCodes.Status201Created, new RegisterResponse(user.Id, request.Email));

    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var validationResult = await loginValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return ValidationProblem(ModelState);
        }

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return InvalidCredentials();
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, true);
        if (!signInResult.Succeeded)
        {
            return InvalidCredentials();
        }

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.CreateAccessToken(user, roles);
        var refresh = tokenService.CreateRefreshToken();
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refresh.TokenHash,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = refresh.ExpiresAt

        });
        await db.SaveChangesAsync(ct);
        SetRefreshCookie(refresh.Token, refresh.ExpiresAt);
        return Ok(new TokenResponse(accessToken.Token, accessToken.ExpiresAt));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(claims);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin-ping")]
    public IActionResult AdminPing()
    {
        return Ok(new {message = "pong"});
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var token = Request.Cookies[refreshOptions.Value.CookieName];
        if(string.IsNullOrEmpty(token)) return RefreshFailed();
        var hash = tokenService.HashRefreshToken(token);
        
        var stored = await db.RefreshTokens
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (stored is null) return RefreshFailed();
        if (stored.RevokedAt is not null)
        {
            await db.RefreshTokens
                .Where(x => x.UserId == stored.UserId && x.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, now), ct);
            return RefreshFailed();
        }
        if (stored.ExpiresAt <= now) return RefreshFailed();
        if (await userManager.IsLockedOutAsync(stored.User)) return RefreshFailed();

        var newRefresh = tokenService.CreateRefreshToken();
        var newEntity = new RefreshToken
        {
            UserId = stored.UserId,
            TokenHash = newRefresh.TokenHash,
            CreatedAt = now,
            ExpiresAt = newRefresh.ExpiresAt
        };
        db.RefreshTokens.Add(newEntity);
        stored.RevokedAt = now;
        stored.ReplacedById = newEntity.Id;
        var roles = await userManager.GetRolesAsync(stored.User);
        var accessToken = tokenService.CreateAccessToken(stored.User, roles);
        await db.SaveChangesAsync(ct);
        SetRefreshCookie(newRefresh.Token, newRefresh.ExpiresAt);
        return Ok(new TokenResponse(accessToken.Token, accessToken.ExpiresAt));
    }

    private ObjectResult InvalidCredentials() =>
        Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid email or password");

    public ObjectResult RefreshFailed()
    {
        Response.Cookies.Delete(
            refreshOptions.Value.CookieName,
            new CookieOptions{Path = refreshOptions.Value.CookiePath}
        );
    
        return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid refresh token");
    }

    private void SetRefreshCookie(string token, DateTimeOffset expiresAt)
    {
        Response.Cookies.Append(refreshOptions.Value.CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true, 
            SameSite = SameSiteMode.Strict,
            Path = refreshOptions.Value.CookiePath,
            Expires = expiresAt
        });
    }
}
