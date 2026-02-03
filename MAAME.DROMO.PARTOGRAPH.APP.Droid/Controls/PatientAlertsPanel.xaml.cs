using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Controls
{
    public partial class PatientAlertsPanel : ContentView
    {
        private PartographMonitoringService? _monitoringService;
        private Guid? _partographId;
        private bool _isExpanded = false;
        private const int COLLAPSED_MAX_ITEMS = 3;

        public PatientAlertsPanel()
        {
            InitializeComponent();
        }

        #region Bindable Properties

        public static readonly BindableProperty PartographIdProperty =
            BindableProperty.Create(
                nameof(PartographId),
                typeof(Guid?),
                typeof(PatientAlertsPanel),
                null,
                propertyChanged: OnPartographIdChanged);

        public Guid? PartographId
        {
            get => (Guid?)GetValue(PartographIdProperty);
            set => SetValue(PartographIdProperty, value);
        }

        private static void OnPartographIdChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PatientAlertsPanel panel && newValue is Guid partographId)
            {
                panel._partographId = partographId;
                panel.RefreshAlerts();
            }
        }

        #endregion

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            // Get monitoring service from DI
            _monitoringService = IPlatformApplication.Current?.Services.GetService<PartographMonitoringService>();

            if (_monitoringService != null)
            {
                // Subscribe to notification changes
                _monitoringService.NotificationCountChanged += OnNotificationCountChanged;
                _monitoringService.NotificationAdded += OnNotificationAdded;
                _monitoringService.AlertEscalated += OnAlertEscalated;

                // Initial refresh
                RefreshAlerts();
            }
        }

        private void OnNotificationCountChanged(object? sender, int count)
        {
            MainThread.BeginInvokeOnMainThread(() => RefreshAlerts());
        }

        private void OnNotificationAdded(object? sender, NotificationItem notification)
        {
            if (notification.PartographId == _partographId)
            {
                MainThread.BeginInvokeOnMainThread(() => RefreshAlerts());
            }
        }

        private void OnAlertEscalated(object? sender, (NotificationItem Alert, int Level) args)
        {
            if (args.Alert.PartographId == _partographId)
            {
                MainThread.BeginInvokeOnMainThread(() => RefreshAlerts());
            }
        }

        /// <summary>
        /// Refreshes the alerts display for the current partograph
        /// </summary>
        public void RefreshAlerts()
        {
            if (_monitoringService == null || _partographId == null) return;

            var alerts = _monitoringService.GetAlertsForPartograph(_partographId.Value)
                .Where(a => !a.IsAcknowledged)
                .OrderByDescending(a => a.Severity)
                .ThenByDescending(a => a.EscalationLevel)
                .ToList();

            // Update visibility
            MainContainer.IsVisible = alerts.Any();

            if (!alerts.Any()) return;

            // Update count badge
            AlertCountLabel.Text = alerts.Count.ToString();
            CountBadge.BackgroundColor = alerts.Any(a => a.Severity == AlertSeverity.Critical)
                ? Color.FromArgb("#EF5350")
                : Color.FromArgb("#FF9800");

            // Update summary
            var criticalCount = alerts.Count(a => a.Severity == AlertSeverity.Critical);
            var warningCount = alerts.Count(a => a.Severity == AlertSeverity.Warning);
            var escalatedCount = alerts.Count(a => a.EscalationLevel > 0);

            var summaryParts = new List<string>();
            if (criticalCount > 0) summaryParts.Add($"{criticalCount} critical");
            if (warningCount > 0) summaryParts.Add($"{warningCount} warning");
            if (escalatedCount > 0) summaryParts.Add($"{escalatedCount} escalated");
            AlertSummaryLabel.Text = string.Join(", ", summaryParts);

            // Update border color based on severity
            MainContainer.Stroke = alerts.Any(a => a.Severity == AlertSeverity.Critical)
                ? Color.FromArgb("#EF5350")
                : Color.FromArgb("#FF9800");
            MainContainer.BackgroundColor = alerts.Any(a => a.Severity == AlertSeverity.Critical)
                ? Color.FromArgb("#FFEBEE")
                : Color.FromArgb("#FFF8E1");

            // Build alert items
            AlertsContainer.Children.Clear();
            var itemsToShow = _isExpanded ? alerts : alerts.Take(COLLAPSED_MAX_ITEMS);

            foreach (var alert in itemsToShow)
            {
                var alertItem = CreateAlertItem(alert);
                AlertsContainer.Children.Add(alertItem);
            }

            // Show expand button if more items
            ExpandCollapseButton.IsVisible = alerts.Count > COLLAPSED_MAX_ITEMS;
            ExpandCollapseButton.Text = _isExpanded ? "Show Less" : $"Show {alerts.Count - COLLAPSED_MAX_ITEMS} More";
        }

        private Border CreateAlertItem(NotificationItem alert)
        {
            var severityColor = alert.Severity switch
            {
                AlertSeverity.Critical => "#EF5350",
                AlertSeverity.Warning => "#FF9800",
                _ => "#2196F3"
            };

            var border = new Border
            {
                BackgroundColor = Color.FromArgb(alert.Severity == AlertSeverity.Critical ? "#FFCDD2" : "#FFE0B2"),
                Padding = new Thickness(10, 8),
                StrokeThickness = 0
            };
            border.StrokeShape = new RoundRectangle { CornerRadius = 8 };

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 10
            };

            // Icon
            var iconLabel = new Label
            {
                Text = alert.Type == NotificationType.MeasurementDue ? "⏰" : "🏥",
                FontSize = 18,
                VerticalOptions = LayoutOptions.Center
            };
            grid.Add(iconLabel, 0);

            // Content
            var contentStack = new VerticalStackLayout
            {
                Spacing = 2,
                VerticalOptions = LayoutOptions.Center
            };

            var titleStack = new HorizontalStackLayout { Spacing = 6 };
            titleStack.Add(new Label
            {
                Text = alert.Title,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb(severityColor),
                VerticalOptions = LayoutOptions.Center
            });

            // Escalation badge
            if (alert.EscalationLevel > 0)
            {
                var escalationBadge = new Border
                {
                    BackgroundColor = alert.EscalationLevel == 3 ? Color.FromArgb("#C62828") : Color.FromArgb("#E65100"),
                    Padding = new Thickness(4, 2),
                    StrokeThickness = 0,
                    VerticalOptions = LayoutOptions.Center
                };
                escalationBadge.StrokeShape = new RoundRectangle { CornerRadius = 4 };
                escalationBadge.Content = new Label
                {
                    Text = alert.EscalationBadge,
                    FontSize = 8,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                };
                titleStack.Add(escalationBadge);
            }

            contentStack.Add(titleStack);
            contentStack.Add(new Label
            {
                Text = alert.Message,
                FontSize = 11,
                TextColor = Color.FromArgb("#666"),
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1
            });
            grid.Add(contentStack, 1);

            // Quick Action Button
            if (alert.ShowQuickAction)
            {
                var actionButton = new Button
                {
                    Text = "Record",
                    FontSize = 10,
                    Padding = new Thickness(8, 4),
                    BackgroundColor = Color.FromArgb("#2196F3"),
                    TextColor = Colors.White,
                    CornerRadius = 4,
                    VerticalOptions = LayoutOptions.Center,
                    HeightRequest = 28
                };
                actionButton.Clicked += async (s, e) => await OnQuickActionAsync(alert);
                grid.Add(actionButton, 2);
            }
            else
            {
                var timeLabel = new Label
                {
                    Text = alert.TimeAgo,
                    FontSize = 10,
                    TextColor = Color.FromArgb("#999"),
                    VerticalOptions = LayoutOptions.Center
                };
                grid.Add(timeLabel, 2);
            }

            // Tap gesture to acknowledge
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) => await OnAlertTappedAsync(alert);
            border.GestureRecognizers.Add(tapGesture);

            border.Content = grid;
            return border;
        }

        private async Task OnQuickActionAsync(NotificationItem alert)
        {
            if (_monitoringService == null) return;

            try
            {
                // Acknowledge the alert
                _monitoringService.AcknowledgeNotification(alert.Id);

                // The parent PartographPage will handle opening the modal
                // Send a message or event that the page can listen to
                MessagingCenter.Send(this, "OpenMeasurementModal", alert.QuickActionRoute);

                RefreshAlerts();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Quick action error: {ex.Message}");
            }
        }

        private async Task OnAlertTappedAsync(NotificationItem alert)
        {
            if (_monitoringService == null) return;

            var acknowledged = await Shell.Current.DisplayAlert(
                "Acknowledge Alert",
                $"Acknowledge this alert?\n\n{alert.Title}\n{alert.Message}",
                "Acknowledge",
                "Cancel");

            if (acknowledged)
            {
                _monitoringService.AcknowledgeNotification(alert.Id);
                await AppShell.DisplayToastAsync("Alert acknowledged");
                RefreshAlerts();
            }
        }

        private void OnExpandCollapse(object? sender, EventArgs e)
        {
            _isExpanded = !_isExpanded;
            RefreshAlerts();
        }

        /// <summary>
        /// Cleanup subscriptions
        /// </summary>
        public void Dispose()
        {
            if (_monitoringService != null)
            {
                _monitoringService.NotificationCountChanged -= OnNotificationCountChanged;
                _monitoringService.NotificationAdded -= OnNotificationAdded;
                _monitoringService.AlertEscalated -= OnAlertEscalated;
            }
        }
    }
}
