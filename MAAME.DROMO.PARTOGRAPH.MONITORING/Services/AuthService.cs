using System.Net.Http.Json;
using Blazored.LocalStorage;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private const string SessionKey = "monitoring_session";

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/monitoring/auth/login", new
                {
                    email = request.Email,
                    password = request.Password
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiLoginResponse>();

                    if (result?.Success == true && result.User != null)
                    {
                        var userDto = new MonitoringUserDto
                        {
                            ID = result.User.Id,
                            FullName = result.User.FullName,
                            Email = result.User.Email,
                            AccessLevel = result.User.AccessLevel,
                            Role = result.User.Role,
                            RegionID = result.User.RegionId,
                            RegionName = result.User.RegionName,
                            DistrictID = result.User.DistrictId,
                            DistrictName = result.User.DistrictName
                        };

                        var session = new UserSession
                        {
                            Token = result.Token ?? "",
                            RefreshToken = result.RefreshToken ?? "",
                            User = userDto,
                            ExpiresAt = DateTime.UtcNow.AddHours(8)
                        };

                        await _localStorage.SetItemAsync(SessionKey, session);

                        return new LoginResult
                        {
                            Success = true,
                            Token = result.Token,
                            RefreshToken = result.RefreshToken,
                            User = userDto
                        };
                    }
                }

                var errorContent = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = errorContent?.Error ?? "Login failed. Please try again."
                };
            }
            catch (Exception ex)
            {
                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _localStorage.RemoveItemAsync(SessionKey);
            }
            catch
            {
                // Ignore errors during logout
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var session = await _localStorage.GetItemAsync<UserSession>(SessionKey);
                return session != null && session.ExpiresAt > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserSession?> GetCurrentSessionAsync()
        {
            try
            {
                return await _localStorage.GetItemAsync<UserSession>(SessionKey);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                var session = await _localStorage.GetItemAsync<UserSession>(SessionKey);
                if (session == null || string.IsNullOrEmpty(session.RefreshToken))
                    return false;

                var response = await _httpClient.PostAsJsonAsync("api/monitoring/auth/refresh", new
                {
                    refreshToken = session.RefreshToken
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiRefreshResponse>();
                    if (result != null)
                    {
                        session.Token = result.Token ?? session.Token;
                        session.RefreshToken = result.RefreshToken ?? session.RefreshToken;
                        session.ExpiresAt = DateTime.UtcNow.AddHours(8);

                        await _localStorage.SetItemAsync(SessionKey, session);
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    // API Response models
    internal class ApiLoginResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public ApiUserResponse? User { get; set; }
    }

    internal class ApiUserResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string AccessLevel { get; set; } = "";
        public string Role { get; set; } = "";
        public Guid? RegionId { get; set; }
        public string? RegionName { get; set; }
        public Guid? DistrictId { get; set; }
        public string? DistrictName { get; set; }
    }

    internal class ApiRefreshResponse
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }

    //internal class ApiErrorResponse
    //{
    //    public string? Error { get; set; }
    //}
}
