using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    /// <summary>
    /// Robson Classification System - WHO 2017 Implementation
    /// Reference: WHO Robson Classification: Implementation Manual (ISBN 978-92-4-151319-7)
    ///
    /// The Robson Classification classifies all women admitted for delivery into one of 10
    /// mutually exclusive groups based on five basic obstetric characteristics:
    /// 1. Parity (nulliparous vs multiparous)
    /// 2. Previous cesarean section
    /// 3. Gestational age (≥37 weeks vs <37 weeks)
    /// 4. Onset of labor (spontaneous, induced, or cesarean before labor)
    /// 5. Fetal presentation (cephalic, breech, transverse/oblique)
    /// 6. Number of fetuses (singleton vs multiple)
    /// </summary>
    public class RobsonClassification
    {
        public Guid? ID { get; set; }
        public Guid? PartographID { get; set; }
        public DateTime ClassifiedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("PartographID")]
        [JsonIgnore]
        public Partograph? Partograph { get; set; }

        // Classification Inputs (WHO Robson Criteria)
        /// <summary>
        /// Parity: Number of previous births (0 = nulliparous, ≥1 = multiparous)
        /// </summary>
        public int Parity { get; set; }

        /// <summary>
        /// Whether the woman is nulliparous (no previous births at ≥22 weeks)
        /// </summary>
        public bool IsNulliparous => Parity == 0;

        /// <summary>
        /// Whether the woman has had a previous cesarean section
        /// </summary>
        public bool HasPreviousCesarean { get; set; }

        /// <summary>
        /// Number of previous cesarean sections
        /// </summary>
        public int NumberOfPreviousCesareans { get; set; }

        /// <summary>
        /// Gestational age in completed weeks at delivery
        /// </summary>
        public int GestationalAgeWeeks { get; set; }

        /// <summary>
        /// Whether pregnancy is term (≥37 weeks)
        /// </summary>
        public bool IsTerm => GestationalAgeWeeks >= 37;

        /// <summary>
        /// Whether pregnancy is preterm (<37 weeks)
        /// </summary>
        public bool IsPreterm => GestationalAgeWeeks < 37;

        /// <summary>
        /// Onset of labor type
        /// </summary>
        public LaborOnsetType LaborOnset { get; set; } = LaborOnsetType.Spontaneous;

        /// <summary>
        /// Fetal presentation at delivery
        /// </summary>
        public FetalPresentationType FetalPresentation { get; set; } = FetalPresentationType.Cephalic;

        /// <summary>
        /// Number of fetuses
        /// </summary>
        public int NumberOfFetuses { get; set; } = 1;

        /// <summary>
        /// Whether pregnancy is singleton
        /// </summary>
        public bool IsSingleton => NumberOfFetuses == 1;

        /// <summary>
        /// Whether pregnancy is multiple (twins, triplets, etc.)
        /// </summary>
        public bool IsMultiple => NumberOfFetuses > 1;

        // Classification Output
        /// <summary>
        /// The Robson Group (1-10) assigned to this delivery
        /// </summary>
        public RobsonGroup Group { get; set; } = RobsonGroup.Unclassified;

        /// <summary>
        /// Delivery mode (for CS rate calculation)
        /// </summary>
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.SpontaneousVaginal;

        /// <summary>
        /// Whether delivery was by cesarean section
        /// </summary>
        public bool IsCesareanSection => DeliveryMode == DeliveryMode.CaesareanSection;

        /// <summary>
        /// CS indication if cesarean was performed
        /// </summary>
        public string CesareanIndication { get; set; } = string.Empty;

        /// <summary>
        /// Whether CS was emergency or elective
        /// </summary>
        public CesareanType? CesareanType { get; set; }

        // Recording Information
        public string ClassifiedBy { get; set; } = string.Empty;
        public Guid? ClassifiedByStaffID { get; set; }
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Whether classification was automatically calculated or manually assigned
        /// </summary>
        public bool IsAutoClassified { get; set; } = true;

        /// <summary>
        /// Classification validation status
        /// </summary>
        public ClassificationValidationStatus ValidationStatus { get; set; } = ClassificationValidationStatus.Pending;

        /// <summary>
        /// Reason for manual override if applicable
        /// </summary>
        public string OverrideReason { get; set; } = string.Empty;

        // Computed Properties for Display
        [NotMapped]
        [JsonIgnore]
        public string GroupDescription => GetGroupDescription(Group);

        [NotMapped]
        [JsonIgnore]
        public string GroupName => $"Group {(int)Group}";

        [NotMapped]
        [JsonIgnore]
        public string ClassificationSummary => GetClassificationSummary();

        // Sync columns
        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
        public long? DeletedTime { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string OriginDeviceId { get; set; } = string.Empty;
        public int SyncStatus { get; set; }
        public int Version { get; set; }
        public int ServerVersion { get; set; }
        public int Deleted { get; set; }
        public string ConflictData { get; set; } = string.Empty;
        public string DataHash { get; set; } = string.Empty;

        [NotMapped]
        [JsonIgnore]
        public bool HasConflict => SyncStatus == 2;

        [NotMapped]
        [JsonIgnore]
        public bool NeedsSync => SyncStatus == 0;

        /// <summary>
        /// Calculates the Robson Group based on the obstetric characteristics
        /// Reference: WHO Robson Classification Implementation Manual 2017
        /// </summary>
        public RobsonGroup CalculateRobsonGroup()
        {
            // Group 8: All multiple pregnancies (including previous CS)
            if (IsMultiple)
            {
                Group = RobsonGroup.Group8;
                return Group;
            }

            // Group 9: All transverse or oblique lies (including previous CS)
            if (FetalPresentation == FetalPresentationType.Transverse ||
                FetalPresentation == FetalPresentationType.Oblique)
            {
                Group = RobsonGroup.Group9;
                return Group;
            }

            // Group 10: All single cephalic, ≤36 weeks (including previous CS)
            if (IsSingleton && FetalPresentation == FetalPresentationType.Cephalic && IsPreterm)
            {
                Group = RobsonGroup.Group10;
                return Group;
            }

            // Groups 6 & 7: Breech presentations
            if (FetalPresentation == FetalPresentationType.Breech)
            {
                // Group 6: All nulliparous breeches
                if (IsNulliparous)
                {
                    Group = RobsonGroup.Group6;
                    return Group;
                }
                // Group 7: All multiparous breeches (including previous CS)
                else
                {
                    Group = RobsonGroup.Group7;
                    return Group;
                }
            }

            // Groups 1-5: Singleton, cephalic, term (≥37 weeks)
            if (IsSingleton && FetalPresentation == FetalPresentationType.Cephalic && IsTerm)
            {
                // Group 5: Previous CS, single cephalic, ≥37 weeks
                if (HasPreviousCesarean)
                {
                    Group = RobsonGroup.Group5;
                    return Group;
                }

                // Nulliparous without previous CS
                if (IsNulliparous)
                {
                    // Group 1: Spontaneous labor
                    if (LaborOnset == LaborOnsetType.Spontaneous)
                    {
                        Group = RobsonGroup.Group1;
                        return Group;
                    }
                    // Group 2: Induced or CS before labor
                    else
                    {
                        Group = RobsonGroup.Group2;
                        return Group;
                    }
                }
                // Multiparous without previous CS
                else
                {
                    // Group 3: Spontaneous labor
                    if (LaborOnset == LaborOnsetType.Spontaneous)
                    {
                        Group = RobsonGroup.Group3;
                        return Group;
                    }
                    // Group 4: Induced or CS before labor
                    else
                    {
                        Group = RobsonGroup.Group4;
                        return Group;
                    }
                }
            }

            // If we get here, classification could not be determined
            Group = RobsonGroup.Unclassified;
            return Group;
        }

        /// <summary>
        /// Gets the WHO standard description for each Robson group
        /// </summary>
        public static string GetGroupDescription(RobsonGroup group)
        {
            return group switch
            {
                RobsonGroup.Group1 => "Nulliparous, single cephalic, ≥37 weeks, spontaneous labour",
                RobsonGroup.Group2 => "Nulliparous, single cephalic, ≥37 weeks, induced or CS before labour",
                RobsonGroup.Group3 => "Multiparous (excluding previous CS), single cephalic, ≥37 weeks, spontaneous labour",
                RobsonGroup.Group4 => "Multiparous (excluding previous CS), single cephalic, ≥37 weeks, induced or CS before labour",
                RobsonGroup.Group5 => "Previous CS, single cephalic, ≥37 weeks",
                RobsonGroup.Group6 => "All nulliparous breeches",
                RobsonGroup.Group7 => "All multiparous breeches (including previous CS)",
                RobsonGroup.Group8 => "All multiple pregnancies (including previous CS)",
                RobsonGroup.Group9 => "All transverse or oblique lies (including previous CS)",
                RobsonGroup.Group10 => "All single cephalic, ≤36 weeks (including previous CS)",
                RobsonGroup.Unclassified => "Unable to classify - incomplete data",
                _ => "Unknown classification"
            };
        }

        /// <summary>
        /// Gets a summary of the classification criteria used
        /// </summary>
        private string GetClassificationSummary()
        {
            var sb = new StringBuilder();
            sb.Append(IsNulliparous ? "Nulliparous" : "Multiparous");
            sb.Append(HasPreviousCesarean ? $" (Prev CS: {NumberOfPreviousCesareans})" : " (No prev CS)");
            sb.Append($" | {(IsSingleton ? "Singleton" : $"Multiple ({NumberOfFetuses})")}");
            sb.Append($" | {FetalPresentation}");
            sb.Append($" | {GestationalAgeWeeks}w ({(IsTerm ? "Term" : "Preterm")})");
            sb.Append($" | {LaborOnset} onset");
            return sb.ToString();
        }

        public string CalculateHash()
        {
            var data = $"{ID}|{PartographID}|{Group}|{Parity}|{HasPreviousCesarean}|{GestationalAgeWeeks}|{LaborOnset}|{FetalPresentation}|{NumberOfFetuses}|{DeliveryMode}|{UpdatedTime}";
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hashBytes);
        }

        public void UpdateDataHash()
        {
            DataHash = CalculateHash();
        }
    }

    /// <summary>
    /// The 10 Robson Groups as defined by WHO
    /// </summary>
    public enum RobsonGroup
    {
        Unclassified = 0,
        /// <summary>
        /// Nulliparous, single cephalic, ≥37 weeks, spontaneous labour
        /// </summary>
        Group1 = 1,
        /// <summary>
        /// Nulliparous, single cephalic, ≥37 weeks, induced or CS before labour
        /// </summary>
        Group2 = 2,
        /// <summary>
        /// Multiparous (excluding previous CS), single cephalic, ≥37 weeks, spontaneous labour
        /// </summary>
        Group3 = 3,
        /// <summary>
        /// Multiparous (excluding previous CS), single cephalic, ≥37 weeks, induced or CS before labour
        /// </summary>
        Group4 = 4,
        /// <summary>
        /// Previous CS, single cephalic, ≥37 weeks
        /// </summary>
        Group5 = 5,
        /// <summary>
        /// All nulliparous breeches
        /// </summary>
        Group6 = 6,
        /// <summary>
        /// All multiparous breeches (including previous CS)
        /// </summary>
        Group7 = 7,
        /// <summary>
        /// All multiple pregnancies (including previous CS)
        /// </summary>
        Group8 = 8,
        /// <summary>
        /// All transverse or oblique lies (including previous CS)
        /// </summary>
        Group9 = 9,
        /// <summary>
        /// All single cephalic, ≤36 weeks (including previous CS)
        /// </summary>
        Group10 = 10
    }

    /// <summary>
    /// Type of labor onset for Robson Classification
    /// </summary>
    public enum LaborOnsetType
    {
        /// <summary>
        /// Spontaneous onset of labor
        /// </summary>
        Spontaneous,
        /// <summary>
        /// Induced labor (cervical ripening, oxytocin, ARM, etc.)
        /// </summary>
        Induced,
        /// <summary>
        /// Cesarean section performed before labor onset
        /// </summary>
        CesareanBeforeLabor
    }

    /// <summary>
    /// Fetal presentation types for Robson Classification
    /// </summary>
    public enum FetalPresentationType
    {
        /// <summary>
        /// Cephalic (head first) - includes vertex, face, brow
        /// </summary>
        Cephalic,
        /// <summary>
        /// Breech presentation
        /// </summary>
        Breech,
        /// <summary>
        /// Transverse lie
        /// </summary>
        Transverse,
        /// <summary>
        /// Oblique lie
        /// </summary>
        Oblique
    }

    /// <summary>
    /// Type of cesarean section
    /// </summary>
    public enum CesareanType
    {
        /// <summary>
        /// Elective/planned cesarean
        /// </summary>
        Elective,
        /// <summary>
        /// Emergency cesarean
        /// </summary>
        Emergency
    }

    /// <summary>
    /// Validation status for Robson classification
    /// </summary>
    public enum ClassificationValidationStatus
    {
        /// <summary>
        /// Classification pending validation
        /// </summary>
        Pending,
        /// <summary>
        /// Classification validated and confirmed
        /// </summary>
        Validated,
        /// <summary>
        /// Classification needs review - data inconsistency
        /// </summary>
        NeedsReview,
        /// <summary>
        /// Classification manually overridden
        /// </summary>
        ManualOverride
    }
}
