using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class ReportVisualizationService : IReportVisualizationService
    {
        private readonly HttpClient _httpClient;

        public ReportVisualizationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ReportVisualization> GetDeliveryReportAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<ReportVisualization>($"api/monitoring/reports/deliveries{queryParams}");
                return response ?? GenerateMockDeliveryReport(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting delivery report: {ex.Message}");
                return GenerateMockDeliveryReport(filter);
            }
        }

        public async Task<ReportVisualization> GetComplicationReportAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<ReportVisualization>($"api/monitoring/reports/complications{queryParams}");
                return response ?? GenerateMockComplicationReport(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting complication report: {ex.Message}");
                return GenerateMockComplicationReport(filter);
            }
        }

        public async Task<ReportVisualization> GetOutcomeReportAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<ReportVisualization>($"api/monitoring/reports/outcomes{queryParams}");
                return response ?? GenerateMockOutcomeReport(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting outcome report: {ex.Message}");
                return GenerateMockOutcomeReport(filter);
            }
        }

        public async Task<ReportVisualization> GetAlertResponseReportAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<ReportVisualization>($"api/monitoring/reports/alert-response{queryParams}");
                return response ?? GenerateMockAlertResponseReport(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting alert response report: {ex.Message}");
                return GenerateMockAlertResponseReport(filter);
            }
        }

        public async Task<ReportVisualization> GetWHOComplianceReportAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<ReportVisualization>($"api/monitoring/reports/who-compliance{queryParams}");
                return response ?? GenerateMockWHOComplianceReport(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting WHO compliance report: {ex.Message}");
                return GenerateMockWHOComplianceReport(filter);
            }
        }

        public async Task<ReportVisualization> GetStaffPerformanceReportAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<ReportVisualization>($"api/monitoring/reports/staff-performance{queryParams}");
                return response ?? GenerateMockStaffPerformanceReport(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting staff performance report: {ex.Message}");
                return GenerateMockStaffPerformanceReport(filter);
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
            if (filter.StartDate.HasValue)
                parameters.Add($"startDate={filter.StartDate.Value:yyyy-MM-dd}");
            if (filter.EndDate.HasValue)
                parameters.Add($"endDate={filter.EndDate.Value:yyyy-MM-dd}");

            return parameters.Count > 0 ? "?" + string.Join("&", parameters) : "";
        }

        private ReportVisualization GenerateMockDeliveryReport(DashboardFilter? filter)
        {
            var random = new Random();
            var totalDeliveries = random.Next(200, 500);

            return new ReportVisualization
            {
                ReportType = "Deliveries",
                Title = "Monthly Delivery Report",
                GeneratedAt = DateTime.UtcNow,
                Filter = filter,
                SummaryStats = new Dictionary<string, object>
                {
                    { "TotalDeliveries", totalDeliveries },
                    { "NormalVaginal", (int)(totalDeliveries * 0.72) },
                    { "AssistedVaginal", (int)(totalDeliveries * 0.08) },
                    { "Caesarean", (int)(totalDeliveries * 0.20) },
                    { "CaesareanRate", 20.0 },
                    { "ChangeFromLastMonth", random.Next(-5, 10) }
                },
                PieChartData = new List<ChartDataPoint>
                {
                    new() { Label = "Normal Vaginal", Value = 72, Color = "#28a745" },
                    new() { Label = "Assisted Vaginal", Value = 8, Color = "#17a2b8" },
                    new() { Label = "Elective C-Section", Value = 8, Color = "#ffc107" },
                    new() { Label = "Emergency C-Section", Value = 12, Color = "#dc3545" }
                },
                LineChartData = GenerateTrendData("Deliveries", 30),
                TableHeaders = new List<string> { "Facility", "Total", "Normal", "Assisted", "C-Section", "Rate" },
                TableData = GenerateFacilityDeliveryData()
            };
        }

        private ReportVisualization GenerateMockComplicationReport(DashboardFilter? filter)
        {
            var random = new Random();

            return new ReportVisualization
            {
                ReportType = "Complications",
                Title = "Maternal & Neonatal Complications Report",
                GeneratedAt = DateTime.UtcNow,
                Filter = filter,
                SummaryStats = new Dictionary<string, object>
                {
                    { "TotalComplications", random.Next(30, 80) },
                    { "MaternalComplications", random.Next(15, 40) },
                    { "NeonatalComplications", random.Next(10, 35) },
                    { "ComplicationRate", Math.Round(5 + random.NextDouble() * 5, 1) },
                    { "MostCommonType", "Postpartum Hemorrhage" }
                },
                BarChartData = new List<ChartDataPoint>
                {
                    new() { Label = "Postpartum Hemorrhage", Value = random.Next(10, 25), Color = "#dc3545" },
                    new() { Label = "Perineal Tears", Value = random.Next(8, 18), Color = "#fd7e14" },
                    new() { Label = "Hypertensive Disorders", Value = random.Next(5, 15), Color = "#ffc107" },
                    new() { Label = "Prolonged Labor", Value = random.Next(5, 12), Color = "#6c757d" },
                    new() { Label = "Fetal Distress", Value = random.Next(4, 10), Color = "#17a2b8" },
                    new() { Label = "Cord Prolapse", Value = random.Next(1, 5), Color = "#6610f2" }
                },
                LineChartData = GenerateTrendData("Complications", 30),
                TableHeaders = new List<string> { "Complication", "Count", "Severity", "Outcome", "Action Taken" },
                TableData = GenerateComplicationTableData()
            };
        }

        private ReportVisualization GenerateMockOutcomeReport(DashboardFilter? filter)
        {
            var random = new Random();
            var totalBirths = random.Next(200, 500);

            return new ReportVisualization
            {
                ReportType = "Outcomes",
                Title = "Birth Outcomes Report",
                GeneratedAt = DateTime.UtcNow,
                Filter = filter,
                SummaryStats = new Dictionary<string, object>
                {
                    { "TotalBirths", totalBirths },
                    { "LiveBirths", (int)(totalBirths * 0.98) },
                    { "Stillbirths", (int)(totalBirths * 0.02) },
                    { "NeonatalDeaths", random.Next(1, 5) },
                    { "AverageAPGAR", Math.Round(7.5 + random.NextDouble() * 1.5, 1) },
                    { "AverageBirthWeight", Math.Round(2800 + random.NextDouble() * 600, 0) }
                },
                PieChartData = new List<ChartDataPoint>
                {
                    new() { Label = "Normal Weight (2500-4000g)", Value = 75, Color = "#28a745" },
                    new() { Label = "Low Birth Weight (<2500g)", Value = 15, Color = "#ffc107" },
                    new() { Label = "Very Low Birth Weight (<1500g)", Value = 5, Color = "#dc3545" },
                    new() { Label = "Macrosomia (>4000g)", Value = 5, Color = "#17a2b8" }
                },
                BarChartData = new List<ChartDataPoint>
                {
                    new() { Label = "APGAR 7-10", Value = 85, Color = "#28a745" },
                    new() { Label = "APGAR 4-6", Value = 10, Color = "#ffc107" },
                    new() { Label = "APGAR 0-3", Value = 5, Color = "#dc3545" }
                },
                LineChartData = GenerateTrendData("LiveBirths", 30),
                TableHeaders = new List<string> { "Metric", "This Month", "Last Month", "Change", "Status" },
                TableData = GenerateOutcomeTableData()
            };
        }

        private ReportVisualization GenerateMockAlertResponseReport(DashboardFilter? filter)
        {
            var random = new Random();

            return new ReportVisualization
            {
                ReportType = "AlertResponse",
                Title = "Alert Response Time Analysis",
                GeneratedAt = DateTime.UtcNow,
                Filter = filter,
                SummaryStats = new Dictionary<string, object>
                {
                    { "TotalAlerts", random.Next(100, 300) },
                    { "AcknowledgedAlerts", random.Next(80, 280) },
                    { "AverageResponseTimeMinutes", Math.Round(5 + random.NextDouble() * 10, 1) },
                    { "CriticalResponseTimeMinutes", Math.Round(3 + random.NextDouble() * 5, 1) },
                    { "ResponseCompliance", Math.Round(75 + random.NextDouble() * 20, 1) },
                    { "EscalatedAlerts", random.Next(5, 25) }
                },
                BarChartData = new List<ChartDataPoint>
                {
                    new() { Label = "< 5 min", Value = random.Next(40, 60), Color = "#28a745" },
                    new() { Label = "5-10 min", Value = random.Next(20, 35), Color = "#17a2b8" },
                    new() { Label = "10-15 min", Value = random.Next(10, 20), Color = "#ffc107" },
                    new() { Label = "15-30 min", Value = random.Next(5, 15), Color = "#fd7e14" },
                    new() { Label = "> 30 min", Value = random.Next(2, 10), Color = "#dc3545" }
                },
                LineChartData = new List<TimeSeriesData>
                {
                    GenerateTrendData("CriticalAlerts", 30).First(),
                    GenerateTrendData("WarningAlerts", 30).First()
                },
                TableHeaders = new List<string> { "Alert Type", "Count", "Avg Response", "Compliance", "Escalated" },
                TableData = GenerateAlertResponseTableData()
            };
        }

        private ReportVisualization GenerateMockWHOComplianceReport(DashboardFilter? filter)
        {
            var random = new Random();

            return new ReportVisualization
            {
                ReportType = "WHOCompliance",
                Title = "WHO 2020 Guidelines Compliance Report",
                GeneratedAt = DateTime.UtcNow,
                Filter = filter,
                SummaryStats = new Dictionary<string, object>
                {
                    { "OverallCompliance", Math.Round(70 + random.NextDouble() * 25, 1) },
                    { "FHRMonitoringCompliance", Math.Round(75 + random.NextDouble() * 20, 1) },
                    { "BPMonitoringCompliance", Math.Round(80 + random.NextDouble() * 15, 1) },
                    { "PartographCompleteness", Math.Round(65 + random.NextDouble() * 30, 1) },
                    { "ActiveLaborDefinitionCompliance", Math.Round(85 + random.NextDouble() * 12, 1) }
                },
                BarChartData = new List<ChartDataPoint>
                {
                    new() { Label = "FHR q30min", Value = random.Next(70, 95), Color = "#28a745" },
                    new() { Label = "BP q4h", Value = random.Next(75, 90), Color = "#17a2b8" },
                    new() { Label = "Temp q4h", Value = random.Next(60, 85), Color = "#6c757d" },
                    new() { Label = "Dilatation q4h", Value = random.Next(65, 88), Color = "#ffc107" },
                    new() { Label = "Active Labor 5cm", Value = random.Next(80, 95), Color = "#6610f2" },
                    new() { Label = "Alert/Action Lines", Value = random.Next(70, 90), Color = "#fd7e14" }
                },
                LineChartData = GenerateTrendData("Compliance", 30),
                TableHeaders = new List<string> { "Guideline", "Target", "Achieved", "Gap", "Trend" },
                TableData = GenerateWHOComplianceTableData()
            };
        }

        private ReportVisualization GenerateMockStaffPerformanceReport(DashboardFilter? filter)
        {
            var random = new Random();

            return new ReportVisualization
            {
                ReportType = "StaffPerformance",
                Title = "Staff Performance Report",
                GeneratedAt = DateTime.UtcNow,
                Filter = filter,
                SummaryStats = new Dictionary<string, object>
                {
                    { "TotalStaff", random.Next(20, 50) },
                    { "AverageDeliveriesPerStaff", Math.Round(8 + random.NextDouble() * 10, 1) },
                    { "AverageAlertResponseTime", Math.Round(6 + random.NextDouble() * 8, 1) },
                    { "DataQualityScore", Math.Round(70 + random.NextDouble() * 25, 1) },
                    { "TopPerformer", "MW Ama Darko" }
                },
                BarChartData = new List<ChartDataPoint>
                {
                    new() { Label = "MW Ama Darko", Value = 95, Color = "#28a745" },
                    new() { Label = "Dr. Kofi Mensah", Value = 92, Color = "#28a745" },
                    new() { Label = "MW Yaa Asante", Value = 88, Color = "#17a2b8" },
                    new() { Label = "MW Efua Boateng", Value = 85, Color = "#17a2b8" },
                    new() { Label = "Dr. Kwame Owusu", Value = 82, Color = "#ffc107" }
                },
                TableHeaders = new List<string> { "Staff", "Deliveries", "Response Time", "Quality Score", "Grade" },
                TableData = GenerateStaffPerformanceTableData()
            };
        }

        private List<TimeSeriesData> GenerateTrendData(string seriesName, int days)
        {
            var random = new Random();
            var baseValue = seriesName switch
            {
                "Deliveries" => random.Next(10, 20),
                "Complications" => random.Next(2, 5),
                "LiveBirths" => random.Next(10, 20),
                "CriticalAlerts" => random.Next(3, 8),
                "WarningAlerts" => random.Next(5, 12),
                "Compliance" => 70,
                _ => random.Next(5, 15)
            };

            var dataPoints = new List<TimeSeriesPoint>();
            for (int i = days - 1; i >= 0; i--)
            {
                dataPoints.Add(new TimeSeriesPoint
                {
                    Timestamp = DateTime.UtcNow.Date.AddDays(-i),
                    Value = baseValue + random.Next(-3, 4) + (seriesName == "Compliance" ? random.NextDouble() * 10 : 0)
                });
            }

            return new List<TimeSeriesData>
            {
                new TimeSeriesData
                {
                    SeriesName = seriesName,
                    Color = seriesName switch
                    {
                        "Deliveries" => "#007bff",
                        "Complications" => "#dc3545",
                        "LiveBirths" => "#28a745",
                        "CriticalAlerts" => "#dc3545",
                        "WarningAlerts" => "#ffc107",
                        "Compliance" => "#17a2b8",
                        _ => "#6c757d"
                    },
                    DataPoints = dataPoints
                }
            };
        }

        private List<Dictionary<string, object>> GenerateFacilityDeliveryData()
        {
            var random = new Random();
            var facilities = new[] { "Korle Bu", "Ridge Hospital", "La General", "Tema General", "Achimota" };

            return facilities.Select(f =>
            {
                var total = random.Next(30, 80);
                var normal = (int)(total * (0.65 + random.NextDouble() * 0.15));
                var assisted = (int)(total * (0.05 + random.NextDouble() * 0.08));
                var csection = total - normal - assisted;

                return new Dictionary<string, object>
                {
                    { "Facility", f },
                    { "Total", total },
                    { "Normal", normal },
                    { "Assisted", assisted },
                    { "C-Section", csection },
                    { "Rate", $"{Math.Round((double)csection / total * 100, 1)}%" }
                };
            }).ToList();
        }

        private List<Dictionary<string, object>> GenerateComplicationTableData()
        {
            return new List<Dictionary<string, object>>
            {
                new() { { "Complication", "Postpartum Hemorrhage" }, { "Count", 15 }, { "Severity", "High" }, { "Outcome", "All Recovered" }, { "Action Taken", "Uterotonics, Manual removal" } },
                new() { { "Complication", "Perineal Tears" }, { "Count", 12 }, { "Severity", "Medium" }, { "Outcome", "Repaired" }, { "Action Taken", "Surgical repair" } },
                new() { { "Complication", "Hypertensive Disorders" }, { "Count", 8 }, { "Severity", "High" }, { "Outcome", "Managed" }, { "Action Taken", "MgSO4, Antihypertensives" } },
                new() { { "Complication", "Prolonged Labor" }, { "Count", 6 }, { "Severity", "Medium" }, { "Outcome", "Delivered" }, { "Action Taken", "Augmentation, C-Section" } },
                new() { { "Complication", "Fetal Distress" }, { "Count", 5 }, { "Severity", "Critical" }, { "Outcome", "Emergency delivery" }, { "Action Taken", "Emergency C-Section" } }
            };
        }

        private List<Dictionary<string, object>> GenerateOutcomeTableData()
        {
            return new List<Dictionary<string, object>>
            {
                new() { { "Metric", "Live Births" }, { "This Month", 245 }, { "Last Month", 238 }, { "Change", "+3%" }, { "Status", "Good" } },
                new() { { "Metric", "Stillbirths" }, { "This Month", 5 }, { "Last Month", 6 }, { "Change", "-17%" }, { "Status", "Improving" } },
                new() { { "Metric", "Neonatal Deaths" }, { "This Month", 2 }, { "Last Month", 3 }, { "Change", "-33%" }, { "Status", "Improving" } },
                new() { { "Metric", "Average APGAR" }, { "This Month", 8.2 }, { "Last Month", 8.0 }, { "Change", "+2.5%" }, { "Status", "Good" } },
                new() { { "Metric", "Low Birth Weight" }, { "This Month", "12%" }, { "Last Month", "14%" }, { "Change", "-14%" }, { "Status", "Improving" } }
            };
        }

        private List<Dictionary<string, object>> GenerateAlertResponseTableData()
        {
            return new List<Dictionary<string, object>>
            {
                new() { { "Alert Type", "FHR Critical" }, { "Count", 25 }, { "Avg Response", "3.2 min" }, { "Compliance", "92%" }, { "Escalated", 2 } },
                new() { { "Alert Type", "BP Critical" }, { "Count", 18 }, { "Avg Response", "4.1 min" }, { "Compliance", "89%" }, { "Escalated", 3 } },
                new() { { "Alert Type", "Labor Warning" }, { "Count", 45 }, { "Avg Response", "8.5 min" }, { "Compliance", "78%" }, { "Escalated", 8 } },
                new() { { "Alert Type", "Temperature" }, { "Count", 12 }, { "Avg Response", "12.3 min" }, { "Compliance", "70%" }, { "Escalated", 4 } },
                new() { { "Alert Type", "Measurement Due" }, { "Count", 85 }, { "Avg Response", "15.8 min" }, { "Compliance", "65%" }, { "Escalated", 12 } }
            };
        }

        private List<Dictionary<string, object>> GenerateWHOComplianceTableData()
        {
            return new List<Dictionary<string, object>>
            {
                new() { { "Guideline", "FHR every 30 minutes" }, { "Target", "100%" }, { "Achieved", "85%" }, { "Gap", "-15%" }, { "Trend", "Improving" } },
                new() { { "Guideline", "BP every 4 hours" }, { "Target", "100%" }, { "Achieved", "88%" }, { "Gap", "-12%" }, { "Trend", "Stable" } },
                new() { { "Guideline", "Active labor at 5cm" }, { "Target", "100%" }, { "Achieved", "92%" }, { "Gap", "-8%" }, { "Trend", "Improving" } },
                new() { { "Guideline", "Partograph completion" }, { "Target", "100%" }, { "Achieved", "78%" }, { "Gap", "-22%" }, { "Trend", "Improving" } },
                new() { { "Guideline", "Alert line monitoring" }, { "Target", "100%" }, { "Achieved", "82%" }, { "Gap", "-18%" }, { "Trend", "Stable" } }
            };
        }

        private List<Dictionary<string, object>> GenerateStaffPerformanceTableData()
        {
            return new List<Dictionary<string, object>>
            {
                new() { { "Staff", "MW Ama Darko" }, { "Deliveries", 28 }, { "Response Time", "4.2 min" }, { "Quality Score", 95.0 }, { "Grade", "A" } },
                new() { { "Staff", "Dr. Kofi Mensah" }, { "Deliveries", 35 }, { "Response Time", "3.8 min" }, { "Quality Score", 92.0 }, { "Grade", "A" } },
                new() { { "Staff", "MW Yaa Asante" }, { "Deliveries", 24 }, { "Response Time", "5.5 min" }, { "Quality Score", 88.0 }, { "Grade", "B" } },
                new() { { "Staff", "MW Efua Boateng" }, { "Deliveries", 22 }, { "Response Time", "6.1 min" }, { "Quality Score", 85.0 }, { "Grade", "B" } },
                new() { { "Staff", "Dr. Kwame Owusu" }, { "Deliveries", 30 }, { "Response Time", "5.0 min" }, { "Quality Score", 82.0 }, { "Grade", "B" } }
            };
        }
    }
}
