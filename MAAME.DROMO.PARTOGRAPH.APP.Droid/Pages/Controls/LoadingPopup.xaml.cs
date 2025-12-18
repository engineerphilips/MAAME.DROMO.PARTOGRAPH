using CommunityToolkit.Maui.Views;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Controls
{
    public partial class LoadingPopup : Popup
    {
        private CancellationTokenSource? _animationCts;
        private bool _showProgress;
        private const double MaxProgressWidth = 220;

        public LoadingPopup()
        {
            InitializeComponent();
            Opened += (s, e) => StartAnimation();
            Closed += (s, e) => StopAnimation();
        }

        public LoadingPopup(string message, string subMessage = "Please wait...", bool showProgress = false)
        {
            InitializeComponent();
            MessageLabel.Text = message;
            SubMessageLabel.Text = subMessage;
            _showProgress = showProgress;

            if (showProgress)
            {
                SetProgressMode(true);
            }

            Opened += (s, e) => StartAnimation();
            Closed += (s, e) => StopAnimation();
        }

        private void StartAnimation()
        {
            _animationCts?.Cancel();
            _animationCts = new CancellationTokenSource();

            // Subtle pulse animation on the icon
            var animation = new Animation(v => IconBorder.Scale = v, 1.0, 1.1);
            animation.Commit(IconBorder, "PulseAnimation", 16, 800, Easing.SinInOut,
                (v, c) => IconBorder.Scale = 1.0, () => !(_animationCts?.IsCancellationRequested ?? true));
        }

        private void StopAnimation()
        {
            _animationCts?.Cancel();
            IconBorder.CancelAnimations();
            IconBorder.Scale = 1.0;
        }

        /// <summary>
        /// Switches between progress bar mode and indeterminate (spinner) mode.
        /// </summary>
        public void SetProgressMode(bool showProgress)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _showProgress = showProgress;
                ProgressContainer.IsVisible = showProgress;
                IndeterminateIndicator.IsVisible = !showProgress;
                IndeterminateIndicator.IsRunning = !showProgress;

                if (showProgress)
                {
                    UpdateProgress(0);
                }
            });
        }

        /// <summary>
        /// Updates the progress bar value (0.0 to 1.0).
        /// </summary>
        public void UpdateProgress(double progress, string? message = null, string? subMessage = null)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Ensure progress is clamped between 0 and 1
                progress = Math.Clamp(progress, 0, 1);

                // Update progress bar width with animation
                var targetWidth = progress * MaxProgressWidth;
                ProgressFill.Animate("ProgressAnimation",
                    new Animation(v => ProgressFill.WidthRequest = v, ProgressFill.WidthRequest, targetWidth),
                    length: 150,
                    easing: Easing.CubicOut);

                // Update percentage label
                var percent = (int)(progress * 100);
                ProgressPercentLabel.Text = $"{percent}%";

                // Update messages if provided
                if (message != null)
                {
                    MessageLabel.Text = message;
                }

                if (subMessage != null)
                {
                    SubMessageLabel.Text = subMessage;
                }
            });
        }

        public void UpdateMessage(string message, string? subMessage = null)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessageLabel.Text = message;
                if (subMessage != null)
                {
                    SubMessageLabel.Text = subMessage;
                }
            });
        }
    }
}
