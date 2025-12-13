using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    /// <summary>
    /// Records detailed information about each baby born
    /// Based on WHO Maternal and Perinatal Health Standards for Improved Quality of Care 2020
    /// WHO ISBN: 978-92-4-001756-6 (electronic version)
    /// </summary>
    public class BabyDetails
    {
        public Guid? ID { get; set; }
        public Guid? PartographID { get; set; }
        public Guid? BirthOutcomeID { get; set; }

        // Baby Identification
        public int BabyNumber { get; set; } = 1; // 1 for singleton, 1,2,3 for multiples
        public string BabyTag { get; set; } = string.Empty; // e.g., "Baby A", "Twin 1"

        // Birth Information
        public DateTime BirthTime { get; set; } = DateTime.Now;
        public BabySex Sex { get; set; } = BabySex.Unknown;

        // Vital Status
        public BabyVitalStatus VitalStatus { get; set; } = BabyVitalStatus.LiveBirth;
        public DateTime? DeathTime { get; set; }
        public string DeathCause { get; set; } = string.Empty;
        public bool StillbirthMacerated { get; set; } = false; // For stillbirths

        // Anthropometric Measurements (WHO 2020 - Essential newborn care)
        public decimal BirthWeight { get; set; } // in grams (WHO recommends weighing within 1 hour)
        public decimal Length { get; set; } // in cm (crown-heel length)
        public decimal HeadCircumference { get; set; } // in cm (occipitofrontal)
        public decimal ChestCircumference { get; set; } // in cm

        // APGAR Scores (WHO 2020 - Pages 89-90)
        // Score components: Heart rate, Respiratory effort, Muscle tone, Reflex irritability, Color
        // Score interpretation: 7-10 (normal), 4-6 (moderately abnormal), 0-3 (severely abnormal)
        public int? Apgar1Min { get; set; } // At 1 minute
        public int? Apgar5Min { get; set; } // At 5 minutes
        //public int? Apgar10Min { get; set; } // At 10 minutes (if 5-min score <7)

        // APGAR 1-Minute Component Scores (0-2 each)
        public int? Apgar1HeartRate { get; set; } // 0=Absent, 1=<100bpm, 2=≥100bpm
        public int? Apgar1RespiratoryEffort { get; set; } // 0=Absent, 1=Slow/irregular, 2=Good/crying
        public int? Apgar1MuscleTone { get; set; } // 0=Limp, 1=Some flexion, 2=Active motion
        public int? Apgar1ReflexIrritability { get; set; } // 0=No response, 1=Grimace, 2=Cry/sneeze/cough
        public int? Apgar1Color { get; set; } // 0=Blue/pale, 1=Body pink extremities blue, 2=Completely pink

        // APGAR 5-Minute Component Scores (0-2 each)
        public int? Apgar5HeartRate { get; set; } // 0=Absent, 1=<100bpm, 2=≥100bpm
        public int? Apgar5RespiratoryEffort { get; set; } // 0=Absent, 1=Slow/irregular, 2=Good/crying
        public int? Apgar5MuscleTone { get; set; } // 0=Limp, 1=Some flexion, 2=Active motion
        public int? Apgar5ReflexIrritability { get; set; } // 0=No response, 1=Grimace, 2=Cry/sneeze/cough
        public int? Apgar5Color { get; set; } // 0=Blue/pale, 1=Body pink extremities blue, 2=Completely pink

        // Resuscitation (WHO 2020 - Pages 91-93)
        public bool ResuscitationRequired { get; set; } = false;
        public string ResuscitationSteps { get; set; } = string.Empty; // e.g., "Stimulation only", "Bag-mask ventilation", "Intubation"
        public int? ResuscitationDuration { get; set; } // in minutes
        public bool OxygenGiven { get; set; } = false;
        public bool IntubationPerformed { get; set; } = false;
        public bool ChestCompressionsGiven { get; set; } = false;
        public bool MedicationsGiven { get; set; } = false;
        public string MedicationDetails { get; set; } = string.Empty; // e.g., "Epinephrine 0.1ml IV"

        // Immediate Newborn Care (WHO 2020 - Pages 88-89)
        //public bool SkinToSkinContact { get; set; } = true; // Immediate uninterrupted skin-to-skin
        public bool EarlyBreastfeedingInitiated { get; set; } = true; // Within 1 hour
        public bool DelayedCordClamping { get; set; } = true; // 1-3 minutes recommended
        public int? CordClampingTime { get; set; } // in seconds
        public bool VitaminKGiven { get; set; } = true; // 1mg IM recommended
        public bool EyeProphylaxisGiven { get; set; } = true; // Prevent ophthalmia neonatorum
        public bool HepatitisBVaccineGiven { get; set; } = false;

        // Thermal Care (WHO 2020 - Page 94)
        public decimal? FirstTemperature { get; set; } // Within 1 hour
        public bool KangarooMotherCare { get; set; } = false; // For LBW babies

        // Birth Classification
        public BirthWeightClassification WeightClassification { get; set; } = BirthWeightClassification.Normal;
        public GestationalAgeClassification GestationalClassification { get; set; } = GestationalAgeClassification.Term;

        // Congenital Abnormalities
        public bool CongenitalAbnormalitiesPresent { get; set; } = false;
        public string CongenitalAbnormalitiesDescription { get; set; } = string.Empty;

        // Birth Injuries
        public bool BirthInjuriesPresent { get; set; } = false;
        public string BirthInjuriesDescription { get; set; } = string.Empty;

        // Clinical Status at Birth
        //public bool Breathing { get; set; } = true;
        //public bool Crying { get; set; } = true;
        //public bool GoodMuscleTone { get; set; } = true;
        //public SkinColor SkinColor { get; set; } = SkinColor.Pink;

        // Special Care
        public bool RequiresSpecialCare { get; set; } = false;
        public string SpecialCareReason { get; set; } = string.Empty;
        public bool AdmittedToNICU { get; set; } = false;
        public DateTime? NICUAdmissionTime { get; set; }

        // Feeding
        public FeedingMethod FeedingMethod { get; set; } = FeedingMethod.Breastfeeding;
        public string FeedingNotes { get; set; } = string.Empty;

        // Complications
        public bool AsphyxiaNeonatorum { get; set; } = false;
        public bool RespiratorydistressSyndrome { get; set; } = false;
        public bool Sepsis { get; set; } = false;
        public bool Jaundice { get; set; } = false;
        public bool Hypothermia { get; set; } = false;
        public bool Hypoglycemia { get; set; } = false;
        public string OtherComplications { get; set; } = string.Empty;

        // Recording Information
        public string HandlerName { get; set; } = string.Empty;
        public Guid? Handler { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Navigation Properties
        [JsonIgnore]
        public Partograph Partograph { get; set; }

        [JsonIgnore]
        public BirthOutcome BirthOutcome { get; set; }

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

        [NotMapped]
        [JsonIgnore]
        public bool IsLowBirthWeight => BirthWeight < 2500;

        [NotMapped]
        [JsonIgnore]
        public bool IsVeryLowBirthWeight => BirthWeight < 1500;

        [NotMapped]
        [JsonIgnore]
        public bool IsExtremelyLowBirthWeight => BirthWeight < 1000;

        [NotMapped]
        [JsonIgnore]
        public string WeightStatus
        {
            get
            {
                if (BirthWeight < 1000) return "Extremely Low Birth Weight";
                if (BirthWeight < 1500) return "Very Low Birth Weight";
                if (BirthWeight < 2500) return "Low Birth Weight";
                if (BirthWeight > 4000) return "Macrosomia";
                return "Normal Birth Weight";
            }
        }

        public string CalculateHash()
        {
            var data = $"{ID}|{PartographID}|{BirthWeight}|{VitalStatus}";
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }

    public enum BabySex
    {
        Male,
        Female,
        Ambiguous,
        Unknown
    }

    public enum BabyVitalStatus
    {
        LiveBirth,
        FreshStillbirth, // Intrapartum death
        MaceratedStillbirth, // Antepartum death
        EarlyNeonatalDeath, // Death within 7 days
        Survived
    }

    public enum BirthWeightClassification
    {
        ExtremelyLowBirthWeight, // <1000g
        VeryLowBirthWeight, // 1000-1499g
        LowBirthWeight, // 1500-2499g
        Normal, // 2500-3999g
        Macrosomia // ≥4000g
    }

    public enum GestationalAgeClassification
    {
        ExtremelyPreterm, // <28 weeks
        VeryPreterm, // 28-31 weeks
        ModeratePreterm, // 32-33 weeks
        LatePreterm, // 34-36 weeks
        Term, // 37-41 weeks
        PostTerm // ≥42 weeks
    }

    public enum SkinColor
    {
        Pink, // Normal
        Pale,
        Cyanotic, // Blue (central cyanosis)
        AcrocyanoticOnly, // Blue hands/feet only (normal in first 24h)
        Jaundiced
    }

    public enum FeedingMethod
    {
        Breastfeeding,
        FormulaFeeding,
        MixedFeeding,
        TubeFeeding,
        IVFluids,
        NotFeedingYet
    }
}
