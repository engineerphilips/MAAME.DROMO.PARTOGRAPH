using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Models
{
    // Base class for all partograph measurements
    public abstract class BasePartographMeasurement
    {
        public Guid? ID { get; set; }
        public Guid? PartographID { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public string HandlerName { get; set; } = string.Empty;
        public Guid? Handler { get; set; }
        public string Notes { get; set; } = string.Empty;

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

        [Ignore]
        public bool HasConflict => SyncStatus == 2;

        [Ignore]
        public bool NeedsSync => SyncStatus == 0;

        public string CalculateHash()
        {
            var data = $"{Time}|{Handler}";
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }

    // Fetal Position
    public class FetalPosition : BasePartographMeasurement
    {
        public string Position { get; set; } = string.Empty; // LOA, ROA, LOP, ROP, etc.
        public string? PositionDisplay => Position != null ? Position.ToString() : string.Empty;
        //public string Presentation { get; set; } = "Vertex"; // Vertex, Breech, Transverse
        //public string Lie { get; set; } = "Longitudinal"; // Longitudinal, Transverse, Oblique
        //public bool Engaged { get; set; }
        //public string Flexion { get; set; } = "Flexed"; // Flexed, Deflexed, Extended
    }

    // Enhanced Vital Signs (every hour)
    public class EnhancedVitalSignEntry : BasePartographMeasurement
    {
        public int PulseRate { get; set; }
        public string PulseRhythm { get; set; } = "Regular"; // Regular, Irregular
        public string PulseVolume { get; set; } = "Normal"; // Weak, Normal, Bounding
        public int SystolicBP { get; set; }
        public int DiastolicBP { get; set; }
        public string BPPosition { get; set; } = "Left arm"; // Left arm, Right arm
        public decimal Temperature { get; set; }
        public string TemperatureRoute { get; set; } = "Oral"; // Oral, Axillary, Tympanic
        public int RespiratoryRate { get; set; }
        public string RespiratoryPattern { get; set; } = "Normal"; // Normal, Shallow, Deep, Irregular
    }

    // Medication Administration
    public class MedicationEntry : BasePartographMeasurement
    {
        public string MedicationName { get; set; } = string.Empty;
        public string Dose { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty; // IV, IM, PO, Sublingual, etc.
        public DateTime AdministrationTime { get; set; } = DateTime.Now;
        public string Indication { get; set; } = string.Empty;
        public string PrescribedBy { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public bool AdverseReaction { get; set; }
        public string AdverseReactionDetails { get; set; } = string.Empty;
    }

    // IV Fluid Management
    public class IVFluidEntry : BasePartographMeasurement
    {
        public string FluidType { get; set; } = string.Empty; // Normal saline, Hartmann's, Dextrose, etc.
        public int VolumeInfused { get; set; }
        public string Rate { get; set; } = string.Empty; // ml/hr
        public DateTime? StartTime { get; set; }
        public string Additives { get; set; } = string.Empty; // KCl, Syntocinon, etc.
        public string IVSite { get; set; } = string.Empty; // Left hand, Right forearm, etc.
        public bool SiteHealthy { get; set; }
        public string SiteCondition { get; set; } = string.Empty; // Clean, Inflamed, Swollen, etc.
    }

    // Assessment and Plan
    public class AssessmentPlanEntry : BasePartographMeasurement
    {
        public string LaborProgress { get; set; } = string.Empty; // Normal, Delayed, Rapid
        public string FetalWellbeing { get; set; } = string.Empty; // Satisfactory, Concerning, Compromised
        public string MaternalCondition { get; set; } = string.Empty; // Stable, Concerned, Critical
        public string RiskFactors { get; set; } = string.Empty;
        public string Complications { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string ExpectedDelivery { get; set; } = string.Empty; // Normal vaginal, Instrumental, Cesarean
        public bool RequiresIntervention { get; set; }
        public string InterventionRequired { get; set; } = string.Empty;
        public DateTime? NextAssessment { get; set; }
        public string AssessedBy { get; set; } = string.Empty;
    }

    // Measurement Schedule Helper
    public static class MeasurementSchedule
    {
        public static readonly Dictionary<Type, TimeSpan> ScheduleIntervals = new()
        {
            { typeof(FHR), TimeSpan.FromMinutes(30) },
            //{ typeof(FHRDecelerationEntry), TimeSpan.FromMinutes(30) },
            { typeof(Contraction), TimeSpan.FromMinutes(30) },
            { typeof(EnhancedVitalSignEntry), TimeSpan.FromHours(1) },
            { typeof(Urine), TimeSpan.FromHours(1) },
            { typeof(CervixDilatation), TimeSpan.FromHours(4) }, // Or as needed
            { typeof(HeadDescent), TimeSpan.FromHours(4) }, // Or as needed
            { typeof(AssessmentPlanEntry), TimeSpan.FromHours(4) }
        };

        public static bool IsDue(Type measurementType, DateTime lastRecorded)
        {
            if (!ScheduleIntervals.TryGetValue(measurementType, out var interval))
                return false;

            return DateTime.Now - lastRecorded >= interval;
        }

        public static DateTime GetNextDueTime(Type measurementType, DateTime lastRecorded)
        {
            if (!ScheduleIntervals.TryGetValue(measurementType, out var interval))
                return DateTime.Now;

            return lastRecorded.Add(interval);
        }
    }

    [SQLite.Table("Tbl_SyncMetadata")]
    public class SyncMetadata
    {
        [PrimaryKey]
        public string TableName { get; set; }
        public long LastPullTimestamp { get; set; }
        public long LastPushTimestamp { get; set; }
        public long LastSuccessfulSync { get; set; }
        public int PendingPushCount { get; set; }
        public int ConflictCount { get; set; }
        public string DeviceId { get; set; }
    }

    // DTOs for API communication
    public class SyncPullRequest
    {
        public string DeviceId { get; set; }
        public long LastSyncTimestamp { get; set; }
        public string TableName { get; set; }
    }

    public class SyncPullResponse
    {
        public List<CompanionEntry> Records { get; set; }
        public long ServerTimestamp { get; set; }
        public bool HasMore { get; set; }
    }

    public class SyncPushRequest
    {
        public string DeviceId { get; set; }
        public List<CompanionEntry> Changes { get; set; }
    }

    public class SyncPushResponse
    {
        public List<string> Success { get; set; }
        public List<ConflictRecord> Conflicts { get; set; }
        public List<SyncError> Errors { get; set; }
    }

    public class ConflictRecord
    {
        public string Id { get; set; }
        public CompanionEntry ServerRecord { get; set; }
    }

    public class SyncError
    {
        public string Id { get; set; }
        public string ErrorMessage { get; set; }
    }
}
