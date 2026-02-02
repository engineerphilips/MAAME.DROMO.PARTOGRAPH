using MAAME.DROMO.PARTOGRAPH.MODEL;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Controllers;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Services
{
    /// <summary>
    /// Service implementation for Robson Classification operations
    /// Reference: WHO Robson Classification: Implementation Manual (ISBN 978-92-4-151319-7)
    /// </summary>
    public class RobsonService : IRobsonService
    {
        private readonly PartographDbContext _context;
        private readonly ILogger<RobsonService> _logger;

        public RobsonService(PartographDbContext context, ILogger<RobsonService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RobsonClassificationReport> GenerateReportAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null,
            Guid? regionId = null,
            Guid? districtId = null)
        {
            var report = new RobsonClassificationReport
            {
                StartDate = startDate,
                EndDate = endDate,
                GeneratedAt = DateTime.Now
            };

            // Get classifications for the period
            var classifications = await GetClassificationsQuery(startDate, endDate, facilityId, regionId, districtId)
                .ToListAsync();

            report.TotalDeliveries = classifications.Count;
            report.TotalCesareanSections = classifications.Count(c => c.IsCesareanSection);
            report.TotalClassified = classifications.Count(c => c.Group != RobsonGroup.Unclassified);
            report.TotalUnclassified = classifications.Count(c => c.Group == RobsonGroup.Unclassified);

            // Generate group statistics
            report.GroupStatistics = GenerateGroupStatistics(classifications, report.TotalDeliveries, report.TotalCesareanSections);

            // Generate quality indicators
            report.QualityIndicators = GenerateQualityIndicators(classifications, report.GroupStatistics);

            // Get facility info if specific facility
            if (facilityId.HasValue)
            {
                var facility = await _context.Facilities
                    .Include(f => f.District)
                        .ThenInclude(d => d!.Region)
                    .FirstOrDefaultAsync(f => f.ID == facilityId.Value);
                if (facility != null)
                {
                    report.FacilityName = facility.Name;
                    report.FacilityCode = facility.Code;
                    report.Region = facility.District?.Region?.Name ?? string.Empty;
                }
            }

            return report;
        }

        public async Task<RobsonDashboardSummary> GetDashboardSummaryAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null,
            Guid? regionId = null,
            Guid? districtId = null)
        {
            var classifications = await GetClassificationsQuery(startDate, endDate, facilityId, regionId, districtId)
                .ToListAsync();

            var totalDeliveries = classifications.Count;
            var totalCesareans = classifications.Count(c => c.IsCesareanSection);

            var summary = new RobsonDashboardSummary
            {
                LastUpdated = DateTime.Now,
                Period = $"{startDate:MMM yyyy} - {endDate:MMM yyyy}",
                TotalDeliveries = totalDeliveries,
                TotalCesareans = totalCesareans,
                OverallCSRate = totalDeliveries > 0 ? Math.Round((decimal)totalCesareans / totalDeliveries * 100, 2) : 0
            };

            // Group-specific rates
            var group1 = classifications.Where(c => c.Group == RobsonGroup.Group1).ToList();
            var group3 = classifications.Where(c => c.Group == RobsonGroup.Group3).ToList();
            var group5 = classifications.Where(c => c.Group == RobsonGroup.Group5).ToList();

            summary.Group1CSRate = group1.Count > 0
                ? Math.Round((decimal)group1.Count(c => c.IsCesareanSection) / group1.Count * 100, 2) : 0;
            summary.Group3CSRate = group3.Count > 0
                ? Math.Round((decimal)group3.Count(c => c.IsCesareanSection) / group3.Count * 100, 2) : 0;
            summary.Group5CSRate = group5.Count > 0
                ? Math.Round((decimal)group5.Count(c => c.IsCesareanSection) / group5.Count * 100, 2) : 0;
            summary.Group5Proportion = totalDeliveries > 0
                ? Math.Round((decimal)group5.Count / totalDeliveries * 100, 2) : 0;
            summary.VBACRate = 100 - summary.Group5CSRate;

            // Group distribution
            summary.GroupDistribution = GetGroupDistribution(classifications, totalDeliveries);

            // Top CS contributors
            var groupStats = GenerateGroupStatistics(classifications, totalDeliveries, totalCesareans);
            summary.TopCSContributors = groupStats
                .Where(g => g.CesareansInGroup > 0)
                .OrderByDescending(g => g.AbsoluteContribution)
                .Take(5)
                .ToList();

            return summary;
        }

        public async Task<List<RobsonGroupStatistics>> GetGroupStatisticsAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null)
        {
            var classifications = await GetClassificationsQuery(startDate, endDate, facilityId)
                .ToListAsync();

            var totalDeliveries = classifications.Count;
            var totalCesareans = classifications.Count(c => c.IsCesareanSection);

            return GenerateGroupStatistics(classifications, totalDeliveries, totalCesareans);
        }

        public async Task<List<RobsonMonthlyTrend>> GetMonthlyTrendsAsync(
            int year,
            Guid? facilityId = null,
            Guid? regionId = null)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            var classifications = await GetClassificationsQuery(startDate, endDate, facilityId, regionId)
                .ToListAsync();

            var trends = new List<RobsonMonthlyTrend>();

            for (int month = 1; month <= 12; month++)
            {
                var monthData = classifications
                    .Where(c => c.ClassifiedAt.Month == month)
                    .ToList();

                var totalDeliveries = monthData.Count;
                var totalCS = monthData.Count(c => c.IsCesareanSection);

                var group1 = monthData.Where(c => c.Group == RobsonGroup.Group1).ToList();
                var group2 = monthData.Where(c => c.Group == RobsonGroup.Group2).ToList();
                var group3 = monthData.Where(c => c.Group == RobsonGroup.Group3).ToList();
                var group4 = monthData.Where(c => c.Group == RobsonGroup.Group4).ToList();
                var group5 = monthData.Where(c => c.Group == RobsonGroup.Group5).ToList();

                trends.Add(new RobsonMonthlyTrend
                {
                    Year = year,
                    Month = month,
                    TotalDeliveries = totalDeliveries,
                    TotalCS = totalCS,
                    Group1Size = group1.Count,
                    Group2Size = group2.Count,
                    Group3Size = group3.Count,
                    Group4Size = group4.Count,
                    Group5Size = group5.Count,
                    Group1CSRate = group1.Count > 0 ? Math.Round((decimal)group1.Count(c => c.IsCesareanSection) / group1.Count * 100, 2) : 0,
                    Group2CSRate = group2.Count > 0 ? Math.Round((decimal)group2.Count(c => c.IsCesareanSection) / group2.Count * 100, 2) : 0,
                    Group3CSRate = group3.Count > 0 ? Math.Round((decimal)group3.Count(c => c.IsCesareanSection) / group3.Count * 100, 2) : 0,
                    Group4CSRate = group4.Count > 0 ? Math.Round((decimal)group4.Count(c => c.IsCesareanSection) / group4.Count * 100, 2) : 0,
                    Group5CSRate = group5.Count > 0 ? Math.Round((decimal)group5.Count(c => c.IsCesareanSection) / group5.Count * 100, 2) : 0,
                    VBACRate = group5.Count > 0 ? Math.Round((decimal)group5.Count(c => !c.IsCesareanSection) / group5.Count * 100, 2) : 0
                });
            }

            return trends;
        }

        public async Task<RobsonComparativeReport> GetComparativeReportAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? regionId = null)
        {
            var report = new RobsonComparativeReport
            {
                StartDate = startDate,
                EndDate = endDate,
                GeneratedAt = DateTime.Now,
                ComparisonType = "Facilities"
            };

            // Get all facilities (filtered by region if specified)
            var facilitiesQuery = _context.Facilities
                .Include(f => f.District)
                    .ThenInclude(d => d!.Region)
                .AsQueryable();
            if (regionId.HasValue)
                facilitiesQuery = facilitiesQuery.Where(f => f.District != null && f.District.RegionID == regionId);

            var facilities = await facilitiesQuery.ToListAsync();

            var csRates = new List<decimal>();

            foreach (var facility in facilities)
            {
                var classifications = await GetClassificationsQuery(startDate, endDate, facility.ID)
                    .ToListAsync();

                if (classifications.Count == 0) continue;

                var totalDeliveries = classifications.Count;
                var totalCS = classifications.Count(c => c.IsCesareanSection);
                var csRate = Math.Round((decimal)totalCS / totalDeliveries * 100, 2);
                csRates.Add(csRate);

                var group1 = classifications.Where(c => c.Group == RobsonGroup.Group1).ToList();
                var group3 = classifications.Where(c => c.Group == RobsonGroup.Group3).ToList();
                var group5 = classifications.Where(c => c.Group == RobsonGroup.Group5).ToList();

                report.FacilityComparisons.Add(new FacilityRobsonSummary
                {
                    FacilityName = facility.Name,
                    FacilityCode = facility.Code,
                    FacilityType = facility.Type,
                    Region = facility.District?.Region?.Name ?? string.Empty,
                    TotalDeliveries = totalDeliveries,
                    TotalCS = totalCS,
                    Group1CSRate = group1.Count > 0 ? Math.Round((decimal)group1.Count(c => c.IsCesareanSection) / group1.Count * 100, 2) : 0,
                    Group3CSRate = group3.Count > 0 ? Math.Round((decimal)group3.Count(c => c.IsCesareanSection) / group3.Count * 100, 2) : 0,
                    Group5Proportion = totalDeliveries > 0 ? Math.Round((decimal)group5.Count / totalDeliveries * 100, 2) : 0,
                    Group5CSRate = group5.Count > 0 ? Math.Round((decimal)group5.Count(c => c.IsCesareanSection) / group5.Count * 100, 2) : 0,
                    PerformanceCategory = GetPerformanceCategory(csRate)
                });
            }

            if (csRates.Count > 0)
            {
                report.MeanCSRate = Math.Round(csRates.Average(), 2);
                report.MedianCSRate = GetMedian(csRates);
                report.StandardDeviation = CalculateStandardDeviation(csRates);

                report.HighCSRateFacilities = report.FacilityComparisons
                    .Where(f => f.OverallCSRate > report.MeanCSRate + report.StandardDeviation)
                    .Select(f => f.FacilityName)
                    .ToList();

                report.LowCSRateFacilities = report.FacilityComparisons
                    .Where(f => f.OverallCSRate < report.MeanCSRate - report.StandardDeviation)
                    .Select(f => f.FacilityName)
                    .ToList();
            }

            return report;
        }

        public async Task<RobsonQualityIndicators> GetQualityIndicatorsAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null)
        {
            var classifications = await GetClassificationsQuery(startDate, endDate, facilityId)
                .ToListAsync();

            var groupStats = GenerateGroupStatistics(classifications, classifications.Count,
                classifications.Count(c => c.IsCesareanSection));

            return GenerateQualityIndicators(classifications, groupStats);
        }

        public async Task<Group2SubAnalysis> GetGroup2AnalysisAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null)
        {
            var group2 = await GetClassificationsQuery(startDate, endDate, facilityId)
                .Where(c => c.Group == RobsonGroup.Group2)
                .ToListAsync();

            var group2a = group2.Where(c => c.LaborOnset == LaborOnsetType.Induced).ToList();
            var group2b = group2.Where(c => c.LaborOnset == LaborOnsetType.CesareanBeforeLabor).ToList();

            return new Group2SubAnalysis
            {
                Group2aTotal = group2a.Count,
                Group2aCesareans = group2a.Count(c => c.IsCesareanSection),
                Group2bTotal = group2b.Count,
                Group2bCesareans = group2b.Count // All are CS by definition
            };
        }

        public async Task<Group5SubAnalysis> GetGroup5AnalysisAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null)
        {
            var group5 = await GetClassificationsQuery(startDate, endDate, facilityId)
                .Where(c => c.Group == RobsonGroup.Group5)
                .ToListAsync();

            var group51 = group5.Where(c => c.LaborOnset == LaborOnsetType.Spontaneous).ToList();
            var group52 = group5.Where(c => c.LaborOnset == LaborOnsetType.Induced).ToList();
            var group53 = group5.Where(c => c.LaborOnset == LaborOnsetType.CesareanBeforeLabor).ToList();

            return new Group5SubAnalysis
            {
                Group51Total = group51.Count,
                Group51Cesareans = group51.Count(c => c.IsCesareanSection),
                Group52Total = group52.Count,
                Group52Cesareans = group52.Count(c => c.IsCesareanSection),
                Group53Total = group53.Count,
                Group53Cesareans = group53.Count,
                OnePreviousCS = group5.Count(c => c.NumberOfPreviousCesareans == 1),
                TwoPreviousCS = group5.Count(c => c.NumberOfPreviousCesareans == 2),
                ThreeOrMorePreviousCS = group5.Count(c => c.NumberOfPreviousCesareans >= 3)
            };
        }

        public async Task<List<RobsonCaseRecord>> GetCaseRecordsAsync(
            DateTime startDate,
            DateTime endDate,
            RobsonGroup? group = null,
            Guid? facilityId = null,
            int pageSize = 100,
            int page = 1)
        {
            var query = GetClassificationsQuery(startDate, endDate, facilityId);

            if (group.HasValue)
                query = query.Where(c => c.Group == group.Value);

            var classifications = await query
                .Include(c => c.Partograph)
                    .ThenInclude(p => p!.Patient)
                .OrderByDescending(c => c.ClassifiedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return classifications.Select(c => new RobsonCaseRecord
            {
                PartographID = c.PartographID ?? Guid.Empty,
                PatientName = c.Partograph?.Patient?.Name ?? "Unknown",
                HospitalNumber = c.Partograph?.Patient?.HospitalNumber ?? "",
                DeliveryDate = c.ClassifiedAt,
                Age = c.Partograph?.Patient?.Age ?? 0,
                Parity = c.Parity,
                GestationalAgeWeeks = c.GestationalAgeWeeks,
                HasPreviousCS = c.HasPreviousCesarean,
                NumberOfPreviousCS = c.NumberOfPreviousCesareans,
                LaborOnset = c.LaborOnset,
                FetalPresentation = c.FetalPresentation,
                NumberOfFetuses = c.NumberOfFetuses,
                Group = c.Group,
                DeliveryMode = c.DeliveryMode,
                CSIndication = c.CesareanIndication,
                CSType = c.CesareanType,
                ClassificationSummary = c.ClassificationSummary
            }).ToList();
        }

        public async Task<List<RobsonActionItem>> GetActionItemsAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null)
        {
            var classifications = await GetClassificationsQuery(startDate, endDate, facilityId)
                .ToListAsync();

            var totalDeliveries = classifications.Count;
            var totalCS = classifications.Count(c => c.IsCesareanSection);
            var overallCSRate = totalDeliveries > 0 ? (decimal)totalCS / totalDeliveries * 100 : 0;

            var actionItems = new List<RobsonActionItem>();

            // Check overall CS rate
            if (overallCSRate > 15)
            {
                actionItems.Add(new RobsonActionItem
                {
                    Category = "Clinical",
                    Priority = overallCSRate > 25 ? "High" : "Medium",
                    Description = $"Overall CS rate ({overallCSRate:F1}%) exceeds WHO recommended range",
                    Recommendation = "Review CS indications and consider audit of emergency vs elective CS"
                });
            }

            // Check Group 1 CS rate
            var group1 = classifications.Where(c => c.Group == RobsonGroup.Group1).ToList();
            if (group1.Count > 0)
            {
                var group1CSRate = (decimal)group1.Count(c => c.IsCesareanSection) / group1.Count * 100;
                if (group1CSRate > 10)
                {
                    actionItems.Add(new RobsonActionItem
                    {
                        Category = "Clinical",
                        Priority = "High",
                        Description = $"Group 1 CS rate ({group1CSRate:F1}%) exceeds benchmark of 10%",
                        Recommendation = "Review labor management protocols for nulliparous women in spontaneous labor",
                        RelatedGroup = RobsonGroup.Group1
                    });
                }
            }

            // Check Group 3 CS rate
            var group3 = classifications.Where(c => c.Group == RobsonGroup.Group3).ToList();
            if (group3.Count > 0)
            {
                var group3CSRate = (decimal)group3.Count(c => c.IsCesareanSection) / group3.Count * 100;
                if (group3CSRate > 3)
                {
                    actionItems.Add(new RobsonActionItem
                    {
                        Category = "Audit",
                        Priority = "High",
                        Description = $"Group 3 CS rate ({group3CSRate:F1}%) exceeds benchmark of 3%",
                        Recommendation = "Audit all cesareans in multiparous women with spontaneous labor",
                        RelatedGroup = RobsonGroup.Group3
                    });
                }
            }

            // Check Group 5 proportion
            var group5 = classifications.Where(c => c.Group == RobsonGroup.Group5).ToList();
            if (totalDeliveries > 0)
            {
                var group5Proportion = (decimal)group5.Count / totalDeliveries * 100;
                if (group5Proportion > 10)
                {
                    actionItems.Add(new RobsonActionItem
                    {
                        Category = "Clinical",
                        Priority = "Medium",
                        Description = $"Group 5 proportion ({group5Proportion:F1}%) exceeds 10%",
                        Recommendation = "Focus on reducing primary cesareans to decrease future Group 5 burden",
                        RelatedGroup = RobsonGroup.Group5
                    });
                }

                // Check VBAC rate
                if (group5.Count > 0)
                {
                    var vbacRate = (decimal)group5.Count(c => !c.IsCesareanSection) / group5.Count * 100;
                    if (vbacRate < 20)
                    {
                        actionItems.Add(new RobsonActionItem
                        {
                            Category = "Training",
                            Priority = "Medium",
                            Description = $"VBAC rate ({vbacRate:F1}%) is low",
                            Recommendation = "Consider VBAC training and counseling programs for eligible women",
                            RelatedGroup = RobsonGroup.Group5
                        });
                    }
                }
            }

            return actionItems;
        }

        public async Task<RobsonClassification?> ClassifyDeliveryAsync(Guid partographId)
        {
            var partograph = await _context.Partographs
                .Include(p => p.Patient)
                .Include(p => p.FetalPositions)
                .FirstOrDefaultAsync(p => p.ID == partographId);

            if (partograph == null) return null;

            // Check if already classified
            var existing = await _context.RobsonClassifications
                .FirstOrDefaultAsync(r => r.PartographID == partographId);

            if (existing != null) return existing;

            // Get birth outcome for delivery mode
            var birthOutcome = await _context.BirthOutcomes
                .FirstOrDefaultAsync(b => b.PartographID == partographId);

            var babyDetails = await _context.BabyDetails
                .Where(b => b.PartographID == partographId)
                .ToListAsync();

            // Create classification
            var classification = new RobsonClassification
            {
                ID = Guid.NewGuid(),
                PartographID = partographId,
                ClassifiedAt = DateTime.Now,
                Parity = partograph.Parity,
                HasPreviousCesarean = partograph.Patient?.HasPreviousCSection ?? false,
                NumberOfPreviousCesareans = partograph.Patient?.NumberOfPreviousCsections ?? 0,
                GestationalAgeWeeks = GetGestationalWeeks(partograph),
                LaborOnset = partograph.LaborOnset,
                FetalPresentation = GetFetalPresentation(partograph),
                NumberOfFetuses = birthOutcome?.NumberOfBabies ?? babyDetails.Count,
                DeliveryMode = GetDeliveryMode(birthOutcome),
                IsAutoClassified = true,
                ValidationStatus = ClassificationValidationStatus.Validated,
                CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // Calculate group
            classification.CalculateRobsonGroup();

            _context.RobsonClassifications.Add(classification);
            await _context.SaveChangesAsync();

            return classification;
        }

        public async Task<BatchClassifyResponse> BatchClassifyDeliveriesAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null)
        {
            var response = new BatchClassifyResponse();

            // Get completed partographs without classification
            var query = _context.Partographs 
                .Include(p => p.Patient) 
                .Include(p => p.FetalPositions)
                .Where(p => p.Status == LaborStatus.Completed)
                .Where(p => p.DeliveryTime >= startDate && p.DeliveryTime <= endDate);

            if (facilityId.HasValue)
                query = query.Where(p => p.FacilityID == facilityId);

            var partographs = await query.ToListAsync();

            // Get already classified IDs
            var classifiedIds = await _context.RobsonClassifications
                .Where(r => r.PartographID != null)
                .Select(r => r.PartographID)
                .ToListAsync();

            foreach (var partograph in partographs)
            {
                if (classifiedIds.Contains(partograph.ID))
                {
                    response.AlreadyClassifiedCount++;
                    continue;
                }

                try
                {
                    var classification = await ClassifyDeliveryAsync(partograph.ID!.Value);
                    if (classification != null)
                        response.ClassifiedCount++;
                    else
                        response.FailedCount++;
                }
                catch (Exception ex)
                {
                    response.FailedCount++;
                    response.Errors.Add($"Failed to classify {partograph.ID}: {ex.Message}");
                }
            }

            return response;
        }

        #region Private Helper Methods

        private IQueryable<RobsonClassification> GetClassificationsQuery(
            DateTime startDate,
            DateTime endDate,
            Guid? facilityId = null,
            Guid? regionId = null,
            Guid? districtId = null)
        {
            var query = _context.RobsonClassifications
                .Include(r => r.Partograph)
                .Where(r => r.ClassifiedAt >= startDate && r.ClassifiedAt <= endDate);

            // Filter by facility - use direct Partograph.FacilityID
            if (facilityId.HasValue)
            {
                query = query
                    .Where(r => r.Partograph != null && r.Partograph.FacilityID == facilityId);
            }

            // Filter by region - join through Partograph.FacilityID to Facility to get RegionID
            if (regionId.HasValue)
            {
                query = query
                    .Where(r => r.Partograph != null && r.Partograph.FacilityID != null)
                    .Join(_context.Facilities.Include(f => f.District),
                        r => r.Partograph!.FacilityID,
                        f => f.ID,
                        (r, f) => new { Classification = r, Facility = f })
                    .Where(x => x.Facility.District != null && x.Facility.District.RegionID == regionId)
                    .Select(x => x.Classification);
            }

            // Filter by district - join through Partograph.FacilityID to Facility to get DistrictID
            if (districtId.HasValue)
            {
                query = query
                    .Where(r => r.Partograph != null && r.Partograph.FacilityID != null)
                    .Join(_context.Facilities,
                        r => r.Partograph!.FacilityID,
                        f => f.ID,
                        (r, f) => new { Classification = r, Facility = f })
                    .Where(x => x.Facility.DistrictID == districtId)
                    .Select(x => x.Classification);
            }

            return query;
        }

        private List<RobsonGroupStatistics> GenerateGroupStatistics(
            List<RobsonClassification> classifications,
            int totalDeliveries,
            int totalCesareans)
        {
            var stats = new List<RobsonGroupStatistics>();

            for (int i = 1; i <= 10; i++)
            {
                var group = (RobsonGroup)i;
                var groupData = classifications.Where(c => c.Group == group).ToList();
                var groupCS = groupData.Count(c => c.IsCesareanSection);

                var stat = new RobsonGroupStatistics
                {
                    Group = group,
                    TotalInGroup = groupData.Count,
                    CesareansInGroup = groupCS,
                    RelativeGroupSize = totalDeliveries > 0
                        ? Math.Round((decimal)groupData.Count / totalDeliveries * 100, 2) : 0,
                    AbsoluteContribution = totalDeliveries > 0
                        ? Math.Round((decimal)groupCS / totalDeliveries * 100, 2) : 0,
                    RelativeContribution = totalCesareans > 0
                        ? Math.Round((decimal)groupCS / totalCesareans * 100, 2) : 0,
                    WHOExpectedCSRate = GetWHOExpectedCSRate(group),
                    WHOExpectedGroupSize = GetWHOExpectedGroupSize(group)
                };

                stats.Add(stat);
            }

            return stats;
        }

        private RobsonQualityIndicators GenerateQualityIndicators(
            List<RobsonClassification> classifications,
            List<RobsonGroupStatistics> groupStats)
        {
            var totalDeliveries = classifications.Count;
            var totalCS = classifications.Count(c => c.IsCesareanSection);
            var overallCSRate = totalDeliveries > 0 ? Math.Round((decimal)totalCS / totalDeliveries * 100, 2) : 0;

            var indicators = new RobsonQualityIndicators
            {
                OverallCSRate = overallCSRate,
                Group1CSRate = groupStats.FirstOrDefault(g => g.Group == RobsonGroup.Group1)?.CSRateWithinGroup ?? 0,
                Group3CSRate = groupStats.FirstOrDefault(g => g.Group == RobsonGroup.Group3)?.CSRateWithinGroup ?? 0,
                Group5CSRate = groupStats.FirstOrDefault(g => g.Group == RobsonGroup.Group5)?.CSRateWithinGroup ?? 0,
                Group5Proportion = groupStats.FirstOrDefault(g => g.Group == RobsonGroup.Group5)?.RelativeGroupSize ?? 0,
                DataCompletenessRate = totalDeliveries > 0
                    ? Math.Round((decimal)classifications.Count(c => c.Group != RobsonGroup.Unclassified) / totalDeliveries * 100, 2) : 0
            };

            // Calculate Robson Index (Groups 1, 2, 5 contribution)
            indicators.RobsonIndex = groupStats
                .Where(g => g.Group == RobsonGroup.Group1 || g.Group == RobsonGroup.Group2 || g.Group == RobsonGroup.Group5)
                .Sum(g => g.RelativeContribution);

            // Generate findings
            if (indicators.OverallCSRate > 15)
                indicators.KeyFindings.Add($"Overall CS rate ({indicators.OverallCSRate}%) exceeds WHO recommended range");
            if (!indicators.Group1WithinBenchmark)
                indicators.KeyFindings.Add($"Group 1 CS rate ({indicators.Group1CSRate}%) outside benchmark (5-10%)");
            if (!indicators.Group3WithinBenchmark)
                indicators.KeyFindings.Add($"Group 3 CS rate ({indicators.Group3CSRate}%) exceeds benchmark (≤3%)");
            if (!indicators.Group5ProportionAcceptable)
                indicators.KeyFindings.Add($"Group 5 proportion ({indicators.Group5Proportion}%) exceeds 10%");

            return indicators;
        }

        private List<RobsonGroupDistribution> GetGroupDistribution(List<RobsonClassification> classifications, int totalDeliveries)
        {
            var colors = new Dictionary<RobsonGroup, string>
            {
                { RobsonGroup.Group1, "#4CAF50" },
                { RobsonGroup.Group2, "#8BC34A" },
                { RobsonGroup.Group3, "#2196F3" },
                { RobsonGroup.Group4, "#03A9F4" },
                { RobsonGroup.Group5, "#FF9800" },
                { RobsonGroup.Group6, "#9C27B0" },
                { RobsonGroup.Group7, "#673AB7" },
                { RobsonGroup.Group8, "#E91E63" },
                { RobsonGroup.Group9, "#F44336" },
                { RobsonGroup.Group10, "#FFC107" }
            };

            var distribution = new List<RobsonGroupDistribution>();

            for (int i = 1; i <= 10; i++)
            {
                var group = (RobsonGroup)i;
                var count = classifications.Count(c => c.Group == group);
                distribution.Add(new RobsonGroupDistribution
                {
                    Group = group,
                    Count = count,
                    Percentage = totalDeliveries > 0 ? Math.Round((decimal)count / totalDeliveries * 100, 2) : 0,
                    Color = colors.GetValueOrDefault(group, "#808080")
                });
            }

            return distribution;
        }

        private int GetGestationalWeeks(Partograph partograph)
        {
            if (partograph.ExpectedDeliveryDate.HasValue)
            {
                var daysRemaining = (partograph.ExpectedDeliveryDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Now).Days;
                return 40 - (daysRemaining / 7); // Approximate
            }
            return 40; // Default to term
        }

        private FetalPresentationType GetFetalPresentation(Partograph partograph)
        {
            var latestPosition = partograph.FetalPositions?
                .OrderByDescending(f => f.Time)
                .FirstOrDefault();

            if (latestPosition == null) return FetalPresentationType.Cephalic;

            return latestPosition.Presentation?.ToLower() switch
            {
                "breech" => FetalPresentationType.Breech,
                "transverse" => FetalPresentationType.Transverse,
                "oblique" => FetalPresentationType.Oblique,
                _ => FetalPresentationType.Cephalic
            };
        }

        private DeliveryMode GetDeliveryMode(BirthOutcome? outcome)
        {
            if (outcome == null) return DeliveryMode.SpontaneousVaginal;

            return outcome?.DeliveryMode.ToString().ToLower() switch
            {
                "caesareansection" or "cesarean" or "cs" => DeliveryMode.CaesareanSection,
                "assistedvaginal" or "vacuum" or "forceps" => DeliveryMode.AssistedVaginal,
                "breechdelivery" or "breech" => DeliveryMode.BreechDelivery,
                _ => DeliveryMode.SpontaneousVaginal
            };
        }

        private decimal? GetWHOExpectedCSRate(RobsonGroup group)
        {
            return group switch
            {
                RobsonGroup.Group1 => 10m,
                RobsonGroup.Group2 => 35m,
                RobsonGroup.Group3 => 3m,
                RobsonGroup.Group4 => 20m,
                RobsonGroup.Group5 => 75m,
                RobsonGroup.Group6 => 90m,
                RobsonGroup.Group7 => 85m,
                RobsonGroup.Group8 => 60m,
                RobsonGroup.Group9 => 100m,
                RobsonGroup.Group10 => 30m,
                _ => null
            };
        }

        private decimal? GetWHOExpectedGroupSize(RobsonGroup group)
        {
            return group switch
            {
                RobsonGroup.Group1 => 35m,
                RobsonGroup.Group2 => 10m,
                RobsonGroup.Group3 => 30m,
                RobsonGroup.Group4 => 5m,
                RobsonGroup.Group5 => 8m,
                RobsonGroup.Group6 => 1.5m,
                RobsonGroup.Group7 => 1m,
                RobsonGroup.Group8 => 1.5m,
                RobsonGroup.Group9 => 0.5m,
                RobsonGroup.Group10 => 5m,
                _ => null
            };
        }

        private string GetPerformanceCategory(decimal csRate)
        {
            if (csRate >= 10 && csRate <= 15) return "Optimal";
            if (csRate > 15 && csRate <= 20) return "Acceptable";
            return "Needs Review";
        }

        private decimal GetMedian(List<decimal> values)
        {
            if (values.Count == 0) return 0;
            var sorted = values.OrderBy(v => v).ToList();
            int mid = sorted.Count / 2;
            return sorted.Count % 2 == 0
                ? (sorted[mid - 1] + sorted[mid]) / 2
                : sorted[mid];
        }

        private decimal CalculateStandardDeviation(List<decimal> values)
        {
            if (values.Count <= 1) return 0;
            var avg = values.Average();
            var sumOfSquares = values.Sum(v => (v - avg) * (v - avg));
            return (decimal)Math.Sqrt((double)(sumOfSquares / (values.Count - 1)));
        }

        #endregion
    }
}
