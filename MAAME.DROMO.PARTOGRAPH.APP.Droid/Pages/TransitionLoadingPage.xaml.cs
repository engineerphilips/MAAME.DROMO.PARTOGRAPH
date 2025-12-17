namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages
{
    public partial class TransitionLoadingPage : ContentPage
    {
        private CancellationTokenSource? _animationCts;

        public TransitionLoadingPage()
        {
            InitializeComponent();
        }

        public TransitionLoadingPage(string message, string subMessage = "Please wait...")
        {
            InitializeComponent();
            LoadingMessageLabel.Text = message;
            SubMessageLabel.Text = subMessage;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            StartAnimations();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopAnimations();
        }

        private void StartAnimations()
        {
            _animationCts?.Cancel();
            _animationCts = new CancellationTokenSource();

            // Logo pulse animation
            var pulseAnimation = new Animation(v => LogoBorder.Scale = v, 1.0, 1.08);
            pulseAnimation.Commit(LogoBorder, "PulseAnimation", 16, 800, Easing.SinInOut,
                (v, c) => LogoBorder.Scale = 1.0, () => !_animationCts.IsCancellationRequested);

            // Progress bar animation (indeterminate-like)
            AnimateProgressBar();
        }

        private async void AnimateProgressBar()
        {
            try
            {
                while (!_animationCts?.IsCancellationRequested ?? false)
                {
                    await LoadingProgressBar.ProgressTo(0.7, 800, Easing.CubicInOut);
                    await LoadingProgressBar.ProgressTo(0.3, 800, Easing.CubicInOut);
                }
            }
            catch (TaskCanceledException)
            {
                // Animation was cancelled
            }
        }

        private void StopAnimations()
        {
            _animationCts?.Cancel();
            LogoBorder.CancelAnimations();
            LogoBorder.Scale = 1.0;
        }

        public void UpdateMessage(string message, string? subMessage = null)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadingMessageLabel.Text = message;
                if (subMessage != null)
                {
                    SubMessageLabel.Text = subMessage;
                }
            });
        }

        public void SetProgress(double progress)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadingProgressBar.Progress = progress;
            });
        }

        /// <summary>
        /// Shows the loading page during a page transition and executes the navigation action.
        /// </summary>
        public static async Task TransitionToAsync(Func<Page> createPage, string message = "Loading...", string subMessage = "Please wait...")
        {
            var loadingPage = new TransitionLoadingPage(message, subMessage);
            Application.Current!.MainPage = loadingPage;

            // Allow UI to render
            await Task.Delay(50);

            try
            {
                // Create the target page
                loadingPage.UpdateMessage(message, "Preparing content...");
                var targetPage = createPage();

                // Small delay for smooth transition
                await Task.Delay(150);

                loadingPage.UpdateMessage(message, "Almost ready...");
                await Task.Delay(100);

                // Navigate to target page
                Application.Current.MainPage = targetPage;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Transition error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Shows the loading page during an async page transition.
        /// </summary>
        public static async Task TransitionToAsync(Func<Task<Page>> createPageAsync, string message = "Loading...", string subMessage = "Please wait...")
        {
            var loadingPage = new TransitionLoadingPage(message, subMessage);
            Application.Current!.MainPage = loadingPage;

            // Allow UI to render
            await Task.Delay(50);

            try
            {
                // Create the target page asynchronously
                loadingPage.UpdateMessage(message, "Preparing content...");
                var targetPage = await createPageAsync();

                // Small delay for smooth transition
                await Task.Delay(100);

                loadingPage.UpdateMessage(message, "Almost ready...");
                await Task.Delay(50);

                // Navigate to target page
                Application.Current.MainPage = targetPage;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Transition error: {ex.Message}");
                throw;
            }
        }
    }
}
