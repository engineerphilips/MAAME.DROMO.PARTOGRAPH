using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    /// <summary>
    /// Represents an administrative district within a region
    /// Used for hierarchical monitoring at the regional level
    /// </summary>
    public class District
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty; // e.g., "AMA", "TMA"

        // Type of district (Metropolitan, Municipal, District)
        public string Type { get; set; } = "District"; // Metropolitan, Municipal, District

        // Parent region reference (join Region table to get name)
        public Guid RegionID { get; set; }

        // District capital
        public string Capital { get; set; } = string.Empty;

        // Population data for analysis
        public long Population { get; set; }
        public int ExpectedAnnualDeliveries { get; set; }

        // Contact information for district health directorate
        public string DirectorName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // GPS coordinates of district center
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public bool IsActive { get; set; } = true;

        // Sync columns
        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
        public long? DeletedTime { get; set; }
        public int Deleted { get; set; }

        // Navigation properties
        [IgnoreDataMember]
        public Region? Region { get; set; }

        [IgnoreDataMember]
        public ICollection<Facility>? Facilities { get; set; }

        public string CalculateHash()
        {
            var data = $"{ID}|{Name}|{Code}|{Type}|{RegionID}|{Capital}|{IsActive}";
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
