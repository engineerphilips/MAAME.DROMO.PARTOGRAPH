using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public class AppShellModel : INotifyPropertyChanged
    {
        public AppShellModel()
        {
            // Initialize commands
            OpenHelpCommand = new Command(async () => await OpenHelpAsync());
            ShowNotificationsCommand = new Command(async () => await ShowNotificationsAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
        }

        public string? Name => Constants.Staff?.Name;
        public string? Role => Constants.Staff?.Role;
        public string? Facility
        {
            get
            {
                // For Super/Admin, show selected facility
                if (Constants.IsSuperOrAdmin())
                {
                    return Constants.SelectedFacility?.Name ?? "No Facility Selected";
                }
                // For other users, show their assigned facility
                return Constants.Staff?.FacilityName ?? "No Facility";
            }
        }
        public string? Email => Constants.Staff?.Email;

        // Get app version from assembly
        public string AppVersion
        {
            get
            {
                var version = AppInfo.Current.VersionString;
                return $"v{version}";
            }
        }

        // Show user details only for non-Super/Admin users
        public bool ShowUserDetails => !Constants.IsSuperOrAdmin();

        // Show facilities menu only for Super/Admin users
        public bool ShowFacilitiesMenu => Constants.IsSuperOrAdmin();

        // Show users menu for Super/Maame.Dromo.Admin/Admin users
        public bool ShowUsersMenu => Constants.Staff?.Role == "SUPER-ADMIN" ||
                                      Constants.Staff?.Role == "Maame.Dromo.Admin" ||
                                      Constants.Staff?.Role == "ADMIN";

        #region Commands

        public ICommand OpenHelpCommand { get; }
        public ICommand ShowNotificationsCommand { get; }
        public ICommand LogoutCommand { get; }

        #endregion

        #region Command Methods

        private async Task OpenHelpAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("//help");
            }
            catch (Exception ex)
            {
                // Handle navigation error
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        private async Task ShowNotificationsAsync()
        {
            // TODO: Implement notifications functionality
            await AppShell.DisplayToastAsync("Notifications feature coming soon");
        }

        private async Task LogoutAsync()
        {
            try
            {
                // Clear user data
                Constants.Staff = null;
                Constants.SelectedFacility = null;

                // Navigate to login page
                await Shell.Current.GoToAsync("//login");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
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

        // Method to refresh bindings when facility selection changes
        public void RefreshBindings()
        {
            OnPropertyChanged(nameof(Facility));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Role));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(AppVersion));
            OnPropertyChanged(nameof(ShowUserDetails));
            OnPropertyChanged(nameof(ShowFacilitiesMenu));
            OnPropertyChanged(nameof(ShowUsersMenu));
        }

        #endregion
    }
}
