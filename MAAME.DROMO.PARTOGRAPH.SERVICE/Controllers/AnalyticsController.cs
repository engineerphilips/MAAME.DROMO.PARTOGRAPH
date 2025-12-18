using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Models;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Controllers
{
    /// <summary>
    /// Analytics API for external web applications to interface with partograph data
    /// Provides high-level aggregated statistics and insights
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly PartographDbContext _context;
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(
            PartographDbContext context,
            IAnalyticsService analyticsService,
            ILogger<AnalyticsController> logger)
        {
            _context = context;
            _analyticsService = analyticsService;
            _logger = logger;
        }

        #region Dashboard Endpoints

        /// <summary>
        /// Get overall system dashboard statistics
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddMonths(-1);
                var end = endDate ?? DateTime.Today.AddDays(1);

                var stats = await _analyticsService.GetDashboardStatsAsync(start, end);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return StatusCode(500, new { error = "Failed to get dashboard stats", message = ex.Message });
            }
        }

        /// <summary>
        /// Get real-time summary of active labor cases
        /// </summary>
        [HttpGet("dashboard/realtime")]
        public async Task<IActionResult> GetRealtimeStats()
        {
            try
            {
                var stats = await _analyticsService.GetRealtimeStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting realtime stats");
                return StatusCode(500, new { error = "Failed to get realtime stats", message = ex.Message });
            }
        }

        #endregion

        #region Facility Statistics

        /// <summary>
        /// Get daily statistics for a facility
        /// </summary>
        [HttpGet("facilities/{facilityId}/daily")]
        public async Task<ActionResult<IEnumerable<DailyFacilityStats>>> GetFacilityDailyStats(
            Guid facilityId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var stats = await _context.DailyFacilityStats
                    .Where(s => s.FacilityID == facilityId && s.Date >= startDate && s.Date <= endDate)
                    .OrderBy(s => s.Date)
                    .ToListAsync();

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily facility stats for {FacilityId}", facilityId);
                return StatusCode(500, new { error = "Failed to get facility daily stats", message = ex.Message });
            }
        }

        /// <summary>
        /// Get monthly statistics for a facility
        /// </summary>
        [HttpGet("facilities/{facilityId}/monthly")]
        public async Task<ActionResult<IEnumerable<MonthlyFacilityStats>>> GetFacilityMonthlyStats(
            Guid facilityId,
            [FromQuery] int year,
            [FromQuery] int? month = null)
        {
            try
            {
                var query = _context.MonthlyFacilityStats
                    .Where(s => s.FacilityID == facilityId && s.Year == year);

                if (month.HasValue)
                    query = query.Where(s => s.Month == month.Value);

                var stats = await query.OrderBy(s => s.Month).ToListAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly facility stats for {FacilityId}", facilityId);
                return StatusCode(500, new { error = "Failed to get facility monthly stats", message = ex.Message });
            }
        }

        /// <summary>
        /// Get performance snapshot for a facility
        /// </summary>
        [HttpGet("facilities/{facilityId}/performance")]
        public async Task<ActionResult<FacilityPerformanceSnapshot>> GetFacilityPerformance(Guid facilityId)
        {
            try
            {
                var snapshot = await _context.FacilityPerformanceSnapshots
                    .Where(s => s.FacilityID == facilityId)
                    .OrderByDescending(s => s.SnapshotDate)
                    .FirstOrDefaultAsync();

                if (snapshot == null)
                    return NotFound(new { error = "No performance data found for this facility" });

                return Ok(snapshot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facility performance for {FacilityId}", facilityId);
                return StatusCode(500, new { error = "Failed to get facility performance", message = ex.Message });
            }
        }

        /// <summary>
        /// Get all facilities performance rankings
        /// </summary>
        [HttpGet("facilities/rankings")]
        public async Task<IActionResult> GetFacilityRankings([FromQuery] string? region = null)
        {
            try
            {
                var query = _context.FacilityPerformanceSnapshots.AsQueryable();

                if (!string.IsNullOrEmpty(region))
                    query = query.Where(s => s.Region == region);

                var rankings = await query
                    .GroupBy(s => s.FacilityID)
                    .Select(g => g.OrderByDescending(s => s.SnapshotDate).First())
                    .OrderByDescending(s => s.OverallPerformanceScore)
                    .ToListAsync();

                return Ok(rankings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facility rankings");
                return StatusCode(500, new { error = "Failed to get facility rankings", message = ex.Message });
            }
        }

        #endregion

        #region Delivery Outcome Statistics

        /// <summary>
        /// Get delivery outcomes summary
        /// </summary>
        [HttpGet("deliveries")]
        public async Task<IActionResult> GetDeliveryOutcomes(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                var query = _context.DeliveryOutcomeSummaries
                    .Where(d => d.DeliveryTime >= startDate && d.DeliveryTime <= endDate);

                if (facilityId.HasValue)
                    query = query.Where(d => d.FacilityID == facilityId);

                var outcomes = await query.ToListAsync();

                var summary = new
                {
                    totalDeliveries = outcomes.Count,
                    deliveryModes = outcomes.GroupBy(o => o.DeliveryMode)
                        .Select(g => new { mode = g.Key, count = g.Count() }),
                    caesareanRate = outcomes.Count > 0
                        ? (double)outcomes.Count(o => o.DeliveryMode == "CaesareanSection") / outcomes.Count * 100
                        : 0,
                    liveBirths = outcomes.Count(o => o.LiveBirth),
                    stillbirths = outcomes.Count(o => o.Stillbirth),
                    averageLaborDuration = outcomes.Where(o => o.LaborDurationHours.HasValue)
                        .Average(o => o.LaborDurationHours),
                    pphCases = outcomes.Count(o => o.PPHOccurred),
                    referralRate = outcomes.Count > 0
                        ? (double)outcomes.Count(o => o.WasReferred) / outcomes.Count * 100
                        : 0
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery outcomes");
                return StatusCode(500, new { error = "Failed to get delivery outcomes", message = ex.Message });
            }
        }

        /// <summary>
        /// Get detailed delivery outcome records
        /// </summary>
        [HttpGet("deliveries/details")]
        public async Task<ActionResult<IEnumerable<DeliveryOutcomeSummary>>> GetDeliveryDetails(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? facilityId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var query = _context.DeliveryOutcomeSummaries
                    .Where(d => d.DeliveryTime >= startDate && d.DeliveryTime <= endDate);

                if (facilityId.HasValue)
                    query = query.Where(d => d.FacilityID == facilityId);

                var total = await query.CountAsync();
                var records = await query
                    .OrderByDescending(d => d.DeliveryTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    total,
                    page,
                    pageSize,
                    records
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery details");
                return StatusCode(500, new { error = "Failed to get delivery details", message = ex.Message });
            }
        }

        #endregion

        #region Mortality Statistics

        /// <summary>
        /// Get maternal mortality statistics
        /// </summary>
        [HttpGet("mortality/maternal")]
        public async Task<IActionResult> GetMaternalMortalityStats(
            [FromQuery] int year,
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                var query = _context.MaternalMortalityRecords.Where(m => m.Year == year);

                if (facilityId.HasValue)
                    query = query.Where(m => m.FacilityID == facilityId);

                var records = await query.ToListAsync();

                var summary = new
                {
                    year,
                    totalDeaths = records.Count,
                    byMonth = records.GroupBy(r => r.Month)
                        .Select(g => new { month = g.Key, count = g.Count() }),
                    byCause = records.GroupBy(r => r.DirectCauseCategory)
                        .Select(g => new { cause = g.Key, count = g.Count() }),
                    directObstetricDeaths = records.Count(r => r.DirectObstetricCause),
                    indirectDeaths = records.Count(r => !r.DirectObstetricCause),
                    delays = new
                    {
                        delay1 = records.Count(r => r.Delay1SeekingCare),
                        delay2 = records.Count(r => r.Delay2ReachingCare),
                        delay3 = records.Count(r => r.Delay3ReceivingCare)
                    },
                    mdrCompleted = records.Count(r => r.MDRCompleted)
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maternal mortality stats");
                return StatusCode(500, new { error = "Failed to get maternal mortality stats", message = ex.Message });
            }
        }

        /// <summary>
        /// Get neonatal outcome statistics
        /// </summary>
        [HttpGet("mortality/neonatal")]
        public async Task<IActionResult> GetNeonatalOutcomeStats(
            [FromQuery] int year,
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                var query = _context.NeonatalOutcomeRecords.Where(n => n.Year == year);

                if (facilityId.HasValue)
                    query = query.Where(n => n.FacilityID == facilityId);

                var records = await query.ToListAsync();

                var summary = new
                {
                    year,
                    totalOutcomes = records.Count,
                    byOutcomeType = records.GroupBy(r => r.OutcomeType)
                        .Select(g => new { outcomeType = g.Key, count = g.Count() }),
                    stillbirths = records.Count(r => r.OutcomeType.Contains("Stillbirth")),
                    freshStillbirths = records.Count(r => r.OutcomeType == "FreshStillbirth"),
                    maceratedStillbirths = records.Count(r => r.OutcomeType == "MaceratedStillbirth"),
                    neonatalDeaths = records.Count(r => r.OutcomeType == "NeonatalDeath"),
                    byCauseCategory = records.GroupBy(r => r.CauseCategory)
                        .Select(g => new { category = g.Key, count = g.Count() }),
                    lowBirthWeight = records.Count(r => r.BirthWeightCategory == "LowBirthWeight"),
                    preterm = records.Count(r => r.GestationalCategory != "Term"),
                    resuscitationRequired = records.Count(r => r.ResuscitationAttempted),
                    lowAPGAR = records.Count(r => r.LowAPGAR)
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting neonatal outcome stats");
                return StatusCode(500, new { error = "Failed to get neonatal outcome stats", message = ex.Message });
            }
        }

        #endregion

        #region Complication Statistics

        /// <summary>
        /// Get complication analytics
        /// </summary>
        [HttpGet("complications")]
        public async Task<IActionResult> GetComplicationStats(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                var query = _context.ComplicationAnalytics
                    .Where(c => c.OccurrenceDateTime >= startDate && c.OccurrenceDateTime <= endDate);

                if (facilityId.HasValue)
                    query = query.Where(c => c.FacilityID == facilityId);

                var records = await query.ToListAsync();

                var summary = new
                {
                    totalComplications = records.Count,
                    byCategory = records.GroupBy(r => r.ComplicationCategory)
                        .Select(g => new { category = g.Key, count = g.Count() }),
                    bySeverity = records.GroupBy(r => r.Severity)
                        .Select(g => new { severity = g.Key, count = g.Count() }),
                    specificComplications = new
                    {
                        pph = records.Count(r => r.PPH),
                        eclampsia = records.Count(r => r.Eclampsia),
                        preeclampsia = records.Count(r => r.PreeclampsiaSevere),
                        sepsis = records.Count(r => r.Sepsis),
                        obstructedLabor = records.Count(r => r.ObstructedLabor),
                        fetalDistress = records.Count(r => r.FetalDistress),
                        uterineRupture = records.Count(r => r.UterineRupture),
                        shoulderDystocia = records.Count(r => r.ShoulderDystocia)
                    },
                    averageResponseTimeMinutes = records.Where(r => r.TimeToResponseMinutes.HasValue)
                        .Average(r => r.TimeToResponseMinutes),
                    surgicalInterventionRequired = records.Count(r => r.SurgicalInterventionRequired),
                    preventableComplications = records.Count(r => r.Preventable)
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting complication stats");
                return StatusCode(500, new { error = "Failed to get complication stats", message = ex.Message });
            }
        }

        #endregion

        #region Referral Statistics

        /// <summary>
        /// Get referral analytics
        /// </summary>
        [HttpGet("referrals")]
        public async Task<IActionResult> GetReferralStats(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? sourceFacilityId = null,
            [FromQuery] Guid? destinationFacilityId = null)
        {
            try
            {
                var query = _context.ReferralAnalytics
                    .Where(r => r.ReferralDateTime >= startDate && r.ReferralDateTime <= endDate);

                if (sourceFacilityId.HasValue)
                    query = query.Where(r => r.SourceFacilityID == sourceFacilityId);

                if (destinationFacilityId.HasValue)
                    query = query.Where(r => r.DestinationFacilityID == destinationFacilityId);

                var records = await query.ToListAsync();

                var summary = new
                {
                    totalReferrals = records.Count,
                    byUrgency = records.GroupBy(r => r.Urgency)
                        .Select(g => new { urgency = g.Key, count = g.Count() }),
                    byType = records.GroupBy(r => r.ReferralType)
                        .Select(g => new { type = g.Key, count = g.Count() }),
                    byPrimaryReason = records.GroupBy(r => r.PrimaryReason)
                        .Select(g => new { reason = g.Key, count = g.Count() })
                        .OrderByDescending(r => r.count)
                        .Take(10),
                    byTransportMode = records.GroupBy(r => r.TransportMode)
                        .Select(g => new { mode = g.Key, count = g.Count() }),
                    averageTransportDuration = records.Where(r => r.TransportDurationMinutes.HasValue)
                        .Average(r => r.TransportDurationMinutes),
                    skilledAttendantAccompanied = records.Count(r => r.SkilledAttendantAccompanied),
                    destinationNotified = records.Count(r => r.DestinationNotifiedBeforeArrival),
                    byStatus = records.GroupBy(r => r.ReferralStatus)
                        .Select(g => new { status = g.Key, count = g.Count() }),
                    topSourceFacilities = records.GroupBy(r => r.SourceFacilityName)
                        .Select(g => new { facility = g.Key, count = g.Count() })
                        .OrderByDescending(f => f.count)
                        .Take(10),
                    topDestinationFacilities = records.GroupBy(r => r.DestinationFacilityName)
                        .Select(g => new { facility = g.Key, count = g.Count() })
                        .OrderByDescending(f => f.count)
                        .Take(10)
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting referral stats");
                return StatusCode(500, new { error = "Failed to get referral stats", message = ex.Message });
            }
        }

        #endregion

        #region Labor Progress Statistics

        /// <summary>
        /// Get labor progress analytics
        /// </summary>
        [HttpGet("labor-progress")]
        public async Task<IActionResult> GetLaborProgressStats(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                var query = _context.LaborProgressAnalytics
                    .Where(l => l.LaborStartTime >= startDate && l.LaborStartTime <= endDate);

                if (facilityId.HasValue)
                    query = query.Where(l => l.FacilityID == facilityId);

                var records = await query.ToListAsync();

                var summary = new
                {
                    totalLabors = records.Count,
                    byProgressPattern = records.GroupBy(r => r.LaborProgressPattern)
                        .Select(g => new { pattern = g.Key, count = g.Count() }),
                    averageDurations = new
                    {
                        totalLaborHours = records.Where(r => r.TotalLaborDurationHours.HasValue)
                            .Average(r => r.TotalLaborDurationHours),
                        firstStageHours = records.Where(r => r.FirstStageDurationHours.HasValue)
                            .Average(r => r.FirstStageDurationHours),
                        activePhasehours = records.Where(r => r.ActivePhaseDurationHours.HasValue)
                            .Average(r => r.ActivePhaseDurationHours),
                        secondStageMinutes = records.Where(r => r.SecondStageDurationMinutes.HasValue)
                            .Average(r => r.SecondStageDurationMinutes)
                    },
                    alertLineCrossings = records.Count(r => r.CrossedAlertLine),
                    actionLineCrossings = records.Count(r => r.CrossedActionLine),
                    augmentationUsed = records.Count(r => r.AugmentationUsed),
                    fhrAbnormalities = new
                    {
                        decelerations = records.Count(r => r.FHRDecelerations),
                        tachycardia = records.Count(r => r.FHRTachycardia),
                        bradycardia = records.Count(r => r.FHRBradycardia)
                    },
                    tachysystole = records.Count(r => r.Tachysystole),
                    whoCompliantMonitoring = records.Count(r => r.WHOCompliantMonitoring),
                    averageCompliancePercent = records.Where(r => r.MonitoringCompliancePercent.HasValue)
                        .Average(r => r.MonitoringCompliancePercent),
                    byDeliveryMode = records.GroupBy(r => r.DeliveryMode)
                        .Select(g => new { mode = g.Key, count = g.Count() })
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting labor progress stats");
                return StatusCode(500, new { error = "Failed to get labor progress stats", message = ex.Message });
            }
        }

        #endregion

        #region Alert Statistics

        /// <summary>
        /// Get alert summary statistics
        /// </summary>
        [HttpGet("alerts")]
        public async Task<IActionResult> GetAlertStats(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? facilityId = null,
            [FromQuery] bool? unresolvedOnly = false)
        {
            try
            {
                var query = _context.AlertSummaries
                    .Where(a => a.AlertDateTime >= startDate && a.AlertDateTime <= endDate);

                if (facilityId.HasValue)
                    query = query.Where(a => a.FacilityID == facilityId);

                if (unresolvedOnly == true)
                    query = query.Where(a => !a.Resolved);

                var records = await query.ToListAsync();

                var summary = new
                {
                    totalAlerts = records.Count,
                    byType = records.GroupBy(r => r.AlertType)
                        .Select(g => new { type = g.Key, count = g.Count() }),
                    byCategory = records.GroupBy(r => r.AlertCategory)
                        .Select(g => new { category = g.Key, count = g.Count() }),
                    bySeverity = records.GroupBy(r => r.AlertSeverity)
                        .Select(g => new { severity = g.Key, count = g.Count() }),
                    acknowledged = records.Count(r => r.Acknowledged),
                    actionTaken = records.Count(r => r.ActionTaken),
                    resolved = records.Count(r => r.Resolved),
                    averageResponseTimeMinutes = records.Where(r => r.ResponseTimeMinutes.HasValue)
                        .Average(r => r.ResponseTimeMinutes),
                    byOutcome = records.GroupBy(r => r.OutcomeAssessment)
                        .Select(g => new { outcome = g.Key, count = g.Count() }),
                    criticalUnresolved = records.Count(r => r.AlertSeverity == "Critical" && !r.Resolved)
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert stats");
                return StatusCode(500, new { error = "Failed to get alert stats", message = ex.Message });
            }
        }

        /// <summary>
        /// Get active/unresolved alerts
        /// </summary>
        [HttpGet("alerts/active")]
        public async Task<ActionResult<IEnumerable<AlertSummary>>> GetActiveAlerts(
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                var query = _context.AlertSummaries.Where(a => !a.Resolved);

                if (facilityId.HasValue)
                    query = query.Where(a => a.FacilityID == facilityId);

                var alerts = await query
                    .OrderByDescending(a => a.AlertSeverity == "Critical")
                    .ThenByDescending(a => a.AlertDateTime)
                    .ToListAsync();

                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active alerts");
                return StatusCode(500, new { error = "Failed to get active alerts", message = ex.Message });
            }
        }

        #endregion

        #region Data Aggregation Triggers

        /// <summary>
        /// Trigger daily statistics computation for a specific date
        /// </summary>
        [HttpPost("compute/daily")]
        public async Task<IActionResult> ComputeDailyStats([FromQuery] DateTime date, [FromQuery] Guid? facilityId = null)
        {
            try
            {
                await _analyticsService.ComputeDailyStatsAsync(date, facilityId);
                return Ok(new { message = $"Daily stats computed for {date:yyyy-MM-dd}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing daily stats for {Date}", date);
                return StatusCode(500, new { error = "Failed to compute daily stats", message = ex.Message });
            }
        }

        /// <summary>
        /// Trigger monthly statistics computation
        /// </summary>
        [HttpPost("compute/monthly")]
        public async Task<IActionResult> ComputeMonthlyStats([FromQuery] int year, [FromQuery] int month, [FromQuery] Guid? facilityId = null)
        {
            try
            {
                await _analyticsService.ComputeMonthlyStatsAsync(year, month, facilityId);
                return Ok(new { message = $"Monthly stats computed for {year}-{month:D2}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing monthly stats for {Year}-{Month}", year, month);
                return StatusCode(500, new { error = "Failed to compute monthly stats", message = ex.Message });
            }
        }

        /// <summary>
        /// Trigger facility performance snapshot computation
        /// </summary>
        [HttpPost("compute/performance")]
        public async Task<IActionResult> ComputePerformanceSnapshot([FromQuery] Guid? facilityId = null)
        {
            try
            {
                await _analyticsService.ComputeFacilityPerformanceAsync(facilityId);
                return Ok(new { message = "Performance snapshot computed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing performance snapshot");
                return StatusCode(500, new { error = "Failed to compute performance snapshot", message = ex.Message });
            }
        }

        #endregion

        #region Export Endpoints

        /// <summary>
        /// Export analytics data for external systems
        /// </summary>
        [HttpGet("export")]
        public async Task<IActionResult> ExportAnalytics(
            [FromQuery] string dataType,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? facilityId = null,
            [FromQuery] string format = "json")
        {
            try
            {
                object? data = dataType.ToLower() switch
                {
                    "deliveries" => await _context.DeliveryOutcomeSummaries
                        .Where(d => d.DeliveryTime >= startDate && d.DeliveryTime <= endDate)
                        .Where(d => !facilityId.HasValue || d.FacilityID == facilityId)
                        .ToListAsync(),

                    "maternal-mortality" => await _context.MaternalMortalityRecords
                        .Where(m => m.DeathDateTime >= startDate && m.DeathDateTime <= endDate)
                        .Where(m => !facilityId.HasValue || m.FacilityID == facilityId)
                        .ToListAsync(),

                    "neonatal-outcomes" => await _context.NeonatalOutcomeRecords
                        .Where(n => n.BirthDateTime >= startDate && n.BirthDateTime <= endDate)
                        .Where(n => !facilityId.HasValue || n.FacilityID == facilityId)
                        .ToListAsync(),

                    "complications" => await _context.ComplicationAnalytics
                        .Where(c => c.OccurrenceDateTime >= startDate && c.OccurrenceDateTime <= endDate)
                        .Where(c => !facilityId.HasValue || c.FacilityID == facilityId)
                        .ToListAsync(),

                    "referrals" => await _context.ReferralAnalytics
                        .Where(r => r.ReferralDateTime >= startDate && r.ReferralDateTime <= endDate)
                        .Where(r => !facilityId.HasValue || r.SourceFacilityID == facilityId)
                        .ToListAsync(),

                    _ => null
                };

                if (data == null)
                    return BadRequest(new { error = "Invalid data type", validTypes = new[] { "deliveries", "maternal-mortality", "neonatal-outcomes", "complications", "referrals" } });

                return Ok(new
                {
                    dataType,
                    startDate,
                    endDate,
                    facilityId,
                    exportedAt = DateTime.UtcNow,
                    data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting analytics data");
                return StatusCode(500, new { error = "Failed to export analytics data", message = ex.Message });
            }
        }

        #endregion
    }
}
