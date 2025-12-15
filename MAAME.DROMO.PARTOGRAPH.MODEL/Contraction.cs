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
    }
}
