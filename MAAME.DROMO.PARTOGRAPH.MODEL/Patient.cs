
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    public class Patient
    {
        public Guid? ID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Name => $"{FirstName} {LastName}";

        public string HospitalNumber { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public string BloodGroup { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public string EmergencyContactRelationship { get; set; } = string.Empty;

        //User
        public string HandlerName { get; set; } = string.Empty;
        public Guid? Handler { get; set; }

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

        public string CalculateHash()
        {
            var data = $"{ID}|{Handler}";
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }

        // Navigation Properties
        public List<Partograph> PartographEntries { get; set; } = [];
        //public List<VitalSign> VitalSigns { get; set; } = [];
        public List<MedicalNote> MedicalNotes { get; set; } = [];
    }

    public enum LaborStatus
    {
        Pending,    // Not in active labor
        Active,     // In active labor
        Completed,  // Delivered
        Emergency   // Requires immediate attention
    }
}
