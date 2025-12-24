using System.Net.Http.Json;
using System.Text.Json;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;

#region API Models

/// <summary>
/// Request model for user login
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for token refresh
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Response model from authentication endpoints
/// </summary>
public class AuthResult
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
/// Error response from API
/// </summary>
public class ApiErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

#endregion

/// <summary>
/// Interface for authentication API operations
/// </summary>
public interface IAuthApiClient
{
    /// <summary>
    /// Authenticates user with email and password
    /// </summary>
    Task<AuthResult?> LoginAsync(string email, string password);

    /// <summary>
    /// Refreshes the access token using refresh token
    /// </summary>
    Task<AuthResult?> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Validates the current access token with the server
    /// </summary>
    Task<bool> ValidateTokenAsync(string accessToken);

    /// <summary>
    /// Tests connection to the auth server
    /// </summary>
    Task<bool> TestConnectionAsync();
}

/// <summary>
/// HTTP client for authentication API calls
/// </summary>
public class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthApiClient(HttpClient httpClient, ILogger<AuthApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<AuthResult?> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginRequest
            {
                Email = email,
                Password = password,
                DeviceId = DeviceIdentity.GetOrCreateDeviceId()
            };

            _logger.LogInformation("Attempting API login for {Email}", email);

            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResult>(_jsonOptions);
                _logger.LogInformation("API login successful for {Email}", email);
                return result;
            }

            // Handle error response
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("API login failed with status {StatusCode}: {Error}",
                response.StatusCode, errorContent);

            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during API login");
            throw new AuthenticationException("Unable to connect to authentication server. Please check your network connection.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout during API login");
            throw new AuthenticationException("Authentication request timed out. Please try again.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during API login");
            throw new AuthenticationException("An unexpected error occurred during login.", ex);
        }
    }

    public async Task<AuthResult?> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var request = new RefreshTokenRequest { RefreshToken = refreshToken };

            _logger.LogInformation("Attempting to refresh access token");

            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", request, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResult>(_jsonOptions);
                _logger.LogInformation("Token refresh successful");
                return result;
            }

            _logger.LogWarning("Token refresh failed with status {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return null;
        }
    }

    public async Task<bool> ValidateTokenAsync(string accessToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/validate");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return false;
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/sync/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Connection test failed");
            return false;
        }
    }
}

/// <summary>
/// Exception thrown when authentication fails
/// </summary>
public class AuthenticationException : Exception
{
    public AuthenticationException(string message) : base(message) { }
    public AuthenticationException(string message, Exception innerException) : base(message, innerException) { }
}
