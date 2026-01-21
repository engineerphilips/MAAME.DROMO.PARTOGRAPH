using MAAME.DROMO.PARTOGRAPH.MODEL;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    /// <summary>
    /// Service interface for Robson Classification analysis and reporting
    /// Reference: WHO Robson Classification: Implementation Manual (ISBN 978-92-4-151319-7)
    /// </summary>
    public interface IRobsonClassificationService
    {
        /// <summary>
        /// Gets the complete Robson Classification report for a specified period
        /// </summary>
        Task<RobsonClassificationReport> GetRobsonReportAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null,
            Guid? regionId = null,
            Guid? districtId = null);

        /// <summary>
        /// Gets the Robson dashboard summary for quick overview
        /// </summary>
        Task<RobsonDashboardSummary> GetDashboardSummaryAsync(DashboardFilter? filter = null);

        /// <summary>
        /// Gets Robson group statistics for a specified period
        /// </summary>
        Task<List<RobsonGroupStatistics>> GetGroupStatisticsAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null);

        /// <summary>
        /// Gets monthly trends for Robson analysis
        /// </summary>
        Task<List<RobsonMonthlyTrend>> GetMonthlyTrendsAsync(
            int year,
            Guid? facilityId = null,
            Guid? regionId = null);

        /// <summary>
        /// Gets comparative report across facilities
        /// </summary>
        Task<RobsonComparativeReport> GetComparativeReportAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? regionId = null);

        /// <summary>
        /// Gets quality indicators derived from Robson analysis
        /// </summary>
        Task<RobsonQualityIndicators> GetQualityIndicatorsAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null);

        /// <summary>
        /// Gets Group 2 sub-analysis (2a: induced vs 2b: pre-labor CS)
        /// </summary>
        Task<Group2SubAnalysis> GetGroup2AnalysisAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null);

        /// <summary>
        /// Gets Group 5 sub-analysis (VBAC analysis)
        /// </summary>
        Task<Group5SubAnalysis> GetGroup5AnalysisAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null);

        /// <summary>
        /// Gets individual case records for audit
        /// </summary>
        Task<List<RobsonCaseRecord>> GetCaseRecordsAsync(
            DateTime startDate,
            DateTime endDate,
            RobsonGroup? filterGroup = null,
            Guid? facilityId = null,
            int pageSize = 100,
            int page = 1);

        /// <summary>
        /// Gets action items based on Robson analysis
        /// </summary>
        Task<List<RobsonActionItem>> GetActionItemsAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null);

        /// <summary>
        /// Classifies a single partograph delivery and returns the Robson group
        /// </summary>
        Task<RobsonClassification?> ClassifyDeliveryAsync(Guid partographId);

        /// <summary>
        /// Batch classify all unclassified deliveries in a date range
        /// </summary>
        Task<int> BatchClassifyDeliveriesAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null);
    }
}
