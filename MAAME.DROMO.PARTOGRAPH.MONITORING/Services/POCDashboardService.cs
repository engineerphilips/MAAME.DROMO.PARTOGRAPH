using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public interface IPOCDashboardService
    {
        Task<POCDashboardData> GetDashboardDataAsync();
        Task<POCProgress> GetPOCProgressAsync(Guid? facilityId = null, Guid? districtId = null, Guid? regionId = null);
        Task<List<POCProgress>> GetPOCProgressHistoryAsync(DateTime startDate, DateTime endDate);
        Task<POCBaseline> GetBaselineAsync(Guid? facilityId = null);
        Task<POCBaseline> UpdateBaselineAsync(POCBaseline baseline);
        Task<List<UserSurvey>> GetActiveSurveysAsync();
        Task<SurveyResponsesSummary> GetSurveyResponsesSummaryAsync(Guid surveyId);
        Task SeedDefaultSurveyAsync();
    }

    public class POCDashboardService : IPOCDashboardService
    {
        private readonly HttpClient _httpClient;

        public POCDashboardService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<POCDashboardData> GetDashboardDataAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<POCDashboardData>("api/poc/dashboard");
                return response ?? GetMockDashboardData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting POC dashboard data: {ex.Message}");
                return GetMockDashboardData();
            }
        }

        public async Task<POCProgress> GetPOCProgressAsync(Guid? facilityId = null, Guid? districtId = null, Guid? regionId = null)
        {
            try
            {
                var query = new List<string>();
                if (facilityId.HasValue) query.Add($"facilityId={facilityId}");
                if (districtId.HasValue) query.Add($"districtId={districtId}");
                if (regionId.HasValue) query.Add($"regionId={regionId}");

                var queryString = query.Any() ? "?" + string.Join("&", query) : "";
                var response = await _httpClient.GetFromJsonAsync<POCProgress>($"api/poc/progress{queryString}");
                return response ?? GetMockProgress();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting POC progress: {ex.Message}");
                return GetMockProgress();
            }
        }

        public async Task<List<POCProgress>> GetPOCProgressHistoryAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<POCProgress>>(
                    $"api/poc/progress/history?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
                return response ?? GetMockProgressHistory();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting POC progress history: {ex.Message}");
                return GetMockProgressHistory();
            }
        }

        public async Task<POCBaseline> GetBaselineAsync(Guid? facilityId = null)
        {
            try
            {
                var query = facilityId.HasValue ? $"?facilityId={facilityId}" : "";
                var response = await _httpClient.GetFromJsonAsync<POCBaseline>($"api/poc/baseline{query}");
                return response ?? GetMockBaseline();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting baseline: {ex.Message}");
                return GetMockBaseline();
            }
        }

        public async Task<POCBaseline> UpdateBaselineAsync(POCBaseline baseline)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/poc/baseline", baseline);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<POCBaseline>() ?? baseline;
                }
                return baseline;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating baseline: {ex.Message}");
                return baseline;
            }
        }

        public async Task<List<UserSurvey>> GetActiveSurveysAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<UserSurvey>>("api/poc/surveys");
                return response ?? new List<UserSurvey>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting active surveys: {ex.Message}");
                return new List<UserSurvey>();
            }
        }

        public async Task<SurveyResponsesSummary> GetSurveyResponsesSummaryAsync(Guid surveyId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<SurveyResponsesSummary>($"api/poc/surveys/{surveyId}/responses/summary");
                return response ?? new SurveyResponsesSummary();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting survey responses summary: {ex.Message}");
                return new SurveyResponsesSummary();
            }
        }

        public async Task SeedDefaultSurveyAsync()
        {
            try
            {
                await _httpClient.PostAsync("api/poc/surveys/seed-default", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding default survey: {ex.Message}");
            }
        }

        #region Mock Data for Development

        private POCDashboardData GetMockDashboardData()
        {
            return new POCDashboardData
            {
                Summary = GetMockProgress(),
                Adoption = new AdoptionMetricsDto
                {
                    TotalHealthcareWorkers = 250,
                    ActivePartographUsers = 145,
                    AdoptionRate = 58.0m,
                    Target = 70.0m,
                    RegularUsers = 98
                },
                Satisfaction = new SatisfactionMetricsDto
                {
                    TotalResponses = 180,
                    OverallScore = 3.8m,
                    Target = 4.0m,
                    EaseOfUseScore = 4.1m,
                    WorkflowImpactScore = 3.6m,
                    PerceivedBenefitsScore = 3.9m
                },
                Reporting = new ReportingMetricsDto
                {
                    TotalEmergencyCases = 85,
                    ReportedWithinTarget = 58,
                    ReportingRate = 68.2m,
                    Target = 70.0m,
                    AverageReportingTimeMinutes = 24.5m
                },
                Complications = new ComplicationsMetricsDto
                {
                    TotalDeliveries = 1250,
                    TotalComplications = 112,
                    CurrentComplicationRate = 8.96m,
                    BaselineComplicationRate = 11.2m,
                    ReductionPercent = 20.0m,
                    Target = 15.0m,
                    PPHCases = 45,
                    ObstructedLaborCases = 28,
                    BirthAsphyxiaCases = 22,
                    EclampsiaCases = 17
                },
                ResponseTime = new ResponseTimeMetricsDto
                {
                    TotalEmergencyReferrals = 85,
                    CurrentAverageMinutes = 62.5m,
                    BaselineAverageMinutes = 95.0m,
                    ReductionPercent = 34.2m,
                    Target = 30.0m
                },
                Baseline = GetMockBaseline(),
                GeneratedAt = DateTime.UtcNow
            };
        }

        private POCProgress GetMockProgress()
        {
            return new POCProgress
            {
                ID = Guid.NewGuid(),
                SnapshotDate = DateTime.UtcNow.Date,
                PeriodType = "Current",

                // POC 1: Adoption
                TotalHealthcareWorkers = 250,
                ActivePartographUsers = 145,
                AdoptionRate = 58.0m,
                AdoptionTarget = 70.0m,

                // POC 2: Satisfaction
                TotalSurveyResponses = 180,
                AverageSatisfactionScore = 3.8m,
                SatisfactionTarget = 4.0m,
                EaseOfUseAverage = 4.1m,
                WorkflowImpactAverage = 3.6m,
                PerceivedBenefitsAverage = 3.9m,

                // POC 3: Reporting
                TotalEmergencyCases = 85,
                EmergenciesReportedWithin30Min = 58,
                RealTimeReportingRate = 68.2m,
                ReportingTarget = 70.0m,
                AverageReportingTimeMinutes = 24.5m,

                // POC 4: Complications
                TotalDeliveries = 1250,
                TotalComplications = 112,
                ComplicationRate = 8.96m,
                BaselineComplicationRate = 11.2m,
                ComplicationReductionPercent = 20.0m,
                ComplicationReductionTarget = 15.0m,
                PPHCases = 45,
                ObstructedLaborCases = 28,
                BirthAsphyxiaCases = 22,
                EclampsiaCases = 17,

                // POC 5: Response Time
                TotalEmergencyReferrals = 85,
                AverageTimeToReferralMinutes = 62.5m,
                BaselineTimeToReferralMinutes = 95.0m,
                ResponseTimeReductionPercent = 34.2m,
                ResponseTimeReductionTarget = 30.0m,

                // Overall
                TargetsMet = 3,
                TotalTargets = 5,
                OverallPOCProgress = 60.0m
            };
        }

        private List<POCProgress> GetMockProgressHistory()
        {
            var history = new List<POCProgress>();
            var random = new Random();

            for (int i = 8; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                history.Add(new POCProgress
                {
                    SnapshotDate = date,
                    PeriodType = "Monthly",
                    AdoptionRate = 30 + (i * 3.5m) + (decimal)(random.NextDouble() * 5),
                    AverageSatisfactionScore = 3.0m + (i * 0.1m) + (decimal)(random.NextDouble() * 0.2),
                    RealTimeReportingRate = 40 + (i * 3.5m) + (decimal)(random.NextDouble() * 5),
                    ComplicationReductionPercent = (i * 2.5m) + (decimal)(random.NextDouble() * 3),
                    ResponseTimeReductionPercent = (i * 4m) + (decimal)(random.NextDouble() * 3),
                    TargetsMet = Math.Min(5, i / 2),
                    TotalTargets = 5
                });
            }

            return history;
        }

        private POCBaseline GetMockBaseline()
        {
            return new POCBaseline
            {
                ID = Guid.NewGuid(),
                BaselinePeriodStart = DateTime.UtcNow.AddYears(-1),
                BaselinePeriodEnd = DateTime.UtcNow.AddMonths(-1),
                DataSource = "Historical facility records (2024)",
                BaselineComplicationRate = 11.2m,
                BaselinePPHCases = 52,
                BaselineObstructedLaborCases = 35,
                BaselineBirthAsphyxiaCases = 28,
                BaselineTotalDeliveries = 1150,
                BaselineAverageTimeToReferralMinutes = 95.0m,
                BaselineTotalReferrals = 92,
                IsApproved = true,
                ApprovedBy = "Dr. Kwame Asante",
                ApprovedDate = DateTime.UtcNow.AddMonths(-8)
            };
        }

        #endregion
    }

    #region DTOs

    public class POCDashboardData
    {
        public POCProgress Summary { get; set; } = new();
        public AdoptionMetricsDto Adoption { get; set; } = new();
        public SatisfactionMetricsDto Satisfaction { get; set; } = new();
        public ReportingMetricsDto Reporting { get; set; } = new();
        public ComplicationsMetricsDto Complications { get; set; } = new();
        public ResponseTimeMetricsDto ResponseTime { get; set; } = new();
        public POCBaseline Baseline { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class AdoptionMetricsDto
    {
        public int TotalHealthcareWorkers { get; set; }
        public int ActivePartographUsers { get; set; }
        public decimal AdoptionRate { get; set; }
        public decimal Target { get; set; }
        public bool TargetMet => AdoptionRate >= Target;
        public int RegularUsers { get; set; }
    }

    public class SatisfactionMetricsDto
    {
        public int TotalResponses { get; set; }
        public decimal OverallScore { get; set; }
        public decimal Target { get; set; }
        public bool TargetMet => OverallScore >= Target;
        public decimal? EaseOfUseScore { get; set; }
        public decimal? WorkflowImpactScore { get; set; }
        public decimal? PerceivedBenefitsScore { get; set; }
    }

    public class ReportingMetricsDto
    {
        public int TotalEmergencyCases { get; set; }
        public int ReportedWithinTarget { get; set; }
        public decimal ReportingRate { get; set; }
        public decimal Target { get; set; }
        public bool TargetMet => ReportingRate >= Target;
        public decimal AverageReportingTimeMinutes { get; set; }
    }

    public class ComplicationsMetricsDto
    {
        public int TotalDeliveries { get; set; }
        public int TotalComplications { get; set; }
        public decimal CurrentComplicationRate { get; set; }
        public decimal BaselineComplicationRate { get; set; }
        public decimal ReductionPercent { get; set; }
        public decimal Target { get; set; }
        public bool TargetMet => ReductionPercent >= Target;
        public int PPHCases { get; set; }
        public int ObstructedLaborCases { get; set; }
        public int BirthAsphyxiaCases { get; set; }
        public int EclampsiaCases { get; set; }
    }

    public class ResponseTimeMetricsDto
    {
        public int TotalEmergencyReferrals { get; set; }
        public decimal CurrentAverageMinutes { get; set; }
        public decimal BaselineAverageMinutes { get; set; }
        public decimal ReductionPercent { get; set; }
        public decimal Target { get; set; }
        public bool TargetMet => ReductionPercent >= Target;
    }

    public class SurveyResponsesSummary
    {
        public int TotalResponses { get; set; }
        public double AverageOverallScore { get; set; }
        public double? AverageEaseOfUse { get; set; }
        public double? AverageWorkflowImpact { get; set; }
        public double? AveragePerceivedBenefits { get; set; }
    }

    #endregion
}
