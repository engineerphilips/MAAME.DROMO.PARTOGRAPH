using System;
using System.Collections.Generic;
using System.Linq;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    /// <summary>
    /// Robson Classification Report - WHO 2017 Standard Report Format
    /// Reference: WHO Robson Classification: Implementation Manual (ISBN 978-92-4-151319-7)
    ///
    /// This report provides standardized cesarean section rate analysis using the
    /// Robson Ten-Group Classification System, enabling comparisons across facilities,
    /// regions, and time periods.
    /// </summary>
    public class RobsonClassificationReport : BaseReport
    {
        // Report Identification
        public string FacilityName { get; set; } = string.Empty;
        public string FacilityCode { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;

        // Overall Statistics
        public int TotalDeliveries { get; set; }
        public int TotalCesareanSections { get; set; }
        public decimal OverallCesareanSectionRate => TotalDeliveries > 0
            ? Math.Round((decimal)TotalCesareanSections / TotalDeliveries * 100, 2) : 0;

        // Group-wise Data (WHO Standard Table)
        public List<RobsonGroupStatistics> GroupStatistics { get; set; } = new();

        // Summary Statistics
        public int TotalClassified { get; set; }
        public int TotalUnclassified { get; set; }
        public decimal ClassificationRate => TotalDeliveries > 0
            ? Math.Round((decimal)TotalClassified / TotalDeliveries * 100, 2) : 0;

        // WHO Key Performance Indicators
        /// <summary>
        /// Contribution of Groups 1, 2, and 5 to overall CS rate
        /// These groups typically contribute 60-70% of all cesareans
        /// </summary>
        public decimal ContributionGroups125 => GroupStatistics
            .Where(g => g.Group == RobsonGroup.Group1 || g.Group == RobsonGroup.Group2 || g.Group == RobsonGroup.Group5)
            .Sum(g => g.AbsoluteContribution);

        /// <summary>
        /// CS rate in Group 1 (should be 5-10% for optimal care)
        /// </summary>
        public decimal Group1CSRate => GetGroupCSRate(RobsonGroup.Group1);

        /// <summary>
        /// CS rate in Group 2 (should be 20-35% for optimal care)
        /// </summary>
        public decimal Group2CSRate => GetGroupCSRate(RobsonGroup.Group2);

        /// <summary>
        /// CS rate in Group 3 (should be ≤3% for optimal care)
        /// </summary>
        public decimal Group3CSRate => GetGroupCSRate(RobsonGroup.Group3);

        /// <summary>
        /// CS rate in Group 5 (VBAC indicator - lower is better but depends on context)
        /// </summary>
        public decimal Group5CSRate => GetGroupCSRate(RobsonGroup.Group5);

        /// <summary>
        /// Proportion of Group 5 in total deliveries (should be <10% ideally)
        /// </summary>
        public decimal Group5Proportion => GroupStatistics
            .FirstOrDefault(g => g.Group == RobsonGroup.Group5)?.RelativeGroupSize ?? 0;

        /// <summary>
        /// Proportion of Group 1 (largest group typically 30-40%)
        /// </summary>
        public decimal Group1Proportion => GroupStatistics
            .FirstOrDefault(g => g.Group == RobsonGroup.Group1)?.RelativeGroupSize ?? 0;

        // Quality Indicators
        public RobsonQualityIndicators QualityIndicators { get; set; } = new();

        // Sub-group Analysis (Groups 2 and 5 breakdown)
        public Group2SubAnalysis Group2Analysis { get; set; } = new();
        public Group5SubAnalysis Group5Analysis { get; set; } = new();

        // Individual Cases for Audit
        public List<RobsonCaseRecord> CaseRecords { get; set; } = new();

        // Trend Data
        public List<RobsonMonthlyTrend> MonthlyTrends { get; set; } = new();

        private decimal GetGroupCSRate(RobsonGroup group)
        {
            return GroupStatistics.FirstOrDefault(g => g.Group == group)?.CSRateWithinGroup ?? 0;
        }
    }

    /// <summary>
    /// Statistics for each Robson Group - WHO Standard Table Format
    /// </summary>
    public class RobsonGroupStatistics
    {
        public RobsonGroup Group { get; set; }
        public string GroupName => $"Group {(int)Group}";
        public string GroupDescription => RobsonClassification.GetGroupDescription(Group);

        // Column 1: Number of deliveries in group
        public int TotalInGroup { get; set; }

        // Column 2: Number of CS in group
        public int CesareansInGroup { get; set; }

        // Column 3: Number of vaginal deliveries in group
        public int VaginalDeliveriesInGroup => TotalInGroup - CesareansInGroup;

        // Column 4: Relative size of group (% of all deliveries)
        public decimal RelativeGroupSize { get; set; }

        // Column 5: CS rate within group (%)
        public decimal CSRateWithinGroup => TotalInGroup > 0
            ? Math.Round((decimal)CesareansInGroup / TotalInGroup * 100, 2) : 0;

        // Column 6: Absolute contribution to overall CS rate (%)
        public decimal AbsoluteContribution { get; set; }

        // Column 7: Relative contribution to overall CS rate (%)
        public decimal RelativeContribution { get; set; }

        // Additional Analytics
        public decimal VaginalDeliveryRate => TotalInGroup > 0
            ? Math.Round((decimal)VaginalDeliveriesInGroup / TotalInGroup * 100, 2) : 0;

        // WHO Benchmarks for reference
        public decimal? WHOExpectedCSRate { get; set; }
        public decimal? WHOExpectedGroupSize { get; set; }

        // Comparison to benchmark
        public string PerformanceVsBenchmark => WHOExpectedCSRate.HasValue && WHOExpectedCSRate > 0
            ? CSRateWithinGroup > WHOExpectedCSRate.Value * 1.2m ? "Above Benchmark"
              : CSRateWithinGroup < WHOExpectedCSRate.Value * 0.8m ? "Below Benchmark"
              : "Within Range"
            : "No Benchmark";

        // Trend indicators
        public decimal PreviousPeriodCSRate { get; set; }
        public decimal CSRateChange => CSRateWithinGroup - PreviousPeriodCSRate;
        public string TrendDirection => CSRateChange > 1 ? "Increasing"
            : CSRateChange < -1 ? "Decreasing" : "Stable";
    }

    /// <summary>
    /// WHO Quality Indicators derived from Robson Classification
    /// </summary>
    public class RobsonQualityIndicators
    {
        /// <summary>
        /// Overall CS rate - WHO recommends 10-15% as optimal
        /// </summary>
        public decimal OverallCSRate { get; set; }

        /// <summary>
        /// Assessment: Is overall CS rate within WHO recommended range?
        /// </summary>
        public string OverallCSRateAssessment => OverallCSRate switch
        {
            < 10 => "Below WHO range - may indicate underuse of CS",
            <= 15 => "Within WHO optimal range (10-15%)",
            <= 20 => "Slightly above WHO range",
            <= 25 => "Moderately elevated",
            _ => "High - review indications"
        };

        /// <summary>
        /// Group 1 CS rate should be 5-10%
        /// </summary>
        public decimal Group1CSRate { get; set; }
        public bool Group1WithinBenchmark => Group1CSRate >= 5 && Group1CSRate <= 10;

        /// <summary>
        /// Group 2a (induced) CS rate - typically 25-35%
        /// </summary>
        public decimal Group2aCSRate { get; set; }

        /// <summary>
        /// Group 2b (pre-labor CS) rate - 100% by definition
        /// </summary>
        public decimal Group2bCSRate { get; set; }

        /// <summary>
        /// Group 3 CS rate should be ≤3%
        /// </summary>
        public decimal Group3CSRate { get; set; }
        public bool Group3WithinBenchmark => Group3CSRate <= 3;

        /// <summary>
        /// Group 4a (induced) CS rate - typically 15-25%
        /// </summary>
        public decimal Group4aCSRate { get; set; }

        /// <summary>
        /// Group 5 proportion of all deliveries - should be <10%
        /// Indicates previous CS burden
        /// </summary>
        public decimal Group5Proportion { get; set; }
        public bool Group5ProportionAcceptable => Group5Proportion < 10;

        /// <summary>
        /// Group 5 CS rate (VBAC indicator)
        /// </summary>
        public decimal Group5CSRate { get; set; }

        /// <summary>
        /// VBAC rate = (100 - Group5CSRate) for those with previous CS
        /// </summary>
        public decimal VBACRate => 100 - Group5CSRate;

        /// <summary>
        /// Robson Index = (Group1 + Group2 + Group5) contribution to CS rate
        /// Should typically be 60-70%
        /// </summary>
        public decimal RobsonIndex { get; set; }

        /// <summary>
        /// Data completeness score (% of deliveries successfully classified)
        /// </summary>
        public decimal DataCompletenessRate { get; set; }

        // Recommendations based on analysis
        public List<string> KeyFindings { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Group 2 Sub-analysis (2a: Induced vs 2b: Pre-labor CS)
    /// </summary>
    public class Group2SubAnalysis
    {
        // Group 2a: Nulliparous, single cephalic, ≥37 weeks, induced
        public int Group2aTotal { get; set; }
        public int Group2aCesareans { get; set; }
        public decimal Group2aCSRate => Group2aTotal > 0
            ? Math.Round((decimal)Group2aCesareans / Group2aTotal * 100, 2) : 0;

        // Group 2b: Nulliparous, single cephalic, ≥37 weeks, CS before labor
        public int Group2bTotal { get; set; }
        public int Group2bCesareans { get; set; } // Will equal Group2bTotal by definition
        public decimal Group2bCSRate => Group2bTotal > 0 ? 100 : 0;

        // Induction to CS conversion rate
        public decimal InductionToCSRate => Group2aCSRate;

        // Pre-labor CS rate (of all Group 2)
        public decimal PreLaborCSProportion => (Group2aTotal + Group2bTotal) > 0
            ? Math.Round((decimal)Group2bTotal / (Group2aTotal + Group2bTotal) * 100, 2) : 0;

        // Common indications for Group 2a CS
        public Dictionary<string, int> InductionCSIndications { get; set; } = new();

        // Common indications for Group 2b (elective CS)
        public Dictionary<string, int> ElectiveCSIndications { get; set; } = new();
    }

    /// <summary>
    /// Group 5 Sub-analysis (5.1: Spontaneous vs 5.2: Induced vs 5.3: Pre-labor CS)
    /// </summary>
    public class Group5SubAnalysis
    {
        // Group 5.1: Previous CS, single cephalic, ≥37 weeks, spontaneous labor
        public int Group51Total { get; set; }
        public int Group51Cesareans { get; set; }
        public decimal Group51CSRate => Group51Total > 0
            ? Math.Round((decimal)Group51Cesareans / Group51Total * 100, 2) : 0;
        public decimal Group51VBACRate => 100 - Group51CSRate;

        // Group 5.2: Previous CS, single cephalic, ≥37 weeks, induced
        public int Group52Total { get; set; }
        public int Group52Cesareans { get; set; }
        public decimal Group52CSRate => Group52Total > 0
            ? Math.Round((decimal)Group52Cesareans / Group52Total * 100, 2) : 0;
        public decimal Group52VBACRate => 100 - Group52CSRate;

        // Group 5.3: Previous CS, single cephalic, ≥37 weeks, CS before labor (repeat elective)
        public int Group53Total { get; set; }
        public int Group53Cesareans { get; set; } // Will equal Group53Total
        public decimal Group53CSRate => Group53Total > 0 ? 100 : 0;

        // VBAC attempt rate (those who entered labor)
        public decimal VBACAttemptRate => (Group51Total + Group52Total + Group53Total) > 0
            ? Math.Round((decimal)(Group51Total + Group52Total) / (Group51Total + Group52Total + Group53Total) * 100, 2) : 0;

        // VBAC success rate (among those who attempted)
        public int VaginalDeliveries => (Group51Total - Group51Cesareans) + (Group52Total - Group52Cesareans);
        public int VBACAttempts => Group51Total + Group52Total;
        public decimal VBACSuccessRate => VBACAttempts > 0
            ? Math.Round((decimal)VaginalDeliveries / VBACAttempts * 100, 2) : 0;

        // Repeat CS rate
        public decimal RepeatElectiveCSProportion => (Group51Total + Group52Total + Group53Total) > 0
            ? Math.Round((decimal)Group53Total / (Group51Total + Group52Total + Group53Total) * 100, 2) : 0;

        // Previous CS count distribution
        public int OnePreviousCS { get; set; }
        public int TwoPreviousCS { get; set; }
        public int ThreeOrMorePreviousCS { get; set; }
    }

    /// <summary>
    /// Individual case record for audit purposes
    /// </summary>
    public class RobsonCaseRecord
    {
        public Guid PartographID { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string HospitalNumber { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public int Age { get; set; }
        public int Parity { get; set; }
        public int GestationalAgeWeeks { get; set; }
        public bool HasPreviousCS { get; set; }
        public int NumberOfPreviousCS { get; set; }
        public LaborOnsetType LaborOnset { get; set; }
        public FetalPresentationType FetalPresentation { get; set; }
        public int NumberOfFetuses { get; set; }
        public RobsonGroup Group { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public bool IsCesarean => DeliveryMode == DeliveryMode.CaesareanSection;
        public string CSIndication { get; set; } = string.Empty;
        public CesareanType? CSType { get; set; }
        public string AttendingStaff { get; set; } = string.Empty;
        public string ClassificationSummary { get; set; } = string.Empty;
        public BabyVitalStatus? BabyOutcome { get; set; }
        public MaternalOutcomeStatus? MaternalOutcome { get; set; }
    }

    /// <summary>
    /// Monthly trend data for Robson analysis
    /// </summary>
    public class RobsonMonthlyTrend
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Period => $"{Year}-{Month:D2}";
        public int TotalDeliveries { get; set; }
        public int TotalCS { get; set; }
        public decimal OverallCSRate => TotalDeliveries > 0
            ? Math.Round((decimal)TotalCS / TotalDeliveries * 100, 2) : 0;

        // Group-wise CS rates
        public decimal Group1CSRate { get; set; }
        public decimal Group2CSRate { get; set; }
        public decimal Group3CSRate { get; set; }
        public decimal Group4CSRate { get; set; }
        public decimal Group5CSRate { get; set; }

        // Group-wise sizes
        public int Group1Size { get; set; }
        public int Group2Size { get; set; }
        public int Group3Size { get; set; }
        public int Group4Size { get; set; }
        public int Group5Size { get; set; }

        // Key metrics
        public decimal VBACRate { get; set; }
        public decimal Group5Proportion => TotalDeliveries > 0
            ? Math.Round((decimal)Group5Size / TotalDeliveries * 100, 2) : 0;
    }

    /// <summary>
    /// Comparative Robson Report for benchmarking
    /// </summary>
    public class RobsonComparativeReport : BaseReport
    {
        public string ComparisonType { get; set; } = string.Empty; // "Facilities", "Regions", "Time Periods"

        public List<FacilityRobsonSummary> FacilityComparisons { get; set; } = new();
        public List<RegionalRobsonSummary> RegionalComparisons { get; set; } = new();
        public List<TemporalRobsonComparison> TemporalComparisons { get; set; } = new();

        // National/Regional benchmarks
        public decimal NationalCSRate { get; set; }
        public decimal RegionalCSRate { get; set; }
        public decimal WHORecommendedCSRate { get; set; } = 15; // WHO recommends 10-15%

        // Statistical analysis
        public decimal MeanCSRate { get; set; }
        public decimal MedianCSRate { get; set; }
        public decimal StandardDeviation { get; set; }
        public decimal InterquartileRange { get; set; }

        // Outlier identification
        public List<string> HighCSRateFacilities { get; set; } = new();
        public List<string> LowCSRateFacilities { get; set; } = new();
    }

    /// <summary>
    /// Facility-level Robson summary for comparative analysis
    /// </summary>
    public class FacilityRobsonSummary
    {
        public string FacilityName { get; set; } = string.Empty;
        public string FacilityCode { get; set; } = string.Empty;
        public string FacilityType { get; set; } = string.Empty; // Hospital, Health Centre, etc.
        public string Region { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;

        public int TotalDeliveries { get; set; }
        public int TotalCS { get; set; }
        public decimal OverallCSRate => TotalDeliveries > 0
            ? Math.Round((decimal)TotalCS / TotalDeliveries * 100, 2) : 0;

        // Key Robson metrics
        public decimal Group1CSRate { get; set; }
        public decimal Group3CSRate { get; set; }
        public decimal Group5Proportion { get; set; }
        public decimal Group5CSRate { get; set; }
        public decimal VBACRate => 100 - Group5CSRate;

        // Contribution to overall CS
        public decimal Groups125Contribution { get; set; }

        // Performance rating
        public string PerformanceCategory { get; set; } = string.Empty; // "Optimal", "Acceptable", "Needs Review"
    }

    /// <summary>
    /// Regional Robson summary
    /// </summary>
    public class RegionalRobsonSummary
    {
        public string Region { get; set; } = string.Empty;
        public int NumberOfFacilities { get; set; }
        public int TotalDeliveries { get; set; }
        public int TotalCS { get; set; }
        public decimal OverallCSRate => TotalDeliveries > 0
            ? Math.Round((decimal)TotalCS / TotalDeliveries * 100, 2) : 0;

        // Key Robson metrics
        public decimal Group1CSRate { get; set; }
        public decimal Group3CSRate { get; set; }
        public decimal Group5Proportion { get; set; }
        public decimal VBACRate { get; set; }

        // Variation metrics
        public decimal LowestFacilityCSRate { get; set; }
        public decimal HighestFacilityCSRate { get; set; }
        public decimal CSRateVariation => HighestFacilityCSRate - LowestFacilityCSRate;
    }

    /// <summary>
    /// Temporal comparison for trend analysis
    /// </summary>
    public class TemporalRobsonComparison
    {
        public string Period1 { get; set; } = string.Empty; // e.g., "2023-Q1"
        public string Period2 { get; set; } = string.Empty; // e.g., "2024-Q1"

        public int Period1Deliveries { get; set; }
        public int Period2Deliveries { get; set; }

        public decimal Period1CSRate { get; set; }
        public decimal Period2CSRate { get; set; }
        public decimal CSRateChange => Period2CSRate - Period1CSRate;
        public decimal CSRateChangePercent => Period1CSRate > 0
            ? Math.Round(CSRateChange / Period1CSRate * 100, 2) : 0;

        // Group-wise changes
        public decimal Group1CSRateChange { get; set; }
        public decimal Group3CSRateChange { get; set; }
        public decimal Group5ProportionChange { get; set; }
        public decimal VBACRateChange { get; set; }

        // Statistical significance
        public bool IsStatisticallySignificant { get; set; }
        public decimal PValue { get; set; }
    }

    /// <summary>
    /// Dashboard summary for Robson monitoring
    /// </summary>
    public class RobsonDashboardSummary
    {
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public string Period { get; set; } = string.Empty;

        // Key Metrics (KPIs)
        public decimal OverallCSRate { get; set; }
        public decimal OverallCSRateTrend { get; set; }
        public string OverallCSRateTrendDirection { get; set; } = string.Empty;

        public decimal Group1CSRate { get; set; }
        public decimal Group3CSRate { get; set; }
        public decimal Group5CSRate { get; set; }
        public decimal VBACRate { get; set; }

        public decimal Group5Proportion { get; set; }

        // Alert indicators
        public bool IsGroup1CSRateHigh => Group1CSRate > 10;
        public bool IsGroup3CSRateHigh => Group3CSRate > 3;
        public bool IsGroup5ProportionHigh => Group5Proportion > 10;
        public bool IsOverallCSRateAboveWHO => OverallCSRate > 15;

        // Distribution summary
        public int TotalDeliveries { get; set; }
        public int TotalCesareans { get; set; }
        public int TotalVaginalDeliveries => TotalDeliveries - TotalCesareans;

        // Group distribution (for pie chart)
        public List<RobsonGroupDistribution> GroupDistribution { get; set; } = new();

        // Top contributors to CS rate
        public List<RobsonGroupStatistics> TopCSContributors { get; set; } = new();

        // Recent trend data
        public List<RobsonMonthlyTrend> RecentTrends { get; set; } = new();

        // Action items
        public List<RobsonActionItem> ActionItems { get; set; } = new();
    }

    /// <summary>
    /// Group distribution for visualization
    /// </summary>
    public class RobsonGroupDistribution
    {
        public RobsonGroup Group { get; set; }
        public string GroupName => $"Group {(int)Group}";
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    /// <summary>
    /// Action items generated from Robson analysis
    /// </summary>
    public class RobsonActionItem
    {
        public string Category { get; set; } = string.Empty; // "Clinical", "Audit", "Training"
        public string Priority { get; set; } = string.Empty; // "High", "Medium", "Low"
        public string Description { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public RobsonGroup? RelatedGroup { get; set; }
        public DateTime IdentifiedDate { get; set; } = DateTime.Now;
        public bool IsResolved { get; set; }
    }
}
