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

        // Complications
        public int PostpartumHemorrhages { get; set; }
        public int Eclampsia { get; set; }
        public int ObstructedLabor { get; set; }
        public int RupturedUterus { get; set; }

        // Neonatal Outcomes
        public int LowBirthWeightBabies { get; set; }
        public int NICUAdmissions { get; set; }
        public decimal AverageApgar1Min { get; set; }
        public decimal AverageApgar5Min { get; set; }

        // Labor Statistics
        public decimal AverageLaborDuration { get; set; } // in hours
        public int ProlongedLabors { get; set; }
        public int RapidLabors { get; set; }

        // WHO Compliance
        public decimal AlertLineCrossingsPercentage { get; set; }
        public decimal ActionLineCrossingsPercentage { get; set; }
    }

    // Maternal Complications Report Model
    public class MaternalComplicationsReport : BaseReport
    {
        public int TotalCases { get; set; }
        public List<MaternalComplicationCase> Cases { get; set; } = new();

        // Summary Statistics
        public int HypertensiveDisorders { get; set; }
        public int PostpartumHemorrhages { get; set; }
        public int SepticShock { get; set; }
        public int ObstructedLabor { get; set; }
        public int RupturedUterus { get; set; }
        public int Eclampsia { get; set; }
        public int MaternalDeaths { get; set; }

        // Blood Loss Statistics
        public decimal AverageBloodLoss { get; set; }
        public int CasesExceeding500ml { get; set; }
        public int CasesExceeding1000ml { get; set; }

        // Perineal Trauma
        public int IntactPerineum { get; set; }
        public int FirstDegreeTears { get; set; }
        public int SecondDegreeTears { get; set; }
        public int ThirdDegreeTears { get; set; }
        public int FourthDegreeTears { get; set; }
        public int Episiotomies { get; set; }
    }

    public class MaternalComplicationCase
    {
        public Guid PatientID { get; set; }
        public string PatientName { get; set; }
        public int Age { get; set; }
        public string HospitalNumber { get; set; }
        public DateTime DeliveryDate { get; set; }
        public List<string> Complications { get; set; } = new();
        public MaternalOutcomeStatus Outcome { get; set; }
        public int EstimatedBloodLoss { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
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
        public decimal StillbirthRate => TotalBirths > 0 ? (decimal)(FreshStillbirths + MaceratedStillbirths) / TotalBirths * 1000 : 0;
        public decimal EarlyNeonatalMortalityRate => LiveBirths > 0 ? (decimal)EarlyNeonatalDeaths / LiveBirths * 1000 : 0;

        // Birth Weight Distribution
        public int ExtremelyLowBirthWeight { get; set; } // <1000g
        public int VeryLowBirthWeight { get; set; } // 1000-1499g
        public int LowBirthWeight { get; set; } // 1500-2499g
        public int NormalWeight { get; set; } // 2500-3999g
        public int Macrosomia { get; set; } // >=4000g
        public decimal AverageBirthWeight { get; set; }

        // APGAR Scores
        public decimal AverageApgar1Min { get; set; }
        public decimal AverageApgar5Min { get; set; }
        public int Apgar1MinBelow7 { get; set; }
        public int Apgar5MinBelow7 { get; set; }

        // Resuscitation
        public int ResuscitationRequired { get; set; }
        public int BagMaskVentilation { get; set; }
        public int Intubations { get; set; }
        public int ChestCompressions { get; set; }

        // NICU Admissions
        public int NICUAdmissions { get; set; }
        public decimal NICUAdmissionRate => TotalBirths > 0 ? (decimal)NICUAdmissions / TotalBirths * 100 : 0;

        // Complications
        public int BirthAsphyxia { get; set; }
        public int RespiratoryDistress { get; set; }
        public int Sepsis { get; set; }
        public int Jaundice { get; set; }
        public int Hypothermia { get; set; }
        public int CongenitalAbnormalities { get; set; }
        public int BirthInjuries { get; set; }
    }

    public class NeonatalOutcomeCase
    {
        public Guid BabyID { get; set; }
        public string MotherName { get; set; }
        public string HospitalNumber { get; set; }
        public DateTime BirthTime { get; set; }
        public BabySex Sex { get; set; }
        public decimal BirthWeight { get; set; }
        public int? Apgar1Min { get; set; }
        public int? Apgar5Min { get; set; }
        public BabyVitalStatus VitalStatus { get; set; }
        public bool ResuscitationRequired { get; set; }
        public bool NICUAdmission { get; set; }
        public List<string> Complications { get; set; } = new();
    }

    // Alert Response Time Report Model
    public class AlertResponseTimeReport : BaseReport
    {
        public int TotalAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public int WarningAlerts { get; set; }
        public int InfoAlerts { get; set; }

        public decimal AverageResponseTime { get; set; } // in minutes
        public decimal MedianResponseTime { get; set; }
        public int AlertsUnder5Minutes { get; set; }
        public int AlertsUnder15Minutes { get; set; }
        public int AlertsOver30Minutes { get; set; }
        public int UnacknowledgedAlerts { get; set; }

        public List<AlertResponseCase> Cases { get; set; } = new();

        // Alert Type Breakdown
        public Dictionary<string, int> AlertTypeFrequency { get; set; } = new();
        public Dictionary<string, double> AverageResponseTimeByType { get; set; } = new();
    }

    public class AlertResponseCase
    {
        public Guid PartographID { get; set; }
        public string PatientName { get; set; }
        public string AlertType { get; set; }
        public string AlertSeverity { get; set; }
        public DateTime AlertTime { get; set; }
        public DateTime? AcknowledgedTime { get; set; }
        public double? ResponseTimeMinutes { get; set; }
        public string HandlerName { get; set; }
        public string Outcome { get; set; }
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
        public decimal AlertLineCrossingRate => TotalPartographs > 0 ? (decimal)AlertLineCrossings / TotalPartographs * 100 : 0;

        // Monitoring Frequency Compliance
        public decimal FHREvery30MinCompliance { get; set; } // Percentage
        public decimal VEEvery4HoursCompliance { get; set; }
        public decimal VitalSignsHourlyCompliance { get; set; }
        public decimal ContractionsEvery30MinCompliance { get; set; }

        // Essential Care Practices (WHO 2020)
        public int DelayedCordClampingCount { get; set; }
        public int SkinToSkinContactCount { get; set; }
        public int EarlyBreastfeedingCount { get; set; }
        public int VitaminKGivenCount { get; set; }
        public decimal DelayedCordClampingRate => TotalPartographs > 0 ? (decimal)DelayedCordClampingCount / TotalPartographs * 100 : 0;

        // Documentation Quality
        public decimal AverageDataCompleteness { get; set; } // Percentage
        public int MissingCriticalData { get; set; }

        public List<WHOComplianceCase> NonCompliantCases { get; set; } = new();
    }

    public class WHOComplianceCase
    {
        public Guid PartographID { get; set; }
        public string PatientName { get; set; }
        public string HospitalNumber { get; set; }
        public DateTime LaborStartTime { get; set; }
        public List<string> NonComplianceIssues { get; set; } = new();
    }

    // Staff Performance Report Model
    public class StaffPerformanceReport : BaseReport
    {
        public int TotalStaff { get; set; }
        public List<StaffPerformanceData> StaffPerformance { get; set; } = new();
    }

    public class StaffPerformanceData
    {
        public Guid StaffID { get; set; }
        public string StaffName { get; set; }
        public string Role { get; set; }

        // Workload
        public int TotalDeliveries { get; set; }
        public int ActivePatients { get; set; }
        public decimal AveragePatientsPerShift { get; set; }

        // Outcomes
        public int SuccessfulDeliveries { get; set; }
        public int Complications { get; set; }
        public decimal ComplicationRate => TotalDeliveries > 0 ? (decimal)Complications / TotalDeliveries * 100 : 0;

        // Documentation Quality
        public decimal DocumentationCompleteness { get; set; } // Percentage
        public int MissingDataPoints { get; set; }
        public decimal AverageResponseTimeMinutes { get; set; }

        // Compliance
        public decimal WHOProtocolCompliance { get; set; } // Percentage
    }

    // Offline Sync Status Report Model
    public class OfflineSyncStatusReport : BaseReport
    {
        public int TotalDevices { get; set; }
        public int ActiveDevices { get; set; }
        public int DevicesWithPendingChanges { get; set; }
        public int TotalPendingChanges { get; set; }
        public int TotalConflicts { get; set; }

        public List<DeviceSyncStatus> DeviceStatuses { get; set; } = new();
    }

    public class DeviceSyncStatus
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public DateTime? LastSyncTime { get; set; }
        public int PendingChanges { get; set; }
        public int Conflicts { get; set; }
        public bool IsOnline { get; set; }
        public string SyncStatus { get; set; }
        public long DataVolume { get; set; } // in bytes
    }

    // Birth Weight & APGAR Analysis Model
    public class BirthWeightApgarAnalysis : BaseReport
    {
        public int TotalBabies { get; set; }

        // Birth Weight Distribution
        public List<BirthWeightCategory> WeightDistribution { get; set; } = new();
        public decimal AverageBirthWeight { get; set; }
        public decimal MedianBirthWeight { get; set; }
        public decimal StandardDeviation { get; set; }

        // APGAR Distribution
        public List<ApgarScoreCategory> Apgar1MinDistribution { get; set; } = new();
        public List<ApgarScoreCategory> Apgar5MinDistribution { get; set; } = new();

        // Correlation Analysis
        public decimal LowBirthWeightLowApgarCorrelation { get; set; }
        public List<BirthWeightApgarCorrelation> CorrelationData { get; set; } = new();
    }

    public class BirthWeightCategory
    {
        public string Category { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public decimal MinWeight { get; set; }
        public decimal MaxWeight { get; set; }
    }

    public class ApgarScoreCategory
    {
        public string ScoreRange { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class BirthWeightApgarCorrelation
    {
        public string WeightCategory { get; set; }
        public decimal AverageApgar1Min { get; set; }
        public decimal AverageApgar5Min { get; set; }
        public int Count { get; set; }
    }

    // Trend Analytics Model
    public class TrendAnalyticsReport : BaseReport
    {
        public List<MonthlyTrend> MonthlyTrends { get; set; } = new();
        public List<QuarterlyTrend> QuarterlyTrends { get; set; } = new();

        // Key Performance Indicators
        public TrendIndicator CaesareanSectionRate { get; set; }
        public TrendIndicator MaternalMortalityRate { get; set; }
        public TrendIndicator NeonatalMortalityRate { get; set; }
        public TrendIndicator StillbirthRate { get; set; }
        public TrendIndicator PostpartumHemorrhageRate { get; set; }
        public TrendIndicator WHOComplianceRate { get; set; }
    }

    public class MonthlyTrend
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int TotalDeliveries { get; set; }
        public int Complications { get; set; }
        public decimal CaesareanSectionRate { get; set; }
        public decimal MaternalMortalityRate { get; set; }
        public decimal NeonatalMortalityRate { get; set; }
    }

    public class QuarterlyTrend
    {
        public int Year { get; set; }
        public int Quarter { get; set; }
        public int TotalDeliveries { get; set; }
        public decimal AverageLaborDuration { get; set; }
        public decimal ComplicationRate { get; set; }
        public decimal WHOComplianceRate { get; set; }
    }

    public class TrendIndicator
    {
        public string Name { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal PreviousPeriodValue { get; set; }
        public decimal ChangePercentage { get; set; }
        public string Trend { get; set; } // "Increasing", "Decreasing", "Stable"
        public string TrendDirection { get; set; } // "Positive", "Negative", "Neutral"
    }

    // Individual Partograph PDF Export Data
    public class PartographPDFData
    {
        public Patient Patient { get; set; }
        public Partograph Partograph { get; set; }
        public BirthOutcome BirthOutcome { get; set; }
        public List<BabyDetails> Babies { get; set; } = new();

        // All measurements
        public List<FHR> FHRMeasurements { get; set; } = new();
        public List<Contraction> Contractions { get; set; } = new();
        public List<CervixDilatation> CervicalDilations { get; set; } = new();
        public List<HeadDescent> HeadDescents { get; set; } = new();
        public List<BP> BloodPressures { get; set; } = new();
        public List<Temperature> Temperatures { get; set; } = new();
        public List<Urine> UrineOutputs { get; set; } = new();

        // Chart images (base64 encoded)
        public string CervicalDilationChartImage { get; set; }
        public string FHRChartImage { get; set; }
        public string VitalSignsChartImage { get; set; }

        // Summary
        public string LaborDuration { get; set; }
        public List<string> ClinicalNotes { get; set; } = new();
        public List<string> CriticalAlerts { get; set; } = new();
    }
}
