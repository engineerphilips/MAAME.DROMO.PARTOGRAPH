namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Models
{
    #region Live Labor Board Models

    /// <summary>
    /// Real-time active labor case for live labor board
    /// </summary>
    public class LiveLaborCase
    {
        public Guid PartographId { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string HospitalNumber { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gravida { get; set; } = string.Empty;
        public string Parity { get; set; } = string.Empty;

        // Facility info
        public Guid FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;

        // Labor progress
        public DateTime AdmissionTime { get; set; }
        public DateTime? LaborStartTime { get; set; }
        public TimeSpan LaborDuration => LaborStartTime.HasValue ? DateTime.UtcNow - LaborStartTime.Value : TimeSpan.Zero;
        public int CurrentDilatation { get; set; }
        public int CurrentStation { get; set; }
        public string LaborStage { get; set; } = "First Stage"; // First Stage, Second Stage, Third Stage, Fourth Stage

        // Latest vitals
        public int? LatestFHR { get; set; }
        public DateTime? LatestFHRTime { get; set; }
        public int? LatestSystolicBP { get; set; }
        public int? LatestDiastolicBP { get; set; }
        public DateTime? LatestBPTime { get; set; }
        public decimal? LatestTemperature { get; set; }
        public int? ContractionsPerTenMinutes { get; set; }

        // Risk assessment
        public string RiskLevel { get; set; } = "Normal"; // Normal, Moderate, High, Critical
        public List<string> ActiveAlerts { get; set; } = new();
        public List<ClinicalAlert> ClinicalAlerts { get; set; } = new(); // Smart Alert Objects
        public int AlertCount { get; set; }
        public int CriticalAlertCount { get; set; }

        // Staff assignment
        public string AssignedMidwife { get; set; } = string.Empty;
        public DateTime? LastAssessmentTime { get; set; }

        // Measurement due status
        public bool IsFHRDue { get; set; }
        public bool IsBPDue { get; set; }
        public bool IsDilatationDue { get; set; }
        public int MinutesUntilNextMeasurement { get; set; }
    }

    /// <summary>
    /// Summary for live labor board header
    /// </summary>
    public class LiveLaborSummary
    {
        public int TotalActiveCases { get; set; }
        public int CriticalCases { get; set; }
        public int HighRiskCases { get; set; }
        public int ModerateRiskCases { get; set; }
        public int NormalCases { get; set; }
        public int MeasurementsDue { get; set; }
        public int UnacknowledgedAlerts { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    #endregion

    #region Alert Acknowledgment Models

    /// <summary>
    /// Enhanced alert with acknowledgment tracking
    /// </summary>
    public class EnhancedAlert
    {
        public Guid Id { get; set; }
        public Guid PartographId { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;

        // Alert details
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Info"; // Info, Warning, Critical
        public string Category { get; set; } = string.Empty; // Fetal, Maternal, Labor, Hydration
        public string AlertType { get; set; } = string.Empty; // FHR_LOW, BP_HIGH, PROLONGED_LABOR, etc.

        // Timing
        public DateTime CreatedAt { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // Acknowledgment tracking
        public bool IsAcknowledged { get; set; }
        public string? AcknowledgedBy { get; set; }
        public string? AcknowledgmentNotes { get; set; }
        public double? ResponseTimeMinutes => IsAcknowledged && AcknowledgedAt.HasValue
            ? (AcknowledgedAt.Value - CreatedAt).TotalMinutes
            : null;

        // Resolution
        public bool IsResolved { get; set; }
        public string? ResolvedBy { get; set; }
        public string? ResolutionNotes { get; set; }
        public string? ActionTaken { get; set; }

        // Escalation
        public bool IsEscalated { get; set; }
        public DateTime? EscalatedAt { get; set; }
        public string? EscalatedTo { get; set; }
        public int EscalationLevel { get; set; } // 0 = no escalation, 1 = supervisor, 2 = district, 3 = regional
    }

    /// <summary>
    /// Alert acknowledgment request
    /// </summary>
    public class AlertAcknowledgmentRequest
    {
        public Guid AlertId { get; set; }
        public string AcknowledgedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Alert resolution request
    /// </summary>
    public class AlertResolutionRequest
    {
        public Guid AlertId { get; set; }
        public string ResolvedBy { get; set; } = string.Empty;
        public string ActionTaken { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Alert response time statistics
    /// </summary>
    public class AlertResponseMetrics
    {
        public string Period { get; set; } = string.Empty; // Today, This Week, This Month
        public int TotalAlerts { get; set; }
        public int AcknowledgedAlerts { get; set; }
        public int ResolvedAlerts { get; set; }
        public int PendingAlerts { get; set; }
        public int EscalatedAlerts { get; set; }

        // Response times
        public double AverageResponseTimeMinutes { get; set; }
        public double MedianResponseTimeMinutes { get; set; }
        public double P95ResponseTimeMinutes { get; set; }
        public double MinResponseTimeMinutes { get; set; }
        public double MaxResponseTimeMinutes { get; set; }

        // By severity
        public double CriticalAvgResponseTimeMinutes { get; set; }
        public double WarningAvgResponseTimeMinutes { get; set; }

        // Target compliance (e.g., Critical within 5 mins, Warning within 15 mins)
        public double CriticalResponseCompliance { get; set; } // % within target
        public double WarningResponseCompliance { get; set; }
        public double OverallResponseCompliance { get; set; }
    }

    #endregion

    #region Predictive Analytics Models

    /// <summary>
    /// Risk prediction for a patient
    /// </summary>
    public class RiskPrediction
    {
        public Guid PatientId { get; set; }
        public Guid PartographId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;

        // Overall risk score (0-100)
        public double OverallRiskScore { get; set; }
        public string RiskCategory { get; set; } = "Low"; // Low, Moderate, High, Critical

        // Individual risk scores
        public double CaesareanRisk { get; set; }
        public double HemorrhageRisk { get; set; }
        public double ProlongedLaborRisk { get; set; }
        public double FetalDistressRisk { get; set; }
        public double PreeclampsiaRisk { get; set; }
        public double InfectionRisk { get; set; }

        // Contributing factors
        public List<RiskFactor> RiskFactors { get; set; } = new();

        // Recommendations
        public List<string> Recommendations { get; set; } = new();

        // Prediction metadata
        public DateTime PredictionTime { get; set; }
        public string ModelVersion { get; set; } = "1.0";
        public double ConfidenceScore { get; set; }
    }

    /// <summary>
    /// Individual risk factor
    /// </summary>
    public class RiskFactor
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Contribution { get; set; } // How much this contributes to overall risk
        public string Value { get; set; } = string.Empty; // Current value
        public string Threshold { get; set; } = string.Empty; // Normal threshold
        public string Severity { get; set; } = "Low"; // Low, Medium, High
    }

    /// <summary>
    /// Facility-level risk summary
    /// </summary>
    public class FacilityRiskSummary
    {
        public Guid FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public int TotalActiveCases { get; set; }
        public int HighRiskCases { get; set; }
        public int ModerateRiskCases { get; set; }
        public int LowRiskCases { get; set; }
        public double AverageRiskScore { get; set; }
        public List<RiskPrediction> HighRiskPatients { get; set; } = new();
    }

    #endregion

    #region Data Quality Models

    /// <summary>
    /// Data quality score for a facility
    /// </summary>
    public class DataQualityScore
    {
        public Guid? FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;

        // Overall score (0-100)
        public double OverallScore { get; set; }
        public string Grade { get; set; } = "B"; // A, B, C, D, F

        // Component scores
        public double CompletenessScore { get; set; } // % of required fields filled
        public double TimelinessScore { get; set; } // Measurements taken on time
        public double AccuracyScore { get; set; } // Values within valid ranges
        public double ConsistencyScore { get; set; } // Data patterns make sense

        // Detailed metrics
        public int TotalPartographs { get; set; }
        public int CompletePartographs { get; set; }
        public int PartographsWithMissingData { get; set; }
        public int LateMeasurements { get; set; }
        public int OutOfRangeValues { get; set; }
        public int InconsistentRecords { get; set; }

        // Gap analysis
        public List<DataGap> IdentifiedGaps { get; set; } = new();

        // Trend
        public double PreviousPeriodScore { get; set; }
        public double ScoreChange { get; set; }
        public string Trend { get; set; } = "Stable"; // Improving, Stable, Declining
    }

    /// <summary>
    /// Identified data gap
    /// </summary>
    public class DataGap
    {
        public string GapType { get; set; } = string.Empty; // MissingFHR, LateBP, IncompleteOutcome
        public string Description { get; set; } = string.Empty;
        public int AffectedRecords { get; set; }
        public string Severity { get; set; } = "Low"; // Low, Medium, High
        public string Recommendation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Field-level completeness report
    /// </summary>
    public class FieldCompletenessReport
    {
        public string FieldName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
        public int FilledRecords { get; set; }
        public double CompletenessPercent => TotalRecords > 0 ? (double)FilledRecords / TotalRecords * 100 : 0;
        public bool IsRequired { get; set; }
        public string Status { get; set; } = "Good"; // Good, Warning, Critical
    }

    #endregion

    #region Comparative Benchmarking Models

    /// <summary>
    /// Benchmark comparison data
    /// </summary>
    public class BenchmarkComparison
    {
        public string EntityName { get; set; } = string.Empty; // Facility, District, or Region name
        public string EntityType { get; set; } = "Facility"; // Facility, District, Region
        public Guid EntityId { get; set; }

        // Current metrics
        public double CaesareanRate { get; set; }
        public double ComplicationRate { get; set; }
        public double ReferralRate { get; set; }
        public double StillbirthRate { get; set; }
        public double NeonatalMortalityRate { get; set; }
        public double MaternalMortalityRatio { get; set; }
        public double AverageAlertResponseTime { get; set; }
        public double DataQualityScore { get; set; }

        // Benchmark values (national/regional averages)
        public double BenchmarkCaesareanRate { get; set; }
        public double BenchmarkComplicationRate { get; set; }
        public double BenchmarkReferralRate { get; set; }
        public double BenchmarkStillbirthRate { get; set; }
        public double BenchmarkNeonatalMortalityRate { get; set; }
        public double BenchmarkMaternalMortalityRatio { get; set; }
        public double BenchmarkAlertResponseTime { get; set; }
        public double BenchmarkDataQualityScore { get; set; }

        // WHO targets
        public double WHOTargetCaesareanRate { get; set; } = 15.0; // 10-15% recommended
        public double WHOTargetStillbirthRate { get; set; } = 12.0; // per 1000 births

        // Performance indicators
        public string OverallPerformance { get; set; } = "Average"; // Excellent, Good, Average, Below Average, Poor
        public List<PerformanceIndicator> Indicators { get; set; } = new();

        // Ranking
        public int Rank { get; set; }
        public int TotalEntities { get; set; }
        public int Percentile { get; set; }
    }

    /// <summary>
    /// Individual performance indicator
    /// </summary>
    public class PerformanceIndicator
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public double Benchmark { get; set; }
        public double Variance { get; set; } // Value - Benchmark
        public double VariancePercent { get; set; }
        public string Status { get; set; } = "OnTarget"; // AboveTarget, OnTarget, BelowTarget
        public string Trend { get; set; } = "Stable"; // Improving, Stable, Declining
    }

    /// <summary>
    /// Facility ranking entry
    /// </summary>
    public class FacilityRanking
    {
        public Guid FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public string FacilityType { get; set; } = string.Empty;

        public int OverallRank { get; set; }
        public double OverallScore { get; set; }
        public string PerformanceGrade { get; set; } = "B";

        public double CaesareanRate { get; set; }
        public double ComplicationRate { get; set; }
        public double DataQualityScore { get; set; }
        public double AlertResponseScore { get; set; }

        public int TotalDeliveries { get; set; }
        public int RankChange { get; set; } // vs previous period
    }

    #endregion

    #region Alert Threshold Configuration Models

    /// <summary>
    /// Configurable alert thresholds for a facility
    /// </summary>
    public class AlertThresholdConfiguration
    {
        public Guid Id { get; set; }
        public Guid? FacilityId { get; set; } // null = global defaults
        public string FacilityName { get; set; } = string.Empty;
        public bool UseCustomThresholds { get; set; }

        // FHR thresholds
        public int FHRCriticalLow { get; set; } = 100;
        public int FHRWarningLow { get; set; } = 110;
        public int FHRWarningHigh { get; set; } = 160;
        public int FHRCriticalHigh { get; set; } = 180;

        // BP thresholds
        public int SystolicWarning { get; set; } = 140;
        public int SystolicCritical { get; set; } = 160;
        public int DiastolicWarning { get; set; } = 90;
        public int DiastolicCritical { get; set; } = 110;

        // Temperature thresholds
        public decimal TemperatureWarning { get; set; } = 38.0m;
        public decimal TemperatureCritical { get; set; } = 38.5m;

        // Contraction thresholds
        public int TachysystoleThreshold { get; set; } = 5; // contractions per 10 min

        // Labor duration thresholds (hours)
        public int FirstStageDurationWarning { get; set; } = 12;
        public int FirstStageDurationCritical { get; set; } = 18;
        public int SecondStageDurationWarning { get; set; } = 2;
        public int SecondStageDurationCritical { get; set; } = 3;

        // Measurement intervals (minutes)
        public int FHRMeasurementInterval { get; set; } = 30;
        public int BPMeasurementInterval { get; set; } = 60;
        public int DilatationMeasurementInterval { get; set; } = 240; // 4 hours

        // Alert escalation settings
        public int EscalationTimeoutMinutes { get; set; } = 15;
        public bool AutoEscalateUnacknowledged { get; set; } = true;

        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
    }

    #endregion

    #region Push Notification Models

    /// <summary>
    /// Push notification subscription
    /// </summary>
    public class NotificationSubscription
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        // Subscription preferences
        public bool EmailEnabled { get; set; } = true;
        public bool SmsEnabled { get; set; }
        public bool PushEnabled { get; set; } = true;
        public bool InAppEnabled { get; set; } = true;

        // Alert level preferences
        public bool CriticalAlerts { get; set; } = true;
        public bool WarningAlerts { get; set; } = true;
        public bool InfoAlerts { get; set; }

        // Category preferences
        public bool FetalAlerts { get; set; } = true;
        public bool MaternalAlerts { get; set; } = true;
        public bool LaborAlerts { get; set; } = true;
        public bool SystemAlerts { get; set; }

        // Scope
        public Guid? RegionId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? FacilityId { get; set; }

        // Quiet hours
        public bool QuietHoursEnabled { get; set; }
        public TimeOnly QuietHoursStart { get; set; } = new TimeOnly(22, 0);
        public TimeOnly QuietHoursEnd { get; set; } = new TimeOnly(6, 0);

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Notification to be sent
    /// </summary>
    public class NotificationMessage
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Severity { get; set; } = "Info";
        public string Category { get; set; } = string.Empty;

        // Related entities
        public Guid? AlertId { get; set; }
        public Guid? PartographId { get; set; }
        public Guid? FacilityId { get; set; }

        // Action
        public string? ActionUrl { get; set; }
        public string? ActionLabel { get; set; }

        // Status
        public DateTime CreatedAt { get; set; }
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    #endregion

    #region Offline Alert Queue Models

    /// <summary>
    /// Queued alert for offline scenarios
    /// </summary>
    public class QueuedAlert
    {
        public Guid Id { get; set; }
        public EnhancedAlert Alert { get; set; } = new();
        public DateTime QueuedAt { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? SyncedAt { get; set; }
        public int SyncAttempts { get; set; }
        public string? LastSyncError { get; set; }
    }

    /// <summary>
    /// Offline queue status
    /// </summary>
    public class OfflineQueueStatus
    {
        public bool IsOnline { get; set; }
        public int PendingAlerts { get; set; }
        public int PendingAcknowledgments { get; set; }
        public DateTime? LastSyncTime { get; set; }
        public DateTime? NextSyncAttempt { get; set; }
        public List<string> SyncErrors { get; set; } = new();
    }

    #endregion

    #region Report Visualization Models

    /// <summary>
    /// Chart data point
    /// </summary>
    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
        public string? Color { get; set; }
        public string? Category { get; set; }
    }

    /// <summary>
    /// Time series data for charts
    /// </summary>
    public class TimeSeriesData
    {
        public string SeriesName { get; set; } = string.Empty;
        public string Color { get; set; } = "#007bff";
        public List<TimeSeriesPoint> DataPoints { get; set; } = new();
    }

    public class TimeSeriesPoint
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }

    /// <summary>
    /// Report visualization data
    /// </summary>
    public class ReportVisualization
    {
        public string ReportType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public DashboardFilter? Filter { get; set; }

        // Summary statistics
        public Dictionary<string, object> SummaryStats { get; set; } = new();

        // Chart data
        public List<ChartDataPoint> PieChartData { get; set; } = new();
        public List<ChartDataPoint> BarChartData { get; set; } = new();
        public List<TimeSeriesData> LineChartData { get; set; } = new();

        // Table data
        public List<Dictionary<string, object>> TableData { get; set; } = new();
        public List<string> TableHeaders { get; set; } = new();
    }

    #endregion
}
