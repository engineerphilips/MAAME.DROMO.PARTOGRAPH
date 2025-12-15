namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Birth Companion - WHO Labour Care Guide Section 2: Supportive Care
    // WHO 2020 CORE RECOMMENDATION: All women should have companion of choice during labor and birth
    // Continuous support improves labor outcomes and satisfaction
    public class CompanionEntry : BasePartographMeasurement
    {
        // Basic Companion Status (legacy field)
        public string? Companion { get; set; }
        public string? CompanionDisplay => Companion != null ? (Companion == "Y" ? "Yes" : Companion == "N" ? "No" : Companion == "D" ? "Declined" : string.Empty) : string.Empty;

        // Companion Present
        public bool CompanionPresent { get; set; }

        // Companion Type/Relationship
        public string CompanionType { get; set; } = string.Empty; // Partner, Mother, Sister, Friend, Doula, CulturalBirthAttendant, Other

        // Number of Companions (WHO: at least one, some facilities allow more)
        public int NumberOfCompanions { get; set; }

        // Companion Details
        public string CompanionName { get; set; } = string.Empty;
        public string CompanionRelationship { get; set; } = string.Empty;

        // Presence Duration
        public DateTime? ArrivalTime { get; set; }
        public DateTime? DepartureTime { get; set; }
        public int? DurationMinutes { get; set; }
        public bool ContinuousPresence { get; set; }

        // Participation Level (WHO: active support improves outcomes)
        public string ParticipationLevel { get; set; } = string.Empty; // ActiveSupport, Passive, Observing

        // Support Activities Provided
        public string SupportActivities { get; set; } = string.Empty; // EmotionalSupport, PhysicalSupport, Massage, Breathing, Advocacy, InformationSharing

        // Patient Preference
        public bool PatientRequestedCompanion { get; set; }
        public bool PatientDeclinedCompanion { get; set; }
        public string ReasonForNoCompanion { get; set; } = string.Empty; // PatientChoice, NoCompanionAvailable, FacilityPolicy, COVID19

        // Staff Engagement (WHO: encourage staff to involve companion)
        public bool StaffOrientedCompanion { get; set; }
        public bool CompanionInvolvedInDecisions { get; set; }

        // Special Considerations
        public bool LanguageBarrier { get; set; }
        public bool InterpreterRequired { get; set; }
        public bool CulturalPractices { get; set; }
        public string CulturalPracticesDetails { get; set; } = string.Empty;

        // WHO 2020 Recommendations
        public string ClinicalAlert { get; set; } = string.Empty;
        // No companion present for prolonged period (WHO: should have continuous support)
        // Companion removed against patient wishes (WHO: companion is a right)
    }
}
