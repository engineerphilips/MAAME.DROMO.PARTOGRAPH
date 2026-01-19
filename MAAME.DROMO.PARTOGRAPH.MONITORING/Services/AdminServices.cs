using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    // =============================================
    // User Activity Service
    // =============================================

    public interface IUserActivityService
    {
        Task<UserActivityStatistics> GetActivityStatisticsAsync(string? accessLevel = null, Guid? regionId = null, Guid? districtId = null);
        Task<List<UserActivitySummary>> GetUserActivitiesAsync(string? accessLevel = null, Guid? regionId = null, Guid? districtId = null);
        Task<List<UserActivitySummary>> GetInactiveUsersAsync(int daysInactive = 30, string? accessLevel = null, Guid? regionId = null, Guid? districtId = null);
        Task<List<LoginAttempt>> GetLoginAttemptsAsync(DateTime? from = null, DateTime? to = null, bool? successOnly = null, Guid? userId = null);
        Task<List<LoginAttempt>> GetFailedLoginAttemptsAsync(int hours = 24);
        Task<List<UserSession>> GetActiveSessionsAsync(string? accessLevel = null, Guid? regionId = null, Guid? districtId = null);
        Task<bool> TerminateSessionAsync(Guid sessionId);
    }

    public class UserActivityService : IUserActivityService
    {
        private readonly HttpClient _httpClient;

        public UserActivityService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserActivityStatistics> GetActivityStatisticsAsync(string? accessLevel = null, Guid? regionId = null, Guid? districtId = null)
        {
            try
            {
                var query = BuildQueryString(accessLevel, regionId, districtId);
                var response = await _httpClient.GetFromJsonAsync<UserActivityStatistics>($"api/monitoring/admin/activity/statistics{query}");
                return response ?? new UserActivityStatistics();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting activity statistics: {ex.Message}");
                return GetDemoActivityStatistics();
            }
        }

        public async Task<List<UserActivitySummary>> GetUserActivitiesAsync(string? accessLevel = null, Guid? regionId = null, Guid? districtId = null)
        {
            try
            {
                var query = BuildQueryString(accessLevel, regionId, districtId);
                var response = await _httpClient.GetFromJsonAsync<List<UserActivitySummary>>($"api/monitoring/admin/activity/users{query}");
                return response ?? new List<UserActivitySummary>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user activities: {ex.Message}");
                return GetDemoUserActivities();
            }
        }

        public async Task<List<UserActivitySummary>> GetInactiveUsersAsync(int daysInactive = 30, string? accessLevel = null, Guid? regionId = null, Guid? districtId = null)
        {
            try
            {
                var query = BuildQueryString(accessLevel, regionId, districtId);
                var separator = string.IsNullOrEmpty(query) ? "?" : "&";
                var response = await _httpClient.GetFromJsonAsync<List<UserActivitySummary>>($"api/monitoring/admin/activity/inactive{query}{separator}days={daysInactive}");
                return response ?? new List<UserActivitySummary>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting inactive users: {ex.Message}");
                return GetDemoUserActivities().Where(u => u.ActivityStatus == "Inactive" || u.ActivityStatus == "Dormant").ToList();
            }
        }

        public async Task<List<LoginAttempt>> GetLoginAttemptsAsync(DateTime? from = null, DateTime? to = null, bool? successOnly = null, Guid? userId = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (from.HasValue) queryParams.Add($"from={from.Value:yyyy-MM-dd}");
                if (to.HasValue) queryParams.Add($"to={to.Value:yyyy-MM-dd}");
                if (successOnly.HasValue) queryParams.Add($"successOnly={successOnly.Value}");
                if (userId.HasValue) queryParams.Add($"userId={userId.Value}");

                var query = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetFromJsonAsync<List<LoginAttempt>>($"api/monitoring/admin/activity/logins{query}");
                return response ?? new List<LoginAttempt>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting login attempts: {ex.Message}");
                return GetDemoLoginAttempts();
            }
        }

        public async Task<List<LoginAttempt>> GetFailedLoginAttemptsAsync(int hours = 24)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<LoginAttempt>>($"api/monitoring/admin/activity/logins/failed?hours={hours}");
                return response ?? new List<LoginAttempt>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting failed login attempts: {ex.Message}");
                return GetDemoLoginAttempts().Where(l => !l.IsSuccessful).ToList();
            }
        }

        public async Task<List<UserSession>> GetActiveSessionsAsync(string? accessLevel = null, Guid? regionId = null, Guid? districtId = null)
        {
            try
            {
                var query = BuildQueryString(accessLevel, regionId, districtId);
                var response = await _httpClient.GetFromJsonAsync<List<UserSession>>($"api/monitoring/admin/activity/sessions{query}");
                return response ?? new List<UserSession>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting active sessions: {ex.Message}");
                return new List<UserSession>();
            }
        }

        public async Task<bool> TerminateSessionAsync(Guid sessionId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/monitoring/admin/activity/sessions/{sessionId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error terminating session: {ex.Message}");
                return false;
            }
        }

        private string BuildQueryString(string? accessLevel, Guid? regionId, Guid? districtId)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(accessLevel)) queryParams.Add($"accessLevel={accessLevel}");
            if (regionId.HasValue) queryParams.Add($"regionId={regionId.Value}");
            if (districtId.HasValue) queryParams.Add($"districtId={districtId.Value}");
            return queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
        }

        private UserActivityStatistics GetDemoActivityStatistics()
        {
            return new UserActivityStatistics
            {
                TotalUsers = 156,
                ActiveToday = 42,
                ActiveThisWeek = 98,
                InactiveUsers = 23,
                DormantUsers = 8,
                FailedLoginsToday = 5,
                LockedAccounts = 2,
                OnlineNow = 18
            };
        }

        private List<UserActivitySummary> GetDemoUserActivities()
        {
            var random = new Random();
            var activities = new List<UserActivitySummary>();
            var names = new[] { "Kwame Asante", "Ama Mensah", "Kofi Owusu", "Abena Boateng", "Yaw Adjei", "Akua Darko", "Kwesi Amponsah", "Efua Ansah" };
            var accessLevels = new[] { "National", "Regional", "District" };
            var roles = new[] { "Admin", "Manager", "Analyst", "Viewer" };
            var regions = new[] { "Greater Accra", "Ashanti", "Western", "Eastern" };
            var districts = new[] { "Accra Metro", "Kumasi Metro", "Sekondi-Takoradi", "Koforidua" };

            for (int i = 0; i < 20; i++)
            {
                var lastLogin = DateTime.UtcNow.AddDays(-random.Next(0, 120));
                var status = lastLogin > DateTime.UtcNow.AddDays(-7) ? "Active" :
                            lastLogin > DateTime.UtcNow.AddDays(-30) ? "Inactive" : "Dormant";

                activities.Add(new UserActivitySummary
                {
                    UserID = Guid.NewGuid(),
                    FullName = names[random.Next(names.Length)] + " " + (i + 1),
                    Email = $"user{i + 1}@health.gov.gh",
                    AccessLevel = accessLevels[random.Next(accessLevels.Length)],
                    Role = roles[random.Next(roles.Length)],
                    RegionName = regions[random.Next(regions.Length)],
                    DistrictName = districts[random.Next(districts.Length)],
                    LastLogin = lastLogin,
                    LastActivity = lastLogin.AddMinutes(random.Next(1, 480)),
                    LoginCountToday = status == "Active" ? random.Next(1, 5) : 0,
                    LoginCountThisWeek = status == "Active" ? random.Next(3, 15) : 0,
                    LoginCountThisMonth = random.Next(5, 30),
                    FailedLoginAttempts = random.Next(0, 3),
                    IsCurrentlyOnline = status == "Active" && random.Next(0, 3) == 0,
                    ActivityStatus = status
                });
            }

            return activities;
        }

        private List<LoginAttempt> GetDemoLoginAttempts()
        {
            var random = new Random();
            var attempts = new List<LoginAttempt>();
            var names = new[] { "Kwame Asante", "Ama Mensah", "Kofi Owusu", "Unknown User" };
            var failureReasons = new[] { "Invalid password", "Account locked", "Invalid email", "Too many attempts" };

            for (int i = 0; i < 50; i++)
            {
                var isSuccess = random.Next(0, 10) > 2; // 70% success rate
                attempts.Add(new LoginAttempt
                {
                    ID = Guid.NewGuid(),
                    UserID = isSuccess ? Guid.NewGuid() : null,
                    Email = $"user{random.Next(1, 20)}@health.gov.gh",
                    UserFullName = isSuccess ? names[random.Next(names.Length - 1)] : null,
                    AttemptTime = DateTime.UtcNow.AddHours(-random.Next(0, 72)),
                    IsSuccessful = isSuccess,
                    FailureReason = isSuccess ? null : failureReasons[random.Next(failureReasons.Length)],
                    IPAddress = $"192.168.{random.Next(1, 255)}.{random.Next(1, 255)}",
                    Location = "Accra, Ghana"
                });
            }

            return attempts.OrderByDescending(a => a.AttemptTime).ToList();
        }
    }

    // =============================================
    // Notification Service
    // =============================================

    public interface INotificationAdminService
    {
        Task<NotificationSummary> GetNotificationSummaryAsync();
        Task<List<AdminNotification>> GetNotificationsAsync(int page = 1, int pageSize = 20, bool unreadOnly = false);
        Task<bool> MarkAsReadAsync(Guid notificationId);
        Task<bool> MarkAllAsReadAsync();
        Task<NotificationPreferences> GetPreferencesAsync();
        Task<bool> UpdatePreferencesAsync(NotificationPreferences preferences);
        Task<bool> SendNotificationAsync(AdminNotification notification);
    }

    public class NotificationAdminService : INotificationAdminService
    {
        private readonly HttpClient _httpClient;

        public NotificationAdminService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<NotificationSummary> GetNotificationSummaryAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<NotificationSummary>("api/monitoring/admin/notifications/summary");
                return response ?? new NotificationSummary();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting notification summary: {ex.Message}");
                return GetDemoNotificationSummary();
            }
        }

        public async Task<List<AdminNotification>> GetNotificationsAsync(int page = 1, int pageSize = 20, bool unreadOnly = false)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<AdminNotification>>($"api/monitoring/admin/notifications?page={page}&pageSize={pageSize}&unreadOnly={unreadOnly}");
                return response ?? new List<AdminNotification>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting notifications: {ex.Message}");
                return GetDemoNotifications();
            }
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"api/monitoring/admin/notifications/{notificationId}/read", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking notification as read: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MarkAllAsReadAsync()
        {
            try
            {
                var response = await _httpClient.PatchAsync("api/monitoring/admin/notifications/read-all", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking all notifications as read: {ex.Message}");
                return false;
            }
        }

        public async Task<NotificationPreferences> GetPreferencesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<NotificationPreferences>("api/monitoring/admin/notifications/preferences");
                return response ?? new NotificationPreferences();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting notification preferences: {ex.Message}");
                return new NotificationPreferences();
            }
        }

        public async Task<bool> UpdatePreferencesAsync(NotificationPreferences preferences)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/monitoring/admin/notifications/preferences", preferences);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating notification preferences: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendNotificationAsync(AdminNotification notification)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/monitoring/admin/notifications", notification);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending notification: {ex.Message}");
                return false;
            }
        }

        private NotificationSummary GetDemoNotificationSummary()
        {
            return new NotificationSummary
            {
                TotalUnread = 5,
                CriticalCount = 1,
                WarningCount = 2,
                InfoCount = 2,
                RecentNotifications = GetDemoNotifications().Take(5).ToList()
            };
        }

        private List<AdminNotification> GetDemoNotifications()
        {
            return new List<AdminNotification>
            {
                new AdminNotification
                {
                    ID = Guid.NewGuid(),
                    Type = "FacilityAdded",
                    Title = "New Facility Registered",
                    Message = "Ridge Hospital has been added to Accra Metro district.",
                    Severity = "Info",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                    IsRead = false,
                    ActionUrl = "/facilities"
                },
                new AdminNotification
                {
                    ID = Guid.NewGuid(),
                    Type = "UserIssue",
                    Title = "Account Locked",
                    Message = "User kwame.asante@health.gov.gh has been locked after 5 failed login attempts.",
                    Severity = "Warning",
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    IsRead = false,
                    ActionUrl = "/users"
                },
                new AdminNotification
                {
                    ID = Guid.NewGuid(),
                    Type = "Alert",
                    Title = "Critical Alert Rate Spike",
                    Message = "There has been a 200% increase in critical alerts in the last hour.",
                    Severity = "Critical",
                    CreatedAt = DateTime.UtcNow.AddHours(-3),
                    IsRead = false,
                    ActionUrl = "/alerts"
                },
                new AdminNotification
                {
                    ID = Guid.NewGuid(),
                    Type = "PasswordReset",
                    Title = "Password Reset Completed",
                    Message = "Password was reset for user ama.mensah@health.gov.gh",
                    Severity = "Info",
                    CreatedAt = DateTime.UtcNow.AddHours(-5),
                    IsRead = true,
                    ReadAt = DateTime.UtcNow.AddHours(-4)
                },
                new AdminNotification
                {
                    ID = Guid.NewGuid(),
                    Type = "System",
                    Title = "System Maintenance Scheduled",
                    Message = "System maintenance is scheduled for Sunday 2:00 AM - 4:00 AM GMT.",
                    Severity = "Warning",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    IsRead = true,
                    ReadAt = DateTime.UtcNow.AddHours(-20)
                }
            };
        }
    }

    // =============================================
    // Feature Flags Service
    // =============================================

    public interface IFeatureFlagService
    {
        Task<List<FeatureFlagWithScopes>> GetAllFeatureFlagsAsync();
        Task<FeatureFlagWithScopes?> GetFeatureFlagAsync(Guid flagId);
        Task<bool> IsFeatureEnabledAsync(string featureKey, Guid? regionId = null, Guid? districtId = null, Guid? userId = null);
        Task<bool> UpdateFeatureFlagScopeAsync(FeatureFlagUpdateRequest request);
        Task<bool> CreateFeatureFlagAsync(FeatureFlag flag);
        Task<bool> DeleteFeatureFlagAsync(Guid flagId);
    }

    public class FeatureFlagService : IFeatureFlagService
    {
        private readonly HttpClient _httpClient;

        public FeatureFlagService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<FeatureFlagWithScopes>> GetAllFeatureFlagsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<FeatureFlagWithScopes>>("api/monitoring/admin/feature-flags");
                return response ?? new List<FeatureFlagWithScopes>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting feature flags: {ex.Message}");
                return GetDemoFeatureFlags();
            }
        }

        public async Task<FeatureFlagWithScopes?> GetFeatureFlagAsync(Guid flagId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<FeatureFlagWithScopes>($"api/monitoring/admin/feature-flags/{flagId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting feature flag: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> IsFeatureEnabledAsync(string featureKey, Guid? regionId = null, Guid? districtId = null, Guid? userId = null)
        {
            try
            {
                var queryParams = new List<string> { $"key={featureKey}" };
                if (regionId.HasValue) queryParams.Add($"regionId={regionId.Value}");
                if (districtId.HasValue) queryParams.Add($"districtId={districtId.Value}");
                if (userId.HasValue) queryParams.Add($"userId={userId.Value}");

                var query = string.Join("&", queryParams);
                var response = await _httpClient.GetFromJsonAsync<FeatureFlagCheckResponse>($"api/monitoring/admin/feature-flags/check?{query}");
                return response?.IsEnabled ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking feature flag: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateFeatureFlagScopeAsync(FeatureFlagUpdateRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/monitoring/admin/feature-flags/scope", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating feature flag scope: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateFeatureFlagAsync(FeatureFlag flag)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/monitoring/admin/feature-flags", flag);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating feature flag: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteFeatureFlagAsync(Guid flagId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/monitoring/admin/feature-flags/{flagId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting feature flag: {ex.Message}");
                return false;
            }
        }

        private List<FeatureFlagWithScopes> GetDemoFeatureFlags()
        {
            return new List<FeatureFlagWithScopes>
            {
                new FeatureFlagWithScopes
                {
                    Flag = new FeatureFlag
                    {
                        ID = Guid.NewGuid(),
                        Key = "predictive_analytics",
                        Name = "Predictive Analytics",
                        Description = "AI-powered risk prediction and outcome forecasting",
                        Category = "Analytics",
                        IsGloballyEnabled = false,
                        CreatedAt = DateTime.UtcNow.AddMonths(-3)
                    },
                    EnabledRegionsCount = 3,
                    EnabledDistrictsCount = 8,
                    EnabledUsersCount = 0
                },
                new FeatureFlagWithScopes
                {
                    Flag = new FeatureFlag
                    {
                        ID = Guid.NewGuid(),
                        Key = "advanced_reports",
                        Name = "Advanced Reports",
                        Description = "Enhanced reporting with custom templates and scheduling",
                        Category = "Reporting",
                        IsGloballyEnabled = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-6)
                    },
                    EnabledRegionsCount = 16,
                    EnabledDistrictsCount = 216,
                    EnabledUsersCount = 0
                },
                new FeatureFlagWithScopes
                {
                    Flag = new FeatureFlag
                    {
                        ID = Guid.NewGuid(),
                        Key = "real_time_monitoring",
                        Name = "Real-time Monitoring Dashboard",
                        Description = "Live updates and real-time data streaming",
                        Category = "Monitoring",
                        IsGloballyEnabled = false,
                        CreatedAt = DateTime.UtcNow.AddMonths(-1)
                    },
                    EnabledRegionsCount = 1,
                    EnabledDistrictsCount = 5,
                    EnabledUsersCount = 12
                },
                new FeatureFlagWithScopes
                {
                    Flag = new FeatureFlag
                    {
                        ID = Guid.NewGuid(),
                        Key = "mobile_notifications",
                        Name = "Mobile Push Notifications",
                        Description = "Push notifications for mobile app users",
                        Category = "Notifications",
                        IsGloballyEnabled = false,
                        CreatedAt = DateTime.UtcNow.AddWeeks(-2)
                    },
                    EnabledRegionsCount = 2,
                    EnabledDistrictsCount = 0,
                    EnabledUsersCount = 25
                }
            };
        }

        private class FeatureFlagCheckResponse
        {
            public bool IsEnabled { get; set; }
        }
    }

    // =============================================
    // Rate Limiting Service
    // =============================================

    public interface IRateLimitService
    {
        Task<List<RateLimitConfig>> GetRateLimitConfigsAsync();
        Task<bool> UpdateRateLimitConfigAsync(RateLimitConfig config);
        Task<List<RateLimitStatus>> GetCurrentUsageAsync();
        Task<List<RateLimitViolation>> GetRecentViolationsAsync(int hours = 24);
        Task<bool> ClearThrottleAsync(Guid userId);
    }

    public class RateLimitService : IRateLimitService
    {
        private readonly HttpClient _httpClient;

        public RateLimitService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<RateLimitConfig>> GetRateLimitConfigsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<RateLimitConfig>>("api/monitoring/admin/rate-limits/config");
                return response ?? new List<RateLimitConfig>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting rate limit configs: {ex.Message}");
                return GetDemoRateLimitConfigs();
            }
        }

        public async Task<bool> UpdateRateLimitConfigAsync(RateLimitConfig config)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/monitoring/admin/rate-limits/config", config);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating rate limit config: {ex.Message}");
                return false;
            }
        }

        public async Task<List<RateLimitStatus>> GetCurrentUsageAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<RateLimitStatus>>("api/monitoring/admin/rate-limits/usage");
                return response ?? new List<RateLimitStatus>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting rate limit usage: {ex.Message}");
                return GetDemoRateLimitUsage();
            }
        }

        public async Task<List<RateLimitViolation>> GetRecentViolationsAsync(int hours = 24)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<RateLimitViolation>>($"api/monitoring/admin/rate-limits/violations?hours={hours}");
                return response ?? new List<RateLimitViolation>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting rate limit violations: {ex.Message}");
                return GetDemoViolations();
            }
        }

        public async Task<bool> ClearThrottleAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/monitoring/admin/rate-limits/throttle/{userId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing throttle: {ex.Message}");
                return false;
            }
        }

        private List<RateLimitConfig> GetDemoRateLimitConfigs()
        {
            return new List<RateLimitConfig>
            {
                new RateLimitConfig
                {
                    ID = Guid.NewGuid(),
                    AccessLevel = "National",
                    Endpoint = "*",
                    RequestsPerMinute = 120,
                    RequestsPerHour = 3600,
                    RequestsPerDay = 50000,
                    BurstLimit = 30,
                    IsEnabled = true
                },
                new RateLimitConfig
                {
                    ID = Guid.NewGuid(),
                    AccessLevel = "Regional",
                    Endpoint = "*",
                    RequestsPerMinute = 60,
                    RequestsPerHour = 1800,
                    RequestsPerDay = 25000,
                    BurstLimit = 20,
                    IsEnabled = true
                },
                new RateLimitConfig
                {
                    ID = Guid.NewGuid(),
                    AccessLevel = "District",
                    Endpoint = "*",
                    RequestsPerMinute = 30,
                    RequestsPerHour = 900,
                    RequestsPerDay = 10000,
                    BurstLimit = 10,
                    IsEnabled = true
                }
            };
        }

        private List<RateLimitStatus> GetDemoRateLimitUsage()
        {
            var random = new Random();
            return new List<RateLimitStatus>
            {
                new RateLimitStatus
                {
                    UserID = Guid.NewGuid(),
                    UserName = "Kwame Asante",
                    AccessLevel = "National",
                    RequestsThisMinute = random.Next(10, 50),
                    RequestsThisHour = random.Next(100, 500),
                    RequestsToday = random.Next(1000, 5000),
                    LimitPerMinute = 120,
                    LimitPerHour = 3600,
                    LimitPerDay = 50000,
                    UsagePercentage = random.Next(5, 40),
                    IsThrottled = false
                },
                new RateLimitStatus
                {
                    UserID = Guid.NewGuid(),
                    UserName = "Ama Mensah",
                    AccessLevel = "Regional",
                    RequestsThisMinute = random.Next(5, 30),
                    RequestsThisHour = random.Next(50, 300),
                    RequestsToday = random.Next(500, 3000),
                    LimitPerMinute = 60,
                    LimitPerHour = 1800,
                    LimitPerDay = 25000,
                    UsagePercentage = random.Next(10, 60),
                    IsThrottled = false
                }
            };
        }

        private List<RateLimitViolation> GetDemoViolations()
        {
            return new List<RateLimitViolation>
            {
                new RateLimitViolation
                {
                    ID = Guid.NewGuid(),
                    UserID = Guid.NewGuid(),
                    UserName = "Test User",
                    Endpoint = "/api/monitoring/facilities",
                    ViolationTime = DateTime.UtcNow.AddHours(-2),
                    ViolationType = "MinuteLimit",
                    RequestCount = 35,
                    Limit = 30,
                    IPAddress = "192.168.1.100"
                }
            };
        }
    }
}
