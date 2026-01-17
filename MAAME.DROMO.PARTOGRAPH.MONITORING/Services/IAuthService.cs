using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(LoginRequest request);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<UserSession?> GetCurrentSessionAsync();
        Task<bool> RefreshTokenAsync();
    }
}
