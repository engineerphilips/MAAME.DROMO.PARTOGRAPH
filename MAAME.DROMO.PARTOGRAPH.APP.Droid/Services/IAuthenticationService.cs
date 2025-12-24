namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Gets the current staff name
        /// </summary>
        string GetCurrentStaffName();

        /// <summary>
        /// Checks if user is authenticated
        /// </summary>
        bool IsAuthenticated();

        /// <summary>
        /// Logs in with email/staffId and password (tries API first, falls back to local)
        /// </summary>
        Task<bool> LoginAsync(string emailOrStaffId, string password);

        /// <summary>
        /// Logs in using API authentication only
        /// </summary>
        Task<LoginResult> LoginWithApiAsync(string email, string password);

        /// <summary>
        /// Logs out and clears all tokens
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// Gets a valid access token, refreshing if necessary
        /// </summary>
        Task<string?> GetValidAccessTokenAsync();

        /// <summary>
        /// Refreshes the access token using stored refresh token
        /// </summary>
        Task<bool> RefreshAccessTokenAsync();

        /// <summary>
        /// Checks if user has valid JWT tokens
        /// </summary>
        bool HasValidTokens();
    }

    /// <summary>
    /// Result of login attempt with detailed status
    /// </summary>
    public class LoginResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsNetworkError { get; set; }
        public bool IsInvalidCredentials { get; set; }

        public static LoginResult Successful() => new() { Success = true };

        public static LoginResult Failed(string message, bool isNetworkError = false, bool isInvalidCredentials = false) =>
            new()
            {
                Success = false,
                ErrorMessage = message,
                IsNetworkError = isNetworkError,
                IsInvalidCredentials = isInvalidCredentials
            };
    }
}
