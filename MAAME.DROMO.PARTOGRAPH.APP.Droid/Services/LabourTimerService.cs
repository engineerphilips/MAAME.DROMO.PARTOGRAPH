using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.Timers;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    /// <summary>
    /// Service for managing labour stage timers and reminders
    /// Implements WHO 2020 guidelines for monitoring intervals
    /// </summary>
    public class LabourTimerService : IDisposable
    {
        private readonly ILogger<LabourTimerService> _logger;
        private System.Timers.Timer? _monitoringTimer;
        private Partograph? _currentPartograph;
        private bool _disposed;

        // Timer events for UI updates
        public event EventHandler<TimerEventArgs>? OnApgar1Reminder;
        public event EventHandler<TimerEventArgs>? OnApgar5Reminder;
        public event EventHandler<TimerEventArgs>? OnApgar10Reminder;
        public event EventHandler<TimerEventArgs>? OnPlacentaWarning;
        public event EventHandler<TimerEventArgs>? OnPlacentaCritical;
        public event EventHandler<TimerEventArgs>? OnVitalSignsReminder;
        public event EventHandler<TimerEventArgs>? OnSecondStageWarning;
        public event EventHandler<TimerEventArgs>? OnFourthStageComplete;
        public event EventHandler<StageTimerUpdateEventArgs>? OnTimerUpdate;

        // Track which reminders have been fired
        private bool _apgar1Fired;
        private bool _apgar5Fired;
        private bool _apgar10Fired;
        private bool _placentaWarningFired;
        private bool _placentaCriticalFired;
        private DateTime _lastVitalSignsReminder;
        private bool _secondStageWarningFired;
        private bool _fourthStageCompleteFired;

        public LabourTimerService(ILogger<LabourTimerService> logger)
        {
            _logger = logger;
            _lastVitalSignsReminder = DateTime.MinValue;
        }

        /// <summary>
        /// Starts monitoring timers for the given partograph
        /// </summary>
        public void StartMonitoring(Partograph partograph)
        {
            StopMonitoring();

            _currentPartograph = partograph;
            ResetReminderFlags();

            _monitoringTimer = new System.Timers.Timer(1000); // Check every second
            _monitoringTimer.Elapsed += OnTimerElapsed;
            _monitoringTimer.AutoReset = true;
            _monitoringTimer.Start();

            _logger.LogInformation($"Started labour timer monitoring for partograph {partograph.ID}");
        }

        /// <summary>
        /// Stops all monitoring timers
        /// </summary>
        public void StopMonitoring()
        {
            if (_monitoringTimer != null)
            {
                _monitoringTimer.Stop();
                _monitoringTimer.Elapsed -= OnTimerElapsed;
                _monitoringTimer.Dispose();
                _monitoringTimer = null;
            }

            _currentPartograph = null;
            _logger.LogInformation("Stopped labour timer monitoring");
        }

        /// <summary>
        /// Updates the current partograph reference
        /// </summary>
        public void UpdatePartograph(Partograph partograph)
        {
            _currentPartograph = partograph;
        }

        private void ResetReminderFlags()
        {
            _apgar1Fired = false;
            _apgar5Fired = false;
            _apgar10Fired = false;
            _placentaWarningFired = false;
            _placentaCriticalFired = false;
            _secondStageWarningFired = false;
            _fourthStageCompleteFired = false;
            _lastVitalSignsReminder = DateTime.MinValue;
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (_currentPartograph == null)
                return;

            try
            {
                var now = DateTime.Now;

                switch (_currentPartograph.Status)
                {
                    case LaborStatus.SecondStage:
                        CheckSecondStageTimers(now);
                        break;

                    case LaborStatus.ThirdStage:
                        CheckThirdStageTimers(now);
                        break;

                    case LaborStatus.FourthStage:
                        CheckFourthStageTimers(now);
                        break;
                }

                // Fire timer update event for UI
                FireTimerUpdate(now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in labour timer elapsed handler");
            }
        }

        private void CheckSecondStageTimers(DateTime now)
        {
            if (!_currentPartograph!.SecondStageStartTime.HasValue)
                return;

            var duration = now - _currentPartograph.SecondStageStartTime.Value;

            // Second stage warning at 2 hours (WHO guideline)
            if (!_secondStageWarningFired && duration.TotalHours >= 2)
            {
                _secondStageWarningFired = true;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OnSecondStageWarning?.Invoke(this, new TimerEventArgs
                    {
                        Message = "Second stage has exceeded 2 hours - Consider intervention",
                        Severity = AlertSeverity.Warning,
                        Duration = duration
                    });
                });
            }
        }

        private void CheckThirdStageTimers(DateTime now)
        {
            // Check APGAR reminders based on delivery time
            if (_currentPartograph!.DeliveryTime.HasValue)
            {
                var timeSinceDelivery = now - _currentPartograph.DeliveryTime.Value;

                // APGAR 1 minute reminder (at 50 seconds to give time to prepare)
                if (!_apgar1Fired && timeSinceDelivery.TotalSeconds >= 50 && timeSinceDelivery.TotalSeconds < 90)
                {
                    _apgar1Fired = true;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        OnApgar1Reminder?.Invoke(this, new TimerEventArgs
                        {
                            Message = "APGAR 1-minute score due NOW",
                            Severity = AlertSeverity.Info,
                            Duration = timeSinceDelivery
                        });
                    });
                }

                // APGAR 5 minute reminder (at 4:50)
                if (!_apgar5Fired && timeSinceDelivery.TotalMinutes >= 4.83 && timeSinceDelivery.TotalMinutes < 6)
                {
                    _apgar5Fired = true;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        OnApgar5Reminder?.Invoke(this, new TimerEventArgs
                        {
                            Message = "APGAR 5-minute score due NOW",
                            Severity = AlertSeverity.Info,
                            Duration = timeSinceDelivery
                        });
                    });
                }

                // APGAR 10 minute reminder (at 9:50)
                if (!_apgar10Fired && timeSinceDelivery.TotalMinutes >= 9.83 && timeSinceDelivery.TotalMinutes < 11)
                {
                    _apgar10Fired = true;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        OnApgar10Reminder?.Invoke(this, new TimerEventArgs
                        {
                            Message = "APGAR 10-minute score due (if indicated)",
                            Severity = AlertSeverity.Info,
                            Duration = timeSinceDelivery
                        });
                    });
                }
            }

            // Check placenta delivery timers based on third stage start
            if (_currentPartograph.ThirdStageStartTime.HasValue)
            {
                var thirdStageDuration = now - _currentPartograph.ThirdStageStartTime.Value;

                // Warning at 20 minutes
                if (!_placentaWarningFired && thirdStageDuration.TotalMinutes >= 20)
                {
                    _placentaWarningFired = true;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        OnPlacentaWarning?.Invoke(this, new TimerEventArgs
                        {
                            Message = "Third stage: 20 minutes elapsed - Monitor for retained placenta",
                            Severity = AlertSeverity.Warning,
                            Duration = thirdStageDuration
                        });
                    });
                }

                // Critical at 30 minutes
                if (!_placentaCriticalFired && thirdStageDuration.TotalMinutes >= 30)
                {
                    _placentaCriticalFired = true;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        OnPlacentaCritical?.Invoke(this, new TimerEventArgs
                        {
                            Message = "URGENT: Third stage exceeds 30 minutes - Consider manual removal",
                            Severity = AlertSeverity.Critical,
                            Duration = thirdStageDuration
                        });
                    });
                }
            }
        }

        private void CheckFourthStageTimers(DateTime now)
        {
            if (!_currentPartograph!.FourthStageStartTime.HasValue)
                return;

            var duration = now - _currentPartograph.FourthStageStartTime.Value;

            // Vital signs reminder every 15 minutes
            if ((now - _lastVitalSignsReminder).TotalMinutes >= 15)
            {
                _lastVitalSignsReminder = now;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OnVitalSignsReminder?.Invoke(this, new TimerEventArgs
                    {
                        Message = "Time to record vital signs (every 15 minutes in fourth stage)",
                        Severity = AlertSeverity.Info,
                        Duration = duration
                    });
                });
            }

            // Fourth stage complete at 2 hours
            if (!_fourthStageCompleteFired && duration.TotalHours >= 2)
            {
                _fourthStageCompleteFired = true;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OnFourthStageComplete?.Invoke(this, new TimerEventArgs
                    {
                        Message = "Fourth stage monitoring complete (2 hours) - Ready for discharge assessment",
                        Severity = AlertSeverity.Info,
                        Duration = duration
                    });
                });
            }
        }

        private void FireTimerUpdate(DateTime now)
        {
            if (_currentPartograph == null)
                return;

            var args = new StageTimerUpdateEventArgs
            {
                CurrentStage = _currentPartograph.Status,
                Now = now
            };

            switch (_currentPartograph.Status)
            {
                case LaborStatus.FirstStage:
                    if (_currentPartograph.LaborStartTime.HasValue)
                    {
                        args.StageDuration = now - _currentPartograph.LaborStartTime.Value;
                        args.DurationText = FormatDuration(args.StageDuration.Value);
                    }
                    break;

                case LaborStatus.SecondStage:
                    if (_currentPartograph.SecondStageStartTime.HasValue)
                    {
                        args.StageDuration = now - _currentPartograph.SecondStageStartTime.Value;
                        args.DurationText = FormatDuration(args.StageDuration.Value);
                        args.IsOverdue = args.StageDuration.Value.TotalHours >= 2;
                    }
                    break;

                case LaborStatus.ThirdStage:
                    if (_currentPartograph.ThirdStageStartTime.HasValue)
                    {
                        args.StageDuration = now - _currentPartograph.ThirdStageStartTime.Value;
                        args.DurationText = FormatDuration(args.StageDuration.Value);
                        args.IsOverdue = args.StageDuration.Value.TotalMinutes >= 30;

                        // Also calculate time since delivery for APGAR tracking
                        if (_currentPartograph.DeliveryTime.HasValue)
                        {
                            args.TimeSinceDelivery = now - _currentPartograph.DeliveryTime.Value;
                            args.TimeSinceDeliveryText = FormatDuration(args.TimeSinceDelivery.Value);
                        }
                    }
                    break;

                case LaborStatus.FourthStage:
                    if (_currentPartograph.FourthStageStartTime.HasValue)
                    {
                        args.StageDuration = now - _currentPartograph.FourthStageStartTime.Value;
                        args.DurationText = FormatDuration(args.StageDuration.Value);

                        // Calculate remaining time
                        var remaining = TimeSpan.FromHours(2) - args.StageDuration.Value;
                        if (remaining.TotalSeconds > 0)
                        {
                            args.RemainingTime = remaining;
                            args.RemainingTimeText = FormatDuration(remaining);
                        }
                        else
                        {
                            args.RemainingTime = TimeSpan.Zero;
                            args.RemainingTimeText = "Complete";
                        }
                    }
                    break;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnTimerUpdate?.Invoke(this, args);
            });
        }

        /// <summary>
        /// Formats a TimeSpan as a human-readable duration string
        /// </summary>
        public static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
            {
                return $"{(int)duration.TotalHours}h {duration.Minutes:D2}m {duration.Seconds:D2}s";
            }
            else if (duration.TotalMinutes >= 1)
            {
                return $"{(int)duration.TotalMinutes}m {duration.Seconds:D2}s";
            }
            else
            {
                return $"{duration.Seconds}s";
            }
        }

        /// <summary>
        /// Gets the current stage duration
        /// </summary>
        public TimeSpan? GetCurrentStageDuration()
        {
            if (_currentPartograph == null)
                return null;

            var now = DateTime.Now;

            return _currentPartograph.Status switch
            {
                LaborStatus.FirstStage => _currentPartograph.LaborStartTime.HasValue
                    ? now - _currentPartograph.LaborStartTime.Value
                    : null,
                LaborStatus.SecondStage => _currentPartograph.SecondStageStartTime.HasValue
                    ? now - _currentPartograph.SecondStageStartTime.Value
                    : null,
                LaborStatus.ThirdStage => _currentPartograph.ThirdStageStartTime.HasValue
                    ? now - _currentPartograph.ThirdStageStartTime.Value
                    : null,
                LaborStatus.FourthStage => _currentPartograph.FourthStageStartTime.HasValue
                    ? now - _currentPartograph.FourthStageStartTime.Value
                    : null,
                _ => null
            };
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopMonitoring();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Event arguments for timer alerts
    /// </summary>
    public class TimerEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// Event arguments for stage timer updates
    /// </summary>
    public class StageTimerUpdateEventArgs : EventArgs
    {
        public LaborStatus CurrentStage { get; set; }
        public DateTime Now { get; set; }
        public TimeSpan? StageDuration { get; set; }
        public string DurationText { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
        public TimeSpan? TimeSinceDelivery { get; set; }
        public string TimeSinceDeliveryText { get; set; } = string.Empty;
        public TimeSpan? RemainingTime { get; set; }
        public string RemainingTimeText { get; set; } = string.Empty;
    }

    /// <summary>
    /// Alert severity levels
    /// </summary>
    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical
    }
}
