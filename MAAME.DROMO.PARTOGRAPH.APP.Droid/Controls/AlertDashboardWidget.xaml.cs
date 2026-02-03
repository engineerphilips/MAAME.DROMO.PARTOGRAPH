using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Controls
{
    public partial class AlertDashboardWidget : ContentView
    {
        private PartographMonitoringService? _monitoringService;
        private AlertDashboardSummary? _currentSummary;

        public AlertDashboardWidget()
        {
            InitializeComponent();
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            // Get the monitoring service from DI
            _monitoringService = IPlatformApplication.Current?.Services.GetService<PartographMonitoringService>();

            if (_monitoringService != null)
            {
                // Subscribe to notification changes
                _monitoringService.NotificationCountChanged += OnNotificationCountChanged;

                // Initial update
                UpdateDashboard();
            }
        }

        private void OnNotificationCountChanged(object? sender, int count)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateDashboard();
            });
        }

        /// <summary>
        /// Refreshes the dashboard data
        /// </summary>
        public void Refresh()
        {
            UpdateDashboard();
        }

        private void UpdateDashboard()
        {
            if (_monitoringService == null) return;

            _currentSummary = _monitoringService.GetDashboardSummary();

            // Update stats
            TotalAlertsLabel.Text = _currentSummary.TotalActiveAlerts.ToString();
            CriticalAlertsLabel.Text = _currentSummary.CriticalAlerts.ToString();
            WarningAlertsLabel.Text = _currentSummary.WarningAlerts.ToString();
            EscalatedAlertsLabel.Text = _currentSummary.EscalatedAlerts.ToString();

            // Update most urgent patient card
            if (_currentSummary.HasUrgentPatient)
            {
                UrgentPatientCard.IsVisible = true;
                NoAlertsCard.IsVisible = false;
                UrgentPatientNameLabel.Text = _currentSummary.MostUrgentPatientName ?? "Unknown";
                UrgentReasonLabel.Text = _currentSummary.MostUrgentReason ?? "";
            }
            else
            {
                UrgentPatientCard.IsVisible = false;
                NoAlertsCard.IsVisible = _currentSummary.TotalActiveAlerts == 0;
            }

            // Update overdue measurements summary
            UpdateOverdueMeasurements();
        }

        private void UpdateOverdueMeasurements()
        {
            if (_currentSummary == null) return;

            OverdueMeasurementsLayout.Children.Clear();

            if (_currentSummary.OverdueMeasurementsByType.Any())
            {
                OverdueSummaryStack.IsVisible = true;

                foreach (var kvp in _currentSummary.OverdueMeasurementsByType)
                {
                    var badge = CreateOverdueBadge(kvp.Key, kvp.Value);
                    OverdueMeasurementsLayout.Children.Add(badge);
                }
            }
            else
            {
                OverdueSummaryStack.IsVisible = false;
            }
        }

        private Border CreateOverdueBadge(string measurementType, int count)
        {
            var icon = measurementType switch
            {
                "Fetal Heart Rate" or "FHR" => "💓",
                "Contractions" => "📊",
                "Cervical Dilatation" => "📏",
                "Blood Pressure" or "BP" => "🩺",
                "Temperature" => "🌡️",
                "Urine" => "💧",
                _ => "📋"
            };

            var shortName = measurementType switch
            {
                "Fetal Heart Rate" => "FHR",
                "Cervical Dilatation" => "Dilatation",
                "Blood Pressure" => "BP",
                _ => measurementType
            };

            var badge = new Border
            {
                BackgroundColor = Color.FromArgb("#FFEBEE"),
                Padding = new Thickness(8, 4),
                Margin = new Thickness(0, 0, 6, 6),
                StrokeShape = new RoundRectangle { CornerRadius = 8 }
            };

            var content = new HorizontalStackLayout
            {
                Spacing = 4
            };

            content.Children.Add(new Label
            {
                Text = icon,
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center
            });

            content.Children.Add(new Label
            {
                Text = $"{shortName}: {count}",
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#EF5350"),
                VerticalOptions = LayoutOptions.Center
            });

            badge.Content = content;
            return badge;
        }

        private async void OnViewAllClicked(object? sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("notifications");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        private async void OnUrgentPatientTapped(object? sender, TappedEventArgs e)
        {
            if (_currentSummary?.MostUrgentPartographId == null) return;

            try
            {
                await Shell.Current.GoToAsync($"partograph?id={_currentSummary.MostUrgentPartographId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleanup when the widget is removed
        /// </summary>
        public void Dispose()
        {
            if (_monitoringService != null)
            {
                _monitoringService.NotificationCountChanged -= OnNotificationCountChanged;
            }
        }
    }
}
