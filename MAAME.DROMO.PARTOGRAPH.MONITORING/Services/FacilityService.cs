using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class FacilityService : IFacilityService
    {
        private readonly HttpClient _httpClient;

        public FacilityService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<FacilitySummary>> GetFacilitiesByDistrictAsync(Guid districtId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ApiFacilitySummary>>($"api/monitoring/facilities?districtId={districtId}");
                return MapToFacilitySummaries(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting facilities by district: {ex.Message}");
            }

            return new List<FacilitySummary>();
        }

        public async Task<List<FacilitySummary>> GetAllFacilitySummariesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ApiFacilitySummary>>("api/monitoring/facilities");
                return MapToFacilitySummaries(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all facilities: {ex.Message}");
            }

            return new List<FacilitySummary>();
        }

        public async Task<FacilitySummary?> GetFacilitySummaryAsync(Guid facilityId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiFacilitySummary>($"api/monitoring/facilities/{facilityId}");

                if (response != null)
                {
                    return new FacilitySummary
                    {
                        ID = response.Id,
                        Name = response.Name,
                        Code = response.Code,
                        Type = response.Type,
                        Level = response.Level,
                        DistrictName = response.DistrictName,
                        IsActive = response.IsActive,
                        DeliveriesToday = response.DeliveriesToday,
                        DeliveriesThisMonth = response.DeliveriesThisMonth,
                        ActiveLabors = response.ActiveLabors,
                        StaffCount = response.StaffCount,
                        LastActivityTime = response.LastActivityTime,
                        PerformanceStatus = response.PerformanceStatus ?? "Normal"
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting facility: {ex.Message}");
            }

            return null;
        }

        public async Task<DashboardSummary> GetFacilityDashboardAsync(Guid facilityId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiFacilitySummary>($"api/monitoring/facilities/{facilityId}");

                if (response != null)
                {
                    return new DashboardSummary
                    {
                        TotalRegions = 1,
                        TotalDistricts = 1,
                        TotalFacilities = 1,
                        ActiveFacilities = response.IsActive ? 1 : 0,
                        TotalDeliveriesToday = response.DeliveriesToday,
                        TotalDeliveriesThisMonth = response.DeliveriesThisMonth,
                        ActiveLabors = response.ActiveLabors
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting facility dashboard: {ex.Message}");
            }

            return new DashboardSummary();
        }

        private List<FacilitySummary> MapToFacilitySummaries(List<ApiFacilitySummary>? response)
        {
            if (response == null) return new List<FacilitySummary>();

            return response.Select(f => new FacilitySummary
            {
                ID = f.Id,
                Name = f.Name,
                Code = f.Code,
                Type = f.Type,
                Level = f.Level,
                DistrictName = f.DistrictName,
                IsActive = f.IsActive,
                DeliveriesToday = f.DeliveriesToday,
                DeliveriesThisMonth = f.DeliveriesThisMonth,
                ActiveLabors = f.ActiveLabors,
                StaffCount = f.StaffCount,
                LastActivityTime = f.LastActivityTime,
                PerformanceStatus = f.PerformanceStatus ?? "Normal"
            }).ToList();
        }
    }

    // API Response models for Facility
    internal class ApiFacilitySummary
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string Type { get; set; } = "";
        public string Level { get; set; } = "";
        public Guid? DistrictId { get; set; }
        public string DistrictName { get; set; } = "";
        public bool IsActive { get; set; }
        public int DeliveriesToday { get; set; }
        public int DeliveriesThisMonth { get; set; }
        public int ActiveLabors { get; set; }
        public int StaffCount { get; set; }
        public DateTime? LastActivityTime { get; set; }
        public string? PerformanceStatus { get; set; }
    }
}
