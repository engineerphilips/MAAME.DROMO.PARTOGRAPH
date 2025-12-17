using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Controls
{
    public partial class LoadingOverlay : ContentView
    {
        private ILoadingOverlayService? _loadingService;
        private CancellationTokenSource? _animationCts;

        public static readonly BindableProperty IsLoadingProperty =
            BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(LoadingOverlay), false,
                propertyChanged: OnIsLoadingChanged);

        public static readonly BindableProperty LoadingMessageProperty =
            BindableProperty.Create(nameof(LoadingMessage), typeof(string), typeof(LoadingOverlay), "Loading...");

        public static readonly BindableProperty ProgressProperty =
            BindableProperty.Create(nameof(Progress), typeof(double), typeof(LoadingOverlay), 0.0);

        public static readonly BindableProperty IsIndeterminateProperty =
            BindableProperty.Create(nameof(IsIndeterminate), typeof(bool), typeof(LoadingOverlay), true);

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public string LoadingMessage
        {
            get => (string)GetValue(LoadingMessageProperty);
            set => SetValue(LoadingMessageProperty, value);
        }

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        public LoadingOverlay()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Bind this overlay to the LoadingOverlayService for automatic state management.
        /// </summary>
        public void BindToService(ILoadingOverlayService loadingService)
        {
            // Unsubscribe from previous service
            if (_loadingService != null)
            {
                _loadingService.PropertyChanged -= OnLoadingServicePropertyChanged;
            }

            _loadingService = loadingService;

            if (_loadingService != null)
            {
                _loadingService.PropertyChanged += OnLoadingServicePropertyChanged;
                SyncWithService();
            }
        }

        private void OnLoadingServicePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(SyncWithService);
        }

        private void SyncWithService()
        {
            if (_loadingService == null) return;

            IsLoading = _loadingService.IsLoading;
            LoadingMessage = _loadingService.LoadingMessage;
            Progress = _loadingService.Progress;
            IsIndeterminate = _loadingService.IsIndeterminate;
        }

        private static void OnIsLoadingChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LoadingOverlay overlay && newValue is bool isLoading)
            {
                if (isLoading)
                {
                    overlay.StartAnimation();
                }
                else
                {
                    overlay.StopAnimation();
                }
            }
        }

        private void StartAnimation()
        {
            _animationCts?.Cancel();
            _animationCts = new CancellationTokenSource();

            // Subtle pulse animation on the icon
            var animation = new Animation(v => iconLabel.Scale = v, 1.0, 1.1);
            animation.Commit(iconLabel, "PulseAnimation", 16, 800, Easing.SinInOut,
                (v, c) => iconLabel.Scale = 1.0, () => IsLoading);
        }

        private void StopAnimation()
        {
            _animationCts?.Cancel();
            iconLabel.CancelAnimations();
            iconLabel.Scale = 1.0;
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            // Auto-bind to service if available
            if (Handler != null && _loadingService == null)
            {
                var service = IPlatformApplication.Current?.Services?.GetService<ILoadingOverlayService>();
                if (service != null)
                {
                    BindToService(service);
                }
            }
        }
    }
}
