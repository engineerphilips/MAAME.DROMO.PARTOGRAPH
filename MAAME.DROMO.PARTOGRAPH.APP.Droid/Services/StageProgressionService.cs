using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    /// <summary>
    /// Handles automatic and manual stage progression for WHO Four-Stage Labor System
    /// Reference: WHO recommendations for intrapartum care (2020)
    /// </summary>
    public class StageProgressionService
    {
        private readonly PartographRepository _partographRepository;
        private readonly ILogger<StageProgressionService> _logger;

        public StageProgressionService(
            PartographRepository partographRepository,
            ILogger<StageProgressionService> logger)
        {
            _partographRepository = partographRepository;
            _logger = logger;
        }

        /// <summary>
        /// Checks if partograph should automatically progress to the next stage based on measurements
        /// </summary>
        public (bool shouldProgress, LaborStatus nextStage, string reason) CheckAutomaticProgression(Partograph partograph)
        {
            if (partograph == null)
                return (false, partograph?.Status ?? LaborStatus.Pending, "No partograph provided");

            switch (partograph.Status)
            {
                case LaborStatus.Pending:
                    return CheckPendingProgression(partograph);

                case LaborStatus.FirstStage:
                    return CheckFirstStageProgression(partograph);

                case LaborStatus.SecondStage:
                    return CheckSecondStageProgression(partograph);

                case LaborStatus.ThirdStage:
                    return CheckThirdStageProgression(partograph);

                case LaborStatus.FourthStage:
                    return CheckFourthStageProgression(partograph);

                default:
                    return (false, partograph.Status, "No automatic progression for this stage");
            }
        }

        /// <summary>
        /// Checks if labour should start (Pending → FirstStage)
        /// Auto-progression when: contractions recorded or first cervical dilation recorded
        /// </summary>
        private (bool shouldProgress, LaborStatus nextStage, string reason) CheckPendingProgression(Partograph partograph)
        {
            // Check if contractions have been recorded
            var hasContractions = partograph.Contractions?.Any() == true;

            // Check if cervical dilation has been recorded and is >= 4cm (active labour)
            var latestDilation = partograph.Dilatations
                ?.OrderByDescending(d => d.Time)
                .FirstOrDefault();

            if (latestDilation != null && latestDilation.DilatationCm >= 4)
            {
                return (true, LaborStatus.FirstStage,
                    $"Active labour detected: cervical dilation at {latestDilation.DilatationCm}cm");
            }

            if (hasContractions && latestDilation != null)
            {
                return (true, LaborStatus.FirstStage,
                    "Labour indicators present: contractions and cervical dilation recorded");
            }

            return (false, LaborStatus.Pending, "No labour indicators detected yet");
        }

        private (bool shouldProgress, LaborStatus nextStage, string reason) CheckFirstStageProgression(Partograph partograph)
        {
            // Check if dilation has reached 10cm (full dilation)
            var latestDilation = partograph.Dilatations
                ?.OrderByDescending(d => d.Time)
                .FirstOrDefault();

            if (latestDilation != null && latestDilation.DilatationCm >= 10)
            {
                return (true, LaborStatus.SecondStage,
                    $"Full cervical dilation achieved (10cm) at {latestDilation.Time:HH:mm}");
            }

            // Also update the current phase
            partograph.UpdatePhaseFromDilation();

            var currentPhase = partograph.CurrentPhase;
            string phaseInfo = currentPhase switch
            {
                FirstStagePhase.Latent => "Latent phase (0-4cm)",
                FirstStagePhase.ActiveEarly => "Active phase - Early (5-7cm)",
                FirstStagePhase.ActiveAdvanced => "Active phase - Advanced (8-9cm)",
                _ => "Phase not determined"
            };

            return (false, LaborStatus.FirstStage, $"Current: {latestDilation?.DilatationCm ?? 0}cm - {phaseInfo}");
        }

        /// <summary>
        /// Checks if second stage should progress to third stage (SecondStage → ThirdStage)
        /// Auto-progression when: baby delivery time is recorded
        /// </summary>
        private (bool shouldProgress, LaborStatus nextStage, string reason) CheckSecondStageProgression(Partograph partograph)
        {
            // Check if baby has been delivered (DeliveryTime is set)
            if (partograph.DeliveryTime.HasValue)
            {
                return (true, LaborStatus.ThirdStage,
                    $"Baby delivered at {partograph.DeliveryTime.Value:HH:mm} - ready to monitor placenta delivery");
            }

            // Calculate time in second stage
            if (partograph.SecondStageStartTime.HasValue)
            {
                var timeInSecondStage = DateTime.Now - partograph.SecondStageStartTime.Value;

                // Warning if second stage exceeds 2 hours (for primigravida) or 1 hour (for multigravida)
                if (timeInSecondStage.TotalHours >= 2)
                {
                    return (false, LaborStatus.SecondStage,
                        $"⚠️ Second stage duration: {timeInSecondStage.Hours}h {timeInSecondStage.Minutes}m - Consider intervention if no progress");
                }

                return (false, LaborStatus.SecondStage,
                    $"Second stage duration: {timeInSecondStage.Hours}h {timeInSecondStage.Minutes}m - Awaiting baby delivery");
            }

            return (false, LaborStatus.SecondStage, "Monitoring second stage - awaiting baby delivery");
        }

        /// <summary>
        /// Checks if third stage should progress to fourth stage (ThirdStage → FourthStage)
        /// Auto-progression when: placenta delivery time is recorded or 30+ minutes have passed
        /// </summary>
        private (bool shouldProgress, LaborStatus nextStage, string reason) CheckThirdStageProgression(Partograph partograph)
        {
            // Check if placenta has been delivered (FourthStageStartTime being set indicates placenta delivered)
            // or check for ThirdStageStartTime + reasonable time

            if (!partograph.ThirdStageStartTime.HasValue)
            {
                // If DeliveryTime is set, use that as the start of third stage
                if (partograph.DeliveryTime.HasValue)
                {
                    partograph.ThirdStageStartTime = partograph.DeliveryTime;
                }
                else
                {
                    return (false, LaborStatus.ThirdStage, "Third stage start time not recorded");
                }
            }

            var timeInThirdStage = DateTime.Now - partograph.ThirdStageStartTime.Value;

            // Warning if third stage exceeds 30 minutes (retained placenta risk)
            if (timeInThirdStage.TotalMinutes >= 30)
            {
                return (false, LaborStatus.ThirdStage,
                    $"⚠️ Third stage duration: {(int)timeInThirdStage.TotalMinutes} minutes - Consider manual removal if placenta not delivered");
            }

            return (false, LaborStatus.ThirdStage,
                $"Third stage duration: {(int)timeInThirdStage.TotalMinutes} minutes - Monitoring placenta delivery");
        }

        private (bool shouldProgress, LaborStatus nextStage, string reason) CheckFourthStageProgression(Partograph partograph)
        {
            // Check if 2 hours have passed since fourth stage started
            if (!partograph.FourthStageStartTime.HasValue)
                return (false, LaborStatus.FourthStage, "Fourth stage start time not recorded");

            var timeInFourthStage = DateTime.UtcNow - partograph.FourthStageStartTime.Value;
            if (timeInFourthStage.TotalHours >= 2)
            {
                return (true, LaborStatus.Completed,
                    $"2 hours postpartum monitoring completed (started at {partograph.FourthStageStartTime:HH:mm})");
            }

            var remainingMinutes = (int)(120 - timeInFourthStage.TotalMinutes);
            return (false, LaborStatus.FourthStage,
                $"Still monitoring - {remainingMinutes} minutes remaining");
        }

        /// <summary>
        /// Manually progress to the next stage with confirmation
        /// </summary>
        public async Task<(bool success, string message)> ProgressToNextStage(
            Partograph partograph,
            LaborStatus targetStage,
            bool skipValidation = false)
        {
            if (partograph == null)
                return (false, "No partograph provided");

            // Validate stage progression sequence
            if (!skipValidation && !IsValidProgression(partograph.Status, targetStage))
            {
                return (false, $"Invalid progression from {partograph.Status} to {targetStage}");
            }

            var oldStage = partograph.Status;

            try
            {
                // Set the appropriate timestamp and status
                switch (targetStage)
                {
                    case LaborStatus.FirstStage:
                        partograph.Status = LaborStatus.FirstStage;
                        partograph.LaborStartTime ??= DateTime.UtcNow;
                        break;

                    case LaborStatus.SecondStage:
                        partograph.Status = LaborStatus.SecondStage;
                        partograph.SecondStageStartTime = DateTime.UtcNow;
                        _logger.LogInformation($"Second stage started for partograph {partograph.ID}");
                        break;

                    case LaborStatus.ThirdStage:
                        partograph.Status = LaborStatus.ThirdStage;
                        partograph.ThirdStageStartTime = DateTime.UtcNow;
                        partograph.DeliveryTime = DateTime.UtcNow; // Baby delivered
                        _logger.LogInformation($"Third stage started (baby delivered) for partograph {partograph.ID}");
                        break;

                    case LaborStatus.FourthStage:
                        partograph.Status = LaborStatus.FourthStage;
                        partograph.FourthStageStartTime = DateTime.UtcNow;
                        _logger.LogInformation($"Fourth stage started (placenta delivered) for partograph {partograph.ID}");
                        break;

                    case LaborStatus.Completed:
                        partograph.Status = LaborStatus.Completed;
                        partograph.CompletedTime = DateTime.UtcNow;
                        _logger.LogInformation($"Delivery care completed for partograph {partograph.ID}");
                        break;

                    case LaborStatus.Emergency:
                        partograph.Status = LaborStatus.Emergency;
                        _logger.LogWarning($"Partograph {partograph.ID} marked as emergency from stage {oldStage}");
                        break;

                    default:
                        return (false, $"Unsupported target stage: {targetStage}");
                }

                // Save to database
                await _partographRepository.SaveItemAsync(partograph);

                return (true, $"Successfully progressed from {oldStage} to {targetStage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error progressing partograph {partograph.ID} from {oldStage} to {targetStage}");
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates if progression from current stage to target stage is allowed
        /// </summary>
        private bool IsValidProgression(LaborStatus currentStage, LaborStatus targetStage)
        {
            // Emergency can be triggered from any stage
            if (targetStage == LaborStatus.Emergency)
                return true;

            // Valid progression paths
            return (currentStage, targetStage) switch
            {
                (LaborStatus.Pending, LaborStatus.FirstStage) => true,
                (LaborStatus.FirstStage, LaborStatus.SecondStage) => true,
                (LaborStatus.SecondStage, LaborStatus.ThirdStage) => true,
                (LaborStatus.ThirdStage, LaborStatus.FourthStage) => true,
                (LaborStatus.FourthStage, LaborStatus.Completed) => true,
                (LaborStatus.Emergency, _) => true, // Allow recovery from emergency to any stage
                _ => false
            };
        }

        /// <summary>
        /// Gets suggested actions for current stage
        /// </summary>
        public List<string> GetStageSuggestions(Partograph partograph)
        {
            if (partograph == null)
                return new List<string>();

            var suggestions = new List<string>();

            switch (partograph.Status)
            {
                case LaborStatus.Pending:
                    var (pendingShouldProgress, _, pendingReason) = CheckPendingProgression(partograph);
                    if (pendingShouldProgress)
                    {
                        suggestions.Add($"✓ Ready to start labour: {pendingReason}");
                        suggestions.Add("Click 'Start Labour' to begin first stage monitoring");
                    }
                    else
                    {
                        suggestions.Add("Monitor for signs of labour onset");
                        suggestions.Add("Record cervical dilation when contractions become regular");
                        suggestions.Add("Ensure initial assessment is complete");
                    }
                    break;

                case LaborStatus.FirstStage:
                    var (shouldProgress, _, reason) = CheckFirstStageProgression(partograph);
                    if (shouldProgress)
                    {
                        suggestions.Add($"✓ Ready to progress to Second Stage: {reason}");
                    }
                    else
                    {
                        suggestions.Add($"Monitor cervical dilation (current: {reason})");
                        suggestions.Add("Check FHR every 30 minutes");
                        suggestions.Add("Monitor contractions");
                    }
                    break;

                case LaborStatus.SecondStage:
                    var (secondShouldProgress, _, secondReason) = CheckSecondStageProgression(partograph);
                    if (secondShouldProgress)
                    {
                        suggestions.Add($"✓ Ready for Third Stage: {secondReason}");
                        suggestions.Add("Click 'Baby Delivered' to record delivery and proceed");
                    }
                    else
                    {
                        suggestions.Add("Monitor pushing efforts");
                        suggestions.Add("Check FHR every 5 minutes");
                        suggestions.Add($"Status: {secondReason}");
                    }
                    break;

                case LaborStatus.ThirdStage:
                    var (thirdShouldProgress, _, thirdReason) = CheckThirdStageProgression(partograph);
                    suggestions.Add($"Status: {thirdReason}");
                    suggestions.Add("Monitor for placenta delivery (up to 30 minutes)");
                    suggestions.Add("Watch for signs of excessive bleeding");
                    suggestions.Add("Record baby details and birth outcome");
                    suggestions.Add("Click 'Placenta Delivered' to proceed to Fourth Stage");
                    break;

                case LaborStatus.FourthStage:
                    var (canComplete, _, completionReason) = CheckFourthStageProgression(partograph);
                    if (canComplete)
                    {
                        suggestions.Add($"✓ Ready to complete: {completionReason}");
                    }
                    else
                    {
                        suggestions.Add($"Continue monitoring: {completionReason}");
                    }
                    suggestions.Add("Monitor vital signs every 15 minutes");
                    suggestions.Add("Watch for postpartum hemorrhage");
                    suggestions.Add("Ensure mother-baby bonding");
                    break;

                case LaborStatus.Completed:
                    suggestions.Add("✓ Delivery care completed");
                    suggestions.Add("Record final observations");
                    suggestions.Add("Plan for postnatal care");
                    break;

                case LaborStatus.Emergency:
                    suggestions.Add("⚠️ EMERGENCY - Immediate medical attention required");
                    suggestions.Add("Notify senior staff");
                    suggestions.Add("Prepare for potential intervention");
                    break;
            }

            return suggestions;
        }

        /// <summary>
        /// Gets the expected duration for the current stage
        /// </summary>
        public (TimeSpan? expectedDuration, TimeSpan? currentDuration, bool isOverdue) GetStageDuration(Partograph partograph)
        {
            if (partograph == null)
                return (null, null, false);

            DateTime? stageStart = partograph.Status switch
            {
                LaborStatus.FirstStage => partograph.LaborStartTime,
                LaborStatus.SecondStage => partograph.SecondStageStartTime,
                LaborStatus.ThirdStage => partograph.ThirdStageStartTime,
                LaborStatus.FourthStage => partograph.FourthStageStartTime,
                _ => null
            };

            if (!stageStart.HasValue)
                return (null, null, false);

            var currentDuration = DateTime.UtcNow - stageStart.Value;

            // WHO guidelines for stage durations
            TimeSpan? expectedDuration = partograph.Status switch
            {
                LaborStatus.FirstStage => TimeSpan.FromHours(12), // Variable, but typically 8-12 hours for first-time mothers
                LaborStatus.SecondStage => TimeSpan.FromHours(2), // Up to 2 hours (3 hours with epidural)
                LaborStatus.ThirdStage => TimeSpan.FromMinutes(30), // Up to 30 minutes
                LaborStatus.FourthStage => TimeSpan.FromHours(2), // Fixed at 2 hours
                _ => null
            };

            bool isOverdue = expectedDuration.HasValue && currentDuration > expectedDuration.Value;

            return (expectedDuration, currentDuration, isOverdue);
        }
    }
}
