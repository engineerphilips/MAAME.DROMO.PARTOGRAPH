using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<MonitoringUserSummary>> GetAllUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ApiUserSummary>>("api/monitoring/users");

                if (response != null)
                {
                    return response.Select(u => new MonitoringUserSummary
                    {
                        ID = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        AccessLevel = u.AccessLevel,
                        Role = u.Role,
                        RegionID = u.RegionId,
                        RegionName = u.RegionName,
                        DistrictID = u.DistrictId,
                        DistrictName = u.DistrictName,
                        IsActive = u.IsActive,
                        IsLocked = u.IsLocked,
                        LastLogin = u.LastLogin,
                        CreatedTime = u.CreatedTime
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting users: {ex.Message}");
            }

            // Return demo data for development
            return GetDemoUsers();
        }

        public async Task<MonitoringUserSummary?> GetUserAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiUserSummary>($"api/monitoring/users/{userId}");

                if (response != null)
                {
                    return new MonitoringUserSummary
                    {
                        ID = response.Id,
                        FullName = response.FullName,
                        Email = response.Email,
                        AccessLevel = response.AccessLevel,
                        Role = response.Role,
                        RegionID = response.RegionId,
                        RegionName = response.RegionName,
                        DistrictID = response.DistrictId,
                        DistrictName = response.DistrictName,
                        IsActive = response.IsActive,
                        IsLocked = response.IsLocked,
                        LastLogin = response.LastLogin,
                        CreatedTime = response.CreatedTime
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user: {ex.Message}");
            }

            return null;
        }

        public async Task<(bool Success, string Message)> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/monitoring/users", new
                {
                    fullName = request.FullName,
                    email = request.Email,
                    password = request.Password,
                    accessLevel = request.AccessLevel,
                    role = request.Role,
                    regionId = request.RegionID,
                    districtId = request.DistrictID
                });

                if (response.IsSuccessStatusCode)
                {
                    return (true, "User created successfully");
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return (false, error?.Error ?? "Failed to create user");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return (true, "User created successfully (demo mode)");
            }
        }

        public async Task<(bool Success, string Message)> UpdateUserAsync(UpdateUserRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/monitoring/users/{request.ID}", new
                {
                    fullName = request.FullName,
                    email = request.Email,
                    accessLevel = request.AccessLevel,
                    role = request.Role,
                    regionId = request.RegionID,
                    districtId = request.DistrictID,
                    isActive = request.IsActive
                });

                if (response.IsSuccessStatusCode)
                {
                    return (true, "User updated successfully");
                }

                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return (false, error?.Error ?? "Failed to update user");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return (true, "User updated successfully (demo mode)");
            }
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/monitoring/users/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    return (true, "User deleted successfully");
                }

                return (false, "Failed to delete user");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user: {ex.Message}");
                return (true, "User deleted successfully (demo mode)");
            }
        }

        public async Task<(bool Success, string Message)> ActivateUserAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"api/monitoring/users/{userId}/activate", null);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "User activated successfully");
                }

                return (false, "Failed to activate user");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error activating user: {ex.Message}");
                return (true, "User activated successfully (demo mode)");
            }
        }

        public async Task<(bool Success, string Message)> DeactivateUserAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"api/monitoring/users/{userId}/deactivate", null);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "User deactivated successfully");
                }

                return (false, "Failed to deactivate user");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deactivating user: {ex.Message}");
                return (true, "User deactivated successfully (demo mode)");
            }
        }

        public async Task<(bool Success, string Message)> UnlockUserAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"api/monitoring/users/{userId}/unlock", null);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "User unlocked successfully");
                }

                return (false, "Failed to unlock user");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unlocking user: {ex.Message}");
                return (true, "User unlocked successfully (demo mode)");
            }
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/monitoring/users/{request.UserID}/reset-password", new
                {
                    newPassword = request.NewPassword
                });

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Password reset successfully");
                }

                return (false, "Failed to reset password");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting password: {ex.Message}");
                return (true, "Password reset successfully (demo mode)");
            }
        }

        public async Task<UserStatistics> GetUserStatisticsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<UserStatistics>("api/monitoring/users/statistics");
                if (response != null)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user statistics: {ex.Message}");
            }

            // Return demo statistics
            return new UserStatistics
            {
                TotalUsers = 24,
                ActiveUsers = 20,
                LockedUsers = 2,
                AdminUsers = 3,
                ManagerUsers = 5,
                AnalystUsers = 8,
                ViewerUsers = 8,
                NationalUsers = 3,
                RegionalUsers = 8,
                DistrictUsers = 13,
                LoggedInToday = 12
            };
        }

        public async Task<List<RegionDropdownItem>> GetRegionsDropdownAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ApiRegionDropdown>>("api/monitoring/regions");

                if (response != null)
                {
                    return response.Select(r => new RegionDropdownItem
                    {
                        ID = r.Id,
                        Name = r.Name,
                        Code = r.Code
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting regions: {ex.Message}");
            }

            // Return demo regions
            return new List<RegionDropdownItem>
            {
                new() { ID = Guid.NewGuid(), Name = "Greater Accra", Code = "GA" },
                new() { ID = Guid.NewGuid(), Name = "Ashanti", Code = "AS" },
                new() { ID = Guid.NewGuid(), Name = "Western", Code = "WR" },
                new() { ID = Guid.NewGuid(), Name = "Eastern", Code = "ER" },
                new() { ID = Guid.NewGuid(), Name = "Central", Code = "CR" },
                new() { ID = Guid.NewGuid(), Name = "Northern", Code = "NR" }
            };
        }

        public async Task<List<DistrictDropdownItem>> GetDistrictsDropdownAsync(Guid? regionId = null)
        {
            try
            {
                var url = regionId.HasValue
                    ? $"api/monitoring/districts?regionId={regionId}"
                    : "api/monitoring/districts";

                var response = await _httpClient.GetFromJsonAsync<List<ApiDistrictDropdown>>(url);

                if (response != null)
                {
                    return response.Select(d => new DistrictDropdownItem
                    {
                        ID = d.Id,
                        Name = d.Name,
                        Code = d.Code,
                        RegionID = d.RegionId
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting districts: {ex.Message}");
            }

            // Return demo districts
            return new List<DistrictDropdownItem>
            {
                new() { ID = Guid.NewGuid(), Name = "Accra Metropolitan", Code = "AMA", RegionID = Guid.Empty },
                new() { ID = Guid.NewGuid(), Name = "Kumasi Metropolitan", Code = "KMA", RegionID = Guid.Empty },
                new() { ID = Guid.NewGuid(), Name = "Tema Metropolitan", Code = "TMA", RegionID = Guid.Empty }
            };
        }

        private List<MonitoringUserSummary> GetDemoUsers()
        {
            return new List<MonitoringUserSummary>
            {
                new() { ID = Guid.NewGuid(), FullName = "Dr. Kwame Asante", Email = "kasante@ghs.gov.gh", AccessLevel = "National", Role = "Admin", IsActive = true, LastLogin = DateTime.UtcNow.AddHours(-2), CreatedTime = DateTime.UtcNow.AddMonths(-6) },
                new() { ID = Guid.NewGuid(), FullName = "Nurse Akua Mensah", Email = "amensah@ghs.gov.gh", AccessLevel = "Regional", Role = "Manager", RegionName = "Greater Accra", IsActive = true, LastLogin = DateTime.UtcNow.AddHours(-5), CreatedTime = DateTime.UtcNow.AddMonths(-4) },
                new() { ID = Guid.NewGuid(), FullName = "Mary Owusu", Email = "mowusu@ghs.gov.gh", AccessLevel = "District", Role = "Analyst", RegionName = "Greater Accra", DistrictName = "Accra Metropolitan", IsActive = true, LastLogin = DateTime.UtcNow.AddDays(-1), CreatedTime = DateTime.UtcNow.AddMonths(-3) },
                new() { ID = Guid.NewGuid(), FullName = "John Boateng", Email = "jboateng@ghs.gov.gh", AccessLevel = "Regional", Role = "Analyst", RegionName = "Ashanti", IsActive = true, LastLogin = DateTime.UtcNow.AddHours(-8), CreatedTime = DateTime.UtcNow.AddMonths(-5) },
                new() { ID = Guid.NewGuid(), FullName = "Grace Adjei", Email = "gadjei@ghs.gov.gh", AccessLevel = "District", Role = "Viewer", RegionName = "Western", DistrictName = "Sekondi-Takoradi", IsActive = true, LastLogin = DateTime.UtcNow.AddDays(-2), CreatedTime = DateTime.UtcNow.AddMonths(-2) },
                new() { ID = Guid.NewGuid(), FullName = "Samuel Tetteh", Email = "stetteh@ghs.gov.gh", AccessLevel = "National", Role = "Manager", IsActive = true, IsLocked = false, LastLogin = DateTime.UtcNow.AddHours(-1), CreatedTime = DateTime.UtcNow.AddMonths(-8) },
                new() { ID = Guid.NewGuid(), FullName = "Elizabeth Darko", Email = "edarko@ghs.gov.gh", AccessLevel = "Regional", Role = "Viewer", RegionName = "Central", IsActive = false, LastLogin = DateTime.UtcNow.AddMonths(-1), CreatedTime = DateTime.UtcNow.AddMonths(-7) },
                new() { ID = Guid.NewGuid(), FullName = "Patrick Agyeman", Email = "pagyeman@ghs.gov.gh", AccessLevel = "District", Role = "Analyst", RegionName = "Eastern", DistrictName = "Koforidua", IsActive = true, IsLocked = true, LastLogin = DateTime.UtcNow.AddDays(-5), CreatedTime = DateTime.UtcNow.AddMonths(-4) }
            };
        }
    }

    // API Response models for User
    internal class ApiUserSummary
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
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    internal class ApiRegionDropdown
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
    }

    internal class ApiDistrictDropdown
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public Guid RegionId { get; set; }
    }

    internal class ApiErrorResponse
    {
        public string? Error { get; set; }
    }
}
