namespace MAAME.DROMO.PARTOGRAPH.APP.Droid
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var isAuthenticated = false;
            // Here you can check if the user is authenticated
            // For example, you might check a token or session state
            if (isAuthenticated)
            {
                // If authenticated, navigate to the main page
                return new Window(new AppShell());
            }
            // If not authenticated, navigate to the login page
            // You can also use a splash screen or initial loading page here
            // For simplicity, we will just return the AppShell
            // In a real application, you might want to show a splash screen or loading page first
            //return new Window(new SplashPage());
            // For now, we will just return the AppShell directly
            // This will be the main entry point of your application
            // You can replace this with your actual login page or splash screen
            return new Window(new LoginPage());   

            //return new Window(new AppShell());
        }

        protected override async void OnStart()
        {
            base.OnStart();

            // Check authentication and navigate
            //await CheckAuthenticationAndNavigateAsync();
        }

        private async Task CheckAuthenticationAndNavigateAsync()
        {
            try
            {
                // Show startup page briefly
                //await Shell.Current.GoToAsync("//startup");

                // Check authentication
                var isAuthenticated = false;// await _authService.IsAuthenticatedAsync();

                // Small delay for better UX
                await Task.Delay(1500);

                if (isAuthenticated)
                {
                    await Shell.Current.GoToAsync("//main");
                }
                else
                {
                    await Shell.Current.GoToAsync("//login");
                }
            }
            catch (Exception ex)
            {
                // Log error and navigate to login
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.GoToAsync("//login");
            }
        }

    }
}