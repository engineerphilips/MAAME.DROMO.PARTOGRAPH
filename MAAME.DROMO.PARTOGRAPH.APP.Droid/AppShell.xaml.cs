using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            var currentTheme = Application.Current!.RequestedTheme;
            ThemeSegmentedControl.SelectedIndex = currentTheme == AppTheme.Light ? 0 : 1;

            // Register additional routes
            RegisterRoutes();
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
            Routing.RegisterRoute("newpatient", typeof(PatientPage));

            // Partograph routes
            Routing.RegisterRoute("partograph", typeof(PartographPage));
            Routing.RegisterRoute("secondpartograph", typeof(SecondStagePartographPage));
            Routing.RegisterRoute("partographentry", typeof(PartographEntryPage));
            //Routing.RegisterRoute("vitalsigns", typeof(VitalSignsPage));
            //Routing.RegisterRoute("medicalnote", typeof(MedicalNotePage));

            // Settings and profile routes
            Routing.RegisterRoute("settings", typeof(SettingsPage));
            //Routing.RegisterRoute("profile", typeof(ProfilePage));
            //Routing.RegisterRoute("about", typeof(AboutPage));
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
