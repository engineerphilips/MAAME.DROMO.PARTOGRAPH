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
        private int _notificationCount = 0;
        private int _activePatientsCount = 0;
        private int _pendingPatientsCount = 0;
        private string _currentBreadcrumb = string.Empty;
        private bool _isSelected;

        public AppShellModel()
        {
            // Initialize commands
            OpenHelpCommand = new Command(async () => await OpenHelpAsync());
            ShowNotificationsCommand = new Command(async () => await ShowNotificationsAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
            ViewActiveCommand = new Command(async () => await ViewActiveAsync());
            ViewPendingCommand = new Command(async () => await ViewPendingAsync());
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

        // User initials for avatar display
        public string UserInitials
        {
            get
            {
                if (string.IsNullOrEmpty(Constants.Staff?.Name))
                    return "?";

                var nameParts = Constants.Staff.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length >= 2)
                    return $"{nameParts[0][0]}{nameParts[1][0]}".ToUpper();
                else if (nameParts.Length == 1)
                    return nameParts[0].Substring(0, Math.Min(2, nameParts[0].Length)).ToUpper();

                return "?";
            }
        }

        // Avatar background color based on role
        public Color AvatarColor
        {
            get
            {
                return Constants.Staff?.Role switch
                {
                    "SUPER-ADMIN" => Color.FromArgb("#7C3AED"),      // Purple
                    "Maame.Dromo.Admin" => Color.FromArgb("#2563EB"), // Blue
                    "ADMIN" => Color.FromArgb("#0891B2"),             // Cyan
                    "MIDWIFE" => Color.FromArgb("#059669"),           // Green
                    "DOCTOR" => Color.FromArgb("#DC2626"),            // Red
                    "NURSE" => Color.FromArgb("#EA580C"),             // Orange
                    _ => Color.FromArgb("#2196F3")                    // Default Blue
                };
            }
        }

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

        // Notification system
        public int NotificationCount
        {
            get => _notificationCount;
            set => SetProperty(ref _notificationCount, value);
        }

        public bool HasNotifications => NotificationCount > 0;

        public string NotificationCountDisplay => NotificationCount > 99 ? "99+" : NotificationCount.ToString();
        
        public bool IsSelected
        {
            get => _isSelected; 
            set => SetProperty(ref _isSelected, value);
        }

        // Quick stats for footer
        public int ActivePatientsCount
        {
            get => _activePatientsCount;
            set
            {
                SetProperty(ref _activePatientsCount, value);
                OnPropertyChanged(nameof(ActivePatientsDisplay));
            }
        }

        public int PendingPatientsCount
        {
            get => _pendingPatientsCount;
            set
            {
                SetProperty(ref _pendingPatientsCount, value);
                OnPropertyChanged(nameof(PendingPatientsDisplay));
            }
        }

        public string ActivePatientsDisplay => ActivePatientsCount > 0 ? ActivePatientsCount.ToString() : "-";
        public string PendingPatientsDisplay => PendingPatientsCount > 0 ? PendingPatientsCount.ToString() : "-";

        // Current breadcrumb/subtitle
        public string CurrentBreadcrumb
        {
            get => _currentBreadcrumb;
            set => SetProperty(ref _currentBreadcrumb, value);
        }

        // Greeting based on time of day
        public string Greeting
        {
            get
            {
                var hour = DateTime.Now.Hour;
                return hour switch
                {
                    < 12 => "Good Morning",
                    < 17 => "Good Afternoon",
                    _ => "Good Evening"
                };
            }
        }

        // Current date formatted
        public string CurrentDate => DateTime.Now.ToString("dddd, MMMM d");

        #region Commands

        public ICommand OpenHelpCommand { get; }
        public ICommand ShowNotificationsCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ViewActiveCommand { get; }
        public ICommand ViewPendingCommand { get; }

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
            try
            {
                await Shell.Current.GoToAsync("notifications");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        private async Task ViewActiveAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("activepatients");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        private async Task ViewPendingAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("pendingpatients");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
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

        #region Public Methods

        // Update notification count
        public void UpdateNotificationCount(int count)
        {
            NotificationCount = count;
            OnPropertyChanged(nameof(HasNotifications));
            OnPropertyChanged(nameof(NotificationCountDisplay));
        }

        // Update patient stats
        public void UpdatePatientStats(int active, int pending)
        {
            ActivePatientsCount = active;
            PendingPatientsCount = pending;
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
            OnPropertyChanged(nameof(UserInitials));
            OnPropertyChanged(nameof(AvatarColor));
            OnPropertyChanged(nameof(Greeting));
            OnPropertyChanged(nameof(CurrentDate));
        }

        #endregion
    }
}
