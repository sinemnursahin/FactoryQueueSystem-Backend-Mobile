using FactoryQueueSystem.Api.DTOs.Auth;
using FactoryQueueSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryQueueSystem.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request) => ToActionResult(await authService.RegisterDriverAsync(request));

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request) => ToActionResult(await authService.LoginAsync(request));

    private IActionResult ToActionResult<T>(ServiceResult<T> result) =>
        result.Succeeded ? Ok(result.Value) : StatusCode(result.StatusCode, new { message = result.Error });
}
