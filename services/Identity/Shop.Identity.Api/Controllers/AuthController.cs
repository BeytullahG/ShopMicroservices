using Shop.Identity.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Shop.Identity.Api.Entities;
using FluentValidation;
using Shop.Identity.Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace Shop.Identity.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
        UserManager<AppUser> userManager, 
        SignInManager<AppUser> signInMnager,
        ITokenService tokenService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator) : ControllerBase 
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

            var user = new AppUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var CreateResult = await userManager.CreateAsync(user, request.Password);
            if (!CreateResult.Succeeded)
            {
                foreach(var error in CreateResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem(ModelState);
            }
            

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

        var singInResult = await signInMnager.CheckPasswordSignInAsync(user, request.Password, true);
        if (!singInResult.Succeeded)
        {
            return InvalidCredentials();
        }

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.CreateAccessToken(user, roles);
        return Ok(new TokenResponce(accessToken.Token, accessToken.ExpiresAt));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var claims = User.Claims.Select(c => new {c.Type, c.Value});
        return Ok(claims);
    }

    public ObjectResult InvalidCredentials() => 
        Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid email or password");
}
    