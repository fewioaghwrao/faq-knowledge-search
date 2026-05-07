using FaqApp.Api.Dtos.Auth;
using FaqApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FaqApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result is null)
        {
            return Unauthorized(new { message = "メールアドレスまたはパスワードが正しくありません。" });
        }

        return Ok(result);
    }
}