namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Contractions - WHO Labour Care Guide Section 5: Labour Progress
    // WHO 2020: Monitor every 30 minutes in first stage, every 15 minutes in second stage
    // WHO Alert: Contractions lasting ≥20 or ≤60 seconds when assessed every 30 minutes
    public class Contraction : BasePartographMeasurement
    {
        // Frequency (normal active labor: 3-5 contractions per 10 minutes)
        public int FrequencyPer10Min { get; set; } // Range: 0-15

        // Duration (normal: 20-60 seconds)
        public int DurationSeconds { get; set; } // Range: 10-180 seconds

        // Strength (critical for labor assessment and oxytocin management)
        public string Strength { get; set; } = "Moderate"; // Mild, Moderate, Strong

        // Regularity
        public string Regularity { get; set; } = "Regular"; // Regular, Irregular, Coupling

        // Palpable at Rest (indicates hypertonic uterus)
        public bool PalpableAtRest { get; set; }

        // Coordination
        public bool Coordinated { get; set; } = true;

        // Effect on Cervix (when correlated with VE findings)
        public string EffectOnCervix { get; set; } = string.Empty; // Effacing, Dilating, NoChange

        // Intensity (if IUPC used)
        public int? IntensityMmHg { get; set; }

        // Clinical Alerts
        public string ClinicalAlert { get; set; } = string.Empty;
        // Tachysystole: >5 per 10 minutes
        // Hyperstimulation: >10 per 10 minutes (critical - reduce/stop oxytocin)
        // Prolonged: >60 seconds
        // Inadequate: <3 per 10 minutes in active labor

        // WHO 2020 Enhanced Contraction Assessment
        // Pattern Analysis
        public string ContractionPattern { get; set; } = string.Empty; // Regular, Irregular, Coupling, Triplets
        public bool Tachysystole { get; set; } // >5 contractions per 10 min
        public bool Hyperstimulation { get; set; } // Tachysystole with Category 2/3 FHR
        public DateTime? TachysystoleStartTime { get; set; }
        public int? TachysystoleDurationMinutes { get; set; }

        // Intensity Details
        public string IntensityAssessment { get; set; } = string.Empty; // VeryMild, Mild, Moderate, Strong, VeryStrong
        public bool IndentableDuringContraction { get; set; }
        public bool UterusRelaxesBetweenContractions { get; set; } = true;
        public int? RelaxationTimeSeconds { get; set; }

        // IUPC Measurements (if available)
        public int? RestingToneMmHg { get; set; } // Normal: 5-15 mmHg
        public int? PeakPressureMmHg { get; set; }
        public int? MontevideUnits { get; set; } // MVU = sum of peak pressure minus baseline over 10 min

        // Duration Patterns
        public int? ShortestDurationSeconds { get; set; }
        public int? LongestDurationSeconds { get; set; }
        public int? AverageDurationSeconds { get; set; }
        public bool ProlongedContractions { get; set; } // >90 seconds
        public int? ProlongedContractionCount { get; set; }

        // Frequency Patterns
        public string FrequencyTrend { get; set; } = string.Empty; // Increasing, Decreasing, Stable
        public bool IrregularFrequency { get; set; }
        public decimal? AverageIntervalMinutes { get; set; }

        // Maternal Response
        public string MaternalCopingLevel { get; set; } = string.Empty; // CopingWell, SomeDistress, SignificantDistress
        public bool MaternalExhaustion { get; set; }
        public string PainLocation { get; set; } = string.Empty; // Abdomen, Back, Both

        // Oxytocin Relationship
        public bool OnOxytocin { get; set; }
        public string OxytocinEffect { get; set; } = string.Empty; // Adequate, Excessive, Inadequate
        public bool OxytocinAdjustmentNeeded { get; set; }
        public string SuggestedOxytocinAction { get; set; } = string.Empty; // Maintain, Increase, Decrease, Stop

        // Clinical Management
        public bool InterventionRequired { get; set; }
        public string InterventionTaken { get; set; } = string.Empty;
        public DateTime? InterventionTime { get; set; }
        public bool OxytocinStopped { get; set; }
        public bool OxytocinReduced { get; set; }
        public bool TocolyticsGiven { get; set; }

        // Safety Alerts
        public bool HypertonicUterus { get; set; }
        public bool UterineRuptureRisk { get; set; }
        public bool FHRCompromise { get; set; }
        public bool EmergencyConsultRequired { get; set; }

        // Display Property
        public string? ContractionDisplay => $"{FrequencyPer10Min}/10min, {DurationSeconds}s, {Strength}";
    }
}
