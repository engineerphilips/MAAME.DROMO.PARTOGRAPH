namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Blood Pressure and Pulse - WHO Labour Care Guide Section 4: Care of the Woman
    // WHO 2020: Monitor every 4 hours if normal
    // Alert thresholds: Systolic <80 or ≥140 mmHg, Diastolic ≥90 mmHg, Pulse <60 or ≥120 bpm
    public class BP : BasePartographMeasurement
    {
        // Blood Pressure Measurements
        public int Systolic { get; set; } // mmHg
        public int Diastolic { get; set; } // mmHg

        // Pulse Rate
        public int Pulse { get; set; } // bpm

        // Maternal Position (affects BP reading accuracy)
        public string MaternalPosition { get; set; } = "Sitting"; // Sitting, LeftLateral, Supine, Standing

        // Cuff Size (affects reading accuracy)
        public string CuffSize { get; set; } = "Standard"; // Standard, Large, Obese

        // Repeat Measurement (if abnormal reading)
        public bool RepeatMeasurement { get; set; }

        // Irregular Pulse
        public bool IrregularPulse { get; set; }

        // Calculated Values
        public decimal MAP // Mean Arterial Pressure
        {
            get
            {
                return (Systolic + 2 * Diastolic) / 3m;
            }
        }

        public int PulsePressure
        {
            get
            {
                return Systolic - Diastolic;
            }
        }

        // WHO 2020 Alert Status
        public string ClinicalAlert { get; set; } = string.Empty;
        // Severe Hypertension: Systolic ≥160 or Diastolic ≥110
        // Hypertension Warning: Systolic ≥140 or Diastolic ≥90
        // Hypotension: Systolic <80
        // Tachycardia: Pulse ≥120 bpm
        // Bradycardia: Pulse <60 bpm
    }
}
