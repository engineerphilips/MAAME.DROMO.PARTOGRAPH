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

        // WHO 2020 Enhanced BP Assessment
        // Hypertension Classification
        public string BPCategory { get; set; } = string.Empty; // Normal, Elevated, Stage1, Stage2, Severe
        public bool SevereHypertension { get; set; } // ≥160/110
        public bool PreeclampsiaRange { get; set; } // ≥140/90
        public DateTime? FirstElevatedBPTime { get; set; }
        public int? ConsecutiveElevatedReadings { get; set; }

        // Repeat Measurements
        public int? SecondSystolic { get; set; }
        public int? SecondDiastolic { get; set; }
        public DateTime? SecondReadingTime { get; set; }
        public int? ThirdSystolic { get; set; }
        public int? ThirdDiastolic { get; set; }
        public DateTime? ThirdReadingTime { get; set; }

        // Pulse Details
        public string PulseRhythm { get; set; } = string.Empty; // Regular, Irregular, Regularly Irregular
        public string PulseVolume { get; set; } = string.Empty; // Normal, Weak, Bounding
        public string PulseCharacter { get; set; } = string.Empty; // Normal, Thready, Collapsing
        public bool PulseDeficit { get; set; }

        // Hypotension Assessment
        public bool Hypotension { get; set; }
        public string HypotensionCause { get; set; } = string.Empty; // Hemorrhage, Supine, Epidural, Sepsis
        public bool PosturalHypotension { get; set; }
        public int? PosturalDrop { get; set; }

        // Pre-eclampsia Screening
        public bool NewOnsetHypertension { get; set; }
        public bool KnownHypertension { get; set; }
        public bool OnAntihypertensives { get; set; }
        public string AntihypertensiveMedication { get; set; } = string.Empty;
        public DateTime? LastAntihypertensiveDose { get; set; }

        // Associated Symptoms
        public bool Headache { get; set; }
        public bool VisualDisturbances { get; set; }
        public bool EpigastricPain { get; set; }
        public bool Hyperreflexia { get; set; }
        public bool Edema { get; set; }

        // Clinical Response
        public bool EmergencyProtocolActivated { get; set; }
        public bool AntihypertensiveGiven { get; set; }
        public DateTime? AntihypertensiveGivenTime { get; set; }
        public bool MagnesiumSulfateGiven { get; set; }
        public bool IVFluidsGiven { get; set; }
        public bool PositionChanged { get; set; }

        // Display
        public string? BPDisplay => $"{Systolic}/{Diastolic} mmHg, P{Pulse}";
    }
}
