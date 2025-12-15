namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Cervical Dilatation - WHO Labour Care Guide Section 5: Labour Progress
    // WHO 2020: Active labor starts at 5cm (not 4cm as in old partograph)
    // Document during vaginal examination
    public class CervixDilatation : BasePartographMeasurement
    {
        // Dilatation (0-10 cm)
        public int DilatationCm { get; set; } // Range 0-10, cannot regress

        // Effacement (0-100%) - Required for Bishop Score
        public int EffacementPercent { get; set; }

        // Cervical Consistency - Required for Bishop Score
        public string Consistency { get; set; } = "Medium"; // Firm, Medium, Soft

        // Cervical Position - Required for Bishop Score
        public string Position { get; set; } = "Mid"; // Posterior, Mid, Anterior

        // Application to Presenting Part
        public bool ApplicationToHead { get; set; }

        // Cervical Edema
        public string CervicalEdema { get; set; } = "None"; // None, Slight, Moderate, Marked

        // Membrane Status
        public string MembraneStatus { get; set; } = string.Empty; // Intact, Bulging, Ruptured

        // Cervical Lip
        public bool CervicalLip { get; set; }

        // Labor Phase Indicator (WHO 2020 definition)
        // Latent: <5cm, Active: ≥5cm
        public string LaborPhase
        {
            get
            {
                if (DilatationCm < 5) return "Latent";
                if (DilatationCm < 10) return "Active";
                return "FullyDilated";
            }
        }

        // WHO 2020 Enhanced Cervical Assessment
        // Progress Tracking
        public decimal? DilatationRateCmPerHour { get; set; }
        public string ProgressionRate { get; set; } = string.Empty; // Normal, Slow, Rapid, Arrested
        public bool CrossedActionLine { get; set; }
        public bool CrossedAlertLine { get; set; }
        public DateTime? ActionLineCrossedTime { get; set; }

        // Cervical Length
        public decimal? CervicalLengthCm { get; set; }

        // Examination Details
        public string ExaminerName { get; set; } = string.Empty;
        public int? ExamDurationMinutes { get; set; }
        public bool DifficultExam { get; set; }
        public string ExamDifficulty { get; set; } = string.Empty;

        // Cervical Features
        public string CervicalThickness { get; set; } = string.Empty; // Thick, Medium, Thin, PaperThin
        public bool AnteriorCervicalLip { get; set; }
        public bool PosteriorCervicalLip { get; set; }
        public string CervicalDilatationPattern { get; set; } = string.Empty; // Uniform, Irregular

        // Presenting Part Relationship
        public int? StationRelativeToPelvicSpines { get; set; } // -5 to +5
        public string PresentingPartPosition { get; set; } = string.Empty; // OA, OP, LOA, ROA, etc.
        public bool PresentingPartWellApplied { get; set; }

        // Membrane Assessment
        public bool MembranesBulging { get; set; }
        public bool ForewatersPresent { get; set; }
        public bool HindwatersPresent { get; set; }

        // Clinical Alerts
        public string ClinicalAlert { get; set; } = string.Empty;
        public bool ProlongedLatentPhase { get; set; }
        public bool ProtractedActivePhase { get; set; }
        public bool ArrestedDilatation { get; set; }
        public bool PrecipitousLabor { get; set; }

        // Display Property
        public string? CervixDisplay => $"{DilatationCm}cm, {EffacementPercent}%";
    }
}
