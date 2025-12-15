using System;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // IV Fluid Management - WHO Labour Care Guide Section 4: Care of the Woman
    // WHO 2020: Monitor fluid balance carefully to prevent overload/dehydration
    // Document IV fluids every 4 hours or more frequently if indicated
    public class IVFluidEntry : BasePartographMeasurement
    {
        // Fluid Details
        public string FluidType { get; set; } = string.Empty; // NormalSaline, Hartmanns, Dextrose5, Dextrose10, RingersLactate

        // Volume and Rate
        public int VolumeInfused { get; set; } // Total volume infused this entry (ml)
        public decimal RateMlPerHour { get; set; } // Infusion rate (ml/hr)

        // Timing
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? DurationMinutes { get; set; }

        // Additives (WHO: oxytocin often added to IV fluids for augmentation/3rd stage)
        public string Additives { get; set; } = string.Empty; // None, KCl, Syntocinon, MgSO4, Antibiotic

        // Additive Details
        public string AdditiveConcentration { get; set; } = string.Empty;
        public string AdditiveDose { get; set; } = string.Empty;

        // IV Site Assessment (patient safety - prevent complications)
        public string IVSite { get; set; } = string.Empty; // LeftHand, RightHand, LeftForearm, RightForearm, LeftACF, RightACF
        public bool SiteHealthy { get; set; } = true;
        public string SiteCondition { get; set; } = string.Empty; // Clean, Inflamed, Swollen, Tender, Extravasation, Phlebitis

        // Phlebitis Score (VIP Score: 0-5)
        public int PhlebitisScore { get; set; } // 0=NoSymptoms, 1=Slight, 2=EarlyPhlebitis, 3=MediumPhlebitis, 4-5=AdvancedPhlebitis

        // Site Care
        public DateTime? LastSiteAssessment { get; set; }
        public DateTime? LastDressingChange { get; set; }
        public DateTime? CannelaInsertionDate { get; set; }

        // Indication for IV Fluids
        public string Indication { get; set; } = string.Empty; // Hydration, Medication, Augmentation, Resuscitation, NBM

        // Batch/Lot Number (for audit and recall)
        public string BatchNumber { get; set; } = string.Empty;

        // Fluid Balance Tracking
        public int RunningTotalInput { get; set; } // Cumulative IV input for this labor (ml)

        // WHO 2020 Alert Status
        public string ClinicalAlert { get; set; } = string.Empty;
        // Fluid overload: >2500ml without adequate output
        // Phlebitis score ≥2 requires site change
        // Cannula >72 hours old requires replacement
        // Site extravasation requires immediate removal
    }
}
