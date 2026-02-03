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

        // Escalation tracking
        public int EscalationLevel { get; set; } = 0;
        public DateTime? EscalatedAt { get; set; }
        public DateTime? FirstAlertTime { get; set; }

        // Quick action support
        public string QuickActionRoute { get; set; } = string.Empty;

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

        public string EscalationDisplay => EscalationLevel switch
        {
            1 => "⬆️ Escalated",
            2 => "⬆️⬆️ Supervisor notified",
            3 => "🚨 MISSED - Logged",
            _ => ""
        };

        public bool ShowQuickAction => !string.IsNullOrEmpty(QuickActionRoute);
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

        // Escalation
        public int EscalationLevel { get; set; }

        // Quick action
        public string QuickActionRoute { get; set; } = string.Empty;
        public string MeasurementType { get; set; } = string.Empty;

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

        public bool ShowQuickAction => !string.IsNullOrEmpty(QuickActionRoute);
        public bool IsEscalated => EscalationLevel > 0;

        public string EscalationBadge => EscalationLevel switch
        {
            1 => "L1",
            2 => "L2",
            3 => "MISSED",
            _ => ""
        };
    }

    public enum NotificationType
    {
        ClinicalAlert,
        MeasurementDue
    }

    /// <summary>
    /// Service that actively monitors all active partographs and generates alerts when measurements are due.
    /// Implements WHO Labour Care Guide 2020 monitoring intervals with smart escalation and progressive intensity.
    /// </summary>
    public class PartographMonitoringService : IDisposable
    {
        private readonly PartographRepository _partographRepository;
        private readonly AlertHistoryRepository _alertHistoryRepository;
        private readonly AlertEngine _alertEngine;
        private readonly ILogger<PartographMonitoringService>? _logger;
        private readonly AppShellModel? _appShellModel;

        private Timer? _monitoringTimer;
        private Timer? _escalationTimer;
        private bool _isRunning;
        private readonly object _lock = new();

        // Track alert first occurrence for escalation
        private readonly Dictionary<string, DateTime> _alertFirstOccurrence = new();
        private readonly Dictionary<string, int> _alertEscalationLevels = new();

        // Observable collections for notifications
        private readonly ObservableCollection<NotificationItem> _notifications = new();
        private readonly ObservableCollection<MeasurementDueAlert> _measurementDueAlerts = new();
        private readonly List<ClinicalAlert> _clinicalAlerts = new();

        // Events for notification updates
        public event EventHandler<NotificationItem>? NotificationAdded;
        public event EventHandler<int>? NotificationCountChanged;
        public event EventHandler<MeasurementDueAlert>? MeasurementDue;
        public event EventHandler<(NotificationItem Alert, int Level)>? AlertEscalated;
        public event EventHandler<NotificationItem>? AlertMissed;

        // Current shift tracking
        public string CurrentShiftId { get; private set; } = string.Empty;
        public DateTime ShiftStartTime { get; private set; }

        // WHO 2020 Monitoring Intervals (in minutes)
        private const int FHR_INTERVAL_FIRST_STAGE = 30;
        private const int FHR_INTERVAL_SECOND_STAGE = 15;
        private const int CONTRACTION_INTERVAL = 30;
        private const int BP_INTERVAL = 240; // 4 hours
        private const int TEMPERATURE_INTERVAL = 240; // 4 hours
        private const int VAGINAL_EXAM_INTERVAL = 240; // 4 hours
        private const int URINE_INTERVAL = 240; // 4 hours

        // Escalation thresholds (in minutes)
        private const int ESCALATION_LEVEL_1_MINUTES = 5;   // Increase intensity
        private const int ESCALATION_LEVEL_2_MINUTES = 10;  // Notify supervisor
        private const int ESCALATION_LEVEL_3_MINUTES = 15;  // Mark as missed

        // Monitoring timer interval (check every minute)
        private const int MONITORING_INTERVAL_MS = 60000; // 1 minute
        private const int ESCALATION_CHECK_INTERVAL_MS = 30000; // 30 seconds

        public IReadOnlyCollection<NotificationItem> Notifications => _notifications;
        public IReadOnlyCollection<MeasurementDueAlert> MeasurementDueAlerts => _measurementDueAlerts;
        public IReadOnlyCollection<ClinicalAlert> ClinicalAlerts => _clinicalAlerts;

        public int TotalNotificationCount => _notifications.Count(n => !n.IsAcknowledged);
        public int CriticalCount => _notifications.Count(n => !n.IsAcknowledged && n.Severity == AlertSeverity.Critical);
        public int WarningCount => _notifications.Count(n => !n.IsAcknowledged && n.Severity == AlertSeverity.Warning);
        public int EscalatedCount => _notifications.Count(n => !n.IsAcknowledged && n.EscalationLevel > 0);

        public PartographMonitoringService(
            PartographRepository partographRepository,
            AlertHistoryRepository alertHistoryRepository,
            ILogger<PartographMonitoringService>? logger = null)
        {
            _partographRepository = partographRepository;
            _alertHistoryRepository = alertHistoryRepository;
            _alertEngine = new AlertEngine();
            _logger = logger;

            // Try to get AppShellModel from DI
            _appShellModel = IPlatformApplication.Current?.Services.GetService<AppShellModel>();

            // Subscribe to AlertEngine events
            _alertEngine.AlertTriggered += OnAlertTriggered;
            _alertEngine.AlertCleared += OnAlertCleared;

            // Start a new shift
            StartNewShift();
        }

        /// <summary>
        /// Starts a new shift for tracking
        /// </summary>
        public void StartNewShift()
        {
            CurrentShiftId = $"SHIFT_{DateTime.Now:yyyyMMdd_HHmmss}_{Constants.Staff?.ID}";
            ShiftStartTime = DateTime.Now;
            _logger?.LogInformation("New shift started: {ShiftId}", CurrentShiftId);
        }

        /// <summary>
        /// Starts the background monitoring service
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                if (_isRunning) return;

                // Main monitoring timer
                _monitoringTimer = new Timer(
                    async _ => await MonitorPartographsAsync(),
                    null,
                    TimeSpan.Zero,
                    TimeSpan.FromMilliseconds(MONITORING_INTERVAL_MS));

                // Escalation check timer (more frequent)
                _escalationTimer = new Timer(
                    async _ => await CheckEscalationsAsync(),
                    null,
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMilliseconds(ESCALATION_CHECK_INTERVAL_MS));

                _isRunning = true;
                _logger?.LogInformation("Partograph monitoring service started with escalation support");
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
                _escalationTimer?.Dispose();
                _escalationTimer = null;
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
        /// Gets alerts for a specific patient/partograph
        /// </summary>
        public List<NotificationItem> GetAlertsForPartograph(Guid partographId)
        {
            return _notifications.Where(n => n.PartographId == partographId).ToList();
        }

        /// <summary>
        /// Gets the dashboard summary for the home page
        /// </summary>
        public AlertDashboardSummary GetDashboardSummary()
        {
            var summary = new AlertDashboardSummary
            {
                TotalActiveAlerts = TotalNotificationCount,
                CriticalAlerts = CriticalCount,
                WarningAlerts = WarningCount,
                EscalatedAlerts = EscalatedCount,
                OverdueMeasurementsByType = new Dictionary<string, int>()
            };

            // Count overdue by measurement type
            foreach (var alert in _measurementDueAlerts.Where(a => a.IsOverdue && !a.IsAcknowledged))
            {
                if (!summary.OverdueMeasurementsByType.ContainsKey(alert.MeasurementType))
                    summary.OverdueMeasurementsByType[alert.MeasurementType] = 0;
                summary.OverdueMeasurementsByType[alert.MeasurementType]++;
            }

            // Find most urgent patient
            var mostUrgent = _notifications
                .Where(n => !n.IsAcknowledged)
                .OrderByDescending(n => n.Severity)
                .ThenByDescending(n => n.EscalationLevel)
                .FirstOrDefault();

            if (mostUrgent != null)
            {
                summary.MostUrgentPatientName = mostUrgent.PatientName;
                summary.MostUrgentPartographId = mostUrgent.PartographId;
                summary.MostUrgentReason = mostUrgent.Title;
            }

            return summary;
        }

        /// <summary>
        /// Generates a shift handover report
        /// </summary>
        public async Task<ShiftHandoverReport> GenerateShiftHandoverReportAsync()
        {
            var report = new ShiftHandoverReport
            {
                ShiftId = CurrentShiftId,
                ShiftStart = ShiftStartTime,
                ShiftEnd = DateTime.Now,
                StaffName = Constants.Staff?.Name ?? "Unknown",
                FacilityId = Constants.GetFacilityForFiltering(),
                FacilityName = Constants.SelectedFacility?.Name ?? ""
            };

            // Get all alerts from this shift
            var shiftAlerts = await _alertHistoryRepository.GetAlertsByShiftAsync(CurrentShiftId);

            report.TotalAlertsGenerated = shiftAlerts.Count;
            report.AlertsAcknowledged = shiftAlerts.Count(a => a.IsAcknowledged);
            report.AlertsMissed = shiftAlerts.Count(a => a.IsMissed);
            report.CompliancePercentage = report.TotalAlertsGenerated > 0
                ? (double)report.AlertsAcknowledged / report.TotalAlertsGenerated * 100
                : 100;

            // Pending alerts (unacknowledged)
            report.PendingAlerts = shiftAlerts.Where(a => !a.IsAcknowledged && !a.IsResolved).ToList();

            // Resolved alerts this shift
            report.ResolvedAlerts = shiftAlerts.Where(a => a.IsResolved).ToList();

            // Patients requiring attention
            var activePartographs = await _partographRepository.ListAsync();
            report.TotalActivePatients = activePartographs.Count(p =>
                p.Status == LaborStatus.FirstStage ||
                p.Status == LaborStatus.SecondStage ||
                p.Status == LaborStatus.ThirdStage);

            foreach (var alert in _measurementDueAlerts.Where(a => a.IsOverdue && !a.IsAcknowledged))
            {
                report.PatientsRequiringAttention.Add(new PatientAttentionItem
                {
                    PatientId = alert.PatientId,
                    PartographId = alert.PartographId,
                    PatientName = alert.PatientName,
                    Reason = $"{alert.MeasurementType} overdue",
                    Severity = alert.Severity.ToString(),
                    OverdueMinutes = alert.MinutesOverdue
                });

                report.OverdueMeasurements.Add(new OverdueMeasurement
                {
                    PartographId = alert.PartographId,
                    PatientName = alert.PatientName,
                    MeasurementType = alert.MeasurementType,
                    LastMeasurementTime = alert.LastMeasurementTime ?? alert.DueTime,
                    MinutesOverdue = alert.MinutesOverdue
                });
            }

            return report;
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
                    facilityId = Constants.GetFacilityForFiltering();
                }
                else if (Constants.SelectedFacility != null)
                {
                    facilityId = Constants.GetFacilityForFiltering();
                }

                // Get all active partographs (FirstStage, SecondStage, ThirdStage)
                var activePartographs = await _partographRepository.ListAsync();
                activePartographs = activePartographs
                    .Where(p => p.Status == LaborStatus.FirstStage ||
                                p.Status == LaborStatus.SecondStage ||
                                p.Status == LaborStatus.ThirdStage)
                    .Where(p => facilityId == null || p.FacilityID == facilityId)
                    .ToList();

                if (!activePartographs.Any())
                {
                    ClearAllNotifications();
                    return;
                }

                var newMeasurementAlerts = new List<MeasurementDueAlert>();
                var newClinicalAlerts = new List<ClinicalAlert>();

                foreach (var partograph in activePartographs)
                {
                    var dueAlerts = CheckMeasurementsDue(partograph);
                    newMeasurementAlerts.AddRange(dueAlerts);

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
        /// Checks and processes alert escalations
        /// </summary>
        private async Task CheckEscalationsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var alertsToEscalate = new List<(NotificationItem Alert, int NewLevel)>();

                foreach (var notification in _notifications.Where(n => !n.IsAcknowledged))
                {
                    var alertKey = GetAlertKey(notification);

                    if (!_alertFirstOccurrence.ContainsKey(alertKey))
                    {
                        _alertFirstOccurrence[alertKey] = notification.CreatedAt;
                        _alertEscalationLevels[alertKey] = 0;
                    }

                    var minutesSinceFirst = (now - _alertFirstOccurrence[alertKey]).TotalMinutes;
                    var currentLevel = _alertEscalationLevels[alertKey];

                    // Check for escalation
                    if (minutesSinceFirst >= ESCALATION_LEVEL_3_MINUTES && currentLevel < 3)
                    {
                        alertsToEscalate.Add((notification, 3));
                        _alertEscalationLevels[alertKey] = 3;
                    }
                    else if (minutesSinceFirst >= ESCALATION_LEVEL_2_MINUTES && currentLevel < 2)
                    {
                        alertsToEscalate.Add((notification, 2));
                        _alertEscalationLevels[alertKey] = 2;
                    }
                    else if (minutesSinceFirst >= ESCALATION_LEVEL_1_MINUTES && currentLevel < 1)
                    {
                        alertsToEscalate.Add((notification, 1));
                        _alertEscalationLevels[alertKey] = 1;
                    }
                }

                // Process escalations
                foreach (var (alert, newLevel) in alertsToEscalate)
                {
                    await ProcessEscalationAsync(alert, newLevel);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking escalations");
            }
        }

        private async Task ProcessEscalationAsync(NotificationItem alert, int newLevel)
        {
            alert.EscalationLevel = newLevel;

            if (alert.MeasurementDueAlert != null)
            {
                alert.MeasurementDueAlert.EscalationLevel = newLevel;
                alert.MeasurementDueAlert.EscalatedAt = DateTime.Now;
            }

            // Persist escalation to database
            var historyRecord = new AlertHistoryRecord
            {
                Id = alert.Id,
                PartographId = alert.PartographId,
                PatientId = alert.PatientId,
                PatientName = alert.PatientName,
                AlertType = alert.Type.ToString(),
                MeasurementType = alert.MeasurementType,
                Severity = alert.Severity.ToString(),
                Title = alert.Title,
                Message = alert.Message,
                CreatedAt = alert.CreatedAt,
                EscalationLevel = newLevel,
                EscalatedAt = DateTime.Now,
                ShiftId = CurrentShiftId,
                FacilityId = Constants.GetFacilityForFiltering()
            };

            if (newLevel == 3)
            {
                historyRecord.IsMissed = true;
                await _alertHistoryRepository.MarkAlertAsMissedAsync(alert.Id);
                AlertMissed?.Invoke(this, alert);
                _logger?.LogWarning("Alert marked as MISSED: {Title} for {Patient}", alert.Title, alert.PatientName);
            }

            await _alertHistoryRepository.SaveAlertAsync(historyRecord);
            await _alertHistoryRepository.EscalateAlertAsync(alert.Id, newLevel);

            // Trigger escalation event
            AlertEscalated?.Invoke(this, (alert, newLevel));

            // Show escalation notification with progressive intensity
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await ShowEscalationNotificationAsync(alert, newLevel);
            });
        }

        private async Task ShowEscalationNotificationAsync(NotificationItem alert, int level)
        {
            string message;

            switch (level)
            {
                case 1:
                    message = $"⚠️ REMINDER: {alert.Title}";
                    // Stronger vibration
                    await VibrateProgressiveAsync(1);
                    break;
                case 2:
                    message = $"🔔 SUPERVISOR ALERT: {alert.Title} - {alert.PatientName}";
                    // Multiple vibrations
                    await VibrateProgressiveAsync(2);
                    break;
                case 3:
                    message = $"🚨 MISSED ALERT LOGGED: {alert.Title} - {alert.PatientName}";
                    // Urgent vibration pattern
                    await VibrateProgressiveAsync(3);
                    break;
                default:
                    return;
            }

            await AppShell.DisplayToastAsync(message);
        }

        private async Task VibrateProgressiveAsync(int level)
        {
            try
            {
                switch (level)
                {
                    case 1:
                        // Level 1: Two medium vibrations
                        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
                        await Task.Delay(150);
                        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
                        break;
                    case 2:
                        // Level 2: Three strong vibrations
                        for (int i = 0; i < 3; i++)
                        {
                            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(400));
                            await Task.Delay(200);
                        }
                        break;
                    case 3:
                        // Level 3: Urgent pattern - long-short-long-short-long
                        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
                        await Task.Delay(150);
                        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(150));
                        await Task.Delay(100);
                        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
                        await Task.Delay(150);
                        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(150));
                        await Task.Delay(100);
                        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
                        break;
                }
            }
            catch (FeatureNotSupportedException)
            {
                _logger?.LogWarning("Vibration not supported on this device");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error with progressive vibration");
            }
        }

        private string GetAlertKey(NotificationItem notification)
        {
            return $"{notification.PartographId}_{notification.MeasurementType}_{notification.Type}";
        }

        /// <summary>
        /// Checks which measurements are due for a partograph based on WHO 2020 intervals
        /// </summary>
        private List<MeasurementDueAlert> CheckMeasurementsDue(Partograph partograph)
        {
            var alerts = new List<MeasurementDueAlert>();
            var now = DateTime.Now;

            // FHR monitoring
            var fhrInterval = partograph.Status == LaborStatus.SecondStage
                ? FHR_INTERVAL_SECOND_STAGE
                : FHR_INTERVAL_FIRST_STAGE;
            var lastFhr = partograph.Fhrs?.OrderByDescending(f => f.Time).FirstOrDefault();
            if (lastFhr != null)
            {
                var fhrDueTime = lastFhr.Time.AddMinutes(fhrInterval);
                var minutesOverdue = (int)(now - fhrDueTime).TotalMinutes;
                if (minutesOverdue >= -5)
                {
                    alerts.Add(CreateMeasurementDueAlert(
                        partograph, "Fetal Heart Rate", "fhrmodal",
                        lastFhr.Time, fhrDueTime, minutesOverdue,
                        $"FHR measurement is {(minutesOverdue > 0 ? "overdue" : "due")} for {partograph.Name}"));
                }
            }
            else if (partograph.LaborStartTime.HasValue)
            {
                alerts.Add(CreateMeasurementDueAlert(
                    partograph, "Fetal Heart Rate", "fhrmodal",
                    null, partograph.LaborStartTime.Value, 30,
                    $"Initial FHR measurement needed for {partograph.Name}"));
            }

            // Contraction monitoring
            var lastContraction = partograph.Contractions?.OrderByDescending(c => c.Time).FirstOrDefault();
            if (lastContraction != null)
            {
                var contractionDueTime = lastContraction.Time.AddMinutes(CONTRACTION_INTERVAL);
                var minutesOverdue = (int)(now - contractionDueTime).TotalMinutes;
                if (minutesOverdue >= -5)
                {
                    alerts.Add(CreateMeasurementDueAlert(
                        partograph, "Contractions", "contractionmodal",
                        lastContraction.Time, contractionDueTime, minutesOverdue,
                        $"Contraction assessment is {(minutesOverdue > 0 ? "overdue" : "due")} for {partograph.Name}"));
                }
            }

            // Cervical Dilatation
            var lastDilatation = partograph.Dilatations?.OrderByDescending(d => d.Time).FirstOrDefault();
            if (lastDilatation != null)
            {
                var dilatationDueTime = lastDilatation.Time.AddMinutes(VAGINAL_EXAM_INTERVAL);
                var minutesOverdue = (int)(now - dilatationDueTime).TotalMinutes;
                if (minutesOverdue >= -10)
                {
                    alerts.Add(CreateMeasurementDueAlert(
                        partograph, "Cervical Dilatation", "cervixmodal",
                        lastDilatation.Time, dilatationDueTime, minutesOverdue,
                        $"Vaginal examination is {(minutesOverdue > 0 ? "overdue" : "due")} for {partograph.Name}"));
                }
            }

            // Blood Pressure
            var lastBp = partograph.BPs?.OrderByDescending(b => b.Time).FirstOrDefault();
            if (lastBp != null)
            {
                var bpDueTime = lastBp.Time.AddMinutes(BP_INTERVAL);
                var minutesOverdue = (int)(now - bpDueTime).TotalMinutes;
                if (minutesOverdue >= -10)
                {
                    alerts.Add(CreateMeasurementDueAlert(
                        partograph, "Blood Pressure", "bpmodal",
                        lastBp.Time, bpDueTime, minutesOverdue,
                        $"BP measurement is {(minutesOverdue > 0 ? "overdue" : "due")} for {partograph.Name}"));
                }
            }

            // Temperature
            var lastTemp = partograph.Temperatures?.OrderByDescending(t => t.Time).FirstOrDefault();
            if (lastTemp != null)
            {
                var tempDueTime = lastTemp.Time.AddMinutes(TEMPERATURE_INTERVAL);
                var minutesOverdue = (int)(now - tempDueTime).TotalMinutes;
                if (minutesOverdue >= -10)
                {
                    alerts.Add(CreateMeasurementDueAlert(
                        partograph, "Temperature", "temperaturemodal",
                        lastTemp.Time, tempDueTime, minutesOverdue,
                        $"Temperature measurement is {(minutesOverdue > 0 ? "overdue" : "due")} for {partograph.Name}"));
                }
            }

            // Urine
            var lastUrine = partograph.Urines?.OrderByDescending(u => u.Time).FirstOrDefault();
            if (lastUrine != null)
            {
                var urineDueTime = lastUrine.Time.AddMinutes(URINE_INTERVAL);
                var minutesOverdue = (int)(now - urineDueTime).TotalMinutes;
                if (minutesOverdue >= -10)
                {
                    alerts.Add(CreateMeasurementDueAlert(
                        partograph, "Urine", "urinemodal",
                        lastUrine.Time, urineDueTime, minutesOverdue,
                        $"Urine analysis is {(minutesOverdue > 0 ? "overdue" : "due")} for {partograph.Name}"));
                }
            }

            return alerts;
        }

        private MeasurementDueAlert CreateMeasurementDueAlert(
            Partograph partograph,
            string measurementType,
            string quickActionRoute,
            DateTime? lastMeasurementTime,
            DateTime dueTime,
            int minutesOverdue,
            string message)
        {
            AlertSeverity severity;
            if (minutesOverdue > 60) severity = AlertSeverity.Critical;
            else if (minutesOverdue > 0) severity = AlertSeverity.Warning;
            else severity = AlertSeverity.Info;

            var alertKey = $"{partograph.ID}_{measurementType}_MeasurementDue";
            var escalationLevel = _alertEscalationLevels.GetValueOrDefault(alertKey, 0);

            return new MeasurementDueAlert
            {
                PartographId = new Guid(partograph.ID.ToString()),
                PatientId = new Guid(partograph.Patient.ID.ToString()),
                PatientName = partograph.Name ?? "Unknown Patient",
                MeasurementType = measurementType,
                DueTime = dueTime,
                LastMeasurementTime = lastMeasurementTime,
                MinutesOverdue = minutesOverdue,
                Severity = severity,
                Message = message,
                QuickActionRoute = quickActionRoute,
                EscalationLevel = escalationLevel,
                FirstAlertTime = _alertFirstOccurrence.GetValueOrDefault(alertKey)
            };
        }

        /// <summary>
        /// Updates the notification collections and triggers events
        /// </summary>
        private void UpdateNotifications(List<MeasurementDueAlert> measurementAlerts, List<ClinicalAlert> clinicalAlerts)
        {
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

            _measurementDueAlerts.Clear();
            foreach (var alert in measurementAlerts.OrderByDescending(a => a.MinutesOverdue))
            {
                _measurementDueAlerts.Add(alert);
            }

            _clinicalAlerts.Clear();
            _clinicalAlerts.AddRange(clinicalAlerts);

            RebuildNotificationsList();

            _appShellModel?.UpdateNotificationCount(TotalNotificationCount);
            NotificationCountChanged?.Invoke(this, TotalNotificationCount);

            // Save new alerts to history and trigger notifications
            foreach (var alert in newMeasurementAlerts.Where(a => a.Severity != AlertSeverity.Info))
            {
                MeasurementDue?.Invoke(this, alert);
                ShowToastNotificationAsync(alert.Message, alert.Severity);
                SaveAlertToHistoryAsync(alert);
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

        private async void SaveAlertToHistoryAsync(MeasurementDueAlert alert)
        {
            try
            {
                // Check if alert already exists in database
                var exists = await _alertHistoryRepository.AlertExistsAsync(
                    alert.PartographId, alert.MeasurementType);

                if (!exists)
                {
                    var record = new AlertHistoryRecord
                    {
                        Id = alert.Id,
                        PartographId = alert.PartographId,
                        PatientId = alert.PatientId,
                        PatientName = alert.PatientName,
                        AlertType = "MeasurementDue",
                        MeasurementType = alert.MeasurementType,
                        Severity = alert.Severity.ToString(),
                        Title = $"{alert.MeasurementType} Due",
                        Message = alert.Message,
                        CreatedAt = alert.CreatedAt,
                        ShiftId = CurrentShiftId,
                        FacilityId = Constants.GetFacilityForFiltering()
                    };

                    await _alertHistoryRepository.SaveAlertAsync(record);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving alert to history");
            }
        }

        private void RebuildNotificationsList()
        {
            _notifications.Clear();

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
                    MeasurementDueAlert = alert,
                    EscalationLevel = alert.EscalationLevel,
                    QuickActionRoute = alert.QuickActionRoute,
                    MeasurementType = alert.MeasurementType
                });
            }

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

            var sortedNotifications = _notifications
                .OrderByDescending(n => n.EscalationLevel)
                .ThenByDescending(n => n.Severity)
                .ThenByDescending(n => n.CreatedAt)
                .ToList();

            _notifications.Clear();
            foreach (var n in sortedNotifications)
            {
                _notifications.Add(n);
            }
        }

        private async void ShowToastNotificationAsync(string message, AlertSeverity severity)
        {
            try
            {
                if (severity == AlertSeverity.Critical || severity == AlertSeverity.Warning)
                {
                    try
                    {
                        var vibrationDuration = severity == AlertSeverity.Critical
                            ? TimeSpan.FromMilliseconds(500)
                            : TimeSpan.FromMilliseconds(200);

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
                        _logger?.LogWarning("Vibration not supported on this device");
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error vibrating device");
                    }
                }

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
        public async void AcknowledgeNotification(Guid notificationId)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                notification.IsAcknowledged = true;

                var alertKey = GetAlertKey(notification);
                _alertFirstOccurrence.Remove(alertKey);
                _alertEscalationLevels.Remove(alertKey);

                if (notification.MeasurementDueAlert != null)
                {
                    notification.MeasurementDueAlert.IsAcknowledged = true;
                }

                if (notification.ClinicalAlert != null)
                {
                    _alertEngine.AcknowledgeAlert(notification.ClinicalAlert.Id, Constants.Staff?.Name ?? "Unknown");
                }

                // Update database
                await _alertHistoryRepository.AcknowledgeAlertAsync(notificationId, Constants.Staff?.Name ?? "Unknown");

                _appShellModel?.UpdateNotificationCount(TotalNotificationCount);
                NotificationCountChanged?.Invoke(this, TotalNotificationCount);
            }
        }

        /// <summary>
        /// Acknowledges all notifications
        /// </summary>
        public async void AcknowledgeAllNotifications()
        {
            foreach (var notification in _notifications.ToList())
            {
                notification.IsAcknowledged = true;

                var alertKey = GetAlertKey(notification);
                _alertFirstOccurrence.Remove(alertKey);
                _alertEscalationLevels.Remove(alertKey);

                if (notification.MeasurementDueAlert != null)
                {
                    notification.MeasurementDueAlert.IsAcknowledged = true;
                }

                if (notification.ClinicalAlert != null)
                {
                    _alertEngine.AcknowledgeAlert(notification.ClinicalAlert.Id, Constants.Staff?.Name ?? "Unknown");
                }

                await _alertHistoryRepository.AcknowledgeAlertAsync(notification.Id, Constants.Staff?.Name ?? "Unknown");
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
            _alertFirstOccurrence.Clear();
            _alertEscalationLevels.Clear();
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

    /// <summary>
    /// Dashboard summary for the home page alert widget
    /// </summary>
    public class AlertDashboardSummary
    {
        public int TotalActiveAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public int WarningAlerts { get; set; }
        public int EscalatedAlerts { get; set; }
        public Dictionary<string, int> OverdueMeasurementsByType { get; set; } = new();
        public string? MostUrgentPatientName { get; set; }
        public Guid? MostUrgentPartographId { get; set; }
        public string? MostUrgentReason { get; set; }
        public bool HasUrgentPatient => MostUrgentPartographId.HasValue;
    }
}
