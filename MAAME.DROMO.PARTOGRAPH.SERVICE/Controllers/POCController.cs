using MAAME.DROMO.PARTOGRAPH.MODEL;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Data;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Controllers
{
    /// <summary>
    /// POC (Proof of Concept) Metrics API Controller
    /// Provides endpoints for tracking all 5 POC objectives:
    /// 1. Digital Partograph Adoption (70% target)
    /// 2. User Satisfaction (4.0/5.0 target)
    /// 3. Real-Time Reporting (70% within 30 min)
    /// 4. Complication Reduction (15% reduction)
    /// 5. Emergency Response Time (30% reduction)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class POCController : ControllerBase
    {
        private readonly PartographDbContext _context;
        private readonly IPOCMetricsService _pocService;
        private readonly ILogger<POCController> _logger;

        public POCController(
            PartographDbContext context,
            IPOCMetricsService pocService,
            ILogger<POCController> logger)
        {
            _context = context;
            _pocService = pocService;
            _logger = logger;
        }

        #region POC Progress Endpoints

        /// <summary>
        /// Get overall POC progress with all 5 metrics
        /// </summary>
        [HttpGet("progress")]
        public async Task<ActionResult<POCProgress>> GetPOCProgress(
            [FromQuery] Guid? facilityId = null,
            [FromQuery] Guid? districtId = null,
            [FromQuery] Guid? regionId = null)
        {
            try
            {
                var progress = await _pocService.GetPOCProgressAsync(facilityId, districtId, regionId);
                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting POC progress");
                return StatusCode(500, new { error = "Failed to retrieve POC progress" });
            }
        }

        /// <summary>
        /// Get national POC progress summary
        /// </summary>
        [HttpGet("progress/national")]
        public async Task<ActionResult<POCProgress>> GetNationalPOCProgress()
        {
            try
            {
                var progress = await _pocService.GetNationalPOCProgressAsync();
                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting national POC progress");
                return StatusCode(500, new { error = "Failed to retrieve national POC progress" });
            }
        }

        /// <summary>
        /// Get POC progress history for trend analysis
        /// </summary>
        [HttpGet("progress/history")]
        public async Task<ActionResult<List<POCProgress>>> GetPOCProgressHistory(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string periodType = "Monthly")
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-9);
                var end = endDate ?? DateTime.UtcNow;

                var history = await _pocService.GetPOCProgressHistoryAsync(start, end, periodType);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting POC progress history");
                return StatusCode(500, new { error = "Failed to retrieve POC progress history" });
            }
        }

        #endregion

        #region Individual POC Metrics

        /// <summary>
        /// POC 1: Get adoption metrics
        /// </summary>
        [HttpGet("metrics/adoption")]
        public async Task<ActionResult<AdoptionMetrics>> GetAdoptionMetrics([FromQuery] Guid? facilityId = null)
        {
            try
            {
                var metrics = await _pocService.GetAdoptionMetricsAsync(facilityId);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting adoption metrics");
                return StatusCode(500, new { error = "Failed to retrieve adoption metrics" });
            }
        }

        /// <summary>
        /// POC 2: Get satisfaction metrics
        /// </summary>
        [HttpGet("metrics/satisfaction")]
        public async Task<ActionResult<SatisfactionMetrics>> GetSatisfactionMetrics([FromQuery] Guid? facilityId = null)
        {
            try
            {
                var metrics = await _pocService.GetSatisfactionMetricsAsync(facilityId);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting satisfaction metrics");
                return StatusCode(500, new { error = "Failed to retrieve satisfaction metrics" });
            }
        }

        /// <summary>
        /// POC 3: Get emergency reporting metrics
        /// </summary>
        [HttpGet("metrics/reporting")]
        public async Task<ActionResult<EmergencyReportingMetrics>> GetEmergencyReportingMetrics([FromQuery] Guid? facilityId = null)
        {
            try
            {
                var metrics = await _pocService.GetEmergencyReportingMetricsAsync(facilityId);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting emergency reporting metrics");
                return StatusCode(500, new { error = "Failed to retrieve emergency reporting metrics" });
            }
        }

        /// <summary>
        /// POC 4: Get complication reduction metrics
        /// </summary>
        [HttpGet("metrics/complications")]
        public async Task<ActionResult<ComplicationReductionMetrics>> GetComplicationReductionMetrics([FromQuery] Guid? facilityId = null)
        {
            try
            {
                var metrics = await _pocService.GetComplicationReductionMetricsAsync(facilityId);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting complication reduction metrics");
                return StatusCode(500, new { error = "Failed to retrieve complication reduction metrics" });
            }
        }

        /// <summary>
        /// POC 5: Get response time metrics
        /// </summary>
        [HttpGet("metrics/response-time")]
        public async Task<ActionResult<ResponseTimeMetrics>> GetResponseTimeMetrics([FromQuery] Guid? facilityId = null)
        {
            try
            {
                var metrics = await _pocService.GetResponseTimeMetricsAsync(facilityId);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting response time metrics");
                return StatusCode(500, new { error = "Failed to retrieve response time metrics" });
            }
        }

        #endregion

        #region Baseline Management

        /// <summary>
        /// Get or create baseline for comparison
        /// </summary>
        [HttpGet("baseline")]
        public async Task<ActionResult<POCBaseline>> GetBaseline([FromQuery] Guid? facilityId = null)
        {
            try
            {
                var baseline = await _pocService.GetOrCreateBaselineAsync(facilityId);
                return Ok(baseline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting baseline");
                return StatusCode(500, new { error = "Failed to retrieve baseline" });
            }
        }

        /// <summary>
        /// Update baseline settings
        /// </summary>
        [HttpPut("baseline")]
        public async Task<ActionResult<POCBaseline>> UpdateBaseline([FromBody] POCBaseline baseline)
        {
            try
            {
                var updated = await _pocService.UpdateBaselineAsync(baseline);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating baseline");
                return StatusCode(500, new { error = "Failed to update baseline" });
            }
        }

        #endregion

        #region Survey Endpoints

        /// <summary>
        /// Get active surveys
        /// </summary>
        [HttpGet("surveys")]
        public async Task<ActionResult<List<UserSurvey>>> GetActiveSurveys()
        {
            try
            {
                var surveys = await _context.UserSurveys
                    .Where(s => s.Deleted == 0 && s.IsActive)
                    .OrderByDescending(s => s.CreatedTime)
                    .ToListAsync();

                return Ok(surveys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting surveys");
                return StatusCode(500, new { error = "Failed to retrieve surveys" });
            }
        }

        /// <summary>
        /// Get a specific survey by ID
        /// </summary>
        [HttpGet("surveys/{id}")]
        public async Task<ActionResult<UserSurvey>> GetSurvey(Guid id)
        {
            try
            {
                var survey = await _context.UserSurveys.FindAsync(id);
                if (survey == null || survey.Deleted != 0)
                    return NotFound();

                return Ok(survey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting survey {SurveyId}", id);
                return StatusCode(500, new { error = "Failed to retrieve survey" });
            }
        }

        /// <summary>
        /// Create a new survey
        /// </summary>
        [HttpPost("surveys")]
        public async Task<ActionResult<UserSurvey>> CreateSurvey([FromBody] UserSurvey survey)
        {
            try
            {
                survey.ID = Guid.NewGuid();
                survey.CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                survey.UpdatedTime = survey.CreatedTime;

                _context.UserSurveys.Add(survey);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetSurvey), new { id = survey.ID }, survey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating survey");
                return StatusCode(500, new { error = "Failed to create survey" });
            }
        }

        /// <summary>
        /// Submit a survey response
        /// </summary>
        [HttpPost("surveys/{surveyId}/responses")]
        public async Task<ActionResult<SurveyResponse>> SubmitSurveyResponse(Guid surveyId, [FromBody] SurveyResponse response)
        {
            try
            {
                var survey = await _context.UserSurveys.FindAsync(surveyId);
                if (survey == null || survey.Deleted != 0 || !survey.IsActive)
                    return NotFound("Survey not found or inactive");

                response.ID = Guid.NewGuid();
                response.SurveyID = surveyId;
                response.SubmittedAt = DateTime.UtcNow;
                response.CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                response.UpdatedTime = response.CreatedTime;

                // Calculate category scores from answers if not provided
                if (response.Answers.Any())
                {
                    var categoryScores = response.Answers
                        .Where(a => a.RatingValue.HasValue)
                        .GroupBy(a => a.Category)
                        .ToDictionary(g => g.Key, g => (decimal)g.Average(a => a.RatingValue!.Value));

                    if (categoryScores.ContainsKey("EaseOfUse"))
                        response.EaseOfUseScore = categoryScores["EaseOfUse"];
                    if (categoryScores.ContainsKey("WorkflowImpact"))
                        response.WorkflowImpactScore = categoryScores["WorkflowImpact"];
                    if (categoryScores.ContainsKey("PerceivedBenefits"))
                        response.PerceivedBenefitsScore = categoryScores["PerceivedBenefits"];
                    if (categoryScores.ContainsKey("TrainingAdequacy"))
                        response.TrainingAdequacyScore = categoryScores["TrainingAdequacy"];
                    if (categoryScores.ContainsKey("Recommendation"))
                        response.RecommendationScore = categoryScores["Recommendation"];

                    // Calculate overall score
                    var allRatings = response.Answers
                        .Where(a => a.RatingValue.HasValue)
                        .Select(a => a.RatingValue!.Value)
                        .ToList();

                    if (allRatings.Any())
                    {
                        response.OverallSatisfactionScore = Math.Round((decimal)allRatings.Average(), 2);
                    }
                }

                _context.SurveyResponses.Add(response);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetSurveyResponse), new { id = response.ID }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting survey response");
                return StatusCode(500, new { error = "Failed to submit survey response" });
            }
        }

        /// <summary>
        /// Get a survey response
        /// </summary>
        [HttpGet("surveys/responses/{id}")]
        public async Task<ActionResult<SurveyResponse>> GetSurveyResponse(Guid id)
        {
            try
            {
                var response = await _context.SurveyResponses
                    .Include(r => r.Survey)
                    .FirstOrDefaultAsync(r => r.ID == id && r.Deleted == 0);

                if (response == null)
                    return NotFound();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting survey response {ResponseId}", id);
                return StatusCode(500, new { error = "Failed to retrieve survey response" });
            }
        }

        /// <summary>
        /// Get survey responses summary
        /// </summary>
        [HttpGet("surveys/{surveyId}/responses/summary")]
        public async Task<ActionResult> GetSurveyResponsesSummary(Guid surveyId)
        {
            try
            {
                var responses = await _context.SurveyResponses
                    .Where(r => r.SurveyID == surveyId && r.Deleted == 0)
                    .ToListAsync();

                var summary = new
                {
                    TotalResponses = responses.Count,
                    AverageOverallScore = responses.Any() ? Math.Round(responses.Average(r => (double)r.OverallSatisfactionScore), 2) : 0,
                    AverageEaseOfUse = responses.Where(r => r.EaseOfUseScore.HasValue).Any()
                        ? Math.Round(responses.Where(r => r.EaseOfUseScore.HasValue).Average(r => (double)r.EaseOfUseScore!.Value), 2) : (double?)null,
                    AverageWorkflowImpact = responses.Where(r => r.WorkflowImpactScore.HasValue).Any()
                        ? Math.Round(responses.Where(r => r.WorkflowImpactScore.HasValue).Average(r => (double)r.WorkflowImpactScore!.Value), 2) : (double?)null,
                    AveragePerceivedBenefits = responses.Where(r => r.PerceivedBenefitsScore.HasValue).Any()
                        ? Math.Round(responses.Where(r => r.PerceivedBenefitsScore.HasValue).Average(r => (double)r.PerceivedBenefitsScore!.Value), 2) : (double?)null,
                    ScoreDistribution = responses
                        .GroupBy(r => (int)Math.Floor(r.OverallSatisfactionScore))
                        .Select(g => new { Score = g.Key, Count = g.Count() })
                        .OrderBy(s => s.Score)
                        .ToList(),
                    ResponsesByRole = responses
                        .GroupBy(r => r.StaffRole)
                        .Select(g => new { Role = g.Key, Count = g.Count(), AvgScore = Math.Round(g.Average(r => (double)r.OverallSatisfactionScore), 2) })
                        .ToList()
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting survey responses summary");
                return StatusCode(500, new { error = "Failed to retrieve survey responses summary" });
            }
        }

        #endregion

        #region User Action Logging

        /// <summary>
        /// Log a user action for adoption tracking
        /// </summary>
        [HttpPost("actions")]
        public async Task<ActionResult> LogUserAction([FromBody] UserActionLog action)
        {
            try
            {
                action.ID = Guid.NewGuid();
                action.ActionTime = DateTime.UtcNow;
                action.CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                _context.UserActionLogs.Add(action);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, actionId = action.ID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging user action");
                return StatusCode(500, new { error = "Failed to log user action" });
            }
        }

        /// <summary>
        /// Get user action summary for adoption analysis
        /// </summary>
        [HttpGet("actions/summary")]
        public async Task<ActionResult> GetUserActionsSummary(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? staffId = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var query = _context.UserActionLogs
                    .Where(a => a.Deleted == 0 && a.ActionTime >= start && a.ActionTime <= end);

                if (staffId.HasValue)
                    query = query.Where(a => a.StaffID == staffId);

                var actions = await query.ToListAsync();

                var summary = new
                {
                    TotalActions = actions.Count,
                    UniqueUsers = actions.Select(a => a.StaffID).Distinct().Count(),
                    ActionBreakdown = actions
                        .GroupBy(a => a.ActionType)
                        .Select(g => new { Action = g.Key.ToString(), Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToList(),
                    DailyActivity = actions
                        .GroupBy(a => a.ActionTime.Date)
                        .Select(g => new { Date = g.Key, Count = g.Count() })
                        .OrderBy(x => x.Date)
                        .ToList(),
                    TopUsers = actions
                        .GroupBy(a => new { a.StaffID, a.StaffName })
                        .Select(g => new { g.Key.StaffID, g.Key.StaffName, ActionCount = g.Count() })
                        .OrderByDescending(x => x.ActionCount)
                        .Take(10)
                        .ToList()
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user actions summary");
                return StatusCode(500, new { error = "Failed to retrieve user actions summary" });
            }
        }

        #endregion

        #region POC Dashboard Data

        /// <summary>
        /// Get comprehensive dashboard data for POC monitoring
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult> GetPOCDashboard()
        {
            try
            {
                var progress = await _pocService.GetNationalPOCProgressAsync();
                var adoption = await _pocService.GetAdoptionMetricsAsync();
                var satisfaction = await _pocService.GetSatisfactionMetricsAsync();
                var reporting = await _pocService.GetEmergencyReportingMetricsAsync();
                var complications = await _pocService.GetComplicationReductionMetricsAsync();
                var responseTime = await _pocService.GetResponseTimeMetricsAsync();
                var baseline = await _pocService.GetOrCreateBaselineAsync();

                return Ok(new
                {
                    Summary = progress,
                    Adoption = adoption,
                    Satisfaction = satisfaction,
                    Reporting = reporting,
                    Complications = complications,
                    ResponseTime = responseTime,
                    Baseline = baseline,
                    GeneratedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting POC dashboard data");
                return StatusCode(500, new { error = "Failed to retrieve POC dashboard data" });
            }
        }

        #endregion

        #region Seed Default Survey

        /// <summary>
        /// Create the default satisfaction survey
        /// </summary>
        [HttpPost("surveys/seed-default")]
        public async Task<ActionResult> SeedDefaultSurvey()
        {
            try
            {
                // Check if default survey exists
                var existingSurvey = await _context.UserSurveys
                    .FirstOrDefaultAsync(s => s.Title == "MAAME-DROMO User Satisfaction Survey" && s.Deleted == 0);

                if (existingSurvey != null)
                    return Ok(new { message = "Default survey already exists", surveyId = existingSurvey.ID });

                var survey = new UserSurvey
                {
                    ID = Guid.NewGuid(),
                    Title = "MAAME-DROMO User Satisfaction Survey",
                    Description = "Please share your experience using MAAME-DROMO to help us improve the system.",
                    Type = SurveyType.Satisfaction,
                    Frequency = SurveyFrequency.Monthly,
                    IsActive = true,
                    StartDate = DateTime.UtcNow,
                    TargetResponseCount = 500,
                    Questions = new List<SurveyQuestion>
                    {
                        new SurveyQuestion
                        {
                            Order = 1,
                            QuestionText = "How easy is it to use MAAME-DROMO for labor monitoring?",
                            Type = QuestionType.Rating,
                            Category = "EaseOfUse",
                            MinRating = 1,
                            MaxRating = 5,
                            IsRequired = true
                        },
                        new SurveyQuestion
                        {
                            Order = 2,
                            QuestionText = "How well does MAAME-DROMO integrate into your daily workflow?",
                            Type = QuestionType.Rating,
                            Category = "WorkflowImpact",
                            MinRating = 1,
                            MaxRating = 5,
                            IsRequired = true
                        },
                        new SurveyQuestion
                        {
                            Order = 3,
                            QuestionText = "How helpful are the automated alerts and recommendations?",
                            Type = QuestionType.Rating,
                            Category = "PerceivedBenefits",
                            MinRating = 1,
                            MaxRating = 5,
                            IsRequired = true
                        },
                        new SurveyQuestion
                        {
                            Order = 4,
                            QuestionText = "How adequate was the training you received for using MAAME-DROMO?",
                            Type = QuestionType.Rating,
                            Category = "TrainingAdequacy",
                            MinRating = 1,
                            MaxRating = 5,
                            IsRequired = true
                        },
                        new SurveyQuestion
                        {
                            Order = 5,
                            QuestionText = "How likely are you to recommend MAAME-DROMO to colleagues?",
                            Type = QuestionType.Rating,
                            Category = "Recommendation",
                            MinRating = 1,
                            MaxRating = 5,
                            IsRequired = true
                        },
                        new SurveyQuestion
                        {
                            Order = 6,
                            QuestionText = "Has MAAME-DROMO improved patient care at your facility?",
                            Type = QuestionType.YesNo,
                            Category = "PerceivedBenefits",
                            IsRequired = true
                        },
                        new SurveyQuestion
                        {
                            Order = 7,
                            QuestionText = "What features of MAAME-DROMO do you find most useful?",
                            Type = QuestionType.MultiSelect,
                            Category = "PerceivedBenefits",
                            Options = new List<string>
                            {
                                "Labor progress monitoring",
                                "Automated alerts",
                                "Referral management",
                                "Report generation",
                                "Offline functionality"
                            },
                            IsRequired = false
                        },
                        new SurveyQuestion
                        {
                            Order = 8,
                            QuestionText = "What improvements would you like to see in MAAME-DROMO?",
                            Type = QuestionType.Text,
                            Category = "Feedback",
                            IsRequired = false
                        }
                    },
                    CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                _context.UserSurveys.Add(survey);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Default survey created successfully", surveyId = survey.ID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding default survey");
                return StatusCode(500, new { error = "Failed to seed default survey" });
            }
        }

        #endregion
    }
}
