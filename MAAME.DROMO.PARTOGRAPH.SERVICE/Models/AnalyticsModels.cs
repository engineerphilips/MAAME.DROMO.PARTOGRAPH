using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Models
{
    /// <summary>
    /// Daily aggregated statistics per facility for dashboard analytics
    /// </summary>
    public class DailyFacilityStats
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid? FacilityID { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string FacilityCode { get; set; } = string.Empty;

        public DateTime Date { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int WeekOfYear { get; set; }

        // Admission Statistics
        public int TotalAdmissions { get; set; }
        public int LaborAdmissions { get; set; }
        public int AntenatalAdmissions { get; set; }

        // Delivery Statistics
        public int TotalDeliveries { get; set; }
        public int SpontaneousVaginalDeliveries { get; set; }
        public int AssistedVaginalDeliveries { get; set; }
        public int CaesareanSections { get; set; }
        public int EmergencyCaesareans { get; set; }
        public int ElectiveCaesareans { get; set; }

        // Birth Outcomes
        public int LiveBirths { get; set; }
        public int Stillbirths { get; set; }
        public int FreshStillbirths { get; set; }
        public int MaceratedStillbirths { get; set; }
        public int NeonatalDeaths { get; set; }
        public int EarlyNeonatalDeaths { get; set; }

        // Maternal Outcomes
        public int MaternalDeaths { get; set; }
        public int MaternalNearMiss { get; set; }

        // Referrals
        public int TotalReferralsIn { get; set; }
        public int TotalReferralsOut { get; set; }
        public int EmergencyReferrals { get; set; }

        // Complications
        public int PPHCases { get; set; }
        public int EclampsiaPreeclampsiaCases { get; set; }
        public int ObstructedLaborCases { get; set; }
        public int ProlongedLaborCases { get; set; }
        public int FetalDistressCases { get; set; }

        // Resource Utilization
        public int PartographsUsed { get; set; }
        public int PartographsCompleted { get; set; }
        public double AverageLaborDurationHours { get; set; }
        public int TotalStaffOnDuty { get; set; }

        // Data Quality
        public int RecordsWithCompleteData { get; set; }
        public double DataCompletenessPercent { get; set; }

        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
    }

    /// <summary>
    /// Monthly aggregated statistics per facility
    /// </summary>
    public class MonthlyFacilityStats
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid? FacilityID { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string FacilityCode { get; set; } = string.Empty;

        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;

        // Aggregated Delivery Statistics
        public int TotalDeliveries { get; set; }
        public int SVDCount { get; set; }
        public int AssistedDeliveryCount { get; set; }
        public int CaesareanCount { get; set; }
        public double CaesareanRate { get; set; }

        // Birth Outcomes
        public int TotalBirths { get; set; }
        public int LiveBirthCount { get; set; }
        public int StillbirthCount { get; set; }
        public double StillbirthRate { get; set; } // Per 1000 births
        public int NeonatalDeathCount { get; set; }
        public double NeonatalMortalityRate { get; set; } // Per 1000 live births
        public double PerinatalMortalityRate { get; set; } // Per 1000 births

        // Maternal Outcomes
        public int MaternalDeathCount { get; set; }
        public double MaternalMortalityRatio { get; set; } // Per 100,000 live births
        public int MaternalNearMissCount { get; set; }

        // Baby Statistics
        public int TotalBabiesBorn { get; set; }
        public int LowBirthWeightCount { get; set; }
        public int VeryLowBirthWeightCount { get; set; }
        public int PretermBirthCount { get; set; }
        public double AverageBirthWeight { get; set; }
        public double LowBirthWeightRate { get; set; }

        // Complications
        public int PPHCount { get; set; }
        public int SeverePreeclampsiaCount { get; set; }
        public int EclampsiaCount { get; set; }
        public int SepsisCount { get; set; }
        public int ObstructedLaborCount { get; set; }

        // Referrals
        public int ReferralsInCount { get; set; }
        public int ReferralsOutCount { get; set; }
        public double ReferralRate { get; set; }

        // Quality Indicators
        public double PartographUtilizationRate { get; set; }
        public double ActiveManagement3rdStageRate { get; set; }
        public double EarlyBreastfeedingRate { get; set; }
        public double SkinToSkinRate { get; set; }
        public int APGARScoreLessThan7At5MinCount { get; set; }
        public double ResuscitationRate { get; set; }

        // WHO Targets Comparison
        public double CaesareanRateTarget { get; set; } = 15.0; // WHO target
        public bool CaesareanRateWithinTarget { get; set; }

        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
    }

    /// <summary>
    /// Individual delivery outcome summary for analytical queries
    /// </summary>
    public class DeliveryOutcomeSummary
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid? PartographID { get; set; }
        public Guid? PatientID { get; set; }
        public Guid? FacilityID { get; set; }
        public string FacilityName { get; set; } = string.Empty;

        // Patient Demographics (anonymized)
        public int? PatientAge { get; set; }
        public int? Gravida { get; set; }
        public int? Parity { get; set; }
        public int? GestationalAgeWeeks { get; set; }
        public string GestationalCategory { get; set; } = string.Empty; // Preterm, Term, PostTerm

        // Labor Information
        public DateTime? AdmissionTime { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public double? LaborDurationHours { get; set; }
        public string LaborStatus { get; set; } = string.Empty;
        public string FirstStagePhase { get; set; } = string.Empty;
        public bool ProlongedLabor { get; set; }
        public bool AugmentationUsed { get; set; }

        // Delivery Information
        public string DeliveryMode { get; set; } = string.Empty;
        public string DeliveryModeCategory { get; set; } = string.Empty;
        public bool InstrumentUsed { get; set; }
        public string InstrumentType { get; set; } = string.Empty;
        public string CaesareanIndication { get; set; } = string.Empty;

        // Maternal Outcome
        public string MaternalOutcome { get; set; } = string.Empty;
        public bool MaternalDeath { get; set; }
        public string MaternalDeathCause { get; set; } = string.Empty;
        public bool PPHOccurred { get; set; }
        public int? EstimatedBloodLossMl { get; set; }
        public bool BloodTransfusionGiven { get; set; }
        public string MaternalComplications { get; set; } = string.Empty;

        // Neonatal Outcome
        public int NumberOfBabies { get; set; } = 1;
        public string NeonatalOutcome { get; set; } = string.Empty;
        public bool LiveBirth { get; set; }
        public bool Stillbirth { get; set; }
        public bool NeonatalDeath { get; set; }
        public decimal? BirthWeight { get; set; }
        public string BirthWeightCategory { get; set; } = string.Empty;
        public int? APGAR1Min { get; set; }
        public int? APGAR5Min { get; set; }
        public bool ResuscitationRequired { get; set; }
        public bool AdmittedToNICU { get; set; }

        // Care Quality Indicators
        public bool PartographUsed { get; set; }
        public bool PartographComplete { get; set; }
        public bool ActiveManagement3rdStage { get; set; }
        public bool EarlyBreastfeeding { get; set; }
        public bool SkinToSkinContact { get; set; }
        public bool CompanionPresent { get; set; }

        // Risk Factors
        public string RiskFactors { get; set; } = string.Empty;
        public bool HighRisk { get; set; }

        // Referral Information
        public bool WasReferred { get; set; }
        public string ReferralReason { get; set; } = string.Empty;
        public string ReferralUrgency { get; set; } = string.Empty;

        // Handler Information
        public string AttendantType { get; set; } = string.Empty;
        public Guid? PrimaryAttendantID { get; set; }

        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
    }

    /// <summary>
    /// Maternal mortality record for MDR (Maternal Death Review)
    /// </summary>
    public class MaternalMortalityRecord
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid? PartographID { get; set; }
        public Guid? PatientID { get; set; }
        public Guid? BirthOutcomeID { get; set; }
        public Guid? FacilityID { get; set; }
        public string FacilityName { get; set; } = string.Empty;

        public DateTime DeathDateTime { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        // Patient Demographics
        public int? Age { get; set; }
        public int? Parity { get; set; }
        public int? GestationalAge { get; set; }

        // Death Classification
        public string PrimaryCause { get; set; } = string.Empty;
        public string SecondaryCause { get; set; } = string.Empty;
        public string ContributingFactors { get; set; } = string.Empty;
        public string ICDCode { get; set; } = string.Empty;

        // Time and Place of Death
        public string PlaceOfDeath { get; set; } = string.Empty;
        public string TimingOfDeath { get; set; } = string.Empty; // Antepartum, Intrapartum, Postpartum
        public int? HoursPostpartum { get; set; }

        // Direct/Indirect Cause Classification
        public bool DirectObstetricCause { get; set; }
        public string DirectCauseCategory { get; set; } = string.Empty; // Hemorrhage, Hypertensive, Sepsis, Obstructed, etc.
        public string IndirectCauseCategory { get; set; } = string.Empty; // Cardiac, Anemia, Malaria, HIV, etc.

        // Delays (Three Delays Model)
        public bool Delay1SeekingCare { get; set; }
        public string Delay1Details { get; set; } = string.Empty;
        public bool Delay2ReachingCare { get; set; }
        public string Delay2Details { get; set; } = string.Empty;
        public bool Delay3ReceivingCare { get; set; }
        public string Delay3Details { get; set; } = string.Empty;

        // Preventability Assessment
        public string PreventabilityAssessment { get; set; } = string.Empty;
        public string PreventableFactors { get; set; } = string.Empty;
        public string RecommendedActions { get; set; } = string.Empty;

        // Review Status
        public bool MDRCompleted { get; set; }
        public DateTime? MDRDate { get; set; }
        public string MDRFindings { get; set; } = string.Empty;

        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
    }

    /// <summary>
    /// Neonatal outcome record for perinatal death reviews
    /// </summary>
    public class NeonatalOutcomeRecord
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid? PartographID { get; set; }
        public Guid? BabyDetailsID { get; set; }
        public Guid? FacilityID { get; set; }
        public string FacilityName { get; set; } = string.Empty;

        public DateTime BirthDateTime { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        // Outcome Type
        public string OutcomeType { get; set; } = string.Empty; // LiveBirth, FreshStillbirth, MaceratedStillbirth, NeonatalDeath
        public DateTime? DeathDateTime { get; set; }
        public int? HoursAfterBirth { get; set; }

        // Baby Details
        public string Sex { get; set; } = string.Empty;
        public decimal? BirthWeight { get; set; }
        public string BirthWeightCategory { get; set; } = string.Empty;
        public int? GestationalAge { get; set; }
        public string GestationalCategory { get; set; } = string.Empty;
        public int? APGAR1 { get; set; }
        public int? APGAR5 { get; set; }
        public bool LowAPGAR { get; set; }

        // Cause Classification (for deaths)
        public string PrimaryCause { get; set; } = string.Empty;
        public string ICDCode { get; set; } = string.Empty;

        // Categories for stillbirth/neonatal death
        public string CauseCategory { get; set; } = string.Empty; // Asphyxia, Prematurity, Infection, Congenital, Unknown

        // Birth Circumstances
        public string DeliveryMode { get; set; } = string.Empty;
        public bool ProlongedLabor { get; set; }
        public bool MeconiumStained { get; set; }
        public bool CordProlapse { get; set; }
        public bool ShoulderDystocia { get; set; }

        // Resuscitation Details
        public bool ResuscitationAttempted { get; set; }
        public string ResuscitationSteps { get; set; } = string.Empty;
        public int? ResuscitationDurationMinutes { get; set; }

        // Care Quality Indicators
        public bool DelayedCordClamping { get; set; }
        public bool ImmediateSkinToSkin { get; set; }
        public bool EarlyBreastfeeding { get; set; }

        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
    }

    /// <summary>
    /// Complication analytics for tracking and trending
    /// </summary>
    public class ComplicationAnalytics
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid? PartographID { get; set; }
        public Guid? FacilityID { get; set; }
        public string FacilityName { get; set; } = string.Empty;

        public DateTime OccurrenceDateTime { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }

        // Complication Type
        public string ComplicationType { get; set; } = string.Empty;
        public string ComplicationCategory { get; set; } = string.Empty; // Maternal, Fetal, Labor, Delivery
        public string Severity { get; set; } = string.Empty; // Mild, Moderate, Severe, Life-threatening

        // Specific Complications
        public bool PPH { get; set; }
        public int? PPHBloodLossMl { get; set; }
        public bool PreeclampsiaSevere { get; set; }
        public bool Eclampsia { get; set; }
        public bool Sepsis { get; set; }
        public bool ObstructedLabor { get; set; }
        public bool UterineRupture { get; set; }
        public bool FetalDistress { get; set; }
        public bool CordProlapse { get; set; }
        public bool ShoulderDystocia { get; set; }
        public bool PlacentalAbruption { get; set; }

        // Detection and Response
        public string DetectionMethod { get; set; } = string.Empty; // Partograph, Clinical, Monitor
        public DateTime? DetectionTime { get; set; }
        public DateTime? ResponseTime { get; set; }
        public int? TimeToResponseMinutes { get; set; }

        // Management
        public string ManagementActions { get; set; } = string.Empty;
        public bool SurgicalInterventionRequired { get; set; }
        public bool BloodTransfusionRequired { get; set; }
        public bool ICUAdmissionRequired { get; set; }

        // Outcome
        public string MaternalOutcome { get; set; } = string.Empty;
        public string FetalOutcome { get; set; } = string.Empty;
        public bool Preventable { get; set; }
        public string PreventionNotes { get; set; } = string.Empty;

        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
    }

    /// <summary>
    /// Referral analytics for tracking referral patterns
    /// </summary>
    public class ReferralAnalytics
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid? ReferralID { get; set; }
        public Guid? PartographID { get; set; }

        public DateTime ReferralDateTime { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        // Source Facility
        public Guid? SourceFacilityID { get; set; }
        public string SourceFacilityName { get; set; } = string.Empty;
        public string SourceFacilityLevel { get; set; } = string.Empty;
        public string SourceRegion { get; set; } = string.Empty;

        // Destination Facility
        public Guid? DestinationFacilityID { get; set; }
        public string DestinationFacilityName { get; set; } = string.Empty;
        public string DestinationFacilityLevel { get; set; } = string.Empty;
        public string DestinationRegion { get; set; } = string.Empty;

        // Referral Details
        public string ReferralType { get; set; } = string.Empty; // Maternal, Neonatal, Both
        public string Urgency { get; set; } = string.Empty;
        public string PrimaryReason { get; set; } = string.Empty;
        public string SecondaryReasons { get; set; } = string.Empty;

        // Transport
        public string TransportMode { get; set; } = string.Empty;
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public int? TransportDurationMinutes { get; set; }
        public bool SkilledAttendantAccompanied { get; set; }

        // Pre-referral Care
        public string PreReferralInterventions { get; set; } = string.Empty;
        public bool IVLineEstablished { get; set; }
        public bool PartographSent { get; set; }

        // Outcome
        public string ReferralStatus { get; set; } = string.Empty; // Completed, Failed, Declined
        public string OutcomeAtDestination { get; set; } = string.Empty;
        public string MaternalOutcome { get; set; } = string.Empty;
        public string NeonatalOutcome { get; set; } = string.Empty;

        // Quality Metrics
        public int? DecisionToReferralMinutes { get; set; }
        public bool DestinationNotifiedBeforeArrival { get; set; }
        public bool FeedbackReceived { get; set; }

        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
    }

    /// <summary>
    /// Labor progress analytics from partograph data
    /// </summary>
    public class LaborProgressAnalytics
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid? PartographID { get; set; }
        public Guid? FacilityID { get; set; }
        public string FacilityName { get; set; } = string.Empty;

        public DateTime LaborStartTime { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        // Labor Duration Analysis
        public double? TotalLaborDurationHours { get; set; }
        public double? FirstStageDurationHours { get; set; }
        public double? LatentPhaseDurationHours { get; set; }
        public double? ActivePhaseDurationHours { get; set; }
        public double? SecondStageDurationMinutes { get; set; }
        public double? ThirdStageDurationMinutes { get; set; }

        // Progress Pattern
        public string LaborProgressPattern { get; set; } = string.Empty; // Normal, Prolonged, Arrested, Precipitate
        public bool CrossedActionLine { get; set; }
        public bool CrossedAlertLine { get; set; }
        public DateTime? AlertLineCrossTime { get; set; }
        public DateTime? ActionLineCrossTime { get; set; }

        // Cervical Progression
        public int InitialDilationCm { get; set; }
        public DateTime? ActivePhaseOnsetTime { get; set; }
        public double? CervicalDilationRateCmPerHour { get; set; }

        // Descent Pattern
        public int InitialStation { get; set; }
        public string DescentPattern { get; set; } = string.Empty;

        // FHR Pattern Analysis
        public int FHRMeasurementCount { get; set; }
        public int FHRAbnormalCount { get; set; }
        public double? AverageFHR { get; set; }
        public bool FHRDecelerations { get; set; }
        public bool FHRTachycardia { get; set; }
        public bool FHRBradycardia { get; set; }

        // Contraction Analysis
        public int ContractionMeasurementCount { get; set; }
        public double? AverageContractionFrequency { get; set; }
        public double? AverageContractionDuration { get; set; }
        public bool Tachysystole { get; set; }
        public bool HyperstimulationOccurred { get; set; }

        // Interventions
        public bool AugmentationUsed { get; set; }
        public DateTime? AugmentationStartTime { get; set; }
        public bool AmniotomyPerformed { get; set; }
        public DateTime? AmniotomyTime { get; set; }

        // Outcome
        public string DeliveryMode { get; set; } = string.Empty;
        public string ReasonForIntervention { get; set; } = string.Empty;

        // WHO Compliance
        public bool WHOCompliantMonitoring { get; set; }
        public double? MonitoringCompliancePercent { get; set; }

        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
    }

    /// <summary>
    /// Facility performance snapshot for executive dashboards
    /// </summary>
    public class FacilityPerformanceSnapshot
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid? FacilityID { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string FacilityCode { get; set; } = string.Empty;
        public string FacilityType { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;

        public DateTime SnapshotDate { get; set; }
        public string PeriodType { get; set; } = string.Empty; // Daily, Weekly, Monthly, Quarterly, Yearly

        // Performance Scores (0-100)
        public double OverallPerformanceScore { get; set; }
        public double QualityOfCareScore { get; set; }
        public double SafetyScore { get; set; }
        public double EfficiencyScore { get; set; }
        public double PatientExperienceScore { get; set; }

        // Key Metrics
        public double CaesareanRate { get; set; }
        public double StillbirthRate { get; set; }
        public double NeonatalMortalityRate { get; set; }
        public double MaternalMortalityRatio { get; set; }
        public double PartographUtilizationRate { get; set; }
        public double ReferralRate { get; set; }

        // WHO Target Compliance
        public double WHOComplianceScore { get; set; }
        public int TargetsMet { get; set; }
        public int TotalTargets { get; set; }

        // Trend Indicators (-1 = declining, 0 = stable, 1 = improving)
        public int OverallTrend { get; set; }
        public int QualityTrend { get; set; }
        public int SafetyTrend { get; set; }

        // Alerts and Recommendations
        public int ActiveAlertsCount { get; set; }
        public string CriticalAlerts { get; set; } = string.Empty;
        public string RecommendedActions { get; set; } = string.Empty;

        // Comparison
        public double RegionalAverage { get; set; }
        public double NationalAverage { get; set; }
        public int RankInRegion { get; set; }
        public int TotalFacilitiesInRegion { get; set; }

        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
    }

    /// <summary>
    /// Alert summary for monitoring and notifications
    /// </summary>
    public class AlertSummary
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid? FacilityID { get; set; }
        public string FacilityName { get; set; } = string.Empty;

        public Guid? PartographID { get; set; }
        public Guid? PatientID { get; set; }

        public DateTime AlertDateTime { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }

        // Alert Details
        public string AlertType { get; set; } = string.Empty; // Clinical, System, Compliance, Performance
        public string AlertCategory { get; set; } = string.Empty; // FHRAbnormal, ProgressDelay, VitalAbnormal, etc.
        public string AlertSeverity { get; set; } = string.Empty; // Low, Medium, High, Critical
        public string AlertMessage { get; set; } = string.Empty;
        public string AlertSource { get; set; } = string.Empty; // Partograph, System, Review

        // Clinical Context
        public string ClinicalParameter { get; set; } = string.Empty;
        public string MeasuredValue { get; set; } = string.Empty;
        public string NormalRange { get; set; } = string.Empty;
        public string ThresholdViolated { get; set; } = string.Empty;

        // Response
        public bool Acknowledged { get; set; }
        public DateTime? AcknowledgeTime { get; set; }
        public string AcknowledgedBy { get; set; } = string.Empty;
        public bool ActionTaken { get; set; }
        public string ActionDescription { get; set; } = string.Empty;
        public DateTime? ActionTime { get; set; }
        public int? ResponseTimeMinutes { get; set; }

        // Outcome
        public bool Resolved { get; set; }
        public DateTime? ResolvedTime { get; set; }
        public string ResolutionNotes { get; set; } = string.Empty;
        public string OutcomeAssessment { get; set; } = string.Empty; // Positive, Neutral, Adverse

        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
    }
}
