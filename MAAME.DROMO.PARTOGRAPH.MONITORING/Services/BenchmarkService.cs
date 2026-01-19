using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class BenchmarkService : IBenchmarkService
    {
        private readonly HttpClient _httpClient;

        public BenchmarkService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<BenchmarkComparison> GetFacilityBenchmarkAsync(Guid facilityId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<BenchmarkComparison>($"api/monitoring/benchmarks/facility/{facilityId}");
                return response ?? GenerateMockBenchmark("Facility", facilityId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting facility benchmark: {ex.Message}");
                return GenerateMockBenchmark("Facility", facilityId);
            }
        }

        public async Task<BenchmarkComparison> GetDistrictBenchmarkAsync(Guid districtId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<BenchmarkComparison>($"api/monitoring/benchmarks/district/{districtId}");
                return response ?? GenerateMockBenchmark("District", districtId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting district benchmark: {ex.Message}");
                return GenerateMockBenchmark("District", districtId);
            }
        }

        public async Task<BenchmarkComparison> GetRegionBenchmarkAsync(Guid regionId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<BenchmarkComparison>($"api/monitoring/benchmarks/region/{regionId}");
                return response ?? GenerateMockBenchmark("Region", regionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting region benchmark: {ex.Message}");
                return GenerateMockBenchmark("Region", regionId);
            }
        }

        public async Task<List<FacilityRanking>> GetFacilityRankingsAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<List<FacilityRanking>>($"api/monitoring/benchmarks/rankings/facilities{queryParams}");
                return response ?? GenerateMockFacilityRankings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting facility rankings: {ex.Message}");
                return GenerateMockFacilityRankings();
            }
        }

        public async Task<List<FacilityRanking>> GetDistrictRankingsAsync(DashboardFilter? filter = null)
        {
            try
            {
                var queryParams = BuildQueryParams(filter);
                var response = await _httpClient.GetFromJsonAsync<List<FacilityRanking>>($"api/monitoring/benchmarks/rankings/districts{queryParams}");
                return response ?? GenerateMockDistrictRankings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting district rankings: {ex.Message}");
                return GenerateMockDistrictRankings();
            }
        }

        public async Task<List<FacilityRanking>> GetRegionRankingsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<FacilityRanking>>("api/monitoring/benchmarks/rankings/regions");
                return response ?? GenerateMockRegionRankings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting region rankings: {ex.Message}");
                return GenerateMockRegionRankings();
            }
        }

        public async Task<Dictionary<string, double>> GetNationalBenchmarksAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<Dictionary<string, double>>("api/monitoring/benchmarks/national");
                return response ?? GetDefaultBenchmarks();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting national benchmarks: {ex.Message}");
                return GetDefaultBenchmarks();
            }
        }

        public async Task<Dictionary<string, double>> GetRegionalBenchmarksAsync(Guid regionId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<Dictionary<string, double>>($"api/monitoring/benchmarks/regional/{regionId}");
                return response ?? GetDefaultBenchmarks();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting regional benchmarks: {ex.Message}");
                return GetDefaultBenchmarks();
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

        private Dictionary<string, double> GetDefaultBenchmarks()
        {
            return new Dictionary<string, double>
            {
                { "CaesareanRate", 12.5 },
                { "ComplicationRate", 8.2 },
                { "ReferralRate", 5.5 },
                { "StillbirthRate", 10.5 },
                { "NeonatalMortalityRate", 18.0 },
                { "MaternalMortalityRatio", 310.0 },
                { "AlertResponseTime", 8.5 },
                { "DataQualityScore", 78.0 }
            };
        }

        private BenchmarkComparison GenerateMockBenchmark(string entityType, Guid entityId)
        {
            var random = new Random(entityId.GetHashCode());
            var benchmarks = GetDefaultBenchmarks();

            var csRate = 8 + random.NextDouble() * 15;
            var compRate = 5 + random.NextDouble() * 10;
            var refRate = 3 + random.NextDouble() * 8;

            var indicators = new List<PerformanceIndicator>
            {
                CreateIndicator("C-Section Rate", csRate, benchmarks["CaesareanRate"], random),
                CreateIndicator("Complication Rate", compRate, benchmarks["ComplicationRate"], random),
                CreateIndicator("Referral Rate", refRate, benchmarks["ReferralRate"], random),
                CreateIndicator("Alert Response Time", 5 + random.NextDouble() * 10, benchmarks["AlertResponseTime"], random),
                CreateIndicator("Data Quality Score", 60 + random.NextDouble() * 35, benchmarks["DataQualityScore"], random)
            };

            var avgPerformance = indicators.Average(i =>
                i.Status == "AboveTarget" ? 1 : i.Status == "OnTarget" ? 0.5 : 0);

            var overallPerformance = avgPerformance switch
            {
                >= 0.8 => "Excellent",
                >= 0.6 => "Good",
                >= 0.4 => "Average",
                >= 0.2 => "Below Average",
                _ => "Poor"
            };

            return new BenchmarkComparison
            {
                EntityName = $"{entityType} Entity",
                EntityType = entityType,
                EntityId = entityId,
                CaesareanRate = Math.Round(csRate, 1),
                ComplicationRate = Math.Round(compRate, 1),
                ReferralRate = Math.Round(refRate, 1),
                StillbirthRate = Math.Round(8 + random.NextDouble() * 8, 1),
                NeonatalMortalityRate = Math.Round(12 + random.NextDouble() * 15, 1),
                MaternalMortalityRatio = Math.Round(200 + random.NextDouble() * 200, 0),
                AverageAlertResponseTime = Math.Round(5 + random.NextDouble() * 10, 1),
                DataQualityScore = Math.Round(60 + random.NextDouble() * 35, 1),
                BenchmarkCaesareanRate = benchmarks["CaesareanRate"],
                BenchmarkComplicationRate = benchmarks["ComplicationRate"],
                BenchmarkReferralRate = benchmarks["ReferralRate"],
                BenchmarkStillbirthRate = benchmarks["StillbirthRate"],
                BenchmarkNeonatalMortalityRate = benchmarks["NeonatalMortalityRate"],
                BenchmarkMaternalMortalityRatio = benchmarks["MaternalMortalityRatio"],
                BenchmarkAlertResponseTime = benchmarks["AlertResponseTime"],
                BenchmarkDataQualityScore = benchmarks["DataQualityScore"],
                OverallPerformance = overallPerformance,
                Indicators = indicators,
                Rank = random.Next(1, 50),
                TotalEntities = 100,
                Percentile = random.Next(20, 95)
            };
        }

        private PerformanceIndicator CreateIndicator(string name, double value, double benchmark, Random random)
        {
            var variance = value - benchmark;
            var variancePercent = benchmark > 0 ? variance / benchmark * 100 : 0;

            // For rates like C-section, lower is better (within reason)
            // For scores, higher is better
            var isBetterHigher = name.Contains("Score") || name.Contains("Quality");
            var isOnTarget = Math.Abs(variancePercent) < 15;
            var isAbove = isBetterHigher ? value > benchmark : value < benchmark;

            var trends = new[] { "Improving", "Stable", "Declining" };

            return new PerformanceIndicator
            {
                Name = name,
                Value = Math.Round(value, 1),
                Benchmark = Math.Round(benchmark, 1),
                Variance = Math.Round(variance, 1),
                VariancePercent = Math.Round(variancePercent, 1),
                Status = isOnTarget ? "OnTarget" : (isAbove ? "AboveTarget" : "BelowTarget"),
                Trend = trends[random.Next(trends.Length)]
            };
        }

        private List<FacilityRanking> GenerateMockFacilityRankings()
        {
            var facilities = new[]
            {
                ("Korle Bu Teaching Hospital", "Accra Metro", "Greater Accra", "Teaching Hospital"),
                ("Ridge Hospital", "Accra Metro", "Greater Accra", "Regional Hospital"),
                ("La General Hospital", "La Dadekotopon", "Greater Accra", "District Hospital"),
                ("Tema General Hospital", "Tema Metro", "Greater Accra", "Regional Hospital"),
                ("Achimota Hospital", "Ga East", "Greater Accra", "District Hospital"),
                ("Mamprobi Hospital", "Ablekuma South", "Greater Accra", "District Hospital"),
                ("Madina Hospital", "La Nkwantanang", "Greater Accra", "District Hospital"),
                ("Ga South Municipal Hospital", "Ga South", "Greater Accra", "District Hospital"),
                ("Kaneshie Polyclinic", "Okaikwei North", "Greater Accra", "Polyclinic"),
                ("Princess Marie Louise Hospital", "Accra Metro", "Greater Accra", "District Hospital")
            };

            var random = new Random();
            var rankings = facilities.Select((f, i) =>
            {
                var score = 95 - (i * 5) + random.Next(-5, 5);
                return new FacilityRanking
                {
                    FacilityId = Guid.NewGuid(),
                    FacilityName = f.Item1,
                    DistrictName = f.Item2,
                    RegionName = f.Item3,
                    FacilityType = f.Item4,
                    OverallRank = i + 1,
                    OverallScore = Math.Round((double) Math.Max(50, score), 1),
                    PerformanceGrade = score >= 90 ? "A" : score >= 80 ? "B" : score >= 70 ? "C" : score >= 60 ? "D" : "F",
                    CaesareanRate = Math.Round(8 + random.NextDouble() * 12, 1),
                    ComplicationRate = Math.Round(4 + random.NextDouble() * 8, 1),
                    DataQualityScore = Math.Round(60 + random.NextDouble() * 35, 1),
                    AlertResponseScore = Math.Round(70 + random.NextDouble() * 25, 1),
                    TotalDeliveries = random.Next(50, 300),
                    RankChange = random.Next(-5, 6)
                };
            }).OrderBy(r => r.OverallRank).ToList();

            return rankings;
        }

        private List<FacilityRanking> GenerateMockDistrictRankings()
        {
            var districts = new[]
            {
                "Accra Metro", "Tema Metro", "La Dadekotopon", "Ga East",
                "Ga West", "Ga South", "Ablekuma South", "La Nkwantanang"
            };

            var random = new Random();
            return districts.Select((d, i) =>
            {
                var score = 90 - (i * 4) + random.Next(-3, 3);
                return new FacilityRanking
                {
                    FacilityId = Guid.NewGuid(),
                    FacilityName = d,
                    DistrictName = d,
                    RegionName = "Greater Accra",
                    FacilityType = "District",
                    OverallRank = i + 1,
                    OverallScore = Math.Round((double) Math.Max(50, score), 1),
                    PerformanceGrade = score >= 85 ? "A" : score >= 75 ? "B" : score >= 65 ? "C" : "D",
                    TotalDeliveries = random.Next(200, 800),
                    RankChange = random.Next(-3, 4)
                };
            }).OrderBy(r => r.OverallRank).ToList();
        }

        private List<FacilityRanking> GenerateMockRegionRankings()
        {
            var regions = new[]
            {
                "Greater Accra", "Ashanti", "Western", "Central",
                "Eastern", "Volta", "Northern", "Upper East"
            };

            var random = new Random();
            return regions.Select((r, i) =>
            {
                var score = 88 - (i * 3) + random.Next(-2, 2);
                return new FacilityRanking
                {
                    FacilityId = Guid.NewGuid(),
                    FacilityName = r,
                    RegionName = r,
                    FacilityType = "Region",
                    OverallRank = i + 1,
                    OverallScore = Math.Round((double) Math.Max(50, score), 1),
                    PerformanceGrade = score >= 85 ? "A" : score >= 75 ? "B" : score >= 65 ? "C" : "D",
                    TotalDeliveries = random.Next(1000, 5000),
                    RankChange = random.Next(-2, 3)
                };
            }).OrderBy(r => r.OverallRank).ToList();
        }
    }
}
