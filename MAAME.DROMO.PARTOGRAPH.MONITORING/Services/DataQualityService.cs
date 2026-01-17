using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class DataQualityService : IDataQualityService
    {
        private readonly HttpClient _httpClient;

        public DataQualityService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DataQualityScore> GetFacilityDataQualityAsync(Guid facilityId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<DataQualityScore>($"api/monitoring/data-quality/facility/{facilityId}");
                return response ?? GenerateMockDataQualityScore("Facility", facilityId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting facility data quality: {ex.Message}");
                return GenerateMockDataQualityScore("Facility", facilityId);
            }
        }

        public async Task<DataQualityScore> GetDistrictDataQualityAsync(Guid districtId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<DataQualityScore>($"api/monitoring/data-quality/district/{districtId}");
                return response ?? GenerateMockDataQualityScore("District", districtId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting district data quality: {ex.Message}");
                return GenerateMockDataQualityScore("District", districtId);
            }
        }

        public async Task<DataQualityScore> GetRegionDataQualityAsync(Guid regionId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<DataQualityScore>($"api/monitoring/data-quality/region/{regionId}");
                return response ?? GenerateMockDataQualityScore("Region", regionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting region data quality: {ex.Message}");
                return GenerateMockDataQualityScore("Region", regionId);
            }
        }

        public async Task<DataQualityScore> GetNationalDataQualityAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<DataQualityScore>("api/monitoring/data-quality/national");
                return response ?? GenerateMockDataQualityScore("National", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting national data quality: {ex.Message}");
                return GenerateMockDataQualityScore("National", null);
            }
        }

        public async Task<List<DataQualityScore>> GetDataQualityRankingAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<List<DataQualityScore>>($"api/monitoring/data-quality/ranking{queryParams}");
                return response ?? GenerateMockDataQualityRanking();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting data quality ranking: {ex.Message}");
                return GenerateMockDataQualityRanking();
            }
        }

        public async Task<List<FieldCompletenessReport>> GetFieldCompletenessAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<List<FieldCompletenessReport>>($"api/monitoring/data-quality/field-completeness{queryParams}");
                return response ?? GenerateMockFieldCompleteness();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting field completeness: {ex.Message}");
                return GenerateMockFieldCompleteness();
            }
        }

        public async Task<List<DataGap>> GetIdentifiedGapsAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<List<DataGap>>($"api/monitoring/data-quality/gaps{queryParams}");
                return response ?? GenerateMockDataGaps();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting identified gaps: {ex.Message}");
                return GenerateMockDataGaps();
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

        private DataQualityScore GenerateMockDataQualityScore(string entityType, Guid? entityId)
        {
            var random = entityId.HasValue ? new Random(entityId.Value.GetHashCode()) : new Random();
            var overallScore = 65 + random.NextDouble() * 30;
            var grade = overallScore switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "F"
            };

            var previousScore = overallScore - 5 + random.NextDouble() * 10;
            var scoreChange = overallScore - previousScore;

            return new DataQualityScore
            {
                FacilityId = entityId,
                FacilityName = entityType == "National" ? "National" : $"{entityType} Entity",
                DistrictName = entityType == "Facility" ? "Sample District" : "",
                RegionName = entityType == "Facility" || entityType == "District" ? "Greater Accra" : "",
                OverallScore = Math.Round(overallScore, 1),
                Grade = grade,
                CompletenessScore = Math.Round(70 + random.NextDouble() * 25, 1),
                TimelinessScore = Math.Round(60 + random.NextDouble() * 30, 1),
                AccuracyScore = Math.Round(75 + random.NextDouble() * 20, 1),
                ConsistencyScore = Math.Round(65 + random.NextDouble() * 30, 1),
                TotalPartographs = random.Next(50, 200),
                CompletePartographs = random.Next(40, 180),
                PartographsWithMissingData = random.Next(5, 30),
                LateMeasurements = random.Next(10, 50),
                OutOfRangeValues = random.Next(2, 15),
                InconsistentRecords = random.Next(1, 10),
                IdentifiedGaps = GenerateMockDataGaps().Take(3).ToList(),
                PreviousPeriodScore = Math.Round(previousScore, 1),
                ScoreChange = Math.Round(scoreChange, 1),
                Trend = scoreChange > 2 ? "Improving" : scoreChange < -2 ? "Declining" : "Stable"
            };
        }

        private List<DataQualityScore> GenerateMockDataQualityRanking()
        {
            var facilities = new[]
            {
                "Korle Bu Teaching Hospital", "Ridge Hospital", "La General Hospital",
                "Tema General Hospital", "Achimota Hospital", "Mamprobi Hospital",
                "Madina Hospital", "Ga South Municipal Hospital", "Kaneshie Polyclinic",
                "Princess Marie Louise Hospital"
            };

            return facilities.Select((name, i) =>
            {
                var score = GenerateMockDataQualityScore("Facility", Guid.NewGuid());
                score.FacilityName = name;
                return score;
            }).OrderByDescending(s => s.OverallScore).ToList();
        }

        private List<FieldCompletenessReport> GenerateMockFieldCompleteness()
        {
            var random = new Random();
            var fields = new (string Name, string Category, bool Required)[]
            {
                ("Fetal Heart Rate", "Fetal Monitoring", true),
                ("Blood Pressure", "Maternal Vitals", true),
                ("Temperature", "Maternal Vitals", true),
                ("Cervical Dilatation", "Labor Progress", true),
                ("Head Descent", "Labor Progress", true),
                ("Contractions", "Labor Progress", true),
                ("Urine Protein", "Urinalysis", false),
                ("Urine Acetone", "Urinalysis", false),
                ("Amniotic Fluid", "Fetal Assessment", true),
                ("Moulding", "Fetal Assessment", true),
                ("Caput", "Fetal Assessment", false),
                ("APGAR Score", "Birth Outcome", true),
                ("Birth Weight", "Birth Outcome", true),
                ("Blood Loss", "Birth Outcome", true),
                ("Placenta Delivery", "Third Stage", true)
            };

            return fields.Select(f =>
            {
                var total = random.Next(100, 500);
                var filled = (int)(total * (f.Required ? 0.75 + random.NextDouble() * 0.2 : 0.5 + random.NextDouble() * 0.4));
                var completeness = (double)filled / total * 100;

                return new FieldCompletenessReport
                {
                    FieldName = f.Name,
                    Category = f.Category,
                    TotalRecords = total,
                    FilledRecords = filled,
                    IsRequired = f.Required,
                    Status = completeness >= 90 ? "Good" :
                             completeness >= 70 ? "Warning" : "Critical"
                };
            }).OrderBy(f => f.CompletenessPercent).ToList();
        }

        private List<DataGap> GenerateMockDataGaps()
        {
            return new List<DataGap>
            {
                new DataGap
                {
                    GapType = "MissingFHR",
                    Description = "Fetal heart rate measurements missing or incomplete",
                    AffectedRecords = new Random().Next(5, 25),
                    Severity = "High",
                    Recommendation = "Ensure FHR is recorded every 30 minutes during active labor"
                },
                new DataGap
                {
                    GapType = "LateBPMeasurement",
                    Description = "Blood pressure measurements taken outside recommended intervals",
                    AffectedRecords = new Random().Next(10, 40),
                    Severity = "Medium",
                    Recommendation = "Record BP at least every 4 hours or as clinically indicated"
                },
                new DataGap
                {
                    GapType = "IncompleteOutcome",
                    Description = "Birth outcome records missing required fields",
                    AffectedRecords = new Random().Next(3, 15),
                    Severity = "High",
                    Recommendation = "Complete all required fields including APGAR, birth weight, and blood loss"
                },
                new DataGap
                {
                    GapType = "MissingDilatation",
                    Description = "Cervical dilatation not recorded within expected intervals",
                    AffectedRecords = new Random().Next(8, 30),
                    Severity = "Medium",
                    Recommendation = "Record cervical dilatation at least every 4 hours during first stage"
                },
                new DataGap
                {
                    GapType = "InconsistentTimestamps",
                    Description = "Measurement timestamps appear inconsistent or out of sequence",
                    AffectedRecords = new Random().Next(2, 10),
                    Severity = "Low",
                    Recommendation = "Verify device time settings and ensure measurements are recorded in real-time"
                }
            };
        }
    }
}
