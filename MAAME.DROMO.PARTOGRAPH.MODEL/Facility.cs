using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    public class Facility
    {
        public Guid? ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty; // Unique facility code
        public string Type { get; set; } = "Hospital"; // Hospital, Clinic, Health Center
        public string Level { get; set; } = "Primary"; // Primary, Secondary, Tertiary
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Hierarchical reference for monitoring (region derived through District.Region)
        public Guid? DistrictID { get; set; }

        // GPS Location fields
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string GHPostGPS { get; set; } = string.Empty; // Ghana Post GPS Address

        public bool IsActive { get; set; } = true;

        // Sync columns
        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
        public long? DeletedTime { get; set; }

        public string DeviceId { get; set; }
        public string OriginDeviceId { get; set; }
        public int SyncStatus { get; set; }  // 0=pending, 1=synced, 2=conflict

        public int Version { get; set; }
        public int ServerVersion { get; set; }

        public int Deleted { get; set; }
        public string ConflictData { get; set; }
        public string DataHash { get; set; }

        [IgnoreDataMember]
        public bool HasConflict => SyncStatus == 2;

        [IgnoreDataMember]
        public bool NeedsSync => SyncStatus == 0;

        // Navigation property for monitoring hierarchy (Region accessed via District.Region)
        [IgnoreDataMember]
        public District? District { get; set; }

        public string CalculateHash()
        {
            var data = $"{ID}|{Name}|{Code}|{Type}|{Address}|{City}|{DistrictID}|{Country}|{IsActive}";
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
