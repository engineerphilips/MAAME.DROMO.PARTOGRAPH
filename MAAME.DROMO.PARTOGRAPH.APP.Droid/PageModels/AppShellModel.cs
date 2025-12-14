using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public class AppShellModel : INotifyPropertyChanged
    {
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
        }

        #endregion
    }
}
