using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    /// <summary>
    /// Service interface for user management operations
    /// </summary>
    public interface IUserService
    {
        Task<List<MonitoringUserSummary>> GetAllUsersAsync();
        Task<MonitoringUserSummary?> GetUserAsync(Guid userId);
        Task<(bool Success, string Message)> CreateUserAsync(CreateUserRequest request);
        Task<(bool Success, string Message)> UpdateUserAsync(UpdateUserRequest request);
        Task<(bool Success, string Message)> DeleteUserAsync(Guid userId);
        Task<(bool Success, string Message)> ActivateUserAsync(Guid userId);
        Task<(bool Success, string Message)> DeactivateUserAsync(Guid userId);
        Task<(bool Success, string Message)> UnlockUserAsync(Guid userId);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request);
        Task<UserStatistics> GetUserStatisticsAsync();
        Task<List<RegionDropdownItem>> GetRegionsDropdownAsync();
        Task<List<DistrictDropdownItem>> GetDistrictsDropdownAsync(Guid? regionId = null);
    }
}
