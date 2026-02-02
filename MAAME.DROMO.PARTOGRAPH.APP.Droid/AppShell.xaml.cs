using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid
{
    public partial class AppShell : Shell
    {
        private readonly ILoadingOverlayService? _loadingService;
        private readonly BackgroundSyncService? _backgroundSyncService;
        private readonly IConnectivityService? _connectivityService;
        private readonly ILogger<AppShell>? _logger;

        public AppShell()
        {
            InitializeComponent();
            var currentTheme = Application.Current!.RequestedTheme;
            ThemeSegmentedControl.SelectedIndex = currentTheme == AppTheme.Light ? 0 : 1;

            // Set BindingContext for AppShell
            var appShellModel = IPlatformApplication.Current!.Services.GetService<PageModels.AppShellModel>();
            if (appShellModel != null)
            {
                BindingContext = appShellModel;
            }

            // Get loading overlay service for navigation transitions
            _loadingService = IPlatformApplication.Current!.Services.GetService<ILoadingOverlayService>();

            // Get sync services
            _backgroundSyncService = IPlatformApplication.Current!.Services.GetService<BackgroundSyncService>();
            _connectivityService = IPlatformApplication.Current!.Services.GetService<IConnectivityService>();
            _logger = IPlatformApplication.Current!.Services.GetService<ILogger<AppShell>>();

            // Subscribe to navigation events for loading indicator
            Navigating += OnShellNavigating;
            Navigated += OnShellNavigated;

            // Register additional routes
            RegisterRoutes();

            // Initialize sync services after shell is loaded
            InitializeSyncServicesAsync();
        }

        /// <summary>
        /// Initializes background sync services when the app shell loads.
        /// This starts the periodic sync timer and triggers an initial sync if connected.
        /// </summary>
        private async void InitializeSyncServicesAsync()
        {
            try
            {
                // Small delay to allow app to fully initialize
                await Task.Delay(500);

                if (_backgroundSyncService == null)
                {
                    _logger?.LogWarning("BackgroundSyncService not available");
                    return;
                }

                // Start the background sync service (enables periodic sync timer)
                _backgroundSyncService.Start();
                _logger?.LogInformation("Background sync service started");

                // Check if we have connectivity and trigger initial sync
                if (_connectivityService?.IsConnected == true)
                {
                    _logger?.LogInformation("Network available, triggering initial sync");

                    // Trigger initial sync in background (don't block UI)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _backgroundSyncService.TriggerSyncAsync();
                            _logger?.LogInformation("Initial sync completed");

                            // Show toast on main thread
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await DisplayToastAsync("Sync completed");
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Initial sync failed");
                        }
                    });
                }
                else
                {
                    _logger?.LogInformation("No network connectivity, sync will start when connected");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize sync services");
            }
        }

        private void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
        {
            // Show loading for partograph and patient-related navigations (heavy pages)
            var targetRoute = e.Target?.Location?.OriginalString?.ToLowerInvariant() ?? "";

            if (targetRoute.Contains("partograph") ||
                targetRoute.Contains("patient") ||
                targetRoute.Contains("birthoutcome") ||
                targetRoute.Contains("babydetails") ||
                targetRoute.Contains("partographchart"))
            {
                var message = GetLoadingMessage(targetRoute);
                _loadingService?.Show(message);
            }
        }
        
        private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            // Hide loading after navigation completes with a small delay for smooth transition
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100); // Allow page to render
                _loadingService?.Hide();
            });
        }

        private static string GetLoadingMessage(string route)
        {
            if (route.Contains("partographdetails")) return "Loading Partograph Details...";
            if (route.Contains("secondpartograph")) return "Loading Second Stage...";
            if (route.Contains("thirdpartograph")) return "Loading Third Stage...";
            if (route.Contains("fourthpartograph")) return "Loading Fourth Stage...";
            if (route.Contains("partograph")) return "Loading Partograph...";
            if (route.Contains("birthoutcome")) return "Loading Birth Outcome...";
            if (route.Contains("babydetails")) return "Loading Baby Details...";
            if (route.Contains("patient")) return "Loading Patient Data...";
            if (route.Contains("partographchart")) return "Loading Partograph Chart...";
            
            return "Loading...";
        }

        private static void RegisterRoutes()
        {
            // Authentication routes
            Routing.RegisterRoute("login", typeof(LoginPage));

            // Main routes
            Routing.RegisterRoute("home", typeof(HomePage));
            Routing.RegisterRoute("pendingpatients", typeof(PendingPatientsPage));
            Routing.RegisterRoute("activepatients", typeof(ActivePatientsPage));
            Routing.RegisterRoute("completedpatients", typeof(CompletedPatientsPage));

            // Patient management routes
            Routing.RegisterRoute("patient", typeof(PatientPage));
            //Routing.RegisterRoute("newpatient", typeof(PatientPage));
            Routing.RegisterRoute("patienthub", typeof(PatientHubPage));

            // Partograph routes
            Routing.RegisterRoute("partograph", typeof(PartographPage));
            Routing.RegisterRoute("secondpartograph", typeof(SecondStagePartographPage));
            Routing.RegisterRoute("thirdpartograph", typeof(ThirdStagePartographPage));
            Routing.RegisterRoute("fourthpartograph", typeof(FourthStagePartographPage));
            Routing.RegisterRoute("partographentry", typeof(PartographEntryPage));
            Routing.RegisterRoute("partographchart", typeof(PartographChartPage));
            //Routing.RegisterRoute("vitalsigns", typeof(VitalSignsPage));
            //Routing.RegisterRoute("medicalnote", typeof(MedicalNotePage));

            // Settings and profile routes
            Routing.RegisterRoute("settings", typeof(SettingsPage));
            Routing.RegisterRoute("help", typeof(HelpPage));
            //Routing.RegisterRoute("profile", typeof(ProfilePage));
            //Routing.RegisterRoute("about", typeof(AboutPage));

            // User management routes
            Routing.RegisterRoute("signup", typeof(SignupPage));
        }

        public static async Task DisplaySnackbarAsync(string message)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = Color.FromArgb("#2196F3"),
                TextColor = Colors.White,
                ActionButtonTextColor = Colors.Yellow,
                CornerRadius = new CornerRadius(8),
                Font = Microsoft.Maui.Font.SystemFontOfSize(16),
                ActionButtonFont = Microsoft.Maui.Font.SystemFontOfSize(14)
            };

            var snackbar = Snackbar.Make(message, visualOptions: snackbarOptions);

            await snackbar.Show(cancellationTokenSource.Token);
        }

        public static async Task DisplayToastAsync(string message)
        {
            // Toast is currently not working in MCT on Windows
            if (OperatingSystem.IsWindows())
                return;

            var toast = Toast.Make(message, textSize: 16);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            await toast.Show(cts.Token);
        }

        private void SfSegmentedControl_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
        {
            Application.Current!.UserAppTheme = e.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
        }
    }
}
