using MAAME.DROMO.PARTOGRAPH.MODEL;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Services
{
    public interface IPOCMetricsService
    {
        Task<POCProgress> GetPOCProgressAsync(Guid? facilityId = null, Guid? districtId = null, Guid? regionId = null);
        Task<POCProgress> GetNationalPOCProgressAsync();
        Task<List<POCProgress>> GetPOCProgressHistoryAsync(DateTime startDate, DateTime endDate, string periodType = "Monthly");
        Task<AdoptionMetrics> GetAdoptionMetricsAsync(Guid? facilityId = null);
        Task<SatisfactionMetrics> GetSatisfactionMetricsAsync(Guid? facilityId = null);
        Task<EmergencyReportingMetrics> GetEmergencyReportingMetricsAsync(Guid? facilityId = null);
        Task<ComplicationReductionMetrics> GetComplicationReductionMetricsAsync(Guid? facilityId = null);
        Task<ResponseTimeMetrics> GetResponseTimeMetricsAsync(Guid? facilityId = null);
        Task<POCBaseline> GetOrCreateBaselineAsync(Guid? facilityId = null);
        Task<POCBaseline> UpdateBaselineAsync(POCBaseline baseline);
        Task ComputeAndStorePOCProgressAsync(DateTime date);
    }

    public class POCMetricsService : IPOCMetricsService
    {
        private readonly PartographDbContext _context;
        private readonly ILogger<POCMetricsService> _logger;

        // POC Targets
        private const decimal ADOPTION_TARGET = 70.0m;
        private const decimal SATISFACTION_TARGET = 4.0m;
        private const decimal REPORTING_TARGET = 70.0m;
        private const decimal COMPLICATION_REDUCTION_TARGET = 15.0m;
        private const decimal RESPONSE_TIME_REDUCTION_TARGET = 30.0m;
        private const int REPORTING_TIME_THRESHOLD_MINUTES = 30;

        public POCMetricsService(PartographDbContext context, ILogger<POCMetricsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<POCProgress> GetPOCProgressAsync(Guid? facilityId = null, Guid? districtId = null, Guid? regionId = null)
        {
            var progress = new POCProgress
            {
                ID = Guid.NewGuid(),
                SnapshotDate = DateTime.UtcNow.Date,
                PeriodType = "Current",
                FacilityID = facilityId,
                DistrictID = districtId,
                RegionID = regionId
            };

            try
            {
                // Get baseline for comparison
                var baseline = await GetOrCreateBaselineAsync(facilityId);

                // Calculate all 5 POC metrics
                var adoptionMetrics = await GetAdoptionMetricsAsync(facilityId);
                var satisfactionMetrics = await GetSatisfactionMetricsAsync(facilityId);
                var reportingMetrics = await GetEmergencyReportingMetricsAsync(facilityId);
                var complicationMetrics = await GetComplicationReductionMetricsAsync(facilityId);
                var responseMetrics = await GetResponseTimeMetricsAsync(facilityId);

                // POC 1: Adoption
                progress.TotalHealthcareWorkers = adoptionMetrics.TotalHealthcareWorkers;
                progress.ActivePartographUsers = adoptionMetrics.ActivePartographUsers;
                progress.AdoptionRate = adoptionMetrics.AdoptionRate;
                progress.AdoptionTarget = ADOPTION_TARGET;

                // POC 2: Satisfaction
                progress.TotalSurveyResponses = satisfactionMetrics.TotalResponses;
                progress.AverageSatisfactionScore = satisfactionMetrics.OverallScore;
                progress.EaseOfUseAverage = satisfactionMetrics.EaseOfUseScore;
                progress.WorkflowImpactAverage = satisfactionMetrics.WorkflowImpactScore;
                progress.PerceivedBenefitsAverage = satisfactionMetrics.PerceivedBenefitsScore;
                progress.SatisfactionTarget = SATISFACTION_TARGET;

                // POC 3: Real-Time Reporting
                progress.TotalEmergencyCases = reportingMetrics.TotalEmergencyCases;
                progress.EmergenciesReportedWithin30Min = reportingMetrics.ReportedWithinTarget;
                progress.RealTimeReportingRate = reportingMetrics.ReportingRate;
                progress.AverageReportingTimeMinutes = reportingMetrics.AverageReportingTimeMinutes;
                progress.ReportingTarget = REPORTING_TARGET;

                // POC 4: Complication Reduction
                progress.TotalDeliveries = complicationMetrics.TotalDeliveries;
                progress.TotalComplications = complicationMetrics.TotalComplications;
                progress.ComplicationRate = complicationMetrics.CurrentComplicationRate;
                progress.BaselineComplicationRate = baseline?.BaselineComplicationRate ?? complicationMetrics.CurrentComplicationRate;
                progress.ComplicationReductionPercent = complicationMetrics.ReductionPercent;
                progress.PPHCases = complicationMetrics.PPHCases;
                progress.ObstructedLaborCases = complicationMetrics.ObstructedLaborCases;
                progress.BirthAsphyxiaCases = complicationMetrics.BirthAsphyxiaCases;
                progress.EclampsiaCases = complicationMetrics.EclampsiaCases;
                progress.ComplicationReductionTarget = COMPLICATION_REDUCTION_TARGET;

                // POC 5: Response Time
                progress.TotalEmergencyReferrals = responseMetrics.TotalEmergencyReferrals;
                progress.AverageTimeToReferralMinutes = responseMetrics.CurrentAverageMinutes;
                progress.BaselineTimeToReferralMinutes = baseline?.BaselineAverageTimeToReferralMinutes ?? responseMetrics.CurrentAverageMinutes;
                progress.ResponseTimeReductionPercent = responseMetrics.ReductionPercent;
                progress.ResponseTimeReductionTarget = RESPONSE_TIME_REDUCTION_TARGET;

                // Calculate overall progress
                progress.TargetsMet = 0;
                if (progress.AdoptionTargetMet) progress.TargetsMet++;
                if (progress.SatisfactionTargetMet) progress.TargetsMet++;
                if (progress.ReportingTargetMet) progress.TargetsMet++;
                if (progress.ComplicationTargetMet) progress.TargetsMet++;
                if (progress.ResponseTimeTargetMet) progress.TargetsMet++;

                progress.TotalTargets = 5;
                progress.OverallPOCProgress = (decimal)progress.TargetsMet / progress.TotalTargets * 100;

                progress.CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                progress.UpdatedTime = progress.CreatedTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating POC progress");
            }

            return progress;
        }

        public async Task<POCProgress> GetNationalPOCProgressAsync()
        {
            return await GetPOCProgressAsync(null, null, null);
        }

        public async Task<List<POCProgress>> GetPOCProgressHistoryAsync(DateTime startDate, DateTime endDate, string periodType = "Monthly")
        {
            return await _context.POCProgressRecords
                .Where(p => p.SnapshotDate >= startDate && p.SnapshotDate <= endDate && p.PeriodType == periodType)
                .OrderByDescending(p => p.SnapshotDate)
                .ToListAsync();
        }

        public async Task<AdoptionMetrics> GetAdoptionMetricsAsync(Guid? facilityId = null)
        {
            var metrics = new AdoptionMetrics();

            try
            {
                // Get total healthcare workers (staff who are active) - filter by facility if specified
                var staffQuery = _context.Staff.Where(s => s.Deleted == 0);
                if (facilityId.HasValue)
                {
                    staffQuery = staffQuery.Where(s => s.Facility == facilityId);
                }
                metrics.TotalHealthcareWorkers = await staffQuery.CountAsync();

                // Get active partograph users (staff who have completed partographs in last 30 days)
                var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds();

                // Count unique handlers who created partographs in last 30 days - filter by facility if specified
                var partographQuery = _context.Partographs
                    .Where(p => p.Deleted == 0 && p.CreatedTime >= thirtyDaysAgo && p.Handler != null);
                if (facilityId.HasValue)
                {
                    partographQuery = partographQuery.Where(p => p.FacilityID == facilityId);
                }

                var activeUserIds = await partographQuery
                    .Select(p => p.Handler)
                    .Distinct()
                    .CountAsync();

                metrics.ActivePartographUsers = activeUserIds;

                // Calculate adoption rate
                metrics.AdoptionRate = metrics.TotalHealthcareWorkers > 0
                    ? Math.Round((decimal)metrics.ActivePartographUsers / metrics.TotalHealthcareWorkers * 100, 1)
                    : 0;

                metrics.Target = ADOPTION_TARGET;

                // Get breakdown by role - filter by facility if specified
                var roleQuery = _context.Staff.Where(s => s.Deleted == 0);
                if (facilityId.HasValue)
                {
                    roleQuery = roleQuery.Where(s => s.Facility == facilityId);
                }

                var roleBreakdown = await roleQuery
                    .GroupBy(s => s.Role)
                    .Select(g => new RoleAdoption
                    {
                        Role = g.Key,
                        TotalStaff = g.Count()
                    })
                    .ToListAsync();

                metrics.RoleBreakdown = roleBreakdown;

                // Calculate usage patterns - filter by facility if specified
                var usageQuery = _context.Partographs
                    .Where(p => p.Deleted == 0 && p.CreatedTime >= thirtyDaysAgo);
                if (facilityId.HasValue)
                {
                    usageQuery = usageQuery.Where(p => p.FacilityID == facilityId);
                }

                metrics.AveragePartographsPerUser = activeUserIds > 0
                    ? await usageQuery.CountAsync() / (decimal)activeUserIds
                    : 0;

                // Users who completed 5+ partographs (regular users) - filter by facility if specified
                var regularUsersQuery = _context.Partographs
                    .Where(p => p.Deleted == 0 && p.CreatedTime >= thirtyDaysAgo && p.Handler != null);
                if (facilityId.HasValue)
                {
                    regularUsersQuery = regularUsersQuery.Where(p => p.FacilityID == facilityId);
                }

                var regularUsers = await regularUsersQuery
                    .GroupBy(p => p.Handler)
                    .Where(g => g.Count() >= 5)
                    .CountAsync();

                metrics.RegularUsers = regularUsers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating adoption metrics");
            }

            return metrics;
        }

        public async Task<SatisfactionMetrics> GetSatisfactionMetricsAsync(Guid? facilityId = null)
        {
            var metrics = new SatisfactionMetrics();

            try
            {
                var responseQuery = _context.SurveyResponses.Where(r => r.Deleted == 0);

                if (facilityId.HasValue)
                {
                    responseQuery = responseQuery.Where(r => r.FacilityID == facilityId);
                }

                metrics.TotalResponses = await responseQuery.CountAsync();

                if (metrics.TotalResponses > 0)
                {
                    metrics.OverallScore = Math.Round(await responseQuery.AverageAsync(r => r.OverallSatisfactionScore), 2);

                    var withEaseOfUse = responseQuery.Where(r => r.EaseOfUseScore.HasValue);
                    if (await withEaseOfUse.AnyAsync())
                    {
                        metrics.EaseOfUseScore = Math.Round((decimal)await withEaseOfUse.AverageAsync(r => r.EaseOfUseScore!.Value), 2);
                    }

                    var withWorkflow = responseQuery.Where(r => r.WorkflowImpactScore.HasValue);
                    if (await withWorkflow.AnyAsync())
                    {
                        metrics.WorkflowImpactScore = Math.Round((decimal)await withWorkflow.AverageAsync(r => r.WorkflowImpactScore!.Value), 2);
                    }

                    var withBenefits = responseQuery.Where(r => r.PerceivedBenefitsScore.HasValue);
                    if (await withBenefits.AnyAsync())
                    {
                        metrics.PerceivedBenefitsScore = Math.Round((decimal)await withBenefits.AverageAsync(r => r.PerceivedBenefitsScore!.Value), 2);
                    }

                    // Score distribution
                    metrics.ScoreDistribution = await responseQuery
                        .GroupBy(r => (int)Math.Floor(r.OverallSatisfactionScore))
                        .Select(g => new ScoreCount { Score = g.Key, Count = g.Count() })
                        .OrderBy(s => s.Score)
                        .ToListAsync();

                    // Recent trend (last 3 months)
                    var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
                    var recentResponses = await responseQuery
                        .Where(r => r.SubmittedAt >= threeMonthsAgo)
                        .ToListAsync();

                    metrics.RecentTrendScore = recentResponses.Any()
                        ? Math.Round((decimal)recentResponses.Average(r => (double)r.OverallSatisfactionScore), 2)
                        : metrics.OverallScore;
                }

                metrics.Target = SATISFACTION_TARGET;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating satisfaction metrics");
            }

            return metrics;
        }

        public async Task<EmergencyReportingMetrics> GetEmergencyReportingMetricsAsync(Guid? facilityId = null)
        {
            var metrics = new EmergencyReportingMetrics();

            try
            {
                // Get emergency referrals from the last 30 days
                var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds();

                var emergencyReferrals = await _context.Referrals
                    .Where(r => r.Deleted == 0
                        && r.CreatedTime >= thirtyDaysAgo
                        && r.Urgency == ReferralUrgency.Emergency)
                    .ToListAsync();

                metrics.TotalEmergencyCases = emergencyReferrals.Count;

                // Count those reported within 30 minutes
                // Reporting time = time from ReferralTime to when status changed to Accepted or InTransit
                var reportedWithinTarget = emergencyReferrals.Count(r =>
                {
                    if (r.AcceptedTime.HasValue)
                    {
                        var reportingTime = (r.AcceptedTime.Value - r.ReferralTime).TotalMinutes;
                        return reportingTime <= REPORTING_TIME_THRESHOLD_MINUTES;
                    }
                    return false;
                });

                metrics.ReportedWithinTarget = reportedWithinTarget;

                metrics.ReportingRate = metrics.TotalEmergencyCases > 0
                    ? Math.Round((decimal)metrics.ReportedWithinTarget / metrics.TotalEmergencyCases * 100, 1)
                    : 0;

                // Calculate average reporting time
                var validTimes = emergencyReferrals
                    .Where(r => r.AcceptedTime.HasValue)
                    .Select(r => (r.AcceptedTime!.Value - r.ReferralTime).TotalMinutes)
                    .ToList();

                metrics.AverageReportingTimeMinutes = validTimes.Any()
                    ? Math.Round((decimal)validTimes.Average(), 1)
                    : 0;

                metrics.Target = REPORTING_TARGET;
                metrics.TimeThresholdMinutes = REPORTING_TIME_THRESHOLD_MINUTES;

                // Also check alert response times
                var recentAlerts = await _context.AlertSummaries
                    .Where(a => a.AlertDateTime >= DateTime.UtcNow.AddDays(-30)
                        && a.AlertSeverity == "Critical")
                    .ToListAsync();

                var alertsWithResponse = recentAlerts
                    .Where(a => a.ResponseTimeMinutes.HasValue)
                    .Select(a => a.ResponseTimeMinutes!.Value)
                    .ToList();

                metrics.AverageAlertResponseMinutes = alertsWithResponse.Any()
                    ? Math.Round((decimal)alertsWithResponse.Average(), 1)
                    : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating emergency reporting metrics");
            }

            return metrics;
        }

        public async Task<ComplicationReductionMetrics> GetComplicationReductionMetricsAsync(Guid? facilityId = null)
        {
            var metrics = new ComplicationReductionMetrics();

            try
            {
                // Get baseline
                var baseline = await GetOrCreateBaselineAsync(facilityId);

                // Get current period data (last 30 days)
                var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds();

                var birthOutcomes = await _context.BirthOutcomes
                    .Where(b => b.Deleted == 0 && b.CreatedTime >= thirtyDaysAgo)
                    .ToListAsync();

                metrics.TotalDeliveries = birthOutcomes.Count;

                // Count complications
                metrics.PPHCases = birthOutcomes.Count(b => b.PostpartumHemorrhage);
                metrics.ObstructedLaborCases = birthOutcomes.Count(b => b.ObstructedLabor);
                metrics.EclampsiaCases = birthOutcomes.Count(b => b.Eclampsia);

                // Get neonatal outcomes for birth asphyxia
                var neonatalOutcomes = await _context.NeonatalOutcomeRecords
                    .Where(n => n.CreatedTime >= thirtyDaysAgo)
                    .ToListAsync();

                metrics.BirthAsphyxiaCases = neonatalOutcomes.Count(n => n.CauseCategory == "Asphyxia" || n.LowAPGAR);

                metrics.TotalComplications = metrics.PPHCases + metrics.ObstructedLaborCases +
                    metrics.EclampsiaCases + metrics.BirthAsphyxiaCases;

                // Calculate rates
                metrics.CurrentComplicationRate = metrics.TotalDeliveries > 0
                    ? Math.Round((decimal)metrics.TotalComplications / metrics.TotalDeliveries * 100, 2)
                    : 0;

                metrics.BaselineComplicationRate = baseline?.BaselineComplicationRate ?? metrics.CurrentComplicationRate;

                // Calculate reduction
                if (metrics.BaselineComplicationRate > 0)
                {
                    metrics.ReductionPercent = Math.Round(
                        (metrics.BaselineComplicationRate - metrics.CurrentComplicationRate) / metrics.BaselineComplicationRate * 100,
                        1);
                }

                metrics.Target = COMPLICATION_REDUCTION_TARGET;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating complication reduction metrics");
            }

            return metrics;
        }

        public async Task<ResponseTimeMetrics> GetResponseTimeMetricsAsync(Guid? facilityId = null)
        {
            var metrics = new ResponseTimeMetrics();

            try
            {
                // Get baseline
                var baseline = await GetOrCreateBaselineAsync(facilityId);

                // Get emergency referrals from last 30 days
                var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds();

                var emergencyReferrals = await _context.Referrals
                    .Where(r => r.Deleted == 0
                        && r.CreatedTime >= thirtyDaysAgo
                        && r.Urgency == ReferralUrgency.Emergency
                        && r.ArrivalTime.HasValue)
                    .ToListAsync();

                metrics.TotalEmergencyReferrals = emergencyReferrals.Count;

                // Calculate time from identification (ReferralTime) to arrival at referral facility
                var responseTimes = emergencyReferrals
                    .Where(r => r.ArrivalTime.HasValue)
                    .Select(r => (r.ArrivalTime!.Value - r.ReferralTime).TotalMinutes)
                    .Where(t => t > 0 && t < 1440) // Filter out invalid times (> 24 hours)
                    .ToList();

                metrics.CurrentAverageMinutes = responseTimes.Any()
                    ? Math.Round((decimal)responseTimes.Average(), 1)
                    : 0;

                metrics.MedianMinutes = responseTimes.Any()
                    ? (decimal)responseTimes.OrderBy(t => t).ElementAt(responseTimes.Count / 2)
                    : 0;

                metrics.MinMinutes = responseTimes.Any() ? (decimal)responseTimes.Min() : 0;
                metrics.MaxMinutes = responseTimes.Any() ? (decimal)responseTimes.Max() : 0;

                metrics.BaselineAverageMinutes = baseline?.BaselineAverageTimeToReferralMinutes ?? metrics.CurrentAverageMinutes;

                // Calculate reduction
                if (metrics.BaselineAverageMinutes > 0)
                {
                    metrics.ReductionPercent = Math.Round(
                        (metrics.BaselineAverageMinutes - metrics.CurrentAverageMinutes) / metrics.BaselineAverageMinutes * 100,
                        1);
                }

                metrics.Target = RESPONSE_TIME_REDUCTION_TARGET;

                // Time breakdown
                var withDepartureTime = emergencyReferrals.Where(r => r.DepartureTime.HasValue).ToList();
                if (withDepartureTime.Any())
                {
                    metrics.AverageDecisionToDispatchMinutes = Math.Round(
                        (decimal)withDepartureTime.Average(r => (r.DepartureTime!.Value - r.ReferralTime).TotalMinutes),
                        1);

                    var withArrival = withDepartureTime.Where(r => r.ArrivalTime.HasValue).ToList();
                    if (withArrival.Any())
                    {
                        metrics.AverageTransportMinutes = Math.Round(
                            (decimal)withArrival.Average(r => (r.ArrivalTime!.Value - r.DepartureTime!.Value).TotalMinutes),
                            1);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating response time metrics");
            }

            return metrics;
        }

        public async Task<POCBaseline> GetOrCreateBaselineAsync(Guid? facilityId = null)
        {
            var baseline = await _context.POCBaselines
                .Where(b => b.Deleted == 0 && b.FacilityID == facilityId)
                .OrderByDescending(b => b.CreatedTime)
                .FirstOrDefaultAsync();

            if (baseline == null)
            {
                // Create default baseline from historical data if available
                baseline = new POCBaseline
                {
                    ID = Guid.NewGuid(),
                    FacilityID = facilityId,
                    BaselinePeriodStart = DateTime.UtcNow.AddYears(-1),
                    BaselinePeriodEnd = DateTime.UtcNow.AddMonths(-1),
                    DataSource = "Auto-generated from historical data",
                    CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                // Calculate baseline from historical data
                var oneYearAgo = DateTimeOffset.UtcNow.AddYears(-1).ToUnixTimeMilliseconds();
                var oneMonthAgo = DateTimeOffset.UtcNow.AddMonths(-1).ToUnixTimeMilliseconds();

                var historicalBirthOutcomes = await _context.BirthOutcomes
                    .Where(b => b.Deleted == 0 && b.CreatedTime >= oneYearAgo && b.CreatedTime <= oneMonthAgo)
                    .ToListAsync();

                baseline.BaselineTotalDeliveries = historicalBirthOutcomes.Count;
                baseline.BaselinePPHCases = historicalBirthOutcomes.Count(b => b.PostpartumHemorrhage);
                baseline.BaselineObstructedLaborCases = historicalBirthOutcomes.Count(b => b.ObstructedLabor);

                var totalComplications = baseline.BaselinePPHCases + baseline.BaselineObstructedLaborCases;
                baseline.BaselineComplicationRate = baseline.BaselineTotalDeliveries > 0
                    ? Math.Round((decimal)totalComplications / baseline.BaselineTotalDeliveries * 100, 2)
                    : 10.0m; // Default baseline if no data

                // Baseline referral time
                var historicalReferrals = await _context.Referrals
                    .Where(r => r.Deleted == 0
                        && r.CreatedTime >= oneYearAgo
                        && r.CreatedTime <= oneMonthAgo
                        && r.Urgency == ReferralUrgency.Emergency
                        && r.ArrivalTime.HasValue)
                    .ToListAsync();

                baseline.BaselineTotalReferrals = historicalReferrals.Count;

                var historicalResponseTimes = historicalReferrals
                    .Select(r => (r.ArrivalTime!.Value - r.ReferralTime).TotalMinutes)
                    .Where(t => t > 0 && t < 1440)
                    .ToList();

                baseline.BaselineAverageTimeToReferralMinutes = historicalResponseTimes.Any()
                    ? Math.Round((decimal)historicalResponseTimes.Average(), 1)
                    : 90.0m; // Default baseline if no data

                _context.POCBaselines.Add(baseline);
                await _context.SaveChangesAsync();
            }

            return baseline;
        }

        public async Task<POCBaseline> UpdateBaselineAsync(POCBaseline baseline)
        {
            var existing = await _context.POCBaselines.FindAsync(baseline.ID);
            if (existing != null)
            {
                existing.BaselineComplicationRate = baseline.BaselineComplicationRate;
                existing.BaselinePPHCases = baseline.BaselinePPHCases;
                existing.BaselineObstructedLaborCases = baseline.BaselineObstructedLaborCases;
                existing.BaselineBirthAsphyxiaCases = baseline.BaselineBirthAsphyxiaCases;
                existing.BaselineTotalDeliveries = baseline.BaselineTotalDeliveries;
                existing.BaselineAverageTimeToReferralMinutes = baseline.BaselineAverageTimeToReferralMinutes;
                existing.BaselineTotalReferrals = baseline.BaselineTotalReferrals;
                existing.Notes = baseline.Notes;
                existing.IsApproved = baseline.IsApproved;
                existing.ApprovedBy = baseline.ApprovedBy;
                existing.ApprovedDate = baseline.ApprovedDate;
                existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                await _context.SaveChangesAsync();
                return existing;
            }

            return baseline;
        }

        public async Task ComputeAndStorePOCProgressAsync(DateTime date)
        {
            try
            {
                var progress = await GetPOCProgressAsync();
                progress.SnapshotDate = date.Date;
                progress.PeriodType = "Daily";

                // Check if already exists for this date
                var existing = await _context.POCProgressRecords
                    .FirstOrDefaultAsync(p => p.SnapshotDate == date.Date && p.PeriodType == "Daily");

                if (existing != null)
                {
                    // Update existing
                    _context.Entry(existing).CurrentValues.SetValues(progress);
                    existing.ID = existing.ID; // Keep original ID
                    existing.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
                else
                {
                    _context.POCProgressRecords.Add(progress);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing and storing POC progress for {Date}", date);
            }
        }
    }

    // Supporting DTOs
    public class AdoptionMetrics
    {
        public int TotalHealthcareWorkers { get; set; }
        public int ActivePartographUsers { get; set; }
        public decimal AdoptionRate { get; set; }
        public decimal Target { get; set; }
        public bool TargetMet => AdoptionRate >= Target;
        public int RegularUsers { get; set; } // Users with 5+ partographs
        public decimal AveragePartographsPerUser { get; set; }
        public List<RoleAdoption> RoleBreakdown { get; set; } = new();
    }

    public class RoleAdoption
    {
        public string Role { get; set; } = string.Empty;
        public int TotalStaff { get; set; }
        public int ActiveUsers { get; set; }
        public decimal AdoptionRate => TotalStaff > 0 ? Math.Round((decimal)ActiveUsers / TotalStaff * 100, 1) : 0;
    }

    public class SatisfactionMetrics
    {
        public int TotalResponses { get; set; }
        public decimal OverallScore { get; set; }
        public decimal Target { get; set; }
        public bool TargetMet => OverallScore >= Target;
        public decimal? EaseOfUseScore { get; set; }
        public decimal? WorkflowImpactScore { get; set; }
        public decimal? PerceivedBenefitsScore { get; set; }
        public decimal? TrainingAdequacyScore { get; set; }
        public decimal RecentTrendScore { get; set; }
        public List<ScoreCount> ScoreDistribution { get; set; } = new();
    }

    public class ScoreCount
    {
        public int Score { get; set; }
        public int Count { get; set; }
    }

    public class EmergencyReportingMetrics
    {
        public int TotalEmergencyCases { get; set; }
        public int ReportedWithinTarget { get; set; }
        public decimal ReportingRate { get; set; }
        public decimal Target { get; set; }
        public bool TargetMet => ReportingRate >= Target;
        public int TimeThresholdMinutes { get; set; }
        public decimal AverageReportingTimeMinutes { get; set; }
        public decimal AverageAlertResponseMinutes { get; set; }
    }

    public class ComplicationReductionMetrics
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

    public class ResponseTimeMetrics
    {
        public int TotalEmergencyReferrals { get; set; }
        public decimal CurrentAverageMinutes { get; set; }
        public decimal BaselineAverageMinutes { get; set; }
        public decimal ReductionPercent { get; set; }
        public decimal Target { get; set; }
        public bool TargetMet => ReductionPercent >= Target;
        public decimal MedianMinutes { get; set; }
        public decimal MinMinutes { get; set; }
        public decimal MaxMinutes { get; set; }
        public decimal AverageDecisionToDispatchMinutes { get; set; }
        public decimal AverageTransportMinutes { get; set; }
    }
}
