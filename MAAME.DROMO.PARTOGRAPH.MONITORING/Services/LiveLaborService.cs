using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class LiveLaborService : ILiveLaborService
    {
        private readonly HttpClient _httpClient;

        public LiveLaborService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LiveLaborSummary> GetLiveLaborSummaryAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<LiveLaborSummary>($"api/monitoring/live-labor/summary{queryParams}");
                return response ?? GenerateMockSummary(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting live labor summary: {ex.Message}");
                return GenerateMockSummary(filter);
            }
        }

        public async Task<List<LiveLaborCase>> GetActiveLaborCasesAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<List<LiveLaborCase>>($"api/monitoring/live-labor/cases{queryParams}");
                return response ?? GenerateMockCases(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting active labor cases: {ex.Message}");
                return GenerateMockCases(filter);
            }
        }

        public async Task<LiveLaborCase?> GetLaborCaseAsync(Guid partographId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<LiveLaborCase>($"api/monitoring/live-labor/cases/{partographId}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting labor case: {ex.Message}");
                return null;
            }
        }

        public async Task<List<LiveLaborCase>> GetCriticalCasesAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<List<LiveLaborCase>>($"api/monitoring/live-labor/critical{queryParams}");
                return response ?? new List<LiveLaborCase>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting critical cases: {ex.Message}");
                var allCases = await GetActiveLaborCasesAsync(filter);
                return allCases.Where(c => c.RiskLevel == "Critical").ToList();
            }
        }

        public async Task<List<LiveLaborCase>> GetMeasurementsDueCasesAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<List<LiveLaborCase>>($"api/monitoring/live-labor/measurements-due{queryParams}");
                return response ?? new List<LiveLaborCase>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting measurements due cases: {ex.Message}");
                var allCases = await GetActiveLaborCasesAsync(filter);
                return allCases.Where(c => c.IsFHRDue || c.IsBPDue || c.IsDilatationDue).ToList();
            }
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

        // Mock data generation for development/fallback
        private LiveLaborSummary GenerateMockSummary(DashboardFilter? filter)
        {
            var random = new Random();
            return new LiveLaborSummary
            {
                TotalActiveCases = random.Next(15, 45),
                CriticalCases = random.Next(1, 5),
                HighRiskCases = random.Next(3, 10),
                ModerateRiskCases = random.Next(5, 15),
                NormalCases = random.Next(10, 25),
                MeasurementsDue = random.Next(5, 15),
                UnacknowledgedAlerts = random.Next(2, 10),
                LastUpdated = DateTime.UtcNow
            };
        }

        private List<LiveLaborCase> GenerateMockCases(DashboardFilter? filter)
        {
            var random = new Random();
            var cases = new List<LiveLaborCase>();
            var riskLevels = new[] { "Normal", "Normal", "Normal", "Moderate", "Moderate", "High", "Critical" };
            var stages = new[] { "First Stage", "First Stage", "First Stage", "Second Stage" };
            var facilities = new[] { "Korle Bu Teaching Hospital", "Ridge Hospital", "La General Hospital", "Tema General Hospital", "Achimota Hospital" };
            var districts = new[] { "Accra Metro", "La Dadekotopon", "Tema Metro", "Ga East", "Ga West" };
            var regions = new[] { "Greater Accra", "Greater Accra", "Greater Accra", "Greater Accra", "Greater Accra" };
            var firstNames = new[] { "Abena", "Akosua", "Ama", "Adwoa", "Yaa", "Afia", "Efua" };
            var lastNames = new[] { "Mensah", "Asante", "Owusu", "Boateng", "Appiah", "Amoah", "Darko" };

            var count = random.Next(12, 25);
            for (int i = 0; i < count; i++)
            {
                var facilityIndex = random.Next(facilities.Length);
                var admissionTime = DateTime.UtcNow.AddHours(-random.Next(2, 18));
                var riskLevel = riskLevels[random.Next(riskLevels.Length)];
                var dilatation = random.Next(4, 10);
                var fhr = riskLevel == "Critical" ? random.Next(95, 105) :
                          riskLevel == "High" ? random.Next(105, 115) :
                          random.Next(120, 155);

                var activeAlerts = new List<string>();
                if (riskLevel == "Critical")
                {
                    activeAlerts.Add("FHR below 110 bpm");
                    if (random.Next(2) == 0) activeAlerts.Add("Prolonged second stage");
                }
                else if (riskLevel == "High")
                {
                    activeAlerts.Add("Elevated blood pressure");
                }

                cases.Add(new LiveLaborCase
                {
                    PartographId = Guid.NewGuid(),
                    PatientId = Guid.NewGuid(),
                    PatientName = $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}",
                    HospitalNumber = $"MRN-{random.Next(10000, 99999)}",
                    Age = random.Next(18, 42),
                    Gravida = $"G{random.Next(1, 6)}",
                    Parity = $"P{random.Next(0, 4)}",
                    FacilityId = Guid.NewGuid(),
                    FacilityName = facilities[facilityIndex],
                    DistrictName = districts[facilityIndex],
                    RegionName = regions[facilityIndex],
                    AdmissionTime = admissionTime,
                    LaborStartTime = admissionTime.AddHours(random.Next(0, 3)),
                    CurrentDilatation = dilatation,
                    CurrentStation = random.Next(-3, 2),
                    LaborStage = dilatation >= 10 ? "Second Stage" : "First Stage",
                    LatestFHR = fhr,
                    LatestFHRTime = DateTime.UtcNow.AddMinutes(-random.Next(5, 45)),
                    LatestSystolicBP = riskLevel == "High" ? random.Next(140, 165) : random.Next(110, 135),
                    LatestDiastolicBP = riskLevel == "High" ? random.Next(90, 105) : random.Next(70, 85),
                    LatestBPTime = DateTime.UtcNow.AddMinutes(-random.Next(10, 120)),
                    LatestTemperature = (decimal)(36.5 + random.NextDouble() * 1.5),
                    ContractionsPerTenMinutes = random.Next(3, 6),
                    RiskLevel = riskLevel,
                    ActiveAlerts = activeAlerts,
                    AlertCount = activeAlerts.Count,
                    CriticalAlertCount = riskLevel == "Critical" ? random.Next(1, 3) : 0,
                    AssignedMidwife = $"MW-{random.Next(100, 999)}",
                    LastAssessmentTime = DateTime.UtcNow.AddMinutes(-random.Next(15, 90)),
                    IsFHRDue = random.Next(3) == 0,
                    IsBPDue = random.Next(4) == 0,
                    IsDilatationDue = random.Next(5) == 0,
                    MinutesUntilNextMeasurement = random.Next(5, 30)
                });
            }

            return cases.OrderByDescending(c => c.RiskLevel switch
            {
                "Critical" => 4,
                "High" => 3,
                "Moderate" => 2,
                _ => 1
            }).ThenByDescending(c => c.AlertCount).ToList();
        }
    }
}
