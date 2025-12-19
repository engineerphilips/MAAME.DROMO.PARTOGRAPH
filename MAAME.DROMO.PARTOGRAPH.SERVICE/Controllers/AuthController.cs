using MAAME.DROMO.PARTOGRAPH.SERVICE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Controllers;

/// <summary>
/// Controller for authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a staff member and returns a JWT token
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new AuthResult
            {
                Success = false,
                ErrorMessage = "Email and password are required"
            });
        }

        var result = await _authService.AuthenticateAsync(
            request.Email,
            request.Password,
            request.DeviceId);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Authenticates a device for sync operations
    /// </summary>
    [HttpPost("device")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> AuthenticateDevice([FromBody] DeviceAuthRequest request)
    {
        if (string.IsNullOrEmpty(request.DeviceId) || string.IsNullOrEmpty(request.DeviceSecret))
        {
            return BadRequest(new AuthResult
            {
                Success = false,
                ErrorMessage = "Device ID and secret are required"
            });
        }

        var result = await _authService.AuthenticateDeviceAsync(
            request.DeviceId,
            request.DeviceSecret);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Refreshes an expired access token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest(new AuthResult
            {
                Success = false,
                ErrorMessage = "Refresh token is required"
            });
        }

        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Validates the current token (can be used to check if token is still valid)
    /// </summary>
    [HttpGet("validate")]
    [Authorize]
    public IActionResult ValidateToken()
    {
        // If we get here, the token is valid (Authorize attribute handles validation)
        return Ok(new { valid = true, message = "Token is valid" });
    }

    /// <summary>
    /// Gets the current user's information from the token
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var facilityId = User.FindFirst("facility_id")?.Value;
        var facilityName = User.FindFirst("facility_name")?.Value;
        var deviceId = User.FindFirst("device_id")?.Value;
        var role = User.FindFirst("role")?.Value;

        return Ok(new
        {
            userId,
            email,
            name,
            facilityId,
            facilityName,
            deviceId,
            role
        });
    }
}
