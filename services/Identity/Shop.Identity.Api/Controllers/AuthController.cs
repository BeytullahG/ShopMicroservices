using Shop.Identity.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Shop.Identity.Api.Entities;
using FluentValidation;

namespace Shop.Identity.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(UserManager<AppUser> userManager, IValidator<RegisterRequest> validator) : ControllerBase 
{

[HttpPost("register")]
public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
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
}
    