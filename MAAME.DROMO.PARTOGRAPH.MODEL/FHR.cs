namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Fetal Heart Rate - WHO Labour Care Guide Section 3: Care of the Baby
    // WHO 2020: Check FHR every 30 minutes in first stage, every 15 minutes in second stage
    public class FHR : BasePartographMeasurement
    {
        // Multiple Pregnancy Support - identifies which fetus this FHR reading belongs to
        // Aligns with BabyDetails.BabyNumber convention: 1 for singleton, 1,2,3... for multiples
        public int BabyNumber { get; set; } = 1;
        public string BabyTag { get; set; } = string.Empty; // e.g., "Baby A", "Twin 1", "Triplet 2"

        // Baseline Rate (WHO normal range: 110-160 bpm)
        public int? Rate { get; set; } // 110-160 normal

        // Deceleration Type (WHO: important for fetal wellbeing assessment)
        public string Deceleration { get; set; } = "None"; // None, Early, Late, Variable, Prolonged
        public int DecelerationDurationSeconds { get; set; } // Prolonged if >120 seconds

        // Variability (WHO: critical for CTG interpretation - indicates fetal neurological status)
        public string Variability { get; set; } = string.Empty; // Absent, Minimal, Moderate, Marked

        // Accelerations (WHO: presence indicates fetal wellbeing)
        public bool Accelerations { get; set; }

        // Overall Pattern Classification (WHO: guides clinical action)
        public string Pattern { get; set; } = string.Empty; // Reassuring, NonReassuring, Abnormal

        // Monitoring Method (WHO: document method used)
        public string MonitoringMethod { get; set; } = string.Empty; // Auscultation, Doppler, ContinuousCTG

        // Baseline FHR (separate from instantaneous rate)
        public int? BaselineRate { get; set; }

        // WHO Alert Thresholds
        // Bradycardia: <110 bpm, Critical: <100 bpm
        // Tachycardia: >160 bpm, Critical: >180 bpm
        public string ClinicalAlert { get; set; } = string.Empty;

        // WHO 2020 Enhanced FHR Assessment
        // Detailed Variability Assessment
        public int? VariabilityBpm { get; set; } // Numeric value in bpm
        public string VariabilityTrend { get; set; } = string.Empty; // Increasing, Decreasing, Stable
        public bool SinusoidalPattern { get; set; }
        public bool SaltatorPattern { get; set; }

        // Acceleration Details
        public int? AccelerationCount { get; set; }
        public int? AccelerationPeakBpm { get; set; }
        public int? AccelerationDurationSeconds { get; set; }

        // Deceleration Details
        public int? DecelerationNadirBpm { get; set; }
        public string DecelerationRecovery { get; set; } = string.Empty; // Rapid, Slow, Incomplete
        public int? DecelerationAmplitudeBpm { get; set; }
        public string DecelerationTiming { get; set; } = string.Empty; // EarlyPeak, LatePeak, VariableTiming

        // Bradycardia/Tachycardia
        public bool ProlongedBradycardia { get; set; }
        public DateTime? BradycardiaStartTime { get; set; }
        public int? BradycardiaDurationMinutes { get; set; }
        public bool Tachycardia { get; set; }
        public DateTime? TachycardiaStartTime { get; set; }
        public int? TachycardiaDurationMinutes { get; set; }

        // CTG Interpretation (when CTG used)
        public string CTGClassification { get; set; } = string.Empty; // Category1, Category2, Category3
        public bool ReactiveNST { get; set; }
        public DateTime? LastReactiveTime { get; set; }

        // Maternal Context
        public string MaternalPosition { get; set; } = string.Empty; // LeftLateral, Supine, Upright, etc.
        public bool DuringContraction { get; set; }
        public bool BetweenContractions { get; set; }

        // Clinical Response
        public bool InterventionRequired { get; set; }
        public string InterventionTaken { get; set; } = string.Empty;
        public DateTime? InterventionTime { get; set; }
        public bool ChangeInPosition { get; set; }
        public bool OxygenAdministered { get; set; }
        public bool IVFluidsIncreased { get; set; }

        // Obstetric Emergency Indicators
        public bool EmergencyConsultRequired { get; set; }
        public string ConsultReason { get; set; } = string.Empty;
        public DateTime? ConsultTime { get; set; }
        public bool PrepareForEmergencyDelivery { get; set; }

        // Display Property for backward compatibility
        public string? FHRDisplay => Rate?.ToString() ?? string.Empty;
    }
}
