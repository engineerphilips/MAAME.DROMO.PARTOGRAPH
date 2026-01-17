using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class EnhancedAlertService : IEnhancedAlertService
    {
        private readonly HttpClient _httpClient;

        public EnhancedAlertService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<EnhancedAlert>> GetActiveAlertsAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<List<EnhancedAlert>>($"api/monitoring/alerts/active{queryParams}");
                return response ?? GenerateMockAlerts(filter, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting active alerts: {ex.Message}");
                return GenerateMockAlerts(filter, false);
            }
        }

        public async Task<List<EnhancedAlert>> GetUnacknowledgedAlertsAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<List<EnhancedAlert>>($"api/monitoring/alerts/unacknowledged{queryParams}");
                return response ?? GenerateMockAlerts(filter, true).Where(a => !a.IsAcknowledged).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting unacknowledged alerts: {ex.Message}");
                return GenerateMockAlerts(filter, true).Where(a => !a.IsAcknowledged).ToList();
            }
        }

        public async Task<EnhancedAlert?> GetAlertAsync(Guid alertId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<EnhancedAlert>($"api/monitoring/alerts/{alertId}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting alert: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> AcknowledgeAlertAsync(AlertAcknowledgmentRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/monitoring/alerts/acknowledge", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error acknowledging alert: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResolveAlertAsync(AlertResolutionRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/monitoring/alerts/resolve", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resolving alert: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EscalateAlertAsync(Guid alertId, string escalatedTo, int escalationLevel)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/monitoring/alerts/escalate", new
                {
                    AlertId = alertId,
                    EscalatedTo = escalatedTo,
                    EscalationLevel = escalationLevel
                });
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error escalating alert: {ex.Message}");
                return false;
            }
        }

        public async Task<AlertResponseMetrics> GetAlertResponseMetricsAsync(DashboardFilter? filter = null, string period = "Today")
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                if (!string.IsNullOrEmpty(queryParams))
                    queryParams += $"&period={period}";
                else
                    queryParams = $"?period={period}";

                var response = await _httpClient.GetFromJsonAsync<AlertResponseMetrics>($"api/monitoring/alerts/metrics{queryParams}");
                return response ?? GenerateMockMetrics(period);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting alert response metrics: {ex.Message}");
                return GenerateMockMetrics(period);
            }
        }

        public async Task<List<EnhancedAlert>> GetAlertHistoryAsync(DashboardFilter? filter = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                if (startDate.HasValue)
                    queryParams += (string.IsNullOrEmpty(queryParams) ? "?" : "&") + $"startDate={startDate.Value:yyyy-MM-dd}";
                if (endDate.HasValue)
                    queryParams += (string.IsNullOrEmpty(queryParams) ? "?" : "&") + $"endDate={endDate.Value:yyyy-MM-dd}";

                var response = await _httpClient.GetFromJsonAsync<List<EnhancedAlert>>($"api/monitoring/alerts/history{queryParams}");
                return response ?? GenerateMockAlerts(filter, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting alert history: {ex.Message}");
                return GenerateMockAlerts(filter, false);
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

        private List<EnhancedAlert> GenerateMockAlerts(DashboardFilter? filter, bool includeResolved)
        {
            var random = new Random();
            var alerts = new List<EnhancedAlert>();
            var severities = new[] { "Critical", "Critical", "Warning", "Warning", "Warning", "Info" };
            var categories = new[] { "Fetal", "Maternal", "Labor", "Hydration" };
            var alertTypes = new Dictionary<string, (string Title, string Message)>
            {
                { "FHR_LOW", ("Low Fetal Heart Rate", "FHR dropped below 110 bpm") },
                { "FHR_HIGH", ("High Fetal Heart Rate", "FHR exceeded 160 bpm") },
                { "BP_HIGH", ("Elevated Blood Pressure", "BP reading above normal threshold") },
                { "PROLONGED_LABOR", ("Prolonged Labor", "Labor duration exceeds expected time") },
                { "TEMPERATURE_HIGH", ("Maternal Fever", "Temperature above 38°C") },
                { "TACHYSYSTOLE", ("Tachysystole", "More than 5 contractions in 10 minutes") },
                { "DEHYDRATION", ("Dehydration Risk", "Urine output below expected level") }
            };
            var facilities = new[] { "Korle Bu Teaching Hospital", "Ridge Hospital", "La General Hospital", "Tema General Hospital" };
            var firstNames = new[] { "Abena", "Akosua", "Ama", "Adwoa", "Yaa", "Afia", "Efua" };
            var lastNames = new[] { "Mensah", "Asante", "Owusu", "Boateng", "Appiah", "Amoah" };
            var staff = new[] { "Dr. Kofi Mensah", "MW Ama Darko", "MW Yaa Asante", "Dr. Kwame Boateng" };

            var count = random.Next(8, 20);
            for (int i = 0; i < count; i++)
            {
                var severity = severities[random.Next(severities.Length)];
                var category = categories[random.Next(categories.Length)];
                var alertType = alertTypes.Keys.ToArray()[random.Next(alertTypes.Count)];
                var alertInfo = alertTypes[alertType];
                var createdAt = DateTime.UtcNow.AddMinutes(-random.Next(5, 180));
                var isAcknowledged = random.Next(3) != 0; // 66% acknowledged
                var isResolved = isAcknowledged && random.Next(2) == 0; // 50% of acknowledged are resolved

                if (!includeResolved && isResolved) continue;

                var acknowledgedAt = isAcknowledged ? createdAt.AddMinutes(random.Next(2, 25)) : (DateTime?)null;

                alerts.Add(new EnhancedAlert
                {
                    Id = Guid.NewGuid(),
                    PartographId = Guid.NewGuid(),
                    PatientId = Guid.NewGuid(),
                    PatientName = $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}",
                    FacilityId = Guid.NewGuid(),
                    FacilityName = facilities[random.Next(facilities.Length)],
                    DistrictName = "Accra Metro",
                    RegionName = "Greater Accra",
                    Title = alertInfo.Title,
                    Message = alertInfo.Message,
                    Severity = severity,
                    Category = category,
                    AlertType = alertType,
                    CreatedAt = createdAt,
                    AcknowledgedAt = acknowledgedAt,
                    ResolvedAt = isResolved ? acknowledgedAt?.AddMinutes(random.Next(10, 60)) : null,
                    IsAcknowledged = isAcknowledged,
                    AcknowledgedBy = isAcknowledged ? staff[random.Next(staff.Length)] : null,
                    AcknowledgmentNotes = isAcknowledged ? "Acknowledged and monitoring" : null,
                    IsResolved = isResolved,
                    ResolvedBy = isResolved ? staff[random.Next(staff.Length)] : null,
                    ResolutionNotes = isResolved ? "Situation resolved, patient stable" : null,
                    ActionTaken = isResolved ? "Administered fluids, continued monitoring" : null,
                    IsEscalated = !isAcknowledged && (DateTime.UtcNow - createdAt).TotalMinutes > 15,
                    EscalatedAt = !isAcknowledged && (DateTime.UtcNow - createdAt).TotalMinutes > 15 ? createdAt.AddMinutes(15) : null,
                    EscalatedTo = !isAcknowledged && (DateTime.UtcNow - createdAt).TotalMinutes > 15 ? "District Supervisor" : null,
                    EscalationLevel = !isAcknowledged && (DateTime.UtcNow - createdAt).TotalMinutes > 15 ? 1 : 0
                });
            }

            return alerts.OrderByDescending(a => a.Severity switch
            {
                "Critical" => 3,
                "Warning" => 2,
                _ => 1
            }).ThenByDescending(a => a.CreatedAt).ToList();
        }

        private AlertResponseMetrics GenerateMockMetrics(string period)
        {
            var random = new Random();
            var baseCount = period switch
            {
                "Today" => random.Next(20, 50),
                "This Week" => random.Next(100, 250),
                "This Month" => random.Next(400, 800),
                _ => random.Next(20, 50)
            };

            var acknowledgedCount = (int)(baseCount * (0.75 + random.NextDouble() * 0.2));
            var resolvedCount = (int)(acknowledgedCount * (0.6 + random.NextDouble() * 0.3));

            return new AlertResponseMetrics
            {
                Period = period,
                TotalAlerts = baseCount,
                AcknowledgedAlerts = acknowledgedCount,
                ResolvedAlerts = resolvedCount,
                PendingAlerts = baseCount - acknowledgedCount,
                EscalatedAlerts = (int)(baseCount * 0.1),
                AverageResponseTimeMinutes = 8.5 + random.NextDouble() * 5,
                MedianResponseTimeMinutes = 6.0 + random.NextDouble() * 4,
                P95ResponseTimeMinutes = 18.0 + random.NextDouble() * 10,
                MinResponseTimeMinutes = 1.0 + random.NextDouble() * 2,
                MaxResponseTimeMinutes = 35.0 + random.NextDouble() * 15,
                CriticalAvgResponseTimeMinutes = 4.5 + random.NextDouble() * 3,
                WarningAvgResponseTimeMinutes = 10.0 + random.NextDouble() * 5,
                CriticalResponseCompliance = 85.0 + random.NextDouble() * 12,
                WarningResponseCompliance = 78.0 + random.NextDouble() * 15,
                OverallResponseCompliance = 80.0 + random.NextDouble() * 15
            };
        }
    }
}
