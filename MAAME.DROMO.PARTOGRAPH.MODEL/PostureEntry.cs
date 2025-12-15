namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Maternal Posture - WHO Labour Care Guide Section 2: Supportive Care
    // WHO 2020: Encourage mobility and position changes during labor
    // Upright and lateral positions improve labor outcomes
    public class PostureEntry : BasePartographMeasurement
    {
        // Posture/Position
        public string? Posture { get; set; } // Upright, Walking, Sitting, LeftLateral, RightLateral, Supine, HandsAndKnees, Squatting, BirthingBall, WaterImmersion
        public string? PostureDisplay => Posture != null ? Posture.ToString() : string.Empty;

        // Posture Category (WHO: upright positions recommended)
        public string PostureCategory { get; set; } = string.Empty; // Upright, Lateral, Recumbent, Active

        // Duration in this Posture
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? DurationMinutes { get; set; }

        // Reason for Posture
        public string Reason { get; set; } = string.Empty; // Comfort, LaborProgress, MedicalIndication, PatientPreference, FetalPosition, EpiduralInSitu

        // Effect on Labor (WHO: position changes can improve labor progress)
        public string EffectOnLabor { get; set; } = string.Empty; // Improved, NoChange, Worse
        public string EffectOnPain { get; set; } = string.Empty; // Reduced, NoChange, Increased
        public string EffectOnContractions { get; set; } = string.Empty; // Improved, NoChange, Reduced

        // Patient Choice (WHO: respect woman's choice of position)
        public bool PatientChoice { get; set; } = true;
        public bool MedicallyIndicated { get; set; }

        // Mobility Status
        public bool MobileAndActive { get; set; }
        public bool RestrictedMobility { get; set; }
        public string MobilityRestriction { get; set; } = string.Empty; // Epidural, Monitoring, IVLine, Exhaustion

        // Support Equipment Used
        public string SupportEquipment { get; set; } = string.Empty; // BirthingBall, BirthingStool, Mat, Pool, Pillows, ReboSling

        // WHO 2020 Recommendations
        public string ClinicalAlert { get; set; } = string.Empty;
        // Prolonged supine position (avoid - causes aortocaval compression)
        // Encourage position changes every 30-60 minutes if tolerated
    }
}
