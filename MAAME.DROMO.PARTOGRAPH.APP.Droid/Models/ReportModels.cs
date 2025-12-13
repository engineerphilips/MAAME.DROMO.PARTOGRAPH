using System;
using System.Collections.Generic;
using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Models
{
    // Base Report Model
    public abstract class BaseReport
    {
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public string ReportTitle { get; set; } = string.Empty;
    }

    // Monthly Delivery Dashboard Model
    public class MonthlyDeliveryDashboard : BaseReport
    {
        public int TotalDeliveries { get; set; }
        public int LiveBirths { get; set; }
        public int Stillbirths { get; set; }
        public int MaternalDeaths { get; set; }
        public int NeonatalDeaths { get; set; }

        // Delivery Modes
        public int SpontaneousVaginalDeliveries { get; set; }
        public int AssistedVaginalDeliveries { get; set; }
        public int CaesareanSections { get; set; }
        public int BreechDeliveries { get; set; }
        public decimal CaesareanSectionRate => TotalDeliveries > 0 ? (decimal)CaesareanSections / TotalDeliveries * 100 : 0;
        public decimal VaginalDeliveryRate => TotalDeliveries > 0 ? (decimal)(SpontaneousVaginalDeliveries + AssistedVaginalDeliveries) / TotalDeliveries * 100 : 0;

        // Complications
        public int PostpartumHemorrhages { get; set; }
        public int Eclampsia { get; set; }
        public int ObstructedLabor { get; set; }
        public int RupturedUterus { get; set; }
        public int SepticCases { get; set; }
        public int PreEclampsia { get; set; }
        public decimal ComplicationRate => TotalDeliveries > 0 ? (decimal)(PostpartumHemorrhages + Eclampsia + ObstructedLabor + RupturedUterus + SepticCases) / TotalDeliveries * 100 : 0;

        // Neonatal Outcomes
        public int LowBirthWeightBabies { get; set; }
        public int NICUAdmissions { get; set; }
        public decimal AverageApgar1Min { get; set; }
        public decimal AverageApgar5Min { get; set; }
        public int FreshStillbirths { get; set; }
        public int MaceratedStillbirths { get; set; }
        public decimal PerinatalMortalityRate => LiveBirths > 0 ? (decimal)(Stillbirths + NeonatalDeaths) / (LiveBirths + Stillbirths) * 1000 : 0;

        // Labor Statistics
        public decimal AverageLaborDuration { get; set; } // in hours
        public decimal MedianLaborDuration { get; set; }
        public int ProlongedLabors { get; set; }
        public int RapidLabors { get; set; }
        public decimal ProlongedLaborRate => TotalDeliveries > 0 ? (decimal)ProlongedLabors / TotalDeliveries * 100 : 0;

        // WHO Compliance
        public decimal AlertLineCrossingsPercentage { get; set; }
        public decimal ActionLineCrossingsPercentage { get; set; }
        public decimal OverallWHOCompliance { get; set; }

        // Summary Statistics
        public decimal MaternalMortalityRatio => LiveBirths > 0 ? (decimal)MaternalDeaths / LiveBirths * 100000 : 0;
        public decimal NeonatalMortalityRate => LiveBirths > 0 ? (decimal)NeonatalDeaths / LiveBirths * 1000 : 0;
        public decimal StillbirthRate => TotalDeliveries > 0 ? (decimal)Stillbirths / TotalDeliveries * 1000 : 0;
    }

    // Maternal Complications Report Model
    public class MaternalComplicationsReport : BaseReport
    {
        public int TotalCases { get; set; }
        public int TotalDeliveriesInPeriod { get; set; }
        public List<MaternalComplicationCase> Cases { get; set; } = new();

        // Summary Statistics
        public int HypertensiveDisorders { get; set; }
        public int PostpartumHemorrhages { get; set; }
        public int SepticShock { get; set; }
        public int ObstructedLabor { get; set; }
        public int RupturedUterus { get; set; }
        public int Eclampsia { get; set; }
        public int PreEclampsia { get; set; }
        public int MaternalDeaths { get; set; }
        public int AmnioticFluidEmbolism { get; set; }
        public int PlacentalAbruption { get; set; }
        public int PlacentaPrevia { get; set; }
        public int UmbilicalCordProlapse { get; set; }
        public int FetalDistress { get; set; }

        // Complication Rates
        public decimal ComplicationRate => TotalDeliveriesInPeriod > 0 ? (decimal)TotalCases / TotalDeliveriesInPeriod * 100 : 0;
        public decimal MaternalMortalityRate => TotalDeliveriesInPeriod > 0 ? (decimal)MaternalDeaths / TotalDeliveriesInPeriod * 100000 : 0;
        public decimal SevereComplicationRate => TotalCases > 0 ? (decimal)(RupturedUterus + SepticShock + AmnioticFluidEmbolism) / TotalCases * 100 : 0;

        // Blood Loss Statistics
        public decimal AverageBloodLoss { get; set; }
        public decimal MedianBloodLoss { get; set; }
        public int CasesExceeding500ml { get; set; }
        public int CasesExceeding1000ml { get; set; }
        public int CasesExceeding1500ml { get; set; }
        public int BloodTransfusionsRequired { get; set; }

        // Perineal Trauma
        public int IntactPerineum { get; set; }
        public int FirstDegreeTears { get; set; }
        public int SecondDegreeTears { get; set; }
        public int ThirdDegreeTears { get; set; }
        public int FourthDegreeTears { get; set; }
        public int Episiotomies { get; set; }
        public decimal SeverePerinealTraumaRate => TotalDeliveriesInPeriod > 0 ? (decimal)(ThirdDegreeTears + FourthDegreeTears) / TotalDeliveriesInPeriod * 100 : 0;

        // Intervention Statistics
        public int EmergencyCaesareans { get; set; }
        public int ManualPlacentaRemoval { get; set; }
        public int HysterectomiesPerformed { get; set; }
        public int ICUAdmissions { get; set; }
    }

    public class MaternalComplicationCase
    {
        public Guid PatientID { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string HospitalNumber { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public List<string> Complications { get; set; } = new();
        public MaternalOutcomeStatus Outcome { get; set; }
        public int EstimatedBloodLoss { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public int Gravida { get; set; }
        public int Parity { get; set; }
        public string RiskFactors { get; set; } = string.Empty;
        public string InterventionsPerformed { get; set; } = string.Empty;
        public string AttendingStaff { get; set; } = string.Empty;
        public TimeSpan? TimeToIntervention { get; set; }
        public bool ICUAdmission { get; set; }
        public bool BloodTransfusion { get; set; }
        public string ClinicalNotes { get; set; } = string.Empty;
    }

    // Neonatal Outcomes Report Model
    public class NeonatalOutcomesReport : BaseReport
    {
        public int TotalBirths { get; set; }
        public List<NeonatalOutcomeCase> Cases { get; set; } = new();

        // Vital Statistics
        public int LiveBirths { get; set; }
        public int FreshStillbirths { get; set; }
        public int MaceratedStillbirths { get; set; }
        public int EarlyNeonatalDeaths { get; set; }
        public int LateNeonatalDeaths { get; set; }
        public decimal StillbirthRate => TotalBirths > 0 ? (decimal)(FreshStillbirths + MaceratedStillbirths) / TotalBirths * 1000 : 0;
        public decimal EarlyNeonatalMortalityRate => LiveBirths > 0 ? (decimal)EarlyNeonatalDeaths / LiveBirths * 1000 : 0;
        public decimal PerinatalMortalityRate => TotalBirths > 0 ? (decimal)(FreshStillbirths + MaceratedStillbirths + EarlyNeonatalDeaths) / TotalBirths * 1000 : 0;

        // Gender Distribution
        public int MaleBirths { get; set; }
        public int FemaleBirths { get; set; }
        public int AmbiguousSex { get; set; }
        public decimal MaleToFemaleRatio => FemaleBirths > 0 ? (decimal)MaleBirths / FemaleBirths : 0;

        // Birth Weight Distribution
        public int ExtremelyLowBirthWeight { get; set; } // <1000g
        public int VeryLowBirthWeight { get; set; } // 1000-1499g
        public int LowBirthWeight { get; set; } // 1500-2499g
        public int NormalWeight { get; set; } // 2500-3999g
        public int Macrosomia { get; set; } // >=4000g
        public decimal AverageBirthWeight { get; set; }
        public decimal MedianBirthWeight { get; set; }
        public decimal StandardDeviationBirthWeight { get; set; }
        public decimal LowBirthWeightRate => TotalBirths > 0 ? (decimal)(ExtremelyLowBirthWeight + VeryLowBirthWeight + LowBirthWeight) / TotalBirths * 100 : 0;

        // Gestational Age Distribution
        public int ExtremelyPreterm { get; set; } // <28 weeks
        public int VeryPreterm { get; set; } // 28-31 weeks
        public int ModeratePreterm { get; set; } // 32-33 weeks
        public int LatePreterm { get; set; } // 34-36 weeks
        public int Term { get; set; } // 37-41 weeks
        public int PostTerm { get; set; } // >42 weeks
        public decimal PretermBirthRate => TotalBirths > 0 ? (decimal)(ExtremelyPreterm + VeryPreterm + ModeratePreterm + LatePreterm) / TotalBirths * 100 : 0;

        // APGAR Scores
        public decimal AverageApgar1Min { get; set; }
        public decimal AverageApgar5Min { get; set; }
        public decimal AverageApgar10Min { get; set; }
        public int Apgar1MinBelow4 { get; set; }
        public int Apgar1MinBelow7 { get; set; }
        public int Apgar5MinBelow4 { get; set; }
        public int Apgar5MinBelow7 { get; set; }
        public decimal ApgarImprovementRate => Apgar1MinBelow7 > 0 ? (decimal)(Apgar1MinBelow7 - Apgar5MinBelow7) / Apgar1MinBelow7 * 100 : 0;

        // Resuscitation
        public int ResuscitationRequired { get; set; }
        public int BagMaskVentilation { get; set; }
        public int Intubations { get; set; }
        public int ChestCompressions { get; set; }
        public int Adrenaline { get; set; }
        public int SurfactantGiven { get; set; }
        public decimal ResuscitationRate => LiveBirths > 0 ? (decimal)ResuscitationRequired / LiveBirths * 100 : 0;

        // NICU Admissions
        public int NICUAdmissions { get; set; }
        public decimal NICUAdmissionRate => TotalBirths > 0 ? (decimal)NICUAdmissions / TotalBirths * 100 : 0;
        public decimal AverageNICUStayDays { get; set; }

        // Essential Newborn Care (WHO)
        public int SkinToSkinContact { get; set; }
        public int EarlyBreastfeeding { get; set; }
        public int DelayedCordClamping { get; set; }
        public int VitaminKGiven { get; set; }
        public int EyeProphylaxis { get; set; }
        public decimal EssentialCareComplianceRate { get; set; }

        // Complications
        public int BirthAsphyxia { get; set; }
        public int RespiratoryDistress { get; set; }
        public int Sepsis { get; set; }
        public int Jaundice { get; set; }
        public int Hypothermia { get; set; }
        public int Hypoglycemia { get; set; }
        public int CongenitalAbnormalities { get; set; }
        public int BirthInjuries { get; set; }
        public int MeconiumAspiration { get; set; }
        public int IntracranialHemorrhage { get; set; }
        public int NecrotizingEnterocolitis { get; set; }
    }

    public class NeonatalOutcomeCase
    {
        public Guid BabyID { get; set; }
        public string MotherName { get; set; } = string.Empty;
        public string HospitalNumber { get; set; } = string.Empty;
        public DateTime BirthTime { get; set; }
        public BabySex Sex { get; set; }
        public decimal BirthWeight { get; set; }
        public int? GestationalAgeWeeks { get; set; }
        public int? Apgar1Min { get; set; }
        public int? Apgar5Min { get; set; }
        public int? Apgar10Min { get; set; }
        public BabyVitalStatus VitalStatus { get; set; }
        public bool ResuscitationRequired { get; set; }
        public List<string> ResuscitationMethods { get; set; } = new();
        public bool NICUAdmission { get; set; }
        public string NICUReason { get; set; } = string.Empty;
        public List<string> Complications { get; set; } = new();
        public DateTime? DischargeDate { get; set; }
        public string DischargeStatus { get; set; } = string.Empty;
        public bool SkinToSkinDone { get; set; }
        public bool EarlyBreastfeedingDone { get; set; }
        public string ClinicalNotes { get; set; } = string.Empty;
    }

    // Alert Response Time Report Model
    public class AlertResponseTimeReport : BaseReport
    {
        public int TotalAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public int WarningAlerts { get; set; }
        public int InfoAlerts { get; set; }

        // Response Time Metrics
        public decimal AverageResponseTime { get; set; } // in minutes
        public decimal MedianResponseTime { get; set; }
        public decimal MinResponseTime { get; set; }
        public decimal MaxResponseTime { get; set; }
        public decimal StandardDeviationResponseTime { get; set; }
        public int AlertsUnder5Minutes { get; set; }
        public int AlertsUnder15Minutes { get; set; }
        public int AlertsUnder30Minutes { get; set; }
        public int AlertsOver30Minutes { get; set; }
        public int UnacknowledgedAlerts { get; set; }

        // Response Rate Metrics
        public decimal ResponseRateUnder5Min => TotalAlerts > 0 ? (decimal)AlertsUnder5Minutes / TotalAlerts * 100 : 0;
        public decimal ResponseRateUnder15Min => TotalAlerts > 0 ? (decimal)AlertsUnder15Minutes / TotalAlerts * 100 : 0;
        public decimal OverallResponseRate => TotalAlerts > 0 ? (decimal)(TotalAlerts - UnacknowledgedAlerts) / TotalAlerts * 100 : 0;

        // Critical Alert Response
        public decimal AverageCriticalResponseTime { get; set; }
        public int CriticalAlertsUnder5Minutes { get; set; }
        public int CriticalAlertsUnacknowledged { get; set; }
        public decimal CriticalResponseRateUnder5Min => CriticalAlerts > 0 ? (decimal)CriticalAlertsUnder5Minutes / CriticalAlerts * 100 : 0;

        // Alert Outcomes
        public int AlertsWithIntervention { get; set; }
        public int AlertsResolvedSuccessfully { get; set; }
        public int AlertsWithAdverseOutcome { get; set; }
        public int FalsePositiveAlerts { get; set; }
        public decimal SuccessfulResolutionRate => AlertsWithIntervention > 0 ? (decimal)AlertsResolvedSuccessfully / AlertsWithIntervention * 100 : 0;

        // Time-Based Analysis
        public Dictionary<string, int> AlertsByHourOfDay { get; set; } = new();
        public Dictionary<string, int> AlertsByDayOfWeek { get; set; } = new();
        public string PeakAlertHour { get; set; } = string.Empty;
        public string PeakAlertDay { get; set; } = string.Empty;

        public List<AlertResponseCase> Cases { get; set; } = new();

        // Alert Type Breakdown
        public Dictionary<string, int> AlertTypeFrequency { get; set; } = new();
        public Dictionary<string, double> AverageResponseTimeByType { get; set; } = new();
        public List<AlertTypeSummary> AlertTypeSummaries { get; set; } = new();
    }

    public class AlertTypeSummary
    {
        public string AlertType { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int CriticalCount { get; set; }
        public int WarningCount { get; set; }
        public int InfoCount { get; set; }
        public decimal AverageResponseTime { get; set; }
        public decimal ResponseRate { get; set; }
        public int InterventionsRequired { get; set; }
    }

    public class AlertResponseCase
    {
        public Guid AlertID { get; set; }
        public Guid PartographID { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string HospitalNumber { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public string AlertSeverity { get; set; } = string.Empty;
        public string AlertMessage { get; set; } = string.Empty;
        public DateTime AlertTime { get; set; }
        public DateTime? AcknowledgedTime { get; set; }
        public DateTime? ResolvedTime { get; set; }
        public double? ResponseTimeMinutes { get; set; }
        public double? ResolutionTimeMinutes { get; set; }
        public string HandlerName { get; set; } = string.Empty;
        public string HandlerRole { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty;
        public string InterventionTaken { get; set; } = string.Empty;
        public bool WasEscalated { get; set; }
        public string EscalatedTo { get; set; } = string.Empty;
        public string ClinicalNotes { get; set; } = string.Empty;
    }

    // WHO Compliance Metrics Model
    public class WHOComplianceReport : BaseReport
    {
        public int TotalPartographs { get; set; }
        public int CompliantPartographs { get; set; }
        public decimal CompliancePercentage => TotalPartographs > 0 ? (decimal)CompliantPartographs / TotalPartographs * 100 : 0;

        // Labor Progression (WHO 2020 Standards)
        public int ActiveLaborStartedAt5cm { get; set; }
        public int AlertLineCrossings { get; set; }
        public int ActionLineCrossings { get; set; }
        public int AppropriateInterventions { get; set; }
        public decimal AlertLineCrossingRate => TotalPartographs > 0 ? (decimal)AlertLineCrossings / TotalPartographs * 100 : 0;
        public decimal ActionLineCrossingRate => TotalPartographs > 0 ? (decimal)ActionLineCrossings / TotalPartographs * 100 : 0;
        public decimal InterventionRateOnAlertCrossing => AlertLineCrossings > 0 ? (decimal)AppropriateInterventions / AlertLineCrossings * 100 : 0;

        // Monitoring Frequency Compliance (WHO 2020)
        public decimal FHREvery30MinCompliance { get; set; } // Percentage
        public decimal VEEvery4HoursCompliance { get; set; }
        public decimal VitalSignsHourlyCompliance { get; set; }
        public decimal ContractionsEvery30MinCompliance { get; set; }
        public decimal AmnitoticFluidAssessmentCompliance { get; set; }
        public decimal MouldingAssessmentCompliance { get; set; }
        public decimal OverallMonitoringCompliance => (FHREvery30MinCompliance + VEEvery4HoursCompliance + VitalSignsHourlyCompliance + ContractionsEvery30MinCompliance) / 4;

        // Specific Monitoring Metrics
        public int TotalFHRRecordings { get; set; }
        public int ExpectedFHRRecordings { get; set; }
        public int TotalVaginalExams { get; set; }
        public int ExpectedVaginalExams { get; set; }
        public int TotalBPRecordings { get; set; }
        public int ExpectedBPRecordings { get; set; }

        // Essential Care Practices (WHO 2020)
        public int DelayedCordClampingCount { get; set; }
        public int SkinToSkinContactCount { get; set; }
        public int EarlyBreastfeedingCount { get; set; }
        public int VitaminKGivenCount { get; set; }
        public int EyeProphylaxisCount { get; set; }
        public int ActiveManagementThirdStageCount { get; set; }
        public decimal DelayedCordClampingRate => TotalPartographs > 0 ? (decimal)DelayedCordClampingCount / TotalPartographs * 100 : 0;
        public decimal SkinToSkinRate => TotalPartographs > 0 ? (decimal)SkinToSkinContactCount / TotalPartographs * 100 : 0;
        public decimal EarlyBreastfeedingRate => TotalPartographs > 0 ? (decimal)EarlyBreastfeedingCount / TotalPartographs * 100 : 0;
        public decimal VitaminKRate => TotalPartographs > 0 ? (decimal)VitaminKGivenCount / TotalPartographs * 100 : 0;
        public decimal EssentialCareComplianceRate => TotalPartographs > 0 ?
            (decimal)(DelayedCordClampingCount + SkinToSkinContactCount + EarlyBreastfeedingCount + VitaminKGivenCount) / (TotalPartographs * 4) * 100 : 0;

        // Partograph Usage Compliance
        public int PartographsStartedOnTime { get; set; }
        public int PartographsWithCervimetryPlotted { get; set; }
        public int PartographsWithDescentPlotted { get; set; }
        public int PartographsWithAllVitals { get; set; }
        public decimal PartographStartTimeCompliance => TotalPartographs > 0 ? (decimal)PartographsStartedOnTime / TotalPartographs * 100 : 0;

        // Documentation Quality
        public decimal AverageDataCompleteness { get; set; } // Percentage
        public int MissingCriticalData { get; set; }
        public int PartographsWithIncompleteData { get; set; }
        public List<DataCompletenessMetric> DataCompletenessBreakdown { get; set; } = new();

        // Intervention Compliance
        public int TimelyAugmentationCount { get; set; }
        public int AppropriateCaesareanDecisions { get; set; }
        public int OxytocinUsedCorrectly { get; set; }

        // Quality Indicators
        public decimal AverageTimeToActiveLabor { get; set; } // hours
        public decimal AverageSecondStageDuration { get; set; } // hours
        public int ProlongedSecondStage { get; set; }

        public List<WHOComplianceCase> NonCompliantCases { get; set; } = new();
        public List<WHOComplianceCase> FullyCompliantCases { get; set; } = new();
    }

    public class DataCompletenessMetric
    {
        public string DataField { get; set; } = string.Empty;
        public int RecordedCount { get; set; }
        public int ExpectedCount { get; set; }
        public decimal CompletenessRate => ExpectedCount > 0 ? (decimal)RecordedCount / ExpectedCount * 100 : 0;
    }

    public class WHOComplianceCase
    {
        public Guid PartographID { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string HospitalNumber { get; set; } = string.Empty;
        public DateTime LaborStartTime { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public decimal TotalLaborDuration { get; set; }
        public List<string> NonComplianceIssues { get; set; } = new();
        public List<string> ComplianceAchieved { get; set; } = new();
        public decimal OverallComplianceScore { get; set; }
        public string AttendingMidwife { get; set; } = string.Empty;
        public int FHRRecordingCount { get; set; }
        public int VaginalExamCount { get; set; }
        public bool AlertLineCrossed { get; set; }
        public bool ActionLineCrossed { get; set; }
        public string InterventionTaken { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty;
    }

    // Staff Performance Report Model
    public class StaffPerformanceReport : BaseReport
    {
        public int TotalStaff { get; set; }
        public int ActiveStaff { get; set; }
        public List<StaffPerformanceData> StaffPerformance { get; set; } = new();

        // Summary Statistics
        public decimal AverageDeliveriesPerStaff => TotalStaff > 0 ? (decimal)StaffPerformance.Sum(s => s.TotalDeliveries) / TotalStaff : 0;
        public decimal AverageComplicationRate => StaffPerformance.Any() ? StaffPerformance.Average(s => s.ComplicationRate) : 0;
        public decimal AverageWHOCompliance => StaffPerformance.Any() ? StaffPerformance.Average(s => s.WHOProtocolCompliance) : 0;
        public decimal AverageDocumentationScore => StaffPerformance.Any() ? StaffPerformance.Average(s => s.DocumentationCompleteness) : 0;

        // Top Performers
        public List<StaffPerformanceData> TopPerformers { get; set; } = new();
        public List<StaffPerformanceData> NeedingImprovement { get; set; } = new();

        // Role-Based Summary
        public Dictionary<string, RolePerformanceSummary> PerformanceByRole { get; set; } = new();

        // Training Recommendations
        public List<TrainingRecommendation> TrainingRecommendations { get; set; } = new();
    }

    public class RolePerformanceSummary
    {
        public string Role { get; set; } = string.Empty;
        public int StaffCount { get; set; }
        public int TotalDeliveries { get; set; }
        public decimal AverageComplicationRate { get; set; }
        public decimal AverageWHOCompliance { get; set; }
        public decimal AverageDocumentationScore { get; set; }
    }

    public class TrainingRecommendation
    {
        public Guid StaffID { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string TrainingArea { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty; // High, Medium, Low
    }

    public class StaffPerformanceData
    {
        public Guid StaffID { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;

        // Workload Metrics
        public int TotalDeliveries { get; set; }
        public int ActivePatients { get; set; }
        public decimal AveragePatientsPerShift { get; set; }
        public int TotalShifts { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public decimal AverageHoursPerShift => TotalShifts > 0 ? TotalHoursWorked / TotalShifts : 0;

        // Delivery Outcomes
        public int SuccessfulDeliveries { get; set; }
        public int Complications { get; set; }
        public int VaginalDeliveries { get; set; }
        public int AssistedDeliveries { get; set; }
        public int CaesareanSectionsAttended { get; set; }
        public decimal ComplicationRate => TotalDeliveries > 0 ? (decimal)Complications / TotalDeliveries * 100 : 0;
        public decimal SuccessRate => TotalDeliveries > 0 ? (decimal)SuccessfulDeliveries / TotalDeliveries * 100 : 0;

        // Alert Response
        public int AlertsHandled { get; set; }
        public decimal AverageResponseTimeMinutes { get; set; }
        public int AlertsUnder5Minutes { get; set; }
        public decimal AlertResponseRate => AlertsHandled > 0 ? (decimal)AlertsUnder5Minutes / AlertsHandled * 100 : 0;

        // Documentation Quality
        public decimal DocumentationCompleteness { get; set; } // Percentage
        public int MissingDataPoints { get; set; }
        public int PartographsCompleted { get; set; }
        public int PartographsWithFullData { get; set; }
        public decimal DocumentationAccuracy { get; set; }

        // Compliance Metrics
        public decimal WHOProtocolCompliance { get; set; } // Percentage
        public decimal FHRMonitoringCompliance { get; set; }
        public decimal VitalSignsMonitoringCompliance { get; set; }
        public decimal EssentialCareCompliance { get; set; }

        // Patient Feedback (if available)
        public decimal PatientSatisfactionScore { get; set; }
        public int PatientComplaints { get; set; }
        public int PatientCompliments { get; set; }

        // Performance Score
        public decimal OverallPerformanceScore { get; set; }
        public string PerformanceRating { get; set; } = string.Empty; // Excellent, Good, Satisfactory, Needs Improvement

        // Trend Data
        public decimal PreviousPeriodPerformanceScore { get; set; }
        public decimal PerformanceChange => PreviousPeriodPerformanceScore > 0 ?
            (OverallPerformanceScore - PreviousPeriodPerformanceScore) / PreviousPeriodPerformanceScore * 100 : 0;
        public string TrendDirection { get; set; } = string.Empty; // Improving, Stable, Declining
    }

    // Offline Sync Status Report Model
    public class OfflineSyncStatusReport : BaseReport
    {
        public int TotalDevices { get; set; }
        public int ActiveDevices { get; set; }
        public int OfflineDevices { get; set; }
        public int DevicesWithPendingChanges { get; set; }
        public int TotalPendingChanges { get; set; }
        public int TotalConflicts { get; set; }
        public int ResolvedConflicts { get; set; }
        public int UnresolvedConflicts { get; set; }

        // Sync Statistics
        public int TotalSyncsToday { get; set; }
        public int TotalSyncsThisWeek { get; set; }
        public int SuccessfulSyncs { get; set; }
        public int FailedSyncs { get; set; }
        public decimal SyncSuccessRate => (SuccessfulSyncs + FailedSyncs) > 0 ? (decimal)SuccessfulSyncs / (SuccessfulSyncs + FailedSyncs) * 100 : 0;

        // Data Volume Metrics
        public long TotalDataSyncedBytes { get; set; }
        public long TotalPendingDataBytes { get; set; }
        public string TotalDataSyncedFormatted => FormatBytes(TotalDataSyncedBytes);
        public string TotalPendingDataFormatted => FormatBytes(TotalPendingDataBytes);

        // Average Sync Times
        public decimal AverageSyncDurationSeconds { get; set; }
        public decimal AverageTimeSinceLastSyncHours { get; set; }
        public DateTime? LastGlobalSyncTime { get; set; }

        // Device Health
        public int DevicesNeedingAttention { get; set; }
        public int DevicesWithErrors { get; set; }
        public int DevicesOutOfSync { get; set; } // Last sync > 24 hours

        // Data Entity Sync Status
        public List<EntitySyncStatus> EntitySyncStatuses { get; set; } = new();

        public List<DeviceSyncStatus> DeviceStatuses { get; set; } = new();
        public List<SyncConflict> ActiveConflicts { get; set; } = new();
        public List<SyncError> RecentErrors { get; set; } = new();

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }

    public class EntitySyncStatus
    {
        public string EntityType { get; set; } = string.Empty; // Partograph, Patient, BirthOutcome, etc.
        public int TotalRecords { get; set; }
        public int SyncedRecords { get; set; }
        public int PendingRecords { get; set; }
        public int ConflictedRecords { get; set; }
        public DateTime? LastSyncTime { get; set; }
        public decimal SyncPercentage => TotalRecords > 0 ? (decimal)SyncedRecords / TotalRecords * 100 : 100;
    }

    public class SyncConflict
    {
        public Guid ConflictID { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityID { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public DateTime ConflictTime { get; set; }
        public string ConflictType { get; set; } = string.Empty; // Update-Update, Update-Delete, etc.
        public string LocalValue { get; set; } = string.Empty;
        public string ServerValue { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
        public DateTime? ResolvedTime { get; set; }
        public string ResolvedBy { get; set; } = string.Empty;
    }

    public class SyncError
    {
        public Guid ErrorID { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public DateTime ErrorTime { get; set; }
        public string ErrorType { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid? EntityID { get; set; }
        public int RetryCount { get; set; }
        public bool IsResolved { get; set; }
    }

    public class DeviceSyncStatus
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceModel { get; set; } = string.Empty;
        public string OSVersion { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public DateTime? LastSyncTime { get; set; }
        public DateTime? LastOnlineTime { get; set; }
        public int PendingChanges { get; set; }
        public int Conflicts { get; set; }
        public int Errors { get; set; }
        public bool IsOnline { get; set; }
        public string SyncStatus { get; set; } = string.Empty; // Synced, Syncing, Pending, Error, Offline
        public long DataVolume { get; set; } // in bytes
        public string DataVolumeFormatted { get; set; } = string.Empty;
        public decimal BatteryLevel { get; set; }
        public string NetworkType { get; set; } = string.Empty; // WiFi, Mobile, Offline
        public string AssignedUser { get; set; } = string.Empty;
        public string AssignedDepartment { get; set; } = string.Empty;
        public int TotalSyncsToday { get; set; }
        public decimal AverageSyncDurationSeconds { get; set; }
        public List<string> PendingEntityTypes { get; set; } = new();
    }

    // Birth Weight & APGAR Analysis Model
    public class BirthWeightApgarAnalysis : BaseReport
    {
        public int TotalBabies { get; set; }
        public int TotalWithCompleteData { get; set; }

        // Birth Weight Distribution
        public List<BirthWeightCategory> WeightDistribution { get; set; } = new();
        public decimal AverageBirthWeight { get; set; }
        public decimal MedianBirthWeight { get; set; }
        public decimal StandardDeviation { get; set; }
        public decimal MinBirthWeight { get; set; }
        public decimal MaxBirthWeight { get; set; }
        public decimal BirthWeightRange => MaxBirthWeight - MinBirthWeight;
        public int LowBirthWeightCount { get; set; }
        public decimal LowBirthWeightRate => TotalBabies > 0 ? (decimal)LowBirthWeightCount / TotalBabies * 100 : 0;

        // APGAR Distribution
        public List<ApgarScoreCategory> Apgar1MinDistribution { get; set; } = new();
        public List<ApgarScoreCategory> Apgar5MinDistribution { get; set; } = new();
        public List<ApgarScoreCategory> Apgar10MinDistribution { get; set; } = new();
        public decimal AverageApgar1Min { get; set; }
        public decimal AverageApgar5Min { get; set; }
        public decimal AverageApgar10Min { get; set; }
        public decimal MedianApgar1Min { get; set; }
        public decimal MedianApgar5Min { get; set; }
        public int LowApgar1MinCount { get; set; } // <7
        public int LowApgar5MinCount { get; set; } // <7
        public int CriticalApgar1MinCount { get; set; } // 0-3
        public int CriticalApgar5MinCount { get; set; } // 0-3

        // Correlation Analysis
        public decimal LowBirthWeightLowApgarCorrelation { get; set; }
        public decimal PearsonCorrelationCoefficient { get; set; }
        public List<BirthWeightApgarCorrelation> CorrelationData { get; set; } = new();

        // Risk Analysis
        public List<RiskCategoryAnalysis> RiskAnalysis { get; set; } = new();
        public int HighRiskBabies { get; set; }
        public decimal HighRiskPercentage => TotalBabies > 0 ? (decimal)HighRiskBabies / TotalBabies * 100 : 0;

        // Outcomes by Weight Category
        public List<WeightCategoryOutcome> OutcomesByWeight { get; set; } = new();

        // Gender Comparison
        public GenderComparison MaleStatistics { get; set; } = new();
        public GenderComparison FemaleStatistics { get; set; } = new();

        // Gestational Age Analysis
        public List<GestationalAgeWeightAnalysis> GestationalAgeAnalysis { get; set; } = new();

        // Trend Data (if comparing periods)
        public decimal PreviousPeriodAverageWeight { get; set; }
        public decimal WeightTrendChange { get; set; }
        public decimal PreviousPeriodLowBirthWeightRate { get; set; }
        public string TrendDirection { get; set; } = string.Empty;
    }

    public class BirthWeightCategory
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public decimal MinWeight { get; set; }
        public decimal MaxWeight { get; set; }
        public decimal AverageWeight { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }
        public int NICUAdmissions { get; set; }
        public decimal MortalityRate { get; set; }
    }

    public class ApgarScoreCategory
    {
        public string ScoreRange { get; set; } = string.Empty;
        public int MinScore { get; set; }
        public int MaxScore { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public string Interpretation { get; set; } = string.Empty; // Normal, Moderate Depression, Severe Depression
        public decimal AverageBirthWeight { get; set; }
        public int ResuscitationRequired { get; set; }
        public int NICUAdmissions { get; set; }
    }

    public class BirthWeightApgarCorrelation
    {
        public string WeightCategory { get; set; } = string.Empty;
        public decimal AverageApgar1Min { get; set; }
        public decimal AverageApgar5Min { get; set; }
        public decimal AverageApgar10Min { get; set; }
        public int Count { get; set; }
        public decimal ApgarImprovementRate { get; set; }
        public int LowApgar1MinCount { get; set; }
        public int LowApgar5MinCount { get; set; }
        public decimal ResuscitationRate { get; set; }
    }

    public class RiskCategoryAnalysis
    {
        public string RiskCategory { get; set; } = string.Empty; // High, Medium, Low
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public string Criteria { get; set; } = string.Empty;
        public decimal AverageWeight { get; set; }
        public decimal AverageApgar { get; set; }
        public int NICUAdmissions { get; set; }
        public int Mortalities { get; set; }
    }

    public class WeightCategoryOutcome
    {
        public string WeightCategory { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int LiveBirths { get; set; }
        public int Stillbirths { get; set; }
        public int NeonatalDeaths { get; set; }
        public int NICUAdmissions { get; set; }
        public decimal SurvivalRate => TotalCount > 0 ? (decimal)LiveBirths / TotalCount * 100 : 0;
        public decimal NICUAdmissionRate => TotalCount > 0 ? (decimal)NICUAdmissions / TotalCount * 100 : 0;
    }

    public class GenderComparison
    {
        public int Count { get; set; }
        public decimal AverageBirthWeight { get; set; }
        public decimal MedianBirthWeight { get; set; }
        public decimal StandardDeviation { get; set; }
        public decimal AverageApgar1Min { get; set; }
        public decimal AverageApgar5Min { get; set; }
        public int LowBirthWeightCount { get; set; }
        public decimal LowBirthWeightRate => Count > 0 ? (decimal)LowBirthWeightCount / Count * 100 : 0;
    }

    public class GestationalAgeWeightAnalysis
    {
        public string GestationalAgeCategory { get; set; } = string.Empty;
        public int WeeksMin { get; set; }
        public int WeeksMax { get; set; }
        public int Count { get; set; }
        public decimal AverageWeight { get; set; }
        public decimal ExpectedWeight { get; set; }
        public decimal WeightPercentile { get; set; }
        public int SmallForGestationalAge { get; set; }
        public int LargeForGestationalAge { get; set; }
        public decimal AverageApgar1Min { get; set; }
        public decimal AverageApgar5Min { get; set; }
    }

    // Trend Analytics Model
    public class TrendAnalyticsReport : BaseReport
    {
        public List<MonthlyTrend> MonthlyTrends { get; set; } = new();
        public List<QuarterlyTrend> QuarterlyTrends { get; set; } = new();
        public List<YearlyTrend> YearlyTrends { get; set; } = new();
        public List<WeeklyTrend> WeeklyTrends { get; set; } = new();

        // Summary Statistics
        public int TotalPeriods { get; set; }
        public int TotalDeliveriesInPeriod { get; set; }
        public decimal AverageDeliveriesPerMonth { get; set; }
        public decimal DeliveryGrowthRate { get; set; }

        // Key Performance Indicators
        public TrendIndicator CaesareanSectionRate { get; set; } = new();
        public TrendIndicator MaternalMortalityRate { get; set; } = new();
        public TrendIndicator NeonatalMortalityRate { get; set; } = new();
        public TrendIndicator StillbirthRate { get; set; } = new();
        public TrendIndicator PostpartumHemorrhageRate { get; set; } = new();
        public TrendIndicator WHOComplianceRate { get; set; } = new();
        public TrendIndicator LowBirthWeightRate { get; set; } = new();
        public TrendIndicator PretermBirthRate { get; set; } = new();
        public TrendIndicator AverageLaborDuration { get; set; } = new();
        public TrendIndicator AlertResponseTime { get; set; } = new();
        public TrendIndicator DocumentationCompleteness { get; set; } = new();

        // Forecasting (simple linear regression)
        public List<ForecastData> DeliveryForecast { get; set; } = new();
        public List<ForecastData> ComplicationForecast { get; set; } = new();

        // Seasonal Analysis
        public SeasonalAnalysis SeasonalPatterns { get; set; } = new();

        // Comparative Analysis
        public List<PeriodComparison> PeriodComparisons { get; set; } = new();

        // Performance Benchmarks
        public List<BenchmarkComparison> Benchmarks { get; set; } = new();
    }

    public class MonthlyTrend
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty; // "Jan 2024"
        public int TotalDeliveries { get; set; }
        public int LiveBirths { get; set; }
        public int Stillbirths { get; set; }
        public int Complications { get; set; }
        public int MaternalDeaths { get; set; }
        public int NeonatalDeaths { get; set; }
        public decimal CaesareanSectionRate { get; set; }
        public decimal VaginalDeliveryRate { get; set; }
        public decimal MaternalMortalityRate { get; set; }
        public decimal NeonatalMortalityRate { get; set; }
        public decimal StillbirthRate { get; set; }
        public decimal ComplicationRate { get; set; }
        public decimal AverageLaborDuration { get; set; }
        public decimal WHOComplianceRate { get; set; }
        public decimal LowBirthWeightRate { get; set; }
        public decimal AverageApgarScore { get; set; }
        public int NICUAdmissions { get; set; }

        // Month-over-Month Changes
        public decimal DeliveryChangePercent { get; set; }
        public decimal ComplicationChangePercent { get; set; }
    }

    public class QuarterlyTrend
    {
        public int Year { get; set; }
        public int Quarter { get; set; }
        public string QuarterName { get; set; } = string.Empty; // "Q1 2024"
        public int TotalDeliveries { get; set; }
        public int LiveBirths { get; set; }
        public int Complications { get; set; }
        public int MaternalDeaths { get; set; }
        public int NeonatalDeaths { get; set; }
        public decimal AverageLaborDuration { get; set; }
        public decimal ComplicationRate { get; set; }
        public decimal WHOComplianceRate { get; set; }
        public decimal CaesareanSectionRate { get; set; }
        public decimal MaternalMortalityRate { get; set; }
        public decimal NeonatalMortalityRate { get; set; }
        public decimal StillbirthRate { get; set; }
        public decimal LowBirthWeightRate { get; set; }
        public decimal DocumentationCompleteness { get; set; }

        // Quarter-over-Quarter Changes
        public decimal DeliveryChangePercent { get; set; }
        public decimal ComplicationChangePercent { get; set; }
        public string TrendDirection { get; set; } = string.Empty;
    }

    public class YearlyTrend
    {
        public int Year { get; set; }
        public int TotalDeliveries { get; set; }
        public int LiveBirths { get; set; }
        public int Stillbirths { get; set; }
        public int Complications { get; set; }
        public int MaternalDeaths { get; set; }
        public int NeonatalDeaths { get; set; }
        public decimal CaesareanSectionRate { get; set; }
        public decimal MaternalMortalityRatio { get; set; }
        public decimal NeonatalMortalityRate { get; set; }
        public decimal StillbirthRate { get; set; }
        public decimal ComplicationRate { get; set; }
        public decimal WHOComplianceRate { get; set; }
        public decimal YearOverYearGrowth { get; set; }
    }

    public class WeeklyTrend
    {
        public int Year { get; set; }
        public int WeekNumber { get; set; }
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public int TotalDeliveries { get; set; }
        public int Complications { get; set; }
        public decimal CaesareanSectionRate { get; set; }
        public decimal ComplicationRate { get; set; }
    }

    public class TrendIndicator
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty; // "%", "per 1000", "hours"
        public decimal CurrentValue { get; set; }
        public decimal PreviousPeriodValue { get; set; }
        public decimal ChangePercentage { get; set; }
        public decimal ChangeAbsolute => CurrentValue - PreviousPeriodValue;
        public string Trend { get; set; } = string.Empty; // "Increasing", "Decreasing", "Stable"
        public string TrendDirection { get; set; } = string.Empty; // "Positive", "Negative", "Neutral"
        public decimal TargetValue { get; set; }
        public bool IsOnTarget => Math.Abs(CurrentValue - TargetValue) <= (TargetValue * 0.1m); // Within 10%
        public decimal DistanceFromTarget => CurrentValue - TargetValue;
        public string Status { get; set; } = string.Empty; // "Good", "Warning", "Critical"
        public List<decimal> HistoricalValues { get; set; } = new();
        public decimal MovingAverage3Month { get; set; }
        public decimal MovingAverage6Month { get; set; }
    }

    public class ForecastData
    {
        public string Period { get; set; } = string.Empty;
        public DateTime ForecastDate { get; set; }
        public decimal ForecastValue { get; set; }
        public decimal LowerBound { get; set; } // 95% confidence interval
        public decimal UpperBound { get; set; }
        public decimal ConfidenceLevel { get; set; }
    }

    public class SeasonalAnalysis
    {
        public string HighestDeliveryMonth { get; set; } = string.Empty;
        public string LowestDeliveryMonth { get; set; } = string.Empty;
        public decimal SeasonalVariation { get; set; }
        public Dictionary<string, decimal> MonthlyAverages { get; set; } = new();
        public Dictionary<string, decimal> DayOfWeekAverages { get; set; } = new();
        public string PeakDeliveryDay { get; set; } = string.Empty;
        public bool HasSeasonalPattern { get; set; }
    }

    public class PeriodComparison
    {
        public string Period1Name { get; set; } = string.Empty;
        public string Period2Name { get; set; } = string.Empty;
        public DateTime Period1Start { get; set; }
        public DateTime Period1End { get; set; }
        public DateTime Period2Start { get; set; }
        public DateTime Period2End { get; set; }
        public Dictionary<string, ComparisonMetric> Metrics { get; set; } = new();
    }

    public class ComparisonMetric
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal Period1Value { get; set; }
        public decimal Period2Value { get; set; }
        public decimal Difference { get; set; }
        public decimal PercentageChange { get; set; }
        public string ChangeDirection { get; set; } = string.Empty;
        public bool IsStatisticallySignificant { get; set; }
    }

    public class BenchmarkComparison
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal FacilityValue { get; set; }
        public decimal NationalBenchmark { get; set; }
        public decimal WHOBenchmark { get; set; }
        public decimal RegionalAverage { get; set; }
        public string PerformanceRating { get; set; } = string.Empty; // "Above Average", "At Target", "Below Target"
        public decimal GapFromBenchmark { get; set; }
    }

    // Individual Partograph PDF Export Data
    public class PartographPDFData
    {
        // Header Information
        public string ReportTitle { get; set; } = "Partograph Report";
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public string GeneratedBy { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;
        public string FacilityAddress { get; set; } = string.Empty;
        public string FacilityLogo { get; set; } = string.Empty; // Base64 encoded
        public string ReportNumber { get; set; } = string.Empty;

        // Core Data
        public Patient Patient { get; set; } = null!;
        public Partograph Partograph { get; set; } = null!;
        public BirthOutcome BirthOutcome { get; set; } = null!;
        public List<BabyDetails> Babies { get; set; } = new();

        // Patient Summary
        public PatientSummaryData PatientSummary { get; set; } = new();

        // Staff Information
        public string AttendingMidwife { get; set; } = string.Empty;
        public string AttendingDoctor { get; set; } = string.Empty;
        public List<StaffInvolvement> StaffInvolved { get; set; } = new();

        // All measurements
        public List<FHR> FHRMeasurements { get; set; } = new();
        public List<Contraction> Contractions { get; set; } = new();
        public List<CervixDilatation> CervicalDilations { get; set; } = new();
        public List<HeadDescent> HeadDescents { get; set; } = new();
        public List<BP> BloodPressures { get; set; } = new();
        public List<Temperature> Temperatures { get; set; } = new();
        public List<Urine> UrineOutputs { get; set; } = new();
        public List<Pulse> PulseMeasurements { get; set; } = new();
        public List<OxytocinAdministration> OxytocinRecords { get; set; } = new();
        public List<MedicationRecord> MedicationsGiven { get; set; } = new();
        public List<FluidIntake> FluidIntakeRecords { get; set; } = new();

        // Labor Progress Summary
        public LaborProgressSummary LaborProgress { get; set; } = new();

        // Chart images (base64 encoded)
        public string CervicalDilationChartImage { get; set; } = string.Empty;
        public string FHRChartImage { get; set; } = string.Empty;
        public string VitalSignsChartImage { get; set; } = string.Empty;
        public string ContractionChartImage { get; set; } = string.Empty;
        public string HeadDescentChartImage { get; set; } = string.Empty;
        public string CombinedPartographImage { get; set; } = string.Empty;

        // Labor Stages Summary
        public LaborStagesSummary StagesSummary { get; set; } = new();

        // Delivery Summary
        public DeliverySummary DeliverySummary { get; set; } = new();

        // Neonatal Summary
        public List<NeonatalSummary> NeonatalSummaries { get; set; } = new();

        // Summary
        public string LaborDuration { get; set; } = string.Empty;
        public decimal LaborDurationHours { get; set; }
        public List<string> ClinicalNotes { get; set; } = new();
        public List<string> CriticalAlerts { get; set; } = new();
        public List<AlertRecord> DetailedAlerts { get; set; } = new();

        // WHO Compliance for this Partograph
        public PartographComplianceData Compliance { get; set; } = new();

        // Signatures
        public string MidwifeSignature { get; set; } = string.Empty;
        public string DoctorSignature { get; set; } = string.Empty;
        public DateTime? SignedAt { get; set; }

        // Export Metadata
        public string PDFVersion { get; set; } = "1.0";
        public string ExportFormat { get; set; } = "PDF";
        public bool IncludeCharts { get; set; } = true;
        public bool IncludeRawData { get; set; } = false;
    }

    public class PatientSummaryData
    {
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string HospitalNumber { get; set; } = string.Empty;
        public string NationalID { get; set; } = string.Empty;
        public int Gravida { get; set; }
        public int Parity { get; set; }
        public string GravidaParaString => $"G{Gravida}P{Parity}";
        public int GestationalAgeWeeks { get; set; }
        public int GestationalAgeDays { get; set; }
        public string GestationalAgeString => $"{GestationalAgeWeeks}+{GestationalAgeDays} weeks";
        public DateTime? LMP { get; set; }
        public DateTime? EDD { get; set; }
        public string BloodGroup { get; set; } = string.Empty;
        public string RhFactor { get; set; } = string.Empty;
        public string HIVStatus { get; set; } = string.Empty;
        public decimal Height { get; set; }
        public decimal Weight { get; set; }
        public decimal BMI => Height > 0 ? Weight / ((Height / 100) * (Height / 100)) : 0;
        public List<string> RiskFactors { get; set; } = new();
        public List<string> MedicalHistory { get; set; } = new();
        public List<string> ObstetricHistory { get; set; } = new();
        public string AllergiesString { get; set; } = string.Empty;
        public DateTime AdmissionTime { get; set; }
        public string ReferralSource { get; set; } = string.Empty;
    }

    public class StaffInvolvement
    {
        public Guid StaffID { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime InvolvementStart { get; set; }
        public DateTime? InvolvementEnd { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class Pulse
    {
        public DateTime Time { get; set; }
        public int Rate { get; set; }
        public string Quality { get; set; } = string.Empty;
    }

    public class OxytocinAdministration
    {
        public DateTime Time { get; set; }
        public decimal Dose { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string AdministeredBy { get; set; } = string.Empty;
    }

    public class MedicationRecord
    {
        public DateTime Time { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string Dose { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string Indication { get; set; } = string.Empty;
        public string AdministeredBy { get; set; } = string.Empty;
    }

    public class FluidIntake
    {
        public DateTime Time { get; set; }
        public string FluidType { get; set; } = string.Empty;
        public int VolumeML { get; set; }
        public string Route { get; set; } = string.Empty;
    }

    public class LaborProgressSummary
    {
        public DateTime? LaborOnsetTime { get; set; }
        public DateTime? ActivePhaseStart { get; set; }
        public DateTime? SecondStageStart { get; set; }
        public DateTime? ThirdStageStart { get; set; }
        public decimal FirstStageDurationHours { get; set; }
        public decimal ActivePhaseDurationHours { get; set; }
        public decimal SecondStageDurationMinutes { get; set; }
        public decimal ThirdStageDurationMinutes { get; set; }
        public bool ProlongedFirstStage { get; set; }
        public bool ProlongedSecondStage { get; set; }
        public bool ProlongedThirdStage { get; set; }
        public decimal CervicalDilationAtAdmission { get; set; }
        public decimal CervicalDilationRate { get; set; } // cm per hour
        public bool AlertLineCrossed { get; set; }
        public DateTime? AlertLineCrossedTime { get; set; }
        public bool ActionLineCrossed { get; set; }
        public DateTime? ActionLineCrossedTime { get; set; }
        public string InterventionsAtAlertLine { get; set; } = string.Empty;
        public string InterventionsAtActionLine { get; set; } = string.Empty;
    }

    public class LaborStagesSummary
    {
        public FirstStageSummary FirstStage { get; set; } = new();
        public SecondStageSummary SecondStage { get; set; } = new();
        public ThirdStageSummary ThirdStage { get; set; } = new();
        public FourthStageSummary FourthStage { get; set; } = new();
    }

    public class FirstStageSummary
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal DurationHours { get; set; }
        public string LatentPhaseNotes { get; set; } = string.Empty;
        public string ActivePhaseNotes { get; set; } = string.Empty;
        public int FHRReadingsCount { get; set; }
        public int AbnormalFHRCount { get; set; }
        public string MembraneStatus { get; set; } = string.Empty;
        public DateTime? MembraneRuptureTime { get; set; }
        public string LiquorColor { get; set; } = string.Empty;
        public string Complications { get; set; } = string.Empty;
    }

    public class SecondStageSummary
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal DurationMinutes { get; set; }
        public string PushingStartTime { get; set; } = string.Empty;
        public string DeliveryMechanism { get; set; } = string.Empty;
        public string PresentationAtDelivery { get; set; } = string.Empty;
        public bool EpisiotomyPerformed { get; set; }
        public string EpisiotomyIndication { get; set; } = string.Empty;
        public string Complications { get; set; } = string.Empty;
    }

    public class ThirdStageSummary
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal DurationMinutes { get; set; }
        public string PlacentaDeliveryMethod { get; set; } = string.Empty; // Spontaneous, CCT, Manual removal
        public bool AMTSLUsed { get; set; }
        public string OxytocinGiven { get; set; } = string.Empty;
        public bool PlacentaComplete { get; set; }
        public bool MembranesComplete { get; set; }
        public int EstimatedBloodLoss { get; set; }
        public string Complications { get; set; } = string.Empty;
    }

    public class FourthStageSummary
    {
        public DateTime? StartTime { get; set; }
        public decimal MonitoringDurationHours { get; set; }
        public string UterineContraction { get; set; } = string.Empty;
        public string VaginalBleedingAssessment { get; set; } = string.Empty;
        public string VitalSignsStatus { get; set; } = string.Empty;
        public string BladderStatus { get; set; } = string.Empty;
        public string PerinealCondition { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class DeliverySummary
    {
        public DateTime DeliveryTime { get; set; }
        public string DeliveryMode { get; set; } = string.Empty;
        public string DeliveryModeReason { get; set; } = string.Empty;
        public string Presentation { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string PerinealStatus { get; set; } = string.Empty;
        public string PerinealRepairDetails { get; set; } = string.Empty;
        public int EstimatedBloodLoss { get; set; }
        public string CordStatus { get; set; } = string.Empty;
        public bool CordAroundNeck { get; set; }
        public int CordLoops { get; set; }
        public string CordClampingTime { get; set; } = string.Empty;
        public bool DelayedCordClamping { get; set; }
        public string MaternalCondition { get; set; } = string.Empty;
        public string MaternalComplications { get; set; } = string.Empty;
        public string DeliveryNotes { get; set; } = string.Empty;
    }

    public class NeonatalSummary
    {
        public int BabyNumber { get; set; }
        public DateTime BirthTime { get; set; }
        public string Sex { get; set; } = string.Empty;
        public decimal BirthWeight { get; set; }
        public decimal Length { get; set; }
        public decimal HeadCircumference { get; set; }
        public int? Apgar1Min { get; set; }
        public int? Apgar5Min { get; set; }
        public int? Apgar10Min { get; set; }
        public string VitalStatus { get; set; } = string.Empty;
        public bool ResuscitationRequired { get; set; }
        public string ResuscitationDetails { get; set; } = string.Empty;
        public bool SkinToSkinDone { get; set; }
        public DateTime? SkinToSkinTime { get; set; }
        public bool EarlyBreastfeedingDone { get; set; }
        public DateTime? FirstBreastfeedTime { get; set; }
        public bool VitaminKGiven { get; set; }
        public bool EyeProphylaxis { get; set; }
        public string Abnormalities { get; set; } = string.Empty;
        public bool AdmittedToNICU { get; set; }
        public string NICUReason { get; set; } = string.Empty;
        public string NeonatalNotes { get; set; } = string.Empty;
    }

    public class AlertRecord
    {
        public DateTime AlertTime { get; set; }
        public string AlertType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime? AcknowledgedTime { get; set; }
        public string AcknowledgedBy { get; set; } = string.Empty;
        public string ActionTaken { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty;
    }

    public class PartographComplianceData
    {
        public decimal OverallComplianceScore { get; set; }
        public bool FHRMonitoringCompliant { get; set; }
        public int FHRRecordingsExpected { get; set; }
        public int FHRRecordingsActual { get; set; }
        public bool VaginalExamCompliant { get; set; }
        public int VaginalExamsExpected { get; set; }
        public int VaginalExamsActual { get; set; }
        public bool VitalSignsCompliant { get; set; }
        public bool PartographPlottedCorrectly { get; set; }
        public bool EssentialCareCompliant { get; set; }
        public List<string> ComplianceIssues { get; set; } = new();
        public List<string> ComplianceAchievements { get; set; } = new();
    }
}
