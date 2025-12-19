
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    public class Staff
    {
        public Guid? ID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string StaffID { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Midwife"; // Doctor, Midwife, Nurse, Admin
        public string Department { get; set; } = "Labor Ward";
        public string Password { get; set; } = string.Empty; // Legacy - use PasswordHash
        public string? PasswordHash { get; set; } // Hashed password for authentication
        public DateTime LastLogin { get; set; }
        public long LastLoginTime { get; set; } // Unix timestamp for sync
        public bool IsActive { get; set; } = true;

        public Guid? Facility { get; set; }
        public Guid? FacilityID { get; set; } // Alternative property name
        public string FacilityName { get; set; } = string.Empty;
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

        //public string CalculateHash()
        //{
        //    var data = $"{Time}|{Handler}";
        //    using (var sha256 = System.Security.Cryptography.SHA256.Create())
        //    {
        //        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
        //        return Convert.ToBase64String(hashBytes);
        //    }
        //}
    }
}
