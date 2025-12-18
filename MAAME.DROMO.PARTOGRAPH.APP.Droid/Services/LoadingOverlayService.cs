using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Views;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages.Controls;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    /// <summary>
    /// Service to manage global loading overlay state during page transitions.
    /// Provides a smooth loading experience with progress indication using popups.
    /// </summary>
    public interface ILoadingOverlayService : INotifyPropertyChanged
    {
        bool IsLoading { get; }
        string LoadingMessage { get; }
        double Progress { get; }
        bool IsIndeterminate { get; }

        void Show(string message = "Loading...", bool isIndeterminate = true);
        void ShowWithProgress(string message = "Loading data...", string subMessage = "Please wait...");
        void UpdateProgress(double progress, string? message = null, string? subMessage = null);
        void Hide();
        Task ShowAsync(Func<Task> action, string message = "Loading...");
        Task<T> ShowAsync<T>(Func<Task<T>> action, string message = "Loading...");
        Task ShowWithProgressAsync(Func<IProgress<(double progress, string? message)>, Task> action, string message = "Loading data...");
        Task<T> ShowWithProgressAsync<T>(Func<IProgress<(double progress, string? message)>, Task<T>> action, string message = "Loading data...");
    }

    public class LoadingOverlayService : ILoadingOverlayService
    {
        private bool _isLoading;
        private string _loadingMessage = "Loading...";
        private double _progress;
        private bool _isIndeterminate = true;
        private LoadingPopup? _currentPopup;
        private readonly object _lock = new();

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public string LoadingMessage
        {
            get => _loadingMessage;
            private set => SetProperty(ref _loadingMessage, value);
        }

        public double Progress
        {
            get => _progress;
            private set => SetProperty(ref _progress, value);
        }

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            private set => SetProperty(ref _isIndeterminate, value);
        }

        public void Show(string message = "Loading...", bool isIndeterminate = true)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                lock (_lock)
                {
                    LoadingMessage = message;
                    IsIndeterminate = isIndeterminate;
                    Progress = 0;
                    IsLoading = true;

                    // Show popup if not already showing
                    if (_currentPopup == null)
                    {
                        ShowPopup(message, "Please wait...", showProgress: false);
                    }
                    else
                    {
                        _currentPopup.UpdateMessage(message);
                    }
                }
            });
        }

        public void ShowWithProgress(string message = "Loading data...", string subMessage = "Please wait...")
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                lock (_lock)
                {
                    LoadingMessage = message;
                    IsIndeterminate = false;
                    Progress = 0;
                    IsLoading = true;

                    // Show popup with progress bar if not already showing
                    if (_currentPopup == null)
                    {
                        ShowPopup(message, subMessage, showProgress: true);
                    }
                    else
                    {
                        _currentPopup.SetProgressMode(true);
                        _currentPopup.UpdateProgress(0, message, subMessage);
                    }
                }
            });
        }

        private void ShowPopup(string message, string subMessage, bool showProgress)
        {
            try
            {
                var page = Application.Current?.MainPage;
                if (page == null) return;

                // If we're in a Shell, get the current page
                if (page is Shell shell)
                {
                    page = shell.CurrentPage ?? page;
                }

                _currentPopup = new LoadingPopup(message, subMessage, showProgress);
                page.ShowPopup(_currentPopup);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadingOverlayService: Error showing popup - {ex.Message}");
            }
        }

        public void UpdateProgress(double progress, string? message = null, string? subMessage = null)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Progress = Math.Clamp(progress, 0, 1);
                if (message != null)
                {
                    LoadingMessage = message;
                }
                IsIndeterminate = false;

                // Update the popup with progress
                _currentPopup?.UpdateProgress(Progress, message, subMessage);
            });
        }

        public void Hide()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                lock (_lock)
                {
                    IsLoading = false;
                    Progress = 0;
                    LoadingMessage = "Loading...";
                    IsIndeterminate = true;

                    // Close popup
                    if (_currentPopup != null)
                    {
                        try
                        {
                            _currentPopup.Close();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"LoadingOverlayService: Error closing popup - {ex.Message}");
                        }
                        finally
                        {
                            _currentPopup = null;
                        }
                    }
                }
            });
        }

        public async Task ShowAsync(Func<Task> action, string message = "Loading...")
        {
            try
            {
                Show(message);
                await action();
            }
            finally
            {
                Hide();
            }
        }

        public async Task<T> ShowAsync<T>(Func<Task<T>> action, string message = "Loading...")
        {
            try
            {
                Show(message);
                return await action();
            }
            finally
            {
                Hide();
            }
        }

        public async Task ShowWithProgressAsync(Func<IProgress<(double progress, string? message)>, Task> action, string message = "Loading data...")
        {
            try
            {
                ShowWithProgress(message);

                var progress = new Progress<(double progress, string? message)>(report =>
                {
                    UpdateProgress(report.progress, report.message);
                });

                await action(progress);
            }
            finally
            {
                Hide();
            }
        }

        public async Task<T> ShowWithProgressAsync<T>(Func<IProgress<(double progress, string? message)>, Task<T>> action, string message = "Loading data...")
        {
            try
            {
                ShowWithProgress(message);

                var progress = new Progress<(double progress, string? message)>(report =>
                {
                    UpdateProgress(report.progress, report.message);
                });

                return await action(progress);
            }
            finally
            {
                Hide();
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}
