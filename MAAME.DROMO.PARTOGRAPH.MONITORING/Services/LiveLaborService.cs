using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class LiveLaborService : ILiveLaborService
    {
        private readonly HttpClient _httpClient;
        private readonly ICDSService _cdsService;

        public LiveLaborService(HttpClient httpClient, ICDSService cdsService)
        {
            _httpClient = httpClient;
            _cdsService = cdsService;
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
            List<LiveLaborCase> cases;
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<List<LiveLaborCase>>($"api/monitoring/live-labor/cases{queryParams}");
                cases = response ?? GenerateMockCases(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting active labor cases: {ex.Message}");
                cases = GenerateMockCases(filter);
            }

            // Apply Clinical Decision Support (Simulating Backend Logic)
            foreach (var labCase in cases)
            {
                try 
                {
                    // We generate guide data to give the rules engine historical context if needed
                    var guideData = GenerateMockGuideData(labCase);
                    labCase.ClinicalAlerts = _cdsService.EvaluateCase(labCase, guideData);
                    
                    // Update risk level based on CDS
                    if (labCase.ClinicalAlerts.Any(a => a.Severity == Models.ClinicalAlertSeverity.Emergency || a.Severity == Models.ClinicalAlertSeverity.Critical))
                    {
                        labCase.RiskLevel = "Critical";
                    }
                    else if (labCase.ClinicalAlerts.Any(a => a.Severity == Models.ClinicalAlertSeverity.Warning))
                    {
                        if (labCase.RiskLevel != "Critical") labCase.RiskLevel = "High";
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error running CDS for case {labCase.PartographId}: {e.Message}");
                }
            }

            return cases;
        }

        public async Task<LiveLaborCase?> GetLaborCaseAsync(Guid partographId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<LiveLaborCase>($"api/monitoring/live-labor/cases/{partographId}");
                return response ?? GenerateMockCase(partographId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting labor case: {ex.Message}");
                return GenerateMockCase(partographId);
            }
        }

        public async Task<PartographDetailsDto?> GetPartographDetailsAsync(Guid partographId)
        {
            var laborCase = await GetLaborCaseAsync(partographId);
            if (laborCase == null) return null;

            return new PartographDetailsDto
            {
                CaseInfo = laborCase,
                GuideData = GenerateMockGuideData(laborCase)
            };
        }

        private LiveLaborCase GenerateMockCase(Guid partographId)
        {
            var random = new Random();
            return new LiveLaborCase
            {
                PartographId = partographId,
                PatientId = Guid.NewGuid(),
                PatientName = "Ama Osei",
                HospitalNumber = "MRN-12345",
                Age = 28,
                Gravida = "G2",
                Parity = "P1",
                FacilityName = "Korle Bu Teaching Hospital",
                DistrictName = "Accra Metro",
                RegionName = "Greater Accra",
                AdmissionTime = DateTime.UtcNow.AddHours(-6),
                LaborStartTime = DateTime.UtcNow.AddHours(-5),
                CurrentDilatation = 7,
                CurrentStation = 0,
                LaborStage = "First Stage",
                LatestFHR = 138,
                LatestFHRTime = DateTime.UtcNow.AddMinutes(-5),
                LatestSystolicBP = 118,
                LatestDiastolicBP = 76, 
                LatestBPTime = DateTime.UtcNow.AddMinutes(-30),
                LatestTemperature = 36.8m,
                ContractionsPerTenMinutes = 4,
                RiskLevel = "Normal",
                ActiveAlerts = new List<string>(),
                AssignedMidwife = "MW-Sarah",
                LastAssessmentTime = DateTime.UtcNow.AddMinutes(-15)
            };
        }

        private LaborCareGuideData GenerateMockGuideData(LiveLaborCase laborCase)
        {
            var data = new LaborCareGuideData
            {
                StartTime = laborCase.LaborStartTime ?? DateTime.UtcNow.AddHours(-6),
                TimePoints = new List<DateTime>()
            };

            var random = new Random();
            var duration = DateTime.UtcNow - data.StartTime;
            var intervals = (int)(duration.TotalMinutes / 30) + 1; // 30 min intervals

            // Generate time points
            for (int i = 0; i < intervals; i++)
            {
                data.TimePoints.Add(data.StartTime.AddMinutes(i * 30));
            }

            // Generate historical data matching current state
            foreach (var time in data.TimePoints)
            {
                // Progress curve (Sigmoid-like for dilatation)
                double progressRatio = (time - data.StartTime).TotalHours / 8.0; // Assume 8h labor
                if (progressRatio > 1) progressRatio = 1;
                
                // Dilatation (starts at 4cm -> ends at current)
                double dilation = 4 + (laborCase.CurrentDilatation - 4) * progressRatio;
                // Add some noise
                dilation += (random.NextDouble() - 0.5) * 0.5;
                if (dilation > 10) dilation = 10;
                
                // Descent (starts at -3 -> ends at current)
                double descent = -3 + (laborCase.CurrentStation - (-3)) * progressRatio;

                if (time.Minute % 60 == 0 || time.Minute == 0) // Hourly measurements
                {
                    data.Dilatation.Add(new MeasurementPoint { Time = time, Value = Math.Round(dilation, 1) });
                    data.Descent.Add(new MeasurementPoint { Time = time, Value = Math.Round(descent, 0) });
                }

                // Section 1: Supportive Care (every 30m)
                data.Companion.Add(new CategoricalPoint { Time = time, Value = "Y" });
                data.PainRelief.Add(new CategoricalPoint 
                { 
                    Time = time, 
                    Value = random.Next(10) == 0 ? "N" : "Y", // Occasional No
                    IsAlert = false 
                });
                data.OralFluid.Add(new CategoricalPoint { Time = time, Value = "Y" });
                data.Posture.Add(new CategoricalPoint { Time = time, Value = random.Next(2) == 0 ? "MO" : "SUP" });

                // Section 2: Baby (every 30m)
                int baseFHR = 130 + random.Next(-10, 15);
                data.BaselineFHR.Add(new MeasurementPoint 
                { 
                    Time = time, 
                    Value = baseFHR,
                    IsAlert = baseFHR < 110 || baseFHR > 160
                });
                data.FHRDeceleration.Add(new CategoricalPoint { Time = time, Value = "N" });
                data.AmnioticFluid.Add(new CategoricalPoint { Time = time, Value = "C" });
                data.FetalPosition.Add(new CategoricalPoint { Time = time, Value = "OA" });
                data.Caput.Add(new CategoricalPoint { Time = time, Value = "0" });
                data.Moulding.Add(new CategoricalPoint { Time = time, Value = "0" });

                // Section 3: Woman (every 30m)
                int pulse = 80 + random.Next(-10, 15);
                data.Pulse.Add(new MeasurementPoint { Time = time, Value = pulse, IsAlert = pulse > 100 });

                if (time.Minute == 0) // Hourly BP
                {
                    data.SystolicBP.Add(new MeasurementPoint { Time = time, Value = 110 + random.Next(0, 20) });
                    data.DiastolicBP.Add(new MeasurementPoint { Time = time, Value = 70 + random.Next(0, 15) });
                }
                
                if (time.Hour % 4 == 0 && time.Minute == 0) // 4-hourly Temp
                {
                    data.Temperature.Add(new MeasurementPoint { Time = time, Value = 36.5 + random.NextDouble() });
                }
                data.UrineProtein.Add(new CategoricalPoint { Time = time, Value = "-" });
                data.UrineAcetone.Add(new CategoricalPoint { Time = time, Value = "-" });
                data.UrineVolume.Add(new MeasurementPoint { Time = time, Value = random.Next(50, 200) });

                // Section 4: Labor Progress (Contractions)
                int contractions = 3 + (int)(2 * progressRatio);
                if (contractions > 5) contractions = 5;
                data.ContractionFrequency.Add(new MeasurementPoint { Time = time, Value = contractions });
                data.ContractionDuration.Add(new MeasurementPoint { Time = time, Value = random.Next(35, 50) });

                // Section 5: Medication
                if (progressRatio > 0.5)
                {
                    data.Oxytocin.Add(new MeasurementPoint { Time = time, Value = 15 + (int)(10 * (progressRatio - 0.5)) });
                }
                data.IVFluids.Add(new MeasurementPoint { Time = time, Value = 100 });

                // Section 6: Shared Decision Making
                data.Assessment.Add(new CategoricalPoint { Time = time, Value = "NORMAL PROGRESS" });
                data.Plan.Add(new CategoricalPoint { Time = time, Value = "Continue routine monitoring" });
                data.Initials.Add(new CategoricalPoint { Time = time, Value = "MW" });
            }

            return data;
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
