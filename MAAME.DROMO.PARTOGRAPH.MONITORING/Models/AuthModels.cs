using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Models
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? ErrorMessage { get; set; }
        public MonitoringUserDto? User { get; set; }
    }

    public class MonitoringUserDto
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
    }

    public class UserSession
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public MonitoringUserDto User { get; set; } = new();
        public DateTime ExpiresAt { get; set; }
    }
}
