namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Models
{
    // =============================================
    // User Activity Models
    // =============================================

    public class UserActivitySummary
    {
        public Guid UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AccessLevel { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? RegionName { get; set; }
        public string? DistrictName { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? LastActivity { get; set; }
        public int LoginCountToday { get; set; }
        public int LoginCountThisWeek { get; set; }
        public int LoginCountThisMonth { get; set; }
        public int FailedLoginAttempts { get; set; }
        public bool IsCurrentlyOnline { get; set; }
        public string ActivityStatus { get; set; } = "Unknown"; // Active, Inactive, Dormant
    }

    public class UserActivityStatistics
    {
        public int TotalUsers { get; set; }
        public int ActiveToday { get; set; }
        public int ActiveThisWeek { get; set; }
        public int InactiveUsers { get; set; } // No login in 30+ days
        public int DormantUsers { get; set; } // No login in 90+ days
        public int FailedLoginsToday { get; set; }
        public int LockedAccounts { get; set; }
        public int OnlineNow { get; set; }
    }

    public class LoginAttempt
    {
        public Guid ID { get; set; }
        public Guid? UserID { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? UserFullName { get; set; }
        public DateTime AttemptTime { get; set; }
        public bool IsSuccessful { get; set; }
        public string? FailureReason { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Location { get; set; }
    }

    public class UserSession
    {
        public Guid ID { get; set; }
        public Guid UserID { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
        public DateTime? LastActivityTime { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public bool IsActive { get; set; }
    }

    // =============================================
    // Notification Models
    // =============================================

    public class AdminNotification
    {
        public Guid ID { get; set; }
        public string Type { get; set; } = string.Empty; // FacilityAdded, UserIssue, PasswordReset, Alert, System
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Info"; // Info, Warning, Critical
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public Guid? TargetUserId { get; set; }
        public string? TargetAccessLevel { get; set; }
        public Guid? TargetRegionId { get; set; }
        public Guid? TargetDistrictId { get; set; }
        public string? ActionUrl { get; set; }
        public string? RelatedEntityType { get; set; } // Facility, User, etc.
        public Guid? RelatedEntityId { get; set; }
    }

    public class NotificationPreferences
    {
        public Guid UserID { get; set; }
        public bool EmailOnNewFacility { get; set; } = true;
        public bool EmailOnUserIssue { get; set; } = true;
        public bool EmailOnPasswordReset { get; set; } = true;
        public bool EmailOnCriticalAlert { get; set; } = true;
        public bool InAppNotifications { get; set; } = true;
        public bool DailyDigest { get; set; } = false;
        public string? DigestTime { get; set; } = "08:00";
    }

    public class NotificationSummary
    {
        public int TotalUnread { get; set; }
        public int CriticalCount { get; set; }
        public int WarningCount { get; set; }
        public int InfoCount { get; set; }
        public List<AdminNotification> RecentNotifications { get; set; } = new();
    }

    // =============================================
    // Feature Flags Models
    // =============================================

    public class FeatureFlag
    {
        public Guid ID { get; set; }
        public string Key { get; set; } = string.Empty; // e.g., "predictive_analytics", "advanced_reports"
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Analytics, Reporting, Monitoring, etc.
        public bool IsGloballyEnabled { get; set; }
        public bool IsEnabled { get; set; } // Computed based on scope
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class FeatureFlagScope
    {
        public Guid ID { get; set; }
        public Guid FeatureFlagID { get; set; }
        public string ScopeType { get; set; } = string.Empty; // Global, Region, District, User
        public Guid? RegionID { get; set; }
        public string? RegionName { get; set; }
        public Guid? DistrictID { get; set; }
        public string? DistrictName { get; set; }
        public Guid? UserID { get; set; }
        public string? UserName { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime EnabledAt { get; set; }
        public string? EnabledBy { get; set; }
    }

    public class FeatureFlagWithScopes
    {
        public FeatureFlag Flag { get; set; } = new();
        public List<FeatureFlagScope> Scopes { get; set; } = new();
        public int EnabledRegionsCount { get; set; }
        public int EnabledDistrictsCount { get; set; }
        public int EnabledUsersCount { get; set; }
    }

    public class FeatureFlagUpdateRequest
    {
        public Guid FeatureFlagID { get; set; }
        public string ScopeType { get; set; } = string.Empty;
        public Guid? RegionID { get; set; }
        public Guid? DistrictID { get; set; }
        public Guid? UserID { get; set; }
        public bool IsEnabled { get; set; }
    }

    // =============================================
    // Rate Limiting Models
    // =============================================

    public class RateLimitConfig
    {
        public Guid ID { get; set; }
        public string AccessLevel { get; set; } = string.Empty; // National, Regional, District
        public string Endpoint { get; set; } = string.Empty; // API endpoint or "*" for all
        public int RequestsPerMinute { get; set; }
        public int RequestsPerHour { get; set; }
        public int RequestsPerDay { get; set; }
        public int BurstLimit { get; set; } // Max requests in short burst
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class RateLimitStatus
    {
        public Guid UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string AccessLevel { get; set; } = string.Empty;
        public int RequestsThisMinute { get; set; }
        public int RequestsThisHour { get; set; }
        public int RequestsToday { get; set; }
        public int LimitPerMinute { get; set; }
        public int LimitPerHour { get; set; }
        public int LimitPerDay { get; set; }
        public double UsagePercentage { get; set; }
        public bool IsThrottled { get; set; }
        public DateTime? ThrottledUntil { get; set; }
    }

    public class RateLimitViolation
    {
        public Guid ID { get; set; }
        public Guid UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public DateTime ViolationTime { get; set; }
        public string ViolationType { get; set; } = string.Empty; // MinuteLimit, HourLimit, DayLimit, Burst
        public int RequestCount { get; set; }
        public int Limit { get; set; }
        public string? IPAddress { get; set; }
    }

    // =============================================
    // Audit Log Models
    // =============================================

    public class AuditLogEntry
    {
        public Guid ID { get; set; }
        public string Action { get; set; } = string.Empty; // Created, Updated, Deleted, Accessed
        public string EntityType { get; set; } = string.Empty; // User, Facility, FeatureFlag, etc.
        public Guid? EntityID { get; set; }
        public string? EntityName { get; set; }
        public Guid PerformedByUserID { get; set; }
        public string PerformedByUserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? OldValues { get; set; } // JSON
        public string? NewValues { get; set; } // JSON
        public string? IPAddress { get; set; }
        public string? AdditionalInfo { get; set; }
    }
}
