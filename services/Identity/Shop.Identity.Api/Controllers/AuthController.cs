using Shop.Identity.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Identity.Api.Controllers;
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase {

[HttpPost("register")]
public IActionResult Register(RegisterRequest request, CancellationToken ct)
    {
        return Ok();
    }
}
    