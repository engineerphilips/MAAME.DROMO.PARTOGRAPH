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
        private bool _isRefreshing;
        private string _selectedFilter = "All";
        private int _totalCount;
        private int _criticalCount;
        private int _warningCount;
        private int _infoCount;

        public NotificationsPageModel(PartographMonitoringService monitoringService)
        {
            _monitoringService = monitoringService;

            // Initialize commands
            RefreshCommand = new Command(async () => await RefreshAsync());
            AcknowledgeCommand = new Command<NotificationItem>(Acknowledge);
            AcknowledgeAllCommand = new Command(AcknowledgeAll);
            ClearAcknowledgedCommand = new Command(ClearAcknowledged);
            NavigateToPartographCommand = new Command<NotificationItem>(async (n) => await NavigateToPartographAsync(n));
            FilterCommand = new Command<string>(SetFilter);

            // Subscribe to monitoring service events
            _monitoringService.NotificationCountChanged += OnNotificationCountChanged;
            _monitoringService.NotificationAdded += OnNotificationAdded;

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

        public bool HasNotifications => TotalCount > 0;
        public bool HasNoNotifications => TotalCount == 0;

        // Filter selection states
        public bool FilterAllSelected => SelectedFilter == "All";
        public bool FilterCriticalSelected => SelectedFilter == "Critical";
        public bool FilterWarningSelected => SelectedFilter == "Warning";
        public bool FilterDueSelected => SelectedFilter == "Due";

        // Summary text
        public string SummaryText
        {
            get
            {
                if (TotalCount == 0) return "No active notifications";

                var parts = new List<string>();
                if (CriticalCount > 0) parts.Add($"{CriticalCount} critical");
                if (WarningCount > 0) parts.Add($"{WarningCount} warning");
                if (InfoCount > 0) parts.Add($"{InfoCount} info");

                return string.Join(", ", parts);
            }
        }

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand AcknowledgeCommand { get; }
        public ICommand AcknowledgeAllCommand { get; }
        public ICommand ClearAcknowledgedCommand { get; }
        public ICommand NavigateToPartographCommand { get; }
        public ICommand FilterCommand { get; }

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
                // Navigate to the partograph page
                await Shell.Current.GoToAsync($"partograph?id={notification.PartographId}");
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

        public void OnAppearing()
        {
            // Refresh when page appears
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
