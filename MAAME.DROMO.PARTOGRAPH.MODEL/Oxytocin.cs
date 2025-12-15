namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Oxytocin Administration - WHO Labour Care Guide Section 6: Medication
    // Record oxytocin dose and response throughout labour
    public class Oxytocin : BasePartographMeasurement
    {
        // Infusion Status
        public bool InUse { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? StopTime { get; set; }

        // Dosage (WHO recommends recording dose in mU/min or U/L or drops/min)
        public decimal DoseMUnitsPerMin { get; set; } // Range: typically 0-40 mU/min, max 40 per protocols
        public decimal TotalVolumeInfused { get; set; }
        public decimal ConcentrationMUnitsPerMl { get; set; } = 10; // Usually 10 mU/ml (10 units in 1000ml)
        public decimal InfusionRateMlPerHour { get; set; }

        // Indication
        public string Indication { get; set; } = string.Empty; // Induction, Augmentation, ActiveManagement3rdStage

        // Contraindications Check (WHO: must check before administration)
        public bool ContraindicationsChecked { get; set; }
        public bool ContraindicationsPresent { get; set; }
        public string ContraindicationDetails { get; set; } = string.Empty;

        // Response to Oxytocin (WHO: assess and record response)
        public string Response { get; set; } = string.Empty; // Good, Inadequate, Hyperstimulation, FetalDistress

        // Dose Titration
        public string DoseTitration { get; set; } = string.Empty; // Increased, Decreased, Maintained, Stopped
        public int TimeToNextIncrease { get; set; } // Minutes until next dose increase
        public bool MaxDoseReached { get; set; }

        // Stopping Reason
        public string StoppedReason { get; set; } = string.Empty; // Delivery, Hyperstimulation, FetalDistress, Completed

        // Safety Alerts (auto-calculated based on WHO guidelines)
        public string ClinicalAlert { get; set; } = string.Empty;
    }
}
