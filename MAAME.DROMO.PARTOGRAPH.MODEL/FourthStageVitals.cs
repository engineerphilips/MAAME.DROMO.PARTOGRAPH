using System;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    /// <summary>
    /// Fourth Stage Vitals - WHO 2020 Postpartum Monitoring
    /// Records fundal height, bleeding, bladder, and uterine status every 15 minutes for 2 hours
    /// </summary>
    public class FourthStageVitals : BasePartographMeasurement
    {
        // Fundal Height Assessment
        public FundalHeightStatus FundalHeight { get; set; } = FundalHeightStatus.AtUmbilicus;
        public string FundalHeightNotes { get; set; } = string.Empty;

        // Bleeding Assessment
        public BleedingStatus BleedingStatus { get; set; } = BleedingStatus.NormalLochia;
        public int? EstimatedBloodLossMl { get; set; }
        public bool ClotsPresent { get; set; }
        public string BleedingNotes { get; set; } = string.Empty;

        // Uterine Status
        public UterineStatus UterineStatus { get; set; } = UterineStatus.Firm;
        public bool UterineMassage { get; set; }
        public string UterineNotes { get; set; } = string.Empty;

        // Bladder Status
        public BladderStatus BladderStatus { get; set; } = BladderStatus.Empty;
        public bool CatheterizationRequired { get; set; }
        public string BladderNotes { get; set; } = string.Empty;

        // PPH Risk Assessment
        public bool PPHRisk { get; set; }
        public bool PPHProtocolActivated { get; set; }
        public bool UterotonicGiven { get; set; }
        public string UterotonicType { get; set; } = string.Empty;
        public DateTime? UterotonicTime { get; set; }

        // Perineal Status
        public bool PerinealPainControlled { get; set; } = true;
        public bool PerinealSwelling { get; set; }
        public bool PerinealHematoma { get; set; }

        // General Wellbeing
        public bool MaternalComfortable { get; set; } = true;
        public bool BondingInitiated { get; set; }
        public bool BreastfeedingInitiated { get; set; }
        public bool SkinToSkinContact { get; set; }

        // Vital Signs Reference (linking to BP and Temperature tables)
        public Guid? AssociatedBPId { get; set; }
        public Guid? AssociatedTemperatureId { get; set; }

        // Alert/Warning flags
        public bool RequiresAttention { get; set; }
        public string AlertMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Fundal Height Status - Position relative to umbilicus
    /// </summary>
    public enum FundalHeightStatus
    {
        AtUmbilicus,
        OneFingerBelow,
        TwoFingersBelow,
        ThreeFingersBelow,
        AboveUmbilicus,      // Concern - possible uterine atony or full bladder
        NotPalpable          // Very rare, may indicate postpartum collapse
    }

    /// <summary>
    /// Bleeding Status Assessment
    /// </summary>
    public enum BleedingStatus
    {
        NormalLochia,        // Normal rubra lochia
        Minimal,             // Less than expected
        Moderate,            // Within normal range
        Heavy,               // PPH warning
        Excessive,           // PPH - requires immediate action
        Clots               // Significant clots present
    }

    /// <summary>
    /// Uterine Status Assessment
    /// </summary>
    public enum UterineStatus
    {
        Firm,               // Well contracted - normal
        ModeratelyFirm,     // Acceptable
        Soft,               // Concern - monitor closely
        Boggy,              // Atony risk - requires massage/intervention
        NotPalpable         // Immediate concern
    }

    /// <summary>
    /// Bladder Status Assessment
    /// </summary>
    public enum BladderStatus
    {
        Empty,              // Normal - voided or catheterized
        VoidedSpontaneously,// Patient urinated naturally
        Palpable,           // Full bladder - needs attention
        Distended,          // Significantly full - may affect uterine contraction
        Catheterized        // Indwelling catheter in place
    }
}
