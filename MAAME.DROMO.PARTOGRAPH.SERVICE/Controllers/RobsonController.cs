using MAAME.DROMO.PARTOGRAPH.MODEL;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Controllers
{
    /// <summary>
    /// Robson Classification API Controller
    /// Implements WHO Robson Classification: Implementation Manual (2017)
    /// ISBN 978-92-4-151319-7
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RobsonController : ControllerBase
    {
        private readonly PartographDbContext _context;
        private readonly IRobsonService _robsonService;
        private readonly ILogger<RobsonController> _logger;

        public RobsonController(
            PartographDbContext context,
            IRobsonService robsonService,
            ILogger<RobsonController> logger)
        {
            _context = context;
            _robsonService = robsonService;
            _logger = logger;
        }

        /// <summary>
        /// Get complete Robson Classification report for a period
        /// </summary>
        [HttpGet("report")]
        public async Task<ActionResult<RobsonClassificationReport>> GetReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? facilityId = null,
            [FromQuery] Guid? regionId = null,
            [FromQuery] Guid? districtId = null)
        {
            try
            {
                var report = await _robsonService.GenerateReportAsync(
                    startDate, endDate, facilityId, regionId, districtId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Robson report");
                return StatusCode(500, new { error = "Failed to generate report", message = ex.Message });
            }
        }

        /// <summary>
        /// Get Robson dashboard summary for quick overview
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<RobsonDashboardSummary>> GetDashboard(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? facilityId = null,
            [FromQuery] Guid? regionId = null,
            [FromQuery] Guid? districtId = null)
        {
            try
            {
                var start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var end = endDate ?? DateTime.Now;

                var summary = await _robsonService.GetDashboardSummaryAsync(
                    start, end, facilityId, regionId, districtId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Robson dashboard");
                return StatusCode(500, new { error = "Failed to get dashboard", message = ex.Message });
            }
        }

        /// <summary>
        /// Get Robson group statistics
        /// </summary>
        [HttpGet("groups")]
        public async Task<ActionResult<List<RobsonGroupStatistics>>> GetGroupStatistics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                var stats = await _robsonService.GetGroupStatisticsAsync(startDate, endDate, facilityId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group statistics");
                return StatusCode(500, new { error = "Failed to get group statistics", message = ex.Message });
            }
        }

        /// <summary>
        /// Get monthly trends for Robson analysis
        /// </summary>
        [HttpGet("trends")]
        public async Task<ActionResult<List<RobsonMonthlyTrend>>> GetMonthlyTrends(
            [FromQuery] int year,
            [FromQuery] Guid? facilityId = null,
            [FromQuery] Guid? regionId = null)
        {
            try
            {
                var trends = await _robsonService.GetMonthlyTrendsAsync(year, facilityId, regionId);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly trends");
                return StatusCode(500, new { error = "Failed to get trends", message = ex.Message });
            }
        }

        /// <summary>
        /// Get comparative report across facilities
        /// </summary>
        [HttpGet("comparative")]
        public async Task<ActionResult<RobsonComparativeReport>> GetComparativeReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? regionId = null)
        {
            try
            {
                var report = await _robsonService.GetComparativeReportAsync(startDate, endDate, regionId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comparative report");
                return StatusCode(500, new { error = "Failed to get comparative report", message = ex.Message });
            }
        }

        /// <summary>
        /// Get quality indicators derived from Robson analysis
        /// </summary>
        [HttpGet("quality-indicators")]
        public async Task<ActionResult<RobsonQualityIndicators>> GetQualityIndicators(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                var indicators = await _robsonService.GetQualityIndicatorsAsync(startDate, endDate, facilityId);
                return Ok(indicators);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quality indicators");
                return StatusCode(500, new { error = "Failed to get quality indicators", message = ex.Message });
            }
        }

        /// <summary>
        /// Get Group 2 sub-analysis (2a: induced vs 2b: pre-labor CS)
        /// </summary>
        [HttpGet("group2-analysis")]
        public async Task<ActionResult<Group2SubAnalysis>> GetGroup2Analysis(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                var analysis = await _robsonService.GetGroup2AnalysisAsync(startDate, endDate, facilityId);
                return Ok(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Group 2 analysis");
                return StatusCode(500, new { error = "Failed to get Group 2 analysis", message = ex.Message });
            }
        }

        /// <summary>
        /// Get Group 5 sub-analysis (VBAC analysis)
        /// </summary>
        [HttpGet("group5-analysis")]
        public async Task<ActionResult<Group5SubAnalysis>> GetGroup5Analysis(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                var analysis = await _robsonService.GetGroup5AnalysisAsync(startDate, endDate, facilityId);
                return Ok(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Group 5 analysis");
                return StatusCode(500, new { error = "Failed to get Group 5 analysis", message = ex.Message });
            }
        }

        /// <summary>
        /// Get individual case records for audit
        /// </summary>
        [HttpGet("cases")]
        public async Task<ActionResult<List<RobsonCaseRecord>>> GetCaseRecords(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] RobsonGroup? group = null,
            [FromQuery] Guid? facilityId = null,
            [FromQuery] int pageSize = 100,
            [FromQuery] int page = 1)
        {
            try
            {
                var cases = await _robsonService.GetCaseRecordsAsync(
                    startDate, endDate, group, facilityId, pageSize, page);
                return Ok(cases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting case records");
                return StatusCode(500, new { error = "Failed to get case records", message = ex.Message });
            }
        }

        /// <summary>
        /// Get action items based on Robson analysis
        /// </summary>
        [HttpGet("action-items")]
        public async Task<ActionResult<List<RobsonActionItem>>> GetActionItems(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                var items = await _robsonService.GetActionItemsAsync(startDate, endDate, facilityId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting action items");
                return StatusCode(500, new { error = "Failed to get action items", message = ex.Message });
            }
        }

        /// <summary>
        /// Classify a single delivery and return/store the Robson group
        /// </summary>
        [HttpPost("classify/{partographId}")]
        public async Task<ActionResult<RobsonClassification>> ClassifyDelivery(Guid partographId)
        {
            try
            {
                var classification = await _robsonService.ClassifyDeliveryAsync(partographId);
                if (classification == null)
                    return NotFound(new { error = "Partograph not found or cannot be classified" });

                return Ok(classification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error classifying delivery {PartographId}", partographId);
                return StatusCode(500, new { error = "Failed to classify delivery", message = ex.Message });
            }
        }

        /// <summary>
        /// Batch classify all unclassified deliveries in a date range
        /// </summary>
        [HttpPost("batch-classify")]
        public async Task<ActionResult<BatchClassifyResponse>> BatchClassifyDeliveries(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? facilityId = null)
        {
            try
            {
                var result = await _robsonService.BatchClassifyDeliveriesAsync(startDate, endDate, facilityId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch classifying deliveries");
                return StatusCode(500, new { error = "Failed to batch classify", message = ex.Message });
            }
        }

        /// <summary>
        /// Get all Robson classifications (for admin/audit)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<RobsonClassification>>> GetClassifications(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] RobsonGroup? group = null,
            [FromQuery] int pageSize = 50,
            [FromQuery] int page = 1)
        {
            try
            {
                var query = _context.RobsonClassifications.AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(r => r.ClassifiedAt >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(r => r.ClassifiedAt <= endDate.Value);
                if (group.HasValue)
                    query = query.Where(r => r.Group == group.Value);

                var classifications = await query
                    .OrderByDescending(r => r.ClassifiedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(classifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting classifications");
                return StatusCode(500, new { error = "Failed to get classifications", message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Response model for batch classification
    /// </summary>
    public class BatchClassifyResponse
    {
        public int ClassifiedCount { get; set; }
        public int FailedCount { get; set; }
        public int AlreadyClassifiedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
