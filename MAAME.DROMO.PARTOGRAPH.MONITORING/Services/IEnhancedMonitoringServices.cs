using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    /// <summary>
    /// Service for real-time live labor board
    /// </summary>
    public interface ILiveLaborService
    {
        Task<LiveLaborSummary> GetLiveLaborSummaryAsync(DashboardFilter? filter = null);
        Task<List<LiveLaborCase>> GetActiveLaborCasesAsync(DashboardFilter? filter = null);
        Task<LiveLaborCase?> GetLaborCaseAsync(Guid partographId);
        Task<List<LiveLaborCase>> GetCriticalCasesAsync(DashboardFilter? filter = null);
        Task<List<LiveLaborCase>> GetMeasurementsDueCasesAsync(DashboardFilter? filter = null);
    }

    /// <summary>
    /// Service for enhanced alert management with acknowledgment tracking
    /// </summary>
    public interface IEnhancedAlertService
    {
        Task<List<EnhancedAlert>> GetActiveAlertsAsync(DashboardFilter? filter = null);
        Task<List<EnhancedAlert>> GetUnacknowledgedAlertsAsync(DashboardFilter? filter = null);
        Task<EnhancedAlert?> GetAlertAsync(Guid alertId);
        Task<bool> AcknowledgeAlertAsync(AlertAcknowledgmentRequest request);
        Task<bool> ResolveAlertAsync(AlertResolutionRequest request);
        Task<bool> EscalateAlertAsync(Guid alertId, string escalatedTo, int escalationLevel);
        Task<AlertResponseMetrics> GetAlertResponseMetricsAsync(DashboardFilter? filter = null, string period = "Today");
        Task<List<EnhancedAlert>> GetAlertHistoryAsync(DashboardFilter? filter = null, DateTime? startDate = null, DateTime? endDate = null);
    }

    /// <summary>
    /// Service for predictive analytics
    /// </summary>
    public interface IPredictiveAnalyticsService
    {
        Task<RiskPrediction> GetRiskPredictionAsync(Guid partographId);
        Task<List<RiskPrediction>> GetHighRiskPatientsAsync(DashboardFilter? filter = null);
        Task<FacilityRiskSummary> GetFacilityRiskSummaryAsync(Guid facilityId);
        Task<List<FacilityRiskSummary>> GetAllFacilityRiskSummariesAsync(DashboardFilter? filter = null);
        Task<Dictionary<string, double>> GetRiskTrendsAsync(DashboardFilter? filter = null, int days = 30);
    }

    /// <summary>
    /// Service for data quality scoring
    /// </summary>
    public interface IDataQualityService
    {
        Task<DataQualityScore> GetFacilityDataQualityAsync(Guid facilityId);
        Task<DataQualityScore> GetDistrictDataQualityAsync(Guid districtId);
        Task<DataQualityScore> GetRegionDataQualityAsync(Guid regionId);
        Task<DataQualityScore> GetNationalDataQualityAsync();
        Task<List<DataQualityScore>> GetDataQualityRankingAsync(DashboardFilter? filter = null);
        Task<List<FieldCompletenessReport>> GetFieldCompletenessAsync(DashboardFilter? filter = null);
        Task<List<DataGap>> GetIdentifiedGapsAsync(DashboardFilter? filter = null);
    }

    /// <summary>
    /// Service for comparative benchmarking
    /// </summary>
    public interface IBenchmarkService
    {
        Task<BenchmarkComparison> GetFacilityBenchmarkAsync(Guid facilityId);
        Task<BenchmarkComparison> GetDistrictBenchmarkAsync(Guid districtId);
        Task<BenchmarkComparison> GetRegionBenchmarkAsync(Guid regionId);
        Task<List<FacilityRanking>> GetFacilityRankingsAsync(DashboardFilter? filter = null);
        Task<List<FacilityRanking>> GetDistrictRankingsAsync(DashboardFilter? filter = null);
        Task<List<FacilityRanking>> GetRegionRankingsAsync();
        Task<Dictionary<string, double>> GetNationalBenchmarksAsync();
        Task<Dictionary<string, double>> GetRegionalBenchmarksAsync(Guid regionId);
    }

    /// <summary>
    /// Service for configurable alert thresholds
    /// </summary>
    public interface IAlertThresholdService
    {
        Task<AlertThresholdConfiguration> GetGlobalThresholdsAsync();
        Task<AlertThresholdConfiguration> GetFacilityThresholdsAsync(Guid facilityId);
        Task<bool> SaveFacilityThresholdsAsync(AlertThresholdConfiguration config);
        Task<bool> ResetToGlobalDefaultsAsync(Guid facilityId);
        Task<List<AlertThresholdConfiguration>> GetAllCustomThresholdsAsync();
    }

    /// <summary>
    /// Service for push notifications
    /// </summary>
    public interface INotificationService
    {
        Task<NotificationSubscription?> GetSubscriptionAsync(string userId);
        Task<bool> SaveSubscriptionAsync(NotificationSubscription subscription);
        Task<List<NotificationMessage>> GetUnreadNotificationsAsync(string userId);
        Task<List<NotificationMessage>> GetNotificationHistoryAsync(string userId, int count = 50);
        Task<bool> MarkAsReadAsync(Guid notificationId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
    }

    /// <summary>
    /// Service for offline alert queue management
    /// </summary>
    public interface IOfflineQueueService
    {
        Task<OfflineQueueStatus> GetQueueStatusAsync();
        Task QueueAlertAsync(EnhancedAlert alert);
        Task QueueAcknowledgmentAsync(AlertAcknowledgmentRequest request);
        Task<bool> SyncQueueAsync();
        Task<List<QueuedAlert>> GetPendingAlertsAsync();
        Task ClearSyncedItemsAsync();
    }

    /// <summary>
    /// Service for report visualizations
    /// </summary>
    public interface IReportVisualizationService
    {
        Task<ReportVisualization> GetDeliveryReportAsync(DashboardFilter? filter = null);
        Task<ReportVisualization> GetComplicationReportAsync(DashboardFilter? filter = null);
        Task<ReportVisualization> GetOutcomeReportAsync(DashboardFilter? filter = null);
        Task<ReportVisualization> GetAlertResponseReportAsync(DashboardFilter? filter = null);
        Task<ReportVisualization> GetWHOComplianceReportAsync(DashboardFilter? filter = null);
        Task<ReportVisualization> GetStaffPerformanceReportAsync(DashboardFilter? filter = null);
    }
}
