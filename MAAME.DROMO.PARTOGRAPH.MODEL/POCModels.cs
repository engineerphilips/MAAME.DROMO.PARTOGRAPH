using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    /// <summary>
    /// User satisfaction survey for POC tracking
    /// Supports in-app periodic feedback collection
    /// </summary>
    public class UserSurvey
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public SurveyType Type { get; set; } = SurveyType.Satisfaction;
        public SurveyFrequency Frequency { get; set; } = SurveyFrequency.Monthly;

        public bool IsActive { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int TargetResponseCount { get; set; } = 100;

        // Survey Questions (JSON serialized)
        public string QuestionsJson { get; set; } = "[]";

        [NotMapped]
        public List<SurveyQuestion> Questions
        {
            get => string.IsNullOrEmpty(QuestionsJson)
                ? new List<SurveyQuestion>()
                : System.Text.Json.JsonSerializer.Deserialize<List<SurveyQuestion>>(QuestionsJson) ?? new List<SurveyQuestion>();
            set => QuestionsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
        public int Deleted { get; set; }
    }

    public class SurveyQuestion
    {
        public int Order { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public QuestionType Type { get; set; } = QuestionType.Rating;
        public bool IsRequired { get; set; } = true;
        public List<string> Options { get; set; } = new();
        public int? MinRating { get; set; } = 1;
        public int? MaxRating { get; set; } = 5;
        public string Category { get; set; } = string.Empty; // EaseOfUse, WorkflowImpact, Benefits, etc.
    }

    public enum SurveyType
    {
        Satisfaction,
        Usability,
        FeatureRequest,
        Training,
        PostDelivery
    }

    public enum SurveyFrequency
    {
        Once,
        Weekly,
        Monthly,
        Quarterly,
        PostEvent
    }

    public enum QuestionType
    {
        Rating,       // 1-5 scale
        MultiChoice,  // Single selection
        MultiSelect,  // Multiple selection
        Text,         // Free text
        YesNo        // Boolean
    }

    /// <summary>
    /// Individual survey response from a user
    /// </summary>
    public class SurveyResponse
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid SurveyID { get; set; }
        public Guid? StaffID { get; set; }
        public Guid? FacilityID { get; set; }

        public string StaffName { get; set; } = string.Empty;
        public string StaffRole { get; set; } = string.Empty; // Nurse, Midwife, Doctor
        public string FacilityName { get; set; } = string.Empty;
        public string FacilityType { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;

        // Overall satisfaction score (calculated from ratings)
        public decimal OverallSatisfactionScore { get; set; }

        // Individual category scores
        public decimal? EaseOfUseScore { get; set; }
        public decimal? WorkflowImpactScore { get; set; }
        public decimal? PerceivedBenefitsScore { get; set; }
        public decimal? TrainingAdequacyScore { get; set; }
        public decimal? RecommendationScore { get; set; } // Net Promoter Score style

        // Responses (JSON serialized)
        public string AnswersJson { get; set; } = "[]";

        [NotMapped]
        public List<SurveyAnswer> Answers
        {
            get => string.IsNullOrEmpty(AnswersJson)
                ? new List<SurveyAnswer>()
                : System.Text.Json.JsonSerializer.Deserialize<List<SurveyAnswer>>(AnswersJson) ?? new List<SurveyAnswer>();
            set => AnswersJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        // Qualitative feedback
        public string AdditionalComments { get; set; } = string.Empty;
        public string ImprovementSuggestions { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public int CompletionTimeSeconds { get; set; } // Time to complete survey

        // Device info
        public string DeviceId { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;

        // Sync columns
        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
        public int SyncStatus { get; set; }
        public int Deleted { get; set; }

        [ForeignKey("SurveyID")]
        [JsonIgnore]
        public UserSurvey Survey { get; set; }
    }

    public class SurveyAnswer
    {
        public int QuestionOrder { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int? RatingValue { get; set; }
        public string TextValue { get; set; } = string.Empty;
        public List<string> SelectedOptions { get; set; } = new();
        public bool? BoolValue { get; set; }
    }

    /// <summary>
    /// User action log for tracking adoption and engagement
    /// </summary>
    public class UserActionLog
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid? StaffID { get; set; }
        public Guid? FacilityID { get; set; }
        public Guid? PartographID { get; set; }

        public string StaffName { get; set; } = string.Empty;
        public string StaffRole { get; set; } = string.Empty;

        public UserActionType ActionType { get; set; }
        public string ActionDetails { get; set; } = string.Empty;

        public DateTime ActionTime { get; set; } = DateTime.UtcNow;
        public int DurationSeconds { get; set; }

        public string DeviceId { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;

        public long CreatedTime { get; set; }
        public int Deleted { get; set; }
    }

    public enum UserActionType
    {
        // Labor Monitoring Actions
        AppOpened,
        AppClosed,
        PartographCreated,
        PartographViewed,
        LaborMeasurementRecorded,
        FHRRecorded,
        ContractionRecorded,
        VitalSignsRecorded,
        CervicalDilationRecorded,

        // Alert Actions
        AlertViewed,
        AlertAcknowledged,
        AlertActioned,
        RecommendationViewed,

        // Complication Documentation
        ComplicationDocumented,
        MaternalComplicationRecorded,
        NeonatalComplicationRecorded,

        // Referral Actions
        ReferralInitiated,
        ReferralCompleted,
        ReferralViewed,

        // Report Actions
        ReportGenerated,
        ReportSubmitted,
        ReportViewed,

        // Other
        PatientAdmitted,
        DeliveryRecorded,
        SurveyCompleted
    }

    /// <summary>
    /// POC Progress tracking model - aggregated metrics
    /// </summary>
    public class POCProgress
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public DateTime SnapshotDate { get; set; } = DateTime.UtcNow.Date;
        public string PeriodType { get; set; } = "Daily"; // Daily, Weekly, Monthly

        public Guid? FacilityID { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public Guid? DistrictID { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public Guid? RegionID { get; set; }
        public string RegionName { get; set; } = string.Empty;

        // POC 1: Digital Partograph Adoption (Target: 70%)
        public int TotalHealthcareWorkers { get; set; }
        public int ActivePartographUsers { get; set; }
        public decimal AdoptionRate { get; set; } // Percentage
        public decimal AdoptionTarget { get; set; } = 70.0m;
        public bool AdoptionTargetMet => AdoptionRate >= AdoptionTarget;

        // POC 2: User Satisfaction (Target: 4.0/5.0)
        public int TotalSurveyResponses { get; set; }
        public decimal AverageSatisfactionScore { get; set; } // 1-5 scale
        public decimal SatisfactionTarget { get; set; } = 4.0m;
        public bool SatisfactionTargetMet => AverageSatisfactionScore >= SatisfactionTarget;
        public decimal? EaseOfUseAverage { get; set; }
        public decimal? WorkflowImpactAverage { get; set; }
        public decimal? PerceivedBenefitsAverage { get; set; }

        // POC 3: Real-Time Reporting (Target: 70% within 30 min)
        public int TotalEmergencyCases { get; set; }
        public int EmergenciesReportedWithin30Min { get; set; }
        public decimal RealTimeReportingRate { get; set; } // Percentage
        public decimal ReportingTarget { get; set; } = 70.0m;
        public bool ReportingTargetMet => RealTimeReportingRate >= ReportingTarget;
        public decimal AverageReportingTimeMinutes { get; set; }

        // POC 4: Reduce Major Complications (Target: 15% reduction)
        public int TotalDeliveries { get; set; }
        public int TotalComplications { get; set; }
        public decimal ComplicationRate { get; set; } // Per 100 deliveries
        public decimal BaselineComplicationRate { get; set; } // Historical baseline
        public decimal ComplicationReductionPercent { get; set; }
        public decimal ComplicationReductionTarget { get; set; } = 15.0m;
        public bool ComplicationTargetMet => ComplicationReductionPercent >= ComplicationReductionTarget;

        // Individual complication counts
        public int PPHCases { get; set; }
        public int ObstructedLaborCases { get; set; }
        public int BirthAsphyxiaCases { get; set; }
        public int EclampsiaCases { get; set; }
        public int OtherMajorComplications { get; set; }

        // POC 5: Accelerate Emergency Response (Target: 30% reduction)
        public int TotalEmergencyReferrals { get; set; }
        public decimal AverageTimeToReferralMinutes { get; set; }
        public decimal BaselineTimeToReferralMinutes { get; set; }
        public decimal ResponseTimeReductionPercent { get; set; }
        public decimal ResponseTimeReductionTarget { get; set; } = 30.0m;
        public bool ResponseTimeTargetMet => ResponseTimeReductionPercent >= ResponseTimeReductionTarget;

        // Overall POC Progress
        public int TargetsMet { get; set; }
        public int TotalTargets { get; set; } = 5;
        public decimal OverallPOCProgress { get; set; } // Percentage of targets met

        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
    }

    /// <summary>
    /// Baseline settings for POC comparison
    /// </summary>
    public class POCBaseline
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid? FacilityID { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public Guid? DistrictID { get; set; }
        public Guid? RegionID { get; set; }

        public DateTime BaselinePeriodStart { get; set; }
        public DateTime BaselinePeriodEnd { get; set; }
        public string DataSource { get; set; } = string.Empty; // Historical records, facility logs, etc.

        // Baseline metrics (from previous year)
        public decimal BaselineComplicationRate { get; set; } // Per 100 deliveries
        public int BaselinePPHCases { get; set; }
        public int BaselineObstructedLaborCases { get; set; }
        public int BaselineBirthAsphyxiaCases { get; set; }
        public int BaselineTotalDeliveries { get; set; }

        public decimal BaselineAverageTimeToReferralMinutes { get; set; }
        public int BaselineTotalReferrals { get; set; }

        public string Notes { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false;
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovedDate { get; set; }

        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
        public int Deleted { get; set; }
    }
}
