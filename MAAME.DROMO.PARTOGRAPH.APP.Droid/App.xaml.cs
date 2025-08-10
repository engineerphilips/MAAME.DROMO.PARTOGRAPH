using System.Runtime.Versioning;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        [SupportedOSPlatform("android21.0")]
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var isAuthenticated = Preferences.Get("IsAuthenticated", false);

            if (isAuthenticated)
            {
                // If authenticated, navigate to the main shell
                return new Window(new AppShell());
            }

            // If not authenticated, show login page
            return new Window(new LoginPage());
        }

        protected override async void OnStart()
        {
            base.OnStart();

            // Initialize database if needed
            await InitializeDatabase();
        }

        private async Task InitializeDatabase()
        {
            try
            {
                // Check if this is first run
                var isFirstRun = !Preferences.ContainsKey("DatabaseInitialized");

                if (isFirstRun)
                {
                    // Initialize with sample data if needed
                    var serviceProvider = Handler?.MauiContext?.Services;
                    if (serviceProvider != null)
                    {
                        var seedService = serviceProvider.GetService<SeedDataService>();
                        if (seedService != null)
                        {
                            await seedService.LoadSamplePartographData();
                        }
                    }

                    Preferences.Set("DatabaseInitialized", true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
            }
        }

    }
}