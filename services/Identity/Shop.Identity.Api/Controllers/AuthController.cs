using Shop.Identity.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Shop.Identity.Api.Entities;
using FluentValidation;
using Shop.Identity.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Shop.Shared.Constants;
using Shop.Identity.Api.Data;

namespace Shop.Identity.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ITokenService tokenService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        AppDbContext db) : ControllerBase
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
        return Ok(new TokenResponse(accessToken.Token, accessToken.ExpiresAt));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(claims);
    }

    private ObjectResult InvalidCredentials() =>
        Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid email or password");
}
