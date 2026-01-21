using MAAME.DROMO.PARTOGRAPH.MODEL;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Controllers;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Services
{
    /// <summary>
    /// Service interface for Robson Classification operations
    /// Reference: WHO Robson Classification: Implementation Manual (ISBN 978-92-4-151319-7)
    /// </summary>
    public interface IRobsonService
    {
        /// <summary>
        /// Generate complete Robson report for a period
        /// </summary>
        Task<RobsonClassificationReport> GenerateReportAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null,
            Guid? regionId = null,
            Guid? districtId = null);

        /// <summary>
        /// Get dashboard summary for quick overview
        /// </summary>
        Task<RobsonDashboardSummary> GetDashboardSummaryAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null,
            Guid? regionId = null,
            Guid? districtId = null);

        /// <summary>
        /// Get group statistics for a period
        /// </summary>
        Task<List<RobsonGroupStatistics>> GetGroupStatisticsAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null);

        /// <summary>
        /// Get monthly trends for a year
        /// </summary>
        Task<List<RobsonMonthlyTrend>> GetMonthlyTrendsAsync(
            int year,
            Guid? facilityId = null,
            Guid? regionId = null);

        /// <summary>
        /// Get comparative report across facilities
        /// </summary>
        Task<RobsonComparativeReport> GetComparativeReportAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? regionId = null);

        /// <summary>
        /// Get quality indicators from Robson analysis
        /// </summary>
        Task<RobsonQualityIndicators> GetQualityIndicatorsAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null);

        /// <summary>
        /// Get Group 2 sub-analysis
        /// </summary>
        Task<Group2SubAnalysis> GetGroup2AnalysisAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null);

        /// <summary>
        /// Get Group 5 sub-analysis (VBAC)
        /// </summary>
        Task<Group5SubAnalysis> GetGroup5AnalysisAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null);

        /// <summary>
        /// Get individual case records for audit
        /// </summary>
        Task<List<RobsonCaseRecord>> GetCaseRecordsAsync(
            DateTime startDate,
            DateTime endDate,
            RobsonGroup? group = null,
            Guid? facilityId = null,
            int pageSize = 100,
            int page = 1);

        /// <summary>
        /// Get action items based on analysis
        /// </summary>
        Task<List<RobsonActionItem>> GetActionItemsAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null);

        /// <summary>
        /// Classify a single delivery
        /// </summary>
        Task<RobsonClassification?> ClassifyDeliveryAsync(Guid partographId);

        /// <summary>
        /// Batch classify deliveries in a date range
        /// </summary>
        Task<BatchClassifyResponse> BatchClassifyDeliveriesAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null);
    }
}
