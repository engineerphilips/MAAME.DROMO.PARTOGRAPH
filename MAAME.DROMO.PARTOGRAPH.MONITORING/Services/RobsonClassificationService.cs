using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    /// <summary>
    /// Service implementation for Robson Classification analysis and reporting
    /// Reference: WHO Robson Classification: Implementation Manual (ISBN 978-92-4-151319-7)
    /// </summary>
    public class RobsonClassificationService : IRobsonClassificationService
    {
        private readonly HttpClient _httpClient;

        public RobsonClassificationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<RobsonClassificationReport> GetRobsonReportAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null,
            Guid? regionId = null,
            Guid? districtId = null)
        {
            try
            {
                var queryParams = BuildQueryParams(startDate, endDate, facilityId, regionId, districtId);
                var response = await _httpClient.GetFromJsonAsync<RobsonClassificationReport>(
                    $"api/robson/report{queryParams}");

                return response ?? CreateEmptyReport(startDate, endDate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting Robson report: {ex.Message}");
                return CreateEmptyReport(startDate, endDate);
            }
        }

        public async Task<RobsonDashboardSummary> GetDashboardSummaryAsync(DashboardFilter? filter = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var monthStart = new DateTime(today.Year, today.Month, 1);
                var queryParams = BuildQueryParamsFromFilter(filter, monthStart, today);

                var response = await _httpClient.GetFromJsonAsync<RobsonDashboardSummary>(
                    $"api/robson/dashboard{queryParams}");

                return response ?? CreateEmptyDashboardSummary();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting Robson dashboard: {ex.Message}");
                return CreateEmptyDashboardSummary();
            }
        }

        public async Task<List<RobsonGroupStatistics>> GetGroupStatisticsAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null)
        {
            try
            {
                var queryParams = BuildQueryParams(startDate, endDate, facilityId);
                var response = await _httpClient.GetFromJsonAsync<List<RobsonGroupStatistics>>(
                    $"api/robson/groups{queryParams}");

                return response ?? CreateDefaultGroupStatistics();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting group statistics: {ex.Message}");
                return CreateDefaultGroupStatistics();
            }
        }

        public async Task<List<RobsonMonthlyTrend>> GetMonthlyTrendsAsync(
            int year,
            Guid? facilityId = null,
            Guid? regionId = null)
        {
            try
            {
                var facilityParam = facilityId.HasValue ? $"&facilityId={facilityId.Value}" : "";
                var regionParam = regionId.HasValue ? $"&regionId={regionId.Value}" : "";

                var response = await _httpClient.GetFromJsonAsync<List<RobsonMonthlyTrend>>(
                    $"api/robson/trends?year={year}{facilityParam}{regionParam}");

                return response ?? new List<RobsonMonthlyTrend>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting monthly trends: {ex.Message}");
                return new List<RobsonMonthlyTrend>();
            }
        }

        public async Task<RobsonComparativeReport> GetComparativeReportAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? regionId = null)
        {
            try
            {
                var queryParams = BuildQueryParams(startDate, endDate, null, regionId);
                var response = await _httpClient.GetFromJsonAsync<RobsonComparativeReport>(
                    $"api/robson/comparative{queryParams}");

                return response ?? CreateEmptyComparativeReport(startDate, endDate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting comparative report: {ex.Message}");
                return CreateEmptyComparativeReport(startDate, endDate);
            }
        }

        public async Task<RobsonQualityIndicators> GetQualityIndicatorsAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null)
        {
            try
            {
                var queryParams = BuildQueryParams(startDate, endDate, facilityId);
                var response = await _httpClient.GetFromJsonAsync<RobsonQualityIndicators>(
                    $"api/robson/quality-indicators{queryParams}");

                return response ?? new RobsonQualityIndicators();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting quality indicators: {ex.Message}");
                return new RobsonQualityIndicators();
            }
        }

        public async Task<Group2SubAnalysis> GetGroup2AnalysisAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null)
        {
            try
            {
                var queryParams = BuildQueryParams(startDate, endDate, facilityId);
                var response = await _httpClient.GetFromJsonAsync<Group2SubAnalysis>(
                    $"api/robson/group2-analysis{queryParams}");

                return response ?? new Group2SubAnalysis();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting Group 2 analysis: {ex.Message}");
                return new Group2SubAnalysis();
            }
        }

        public async Task<Group5SubAnalysis> GetGroup5AnalysisAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null)
        {
            try
            {
                var queryParams = BuildQueryParams(startDate, endDate, facilityId);
                var response = await _httpClient.GetFromJsonAsync<Group5SubAnalysis>(
                    $"api/robson/group5-analysis{queryParams}");

                return response ?? new Group5SubAnalysis();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting Group 5 analysis: {ex.Message}");
                return new Group5SubAnalysis();
            }
        }

        public async Task<List<RobsonCaseRecord>> GetCaseRecordsAsync(
            DateTime startDate,
            DateTime endDate,
            RobsonGroup? filterGroup = null,
            Guid? facilityId = null,
            int pageSize = 100,
            int page = 1)
        {
            try
            {
                var queryParams = BuildQueryParams(startDate, endDate, facilityId);
                var groupParam = filterGroup.HasValue ? $"&group={(int)filterGroup.Value}" : "";
                var pageParams = $"&pageSize={pageSize}&page={page}";

                var response = await _httpClient.GetFromJsonAsync<List<RobsonCaseRecord>>(
                    $"api/robson/cases{queryParams}{groupParam}{pageParams}");

                return response ?? new List<RobsonCaseRecord>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting case records: {ex.Message}");
                return new List<RobsonCaseRecord>();
            }
        }

        public async Task<List<RobsonActionItem>> GetActionItemsAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null)
        {
            try
            {
                var queryParams = BuildQueryParams(startDate, endDate, facilityId);
                var response = await _httpClient.GetFromJsonAsync<List<RobsonActionItem>>(
                    $"api/robson/action-items{queryParams}");

                return response ?? new List<RobsonActionItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting action items: {ex.Message}");
                return new List<RobsonActionItem>();
            }
        }

        public async Task<RobsonClassification?> ClassifyDeliveryAsync(Guid partographId)
        {
            try
            {
                var response = await _httpClient.PostAsync(
                    $"api/robson/classify/{partographId}", null);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RobsonClassification>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error classifying delivery: {ex.Message}");
            }
            return null;
        }

        public async Task<int> BatchClassifyDeliveriesAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null)
        {
            try
            {
                var queryParams = BuildQueryParams(startDate, endDate, facilityId);
                var response = await _httpClient.PostAsync(
                    $"api/robson/batch-classify{queryParams}", null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<BatchClassifyResult>();
                    return result?.ClassifiedCount ?? 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error batch classifying: {ex.Message}");
            }
            return 0;
        }

        #region Private Helper Methods

        private static string BuildQueryParams(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null,
            Guid? regionId = null,
            Guid? districtId = null)
        {
            var queryParams = $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";

            if (facilityId.HasValue)
                queryParams += $"&facilityId={facilityId.Value}";
            if (regionId.HasValue)
                queryParams += $"&regionId={regionId.Value}";
            if (districtId.HasValue)
                queryParams += $"&districtId={districtId.Value}";

            return queryParams;
        }

        private static string BuildQueryParamsFromFilter(DashboardFilter? filter, DateTime startDate, DateTime endDate)
        {
            var queryParams = $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";

            if (filter?.FacilityID.HasValue == true)
                queryParams += $"&facilityId={filter.FacilityID.Value}";
            if (filter?.RegionID.HasValue == true)
                queryParams += $"&regionId={filter.RegionID.Value}";
            if (filter?.DistrictID.HasValue == true)
                queryParams += $"&districtId={filter.DistrictID.Value}";

            return queryParams;
        }

        private static RobsonClassificationReport CreateEmptyReport(DateTime startDate, DateTime endDate)
        {
            return new RobsonClassificationReport
            {
                StartDate = startDate,
                EndDate = endDate,
                GeneratedAt = DateTime.Now,
                GroupStatistics = CreateDefaultGroupStatistics(),
                QualityIndicators = new RobsonQualityIndicators()
            };
        }

        private static RobsonDashboardSummary CreateEmptyDashboardSummary()
        {
            return new RobsonDashboardSummary
            {
                LastUpdated = DateTime.Now,
                GroupDistribution = CreateDefaultGroupDistribution()
            };
        }

        private static RobsonComparativeReport CreateEmptyComparativeReport(DateTime startDate, DateTime endDate)
        {
            return new RobsonComparativeReport
            {
                StartDate = startDate,
                EndDate = endDate,
                GeneratedAt = DateTime.Now,
                WHORecommendedCSRate = 15
            };
        }

        private static List<RobsonGroupStatistics> CreateDefaultGroupStatistics()
        {
            var groups = new List<RobsonGroupStatistics>();
            for (int i = 1; i <= 10; i++)
            {
                groups.Add(new RobsonGroupStatistics
                {
                    Group = (RobsonGroup)i,
                    WHOExpectedCSRate = GetWHOExpectedCSRate((RobsonGroup)i),
                    WHOExpectedGroupSize = GetWHOExpectedGroupSize((RobsonGroup)i)
                });
            }
            return groups;
        }

        private static List<RobsonGroupDistribution> CreateDefaultGroupDistribution()
        {
            var groups = new List<RobsonGroupDistribution>();
            var colors = GetGroupColors();

            for (int i = 1; i <= 10; i++)
            {
                groups.Add(new RobsonGroupDistribution
                {
                    Group = (RobsonGroup)i,
                    Color = colors.ContainsKey((RobsonGroup)i) ? colors[(RobsonGroup)i] : "#808080"
                });
            }
            return groups;
        }

        /// <summary>
        /// WHO expected CS rates by group (approximate benchmarks)
        /// Reference: WHO Robson Classification Implementation Manual 2017
        /// </summary>
        private static decimal? GetWHOExpectedCSRate(RobsonGroup group)
        {
            return group switch
            {
                RobsonGroup.Group1 => 10m,    // Should be 5-10%
                RobsonGroup.Group2 => 35m,    // Higher due to inductions and pre-labor CS
                RobsonGroup.Group3 => 3m,     // Should be ≤3%
                RobsonGroup.Group4 => 20m,    // Higher due to inductions
                RobsonGroup.Group5 => 75m,    // Highly variable, depends on VBAC policy
                RobsonGroup.Group6 => 90m,    // Most breech deliveries are CS
                RobsonGroup.Group7 => 85m,    // Most breech deliveries are CS
                RobsonGroup.Group8 => 60m,    // Multiple pregnancies
                RobsonGroup.Group9 => 100m,   // Transverse/oblique always CS
                RobsonGroup.Group10 => 30m,   // Preterm varies
                _ => null
            };
        }

        /// <summary>
        /// WHO expected group size as percentage of total deliveries
        /// Reference: WHO Robson Classification Implementation Manual 2017
        /// </summary>
        private static decimal? GetWHOExpectedGroupSize(RobsonGroup group)
        {
            return group switch
            {
                RobsonGroup.Group1 => 35m,   // Largest group, ~30-40%
                RobsonGroup.Group2 => 10m,   // ~8-12%
                RobsonGroup.Group3 => 30m,   // ~25-35%
                RobsonGroup.Group4 => 5m,    // ~3-7%
                RobsonGroup.Group5 => 8m,    // Should be <10%
                RobsonGroup.Group6 => 1.5m,  // ~1-2%
                RobsonGroup.Group7 => 1m,    // ~0.5-1.5%
                RobsonGroup.Group8 => 1.5m,  // ~1-2%
                RobsonGroup.Group9 => 0.5m,  // ~0.3-0.7%
                RobsonGroup.Group10 => 5m,   // ~3-7%
                _ => null
            };
        }

        /// <summary>
        /// Color codes for Robson group visualization
        /// </summary>
        private static Dictionary<RobsonGroup, string> GetGroupColors()
        {
            return new Dictionary<RobsonGroup, string>
            {
                { RobsonGroup.Group1, "#4CAF50" },   // Green
                { RobsonGroup.Group2, "#8BC34A" },   // Light Green
                { RobsonGroup.Group3, "#2196F3" },   // Blue
                { RobsonGroup.Group4, "#03A9F4" },   // Light Blue
                { RobsonGroup.Group5, "#FF9800" },   // Orange (attention - previous CS)
                { RobsonGroup.Group6, "#9C27B0" },   // Purple
                { RobsonGroup.Group7, "#673AB7" },   // Deep Purple
                { RobsonGroup.Group8, "#E91E63" },   // Pink
                { RobsonGroup.Group9, "#F44336" },   // Red (high risk)
                { RobsonGroup.Group10, "#FFC107" }   // Amber (preterm)
            };
        }

        #endregion
    }

    /// <summary>
    /// Response model for batch classification
    /// </summary>
    public class BatchClassifyResult
    {
        public int ClassifiedCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
