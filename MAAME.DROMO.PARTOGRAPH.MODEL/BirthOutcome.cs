using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    /// <summary>
    /// Records the outcome of delivery for both mother and baby
    /// Based on WHO recommendations for birth outcome recording
    /// </summary>
    public class BirthOutcome
    {
        public Guid? ID { get; set; }
        public Guid? PartographID { get; set; }
        public DateTime RecordedTime { get; set; } = DateTime.Now;

        // Maternal Outcome
        public MaternalOutcomeStatus MaternalStatus { get; set; } = MaternalOutcomeStatus.Survived;
        public DateTime? MaternalDeathTime { get; set; }
        public string MaternalDeathCause { get; set; } = string.Empty;
        public string MaternalDeathCircumstances { get; set; } = string.Empty;

        // Delivery Information
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.SpontaneousVaginal;
        public string DeliveryModeDetails { get; set; } = string.Empty; // e.g., "Forceps", "Vacuum", "Emergency CS"
        public DateTime? DeliveryTime { get; set; }

        // Multiple Birth Information
        public int NumberOfBabies { get; set; } = 1;
        public bool IsMultipleBirth => NumberOfBabies > 1;

        // Perineal Status
        public PerinealStatus PerinealStatus { get; set; } = PerinealStatus.Intact;
        public string PerinealDetails { get; set; } = string.Empty; // e.g., "1st degree tear", "Episiotomy"

        // Placental Information
        public DateTime? PlacentaDeliveryTime { get; set; }
        public bool PlacentaComplete { get; set; } = true;
        public int EstimatedBloodLoss { get; set; } // in ml

        // Maternal Complications
        public string MaternalComplications { get; set; } = string.Empty;
        public bool PostpartumHemorrhage { get; set; } = false;
        public bool Eclampsia { get; set; } = false;
        public bool SepticShock { get; set; } = false;
        public bool ObstructedLabor { get; set; } = false;
        public bool RupturedUterus { get; set; } = false;

        // Post-delivery Care
        public bool OxytocinGiven { get; set; } = true; // Active management of 3rd stage
        public bool AntibioticsGiven { get; set; } = false;
        public bool BloodTransfusionGiven { get; set; } = false;

        // Recording Information
        public string HandlerName { get; set; } = string.Empty;
        public Guid? Handler { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Navigation Property
        [JsonIgnore]
        public Partograph Partograph { get; set; }

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

        [NotMapped]
        [JsonIgnore]
        public bool HasConflict => SyncStatus == 2;

        [NotMapped]
        [JsonIgnore]
        public bool NeedsSync => SyncStatus == 0;

        public string CalculateHash()
        {
            var data = $"{ID}|{PartographID}|{MaternalStatus}|{NumberOfBabies}";
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }

    public enum MaternalOutcomeStatus
    {
        Survived,
        Died
    }

    public enum DeliveryMode
    {
        SpontaneousVaginal,
        AssistedVaginal, // Vacuum, Forceps
        CaesareanSection,
        BreechDelivery
    }

    public enum PerinealStatus
    {
        Intact,
        FirstDegreeTear,
        SecondDegreeTear,
        ThirdDegreeTear,
        FourthDegreeTear,
        Episiotomy
    }
}
