using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public class NotificationsPageModel : INotifyPropertyChanged
    {
        private readonly PartographMonitoringService _monitoringService;
        private readonly AlertHistoryRepository _alertHistoryRepository;
        private bool _isRefreshing;
        private string _selectedFilter = "All";
        private int _totalCount;
        private int _criticalCount;
        private int _warningCount;
        private int _infoCount;
        private int _escalatedCount;

        public NotificationsPageModel(
            PartographMonitoringService monitoringService,
            AlertHistoryRepository alertHistoryRepository)
        {
            _monitoringService = monitoringService;
            _alertHistoryRepository = alertHistoryRepository;

            // Initialize commands
            RefreshCommand = new Command(async () => await RefreshAsync());
            AcknowledgeCommand = new Command<NotificationItem>(Acknowledge);
            AcknowledgeAllCommand = new Command(AcknowledgeAll);
            ClearAcknowledgedCommand = new Command(ClearAcknowledged);
            NavigateToPartographCommand = new Command<NotificationItem>(async (n) => await NavigateToPartographAsync(n));
            FilterCommand = new Command<string>(SetFilter);
            QuickActionCommand = new Command<NotificationItem>(async (n) => await ExecuteQuickActionAsync(n));
            ViewShiftReportCommand = new Command(async () => await ViewShiftReportAsync());
            ViewAnalyticsCommand = new Command(async () => await ViewAnalyticsAsync());

            // Subscribe to monitoring service events
            _monitoringService.NotificationCountChanged += OnNotificationCountChanged;
            _monitoringService.NotificationAdded += OnNotificationAdded;
            _monitoringService.AlertEscalated += OnAlertEscalated;

            // Initialize data
            UpdateCounts();
        }

        #region Properties

        public ObservableCollection<NotificationItem> Notifications =>
            new ObservableCollection<NotificationItem>(GetFilteredNotifications());

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (SetProperty(ref _selectedFilter, value))
                {
                    OnPropertyChanged(nameof(Notifications));
                    OnPropertyChanged(nameof(FilterAllSelected));
                    OnPropertyChanged(nameof(FilterCriticalSelected));
                    OnPropertyChanged(nameof(FilterWarningSelected));
                    OnPropertyChanged(nameof(FilterDueSelected));
                    OnPropertyChanged(nameof(FilterEscalatedSelected));
                }
            }
        }

        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        public int CriticalCount
        {
            get => _criticalCount;
            set => SetProperty(ref _criticalCount, value);
        }

        public int WarningCount
        {
            get => _warningCount;
            set => SetProperty(ref _warningCount, value);
        }

        public int InfoCount
        {
            get => _infoCount;
            set => SetProperty(ref _infoCount, value);
        }

        public int EscalatedCount
        {
            get => _escalatedCount;
            set => SetProperty(ref _escalatedCount, value);
        }

        public bool HasNotifications => TotalCount > 0;
        public bool HasNoNotifications => TotalCount == 0;

        // Filter selection states
        public bool FilterAllSelected => SelectedFilter == "All";
        public bool FilterCriticalSelected => SelectedFilter == "Critical";
        public bool FilterWarningSelected => SelectedFilter == "Warning";
        public bool FilterDueSelected => SelectedFilter == "Due";
        public bool FilterEscalatedSelected => SelectedFilter == "Escalated";

        // Summary text
        public string SummaryText
        {
            get
            {
                if (TotalCount == 0) return "No active notifications";

                var parts = new List<string>();
                if (CriticalCount > 0) parts.Add($"{CriticalCount} critical");
                if (WarningCount > 0) parts.Add($"{WarningCount} warning");
                if (EscalatedCount > 0) parts.Add($"{EscalatedCount} escalated");
                if (InfoCount > 0) parts.Add($"{InfoCount} due soon");

                return string.Join(", ", parts);
            }
        }

        // Shift info
        public string CurrentShiftInfo => $"Shift started: {_monitoringService.ShiftStartTime:HH:mm}";

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand AcknowledgeCommand { get; }
        public ICommand AcknowledgeAllCommand { get; }
        public ICommand ClearAcknowledgedCommand { get; }
        public ICommand NavigateToPartographCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand QuickActionCommand { get; }
        public ICommand ViewShiftReportCommand { get; }
        public ICommand ViewAnalyticsCommand { get; }

        #endregion

        #region Methods

        private IEnumerable<NotificationItem> GetFilteredNotifications()
        {
            var notifications = _monitoringService.Notifications;

            return SelectedFilter switch
            {
                "Critical" => notifications.Where(n => n.Severity == AlertSeverity.Critical),
                "Warning" => notifications.Where(n => n.Severity == AlertSeverity.Warning),
                "Due" => notifications.Where(n => n.Type == NotificationType.MeasurementDue),
                "Escalated" => notifications.Where(n => n.EscalationLevel > 0),
                _ => notifications
            };
        }

        private async Task RefreshAsync()
        {
            try
            {
                IsRefreshing = true;
                await _monitoringService.RefreshAsync();
                UpdateCounts();
                OnPropertyChanged(nameof(Notifications));
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void Acknowledge(NotificationItem? notification)
        {
            if (notification == null) return;

            _monitoringService.AcknowledgeNotification(notification.Id);
            UpdateCounts();
            OnPropertyChanged(nameof(Notifications));
        }

        private void AcknowledgeAll()
        {
            _monitoringService.AcknowledgeAllNotifications();
            UpdateCounts();
            OnPropertyChanged(nameof(Notifications));
        }

        private void ClearAcknowledged()
        {
            _monitoringService.ClearAcknowledgedNotifications();
            UpdateCounts();
            OnPropertyChanged(nameof(Notifications));
        }

        private async Task NavigateToPartographAsync(NotificationItem? notification)
        {
            if (notification?.PartographId == null) return;

            try
            {
                await Shell.Current.GoToAsync($"partograph?id={notification.PartographId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Executes quick action to open the measurement modal directly
        /// </summary>
        private async Task ExecuteQuickActionAsync(NotificationItem? notification)
        {
            if (notification == null || !notification.ShowQuickAction) return;

            try
            {
                // Acknowledge the notification first
                _monitoringService.AcknowledgeNotification(notification.Id);

                // Navigate to partograph with the measurement type parameter
                // This will open the partograph page and trigger the appropriate measurement modal
                var route = $"partograph?id={notification.PartographId}&openModal={notification.QuickActionRoute}";
                await Shell.Current.GoToAsync(route);

                UpdateCounts();
                OnPropertyChanged(nameof(Notifications));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Quick action error: {ex.Message}");
                await AppShell.DisplayToastAsync("Unable to open measurement form");
            }
        }

        /// <summary>
        /// Opens the shift handover report
        /// </summary>
        private async Task ViewShiftReportAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("shiftreport");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Opens the alert analytics dashboard
        /// </summary>
        private async Task ViewAnalyticsAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("alertanalytics");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        private void SetFilter(string? filter)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                SelectedFilter = filter;
            }
        }

        private void UpdateCounts()
        {
            var notifications = _monitoringService.Notifications;
            TotalCount = notifications.Count(n => !n.IsAcknowledged);
            CriticalCount = notifications.Count(n => !n.IsAcknowledged && n.Severity == AlertSeverity.Critical);
            WarningCount = notifications.Count(n => !n.IsAcknowledged && n.Severity == AlertSeverity.Warning);
            InfoCount = notifications.Count(n => !n.IsAcknowledged && n.Severity == AlertSeverity.Info);
            EscalatedCount = notifications.Count(n => !n.IsAcknowledged && n.EscalationLevel > 0);

            OnPropertyChanged(nameof(HasNotifications));
            OnPropertyChanged(nameof(HasNoNotifications));
            OnPropertyChanged(nameof(SummaryText));
        }

        private void OnNotificationCountChanged(object? sender, int count)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateCounts();
                OnPropertyChanged(nameof(Notifications));
            });
        }

        private void OnNotificationAdded(object? sender, NotificationItem notification)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateCounts();
                OnPropertyChanged(nameof(Notifications));
            });
        }

        private void OnAlertEscalated(object? sender, (NotificationItem Alert, int Level) args)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateCounts();
                OnPropertyChanged(nameof(Notifications));
            });
        }

        public void OnAppearing()
        {
            _ = RefreshAsync();
        }

        public void OnDisappearing()
        {
            // Cleanup if needed
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
