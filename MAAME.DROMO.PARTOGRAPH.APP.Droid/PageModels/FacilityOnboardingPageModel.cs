using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public class FacilityOnboardingPageModel : INotifyPropertyChanged
    {
        private readonly FacilityRepository _facilityRepository;
        private bool _isBusy = false;
        private string _name = string.Empty;
        private string _code = string.Empty;
        private string _type = "Hospital";
        private string _address = string.Empty;
        private string _city = string.Empty;
        private string _region = string.Empty;
        private string _country = "Ghana";
        private string _phone = string.Empty;
        private string _email = string.Empty;
        private string _latitudeText = string.Empty;
        private string _longitudeText = string.Empty;
        private string _ghPostGPS = string.Empty;

        public FacilityOnboardingPageModel(FacilityRepository facilityRepository)
        {
            _facilityRepository = facilityRepository;

            // Initialize commands
            SaveCommand = new Command(async () => await SaveAsync(), CanSave);
            CancelCommand = new Command(async () => await CancelAsync());
        }

        #region Properties

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    ((Command)SaveCommand).ChangeCanExecute();
                }
            }
        }

        public string Code
        {
            get => _code;
            set
            {
                if (SetProperty(ref _code, value))
                {
                    ((Command)SaveCommand).ChangeCanExecute();
                }
            }
        }

        public string Type
        {
            get => _type;
            set
            {
                if (SetProperty(ref _type, value))
                {
                    ((Command)SaveCommand).ChangeCanExecute();
                }
            }
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public string City
        {
            get => _city;
            set
            {
                if (SetProperty(ref _city, value))
                {
                    ((Command)SaveCommand).ChangeCanExecute();
                }
            }
        }

        public string Region
        {
            get => _region;
            set
            {
                if (SetProperty(ref _region, value))
                {
                    ((Command)SaveCommand).ChangeCanExecute();
                }
            }
        }

        public string Country
        {
            get => _country;
            set
            {
                if (SetProperty(ref _country, value))
                {
                    ((Command)SaveCommand).ChangeCanExecute();
                }
            }
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string LatitudeText
        {
            get => _latitudeText;
            set => SetProperty(ref _latitudeText, value);
        }

        public string LongitudeText
        {
            get => _longitudeText;
            set => SetProperty(ref _longitudeText, value);
        }

        public string GHPostGPS
        {
            get => _ghPostGPS;
            set => SetProperty(ref _ghPostGPS, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion

        #region Commands

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Methods

        public void InitializeAsync()
        {
            // Reset form if needed
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(Code) &&
                   !string.IsNullOrWhiteSpace(Type) &&
                   !string.IsNullOrWhiteSpace(City) &&
                   !string.IsNullOrWhiteSpace(Region) &&
                   !string.IsNullOrWhiteSpace(Country);
        }

        private async Task SaveAsync()
        {
            if (!CanSave())
            {
                await ShowAlertAsync("Validation Error", "Please fill in all required fields (marked with *).");
                return;
            }

            try
            {
                IsBusy = true;

                // Parse GPS coordinates
                double? latitude = null;
                double? longitude = null;

                if (!string.IsNullOrWhiteSpace(LatitudeText))
                {
                    if (double.TryParse(LatitudeText, out double lat))
                    {
                        latitude = lat;
                    }
                    else
                    {
                        await ShowAlertAsync("Validation Error", "Invalid latitude value. Please enter a valid number.");
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(LongitudeText))
                {
                    if (double.TryParse(LongitudeText, out double lon))
                    {
                        longitude = lon;
                    }
                    else
                    {
                        await ShowAlertAsync("Validation Error", "Invalid longitude value. Please enter a valid number.");
                        return;
                    }
                }

                // Create new facility
                var facility = new Facility
                {
                    ID = Guid.NewGuid(),
                    Name = Name.Trim(),
                    Code = Code.Trim().ToUpper(),
                    Type = Type,
                    Address = Address?.Trim() ?? string.Empty,
                    City = City.Trim(),
                    Region = Region,
                    Country = Country.Trim(),
                    Phone = Phone?.Trim() ?? string.Empty,
                    Email = Email?.Trim() ?? string.Empty,
                    Latitude = latitude,
                    Longitude = longitude,
                    GHPostGPS = GHPostGPS?.Trim() ?? string.Empty,
                    IsActive = true,
                    CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    DeviceId = Guid.NewGuid().ToString(),
                    OriginDeviceId = Guid.NewGuid().ToString(),
                    SyncStatus = 0,
                    Version = 1,
                    ServerVersion = 0,
                    Deleted = 0
                };

                // Save to repository
                await _facilityRepository.AddAsync(facility);

                // Show success message
                await ShowAlertAsync("Success", $"Facility '{facility.Name}' has been successfully added!");

                // Navigate back
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Error", $"Failed to save facility: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CancelAsync()
        {
            bool confirm = await ShowConfirmAsync("Cancel", "Are you sure you want to cancel? All unsaved changes will be lost.");
            if (confirm)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }

        private static async Task ShowAlertAsync(string title, string message)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, "OK");
            }
        }

        private static async Task<bool> ShowConfirmAsync(string title, string message)
        {
            if (Application.Current?.MainPage != null)
            {
                return await Application.Current.MainPage.DisplayAlert(title, message, "Yes", "No");
            }
            return false;
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
