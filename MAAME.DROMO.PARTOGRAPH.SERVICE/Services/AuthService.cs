using MAAME.DROMO.PARTOGRAPH.MODEL;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Services;

/// <summary>
/// Implementation of authentication service using JWT tokens
/// </summary>
public class AuthService : IAuthService
{
    private readonly PartographDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        PartographDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<AuthResult> AuthenticateAsync(string email, string password, string deviceId)
    {
        try
        {
            // Find staff member by email
            var staff = await _context.Staff
                .FirstOrDefaultAsync(s => s.Email == email && s.Deleted == 0);

            if (staff == null)
            {
                _logger.LogWarning("Authentication failed: Staff not found for email {Email}", email);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Invalid credentials"
                };
            }

            // Verify password (using simple hash for now - consider using proper password hashing like BCrypt)
            if (!VerifyPassword(password, staff.PasswordHash))
            {
                _logger.LogWarning("Authentication failed: Invalid password for email {Email}", email);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Invalid credentials"
                };
            }

            // Generate tokens
            var accessToken = GenerateToken(staff, deviceId);
            var refreshToken = GenerateRefreshToken();

            // Store refresh token (in production, store in database with expiration)
            staff.LastLoginTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await _context.SaveChangesAsync();

            var expirationMinutes = _configuration.GetValue<int>("JwtSettings:ExpirationMinutes", 60);

            _logger.LogInformation("Staff {StaffId} authenticated successfully from device {DeviceId}", staff.ID, deviceId);

            return new AuthResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                StaffId = staff.ID,
                StaffName = $"{staff.FirstName} {staff.LastName}",
                FacilityId = staff.Facility?.ToString(),
                FacilityName = staff.FacilityName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for email {Email}", email);
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Authentication failed"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<AuthResult> AuthenticateDeviceAsync(string deviceId, string deviceSecret)
    {
        try
        {
            // Validate device secret (this could be an API key stored in configuration)
            var validDeviceSecret = Environment.GetEnvironmentVariable("PARTOGRAPH_DEVICE_SECRET")
                ?? _configuration["SyncSettings:DeviceSecret"];

            if (string.IsNullOrEmpty(validDeviceSecret))
            {
                _logger.LogWarning("Device authentication not configured");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Device authentication not configured"
                };
            }

            if (deviceSecret != validDeviceSecret)
            {
                _logger.LogWarning("Device authentication failed: Invalid device secret for device {DeviceId}", deviceId);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Invalid device credentials"
                };
            }

            // Generate a device-level token
            var accessToken = GenerateDeviceToken(deviceId);
            var refreshToken = GenerateRefreshToken();

            var expirationMinutes = _configuration.GetValue<int>("JwtSettings:ExpirationMinutes", 60);

            _logger.LogInformation("Device {DeviceId} authenticated successfully", deviceId);

            return new AuthResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during device authentication for device {DeviceId}", deviceId);
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Device authentication failed"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        // In a full implementation, validate the refresh token against stored tokens
        // For now, generate a new token pair
        try
        {
            // This is a simplified implementation
            // In production, you should:
            // 1. Validate the refresh token
            // 2. Check if it's expired or revoked
            // 3. Extract the original claims
            // 4. Generate new tokens

            var newRefreshToken = GenerateRefreshToken();
            var expirationMinutes = _configuration.GetValue<int>("JwtSettings:ExpirationMinutes", 60);

            return await Task.FromResult(new AuthResult
            {
                Success = true,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                ErrorMessage = "Please re-authenticate to get a new access token"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Failed to refresh token"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = Environment.GetEnvironmentVariable("PARTOGRAPH_JWT_SECRET")
                ?? _configuration["JwtSettings:SecretKey"];

            if (string.IsNullOrEmpty(secretKey))
                return false;

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };

            tokenHandler.ValidateToken(token, validationParameters, out _);
            return await Task.FromResult(true);
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public string GenerateToken(Staff staff, string deviceId)
    {
        var secretKey = Environment.GetEnvironmentVariable("PARTOGRAPH_JWT_SECRET")
            ?? _configuration["JwtSettings:SecretKey"];

        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JWT secret key not configured");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, staff.ID?.ToString() ?? Guid.Empty.ToString()),
            new Claim(ClaimTypes.Email, staff.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, $"{staff.FirstName} {staff.LastName}"),
            new Claim("facility_id", staff.Facility?.ToString() ?? string.Empty),
            new Claim("facility_name", staff.FacilityName ?? string.Empty),
            new Claim("device_id", deviceId),
            new Claim("role", staff.Role ?? "User"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expirationMinutes = _configuration.GetValue<int>("JwtSettings:ExpirationMinutes", 60);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a token for device-level authentication (no specific user)
    /// </summary>
    private string GenerateDeviceToken(string deviceId)
    {
        var secretKey = Environment.GetEnvironmentVariable("PARTOGRAPH_JWT_SECRET")
            ?? _configuration["JwtSettings:SecretKey"];

        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JWT secret key not configured");

        var claims = new List<Claim>
        {
            new Claim("device_id", deviceId),
            new Claim("auth_type", "device"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expirationMinutes = _configuration.GetValue<int>("JwtSettings:ExpirationMinutes", 60);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc/>
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Verifies a password against a stored hash
    /// </summary>
    private bool VerifyPassword(string password, string? storedHash)
    {
        if (string.IsNullOrEmpty(storedHash))
            return false;

        // Simple SHA256 hash verification
        // In production, use BCrypt or Argon2
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hashedPassword = Convert.ToBase64String(hashedBytes);

        return hashedPassword == storedHash;
    }

}
