using Microsoft.AspNetCore.Mvc;
using VendorManagementSystem.Server.Services;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.Auth;

namespace VendorManagementSystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<AuthResponse>.Fail("Login failed", result.Errors));

        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<AuthResponse>.Fail("Registration failed", result.Errors));

        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    [HttpGet("user-info")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<ApiResponse<UserInfo>>> GetUserInfo()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<UserInfo>.Fail("Not authenticated"));

        var userInfo = await _authService.GetUserInfoAsync(userId);
        if (userInfo == null)
            return NotFound(ApiResponse<UserInfo>.Fail("User not found"));

        return Ok(ApiResponse<UserInfo>.Ok(userInfo));
    }
}
