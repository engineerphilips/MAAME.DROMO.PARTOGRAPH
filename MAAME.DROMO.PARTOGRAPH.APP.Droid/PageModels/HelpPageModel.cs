using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public class HelpPageModel : INotifyPropertyChanged
    {
        public HelpPageModel()
        {
            // Initialize commands
            ReportIssueCommand = new Command(async () => await ReportIssueAsync());
            ViewIssuesCommand = new Command(async () => await ViewIssuesAsync());
            ContactSupportCommand = new Command(async () => await ContactSupportAsync());
            OpenTermsCommand = new Command(async () => await OpenTermsAsync());
            OpenPrivacyCommand = new Command(async () => await OpenPrivacyAsync());
        }

        #region Properties

        // Get app version from assembly
        public string AppVersion
        {
            get
            {
                var version = AppInfo.Current.VersionString;
                return $"Maame.Dromo Partograph App v{version}";
            }
        }

        public string BuildNumber
        {
            get
            {
                var build = AppInfo.Current.BuildString;
                return $"Build {build}";
            }
        }

        #endregion

        #region Commands

        public ICommand ReportIssueCommand { get; }
        public ICommand ViewIssuesCommand { get; }
        public ICommand ContactSupportCommand { get; }
        public ICommand OpenTermsCommand { get; }
        public ICommand OpenPrivacyCommand { get; }

        #endregion

        #region Command Methods

        private async Task ReportIssueAsync()
        {
            try
            {
                // Open email client to report issue
                var message = new EmailMessage
                {
                    Subject = "Maame.Dromo Partograph - Issue Report",
                    Body = "Please describe the issue you're experiencing:\n\n",
                    To = new List<string> { "support@maamedromo.com" }
                };

                await Email.Default.ComposeAsync(message);
            }
            catch (Exception ex)
            {
                await AppShell.DisplayToastAsync("Unable to open email client");
            }
        }

        private async Task ViewIssuesAsync()
        {
            try
            {
                // This could navigate to a page showing past issues or open a web page
                await Browser.Default.OpenAsync("https://support.maamedromo.com/issues",
                    BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                await AppShell.DisplayToastAsync("Unable to open support page");
            }
        }

        private async Task ContactSupportAsync()
        {
            try
            {
                // Open email or phone contact options
                var message = new EmailMessage
                {
                    Subject = "Maame.Dromo Partograph - Support Request",
                    Body = "Hello Support Team,\n\n",
                    To = new List<string> { "support@maamedromo.com" }
                };

                await Email.Default.ComposeAsync(message);
            }
            catch (Exception ex)
            {
                await AppShell.DisplayToastAsync("Unable to open email client");
            }
        }

        private async Task OpenTermsAsync()
        {
            try
            {
                await Browser.Default.OpenAsync("https://maamedromo.com/terms",
                    BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                await AppShell.DisplayToastAsync("Unable to open terms page");
            }
        }

        private async Task OpenPrivacyAsync()
        {
            try
            {
                await Browser.Default.OpenAsync("https://maamedromo.com/privacy",
                    BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                await AppShell.DisplayToastAsync("Unable to open privacy policy");
            }
        }

        #endregion

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
