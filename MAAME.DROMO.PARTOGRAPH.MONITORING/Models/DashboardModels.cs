namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Models
{
    /// <summary>
    /// Summary statistics for dashboard display
    /// </summary>
    public class DashboardSummary
    {
        public int TotalRegions { get; set; }
        public int TotalDistricts { get; set; }
        public int TotalFacilities { get; set; }
        public int ActiveFacilities { get; set; }

        // Delivery statistics
        public int TotalDeliveriesToday { get; set; }
        public int TotalDeliveriesThisMonth { get; set; }
        public int TotalDeliveriesThisYear { get; set; }

        // Active labor monitoring
        public int ActiveLabors { get; set; }
        public int HighRiskLabors { get; set; }

        // Outcomes
        public int NormalDeliveries { get; set; }
        public int CaesareanSections { get; set; }
        public int AssistedDeliveries { get; set; }

        // Complications
        public int ComplicationsToday { get; set; }
        public int ReferralsToday { get; set; }

        // Mortality (This month)
        public int MaternalDeaths { get; set; }
        public int NeonatalDeaths { get; set; }
        public int Stillbirths { get; set; }

        // Trend data for KPI cards (7-day trend)
        public List<int> DeliveryTrend { get; set; } = new();
        public List<int> LaborTrend { get; set; } = new();
        public List<int> ComplicationTrend { get; set; } = new();
        public List<int> FacilityActivityTrend { get; set; } = new();

        // Percentage changes vs previous period
        public double DeliveryChangePercent { get; set; }
        public double LaborChangePercent { get; set; }
        public double ComplicationChangePercent { get; set; }
        public double FacilityActivityChangePercent { get; set; }

        // Previous period values for comparison
        public int PreviousPeriodDeliveries { get; set; }
        public int PreviousPeriodComplications { get; set; }
    }

    /// <summary>
    /// Region summary for national dashboard
    /// </summary>
    public class RegionSummary
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int DistrictCount { get; set; }
        public int FacilityCount { get; set; }
        public int DeliveriesToday { get; set; }
        public int DeliveriesThisMonth { get; set; }
        public int ActiveLabors { get; set; }
        public int Complications { get; set; }
        public int Referrals { get; set; }
        public double CaesareanRate { get; set; }
        public string PerformanceStatus { get; set; } = "Normal"; // Normal, Warning, Critical
    }

    /// <summary>
    /// District summary for regional dashboard
    /// </summary>
    public class DistrictSummary
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public int FacilityCount { get; set; }
        public int ActiveFacilities { get; set; }
        public int DeliveriesToday { get; set; }
        public int DeliveriesThisMonth { get; set; }
        public int ActiveLabors { get; set; }
        public int Complications { get; set; }
        public int Referrals { get; set; }
        public double CaesareanRate { get; set; }
        public string PerformanceStatus { get; set; } = "Normal";
    }

    /// <summary>
    /// Facility summary for district dashboard
    /// </summary>
    public class FacilitySummary
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int DeliveriesToday { get; set; }
        public int DeliveriesThisMonth { get; set; }
        public int ActiveLabors { get; set; }
        public int StaffCount { get; set; }
        public DateTime? LastActivityTime { get; set; }
        public string PerformanceStatus { get; set; } = "Normal";
    }

    /// <summary>
    /// Trend data for charts
    /// </summary>
    public class TrendData
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Delivery mode distribution
    /// </summary>
    public class DeliveryModeDistribution
    {
        public int NormalVaginal { get; set; }
        public int AssistedVaginal { get; set; }
        public int ElectiveCaesarean { get; set; }
        public int EmergencyCaesarean { get; set; }
        public int Total => NormalVaginal + AssistedVaginal + ElectiveCaesarean + EmergencyCaesarean;

        public double NormalVaginalPercent => Total > 0 ? (double)NormalVaginal / Total * 100 : 0;
        public double CaesareanPercent => Total > 0 ? (double)(ElectiveCaesarean + EmergencyCaesarean) / Total * 100 : 0;
    }

    /// <summary>
    /// Alert for dashboard
    /// </summary>
    public class DashboardAlert
    {
        public Guid ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Info"; // Info, Warning, Critical
        public string Category { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }

    /// <summary>
    /// Filter options for data queries
    /// </summary>
    public class DashboardFilter
    {
        public Guid? RegionID { get; set; }
        public Guid? DistrictID { get; set; }
        public Guid? FacilityID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Period { get; set; } // Today, Week, Month, Year, Custom
    }

    /// <summary>
    /// Maternal health indicators with WHO targets
    /// </summary>
    public class MaternalHealthIndicators
    {
        // Mortality indicators (per 100,000 live births for MMR, per 1,000 for others)
        public double MaternalMortalityRatio { get; set; }
        public double NeonatalMortalityRate { get; set; }
        public double StillbirthRate { get; set; }
        public double PerinatalMortalityRate { get; set; }

        // Absolute numbers
        public int TotalLiveBirths { get; set; }
        public int TotalDeliveries { get; set; }
        public int MaternalDeaths { get; set; }
        public int NeonatalDeaths { get; set; }
        public int Stillbirths { get; set; }
        public int EarlyNeonatalDeaths { get; set; }

        // Key indicators
        public double CaesareanRate { get; set; }
        public double AssistedDeliveryRate { get; set; }
        public double ComplicationRate { get; set; }
        public double ReferralCompletionRate { get; set; }
        public double SkilledBirthAttendanceRate { get; set; }

        // WHO Targets
        public double WHOTargetMMR { get; set; } = 70.0; // SDG target: <70 per 100,000
        public double WHOTargetNMR { get; set; } = 12.0; // SDG target: <12 per 1,000
        public double WHOTargetStillbirthRate { get; set; } = 12.0; // per 1,000
        public double WHOTargetCaesareanRateLow { get; set; } = 10.0; // 10-15% optimal
        public double WHOTargetCaesareanRateHigh { get; set; } = 15.0;

        // Trend vs previous period
        public double MMRChangePercent { get; set; }
        public double NMRChangePercent { get; set; }
        public double StillbirthRateChangePercent { get; set; }
        public double CaesareanRateChangePercent { get; set; }

        // Status indicators
        public string MMRStatus => MaternalMortalityRatio <= WHOTargetMMR ? "OnTarget" : "AboveTarget";
        public string NMRStatus => NeonatalMortalityRate <= WHOTargetNMR ? "OnTarget" : "AboveTarget";
        public string CaesareanStatus => CaesareanRate >= WHOTargetCaesareanRateLow && CaesareanRate <= WHOTargetCaesareanRateHigh ? "OnTarget" : "OffTarget";
    }

    /// <summary>
    /// Alert summary for dashboard
    /// </summary>
    public class AlertSummary
    {
        public int TotalActiveAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public int WarningAlerts { get; set; }
        public int InfoAlerts { get; set; }
        public int UnacknowledgedAlerts { get; set; }
        public int EscalatedAlerts { get; set; }
        public double AverageResponseTimeMinutes { get; set; }

        // By category
        public int FetalAlerts { get; set; }
        public int MaternalAlerts { get; set; }
        public int LaborAlerts { get; set; }

        // Response metrics
        public double ResponseCompliancePercent { get; set; }
        public int AlertsRespondedWithinTarget { get; set; }
    }

    /// <summary>
    /// Facility performance for scorecard
    /// </summary>
    public class FacilityPerformanceSummary
    {
        public Guid FacilityID { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public string FacilityType { get; set; } = string.Empty;

        // Key metrics
        public int TotalDeliveries { get; set; }
        public double CaesareanRate { get; set; }
        public double ComplicationRate { get; set; }
        public double ReferralRate { get; set; }
        public double DataQualityScore { get; set; }
        public double AlertResponseScore { get; set; }

        // Overall performance
        public double OverallScore { get; set; }
        public string PerformanceGrade { get; set; } = "B"; // A, B, C, D, F
        public int Rank { get; set; }
        public int RankChange { get; set; } // vs previous period
        public string PerformanceStatus { get; set; } = "Normal"; // Excellent, Good, Normal, Needs Improvement, Critical

        // Activity
        public int ActiveLabors { get; set; }
        public DateTime? LastActivityTime { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Regional/District performance for visualization
    /// </summary>
    public class GeographicPerformance
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Region"; // Region or District
        public string Code { get; set; } = string.Empty;

        // Key metrics
        public int TotalFacilities { get; set; }
        public int ActiveFacilities { get; set; }
        public int TotalDeliveries { get; set; }
        public int DeliveriesToday { get; set; }
        public int ActiveLabors { get; set; }
        public int HighRiskCases { get; set; }
        public int Complications { get; set; }
        public int Referrals { get; set; }

        // Rates
        public double CaesareanRate { get; set; }
        public double ComplicationRate { get; set; }

        // Performance
        public string PerformanceStatus { get; set; } = "Normal";
        public double PerformanceScore { get; set; }

        // For map visualization
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    /// <summary>
    /// Live labor summary for dashboard widget
    /// </summary>
    public class DashboardLiveLaborSummary
    {
        public int TotalActiveCases { get; set; }
        public int CriticalCases { get; set; }
        public int HighRiskCases { get; set; }
        public int ModerateRiskCases { get; set; }
        public int NormalCases { get; set; }
        public int MeasurementsDue { get; set; }
        public int UnacknowledgedAlerts { get; set; }
        public List<LiveLaborCaseBrief> TopCriticalCases { get; set; } = new();
    }

    /// <summary>
    /// Brief labor case info for dashboard widget
    /// </summary>
    public class LiveLaborCaseBrief
    {
        public Guid PartographId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = "Normal";
        public int? LatestFHR { get; set; }
        public int CurrentDilatation { get; set; }
        public string LaborStage { get; set; } = string.Empty;
        public int AlertCount { get; set; }
        public TimeSpan LaborDuration { get; set; }
    }

    /// <summary>
    /// Time period comparison data
    /// </summary>
    public class PeriodComparison
    {
        public string PeriodLabel { get; set; } = string.Empty; // e.g., "This Month", "Last Month"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Current period
        public int CurrentDeliveries { get; set; }
        public int CurrentComplications { get; set; }
        public int CurrentReferrals { get; set; }
        public int CurrentMaternalDeaths { get; set; }
        public int CurrentNeonatalDeaths { get; set; }

        // Previous period
        public int PreviousDeliveries { get; set; }
        public int PreviousComplications { get; set; }
        public int PreviousReferrals { get; set; }
        public int PreviousMaternalDeaths { get; set; }
        public int PreviousNeonatalDeaths { get; set; }

        // Change percentages
        public double DeliveryChange => PreviousDeliveries > 0 ? ((double)(CurrentDeliveries - PreviousDeliveries) / PreviousDeliveries) * 100 : 0;
        public double ComplicationChange => PreviousComplications > 0 ? ((double)(CurrentComplications - PreviousComplications) / PreviousComplications) * 100 : 0;
        public double ReferralChange => PreviousReferrals > 0 ? ((double)(CurrentReferrals - PreviousReferrals) / PreviousReferrals) * 100 : 0;
    }
}
