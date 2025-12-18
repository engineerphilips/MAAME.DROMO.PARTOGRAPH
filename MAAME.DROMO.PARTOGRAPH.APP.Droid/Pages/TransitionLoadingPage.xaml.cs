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

        /// <summary>
        /// Shows the loading page with step-by-step progress tracking during page transition.
        /// Executes preparatory operations with progress updates before creating the target page.
        /// </summary>
        /// <param name="createPage">Function to create the target page</param>
        /// <param name="message">Main loading message</param>
        /// <param name="operations">Array of named operations to execute with progress tracking</param>
        public static async Task TransitionWithProgressAsync(
            Func<Page> createPage,
            string message,
            params (string stepName, Func<Task> operation)[] operations)
        {
            var loadingPage = new TransitionLoadingPage(message, "Initializing...");
            Application.Current!.MainPage = loadingPage;

            // Stop indeterminate animation - we'll show actual progress
            loadingPage.StopAnimations();
            loadingPage.SetProgress(0);

            // Allow UI to render
            await Task.Delay(50);

            try
            {
                var totalSteps = operations.Length + 1; // +1 for page creation step
                var completedSteps = 0;

                // Execute each preparatory operation with progress
                foreach (var (stepName, operation) in operations)
                {
                    var progress = (double)completedSteps / totalSteps;
                    loadingPage.SetProgress(progress);
                    loadingPage.UpdateMessage(message, stepName);

                    await operation();
                    completedSteps++;

                    // Small delay between steps for visual feedback
                    await Task.Delay(50);
                }

                // Final step: Create the target page
                loadingPage.SetProgress((double)completedSteps / totalSteps);
                loadingPage.UpdateMessage(message, "Preparing interface...");

                var targetPage = createPage();
                completedSteps++;

                // Show completion
                loadingPage.SetProgress(1.0);
                loadingPage.UpdateMessage(message, "Ready!");
                await Task.Delay(150);

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
        /// Shows the loading page with step-by-step progress tracking during page transition.
        /// Returns a result from the operations that can be used by the target page.
        /// </summary>
        /// <typeparam name="T">Type of result from preparatory operations</typeparam>
        /// <param name="createPageWithResult">Function to create the target page using the result</param>
        /// <param name="message">Main loading message</param>
        /// <param name="prepareData">Async function that prepares data and reports progress</param>
        public static async Task TransitionWithDataPreloadAsync<T>(
            Func<T, Page> createPageWithResult,
            string message,
            Func<Action<double, string>, Task<T>> prepareData)
        {
            var loadingPage = new TransitionLoadingPage(message, "Initializing...");
            Application.Current!.MainPage = loadingPage;

            // Stop indeterminate animation - we'll show actual progress
            loadingPage.StopAnimations();
            loadingPage.SetProgress(0);

            // Allow UI to render
            await Task.Delay(50);

            try
            {
                // Execute data preparation with progress reporting
                var progressReporter = new Action<double, string>((progress, stepMessage) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        loadingPage.SetProgress(progress);
                        loadingPage.UpdateMessage(message, stepMessage);
                    });
                });

                var result = await prepareData(progressReporter);

                // Final step: Create the target page with prepared data
                loadingPage.SetProgress(0.9);
                loadingPage.UpdateMessage(message, "Preparing interface...");
                await Task.Delay(50);

                var targetPage = createPageWithResult(result);

                // Show completion
                loadingPage.SetProgress(1.0);
                loadingPage.UpdateMessage(message, "Ready!");
                await Task.Delay(150);

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
