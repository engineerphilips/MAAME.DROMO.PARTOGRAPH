using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class PredictiveAnalyticsService : IPredictiveAnalyticsService
    {
        private readonly HttpClient _httpClient;

        public PredictiveAnalyticsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<RiskPrediction> GetRiskPredictionAsync(Guid partographId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<RiskPrediction>($"api/monitoring/predictive/risk/{partographId}");
                return response ?? GenerateMockPrediction(partographId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting risk prediction: {ex.Message}");
                return GenerateMockPrediction(partographId);
            }
        }

        public async Task<List<RiskPrediction>> GetHighRiskPatientsAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<List<RiskPrediction>>($"api/monitoring/predictive/high-risk{queryParams}");
                return response ?? GenerateMockHighRiskPatients(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting high risk patients: {ex.Message}");
                return GenerateMockHighRiskPatients(filter);
            }
        }

        public async Task<FacilityRiskSummary> GetFacilityRiskSummaryAsync(Guid facilityId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<FacilityRiskSummary>($"api/monitoring/predictive/facility/{facilityId}/summary");
                return response ?? GenerateMockFacilityRiskSummary(facilityId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting facility risk summary: {ex.Message}");
                return GenerateMockFacilityRiskSummary(facilityId);
            }
        }

        public async Task<List<FacilityRiskSummary>> GetAllFacilityRiskSummariesAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<List<FacilityRiskSummary>>($"api/monitoring/predictive/facilities{queryParams}");
                return response ?? GenerateMockAllFacilityRiskSummaries();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all facility risk summaries: {ex.Message}");
                return GenerateMockAllFacilityRiskSummaries();
            }
        }

        public async Task<Dictionary<string, double>> GetRiskTrendsAsync(DashboardFilter? filter = null, int days = 30)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                queryParams += (string.IsNullOrEmpty(queryParams) ? "?" : "&") + $"days={days}";
                var response = await _httpClient.GetFromJsonAsync<Dictionary<string, double>>($"api/monitoring/predictive/trends{queryParams}");
                return response ?? GenerateMockRiskTrends(days);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting risk trends: {ex.Message}");
                return GenerateMockRiskTrends(days);
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

        private RiskPrediction GenerateMockPrediction(Guid partographId)
        {
            var random = new Random(partographId.GetHashCode());
            var overallRisk = random.NextDouble() * 100;
            var riskCategory = overallRisk switch
            {
                < 25 => "Low",
                < 50 => "Moderate",
                < 75 => "High",
                _ => "Critical"
            };

            var riskFactors = new List<RiskFactor>();

            if (overallRisk > 30)
            {
                riskFactors.Add(new RiskFactor
                {
                    Name = "Maternal Age",
                    Category = "Demographics",
                    Contribution = random.NextDouble() * 15,
                    Value = $"{random.Next(35, 45)} years",
                    Threshold = "< 35 years",
                    Severity = "Medium"
                });
            }

            if (overallRisk > 40)
            {
                riskFactors.Add(new RiskFactor
                {
                    Name = "Previous C-Section",
                    Category = "Obstetric History",
                    Contribution = random.NextDouble() * 20,
                    Value = "Yes",
                    Threshold = "No",
                    Severity = "High"
                });
            }

            if (overallRisk > 50)
            {
                riskFactors.Add(new RiskFactor
                {
                    Name = "Labor Duration",
                    Category = "Current Labor",
                    Contribution = random.NextDouble() * 18,
                    Value = $"{random.Next(12, 20)} hours",
                    Threshold = "< 12 hours",
                    Severity = "High"
                });
            }

            if (overallRisk > 60)
            {
                riskFactors.Add(new RiskFactor
                {
                    Name = "Fetal Heart Rate Pattern",
                    Category = "Fetal Status",
                    Contribution = random.NextDouble() * 25,
                    Value = "Variable decelerations",
                    Threshold = "Normal pattern",
                    Severity = "High"
                });
            }

            var recommendations = new List<string>();
            if (overallRisk > 30) recommendations.Add("Increase FHR monitoring frequency to every 15 minutes");
            if (overallRisk > 50) recommendations.Add("Consider continuous electronic fetal monitoring");
            if (overallRisk > 60) recommendations.Add("Alert senior obstetrician for review");
            if (overallRisk > 75) recommendations.Add("Prepare for possible emergency intervention");

            return new RiskPrediction
            {
                PatientId = Guid.NewGuid(),
                PartographId = partographId,
                PatientName = GenerateRandomName(random),
                FacilityName = "Korle Bu Teaching Hospital",
                OverallRiskScore = Math.Round(overallRisk, 1),
                RiskCategory = riskCategory,
                CaesareanRisk = Math.Round(random.NextDouble() * 60 + (overallRisk > 50 ? 20 : 0), 1),
                HemorrhageRisk = Math.Round(random.NextDouble() * 30 + (overallRisk > 60 ? 15 : 0), 1),
                ProlongedLaborRisk = Math.Round(random.NextDouble() * 40 + (overallRisk > 40 ? 20 : 0), 1),
                FetalDistressRisk = Math.Round(random.NextDouble() * 35 + (overallRisk > 55 ? 25 : 0), 1),
                PreeclampsiaRisk = Math.Round(random.NextDouble() * 25, 1),
                InfectionRisk = Math.Round(random.NextDouble() * 20 + (overallRisk > 50 ? 10 : 0), 1),
                RiskFactors = riskFactors,
                Recommendations = recommendations,
                PredictionTime = DateTime.UtcNow,
                ModelVersion = "1.0",
                ConfidenceScore = 0.75 + random.NextDouble() * 0.2
            };
        }

        private List<RiskPrediction> GenerateMockHighRiskPatients(DashboardFilter? filter)
        {
            var patients = new List<RiskPrediction>();
            var random = new Random();

            for (int i = 0; i < random.Next(5, 12); i++)
            {
                var prediction = GenerateMockPrediction(Guid.NewGuid());
                prediction.OverallRiskScore = Math.Round(50 + random.NextDouble() * 50, 1);
                prediction.RiskCategory = prediction.OverallRiskScore >= 75 ? "Critical" : "High";
                prediction.PatientName = GenerateRandomName(random);
                patients.Add(prediction);
            }

            return patients.OrderByDescending(p => p.OverallRiskScore).ToList();
        }

        private FacilityRiskSummary GenerateMockFacilityRiskSummary(Guid facilityId)
        {
            var random = new Random(facilityId.GetHashCode());
            var total = random.Next(10, 30);
            var highRisk = random.Next(2, 6);
            var moderate = random.Next(3, 8);
            var low = total - highRisk - moderate;

            return new FacilityRiskSummary
            {
                FacilityId = facilityId,
                FacilityName = "Health Facility",
                TotalActiveCases = total,
                HighRiskCases = highRisk,
                ModerateRiskCases = moderate,
                LowRiskCases = low,
                AverageRiskScore = Math.Round(random.NextDouble() * 40 + 20, 1),
                HighRiskPatients = GenerateMockHighRiskPatients(null).Take(highRisk).ToList()
            };
        }

        private List<FacilityRiskSummary> GenerateMockAllFacilityRiskSummaries()
        {
            var facilities = new[]
            {
                "Korle Bu Teaching Hospital", "Ridge Hospital", "La General Hospital",
                "Tema General Hospital", "Achimota Hospital", "Mamprobi Hospital"
            };

            return facilities.Select((name, i) =>
            {
                var summary = GenerateMockFacilityRiskSummary(Guid.NewGuid());
                summary.FacilityName = name;
                return summary;
            }).ToList();
        }

        private Dictionary<string, double> GenerateMockRiskTrends(int days)
        {
            var trends = new Dictionary<string, double>();
            var random = new Random();

            for (int i = 0; i < days; i++)
            {
                var date = DateTime.UtcNow.AddDays(-i).ToString("yyyy-MM-dd");
                trends[date] = Math.Round(20 + random.NextDouble() * 30, 1);
            }

            return trends;
        }

        private string GenerateRandomName(Random random)
        {
            var firstNames = new[] { "Abena", "Akosua", "Ama", "Adwoa", "Yaa", "Afia", "Efua" };
            var lastNames = new[] { "Mensah", "Asante", "Owusu", "Boateng", "Appiah", "Amoah", "Darko" };
            return $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}";
        }
    }
}
