using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly StaffRepository _staffRepository;
        private readonly IAuthApiClient _authApiClient;
        private readonly ITokenStorageService _tokenStorageService;
        private readonly ILogger<AuthenticationService> _logger;

        // Semaphore to prevent concurrent token refresh operations
        private readonly SemaphoreSlim _refreshLock = new(1, 1);

        public AuthenticationService(
            StaffRepository staffRepository,
            IAuthApiClient authApiClient,
            ITokenStorageService tokenStorageService,
            ILogger<AuthenticationService> logger)
        {
            _staffRepository = staffRepository;
            _authApiClient = authApiClient;
            _tokenStorageService = tokenStorageService;
            _logger = logger;
        }

        /// <summary>
        /// Login with email/staffId and password. Tries API login first, falls back to local database.
        /// </summary>
        public async Task<bool> LoginAsync(string emailOrStaffId, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(emailOrStaffId) || string.IsNullOrEmpty(password))
                    return false;

                // Try API login first
                var apiResult = await LoginWithApiAsync(emailOrStaffId, password);
                if (apiResult.Success)
                {
                    return true;
                }

                // If API failed due to network, try local authentication
                if (apiResult.IsNetworkError)
                {
                    _logger.LogInformation("API login failed due to network, trying local authentication");
                    return await LoginLocalAsync(emailOrStaffId, password);
                }

                // If invalid credentials on API, still try local (for offline-first scenario)
                if (apiResult.IsInvalidCredentials)
                {
                    _logger.LogInformation("API credentials invalid, trying local authentication");
                    return await LoginLocalAsync(emailOrStaffId, password);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return false;
            }
        }

        /// <summary>
        /// Login using API authentication only
        /// </summary>
        public async Task<LoginResult> LoginWithApiAsync(string email, string password)
        {
            try
            {
                var result = await _authApiClient.LoginAsync(email, password);

                if (result == null)
                {
                    return LoginResult.Failed("Invalid credentials", isInvalidCredentials: true);
                }

                // Store tokens
                await _tokenStorageService.StoreTokensAsync(new AuthTokens
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    ExpiresAt = result.ExpiresAt,
                    StaffId = result.StaffId,
                    StaffName = result.StaffName,
                    FacilityId = result.FacilityId,
                    FacilityName = result.FacilityName
                });

                // Store authentication state in preferences
                Preferences.Set("IsAuthenticated", true);
                Preferences.Set("StaffName", result.StaffName);
                Preferences.Set("StaffId", result.StaffId);
                Preferences.Set("FacilityId", result.FacilityId);
                Preferences.Set("FacilityName", result.FacilityName);
                Preferences.Set("LastLogin", DateTime.Now.ToString("O"));
                Preferences.Set("AuthMethod", "API");

                // Try to get staff from local database to populate Constants.Staff
                if (Guid.TryParse(result.StaffId, out var staffGuid))
                {
                    var staff = await _staffRepository.GetByIdAsync(staffGuid);
                    if (staff != null)
                    {
                        Constants.Staff = staff;
                    }
                    else
                    {
                        // Create a minimal staff object from API response
                        Constants.Staff = new MODEL.Staff
                        {
                            ID = staffGuid,
                            FacilityName = result.FacilityName,
                            Facility = Guid.TryParse(result.FacilityId, out var facilityGuid) ? facilityGuid : Guid.Empty,
                            Email = email,
                            IsActive = true,
                            LastLogin = DateTime.Now
                        };
                    }
                }

                _logger.LogInformation("API login successful for {Email}", email);
                return LoginResult.Successful();
            }
            catch (AuthenticationException ex) when (ex.InnerException is HttpRequestException)
            {
                _logger.LogWarning(ex, "Network error during API login");
                return LoginResult.Failed(ex.Message, isNetworkError: true);
            }
            catch (AuthenticationException ex)
            {
                _logger.LogWarning(ex, "Authentication error during API login");
                return LoginResult.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during API login");
                return LoginResult.Failed("An unexpected error occurred", isNetworkError: true);
            }
        }

        /// <summary>
        /// Local database authentication (offline mode)
        /// </summary>
        private async Task<bool> LoginLocalAsync(string emailOrStaffId, string password)
        {
            try
            {
                // Check hardcoded system credentials
                if (emailOrStaffId == "system" && password == "systempassword")
                {
                    Preferences.Set("IsAuthenticated", true);
                    Preferences.Set("StaffName", "Administrator");
                    Preferences.Set("StaffRole", "Super-Admin");
                    Preferences.Set("StaffId", "2bef74f2-a5cd-4f66-99fe-e36a08b29613");
                    Preferences.Set("LastLogin", DateTime.Now.ToString("O"));
                    Preferences.Set("AuthMethod", "Local");

                    Constants.Staff = new MODEL.Staff()
                    {
                        ID = new Guid("2bef74f2-a5cd-4f66-99fe-e36a08b29613"),
                        FacilityName = "SUPER-ADMIN",
                        Role = "SUPER-ADMIN",
                        StaffID = "SUPER",
                        Email = "super@emperorsoftware.co",
                        IsActive = true,
                        Department = "Labour Ward",
                        LastLogin = DateTime.Now,
                    };
                    return true;
                }

                // Try local database authentication
                var staff = await _staffRepository.AuthenticateAsync(emailOrStaffId, password);
                if (staff != null)
                {
                    Preferences.Set("IsAuthenticated", true);
                    Preferences.Set("StaffName", staff.FacilityName);
                    Preferences.Set("StaffRole", staff.Role);
                    Preferences.Set("StaffId", staff.ID.ToString());
                    Preferences.Set("LastLogin", DateTime.Now.ToString("O"));
                    Preferences.Set("AuthMethod", "Local");
                    Constants.Staff = staff;
                    return true;
                }

                // Clear authentication state on failure
                ClearAuthState();
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during local authentication");
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                // Clear stored tokens
                await _tokenStorageService.ClearTokensAsync();

                // Clear authentication state
                ClearAuthState();

                _logger.LogInformation("User logged out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }
        }

        private void ClearAuthState()
        {
            Preferences.Remove("IsAuthenticated");
            Preferences.Remove("StaffName");
            Preferences.Remove("StaffRole");
            Preferences.Remove("StaffId");
            Preferences.Remove("LastLogin");
            Preferences.Remove("FacilityId");
            Preferences.Remove("FacilityName");
            Preferences.Remove("AuthMethod");
            Constants.Staff = null;
        }

        public bool IsAuthenticated()
        {
            return Preferences.Get("IsAuthenticated", false);
        }

        public string GetCurrentStaffName()
        {
            return Preferences.Get("StaffName", "Staff");
        }

        /// <summary>
        /// Gets a valid access token, automatically refreshing if expired
        /// </summary>
        public async Task<string?> GetValidAccessTokenAsync()
        {
            // Check if we're using local auth (no API tokens)
            var authMethod = Preferences.Get("AuthMethod", "Local");
            if (authMethod == "Local")
            {
                _logger.LogDebug("Using local authentication, no JWT token available");
                return null;
            }

            // Try to get existing token
            var token = await _tokenStorageService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }

            // Token is expired or missing, try to refresh
            _logger.LogInformation("Access token expired or missing, attempting refresh");

            if (await RefreshAccessTokenAsync())
            {
                return await _tokenStorageService.GetAccessTokenAsync();
            }

            // Refresh failed - user needs to login again
            _logger.LogWarning("Token refresh failed, user needs to re-authenticate");
            return null;
        }

        /// <summary>
        /// Refreshes the access token using stored refresh token
        /// </summary>
        public async Task<bool> RefreshAccessTokenAsync()
        {
            // Prevent concurrent refresh operations
            await _refreshLock.WaitAsync();
            try
            {
                // Double-check if token was already refreshed by another call
                var currentToken = await _tokenStorageService.GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(currentToken))
                {
                    return true;
                }

                var refreshToken = await _tokenStorageService.GetRefreshTokenAsync();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("No refresh token available");
                    return false;
                }

                var result = await _authApiClient.RefreshTokenAsync(refreshToken);
                if (result == null)
                {
                    _logger.LogWarning("Token refresh returned null");
                    return false;
                }

                // Store new tokens
                await _tokenStorageService.StoreTokensAsync(new AuthTokens
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    ExpiresAt = result.ExpiresAt,
                    StaffId = result.StaffId,
                    StaffName = result.StaffName,
                    FacilityId = result.FacilityId,
                    FacilityName = result.FacilityName
                });

                _logger.LogInformation("Token refresh successful");
                return true;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        public bool HasValidTokens()
        {
            var authMethod = Preferences.Get("AuthMethod", "Local");
            if (authMethod == "Local")
            {
                return false;
            }

            return _tokenStorageService.HasValidTokens();
        }
    }
}
