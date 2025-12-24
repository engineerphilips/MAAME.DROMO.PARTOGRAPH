using System.Text.Json;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;

/// <summary>
/// Service for secure storage and management of JWT tokens
/// </summary>
public interface ITokenStorageService
{
    /// <summary>
    /// Gets the current access token if valid
    /// </summary>
    Task<string?> GetAccessTokenAsync();

    /// <summary>
    /// Gets the refresh token
    /// </summary>
    Task<string?> GetRefreshTokenAsync();

    /// <summary>
    /// Stores authentication tokens securely
    /// </summary>
    Task StoreTokensAsync(AuthTokens tokens);

    /// <summary>
    /// Clears all stored tokens
    /// </summary>
    Task ClearTokensAsync();

    /// <summary>
    /// Checks if access token is expired or about to expire
    /// </summary>
    bool IsAccessTokenExpired();

    /// <summary>
    /// Checks if user has valid tokens stored
    /// </summary>
    bool HasValidTokens();

    /// <summary>
    /// Gets the token expiration time
    /// </summary>
    DateTime? GetTokenExpiration();
}

/// <summary>
/// JWT token pair with expiration info
/// </summary>
public class AuthTokens
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public string FacilityId { get; set; } = string.Empty;
    public string FacilityName { get; set; } = string.Empty;
}

/// <summary>
/// Implementation of token storage using SecureStorage for sensitive data
/// </summary>
public class TokenStorageService : ITokenStorageService
{
    private const string AccessTokenKey = "jwt_access_token";
    private const string RefreshTokenKey = "jwt_refresh_token";
    private const string TokenExpirationKey = "jwt_token_expiration";
    private const string TokenMetadataKey = "jwt_token_metadata";

    // Buffer time before expiration to trigger refresh (5 minutes)
    private static readonly TimeSpan ExpirationBuffer = TimeSpan.FromMinutes(5);

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            // Check if token is expired first
            if (IsAccessTokenExpired())
            {
                return null;
            }

            return await SecureStorage.GetAsync(AccessTokenKey);
        }
        catch (Exception)
        {
            // SecureStorage may throw on some platforms if not properly configured
            return null;
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync(RefreshTokenKey);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task StoreTokensAsync(AuthTokens tokens)
    {
        try
        {
            // Store tokens in secure storage
            await SecureStorage.SetAsync(AccessTokenKey, tokens.AccessToken);
            await SecureStorage.SetAsync(RefreshTokenKey, tokens.RefreshToken);

            // Store expiration in preferences (not sensitive)
            Preferences.Set(TokenExpirationKey, tokens.ExpiresAt.ToString("O"));

            // Store metadata for user context
            var metadata = JsonSerializer.Serialize(new
            {
                tokens.StaffId,
                tokens.StaffName,
                tokens.FacilityId,
                tokens.FacilityName
            });
            Preferences.Set(TokenMetadataKey, metadata);
        }
        catch (Exception)
        {
            // Log error but don't throw - fallback to preferences if secure storage fails
            Preferences.Set(AccessTokenKey, tokens.AccessToken);
            Preferences.Set(RefreshTokenKey, tokens.RefreshToken);
            Preferences.Set(TokenExpirationKey, tokens.ExpiresAt.ToString("O"));
        }
    }

    public async Task ClearTokensAsync()
    {
        try
        {
            SecureStorage.Remove(AccessTokenKey);
            SecureStorage.Remove(RefreshTokenKey);
        }
        catch (Exception)
        {
            // Fallback for preferences storage
            Preferences.Remove(AccessTokenKey);
            Preferences.Remove(RefreshTokenKey);
        }

        Preferences.Remove(TokenExpirationKey);
        Preferences.Remove(TokenMetadataKey);

        await Task.CompletedTask;
    }

    public bool IsAccessTokenExpired()
    {
        var expiration = GetTokenExpiration();
        if (expiration == null)
        {
            return true;
        }

        // Consider expired if within buffer time of expiration
        return DateTime.UtcNow >= expiration.Value.Subtract(ExpirationBuffer);
    }

    public bool HasValidTokens()
    {
        if (IsAccessTokenExpired())
        {
            // Check if we have a refresh token
            var refreshToken = Preferences.Get(RefreshTokenKey, string.Empty);
            if (string.IsNullOrEmpty(refreshToken))
            {
                try
                {
                    // Try secure storage synchronously through a workaround
                    var task = SecureStorage.GetAsync(RefreshTokenKey);
                    task.Wait();
                    return !string.IsNullOrEmpty(task.Result);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        return true;
    }

    public DateTime? GetTokenExpiration()
    {
        var expirationStr = Preferences.Get(TokenExpirationKey, string.Empty);
        if (string.IsNullOrEmpty(expirationStr))
        {
            return null;
        }

        if (DateTime.TryParse(expirationStr, out var expiration))
        {
            return expiration.ToUniversalTime();
        }

        return null;
    }
}
