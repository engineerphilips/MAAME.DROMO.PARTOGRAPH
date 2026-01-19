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

        public async Task<MaternalHealthIndicators> GetMaternalHealthIndicatorsAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<MaternalHealthIndicators>(
                    $"api/monitoring/dashboard/maternal-health{queryParams}");
                return response ?? GenerateMockMaternalHealthIndicators(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting maternal health indicators: {ex.Message}");
                return GenerateMockMaternalHealthIndicators(filter);
            }
        }

        public async Task<AlertSummary> GetAlertSummaryAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<AlertSummary>(
                    $"api/monitoring/dashboard/alert-summary{queryParams}");
                return response ?? GenerateMockAlertSummary(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting alert summary: {ex.Message}");
                return GenerateMockAlertSummary(filter);
            }
        }

        public async Task<List<FacilityPerformanceSummary>> GetTopFacilitiesAsync(DashboardFilter? filter = null, int count = 5)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var separator = string.IsNullOrEmpty(queryParams) ? "?" : "&";
                var response = await _httpClient.GetFromJsonAsync<List<FacilityPerformanceSummary>>(
                    $"api/monitoring/dashboard/top-facilities{queryParams}{separator}count={count}");
                return response ?? GenerateMockFacilityPerformance(filter, count, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting top facilities: {ex.Message}");
                return GenerateMockFacilityPerformance(filter, count, true);
            }
        }

        public async Task<List<FacilityPerformanceSummary>> GetBottomFacilitiesAsync(DashboardFilter? filter = null, int count = 5)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var separator = string.IsNullOrEmpty(queryParams) ? "?" : "&";
                var response = await _httpClient.GetFromJsonAsync<List<FacilityPerformanceSummary>>(
                    $"api/monitoring/dashboard/bottom-facilities{queryParams}{separator}count={count}");
                return response ?? GenerateMockFacilityPerformance(filter, count, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting bottom facilities: {ex.Message}");
                return GenerateMockFacilityPerformance(filter, count, false);
            }
        }

        public async Task<List<GeographicPerformance>> GetGeographicPerformanceAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<List<GeographicPerformance>>(
                    $"api/monitoring/dashboard/geographic-performance{queryParams}");
                return response ?? GenerateMockGeographicPerformance(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting geographic performance: {ex.Message}");
                return GenerateMockGeographicPerformance(filter);
            }
        }

        public async Task<DashboardLiveLaborSummary> GetLiveLaborSummaryAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<DashboardLiveLaborSummary>(
                    $"api/monitoring/dashboard/live-labor-summary{queryParams}");
                return response ?? GenerateMockLiveLaborSummary(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting live labor summary: {ex.Message}");
                return GenerateMockLiveLaborSummary(filter);
            }
        }

        public async Task<PeriodComparison> GetPeriodComparisonAsync(DashboardFilter? filter = null, string period = "Month")
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var separator = string.IsNullOrEmpty(queryParams) ? "?" : "&";
                var response = await _httpClient.GetFromJsonAsync<PeriodComparison>(
                    $"api/monitoring/dashboard/period-comparison{queryParams}{separator}period={period}");
                return response ?? GenerateMockPeriodComparison(filter, period);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting period comparison: {ex.Message}");
                return GenerateMockPeriodComparison(filter, period);
            }
        }

        #region Mock Data Generators

        private MaternalHealthIndicators GenerateMockMaternalHealthIndicators(DashboardFilter? filter)
        {
            var random = new Random();
            var totalDeliveries = random.Next(800, 1500);
            var liveBirths = (int)(totalDeliveries * 0.97);
            var maternalDeaths = random.Next(0, 3);
            var neonatalDeaths = random.Next(5, 15);
            var stillbirths = random.Next(8, 20);

            return new MaternalHealthIndicators
            {
                TotalDeliveries = totalDeliveries,
                TotalLiveBirths = liveBirths,
                MaternalDeaths = maternalDeaths,
                NeonatalDeaths = neonatalDeaths,
                Stillbirths = stillbirths,
                EarlyNeonatalDeaths = random.Next(3, 10),
                MaternalMortalityRatio = liveBirths > 0 ? (maternalDeaths / (double)liveBirths) * 100000 : 0,
                NeonatalMortalityRate = liveBirths > 0 ? (neonatalDeaths / (double)liveBirths) * 1000 : 0,
                StillbirthRate = totalDeliveries > 0 ? (stillbirths / (double)totalDeliveries) * 1000 : 0,
                PerinatalMortalityRate = liveBirths > 0 ? ((stillbirths + neonatalDeaths) / (double)liveBirths) * 1000 : 0,
                CaesareanRate = random.Next(10, 25),
                AssistedDeliveryRate = random.Next(2, 8),
                ComplicationRate = random.Next(5, 15),
                ReferralCompletionRate = random.Next(85, 98),
                SkilledBirthAttendanceRate = random.Next(90, 99),
                MMRChangePercent = random.Next(-20, 20),
                NMRChangePercent = random.Next(-15, 15),
                StillbirthRateChangePercent = random.Next(-10, 10),
                CaesareanRateChangePercent = random.Next(-5, 5)
            };
        }

        private AlertSummary GenerateMockAlertSummary(DashboardFilter? filter)
        {
            var random = new Random();
            var criticalAlerts = random.Next(2, 8);
            var warningAlerts = random.Next(5, 15);
            var infoAlerts = random.Next(3, 10);
            var total = criticalAlerts + warningAlerts + infoAlerts;

            return new AlertSummary
            {
                TotalActiveAlerts = total,
                CriticalAlerts = criticalAlerts,
                WarningAlerts = warningAlerts,
                InfoAlerts = infoAlerts,
                UnacknowledgedAlerts = random.Next(2, total / 2),
                EscalatedAlerts = random.Next(0, 3),
                AverageResponseTimeMinutes = random.Next(5, 25),
                FetalAlerts = random.Next(3, 12),
                MaternalAlerts = random.Next(2, 8),
                LaborAlerts = random.Next(2, 10),
                ResponseCompliancePercent = random.Next(75, 95),
                AlertsRespondedWithinTarget = random.Next(total / 2, total)
            };
        }

        private List<FacilityPerformanceSummary> GenerateMockFacilityPerformance(DashboardFilter? filter, int count, bool isTop)
        {
            var random = new Random();
            var facilities = new List<FacilityPerformanceSummary>();
            var facilityNames = new[] { "Korle Bu Teaching Hospital", "Ridge Hospital", "La General Hospital",
                "Tema General Hospital", "Achimota Hospital", "Greater Accra Regional Hospital",
                "Mamprobi Polyclinic", "LEKMA Hospital", "Kaneshie Polyclinic", "Adabraka Polyclinic" };
            var districts = new[] { "Accra Metro", "La Dadekotopon", "Tema Metro", "Ga East", "Ga West",
                "Adentan", "Ashaiman", "Kpone Katamanso", "Ga Central", "Ga South" };
            var grades = isTop ? new[] { "A", "A", "B", "B", "B" } : new[] { "D", "D", "C", "C", "C" };

            for (int i = 0; i < Math.Min(count, facilityNames.Length); i++)
            {
                facilities.Add(new FacilityPerformanceSummary
                {
                    FacilityID = Guid.NewGuid(),
                    FacilityName = facilityNames[i],
                    DistrictName = districts[i % districts.Length],
                    RegionName = "Greater Accra",
                    FacilityType = i < 3 ? "Hospital" : "Polyclinic",
                    TotalDeliveries = isTop ? random.Next(150, 300) : random.Next(20, 80),
                    CaesareanRate = isTop ? random.Next(10, 16) : random.Next(18, 30),
                    ComplicationRate = isTop ? random.Next(3, 8) : random.Next(12, 25),
                    ReferralRate = isTop ? random.Next(5, 12) : random.Next(15, 30),
                    DataQualityScore = isTop ? random.Next(85, 98) : random.Next(50, 70),
                    AlertResponseScore = isTop ? random.Next(80, 95) : random.Next(40, 65),
                    OverallScore = isTop ? random.Next(80, 98) : random.Next(35, 60),
                    PerformanceGrade = grades[i % grades.Length],
                    Rank = i + 1,
                    RankChange = random.Next(-3, 4),
                    PerformanceStatus = isTop ? (i < 2 ? "Excellent" : "Good") : (i < 2 ? "Critical" : "Needs Improvement"),
                    ActiveLabors = random.Next(2, 12),
                    LastActivityTime = DateTime.UtcNow.AddMinutes(-random.Next(5, 120)),
                    IsActive = true
                });
            }

            return facilities;
        }

        private List<GeographicPerformance> GenerateMockGeographicPerformance(DashboardFilter? filter)
        {
            var random = new Random();
            var performance = new List<GeographicPerformance>();

            // If filter has RegionID, return districts; otherwise return regions
            if (filter?.RegionID != null)
            {
                var districts = new[] { "Accra Metro", "La Dadekotopon", "Tema Metro", "Ga East", "Ga West",
                    "Adentan", "Ashaiman", "Kpone Katamanso", "Ga Central", "Ga South" };

                foreach (var district in districts)
                {
                    performance.Add(new GeographicPerformance
                    {
                        ID = Guid.NewGuid(),
                        Name = district,
                        Type = "District",
                        Code = district.Substring(0, 3).ToUpper(),
                        TotalFacilities = random.Next(8, 25),
                        ActiveFacilities = random.Next(5, 20),
                        TotalDeliveries = random.Next(200, 600),
                        DeliveriesToday = random.Next(5, 25),
                        ActiveLabors = random.Next(5, 20),
                        HighRiskCases = random.Next(1, 5),
                        Complications = random.Next(10, 40),
                        Referrals = random.Next(15, 50),
                        CaesareanRate = random.Next(10, 22),
                        ComplicationRate = random.Next(5, 18),
                        PerformanceStatus = random.Next(3) switch { 0 => "Good", 1 => "Normal", _ => "Warning" },
                        PerformanceScore = random.Next(60, 95)
                    });
                }
            }
            else
            {
                var regions = new[] { "Greater Accra", "Ashanti", "Western", "Eastern", "Central",
                    "Northern", "Volta", "Upper East", "Upper West", "Bono" };

                foreach (var region in regions)
                {
                    performance.Add(new GeographicPerformance
                    {
                        ID = Guid.NewGuid(),
                        Name = region,
                        Type = "Region",
                        Code = region.Substring(0, 2).ToUpper(),
                        TotalFacilities = random.Next(50, 200),
                        ActiveFacilities = random.Next(40, 180),
                        TotalDeliveries = random.Next(1000, 5000),
                        DeliveriesToday = random.Next(30, 150),
                        ActiveLabors = random.Next(20, 80),
                        HighRiskCases = random.Next(5, 20),
                        Complications = random.Next(50, 200),
                        Referrals = random.Next(80, 300),
                        CaesareanRate = random.Next(10, 22),
                        ComplicationRate = random.Next(5, 18),
                        PerformanceStatus = random.Next(3) switch { 0 => "Good", 1 => "Normal", _ => "Warning" },
                        PerformanceScore = random.Next(60, 95)
                    });
                }
            }

            return performance.OrderByDescending(p => p.PerformanceScore).ToList();
        }

        private DashboardLiveLaborSummary GenerateMockLiveLaborSummary(DashboardFilter? filter)
        {
            var random = new Random();
            var totalCases = random.Next(20, 50);
            var criticalCases = random.Next(2, 6);
            var highRiskCases = random.Next(4, 10);
            var moderateCases = random.Next(5, 12);

            var topCriticalCases = new List<LiveLaborCaseBrief>();
            var patientNames = new[] { "Abena Mensah", "Akosua Asante", "Ama Owusu", "Adwoa Boateng", "Yaa Appiah" };
            var facilities = new[] { "Korle Bu Teaching Hospital", "Ridge Hospital", "La General Hospital" };

            for (int i = 0; i < Math.Min(criticalCases, 5); i++)
            {
                topCriticalCases.Add(new LiveLaborCaseBrief
                {
                    PartographId = Guid.NewGuid(),
                    PatientName = patientNames[i % patientNames.Length],
                    FacilityName = facilities[i % facilities.Length],
                    RiskLevel = i < 2 ? "Critical" : "High",
                    LatestFHR = random.Next(95, 115),
                    CurrentDilatation = random.Next(6, 10),
                    LaborStage = "First Stage",
                    AlertCount = random.Next(1, 4),
                    LaborDuration = TimeSpan.FromHours(random.Next(4, 14))
                });
            }

            return new DashboardLiveLaborSummary
            {
                TotalActiveCases = totalCases,
                CriticalCases = criticalCases,
                HighRiskCases = highRiskCases,
                ModerateRiskCases = moderateCases,
                NormalCases = totalCases - criticalCases - highRiskCases - moderateCases,
                MeasurementsDue = random.Next(5, 15),
                UnacknowledgedAlerts = random.Next(3, 10),
                TopCriticalCases = topCriticalCases
            };
        }

        private PeriodComparison GenerateMockPeriodComparison(DashboardFilter? filter, string period)
        {
            var random = new Random();
            var (startDate, endDate) = period switch
            {
                "Week" => (DateTime.UtcNow.Date.AddDays(-7), DateTime.UtcNow.Date),
                "Year" => (DateTime.UtcNow.Date.AddYears(-1), DateTime.UtcNow.Date),
                _ => (new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1), DateTime.UtcNow.Date)
            };

            var currentDeliveries = random.Next(300, 800);
            var previousDeliveries = random.Next(280, 750);
            var currentComplications = random.Next(20, 60);
            var previousComplications = random.Next(25, 65);

            return new PeriodComparison
            {
                PeriodLabel = period switch { "Week" => "This Week", "Year" => "This Year", _ => "This Month" },
                StartDate = startDate,
                EndDate = endDate,
                CurrentDeliveries = currentDeliveries,
                CurrentComplications = currentComplications,
                CurrentReferrals = random.Next(40, 100),
                CurrentMaternalDeaths = random.Next(0, 2),
                CurrentNeonatalDeaths = random.Next(3, 10),
                PreviousDeliveries = previousDeliveries,
                PreviousComplications = previousComplications,
                PreviousReferrals = random.Next(35, 95),
                PreviousMaternalDeaths = random.Next(0, 3),
                PreviousNeonatalDeaths = random.Next(4, 12)
            };
        }

        #endregion

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
