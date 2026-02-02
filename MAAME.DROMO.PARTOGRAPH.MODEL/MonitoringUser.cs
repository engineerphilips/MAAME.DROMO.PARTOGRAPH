using System;
using System.Runtime.Serialization;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    /// <summary>
    /// Represents a user who can access the monitoring dashboard
    /// with hierarchical access levels (National, Regional, District)
    /// </summary>
    public class MonitoringUser
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // Authentication
        public string? PasswordHash { get; set; }
        public string? RefreshToken { get; set; }
        public long RefreshTokenExpiryTime { get; set; }

        // Access level determines what data the user can see
        // National: All regions and districts
        // Regional: All districts within their assigned region
        // District: All facilities within their assigned district
        // Facility: Only data within their assigned facility
        public string AccessLevel { get; set; } = "District"; // National, Regional, District, Facility

        // Role within their access level
        public string Role { get; set; } = "Viewer"; // Admin, Manager, Analyst, Viewer

        // Job title/position
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;

        // Assigned region (for Regional and District users)
        public Guid? RegionID { get; set; }
        public string RegionName { get; set; } = string.Empty;

        // Assigned district (for District users only)
        public Guid? DistrictID { get; set; }
        public string DistrictName { get; set; } = string.Empty;

        // Assigned facility (for Facility users only)
        public Guid? FacilityID { get; set; }
        public string FacilityName { get; set; } = string.Empty;

        // Session tracking
        public DateTime? LastLogin { get; set; }
        public long LastLoginTime { get; set; }
        public string? LastLoginIP { get; set; }
        public int LoginCount { get; set; }
        public int FailedLoginAttempts { get; set; }
        public bool IsLocked { get; set; }
        public long? LockedUntil { get; set; }

        public bool IsActive { get; set; } = true;
        public bool EmailConfirmed { get; set; }
        public bool RequirePasswordChange { get; set; } = true;

        // Audit fields
        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
        public long? DeletedTime { get; set; }
        public int Deleted { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Navigation properties
        [IgnoreDataMember]
        public Region? Region { get; set; }

        [IgnoreDataMember]
        public District? District { get; set; }

        [IgnoreDataMember]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [IgnoreDataMember]
        public bool IsNationalUser => AccessLevel == "National";

        [IgnoreDataMember]
        public bool IsRegionalUser => AccessLevel == "Regional";

        [IgnoreDataMember]
        public bool IsDistrictUser => AccessLevel == "District";

        [IgnoreDataMember]
        public bool IsFacilityUser => AccessLevel == "Facility";

        public string CalculateHash()
        {
            var data = $"{ID}|{Email}|{AccessLevel}|{Role}|{RegionID}|{DistrictID}|{FacilityID}|{IsActive}";
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
