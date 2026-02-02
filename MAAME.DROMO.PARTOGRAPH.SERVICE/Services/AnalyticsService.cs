using MAAME.DROMO.PARTOGRAPH.MODEL;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Models;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Services
{
    public interface IAnalyticsService
    {
        Task<object> GetDashboardStatsAsync(DateTime startDate, DateTime endDate, Guid? facilityId = null);
        Task<object> GetRealtimeStatsAsync(Guid? facilityId = null);
        Task ComputeDailyStatsAsync(DateTime date, Guid? facilityId = null);
        Task ComputeMonthlyStatsAsync(int year, int month, Guid? facilityId = null);
        Task ComputeFacilityPerformanceAsync(Guid? facilityId = null);
        Task ProcessDeliveryOutcomeAsync(Guid partographId);
        Task ProcessComplicationAsync(Guid partographId, string complicationType);
        Task ProcessReferralAsync(Guid referralId);
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly PartographDbContext _context;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(PartographDbContext context, ILogger<AnalyticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<object> GetDashboardStatsAsync(DateTime startDate, DateTime endDate, Guid? facilityId = null)
        {
            var startTimestamp = new DateTimeOffset(startDate).ToUnixTimeMilliseconds();
            var endTimestamp = new DateTimeOffset(endDate).ToUnixTimeMilliseconds();

            // Core counts - filter by facility if specified
            var patientQuery = _context.Patients.Where(p => p.Deleted == 0);
            var partographQuery = _context.Partographs.Where(p => p.Deleted == 0);
            var staffQuery = _context.Staff.Where(s => s.Deleted == 0);

            if (facilityId.HasValue)
            {
                patientQuery = patientQuery.Where(p => p.FacilityID == facilityId);
                partographQuery = partographQuery.Where(p => p.FacilityID == facilityId);
                staffQuery = staffQuery.Where(s => s.Facility == facilityId);
            }

            var patientCount = await patientQuery.CountAsync();
            var partographCount = await partographQuery.CountAsync();
            var staffCount = await staffQuery.CountAsync();
            var facilityCount = facilityId.HasValue ? 1 : await _context.Facilities.CountAsync(f => f.Deleted == 0);

            // Active labors (partographs not completed within date range) - filter by facility
            var activeLabors = await partographQuery
                .Where(p => p.Status != LaborStatus.Completed)
                .CountAsync();

            // Birth outcomes in date range - join through Partograph to filter by facility
            var birthOutcomesQuery = _context.BirthOutcomes
                .Include(b => b.Partograph)
                .Where(b => b.Deleted == 0 && b.CreatedTime >= startTimestamp && b.CreatedTime <= endTimestamp);

            if (facilityId.HasValue)
            {
                birthOutcomesQuery = birthOutcomesQuery.Where(b => b.Partograph != null && b.Partograph.FacilityID == facilityId);
            }

            var birthOutcomes = await birthOutcomesQuery.ToListAsync();

            var totalDeliveries = birthOutcomes.Count;
            var svdCount = birthOutcomes.Count(b => b.DeliveryMode == DeliveryMode.SpontaneousVaginal);
            var csCount = birthOutcomes.Count(b => b.DeliveryMode == DeliveryMode.CaesareanSection);
            var assistedCount = birthOutcomes.Count(b => b.DeliveryMode == DeliveryMode.AssistedVaginal);

            // Baby details - join through Partograph to filter by facility
            var babyDetailsQuery = _context.BabyDetails
                .Include(b => b.Partograph)
                .Where(b => b.Deleted == 0 && b.CreatedTime >= startTimestamp && b.CreatedTime <= endTimestamp);

            if (facilityId.HasValue)
            {
                babyDetailsQuery = babyDetailsQuery.Where(b => b.Partograph != null && b.Partograph.FacilityID == facilityId);
            }

            var babyDetails = await babyDetailsQuery.ToListAsync();

            var liveBirths = babyDetails.Count(b => b.VitalStatus == BabyVitalStatus.LiveBirth || b.VitalStatus == BabyVitalStatus.Survived);
            var stillbirths = babyDetails.Count(b => b.VitalStatus == BabyVitalStatus.FreshStillbirth || b.VitalStatus == BabyVitalStatus.MaceratedStillbirth);
            var neonatalDeaths = babyDetails.Count(b => b.VitalStatus == BabyVitalStatus.EarlyNeonatalDeath);

            // Maternal outcomes
            var maternalDeaths = birthOutcomes.Count(b => b.MaternalStatus == MaternalOutcomeStatus.Died);

            // Complications
            var pphCases = birthOutcomes.Count(b => b.PostpartumHemorrhage);
            var eclampsiaCases = birthOutcomes.Count(b => b.Eclampsia);

            // Referrals - join through Partograph to filter by facility
            var referralsQuery = _context.Referrals
                .Include(r => r.Partograph)
                .Where(r => r.Deleted == 0 && r.CreatedTime >= startTimestamp && r.CreatedTime <= endTimestamp);

            if (facilityId.HasValue)
            {
                referralsQuery = referralsQuery.Where(r => r.Partograph != null && r.Partograph.FacilityID == facilityId);
            }

            var referrals = await referralsQuery.CountAsync();

            var emergencyReferralsQuery = _context.Referrals
                .Include(r => r.Partograph)
                .Where(r => r.Deleted == 0 && r.CreatedTime >= startTimestamp && r.CreatedTime <= endTimestamp
                    && r.Urgency == ReferralUrgency.Emergency);

            if (facilityId.HasValue)
            {
                emergencyReferralsQuery = emergencyReferralsQuery.Where(r => r.Partograph != null && r.Partograph.FacilityID == facilityId);
            }

            var emergencyReferrals = await emergencyReferralsQuery.CountAsync();

            return new
            {
                period = new { startDate, endDate },
                summary = new
                {
                    totalPatients = patientCount,
                    totalPartographs = partographCount,
                    totalStaff = staffCount,
                    totalFacilities = facilityCount,
                    activeLabors
                },
                deliveries = new
                {
                    total = totalDeliveries,
                    spontaneousVaginal = svdCount,
                    caesareanSection = csCount,
                    assisted = assistedCount,
                    caesareanRate = totalDeliveries > 0 ? Math.Round((double)csCount / totalDeliveries * 100, 1) : 0
                },
                outcomes = new
                {
                    liveBirths,
                    stillbirths,
                    neonatalDeaths,
                    stillbirthRate = liveBirths + stillbirths > 0
                        ? Math.Round((double)stillbirths / (liveBirths + stillbirths) * 1000, 1)
                        : 0,
                    maternalDeaths,
                    maternalMortalityRatio = liveBirths > 0
                        ? Math.Round((double)maternalDeaths / liveBirths * 100000, 1)
                        : 0
                },
                complications = new
                {
                    postpartumHemorrhage = pphCases,
                    eclampsia = eclampsiaCases
                },
                referrals = new
                {
                    total = referrals,
                    emergency = emergencyReferrals
                },
                generatedAt = DateTime.UtcNow
            };
        }

        public async Task<object> GetRealtimeStatsAsync(Guid? facilityId = null)
        {
            // Active labors by stage - filter by facility
            var activePartographsQuery = _context.Partographs
                .Where(p => p.Deleted == 0 && p.Status != LaborStatus.Completed);

            if (facilityId.HasValue)
            {
                activePartographsQuery = activePartographsQuery.Where(p => p.FacilityID == facilityId);
            }

            var activePartographs = await activePartographsQuery.ToListAsync();

            var byStage = activePartographs.GroupBy(p => p.Status)
                .Select(g => new { stage = g.Key.ToString(), count = g.Count() });

            // Today's stats
            var today = DateTime.Today;
            var todayStart = new DateTimeOffset(today).ToUnixTimeMilliseconds();
            var todayEnd = new DateTimeOffset(today.AddDays(1)).ToUnixTimeMilliseconds();

            // Today's deliveries - join through Partograph to filter by facility
            var todayDeliveriesQuery = _context.BirthOutcomes
                .Include(b => b.Partograph)
                .Where(b => b.Deleted == 0 && b.CreatedTime >= todayStart && b.CreatedTime < todayEnd);

            if (facilityId.HasValue)
            {
                todayDeliveriesQuery = todayDeliveriesQuery.Where(b => b.Partograph != null && b.Partograph.FacilityID == facilityId);
            }

            var todayDeliveries = await todayDeliveriesQuery.CountAsync();

            // Today's admissions - filter by facility
            var todayAdmissionsQuery = _context.Partographs
                .Where(p => p.Deleted == 0 && p.CreatedTime >= todayStart && p.CreatedTime < todayEnd);

            if (facilityId.HasValue)
            {
                todayAdmissionsQuery = todayAdmissionsQuery.Where(p => p.FacilityID == facilityId);
            }

            var todayAdmissions = await todayAdmissionsQuery.CountAsync();

            // Recent alerts (last 24 hours) - filter by facility
            var last24Hours = DateTime.UtcNow.AddHours(-24);
            var recentAlertsQuery = _context.AlertSummaries
                .Where(a => a.AlertDateTime >= last24Hours && !a.Resolved);

            if (facilityId.HasValue)
            {
                recentAlertsQuery = recentAlertsQuery.Where(a => a.FacilityID == facilityId);
            }

            var recentAlerts = await recentAlertsQuery
                .OrderByDescending(a => a.AlertSeverity == "Critical")
                .ThenByDescending(a => a.AlertDateTime)
                .Take(10)
                .ToListAsync();

            // Pending referrals - join through Partograph to filter by facility
            var pendingReferralsQuery = _context.Referrals
                .Include(r => r.Partograph)
                .Where(r => r.Deleted == 0 && r.Status == ReferralStatus.Pending);

            if (facilityId.HasValue)
            {
                pendingReferralsQuery = pendingReferralsQuery.Where(r => r.Partograph != null && r.Partograph.FacilityID == facilityId);
            }

            var pendingReferrals = await pendingReferralsQuery.CountAsync();

            return new
            {
                timestamp = DateTime.UtcNow,
                activeLabors = new
                {
                    total = activePartographs.Count,
                    byStage
                },
                today = new
                {
                    deliveries = todayDeliveries,
                    admissions = todayAdmissions
                },
                alerts = new
                {
                    unresolvedCount = recentAlerts.Count,
                    critical = recentAlerts.Count(a => a.AlertSeverity == "Critical"),
                    recent = recentAlerts.Select(a => new
                    {
                        a.ID,
                        a.AlertCategory,
                        a.AlertSeverity,
                        a.AlertMessage,
                        a.AlertDateTime
                    })
                },
                pendingReferrals
            };
        }

        public async Task ComputeDailyStatsAsync(DateTime date, Guid? facilityId = null)
        {
            var startTimestamp = new DateTimeOffset(date.Date).ToUnixTimeMilliseconds();
            var endTimestamp = new DateTimeOffset(date.Date.AddDays(1)).ToUnixTimeMilliseconds();

            // Get facilities to compute for
            var facilities = await _context.Facilities
                .Where(f => f.Deleted == 0 && (!facilityId.HasValue || f.ID == facilityId))
                .ToListAsync();

            foreach (var facility in facilities)
            {
                try
                {
                    // Check if stats already exist
                    var existing = await _context.DailyFacilityStats
                        .FirstOrDefaultAsync(s => s.FacilityID == facility.ID && s.Date == date.Date);

                    var stats = existing ?? new DailyFacilityStats
                    {
                        ID = Guid.NewGuid(),
                        FacilityID = facility.ID,
                        FacilityName = facility.Name,
                        FacilityCode = facility.Code,
                        Date = date.Date,
                        Year = date.Year,
                        Month = date.Month,
                        Day = date.Day,
                        WeekOfYear = System.Globalization.ISOWeek.GetWeekOfYear(date),
                        CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };

                    // Get partographs for this facility and date - filter by Partograph.FacilityID
                    var partographs = await _context.Partographs
                        .Where(p => p.Deleted == 0 && p.FacilityID == facility.ID && p.CreatedTime >= startTimestamp && p.CreatedTime < endTimestamp)
                        .ToListAsync();

                    // Get partograph IDs for this facility to filter related data
                    var facilityPartographIds = partographs.Select(p => p.ID).ToList();

                    // BirthOutcomes - join through Partograph to filter by facility
                    var birthOutcomes = await _context.BirthOutcomes
                        .Include(b => b.Partograph)
                        .Where(b => b.Deleted == 0 && b.CreatedTime >= startTimestamp && b.CreatedTime < endTimestamp
                            && b.Partograph != null && b.Partograph.FacilityID == facility.ID)
                        .ToListAsync();

                    // BabyDetails - join through Partograph to filter by facility
                    var babyDetails = await _context.BabyDetails
                        .Include(b => b.Partograph)
                        .Where(b => b.Deleted == 0 && b.CreatedTime >= startTimestamp && b.CreatedTime < endTimestamp
                            && b.Partograph != null && b.Partograph.FacilityID == facility.ID)
                        .ToListAsync();

                    // Referrals - join through Partograph to filter by facility
                    var referrals = await _context.Referrals
                        .Include(r => r.Partograph)
                        .Where(r => r.Deleted == 0 && r.CreatedTime >= startTimestamp && r.CreatedTime < endTimestamp
                            && r.Partograph != null && r.Partograph.FacilityID == facility.ID)
                        .ToListAsync();

                    // Populate stats
                    stats.TotalAdmissions = partographs.Count;
                    stats.TotalDeliveries = birthOutcomes.Count;
                    stats.SpontaneousVaginalDeliveries = birthOutcomes.Count(b => b.DeliveryMode == DeliveryMode.SpontaneousVaginal);
                    stats.AssistedVaginalDeliveries = birthOutcomes.Count(b => b.DeliveryMode == DeliveryMode.AssistedVaginal);
                    stats.CaesareanSections = birthOutcomes.Count(b => b.DeliveryMode == DeliveryMode.CaesareanSection);

                    stats.LiveBirths = babyDetails.Count(b => b.VitalStatus == BabyVitalStatus.LiveBirth || b.VitalStatus == BabyVitalStatus.Survived);
                    stats.Stillbirths = babyDetails.Count(b => b.VitalStatus == BabyVitalStatus.FreshStillbirth || b.VitalStatus == BabyVitalStatus.MaceratedStillbirth);
                    stats.FreshStillbirths = babyDetails.Count(b => b.VitalStatus == BabyVitalStatus.FreshStillbirth);
                    stats.MaceratedStillbirths = babyDetails.Count(b => b.VitalStatus == BabyVitalStatus.MaceratedStillbirth);
                    stats.NeonatalDeaths = babyDetails.Count(b => b.VitalStatus == BabyVitalStatus.EarlyNeonatalDeath);

                    stats.MaternalDeaths = birthOutcomes.Count(b => b.MaternalStatus == MaternalOutcomeStatus.Died);
                    stats.PPHCases = birthOutcomes.Count(b => b.PostpartumHemorrhage);
                    stats.EclampsiaPreeclampsiaCases = birthOutcomes.Count(b => b.Eclampsia);
                    stats.ObstructedLaborCases = birthOutcomes.Count(b => b.ObstructedLabor);

                    stats.TotalReferralsOut = referrals.Count;
                    stats.EmergencyReferrals = referrals.Count(r => r.Urgency == ReferralUrgency.Emergency);

                    stats.PartographsUsed = partographs.Count;
                    stats.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    if (existing == null)
                        _context.DailyFacilityStats.Add(stats);

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error computing daily stats for facility {FacilityId} on {Date}",
                        facility.ID, date);
                }
            }
        }

        public async Task ComputeMonthlyStatsAsync(int year, int month, Guid? facilityId = null)
        {
            var facilities = await _context.Facilities
                .Where(f => f.Deleted == 0 && (!facilityId.HasValue || f.ID == facilityId))
                .ToListAsync();

            foreach (var facility in facilities)
            {
                try
                {
                    var existing = await _context.MonthlyFacilityStats
                        .FirstOrDefaultAsync(s => s.FacilityID == facility.ID && s.Year == year && s.Month == month);

                    var stats = existing ?? new MonthlyFacilityStats
                    {
                        ID = Guid.NewGuid(),
                        FacilityID = facility.ID,
                        FacilityName = facility.Name,
                        FacilityCode = facility.Code,
                        Year = year,
                        Month = month,
                        MonthName = new DateTime(year, month, 1).ToString("MMMM"),
                        CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };

                    // Aggregate daily stats for the month
                    var dailyStats = await _context.DailyFacilityStats
                        .Where(d => d.FacilityID == facility.ID && d.Year == year && d.Month == month)
                        .ToListAsync();

                    if (dailyStats.Any())
                    {
                        stats.TotalDeliveries = dailyStats.Sum(d => d.TotalDeliveries);
                        stats.SVDCount = dailyStats.Sum(d => d.SpontaneousVaginalDeliveries);
                        stats.AssistedDeliveryCount = dailyStats.Sum(d => d.AssistedVaginalDeliveries);
                        stats.CaesareanCount = dailyStats.Sum(d => d.CaesareanSections);
                        stats.CaesareanRate = stats.TotalDeliveries > 0
                            ? Math.Round((double)stats.CaesareanCount / stats.TotalDeliveries * 100, 1)
                            : 0;

                        stats.LiveBirthCount = dailyStats.Sum(d => d.LiveBirths);
                        stats.StillbirthCount = dailyStats.Sum(d => d.Stillbirths);
                        stats.TotalBirths = stats.LiveBirthCount + stats.StillbirthCount;
                        stats.StillbirthRate = stats.TotalBirths > 0
                            ? Math.Round((double)stats.StillbirthCount / stats.TotalBirths * 1000, 1)
                            : 0;

                        stats.NeonatalDeathCount = dailyStats.Sum(d => d.NeonatalDeaths);
                        stats.NeonatalMortalityRate = stats.LiveBirthCount > 0
                            ? Math.Round((double)stats.NeonatalDeathCount / stats.LiveBirthCount * 1000, 1)
                            : 0;

                        stats.PerinatalMortalityRate = stats.TotalBirths > 0
                            ? Math.Round((double)(stats.StillbirthCount + stats.NeonatalDeathCount) / stats.TotalBirths * 1000, 1)
                            : 0;

                        stats.MaternalDeathCount = dailyStats.Sum(d => d.MaternalDeaths);
                        stats.MaternalMortalityRatio = stats.LiveBirthCount > 0
                            ? Math.Round((double)stats.MaternalDeathCount / stats.LiveBirthCount * 100000, 1)
                            : 0;

                        stats.PPHCount = dailyStats.Sum(d => d.PPHCases);
                        stats.EclampsiaCount = dailyStats.Sum(d => d.EclampsiaPreeclampsiaCases);
                        stats.ObstructedLaborCount = dailyStats.Sum(d => d.ObstructedLaborCases);

                        stats.ReferralsOutCount = dailyStats.Sum(d => d.TotalReferralsOut);
                        stats.ReferralRate = stats.TotalDeliveries > 0
                            ? Math.Round((double)stats.ReferralsOutCount / stats.TotalDeliveries * 100, 1)
                            : 0;

                        var partographsUsed = dailyStats.Sum(d => d.PartographsUsed);
                        stats.PartographUtilizationRate = stats.TotalDeliveries > 0
                            ? Math.Round((double)partographsUsed / stats.TotalDeliveries * 100, 1)
                            : 0;
                    }

                    stats.CaesareanRateWithinTarget = stats.CaesareanRate >= 10 && stats.CaesareanRate <= 15;
                    stats.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    if (existing == null)
                        _context.MonthlyFacilityStats.Add(stats);

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error computing monthly stats for facility {FacilityId} for {Year}-{Month}",
                        facility.ID, year, month);
                }
            }
        }

        public async Task ComputeFacilityPerformanceAsync(Guid? facilityId = null)
        {
            var facilities = await _context.Facilities
                .Include(f => f.District)
                    .ThenInclude(d => d!.Region)
                .Where(f => f.Deleted == 0 && (!facilityId.HasValue || f.ID == facilityId))
                .ToListAsync();

            foreach (var facility in facilities)
            {
                try
                {
                    // Get last 3 months of monthly stats for trend analysis
                    var threeMonthsAgo = DateTime.Today.AddMonths(-3);
                    var monthlyStats = await _context.MonthlyFacilityStats
                        .Where(m => m.FacilityID == facility.ID)
                        .OrderByDescending(m => m.Year).ThenByDescending(m => m.Month)
                        .Take(3)
                        .ToListAsync();

                    var latestStats = monthlyStats.FirstOrDefault();

                    var snapshot = new FacilityPerformanceSnapshot
                    {
                        ID = Guid.NewGuid(),
                        FacilityID = facility.ID,
                        FacilityName = facility.Name,
                        FacilityCode = facility.Code,
                        FacilityType = facility.Type,
                        Region = facility.District?.Region?.Name ?? string.Empty,
                        SnapshotDate = DateTime.UtcNow,
                        PeriodType = "Monthly",
                        CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };

                    if (latestStats != null)
                    {
                        snapshot.CaesareanRate = latestStats.CaesareanRate;
                        snapshot.StillbirthRate = latestStats.StillbirthRate;
                        snapshot.NeonatalMortalityRate = latestStats.NeonatalMortalityRate;
                        snapshot.MaternalMortalityRatio = latestStats.MaternalMortalityRatio;
                        snapshot.PartographUtilizationRate = latestStats.PartographUtilizationRate;
                        snapshot.ReferralRate = latestStats.ReferralRate;

                        // Calculate performance scores (simplified scoring)
                        var qualityScore = 100.0;
                        qualityScore -= Math.Abs(latestStats.CaesareanRate - 12.5) * 2; // Penalize deviation from optimal CS rate
                        qualityScore -= latestStats.StillbirthRate / 10;
                        qualityScore = Math.Max(0, Math.Min(100, qualityScore));

                        var safetyScore = 100.0;
                        safetyScore -= latestStats.MaternalMortalityRatio / 100;
                        safetyScore -= latestStats.NeonatalMortalityRate / 5;
                        safetyScore = Math.Max(0, Math.Min(100, safetyScore));

                        snapshot.QualityOfCareScore = Math.Round(qualityScore, 1);
                        snapshot.SafetyScore = Math.Round(safetyScore, 1);
                        snapshot.OverallPerformanceScore = Math.Round((qualityScore + safetyScore) / 2, 1);

                        // WHO Compliance
                        var targetsCount = 5;
                        var targetsMet = 0;
                        if (latestStats.CaesareanRateWithinTarget) targetsMet++;
                        if (latestStats.PartographUtilizationRate >= 90) targetsMet++;
                        if (latestStats.StillbirthRate < 18) targetsMet++; // WHO target < 18/1000
                        if (latestStats.NeonatalMortalityRate < 12) targetsMet++;
                        if (latestStats.MaternalMortalityRatio < 140) targetsMet++;

                        snapshot.TargetsMet = targetsMet;
                        snapshot.TotalTargets = targetsCount;
                        snapshot.WHOComplianceScore = Math.Round((double)targetsMet / targetsCount * 100, 1);

                        // Calculate trends
                        if (monthlyStats.Count >= 2)
                        {
                            var previous = monthlyStats[1];
                            snapshot.OverallTrend = latestStats.CaesareanRate < previous.CaesareanRate
                                && latestStats.StillbirthRate < previous.StillbirthRate ? 1 : -1;
                        }
                    }

                    snapshot.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    _context.FacilityPerformanceSnapshots.Add(snapshot);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error computing performance for facility {FacilityId}", facility.ID);
                }
            }
        }

        public async Task ProcessDeliveryOutcomeAsync(Guid partographId)
        {
            try
            {
                var partograph = await _context.Partographs
                    .FirstOrDefaultAsync(p => p.ID == partographId);

                var birthOutcome = await _context.BirthOutcomes
                    .FirstOrDefaultAsync(b => b.PartographID == partographId);

                var babyDetails = await _context.BabyDetails
                    .Where(b => b.PartographID == partographId)
                    .ToListAsync();

                if (partograph == null || birthOutcome == null)
                    return;

                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.ID == partograph.PatientID);

                var summary = new DeliveryOutcomeSummary
                {
                    ID = Guid.NewGuid(),
                    PartographID = partographId,
                    PatientID = partograph.PatientID,
                    PatientAge = patient?.Age,
                    DeliveryMode = birthOutcome.DeliveryMode.ToString(),
                    NumberOfBabies = birthOutcome.NumberOfBabies,
                    LiveBirth = babyDetails.Any(b => b.VitalStatus == BabyVitalStatus.LiveBirth),
                    Stillbirth = babyDetails.Any(b => b.VitalStatus == BabyVitalStatus.FreshStillbirth || b.VitalStatus == BabyVitalStatus.MaceratedStillbirth),
                    PPHOccurred = birthOutcome.PostpartumHemorrhage,
                    EstimatedBloodLossMl = birthOutcome.EstimatedBloodLoss,
                    PartographUsed = true,
                    CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                if (babyDetails.Any())
                {
                    var firstBaby = babyDetails.First();
                    summary.BirthWeight = firstBaby.BirthWeight;
                    summary.APGAR1Min = firstBaby.Apgar1Min;
                    summary.APGAR5Min = firstBaby.Apgar5Min;
                    summary.ResuscitationRequired = firstBaby.ResuscitationRequired;
                    summary.AdmittedToNICU = firstBaby.AdmittedToNICU;
                }

                _context.DeliveryOutcomeSummaries.Add(summary);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing delivery outcome for partograph {PartographId}", partographId);
            }
        }

        public async Task ProcessComplicationAsync(Guid partographId, string complicationType)
        {
            try
            {
                var analytics = new ComplicationAnalytics
                {
                    ID = Guid.NewGuid(),
                    PartographID = partographId,
                    OccurrenceDateTime = DateTime.UtcNow,
                    Year = DateTime.UtcNow.Year,
                    Month = DateTime.UtcNow.Month,
                    Day = DateTime.UtcNow.Day,
                    ComplicationType = complicationType,
                    CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                switch (complicationType.ToLower())
                {
                    case "pph":
                        analytics.PPH = true;
                        analytics.ComplicationCategory = "Maternal";
                        analytics.Severity = "Severe";
                        break;
                    case "eclampsia":
                        analytics.Eclampsia = true;
                        analytics.ComplicationCategory = "Maternal";
                        analytics.Severity = "Life-threatening";
                        break;
                    case "fetaldistress":
                        analytics.FetalDistress = true;
                        analytics.ComplicationCategory = "Fetal";
                        analytics.Severity = "Severe";
                        break;
                    default:
                        analytics.ComplicationCategory = "Other";
                        analytics.Severity = "Moderate";
                        break;
                }

                _context.ComplicationAnalytics.Add(analytics);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing complication for partograph {PartographId}", partographId);
            }
        }

        public async Task ProcessReferralAsync(Guid referralId)
        {
            try
            {
                var referral = await _context.Referrals
                    .FirstOrDefaultAsync(r => r.ID == referralId);

                if (referral == null) return;

                var analytics = new ReferralAnalytics
                {
                    ID = Guid.NewGuid(),
                    ReferralID = referralId,
                    PartographID = referral.PartographID,
                    ReferralDateTime = referral.ReferralTime,
                    Year = referral.ReferralTime.Year,
                    Month = referral.ReferralTime.Month,
                    SourceFacilityName = referral.ReferringFacilityName,
                    SourceFacilityLevel = referral.ReferringFacilityLevel,
                    DestinationFacilityName = referral.DestinationFacilityName,
                    DestinationFacilityLevel = referral.DestinationFacilityLevel,
                    ReferralType = referral.ReferralType.ToString(),
                    Urgency = referral.Urgency.ToString(),
                    PrimaryReason = referral.PrimaryDiagnosis,
                    TransportMode = referral.TransportMode.ToString(),
                    DepartureTime = referral.DepartureTime,
                    ArrivalTime = referral.ArrivalTime,
                    SkilledAttendantAccompanied = referral.SkillfulAttendantAccompanying,
                    PartographSent = referral.PartographSent,
                    IVLineEstablished = referral.IVLineInsitu,
                    DestinationNotifiedBeforeArrival = referral.DestinationNotified,
                    ReferralStatus = referral.Status.ToString(),
                    CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                if (referral.DepartureTime.HasValue && referral.ArrivalTime.HasValue)
                {
                    analytics.TransportDurationMinutes = (int)(referral.ArrivalTime.Value - referral.DepartureTime.Value).TotalMinutes;
                }

                _context.ReferralAnalytics.Add(analytics);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing referral {ReferralId}", referralId);
            }
        }
    }
}
