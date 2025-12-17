using CommunityToolkit.Maui.Views;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Controls
{
    public partial class LoadingPopup : Popup
    {
        private CancellationTokenSource? _animationCts;

        public LoadingPopup()
        {
            InitializeComponent();
            Opened += (s,e) => StartAnimation();
            Closed += (s,e) => StopAnimation();
        }

        public LoadingPopup(string message, string subMessage = "Please wait...")
        {
            InitializeComponent();
            MessageLabel.Text = message;
            SubMessageLabel.Text = subMessage;
        }

        //protected override void OnOpened()
        //{
        //    base.OnOpened();
        //    StartAnimation();
        //}

        //protected override void OnClosed(object? result)
        //{
        //    base.OnClosed(result);
        //    StopAnimation();
        //}

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
