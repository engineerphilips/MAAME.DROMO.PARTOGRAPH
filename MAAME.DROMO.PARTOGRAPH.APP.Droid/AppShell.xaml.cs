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
            //Routing.RegisterRoute("splash", typeof(SplashPage));
            Routing.RegisterRoute("login", typeof(LoginPage));
            Routing.RegisterRoute("main", typeof(MainPage));
            //Routing.RegisterRoute("about", typeof(AboutPage));
            //Routing.RegisterRoute("signup", typeof(SignUpPage));
            //Routing.RegisterRoute("forgotpassword", typeof(ForgotPasswordPage));
            //Routing.RegisterRoute("patient", typeof(PatientPage));
            //Routing.RegisterRoute("patientmonitoring", typeof(PatientMonitoringPage));

            //Routing.RegisterRoute("PatientDetails", typeof(PatientDetailsPage));
            //Routing.RegisterRoute("AddPatient", typeof(AddPatientPage));
            //Routing.RegisterRoute("EditProfile", typeof(EditProfilePage));
            //Routing.RegisterRoute("Settings", typeof(SettingsPage));
            //Routing.RegisterRoute("PartographForm", typeof(PartographFormPage));
            //Routing.RegisterRoute("MedicalHistory", typeof(MedicalHistoryPage));
        }


        public static async Task DisplaySnackbarAsync(string message)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = Color.FromArgb("#FF3300"),
                TextColor = Colors.White,
                ActionButtonTextColor = Colors.Yellow,
                CornerRadius = new CornerRadius(0),
                Font = Microsoft.Maui.Font.SystemFontOfSize(18),
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

            var toast = Toast.Make(message, textSize: 18);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await toast.Show(cts.Token);
        }

        private void SfSegmentedControl_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
        {
            Application.Current!.UserAppTheme = e.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
        }
    }
}
