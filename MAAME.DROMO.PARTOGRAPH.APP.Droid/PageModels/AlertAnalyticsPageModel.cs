using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public class AlertAnalyticsPageModel : INotifyPropertyChanged
    {
        private readonly AlertHistoryRepository _alertHistoryRepository;
        private readonly PartographMonitoringService _monitoringService;
        private bool _isLoading;
        private AlertAnalytics? _analytics;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _selectedPeriod = "Today";

        public AlertAnalyticsPageModel(
            AlertHistoryRepository alertHistoryRepository,
            PartographMonitoringService monitoringService)
        {
            _alertHistoryRepository = alertHistoryRepository;
            _monitoringService = monitoringService;

            // Default to today
            _startDate = DateTime.Today;
            _endDate = DateTime.Today.AddDays(1).AddSeconds(-1);

            // Initialize commands
            RefreshCommand = new Command(async () => await LoadAnalyticsAsync());
            SetPeriodCommand = new Command<string>(SetPeriod);
            ViewAlertDetailsCommand = new Command<AlertHistoryRecord>(async (a) => await ViewAlertDetailsAsync(a));
            ExportReportCommand = new Command(async () => await ExportReportAsync());
        }

        #region Properties

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public AlertAnalytics? Analytics
        {
            get => _analytics;
            set
            {
                if (SetProperty(ref _analytics, value))
                {
                    OnPropertyChanged(nameof(HasAnalytics));
                    OnPropertyChanged(nameof(TotalAlerts));
                    OnPropertyChanged(nameof(AcknowledgedCount));
                    OnPropertyChanged(nameof(MissedCount));
                    OnPropertyChanged(nameof(CompliancePercentage));
                    OnPropertyChanged(nameof(ComplianceColor));
                    OnPropertyChanged(nameof(AverageResponseTime));
                    OnPropertyChanged(nameof(CriticalAlerts));
                    OnPropertyChanged(nameof(WarningAlerts));
                    OnPropertyChanged(nameof(InfoAlerts));
                    OnPropertyChanged(nameof(AlertsByType));
                    OnPropertyChanged(nameof(AlertsByHour));
                    OnPropertyChanged(nameof(TopAlertTypes));
                    OnPropertyChanged(nameof(RecentAlerts));
                }
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    OnPropertyChanged(nameof(DateRangeDisplay));
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    OnPropertyChanged(nameof(DateRangeDisplay));
                }
            }
        }

        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                if (SetProperty(ref _selectedPeriod, value))
                {
                    OnPropertyChanged(nameof(IsTodaySelected));
                    OnPropertyChanged(nameof(IsWeekSelected));
                    OnPropertyChanged(nameof(IsMonthSelected));
                    OnPropertyChanged(nameof(IsShiftSelected));
                }
            }
        }

        public string DateRangeDisplay => $"{StartDate:MMM dd} - {EndDate:MMM dd, yyyy}";

        public bool HasAnalytics => Analytics != null;

        // Summary Stats
        public int TotalAlerts => Analytics?.TotalAlerts ?? 0;
        public int AcknowledgedCount => Analytics?.AcknowledgedCount ?? 0;
        public int MissedCount => Analytics?.MissedCount ?? 0;
        public double CompliancePercentage => Analytics?.CompliancePercentage ?? 100;
        public double AverageResponseTime => Analytics?.AverageResponseTimeMinutes ?? 0;

        public string ComplianceColor
        {
            get
            {
                if (Analytics == null) return "#4CAF50";
                return CompliancePercentage >= 90 ? "#4CAF50" :
                       CompliancePercentage >= 70 ? "#FF9800" : "#EF5350";
            }
        }

        // By Severity
        public int CriticalAlerts => Analytics?.BySeverity.GetValueOrDefault("Critical", 0) ?? 0;
        public int WarningAlerts => Analytics?.BySeverity.GetValueOrDefault("Warning", 0) ?? 0;
        public int InfoAlerts => Analytics?.BySeverity.GetValueOrDefault("Info", 0) ?? 0;

        // Collections
        public ObservableCollection<AlertTypeCount> AlertsByType =>
            new ObservableCollection<AlertTypeCount>(
                Analytics?.ByType.Select(kvp => new AlertTypeCount { Type = kvp.Key, Count = kvp.Value })
                ?? Enumerable.Empty<AlertTypeCount>());

        public ObservableCollection<AlertHourCount> AlertsByHour =>
            new ObservableCollection<AlertHourCount>(
                Analytics?.ByHour.Select(kvp => new AlertHourCount { Hour = kvp.Key, Count = kvp.Value })
                ?? Enumerable.Empty<AlertHourCount>());

        public ObservableCollection<AlertTypeCount> TopAlertTypes =>
            new ObservableCollection<AlertTypeCount>(
                AlertsByType.OrderByDescending(a => a.Count).Take(5));

        public ObservableCollection<AlertHistoryRecord> RecentAlerts =>
            new ObservableCollection<AlertHistoryRecord>(
                Analytics?.RecentAlerts.Take(10) ?? Enumerable.Empty<AlertHistoryRecord>());

        // Period selection states
        public bool IsTodaySelected => SelectedPeriod == "Today";
        public bool IsWeekSelected => SelectedPeriod == "Week";
        public bool IsMonthSelected => SelectedPeriod == "Month";
        public bool IsShiftSelected => SelectedPeriod == "Shift";

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand SetPeriodCommand { get; }
        public ICommand ViewAlertDetailsCommand { get; }
        public ICommand ExportReportCommand { get; }

        #endregion

        #region Methods

        public async Task LoadAnalyticsAsync()
        {
            try
            {
                IsLoading = true;

                // Get facility ID for filtering
                Guid? facilityId = null;
                if (!Constants.IsSuperOrAdmin())
                {
                    facilityId = Constants.GetFacilityForFiltering();
                }
                else if (Constants.SelectedFacility != null)
                {
                    facilityId = Constants.GetFacilityForFiltering();
                }

                Analytics = await _alertHistoryRepository.GetAnalyticsAsync(StartDate, EndDate, facilityId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading analytics: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SetPeriod(string? period)
        {
            if (string.IsNullOrEmpty(period)) return;

            SelectedPeriod = period;

            switch (period)
            {
                case "Today":
                    StartDate = DateTime.Today;
                    EndDate = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case "Week":
                    StartDate = DateTime.Today.AddDays(-7);
                    EndDate = DateTime.Now;
                    break;
                case "Month":
                    StartDate = DateTime.Today.AddDays(-30);
                    EndDate = DateTime.Now;
                    break;
                case "Shift":
                    StartDate = _monitoringService.ShiftStartTime;
                    EndDate = DateTime.Now;
                    break;
            }

            _ = LoadAnalyticsAsync();
        }

        private async Task ViewAlertDetailsAsync(AlertHistoryRecord? alert)
        {
            if (alert?.PartographId == null) return;

            try
            {
                await Shell.Current.GoToAsync($"partograph?id={alert.PartographId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        private async Task ExportReportAsync()
        {
            if (Analytics == null) return;

            try
            {
                var reportText = GenerateAnalyticsReport();
                await Share.Default.RequestAsync(new ShareTextRequest
                {
                    Text = reportText,
                    Title = $"Alert Analytics Report - {StartDate:yyyy-MM-dd}"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Export error: {ex.Message}");
            }
        }

        private string GenerateAnalyticsReport()
        {
            if (Analytics == null) return "";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine("       ALERT ANALYTICS REPORT");
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine($"Period: {StartDate:yyyy-MM-dd HH:mm} - {EndDate:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Facility: {Constants.SelectedFacility?.Name ?? "All Facilities"}");
            sb.AppendLine();
            sb.AppendLine("───────────────────────────────────────");
            sb.AppendLine("           SUMMARY");
            sb.AppendLine("───────────────────────────────────────");
            sb.AppendLine($"Total Alerts: {TotalAlerts}");
            sb.AppendLine($"Acknowledged: {AcknowledgedCount}");
            sb.AppendLine($"Missed: {MissedCount}");
            sb.AppendLine($"Compliance Rate: {CompliancePercentage:F1}%");
            sb.AppendLine($"Avg Response Time: {AverageResponseTime:F1} minutes");
            sb.AppendLine();
            sb.AppendLine("───────────────────────────────────────");
            sb.AppendLine("         BY SEVERITY");
            sb.AppendLine("───────────────────────────────────────");
            sb.AppendLine($"Critical: {CriticalAlerts}");
            sb.AppendLine($"Warning: {WarningAlerts}");
            sb.AppendLine($"Info: {InfoAlerts}");
            sb.AppendLine();

            if (AlertsByType.Any())
            {
                sb.AppendLine("───────────────────────────────────────");
                sb.AppendLine("         BY TYPE");
                sb.AppendLine("───────────────────────────────────────");
                foreach (var type in AlertsByType.OrderByDescending(t => t.Count))
                {
                    sb.AppendLine($"{type.Type}: {type.Count}");
                }
                sb.AppendLine();
            }

            if (AlertsByHour.Any())
            {
                sb.AppendLine("───────────────────────────────────────");
                sb.AppendLine("         BY HOUR");
                sb.AppendLine("───────────────────────────────────────");
                foreach (var hour in AlertsByHour.OrderBy(h => h.Hour))
                {
                    sb.AppendLine($"{hour.Hour:00}:00 - {hour.Count} alerts");
                }
                sb.AppendLine();
            }

            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine("MAAME DROMO Partograph System");

            return sb.ToString();
        }

        public void OnAppearing()
        {
            _ = LoadAnalyticsAsync();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}
