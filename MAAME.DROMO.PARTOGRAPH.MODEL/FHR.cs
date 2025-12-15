namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Fetal Heart Rate - WHO Labour Care Guide Section 3: Care of the Baby
    // WHO 2020: Check FHR every 30 minutes in first stage, every 15 minutes in second stage
    public class FHR : BasePartographMeasurement
    {
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
    }
}
