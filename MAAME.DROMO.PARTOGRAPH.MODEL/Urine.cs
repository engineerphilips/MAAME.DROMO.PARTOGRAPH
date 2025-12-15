namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Urine Output and Assessment - WHO Labour Care Guide Section 4: Care of the Woman
    // WHO recommends documenting urine output and characteristics during labour
    public class Urine : BasePartographMeasurement
    {
        // Urine Output (critical for fluid balance and renal function)
        public int OutputMl { get; set; }

        // Urine Color
        public string Color { get; set; } = "Yellow"; // Clear, Yellow, DarkYellow, Brown, Red

        // Protein (pre-eclampsia screening)
        public string Protein { get; set; } = "Nil"; // Nil, Trace, Plus1, Plus2, Plus3

        // Ketones (maternal exhaustion/dehydration indicator)
        public string Ketones { get; set; } = "Nil"; // Nil, Trace, Plus1, Plus2, Plus3

        // Glucose
        public string Glucose { get; set; } = "Nil"; // Nil, Trace, Plus1, Plus2, Plus3

        // Specific Gravity (concentration)
        public string SpecificGravity { get; set; } = string.Empty;

        // Voiding Method
        public string VoidingMethod { get; set; } = "Spontaneous"; // Spontaneous, Catheterized, Suprapubic

        // Bladder Assessment
        public bool BladderPalpable { get; set; }
        public DateTime? LastVoided { get; set; }

        // Clinical Alerts
        public string ClinicalAlert { get; set; } = string.Empty;
        // Protein ≥Plus2 suggests pre-eclampsia screening needed
        // Ketones ≥Plus2 indicates prolonged labor/dehydration
        // Output <30ml/hour for 2+ hours = oliguria (renal concern)

        // WHO 2020 Enhanced Urine Assessment
        // Volume and Pattern
        public DateTime? VoidingTime { get; set; }
        public int? TimeSinceLastVoidMinutes { get; set; }
        public int? CumulativeOutputMl { get; set; }
        public decimal? HourlyOutputRate { get; set; } // ml/hour
        public bool Oliguria { get; set; } // <30 ml/hour
        public bool Anuria { get; set; }
        public int? ConsecutiveOliguriaHours { get; set; }

        // Appearance
        public string Clarity { get; set; } = string.Empty; // Clear, Cloudy, Turbid
        public bool Hematuria { get; set; }
        public bool Concentrated { get; set; }
        public bool Dilute { get; set; }
        public string Odor { get; set; } = string.Empty; // Normal, Offensive, Fruity

        // Dipstick Results
        public string BloodDipstick { get; set; } = "Nil";
        public string LeukocytesDipstick { get; set; } = "Nil";
        public string NitritesDipstick { get; set; } = "Nil";
        public float? PHLevel { get; set; }

        // Bladder Management
        public bool BladderFullness { get; set; }
        public string BladderFullnessLevel { get; set; } = string.Empty; // Empty, Quarter, Half, ThreeQuarters, Full
        public bool DifficultVoiding { get; set; }
        public bool UrinaryRetention { get; set; }
        public bool CatheterizationIndicated { get; set; }
        public DateTime? LastCatheterizationTime { get; set; }
        public string CatheterType { get; set; } = string.Empty; // Intermittent, Indwelling

        // Pre-eclampsia Monitoring
        public bool ProteinuriaNewOnset { get; set; }
        public bool ProteinuriaWorsening { get; set; }
        public DateTime? FirstProteinDetectedTime { get; set; }
        public bool LaboratorySampleSent { get; set; }
        public string ProteinCreatinineRatio { get; set; } = string.Empty;

        // Dehydration/Ketosis Assessment
        public bool SignsOfDehydration { get; set; }
        public bool ProlongedLabor { get; set; }
        public bool IncreasedKetoneTrend { get; set; }
        public DateTime? FirstKetoneDetectedTime { get; set; }

        // Fluid Balance
        public int? TotalOralIntakeMl { get; set; }
        public int? TotalIVIntakeMl { get; set; }
        public int? FluidBalanceMl { get; set; }

        // Clinical Response
        public bool EncourageOralFluids { get; set; }
        public bool IVFluidsStarted { get; set; }
        public bool CatheterizationPerformed { get; set; }
        public bool NephrologyConsultRequired { get; set; }

        // Display
        public string? UrineDisplay => $"{OutputMl}ml, Protein:{Protein}, Acetone:{Ketones}";
    }
}
