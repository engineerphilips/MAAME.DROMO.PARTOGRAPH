using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class DistrictService : IDistrictService
    {
        private readonly HttpClient _httpClient;

        public DistrictService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<DistrictSummary>> GetDistrictsByRegionAsync(Guid regionId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ApiDistrictSummary>>($"api/monitoring/districts?regionId={regionId}");
                return MapToDistrictSummaries(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting districts by region: {ex.Message}");
            }

            return new List<DistrictSummary>();
        }

        public async Task<List<DistrictSummary>> GetAllDistrictSummariesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ApiDistrictSummary>>("api/monitoring/districts");
                return MapToDistrictSummaries(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all districts: {ex.Message}");
            }

            return new List<DistrictSummary>();
        }

        public async Task<DistrictSummary?> GetDistrictSummaryAsync(Guid districtId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiDistrictSummary>($"api/monitoring/districts/{districtId}");

                if (response != null)
                {
                    return new DistrictSummary
                    {
                        ID = response.Id,
                        Name = response.Name,
                        Code = response.Code,
                        Type = response.Type,
                        RegionName = response.RegionName,
                        FacilityCount = response.FacilityCount,
                        ActiveFacilities = response.ActiveFacilities,
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
                Console.WriteLine($"Error getting district: {ex.Message}");
            }

            return null;
        }

        public async Task<DashboardSummary> GetDistrictDashboardAsync(Guid districtId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiDashboardSummary>($"api/monitoring/dashboard/summary?districtId={districtId}");

                if (response != null)
                {
                    return new DashboardSummary
                    {
                        TotalRegions = 1,
                        TotalDistricts = 1,
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
                Console.WriteLine($"Error getting district dashboard: {ex.Message}");
            }

            return new DashboardSummary();
        }

        public async Task<List<TrendData>> GetDistrictDeliveryTrendAsync(Guid districtId, int days = 30)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ApiTrendData>>(
                    $"api/monitoring/dashboard/trends/deliveries?districtId={districtId}&days={days}");

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
                Console.WriteLine($"Error getting district delivery trend: {ex.Message}");
            }

            return new List<TrendData>();
        }

        private List<DistrictSummary> MapToDistrictSummaries(List<ApiDistrictSummary>? response)
        {
            if (response == null) return new List<DistrictSummary>();

            return response.Select(d => new DistrictSummary
            {
                ID = d.Id,
                Name = d.Name,
                Code = d.Code,
                Type = d.Type,
                RegionName = d.RegionName,
                FacilityCount = d.FacilityCount,
                ActiveFacilities = d.ActiveFacilities,
                DeliveriesToday = d.DeliveriesToday,
                DeliveriesThisMonth = d.DeliveriesThisMonth,
                ActiveLabors = d.ActiveLabors,
                CaesareanRate = d.CaesareanRate,
                PerformanceStatus = d.PerformanceStatus ?? "Normal"
            }).ToList();
        }
    }

    // API Response models for District
    internal class ApiDistrictSummary
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string Type { get; set; } = "";
        public Guid? RegionId { get; set; }
        public string RegionName { get; set; } = "";
        public int FacilityCount { get; set; }
        public int ActiveFacilities { get; set; }
        public int DeliveriesToday { get; set; }
        public int DeliveriesThisMonth { get; set; }
        public int ActiveLabors { get; set; }
        public double CaesareanRate { get; set; }
        public string? PerformanceStatus { get; set; }
    }
}
