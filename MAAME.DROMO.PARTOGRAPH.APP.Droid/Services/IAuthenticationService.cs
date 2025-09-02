
namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    public interface IAuthenticationService
    {
        string GetCurrentStaffName();
        bool IsAuthenticated();
        Task<bool> LoginAsync(string emailOrStaffId, string password);
        Task LogoutAsync();
    }
}