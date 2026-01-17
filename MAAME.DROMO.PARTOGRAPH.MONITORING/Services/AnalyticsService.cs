using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly HttpClient _httpClient;

        public AnalyticsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<DeliveryOutcomeStats>> GetDeliveryOutcomeStatsAsync(DashboardFilter? filter = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var monthStart = new DateTime(today.Year, today.Month, 1);
                var queryParams = BuildQueryParams(filter, monthStart, today);

                var response = await _httpClient.GetFromJsonAsync<ApiDeliveryOutcomesResponse>(
                    $"api/analytics/deliveries{queryParams}");

                if (response?.DeliveryModes != null)
                {
                    var total = response.TotalDeliveries;
                    return response.DeliveryModes.Select(m => new DeliveryOutcomeStats
                    {
                        Category = m.Mode ?? "Unknown",
                        Count = m.Count,
                        Percentage = total > 0 ? (double)m.Count / total * 100 : 0
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting delivery outcome stats: {ex.Message}");
            }

            return new List<DeliveryOutcomeStats>();
        }

        public async Task<List<ComplicationStats>> GetComplicationStatsAsync(DashboardFilter? filter = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var monthStart = new DateTime(today.Year, today.Month, 1);
                var queryParams = BuildQueryParams(filter, monthStart, today);

                var response = await _httpClient.GetFromJsonAsync<ApiComplicationStatsResponse>(
                    $"api/analytics/complications{queryParams}");

                if (response?.ByCategory != null)
                {
                    return response.ByCategory.Select(c => new ComplicationStats
                    {
                        ComplicationType = c.Category ?? "Unknown",
                        Count = c.Count,
                        Severity = "Unknown"
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting complication stats: {ex.Message}");
            }

            return new List<ComplicationStats>();
        }

        public async Task<List<ReferralStats>> GetReferralStatsAsync(DashboardFilter? filter = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var monthStart = new DateTime(today.Year, today.Month, 1);
                var queryParams = BuildQueryParams(filter, monthStart, today);

                var response = await _httpClient.GetFromJsonAsync<ApiReferralStatsResponse>(
                    $"api/analytics/referrals{queryParams}");

                if (response?.ByPrimaryReason != null)
                {
                    return response.ByPrimaryReason.Select(r => new ReferralStats
                    {
                        Reason = r.Reason ?? "Unspecified",
                        Count = r.Count,
                        Completed = 0,
                        Pending = 0
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting referral stats: {ex.Message}");
            }

            return new List<ReferralStats>();
        }

        public async Task<MortalityStats> GetMortalityStatsAsync(DashboardFilter? filter = null)
        {
            try
            {
                var year = DateTime.UtcNow.Year;
                var facilityParam = "";

                if (filter?.FacilityID.HasValue == true)
                    facilityParam = $"&facilityId={filter.FacilityID.Value}";

                var maternalResponse = await _httpClient.GetFromJsonAsync<ApiMaternalMortalityResponse>(
                    $"api/analytics/mortality/maternal?year={year}{facilityParam}");

                var neonatalResponse = await _httpClient.GetFromJsonAsync<ApiNeonatalOutcomeResponse>(
                    $"api/analytics/mortality/neonatal?year={year}{facilityParam}");

                return new MortalityStats
                {
                    MaternalDeaths = maternalResponse?.TotalDeaths ?? 0,
                    NeonatalDeaths = neonatalResponse?.NeonatalDeaths ?? 0,
                    Stillbirths = neonatalResponse?.Stillbirths ?? 0,
                    EarlyNeonatalDeaths = 0,
                    TotalDeliveries = 0,
                    MaternalMortalityRatio = 0,
                    NeonatalMortalityRate = 0,
                    StillbirthRate = 0
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting mortality stats: {ex.Message}");
            }

            return new MortalityStats();
        }

        public async Task<List<FacilityPerformanceData>> GetFacilityPerformanceAsync(DashboardFilter? filter = null)
        {
            try
            {
                var regionParam = filter?.RegionID.HasValue == true ? $"?region={filter.RegionID.Value}" : "";
                var response = await _httpClient.GetFromJsonAsync<List<ApiFacilityPerformance>>(
                    $"api/analytics/facilities/rankings{regionParam}");

                if (response != null)
                {
                    return response.Select(f => new FacilityPerformanceData
                    {
                        FacilityID = f.FacilityID,
                        FacilityName = f.FacilityName ?? "",
                        DistrictName = f.Region ?? "",
                        TotalDeliveries = f.TotalDeliveries,
                        CaesareanRate = f.CaesareanRate,
                        ComplicationRate = f.ComplicationRate,
                        ReferralRate = 0,
                        PerformanceGrade = MapScoreToGrade(f.OverallPerformanceScore)
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting facility performance: {ex.Message}");
            }

            return new List<FacilityPerformanceData>();
        }

        private string BuildQueryParams(DashboardFilter? filter, DateTime startDate, DateTime endDate)
        {
            var parameters = new List<string>
            {
                $"startDate={startDate:yyyy-MM-dd}",
                $"endDate={endDate:yyyy-MM-dd}"
            };

            if (filter?.FacilityID.HasValue == true)
                parameters.Add($"facilityId={filter.FacilityID.Value}");

            return "?" + string.Join("&", parameters);
        }

        private string MapScoreToGrade(double score)
        {
            return score switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "F"
            };
        }
    }

    // API Response models for Analytics
    internal class ApiDeliveryOutcomesResponse
    {
        public int TotalDeliveries { get; set; }
        public List<ApiDeliveryMode>? DeliveryModes { get; set; }
        public double CaesareanRate { get; set; }
    }

    internal class ApiDeliveryMode
    {
        public string? Mode { get; set; }
        public int Count { get; set; }
    }

    internal class ApiComplicationStatsResponse
    {
        public int TotalComplications { get; set; }
        public List<ApiCategoryCount>? ByCategory { get; set; }
    }

    internal class ApiCategoryCount
    {
        public string? Category { get; set; }
        public int Count { get; set; }
    }

    internal class ApiReferralStatsResponse
    {
        public int TotalReferrals { get; set; }
        public List<ApiReasonCount>? ByPrimaryReason { get; set; }
    }

    internal class ApiReasonCount
    {
        public string? Reason { get; set; }
        public int Count { get; set; }
    }

    internal class ApiMaternalMortalityResponse
    {
        public int Year { get; set; }
        public int TotalDeaths { get; set; }
    }

    internal class ApiNeonatalOutcomeResponse
    {
        public int Year { get; set; }
        public int TotalOutcomes { get; set; }
        public int Stillbirths { get; set; }
        public int NeonatalDeaths { get; set; }
    }

    internal class ApiFacilityPerformance
    {
        public Guid FacilityID { get; set; }
        public string? FacilityName { get; set; }
        public string? Region { get; set; }
        public int TotalDeliveries { get; set; }
        public double CaesareanRate { get; set; }
        public double ComplicationRate { get; set; }
        public double OverallPerformanceScore { get; set; }
    }
}
