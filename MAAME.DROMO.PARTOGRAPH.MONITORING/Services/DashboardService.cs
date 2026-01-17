using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly HttpClient _httpClient;

        public DashboardService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DashboardSummary> GetDashboardSummaryAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<ApiDashboardSummary>($"api/monitoring/dashboard/summary{queryParams}");

                if (response != null)
                {
                    return new DashboardSummary
                    {
                        TotalRegions = response.TotalRegions,
                        TotalDistricts = response.TotalDistricts,
                        TotalFacilities = response.TotalFacilities,
                        ActiveFacilities = response.ActiveFacilities,
                        TotalDeliveriesToday = response.TotalDeliveriesToday,
                        TotalDeliveriesThisMonth = response.TotalDeliveriesThisMonth,
                        TotalDeliveriesThisYear = response.TotalDeliveriesThisYear,
                        ActiveLabors = response.ActiveLabors,
                        HighRiskLabors = response.HighRiskLabors,
                        ComplicationsToday = response.ComplicationsToday,
                        ReferralsToday = response.ReferralsToday,
                        MaternalDeaths = response.MaternalDeaths,
                        NeonatalDeaths = response.NeonatalDeaths,
                        Stillbirths = response.Stillbirths
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting dashboard summary: {ex.Message}");
            }

            return new DashboardSummary();
        }

        public async Task<List<TrendData>> GetDeliveryTrendAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<List<ApiTrendData>>($"api/monitoring/dashboard/trends/deliveries{queryParams}");

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
                Console.WriteLine($"Error getting delivery trends: {ex.Message}");
            }

            return new List<TrendData>();
        }

        public async Task<DeliveryModeDistribution> GetDeliveryModeDistributionAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<ApiDeliveryModeDistribution>($"api/monitoring/dashboard/delivery-modes{queryParams}");

                if (response != null)
                {
                    return new DeliveryModeDistribution
                    {
                        NormalVaginal = response.NormalVaginal,
                        AssistedVaginal = response.AssistedVaginal,
                        ElectiveCaesarean = response.ElectiveCaesarean,
                        EmergencyCaesarean = response.EmergencyCaesarean
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting delivery mode distribution: {ex.Message}");
            }

            return new DeliveryModeDistribution();
        }

        public async Task<List<DashboardAlert>> GetActiveAlertsAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<List<ApiAlert>>($"api/monitoring/dashboard/alerts{queryParams}");

                if (response != null)
                {
                    return response.Select(a => new DashboardAlert
                    {
                        ID = a.Id,
                        Title = a.Title ?? "Alert",
                        Message = a.Message ?? "",
                        Severity = a.Severity ?? "Info",
                        Category = a.Category ?? "General",
                        FacilityName = a.FacilityName ?? "",
                        CreatedAt = a.CreatedAt,
                        IsResolved = a.IsResolved
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting active alerts: {ex.Message}");
            }

            return new List<DashboardAlert>();
        }

        public async Task<List<TrendData>> GetComplicationTrendAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                // Use the analytics endpoint for complications
                var today = DateTime.UtcNow.Date;
                var startDate = today.AddDays(-30);
                var response = await _httpClient.GetFromJsonAsync<List<ApiTrendData>>(
                    $"api/analytics/complications?startDate={startDate:yyyy-MM-dd}&endDate={today:yyyy-MM-dd}");

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
                Console.WriteLine($"Error getting complication trends: {ex.Message}");
            }

            return new List<TrendData>();
        }

        private string BuildQueryParams(DashboardFilter? filter)
        {
            if (filter == null) return "";

            var parameters = new List<string>();

            if (filter.RegionID.HasValue)
                parameters.Add($"regionId={filter.RegionID.Value}");

            if (filter.DistrictID.HasValue)
                parameters.Add($"districtId={filter.DistrictID.Value}");

            if (filter.FacilityID.HasValue)
                parameters.Add($"facilityId={filter.FacilityID.Value}");

            return parameters.Count > 0 ? "?" + string.Join("&", parameters) : "";
        }
    }

    // API Response models for Dashboard
    internal class ApiDashboardSummary
    {
        public int TotalRegions { get; set; }
        public int TotalDistricts { get; set; }
        public int TotalFacilities { get; set; }
        public int ActiveFacilities { get; set; }
        public int TotalDeliveriesToday { get; set; }
        public int TotalDeliveriesThisMonth { get; set; }
        public int TotalDeliveriesThisYear { get; set; }
        public int ActiveLabors { get; set; }
        public int HighRiskLabors { get; set; }
        public int ComplicationsToday { get; set; }
        public int ReferralsToday { get; set; }
        public int MaternalDeaths { get; set; }
        public int NeonatalDeaths { get; set; }
        public int Stillbirths { get; set; }
    }

    internal class ApiTrendData
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = "";
        public int Value { get; set; }
    }

    internal class ApiDeliveryModeDistribution
    {
        public int NormalVaginal { get; set; }
        public int AssistedVaginal { get; set; }
        public int ElectiveCaesarean { get; set; }
        public int EmergencyCaesarean { get; set; }
    }

    internal class ApiAlert
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? Severity { get; set; }
        public string? Category { get; set; }
        public string? FacilityName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }
}
