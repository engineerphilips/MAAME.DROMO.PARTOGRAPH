using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Services;

/// <summary>
/// Service for handling authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user and generates a JWT token
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="password">User password</param>
    /// <param name="deviceId">Device identifier for the mobile app</param>
    /// <returns>Authentication result with token if successful</returns>
    Task<AuthResult> AuthenticateAsync(string email, string password, string deviceId);

    /// <summary>
    /// Authenticates a device for sync operations using device credentials
    /// </summary>
    /// <param name="deviceId">Device identifier</param>
    /// <param name="deviceSecret">Device secret/API key</param>
    /// <returns>Authentication result with token if successful</returns>
    Task<AuthResult> AuthenticateDeviceAsync(string deviceId, string deviceSecret);

    /// <summary>
    /// Refreshes an expired token
    /// </summary>
    /// <param name="refreshToken">The refresh token</param>
    /// <returns>New authentication result with fresh tokens</returns>
    Task<AuthResult> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Validates a JWT token
    /// </summary>
    /// <param name="token">The JWT token to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    Task<bool> ValidateTokenAsync(string token);

    /// <summary>
    /// Generates a JWT token for a staff member
    /// </summary>
    /// <param name="staff">The staff member</param>
    /// <param name="deviceId">Device identifier</param>
    /// <returns>JWT token string</returns>
    string GenerateToken(Staff staff, string deviceId);

    /// <summary>
    /// Generates a refresh token
    /// </summary>
    /// <returns>Refresh token string</returns>
    string GenerateRefreshToken();
}

/// <summary>
/// Result of an authentication operation
/// </summary>
public class AuthResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? StaffId { get; set; }
    public string? StaffName { get; set; }
    public string? FacilityId { get; set; }
    public string? FacilityName { get; set; }
}

/// <summary>
/// Request model for login
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for device authentication
/// </summary>
public class DeviceAuthRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceSecret { get; set; } = string.Empty;
}

/// <summary>
/// Request model for token refresh
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
