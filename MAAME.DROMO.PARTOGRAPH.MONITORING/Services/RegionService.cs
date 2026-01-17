using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class RegionService : IRegionService
    {
        private readonly HttpClient _httpClient;

        public RegionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<RegionSummary>> GetAllRegionSummariesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ApiRegionSummary>>("api/monitoring/regions");

                if (response != null)
                {
                    return response.Select(r => new RegionSummary
                    {
                        ID = r.Id,
                        Name = r.Name,
                        Code = r.Code,
                        DistrictCount = r.DistrictCount,
                        FacilityCount = r.FacilityCount,
                        DeliveriesToday = r.DeliveriesToday,
                        DeliveriesThisMonth = r.DeliveriesThisMonth,
                        ActiveLabors = r.ActiveLabors,
                        CaesareanRate = r.CaesareanRate,
                        PerformanceStatus = r.PerformanceStatus ?? "Normal"
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting regions: {ex.Message}");
            }

            return new List<RegionSummary>();
        }

        public async Task<RegionSummary?> GetRegionSummaryAsync(Guid regionId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiRegionSummary>($"api/monitoring/regions/{regionId}");

                if (response != null)
                {
                    return new RegionSummary
                    {
                        ID = response.Id,
                        Name = response.Name,
                        Code = response.Code,
                        DistrictCount = response.DistrictCount,
                        FacilityCount = response.FacilityCount,
                        DeliveriesToday = response.DeliveriesToday,
                        DeliveriesThisMonth = response.DeliveriesThisMonth,
                        ActiveLabors = response.ActiveLabors,
                        CaesareanRate = response.CaesareanRate,
                        PerformanceStatus = response.PerformanceStatus ?? "Normal"
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting region: {ex.Message}");
            }

            return null;
        }

        public async Task<DashboardSummary> GetRegionDashboardAsync(Guid regionId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiDashboardSummary>($"api/monitoring/dashboard/summary?regionId={regionId}");

                if (response != null)
                {
                    return new DashboardSummary
                    {
                        TotalRegions = 1,
                        TotalDistricts = response.TotalDistricts,
                        TotalFacilities = response.TotalFacilities,
                        ActiveFacilities = response.ActiveFacilities,
                        TotalDeliveriesToday = response.TotalDeliveriesToday,
                        TotalDeliveriesThisMonth = response.TotalDeliveriesThisMonth,
                        TotalDeliveriesThisYear = response.TotalDeliveriesThisYear,
                        ActiveLabors = response.ActiveLabors,
                        HighRiskLabors = response.HighRiskLabors,
                        ComplicationsToday = response.ComplicationsToday
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting region dashboard: {ex.Message}");
            }

            return new DashboardSummary();
        }

        public async Task<List<TrendData>> GetRegionDeliveryTrendAsync(Guid regionId, int days = 30)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ApiTrendData>>(
                    $"api/monitoring/dashboard/trends/deliveries?regionId={regionId}&days={days}");

                if (response != null)
                {
                    return response.Select(t => new TrendData
                    {
                        Date = t.Date,
                        Label = t.Label,
                        Value = t.Value
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting region delivery trend: {ex.Message}");
            }

            return new List<TrendData>();
        }
    }

    // API Response models for Region
    internal class ApiRegionSummary
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public int DistrictCount { get; set; }
        public int FacilityCount { get; set; }
        public int DeliveriesToday { get; set; }
        public int DeliveriesThisMonth { get; set; }
        public int ActiveLabors { get; set; }
        public double CaesareanRate { get; set; }
        public string? PerformanceStatus { get; set; }
    }
}
