using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    /// <summary>
    /// Automatic FHR pattern analysis service implementing WHO 2020 Labour Care Guide algorithms
    /// Detects deceleration patterns based on FHR readings and contraction timing
    /// </summary>
    public class FHRPatternAnalysisService
    {
        private readonly ILogger<FHRPatternAnalysisService> _logger;

        // WHO 2020 FHR thresholds
        private const int FHR_BASELINE_MIN = 110;
        private const int FHR_BASELINE_MAX = 160;
        private const int FHR_DECELERATION_THRESHOLD = 15; // bpm drop from baseline
        private const int PROLONGED_DECELERATION_MINUTES = 2;

        public FHRPatternAnalysisService(ILogger<FHRPatternAnalysisService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Analyzes FHR pattern and automatically detects deceleration type
        /// Based on WHO 2020 guidelines and standard CTG interpretation
        /// </summary>
        public FHRAnalysisResult AnalyzeFHRPattern(
            List<FHR> fhrReadings,
            List<Contraction> contractions,
            FHR currentReading)
        {
            var result = new FHRAnalysisResult
            {
                DetectedDeceleration = "No",
                Confidence = 1.0,
                Reason = "No significant FHR changes detected"
            };

            if (!currentReading.Rate.HasValue || currentReading.Rate.Value <= 0)
            {
                result.Confidence = 0.0;
                result.Reason = "Invalid FHR reading";
                return result;
            }

            // Calculate baseline from recent readings (excluding current)
            var baseline = CalculateBaseline(fhrReadings);
            if (!baseline.HasValue)
            {
                // Use normal range average if no history
                baseline = 135; // Average of 110-160 range
            }

            var currentRate = currentReading.Rate.Value;
            var drop = baseline.Value - currentRate;

            // 1. Check for bradycardia (sustained low FHR)
            if (currentRate < FHR_BASELINE_MIN)
            {
                // Check if prolonged (compare with previous readings)
                if (IsProlongedPattern(fhrReadings, currentReading, FHR_BASELINE_MIN))
                {
                    result.DetectedDeceleration = "Prolonged";
                    result.Confidence = 0.9;
                    result.Reason = $"Sustained FHR below {FHR_BASELINE_MIN} bpm for >2 minutes. Baseline: {baseline} bpm, Current: {currentRate} bpm";
                    result.Severity = "Critical";
                    return result;
                }
            }

            // 2. Check for significant deceleration
            if (drop >= FHR_DECELERATION_THRESHOLD)
            {
                // Analyze pattern relative to contractions
                var contractionAnalysis = AnalyzeContractionTiming(
                    currentReading,
                    contractions,
                    fhrReadings
                );

                if (contractionAnalysis != null)
                {
                    result = contractionAnalysis;
                }
                else
                {
                    // No contraction data or unclear pattern
                    // Classify based on severity and duration
                    if (drop >= 30)
                    {
                        result.DetectedDeceleration = "Variable";
                        result.Confidence = 0.6;
                        result.Reason = $"Significant FHR drop ({drop} bpm) without clear contraction correlation. Baseline: {baseline} bpm, Current: {currentRate} bpm. Possible variable deceleration due to cord compression.";
                        result.Severity = "Warning";
                    }
                    else if (drop >= FHR_DECELERATION_THRESHOLD)
                    {
                        result.DetectedDeceleration = "Early";
                        result.Confidence = 0.5;
                        result.Reason = $"Moderate FHR drop ({drop} bpm). Baseline: {baseline} bpm, Current: {currentRate} bpm. Likely early deceleration (head compression) - monitor pattern.";
                        result.Severity = "Info";
                    }
                }
            }

            // 3. Check for gradual decline pattern (may indicate late decelerations)
            var trendAnalysis = AnalyzeFHRTrend(fhrReadings);
            if (trendAnalysis.IsGradualDecline && drop >= 10)
            {
                result.DetectedDeceleration = "Late";
                result.Confidence = 0.7;
                result.Reason = $"Gradual FHR decline pattern detected. Baseline: {baseline} bpm, Current: {currentRate} bpm, Trend: {trendAnalysis.TrendDescription}. Possible late deceleration - requires immediate evaluation.";
                result.Severity = "Critical";
            }

            return result;
        }

        /// <summary>
        /// Calculates FHR baseline from recent readings
        /// Uses last 3-5 normal readings, excluding current
        /// </summary>
        private int? CalculateBaseline(List<FHR> fhrReadings)
        {
            if (fhrReadings == null || !fhrReadings.Any())
                return null;

            // Get recent readings (last 5, excluding current if it's the last one)
            var recentReadings = fhrReadings
                .Where(f => f.Rate.HasValue && f.Rate.Value > 0)
                .OrderByDescending(f => f.Time)
                .Skip(1) // Skip current reading
                .Take(5)
                .Where(f => f.Rate.Value >= FHR_BASELINE_MIN && f.Rate.Value <= FHR_BASELINE_MAX)
                .ToList();

            if (!recentReadings.Any())
                return null;

            return (int)recentReadings.Average(f => f.Rate.Value);
        }

        /// <summary>
        /// Checks if FHR pattern indicates prolonged deceleration (>2 minutes)
        /// Since readings are every 30 min, checks if consecutive readings show sustained low FHR
        /// </summary>
        private bool IsProlongedPattern(List<FHR> fhrReadings, FHR currentReading, int threshold)
        {
            if (fhrReadings == null || !fhrReadings.Any())
                return false;

            var previousReading = fhrReadings
                .Where(f => f.Time < currentReading.Time && f.Rate.HasValue)
                .OrderByDescending(f => f.Time)
                .FirstOrDefault();

            if (previousReading == null)
                return false;

            // If both current and previous reading below threshold, consider prolonged
            return previousReading.Rate.Value < threshold &&
                   currentReading.Rate.Value < threshold;
        }

        /// <summary>
        /// Analyzes FHR timing relative to contractions
        /// Attempts to classify as Early, Late, or Variable based on temporal relationship
        /// </summary>
        private FHRAnalysisResult? AnalyzeContractionTiming(
            FHR currentReading,
            List<Contraction> contractions,
            List<FHR> fhrReadings)
        {
            if (contractions == null || !contractions.Any())
                return null;

            // Find contractions around the time of FHR reading (within 10 minutes)
            var timeWindow = TimeSpan.FromMinutes(10);
            var nearbyContractions = contractions
                .Where(c => Math.Abs((c.Time - currentReading.Time).TotalMinutes) <= timeWindow.TotalMinutes)
                .OrderBy(c => Math.Abs((c.Time - currentReading.Time).TotalSeconds))
                .ToList();

            if (!nearbyContractions.Any())
                return null;

            var closestContraction = nearbyContractions.First();
            var timeDiff = (currentReading.Time - closestContraction.Time).TotalMinutes;

            // Get previous FHR to assess recovery pattern
            var previousFHR = fhrReadings
                .Where(f => f.Time < currentReading.Time && f.Rate.HasValue)
                .OrderByDescending(f => f.Time)
                .FirstOrDefault();

            // Early deceleration: FHR drop occurs with/slightly before contraction onset
            // Timing: -2 to +2 minutes from contraction
            if (Math.Abs(timeDiff) <= 2)
            {
                return new FHRAnalysisResult
                {
                    DetectedDeceleration = "Early",
                    Confidence = 0.8,
                    Reason = $"FHR deceleration coincides with contraction timing (within 2 min). Likely early deceleration due to head compression during contraction. Monitor pattern.",
                    Severity = "Info"
                };
            }

            // Late deceleration: FHR drop occurs AFTER contraction peak
            // Timing: 2-5 minutes after contraction
            if (timeDiff > 2 && timeDiff <= 5)
            {
                return new FHRAnalysisResult
                {
                    DetectedDeceleration = "Late",
                    Confidence = 0.85,
                    Reason = $"FHR deceleration occurs {timeDiff:F1} min after contraction - pattern suggests LATE deceleration (uteroplacental insufficiency). CRITICAL: Immediate action required.",
                    Severity = "Critical"
                };
            }

            // Variable deceleration: Irregular timing relative to contractions
            // Can occur before, during, or after contractions
            if (Math.Abs(timeDiff) > 5 || nearbyContractions.Count > 1)
            {
                return new FHRAnalysisResult
                {
                    DetectedDeceleration = "Variable",
                    Confidence = 0.75,
                    Reason = $"FHR deceleration shows variable timing relative to contractions - suggests umbilical cord compression. Position change recommended.",
                    Severity = "Warning"
                };
            }

            return null;
        }

        /// <summary>
        /// Analyzes FHR trend over recent readings
        /// Detects gradual decline which may indicate late decelerations
        /// </summary>
        private (bool IsGradualDecline, string TrendDescription) AnalyzeFHRTrend(List<FHR> fhrReadings)
        {
            if (fhrReadings == null || fhrReadings.Count < 3)
                return (false, "Insufficient data");

            var recentReadings = fhrReadings
                .Where(f => f.Rate.HasValue && f.Rate.Value > 0)
                .OrderByDescending(f => f.Time)
                .Take(4)
                .OrderBy(f => f.Time)
                .ToList();

            if (recentReadings.Count < 3)
                return (false, "Insufficient readings");

            // Calculate trend: are values consistently declining?
            bool isDecl ining = true;
            int totalDrop = 0;

            for (int i = 1; i < recentReadings.Count; i++)
            {
                var diff = recentReadings[i].Rate.Value - recentReadings[i - 1].Rate.Value;
                totalDrop += diff;

                if (diff >= 0) // Not declining
                {
                    isDecl ining = false;
                    break;
                }
            }

            if (isDecl ining && totalDrop <= -10)
            {
                return (true, $"Gradual decline of {Math.Abs(totalDrop)} bpm over {recentReadings.Count} readings");
            }

            return (false, "No consistent decline pattern");
        }

        /// <summary>
        /// Provides clinical recommendations based on detected deceleration
        /// </summary>
        public List<string> GetRecommendations(FHRAnalysisResult analysis)
        {
            var recommendations = new List<string>();

            switch (analysis.DetectedDeceleration)
            {
                case "Late":
                    recommendations.Add("URGENT: Immediate action required");
                    recommendations.Add("Change maternal position (left lateral)");
                    recommendations.Add("Discontinue oxytocin if running");
                    recommendations.Add("Administer oxygen to mother (8-10 L/min)");
                    recommendations.Add("IV fluid bolus if hypotensive");
                    recommendations.Add("Check maternal blood pressure");
                    recommendations.Add("Notify obstetrician immediately");
                    recommendations.Add("Prepare for possible expedited delivery");
                    break;

                case "Prolonged":
                    recommendations.Add("URGENT: Immediate obstetric review required");
                    recommendations.Add("Check for cord prolapse if membranes ruptured");
                    recommendations.Add("Change maternal position");
                    recommendations.Add("Discontinue oxytocin if running");
                    recommendations.Add("Administer oxygen to mother");
                    recommendations.Add("Vaginal examination to check for cord prolapse");
                    recommendations.Add("Prepare for emergency delivery if deceleration persists");
                    break;

                case "Variable":
                    recommendations.Add("Change maternal position (left or right lateral)");
                    recommendations.Add("Monitor pattern - assess if persistent or worsening");
                    recommendations.Add("Discontinue oxytocin if running and decelerations severe");
                    recommendations.Add("Administer oxygen if decelerations deep or prolonged");
                    recommendations.Add("Notify obstetrician if pattern worsens or persists");
                    recommendations.Add("Consider amnioinfusion if available and appropriate");
                    break;

                case "Early":
                    recommendations.Add("Continue routine monitoring");
                    recommendations.Add("Early decelerations are usually benign");
                    recommendations.Add("Ensure decelerations are truly 'early' (mirror contractions)");
                    recommendations.Add("Monitor for any change in pattern");
                    break;

                default:
                    recommendations.Add("Continue routine monitoring");
                    recommendations.Add("Record FHR every 30 minutes");
                    break;
            }

            return recommendations;
        }
    }

    /// <summary>
    /// Result of FHR pattern analysis
    /// </summary>
    public class FHRAnalysisResult
    {
        /// <summary>
        /// Detected deceleration type: No, Early, Late, Variable, Prolonged
        /// </summary>
        public string DetectedDeceleration { get; set; } = "No";

        /// <summary>
        /// Confidence level (0.0 - 1.0) in the detection
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Clinical reasoning for the detection
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Severity: Info, Warning, Critical
        /// </summary>
        public string Severity { get; set; } = "Info";
    }
}
