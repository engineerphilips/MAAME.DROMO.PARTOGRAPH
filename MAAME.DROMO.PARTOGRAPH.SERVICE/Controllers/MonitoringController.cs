using MAAME.DROMO.PARTOGRAPH.MODEL;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Controllers
{
    /// <summary>
    /// API Controller for the Monitoring Dashboard
    /// Provides endpoints for hierarchical access (National, Regional, District)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MonitoringController : ControllerBase
    {
        private readonly PartographDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MonitoringController> _logger;

        public MonitoringController(
            PartographDbContext context,
            IConfiguration configuration,
            ILogger<MonitoringController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        #region Authentication

        /// <summary>
        /// Authenticate a monitoring user
        /// </summary>
        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody] MonitoringLoginRequest request)
        {
            try
            {
                var user = await _context.MonitoringUsers
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.Deleted == 0);

                if (user == null)
                    return Unauthorized(new { error = "Invalid email or password" });

                if (!user.IsActive)
                    return Unauthorized(new { error = "Your account is inactive. Please contact administrator." });

                if (user.IsLocked && user.LockedUntil.HasValue && user.LockedUntil > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    return Unauthorized(new { error = "Your account is locked. Please try again later." });

                if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    user.FailedLoginAttempts++;
                    if (user.FailedLoginAttempts >= 5)
                    {
                        user.IsLocked = true;
                        user.LockedUntil = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();
                    }
                    await _context.SaveChangesAsync();
                    return Unauthorized(new { error = "Invalid email or password" });
                }

                // Reset failed attempts on successful login
                user.FailedLoginAttempts = 0;
                user.IsLocked = false;
                user.LockedUntil = null;
                user.LastLogin = DateTime.UtcNow;
                user.LastLoginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                user.LoginCount++;

                // Generate tokens
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds();
                await _context.SaveChangesAsync();

                // Get region, district, and facility names
                string? regionName = null;
                string? districtName = null;
                string? facilityName = null;

                if (user.RegionID.HasValue)
                {
                    var region = await _context.Regions.FindAsync(user.RegionID.Value);
                    regionName = region?.Name;
                }

                if (user.DistrictID.HasValue)
                {
                    var district = await _context.Districts.FindAsync(user.DistrictID.Value);
                    districtName = district?.Name;
                }

                if (user.FacilityID.HasValue)
                {
                    var facility = await _context.Facilities.FindAsync(user.FacilityID.Value);
                    facilityName = facility?.Name;
                }

                return Ok(new
                {
                    success = true,
                    token,
                    refreshToken,
                    user = new
                    {
                        id = user.ID,
                        fullName = $"{user.FirstName} {user.LastName}".Trim(),
                        email = user.Email,
                        accessLevel = user.AccessLevel,
                        role = user.Role,
                        regionId = user.RegionID,
                        regionName,
                        districtId = user.DistrictID,
                        districtName,
                        facilityId = user.FacilityID,
                        facilityName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during monitoring login");
                return StatusCode(500, new { error = "An error occurred during login" });
            }
        }

        /// <summary>
        /// Refresh authentication token
        /// </summary>
        [HttpPost("auth/refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var user = await _context.MonitoringUsers
                    .FirstOrDefaultAsync(u =>
                        u.RefreshToken == request.RefreshToken &&
                        u.RefreshTokenExpiryTime > DateTimeOffset.UtcNow.ToUnixTimeSeconds() &&
                        u.Deleted == 0);

                if (user == null)
                    return Unauthorized(new { error = "Invalid or expired refresh token" });

                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds();
                await _context.SaveChangesAsync();

                return Ok(new { token, refreshToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new { error = "An error occurred refreshing token" });
            }
        }

        #endregion

        #region Dashboard Summary

        /// <summary>
        /// Get dashboard summary statistics
        /// </summary>
        [HttpGet("dashboard/summary")]
        public async Task<IActionResult> GetDashboardSummary(
            [FromQuery] Guid? regionId = null,
            [FromQuery] Guid? districtId = null,
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var todayUnix = new DateTimeOffset(today).ToUnixTimeSeconds();
                var monthStart = new DateTime(today.Year, today.Month, 1);
                var monthStartUnix = new DateTimeOffset(monthStart).ToUnixTimeSeconds();

                // Build facility query based on filter
                IQueryable<Facility> facilityQuery = _context.Facilities.Include(d => d.District).Where(f => f.Deleted == 0);
                if (facilityId.HasValue)
                    facilityQuery = facilityQuery.Where(f => f.ID == facilityId);
                else if (districtId.HasValue)
                    facilityQuery = facilityQuery.Where(f => f.DistrictID == districtId);
                else if (regionId.HasValue)
                    facilityQuery = facilityQuery.Where(f => f.District.RegionID == regionId);

                var facilityIds = await facilityQuery.Select(f => f.ID).ToListAsync();

                var summary = new
                {
                    totalRegions = await _context.Regions.CountAsync(r => r.Deleted == 0),
                    totalDistricts = regionId.HasValue
                        ? await _context.Districts.CountAsync(d => d.RegionID == regionId && d.Deleted == 0)
                        : await _context.Districts.CountAsync(d => d.Deleted == 0),
                    totalFacilities = facilityIds.Count,
                    activeFacilities = await facilityQuery.CountAsync(f => f.IsActive),

                    // Delivery stats
                    totalDeliveriesToday = await _context.DailyFacilityStats
                        .Where(s => s.Date >= today && facilityIds.Contains(s.FacilityID))
                        .SumAsync(s => s.TotalDeliveries),
                    totalDeliveriesThisMonth = await _context.MonthlyFacilityStats
                        .Where(s => s.Year == today.Year && s.Month == today.Month && facilityIds.Contains(s.FacilityID))
                        .SumAsync(s => s.TotalDeliveries),
                    totalDeliveriesThisYear = await _context.MonthlyFacilityStats
                        .Where(s => s.Year == today.Year && facilityIds.Contains(s.FacilityID))
                        .SumAsync(s => s.TotalDeliveries),

                    // Active labors - join with Staff to get facility reference
                    activeLabors = await _context.Partographs
                        .Join(_context.Staff, p => p.Handler, s => s.ID, (p, s) => new { p, s })
                        .CountAsync(ps => facilityIds.Contains(ps.s.Facility ?? Guid.Empty) &&
                                        (ps.p.Status == LaborStatus.FirstStage || ps.p.Status == LaborStatus.SecondStage || ps.p.Status == LaborStatus.ThirdStage || ps.p.Status == LaborStatus.FourthStage) &&
                                        ps.p.Deleted == 0),
                    highRiskLabors = await _context.Partographs
                        .Join(_context.Staff, p => p.Handler, s => s.ID, (p, s) => new { p, s })
                        .CountAsync(ps => facilityIds.Contains(ps.s.Facility ?? Guid.Empty) &&
                                        (ps.p.Status == LaborStatus.FirstStage || ps.p.Status == LaborStatus.SecondStage || ps.p.Status == LaborStatus.ThirdStage || ps.p.Status == LaborStatus.FourthStage) &&
                                        ps.p.RiskScore > 0 &&
                                        ps.p.Deleted == 0),

                    // Complications and referrals today
                    complicationsToday = await _context.ComplicationAnalytics
                        .CountAsync(c => c.OccurrenceDateTime >= today && facilityIds.Contains(c.FacilityID)),
                    referralsToday = await _context.Referrals
                        .CountAsync(r => new DateTimeOffset(r.ReferralTime).ToUnixTimeSeconds() >= todayUnix && r.Deleted == 0),

                    // Mortality this month
                    maternalDeaths = await _context.MaternalMortalityRecords
                        .CountAsync(m => m.Year == today.Year && m.Month == today.Month && facilityIds.Contains(m.FacilityID)),
                    neonatalDeaths = await _context.NeonatalOutcomeRecords
                        .CountAsync(n => n.Year == today.Year && n.Month == today.Month &&
                                        n.OutcomeType == "Death" && facilityIds.Contains(n.FacilityID)),
                    stillbirths = await _context.NeonatalOutcomeRecords
                        .CountAsync(n => n.Year == today.Year && n.Month == today.Month &&
                                        n.OutcomeType.Contains("Stillbirth") && facilityIds.Contains(n.FacilityID))
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary");
                return StatusCode(500, new { error = "Failed to get dashboard summary" });
            }
        }

        /// <summary>
        /// Get delivery mode distribution
        /// </summary>
        [HttpGet("dashboard/delivery-modes")]
        public async Task<IActionResult> GetDeliveryModeDistribution(
            [FromQuery] Guid? regionId = null,
            [FromQuery] Guid? districtId = null,
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                IQueryable<Facility> facilityQuery = _context.Facilities.Include(d => d.District).Where(f => f.Deleted == 0);
                if (facilityId.HasValue)
                    facilityQuery = facilityQuery.Where(f => f.ID == facilityId);
                else if (districtId.HasValue)
                    facilityQuery = facilityQuery.Where(f => f.DistrictID == districtId);
                else if (regionId.HasValue)
                    facilityQuery = facilityQuery.Where(f => f.District.RegionID == regionId);

                var facilityIds = await facilityQuery.Select(f => f.ID).ToListAsync();

                var stats = await _context.MonthlyFacilityStats
                    .Where(s => s.Year == today.Year && s.Month == today.Month && facilityIds.Contains(s.FacilityID))
                    .ToListAsync();

                return Ok(new
                {
                    normalVaginal = stats.Sum(s => s.SVDCount),
                    assistedVaginal = stats.Sum(s => s.AssistedDeliveryCount),
                    electiveCaesarean = stats.Sum(s => s.CaesareanCount),
                    emergencyCaesarean = stats.Sum(s => s.CaesareanCount)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery mode distribution");
                return StatusCode(500, new { error = "Failed to get delivery mode distribution" });
            }
        }

        /// <summary>
        /// Get active alerts
        /// </summary>
        [HttpGet("dashboard/alerts")]
        public async Task<IActionResult> GetActiveAlerts(
            [FromQuery] Guid? regionId = null,
            [FromQuery] Guid? districtId = null,
            [FromQuery] Guid? facilityId = null,
            [FromQuery] int limit = 50)
        {
            try
            {
                IQueryable<Facility> facilityQuery = _context.Facilities.Include(d => d.District).Where(f => f.Deleted == 0);
                if (facilityId.HasValue)
                    facilityQuery = facilityQuery.Where(f => f.ID == facilityId);
                else if (districtId.HasValue)
                    facilityQuery = facilityQuery.Where(f => f.DistrictID == districtId);
                else if (regionId.HasValue)
                    facilityQuery = facilityQuery.Where(f => f.District.RegionID == regionId);

                var facilityIds = await facilityQuery.Select(f => f.ID).ToListAsync();

                var alerts = await _context.AlertSummaries
                    .Where(a => !a.Resolved && facilityIds.Contains(a.FacilityID))
                    .OrderByDescending(a => a.AlertSeverity == "Critical")
                    .ThenByDescending(a => a.AlertDateTime)
                    .Take(limit)
                    .Select(a => new
                    {
                        id = a.ID,
                        title = a.AlertType,
                        message = a.ActionDescription,
                        severity = a.AlertSeverity,
                        category = a.AlertCategory,
                        facilityName = a.FacilityName,
                        createdAt = a.AlertDateTime,
                        isResolved = a.Resolved
                    })
                    .ToListAsync();

                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active alerts");
                return StatusCode(500, new { error = "Failed to get active alerts" });
            }
        }

        /// <summary>
        /// Get delivery trend data
        /// </summary>
        [HttpGet("dashboard/trends/deliveries")]
        public async Task<IActionResult> GetDeliveryTrend(
            [FromQuery] Guid? regionId = null,
            [FromQuery] Guid? districtId = null,
            [FromQuery] Guid? facilityId = null,
            [FromQuery] int days = 30)
        {
            try
            {
                var endDate = DateTime.UtcNow.Date;
                var startDate = endDate.AddDays(-days);

                IQueryable<Facility> facilityQuery = _context.Facilities.Include(d => d.District).Where(f => f.Deleted == 0);
                if (facilityId.HasValue)
                    facilityQuery = facilityQuery.Where(f => f.ID == facilityId);
                else if (districtId.HasValue)
                    facilityQuery = facilityQuery.Where(f => f.DistrictID == districtId);
                else if (regionId.HasValue)
                    facilityQuery = facilityQuery.Where(f => f.District.RegionID == regionId);

                var facilityIds = await facilityQuery.Select(f => f.ID).ToListAsync();

                var trends = await _context.DailyFacilityStats
                    .Where(s => s.Date >= startDate && s.Date <= endDate && facilityIds.Contains(s.FacilityID))
                    .GroupBy(s => s.Date)
                    .Select(g => new
                    {
                        date = g.Key,
                        label = g.Key.ToString("MMM dd"),
                        value = g.Sum(s => s.TotalDeliveries)
                    })
                    .OrderBy(t => t.date)
                    .ToListAsync();

                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery trends");
                return StatusCode(500, new { error = "Failed to get delivery trends" });
            }
        }

        #endregion

        #region Region Endpoints

        /// <summary>
        /// Get all regions with summary statistics
        /// </summary>
        [HttpGet("regions")]
        public async Task<IActionResult> GetAllRegions()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var regions = await _context.Regions
                    .Where(r => r.Deleted == 0)
                    .ToListAsync();

                var summaries = new List<object>();

                foreach (var region in regions)
                {
                    var districtCount = await _context.Districts
                        .CountAsync(d => d.RegionID == region.ID && d.Deleted == 0);

                    var facilityIds = await _context.Facilities.Include(d => d.District)
                        .Where(f => f.District.RegionID == region.ID && f.Deleted == 0)
                        .Select(f => f.ID)
                        .ToListAsync();

                    var todayStats = await _context.DailyFacilityStats
                        .Where(s => s.Date >= today && facilityIds.Contains(s.FacilityID))
                        .ToListAsync();

                    var monthlyStats = await _context.MonthlyFacilityStats
                        .Where(s => s.Year == today.Year && s.Month == today.Month && facilityIds.Contains(s.FacilityID))
                        .ToListAsync();

                    var activeLabors = await _context.Partographs
                        .Join(_context.Staff, p => p.Handler, s => s.ID, (p, s) => new { p, s })
                        .CountAsync(ps => facilityIds.Contains(ps.s.Facility ?? Guid.Empty) &&
                                        (ps.p.Status == LaborStatus.FirstStage || ps.p.Status == LaborStatus.SecondStage || ps.p.Status == LaborStatus.ThirdStage || ps.p.Status == LaborStatus.FourthStage) &&
                                        ps.p.Deleted == 0);

                    var totalDeliveries = monthlyStats.Sum(s => s.TotalDeliveries);
                    var caesareans = monthlyStats.Sum(s => s.CaesareanCount);

                    summaries.Add(new
                    {
                        id = region.ID,
                        name = region.Name,
                        code = region.Code,
                        districtCount,
                        facilityCount = facilityIds.Count,
                        deliveriesToday = todayStats.Sum(s => s.TotalDeliveries),
                        deliveriesThisMonth = totalDeliveries,
                        activeLabors,
                        caesareanRate = totalDeliveries > 0 ? (double)caesareans / totalDeliveries * 100 : 0,
                        performanceStatus = "Normal"
                    });
                }

                return Ok(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting regions");
                return StatusCode(500, new { error = "Failed to get regions" });
            }
        }

        /// <summary>
        /// Get region details
        /// </summary>
        [HttpGet("regions/{regionId}")]
        public async Task<IActionResult> GetRegion(Guid regionId)
        {
            try
            {
                var region = await _context.Regions
                    .FirstOrDefaultAsync(r => r.ID == regionId && r.Deleted == 0);

                if (region == null)
                    return NotFound(new { error = "Region not found" });

                var today = DateTime.UtcNow.Date;
                var districtCount = await _context.Districts
                    .CountAsync(d => d.RegionID == regionId && d.Deleted == 0);

                var facilityIds = await _context.Facilities.Include(d => d.District)
                    .Where(f => f.District.RegionID == regionId && f.Deleted == 0)
                    .Select(f => f.ID)
                    .ToListAsync();

                var todayStats = await _context.DailyFacilityStats
                    .Where(s => s.Date >= today && facilityIds.Contains(s.FacilityID))
                    .ToListAsync();

                var monthlyStats = await _context.MonthlyFacilityStats
                    .Where(s => s.Year == today.Year && s.Month == today.Month && facilityIds.Contains(s.FacilityID))
                    .ToListAsync();

                var totalDeliveries = monthlyStats.Sum(s => s.TotalDeliveries);
                var caesareans = monthlyStats.Sum(s => s.CaesareanCount);

                return Ok(new
                {
                    id = region.ID,
                    name = region.Name,
                    code = region.Code,
                    capital = region.Capital,
                    districtCount,
                    facilityCount = facilityIds.Count,
                    deliveriesToday = todayStats.Sum(s => s.TotalDeliveries),
                    deliveriesThisMonth = totalDeliveries,
                    caesareanRate = totalDeliveries > 0 ? (double)caesareans / totalDeliveries * 100 : 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting region {RegionId}", regionId);
                return StatusCode(500, new { error = "Failed to get region" });
            }
        }

        #endregion

        #region District Endpoints

        /// <summary>
        /// Get districts (optionally filtered by region)
        /// </summary>
        [HttpGet("districts")]
        public async Task<IActionResult> GetDistricts([FromQuery] Guid? regionId = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                var query = _context.Districts
                    .Include(d => d.Region)
                    .Where(d => d.Deleted == 0);
                if (regionId.HasValue)
                    query = query.Where(d => d.RegionID == regionId);

                var districts = await query.ToListAsync();
                var summaries = new List<object>();

                foreach (var district in districts)
                {
                    var facilityIds = await _context.Facilities
                        .Where(f => f.DistrictID == district.ID && f.Deleted == 0)
                        .Select(f => f.ID)
                        .ToListAsync();

                    var activeFacilities = await _context.Facilities
                        .CountAsync(f => f.DistrictID == district.ID && f.IsActive && f.Deleted == 0);

                    var todayStats = await _context.DailyFacilityStats
                        .Where(s => s.Date >= today && facilityIds.Contains(s.FacilityID))
                        .ToListAsync();

                    var monthlyStats = await _context.MonthlyFacilityStats
                        .Where(s => s.Year == today.Year && s.Month == today.Month && facilityIds.Contains(s.FacilityID))
                        .ToListAsync();

                    var activeLabors = await _context.Partographs
                        .Join(_context.Staff, p => p.Handler, s => s.ID, (p, s) => new { p, s })
                        .CountAsync(ps => facilityIds.Contains(ps.s.Facility ?? Guid.Empty) &&
                                        (ps.p.Status == LaborStatus.FirstStage || ps.p.Status == LaborStatus.SecondStage || ps.p.Status == LaborStatus.ThirdStage || ps.p.Status == LaborStatus.FourthStage) &&
                                        ps.p.Deleted == 0);

                    var totalDeliveries = monthlyStats.Sum(s => s.TotalDeliveries);
                    var caesareans = monthlyStats.Sum(s => s.CaesareanCount);

                    summaries.Add(new
                    {
                        id = district.ID,
                        name = district.Name,
                        code = district.Code,
                        type = district.Type,
                        regionId = district.RegionID,
                        regionName = district.Region?.Name,
                        facilityCount = facilityIds.Count,
                        activeFacilities,
                        deliveriesToday = todayStats.Sum(s => s.TotalDeliveries),
                        deliveriesThisMonth = totalDeliveries,
                        activeLabors,
                        caesareanRate = totalDeliveries > 0 ? (double)caesareans / totalDeliveries * 100 : 0,
                        performanceStatus = "Normal"
                    });
                }

                return Ok(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting districts");
                return StatusCode(500, new { error = "Failed to get districts" });
            }
        }

        /// <summary>
        /// Get district details
        /// </summary>
        [HttpGet("districts/{districtId}")]
        public async Task<IActionResult> GetDistrict(Guid districtId)
        {
            try
            {
                var district = await _context.Districts
                    .Include(d => d.Region)
                    .FirstOrDefaultAsync(d => d.ID == districtId && d.Deleted == 0);

                if (district == null)
                    return NotFound(new { error = "District not found" });

                var today = DateTime.UtcNow.Date;
                var facilityIds = await _context.Facilities
                    .Where(f => f.DistrictID == districtId && f.Deleted == 0)
                    .Select(f => f.ID)
                    .ToListAsync();

                var monthlyStats = await _context.MonthlyFacilityStats
                    .Where(s => s.Year == today.Year && s.Month == today.Month && facilityIds.Contains(s.FacilityID))
                    .ToListAsync();

                var totalDeliveries = monthlyStats.Sum(s => s.TotalDeliveries);
                var caesareans = monthlyStats.Sum(s => s.CaesareanCount);

                return Ok(new
                {
                    id = district.ID,
                    name = district.Name,
                    code = district.Code,
                    type = district.Type,
                    regionId = district.RegionID,
                    regionName = district.Region?.Name,
                    capital = district.Capital,
                    facilityCount = facilityIds.Count,
                    deliveriesThisMonth = totalDeliveries,
                    caesareanRate = totalDeliveries > 0 ? (double)caesareans / totalDeliveries * 100 : 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting district {DistrictId}", districtId);
                return StatusCode(500, new { error = "Failed to get district" });
            }
        }

        #endregion

        #region Facility Endpoints

        /// <summary>
        /// Get facilities (optionally filtered by district or region)
        /// </summary>
        [HttpGet("facilities")]
        public async Task<IActionResult> GetFacilities([FromQuery] Guid? districtId = null, [FromQuery] Guid? regionId = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                var query = _context.Facilities
                    .Include(f => f.District)
                        .ThenInclude(d => d!.Region)
                    .Where(f => f.Deleted == 0);
                if (districtId.HasValue)
                    query = query.Where(f => f.DistrictID == districtId);
                if (regionId.HasValue)
                    query = query.Where(f => f.District != null && f.District.RegionID == regionId);

                var facilities = await query.ToListAsync();
                var summaries = new List<object>();

                foreach (var facility in facilities)
                {
                    if (!facility.ID.HasValue) continue;

                    var todayStats = await _context.DailyFacilityStats
                        .FirstOrDefaultAsync(s => s.Date >= today && s.FacilityID == facility.ID.Value);

                    var monthlyStats = await _context.MonthlyFacilityStats
                        .FirstOrDefaultAsync(s => s.Year == today.Year && s.Month == today.Month && s.FacilityID == facility.ID.Value);

                    var activeLabors = await _context.Partographs
                        .Join(_context.Staff, p => p.Handler, s => s.ID, (p, s) => new { p, s })
                        .CountAsync(ps => ps.s.Facility == facility.ID.Value &&
                                        (ps.p.Status == LaborStatus.FirstStage || ps.p.Status == LaborStatus.SecondStage || ps.p.Status == LaborStatus.ThirdStage || ps.p.Status == LaborStatus.FourthStage) &&
                                        ps.p.Deleted == 0);

                    var staffCount = await _context.Staff
                        .CountAsync(s => s.Facility == facility.ID.Value && s.Deleted == 0);

                    var lastActivity = await _context.Partographs
                        .Join(_context.Staff, p => p.Handler, s => s.ID, (p, s) => new { p, s })
                        .Where(ps => ps.s.Facility == facility.ID.Value && ps.p.Deleted == 0)
                        .OrderByDescending(ps => ps.p.UpdatedTime)
                        .Select(ps => ps.p.UpdatedTime)
                        .FirstOrDefaultAsync();

                    summaries.Add(new
                    {
                        id = facility.ID.Value,
                        name = facility.Name,
                        code = facility.Code,
                        type = facility.Type,
                        level = facility.Level,
                        districtId = facility.DistrictID,
                        districtName = facility.District?.Name,
                        regionId = facility.District?.RegionID,
                        regionName = facility.District?.Region?.Name,
                        isActive = facility.IsActive,
                        deliveriesToday = todayStats?.TotalDeliveries ?? 0,
                        deliveriesThisMonth = monthlyStats?.TotalDeliveries ?? 0,
                        activeLabors,
                        staffCount,
                        lastActivityTime = lastActivity > 0 ? DateTimeOffset.FromUnixTimeSeconds(lastActivity).DateTime : (DateTime?)null,
                        performanceStatus = facility.IsActive ? "Normal" : "Inactive"
                    });
                }

                return Ok(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facilities");
                return StatusCode(500, new { error = "Failed to get facilities" });
            }
        }

        /// <summary>
        /// Get facility details
        /// </summary>
        [HttpGet("facilities/{facilityId}")]
        public async Task<IActionResult> GetFacility(Guid facilityId)
        {
            try
            {
                var facility = await _context.Facilities
                    .Include(f => f.District)
                        .ThenInclude(d => d!.Region)
                    .FirstOrDefaultAsync(f => f.ID == facilityId && f.Deleted == 0);

                if (facility == null)
                    return NotFound(new { error = "Facility not found" });

                var today = DateTime.UtcNow.Date;
                var todayStats = await _context.DailyFacilityStats
                    .FirstOrDefaultAsync(s => s.Date >= today && s.FacilityID == facilityId);

                var monthlyStats = await _context.MonthlyFacilityStats
                    .FirstOrDefaultAsync(s => s.Year == today.Year && s.Month == today.Month && s.FacilityID == facilityId);

                return Ok(new
                {
                    id = facility.ID,
                    name = facility.Name,
                    code = facility.Code,
                    type = facility.Type,
                    level = facility.Level,
                    address = facility.Address,
                    city = facility.City,
                    regionId = facility.District?.RegionID,
                    regionName = facility.District?.Region?.Name,
                    districtId = facility.DistrictID,
                    districtName = facility.District?.Name,
                    phone = facility.Phone,
                    email = facility.Email,
                    latitude = facility.Latitude,
                    longitude = facility.Longitude,
                    isActive = facility.IsActive,
                    deliveriesToday = todayStats?.TotalDeliveries ?? 0,
                    deliveriesThisMonth = monthlyStats?.TotalDeliveries ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facility {FacilityId}", facilityId);
                return StatusCode(500, new { error = "Failed to get facility" });
            }
        }

        #endregion

        #region Helper Methods

        private string GenerateJwtToken(MonitoringUser user)
        {
            var secret = _configuration["JwtSettings:Secret"] ?? "5f021d67-3ceb-44cd-8f55-5b10ca9039e1-monitoring";
            var issuer = _configuration["JwtSettings:Issuer"] ?? "PartographMonitoringDashboard";
            var audience = _configuration["JwtSettings:Audience"] ?? "MonitoringUsers";
            var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "480");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("AccessLevel", user.AccessLevel)
            };

            if (user.RegionID.HasValue)
                claims.Add(new Claim("RegionID", user.RegionID.Value.ToString()));

            if (user.DistrictID.HasValue)
                claims.Add(new Claim("DistrictID", user.DistrictID.Value.ToString()));

            if (user.FacilityID.HasValue)
                claims.Add(new Claim("FacilityID", user.FacilityID.Value.ToString()));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private bool VerifyPassword(string password, string? passwordHash)
        {
            if (string.IsNullOrEmpty(passwordHash))
                return false;

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var computedHash = Convert.ToBase64String(hashBytes);
            return computedHash == passwordHash;
        }

        #endregion
    }

    #region Request Models

    public class MonitoringLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    #endregion
}
