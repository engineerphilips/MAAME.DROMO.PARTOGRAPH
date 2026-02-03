using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    /// <summary>
    /// Represents a measurement due alert for active partograph monitoring
    /// </summary>
    public class MeasurementDueAlert
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PartographId { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MeasurementType { get; set; } = string.Empty;
        public DateTime DueTime { get; set; }
        public DateTime? LastMeasurementTime { get; set; }
        public int MinutesOverdue { get; set; }
        public bool IsOverdue => MinutesOverdue > 0;
        public bool IsDue => MinutesOverdue >= -5; // Due within 5 minutes
        public AlertSeverity Severity { get; set; } = AlertSeverity.Info;
        public string Message { get; set; } = string.Empty;
        public bool IsAcknowledged { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // UI Properties
        public string SeverityIcon => Severity switch
        {
            AlertSeverity.Critical => "🚨",
            AlertSeverity.Warning => "⚠️",
            AlertSeverity.Info => "ℹ️",
            _ => "🔔"
        };

        public string SeverityColor => Severity switch
        {
            AlertSeverity.Critical => "#EF5350",  // Red
            AlertSeverity.Warning => "#FF9800",   // Orange
            AlertSeverity.Info => "#2196F3",      // Blue
            _ => "#9E9E9E"
        };

        public string TimeDisplay
        {
            get
            {
                if (MinutesOverdue > 60)
                    return $"{MinutesOverdue / 60}h {MinutesOverdue % 60}m overdue";
                if (MinutesOverdue > 0)
                    return $"{MinutesOverdue}m overdue";
                if (MinutesOverdue == 0)
                    return "Due now";
                return $"Due in {Math.Abs(MinutesOverdue)}m";
            }
        }

        public string MeasurementIcon => MeasurementType switch
        {
            "FHR" or "Fetal Heart Rate" => "💓",
            "Contractions" => "📊",
            "Cervical Dilatation" or "Vaginal Examination" => "📏",
            "Blood Pressure" or "BP" => "🩺",
            "Temperature" => "🌡️",
            "Urine" => "💧",
            _ => "📋"
        };
    }

    /// <summary>
    /// Represents a combined notification item (either a clinical alert or measurement due alert)
    /// </summary>
    public class NotificationItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public Guid? PartographId { get; set; }
        public Guid? PatientId { get; set; }
        public AlertSeverity Severity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsAcknowledged { get; set; }
        public ClinicalAlert? ClinicalAlert { get; set; }
        public MeasurementDueAlert? MeasurementDueAlert { get; set; }

        // UI Properties
        public string SeverityIcon => Severity switch
        {
            AlertSeverity.Critical => "🚨",
            AlertSeverity.Warning => "⚠️",
            AlertSeverity.Info => "ℹ️",
            _ => "🔔"
        };

        public string SeverityColor => Severity switch
        {
            AlertSeverity.Critical => "#EF5350",
            AlertSeverity.Warning => "#FF9800",
            AlertSeverity.Info => "#2196F3",
            _ => "#9E9E9E"
        };

        public string TypeIcon => Type switch
        {
            NotificationType.ClinicalAlert => "🏥",
            NotificationType.MeasurementDue => "⏰",
            _ => "📋"
        };

        public string TimeAgo
        {
            get
            {
                var elapsed = DateTime.Now - CreatedAt;
                if (elapsed.TotalMinutes < 1) return "Just now";
                if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes}m ago";
                if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours}h ago";
                return $"{(int)elapsed.TotalDays}d ago";
            }
        }
    }

    public enum NotificationType
    {
        ClinicalAlert,
        MeasurementDue
    }

    /// <summary>
    /// Service that actively monitors all active partographs and generates alerts when measurements are due.
    /// Implements WHO Labour Care Guide 2020 monitoring intervals.
    /// </summary>
    public class PartographMonitoringService : IDisposable
    {
        private readonly PartographRepository _partographRepository;
        private readonly AlertEngine _alertEngine;
        private readonly ILogger<PartographMonitoringService>? _logger;
        private readonly AppShellModel? _appShellModel;

        private Timer? _monitoringTimer;
        private bool _isRunning;
        private readonly object _lock = new();

        // Observable collections for notifications
        private readonly ObservableCollection<NotificationItem> _notifications = new();
        private readonly ObservableCollection<MeasurementDueAlert> _measurementDueAlerts = new();
        private readonly List<ClinicalAlert> _clinicalAlerts = new();

        // Events for notification updates
        public event EventHandler<NotificationItem>? NotificationAdded;
        public event EventHandler<int>? NotificationCountChanged;
        public event EventHandler<MeasurementDueAlert>? MeasurementDue;

        // WHO 2020 Monitoring Intervals (in minutes)
        private const int FHR_INTERVAL_FIRST_STAGE = 30;
        private const int FHR_INTERVAL_SECOND_STAGE = 15;
        private const int CONTRACTION_INTERVAL = 30;
        private const int BP_INTERVAL = 240; // 4 hours
        private const int TEMPERATURE_INTERVAL = 240; // 4 hours
        private const int VAGINAL_EXAM_INTERVAL = 240; // 4 hours
        private const int URINE_INTERVAL = 240; // 4 hours

        // Monitoring timer interval (check every minute)
        private const int MONITORING_INTERVAL_MS = 60000; // 1 minute

        public IReadOnlyCollection<NotificationItem> Notifications => _notifications;
        public IReadOnlyCollection<MeasurementDueAlert> MeasurementDueAlerts => _measurementDueAlerts;
        public IReadOnlyCollection<ClinicalAlert> ClinicalAlerts => _clinicalAlerts;

        public int TotalNotificationCount => _notifications.Count(n => !n.IsAcknowledged);
        public int CriticalCount => _notifications.Count(n => !n.IsAcknowledged && n.Severity == AlertSeverity.Critical);
        public int WarningCount => _notifications.Count(n => !n.IsAcknowledged && n.Severity == AlertSeverity.Warning);

        public PartographMonitoringService(
            PartographRepository partographRepository,
            ILogger<PartographMonitoringService>? logger = null)
        {
            _partographRepository = partographRepository;
            _alertEngine = new AlertEngine();
            _logger = logger;

            // Try to get AppShellModel from DI
            _appShellModel = IPlatformApplication.Current?.Services.GetService<AppShellModel>();

            // Subscribe to AlertEngine events
            _alertEngine.AlertTriggered += OnAlertTriggered;
            _alertEngine.AlertCleared += OnAlertCleared;
        }

        /// <summary>
        /// Starts the background monitoring service
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                if (_isRunning) return;

                _monitoringTimer = new Timer(
                    async _ => await MonitorPartographsAsync(),
                    null,
                    TimeSpan.Zero, // Start immediately
                    TimeSpan.FromMilliseconds(MONITORING_INTERVAL_MS));

                _isRunning = true;
                _logger?.LogInformation("Partograph monitoring service started");
            }
        }

        /// <summary>
        /// Stops the background monitoring service
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                _monitoringTimer?.Dispose();
                _monitoringTimer = null;
                _isRunning = false;
                _logger?.LogInformation("Partograph monitoring service stopped");
            }
        }

        /// <summary>
        /// Manually triggers a monitoring check
        /// </summary>
        public async Task RefreshAsync()
        {
            await MonitorPartographsAsync();
        }

        /// <summary>
        /// Main monitoring loop - checks all active partographs for due measurements
        /// </summary>
        private async Task MonitorPartographsAsync()
        {
            try
            {
                // Get facility ID based on user role
                Guid? facilityId = null;
                if (!Constants.IsSuperOrAdmin())
                {
                    facilityId = Constants.Staff?.FacilityId;
                }
                else if (Constants.SelectedFacility != null)
                {
                    facilityId = Constants.SelectedFacility.Id;
                }

                // Get all active partographs (FirstStage, SecondStage, ThirdStage)
                var activePartographs = await _partographRepository.ListAsync();
                activePartographs = activePartographs
                    .Where(p => p.Status == LaborStatus.FirstStage ||
                                p.Status == LaborStatus.SecondStage ||
                                p.Status == LaborStatus.ThirdStage)
                    .Where(p => facilityId == null || p.FacilityId == facilityId)
                    .ToList();

                if (!activePartographs.Any())
                {
                    // Clear notifications if no active partographs
                    ClearAllNotifications();
                    return;
                }

                var newMeasurementAlerts = new List<MeasurementDueAlert>();
                var newClinicalAlerts = new List<ClinicalAlert>();

                foreach (var partograph in activePartographs)
                {
                    // Check for due measurements
                    var dueAlerts = CheckMeasurementsDue(partograph);
                    newMeasurementAlerts.AddRange(dueAlerts);

                    // Analyze clinical alerts
                    var clinicalAlerts = _alertEngine.AnalyzePatient(partograph);
                    newClinicalAlerts.AddRange(clinicalAlerts);
                }

                // Update notifications on main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateNotifications(newMeasurementAlerts, newClinicalAlerts);
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error monitoring partographs");
            }
        }

        /// <summary>
        /// Checks which measurements are due for a partograph based on WHO 2020 intervals
        /// </summary>
        private List<MeasurementDueAlert> CheckMeasurementsDue(Partograph partograph)
        {
            var alerts = new List<MeasurementDueAlert>();
            var now = DateTime.Now;

            // FHR monitoring (every 30min in first stage, every 15min in second stage)
            var fhrInterval = partograph.Status == LaborStatus.SecondStage
                ? FHR_INTERVAL_SECOND_STAGE
                : FHR_INTERVAL_FIRST_STAGE;
            var lastFhr = partograph.Fhrs?.OrderByDescending(f => f.Time).FirstOrDefault();
            if (lastFhr != null)
            {
                var fhrDueTime = lastFhr.Time.AddMinutes(fhrInterval);
                var minutesOverdue = (int)(now - fhrDueTime).TotalMinutes;
                if (minutesOverdue >= -5) // Due within 5 minutes or overdue
                {
                    alerts.Add(CreateMeasurementDueAlert(
                        partograph, "Fetal Heart Rate",
                        lastFhr.Time, fhrDueTime, minutesOverdue,
                        $"FHR measurement is {(minutesOverdue > 0 ? "overdue" : "due")} for {partograph.PatientName}"));
                }
            }
            else if (partograph.LaborStartTime.HasValue)
            {
                // No FHR recorded yet - alert immediately
                alerts.Add(CreateMeasurementDueAlert(
                    partograph, "Fetal Heart Rate",
                    null, partograph.LaborStartTime.Value, 30,
                    $"Initial FHR measurement needed for {partograph.PatientName}"));
            }

            // Contraction monitoring (every 30 minutes)
            var lastContraction = partograph.Contractions?.OrderByDescending(c => c.Time).FirstOrDefault();
            if (lastContraction != null)
            {
                var contractionDueTime = lastContraction.Time.AddMinutes(CONTRACTION_INTERVAL);
                var minutesOverdue = (int)(now - contractionDueTime).TotalMinutes;
                if (minutesOverdue >= -5)
                {
                    alerts.Add(CreateMeasurementDueAlert(
                        partograph, "Contractions",
                        lastContraction.Time, contractionDueTime, minutesOverdue,
                        $"Contraction assessment is {(minutesOverdue > 0 ? "overdue" : "due")} for {partograph.PatientName}"));
                }
            }

            // Cervical Dilatation / Vaginal Examination (every 4 hours)
            var lastDilatation = partograph.Dilatations?.OrderByDescending(d => d.Time).FirstOrDefault();
            if (lastDilatation != null)
            {
                var dilatationDueTime = lastDilatation.Time.AddMinutes(VAGINAL_EXAM_INTERVAL);
                var minutesOverdue = (int)(now - dilatationDueTime).TotalMinutes;
                if (minutesOverdue >= -10) // Due within 10 minutes
                {
                    alerts.Add(CreateMeasurementDueAlert(
                        partograph, "Cervical Dilatation",
                        lastDilatation.Time, dilatationDueTime, minutesOverdue,
                        $"Vaginal examination is {(minutesOverdue > 0 ? "overdue" : "due")} for {partograph.PatientName}"));
                }
            }

            // Blood Pressure (every 4 hours)
            var lastBp = partograph.BPs?.OrderByDescending(b => b.Time).FirstOrDefault();
            if (lastBp != null)
            {
                var bpDueTime = lastBp.Time.AddMinutes(BP_INTERVAL);
                var minutesOverdue = (int)(now - bpDueTime).TotalMinutes;
                if (minutesOverdue >= -10)
                {
                    alerts.Add(CreateMeasurementDueAlert(
                        partograph, "Blood Pressure",
                        lastBp.Time, bpDueTime, minutesOverdue,
                        $"BP measurement is {(minutesOverdue > 0 ? "overdue" : "due")} for {partograph.PatientName}"));
                }
            }

            // Temperature (every 4 hours)
            var lastTemp = partograph.Temperatures?.OrderByDescending(t => t.Time).FirstOrDefault();
            if (lastTemp != null)
            {
                var tempDueTime = lastTemp.Time.AddMinutes(TEMPERATURE_INTERVAL);
                var minutesOverdue = (int)(now - tempDueTime).TotalMinutes;
                if (minutesOverdue >= -10)
                {
                    alerts.Add(CreateMeasurementDueAlert(
                        partograph, "Temperature",
                        lastTemp.Time, tempDueTime, minutesOverdue,
                        $"Temperature measurement is {(minutesOverdue > 0 ? "overdue" : "due")} for {partograph.PatientName}"));
                }
            }

            // Urine (every 4 hours)
            var lastUrine = partograph.Urines?.OrderByDescending(u => u.Time).FirstOrDefault();
            if (lastUrine != null)
            {
                var urineDueTime = lastUrine.Time.AddMinutes(URINE_INTERVAL);
                var minutesOverdue = (int)(now - urineDueTime).TotalMinutes;
                if (minutesOverdue >= -10)
                {
                    alerts.Add(CreateMeasurementDueAlert(
                        partograph, "Urine",
                        lastUrine.Time, urineDueTime, minutesOverdue,
                        $"Urine analysis is {(minutesOverdue > 0 ? "overdue" : "due")} for {partograph.PatientName}"));
                }
            }

            return alerts;
        }

        private MeasurementDueAlert CreateMeasurementDueAlert(
            Partograph partograph,
            string measurementType,
            DateTime? lastMeasurementTime,
            DateTime dueTime,
            int minutesOverdue,
            string message)
        {
            AlertSeverity severity;
            if (minutesOverdue > 60) severity = AlertSeverity.Critical;
            else if (minutesOverdue > 0) severity = AlertSeverity.Warning;
            else severity = AlertSeverity.Info;

            return new MeasurementDueAlert
            {
                PartographId = partograph.Id,
                PatientId = partograph.PatientId,
                PatientName = partograph.PatientName ?? "Unknown Patient",
                MeasurementType = measurementType,
                DueTime = dueTime,
                LastMeasurementTime = lastMeasurementTime,
                MinutesOverdue = minutesOverdue,
                Severity = severity,
                Message = message
            };
        }

        /// <summary>
        /// Updates the notification collections and triggers events
        /// </summary>
        private void UpdateNotifications(List<MeasurementDueAlert> measurementAlerts, List<ClinicalAlert> clinicalAlerts)
        {
            // Track new alerts for toast notifications
            var newMeasurementAlerts = measurementAlerts
                .Where(ma => !_measurementDueAlerts.Any(existing =>
                    existing.PartographId == ma.PartographId &&
                    existing.MeasurementType == ma.MeasurementType))
                .ToList();

            var newClinicalAlerts = clinicalAlerts
                .Where(ca => !_clinicalAlerts.Any(existing =>
                    existing.Title == ca.Title &&
                    existing.MeasurementType == ca.MeasurementType))
                .ToList();

            // Update measurement alerts
            _measurementDueAlerts.Clear();
            foreach (var alert in measurementAlerts.OrderByDescending(a => a.MinutesOverdue))
            {
                _measurementDueAlerts.Add(alert);
            }

            // Update clinical alerts
            _clinicalAlerts.Clear();
            _clinicalAlerts.AddRange(clinicalAlerts);

            // Rebuild notifications list
            RebuildNotificationsList();

            // Update AppShell badge
            _appShellModel?.UpdateNotificationCount(TotalNotificationCount);
            NotificationCountChanged?.Invoke(this, TotalNotificationCount);

            // Trigger toast notifications for new critical/warning alerts
            foreach (var alert in newMeasurementAlerts.Where(a => a.Severity != AlertSeverity.Info))
            {
                MeasurementDue?.Invoke(this, alert);
                ShowToastNotificationAsync(alert.Message, alert.Severity);
            }

            foreach (var alert in newClinicalAlerts.Where(a => a.Severity == AlertSeverity.Critical))
            {
                var notification = new NotificationItem
                {
                    Type = NotificationType.ClinicalAlert,
                    Title = alert.Title,
                    Message = alert.Message,
                    Severity = alert.Severity,
                    ClinicalAlert = alert
                };
                NotificationAdded?.Invoke(this, notification);
                ShowToastNotificationAsync($"🚨 {alert.Title}", alert.Severity);
            }
        }

        private void RebuildNotificationsList()
        {
            _notifications.Clear();

            // Add measurement due alerts as notifications
            foreach (var alert in _measurementDueAlerts)
            {
                _notifications.Add(new NotificationItem
                {
                    Id = alert.Id,
                    Type = NotificationType.MeasurementDue,
                    Title = $"{alert.MeasurementIcon} {alert.MeasurementType} Due",
                    Message = alert.Message,
                    PatientName = alert.PatientName,
                    PartographId = alert.PartographId,
                    PatientId = alert.PatientId,
                    Severity = alert.Severity,
                    CreatedAt = alert.CreatedAt,
                    IsAcknowledged = alert.IsAcknowledged,
                    MeasurementDueAlert = alert
                });
            }

            // Add clinical alerts as notifications
            foreach (var alert in _clinicalAlerts.Where(a => a.Severity != AlertSeverity.Info))
            {
                _notifications.Add(new NotificationItem
                {
                    Id = alert.Id,
                    Type = NotificationType.ClinicalAlert,
                    Title = $"{alert.SeverityIcon} {alert.Title}",
                    Message = alert.Message,
                    Severity = alert.Severity,
                    CreatedAt = alert.TriggeredAt,
                    IsAcknowledged = alert.IsAcknowledged,
                    ClinicalAlert = alert
                });
            }

            // Sort by severity (critical first) then by time
            var sortedNotifications = _notifications
                .OrderByDescending(n => n.Severity)
                .ThenByDescending(n => n.CreatedAt)
                .ToList();

            _notifications.Clear();
            foreach (var n in sortedNotifications)
            {
                _notifications.Add(n);
            }
        }

        /// <summary>
        /// Shows a toast notification with vibration for critical alerts
        /// </summary>
        private async void ShowToastNotificationAsync(string message, AlertSeverity severity)
        {
            try
            {
                // Vibrate for critical and warning alerts
                if (severity == AlertSeverity.Critical || severity == AlertSeverity.Warning)
                {
                    try
                    {
                        var vibrationDuration = severity == AlertSeverity.Critical
                            ? TimeSpan.FromMilliseconds(500)
                            : TimeSpan.FromMilliseconds(200);

                        // Vibrate pattern for critical: long-short-long
                        if (severity == AlertSeverity.Critical)
                        {
                            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
                            await Task.Delay(200);
                            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
                            await Task.Delay(100);
                            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
                        }
                        else
                        {
                            Vibration.Default.Vibrate(vibrationDuration);
                        }
                    }
                    catch (FeatureNotSupportedException)
                    {
                        // Vibration not supported on this device
                        _logger?.LogWarning("Vibration not supported on this device");
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error vibrating device");
                    }
                }

                // Show toast notification
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await AppShell.DisplayToastAsync(message);
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error showing toast notification");
            }
        }

        /// <summary>
        /// Acknowledges a notification
        /// </summary>
        public void AcknowledgeNotification(Guid notificationId)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                notification.IsAcknowledged = true;

                if (notification.MeasurementDueAlert != null)
                {
                    notification.MeasurementDueAlert.IsAcknowledged = true;
                }

                if (notification.ClinicalAlert != null)
                {
                    _alertEngine.AcknowledgeAlert(notification.ClinicalAlert.Id, Constants.Staff?.Name ?? "Unknown");
                }

                _appShellModel?.UpdateNotificationCount(TotalNotificationCount);
                NotificationCountChanged?.Invoke(this, TotalNotificationCount);
            }
        }

        /// <summary>
        /// Acknowledges all notifications
        /// </summary>
        public void AcknowledgeAllNotifications()
        {
            foreach (var notification in _notifications)
            {
                notification.IsAcknowledged = true;

                if (notification.MeasurementDueAlert != null)
                {
                    notification.MeasurementDueAlert.IsAcknowledged = true;
                }

                if (notification.ClinicalAlert != null)
                {
                    _alertEngine.AcknowledgeAlert(notification.ClinicalAlert.Id, Constants.Staff?.Name ?? "Unknown");
                }
            }

            _appShellModel?.UpdateNotificationCount(0);
            NotificationCountChanged?.Invoke(this, 0);
        }

        /// <summary>
        /// Clears all acknowledged notifications
        /// </summary>
        public void ClearAcknowledgedNotifications()
        {
            var toRemove = _notifications.Where(n => n.IsAcknowledged).ToList();
            foreach (var notification in toRemove)
            {
                _notifications.Remove(notification);
            }

            _alertEngine.ClearAcknowledgedAlerts();
        }

        /// <summary>
        /// Clears all notifications
        /// </summary>
        public void ClearAllNotifications()
        {
            _notifications.Clear();
            _measurementDueAlerts.Clear();
            _clinicalAlerts.Clear();
            _appShellModel?.UpdateNotificationCount(0);
            NotificationCountChanged?.Invoke(this, 0);
        }

        private void OnAlertTriggered(object? sender, ClinicalAlert alert)
        {
            _logger?.LogInformation("Clinical alert triggered: {Title}", alert.Title);
        }

        private void OnAlertCleared(object? sender, Guid alertId)
        {
            _logger?.LogInformation("Clinical alert cleared: {AlertId}", alertId);
        }

        public void Dispose()
        {
            Stop();
            _alertEngine.AlertTriggered -= OnAlertTriggered;
            _alertEngine.AlertCleared -= OnAlertCleared;
        }
    }
}
