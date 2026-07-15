using System.Threading.Tasks;
using FamilyAI.Application.Common.Interfaces;
using FamilyAI.Contracts.Auth;
using FamilyAI.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace FamilyAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        if (!response.Success)
        {
            return Unauthorized(response);
        }
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }
}
