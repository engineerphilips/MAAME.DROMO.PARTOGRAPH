namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Models
{
    /// <summary>
    /// User management models for admin functionality
    /// </summary>
    public class MonitoringUserSummary
    {
        public Guid ID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AccessLevel { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Guid? RegionID { get; set; }
        public string? RegionName { get; set; }
        public Guid? DistrictID { get; set; }
        public string? DistrictName { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class CreateUserRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AccessLevel { get; set; } = "District";
        public string Role { get; set; } = "Viewer";
        public Guid? RegionID { get; set; }
        public Guid? DistrictID { get; set; }
    }

    public class UpdateUserRequest
    {
        public Guid ID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AccessLevel { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Guid? RegionID { get; set; }
        public Guid? DistrictID { get; set; }
        public bool IsActive { get; set; }
    }

    public class ResetPasswordRequest
    {
        public Guid UserID { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UserStatistics
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int LockedUsers { get; set; }
        public int AdminUsers { get; set; }
        public int ManagerUsers { get; set; }
        public int AnalystUsers { get; set; }
        public int ViewerUsers { get; set; }
        public int NationalUsers { get; set; }
        public int RegionalUsers { get; set; }
        public int DistrictUsers { get; set; }
        public int LoggedInToday { get; set; }
    }

    /// <summary>
    /// Region dropdown item for selection
    /// </summary>
    public class RegionDropdownItem
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// District dropdown item for selection
    /// </summary>
    public class DistrictDropdownItem
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public Guid RegionID { get; set; }
    }
}
