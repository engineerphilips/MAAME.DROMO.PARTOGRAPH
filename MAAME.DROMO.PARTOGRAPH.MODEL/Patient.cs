
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
        public string Address { get; set; } = string.Empty;
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public string EmergencyContactRelationship { get; set; } = string.Empty;

        // Anthropometric Data
        public double? Weight { get; set; } // in kg
        public double? Height { get; set; } // in cm

        // Previous Pregnancy Outcomes
        public bool HasPreviousCSection { get; set; }
        public int? NumberOfPreviousCsections { get; set; }
        public int? LiveBirths { get; set; }
        public int? Stillbirths { get; set; }
        public int? NeonatalDeaths { get; set; }

        //User
        public string HandlerName { get; set; } = string.Empty;
        public Guid? Handler { get; set; }

        // Facility - links patient to facility for reporting (join to get name)
        public Guid? FacilityID { get; set; }

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

        /// <summary>
        /// Calculates a comprehensive hash of all relevant patient fields for change detection.
        /// This hash is used during sync to detect if the record has been modified.
        /// </summary>
        public string CalculateHash()
        {
            // Include all fields that should trigger a sync when changed
            var dataBuilder = new StringBuilder();
            dataBuilder.Append($"{ID}|");
            dataBuilder.Append($"{FirstName}|");
            dataBuilder.Append($"{LastName}|");
            dataBuilder.Append($"{HospitalNumber}|");
            dataBuilder.Append($"{DateOfBirth}|");
            dataBuilder.Append($"{Age}|");
            dataBuilder.Append($"{BloodGroup}|");
            dataBuilder.Append($"{PhoneNumber}|");
            dataBuilder.Append($"{Address}|");
            dataBuilder.Append($"{EmergencyContactName}|");
            dataBuilder.Append($"{EmergencyContactPhone}|");
            dataBuilder.Append($"{EmergencyContactRelationship}|");
            dataBuilder.Append($"{Weight}|");
            dataBuilder.Append($"{Height}|");
            dataBuilder.Append($"{HasPreviousCSection}|");
            dataBuilder.Append($"{NumberOfPreviousCsections}|");
            dataBuilder.Append($"{LiveBirths}|");
            dataBuilder.Append($"{Stillbirths}|");
            dataBuilder.Append($"{NeonatalDeaths}|");
            dataBuilder.Append($"{Handler}|");
            dataBuilder.Append($"{FacilityID}|");
            dataBuilder.Append($"{UpdatedTime}");

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataBuilder.ToString()));
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Updates the DataHash property with the current hash value.
        /// Call this before saving the record.
        /// </summary>
        public void UpdateDataHash()
        {
            DataHash = CalculateHash();
        }

        // Navigation Properties
        public List<Partograph> PartographEntries { get; set; } = [];
        //public List<VitalSign> VitalSigns { get; set; } = [];
        public List<MedicalNote> MedicalNotes { get; set; } = [];
    }

    /// <summary>
    /// WHO-Aligned Four Stage Labor System
    /// Reference: WHO recommendations for intrapartum care (2020)
    /// </summary>
    public enum LaborStatus
    {
        Pending,      // Pre-labor / Not in active labor
        FirstStage,   // Active labor: Onset of regular contractions → Full dilation (10cm)
        SecondStage,  // Full dilation (10cm) → Baby delivery
        ThirdStage,   // Baby delivery → Placenta delivery (up to 30 minutes)
        FourthStage,  // First 2 hours postpartum (immediate recovery period)
        Completed,    // Care completed, can be discharged
        Emergency     // Requires immediate attention (can occur at any stage)
    }

    /// <summary>
    /// First Stage Phase - Sub-phases of first stage labor based on cervical dilation
    /// Reference: WHO recommendations for intrapartum care (2020)
    /// </summary>
    public enum FirstStagePhase
    {
        /// <summary>
        /// Not yet in active labor or phase not determined
        /// </summary>
        NotDetermined,

        /// <summary>
        /// Latent Phase: 0-4cm cervical dilation
        /// Slower progress, irregular contractions
        /// </summary>
        Latent,

        /// <summary>
        /// Early Active Phase: 5-7cm cervical dilation
        /// Regular contractions, faster progress expected
        /// </summary>
        ActiveEarly,

        /// <summary>
        /// Advanced Active Phase: 8-9cm cervical dilation
        /// Strong regular contractions, approaching full dilation
        /// </summary>
        ActiveAdvanced,

        /// <summary>
        /// Transition Phase: 10cm cervical dilation
        /// Full dilation achieved, ready for second stage
        /// </summary>
        Transition
    }
}
