using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public class SignupPageModel : INotifyPropertyChanged
    {
        private readonly StaffRepository _staffRepository;
        private readonly FacilityRepository _facilityRepository;
        private bool _isBusy = false;
        private string _name = string.Empty;
        private string _staffId = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _role = "MIDWIFE";
        private string _department = "Labour Ward";
        private Facility? _selectedFacility = null;
        private ObservableCollection<Facility> _facilities = new();
        private ObservableCollection<string> _availableRoles = new();

        public SignupPageModel(StaffRepository staffRepository, FacilityRepository facilityRepository)
        {
            _staffRepository = staffRepository;
            _facilityRepository = facilityRepository;

            // Initialize commands
            SignUpCommand = new Command(async () => await SignUpAsync(), CanSignUp);
            CancelCommand = new Command(async () => await CancelAsync());

            // Initialize available roles based on current user
            InitializeAvailableRoles();
        }

        #region Properties

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    ((Command)SignUpCommand).ChangeCanExecute();
                }
            }
        }

        public string StaffId
        {
            get => _staffId;
            set
            {
                if (SetProperty(ref _staffId, value))
                {
                    ((Command)SignUpCommand).ChangeCanExecute();
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    ((Command)SignUpCommand).ChangeCanExecute();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    ((Command)SignUpCommand).ChangeCanExecute();
                }
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                {
                    ((Command)SignUpCommand).ChangeCanExecute();
                }
            }
        }

        public string Role
        {
            get => _role;
            set
            {
                if (SetProperty(ref _role, value))
                {
                    ((Command)SignUpCommand).ChangeCanExecute();
                }
            }
        }

        public string Department
        {
            get => _department;
            set => SetProperty(ref _department, value);
        }

        public Facility? SelectedFacility
        {
            get => _selectedFacility;
            set
            {
                if (SetProperty(ref _selectedFacility, value))
                {
                    ((Command)SignUpCommand).ChangeCanExecute();
                }
            }
        }

        public ObservableCollection<Facility> Facilities
        {
            get => _facilities;
            set => SetProperty(ref _facilities, value);
        }

        public ObservableCollection<string> AvailableRoles
        {
            get => _availableRoles;
            set => SetProperty(ref _availableRoles, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // Show facility selection only for Super/Maame.Dromo.Admin
        public bool ShowFacilitySelection => Constants.Staff?.Role == "SUPER-ADMIN" ||
                                              Constants.Staff?.Role == "Maame.Dromo.Admin";

        #endregion

        #region Commands

        public ICommand SignUpCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Methods

        public async Task InitializeAsync()
        {
            ResetForm();
            await LoadFacilitiesAsync();
        }

        /// <summary>
        /// Resets all form fields to their default values for new user onboarding
        /// </summary>
        public void ResetForm()
        {
            Name = string.Empty;
            StaffId = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            Role = "MIDWIFE";
            Department = "Labour Ward";
            SelectedFacility = null;
            IsBusy = false;
        }

        private void InitializeAvailableRoles()
        {
            // Admin cannot create SUPER-ADMIN or Maame.Dromo.Admin
            if (Constants.Staff?.Role == "ADMIN")
            {
                AvailableRoles = new ObservableCollection<string>
                {
                    "MIDWIFE",
                    "NURSE",
                    "DOCTOR",
                    "ADMIN"
                };
            }
            else
            {
                // Super and Maame.Dromo.Admin can create any role
                AvailableRoles = new ObservableCollection<string>
                {
                    "MIDWIFE",
                    "NURSE",
                    "DOCTOR",
                    "ADMIN",
                    "Maame.Dromo.Admin",
                    "SUPER-ADMIN"
                };
            }
        }

        private async Task LoadFacilitiesAsync()
        {
            try
            {
                var facilities = await _facilityRepository.GetAllAsync();
                Facilities.Clear();
                foreach (var facility in facilities)
                {
                    Facilities.Add(facility);
                }

                // Auto-select facility for admin users
                if (Constants.Staff?.Role == "ADMIN" && Constants.Staff?.Facility != null)
                {
                    SelectedFacility = Facilities.FirstOrDefault(f => f.ID == Constants.Staff.Facility);
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Error", $"Failed to load facilities: {ex.Message}");
            }
        }

        private bool CanSignUp()
        {
            bool basicFieldsValid = !string.IsNullOrWhiteSpace(Name) &&
                                   !string.IsNullOrWhiteSpace(StaffId) &&
                                   !string.IsNullOrWhiteSpace(Email) &&
                                   !string.IsNullOrWhiteSpace(Password) &&
                                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                                   !string.IsNullOrWhiteSpace(Role);

            // Super/Maame.Dromo.Admin must select a facility
            if (ShowFacilitySelection)
            {
                return basicFieldsValid && SelectedFacility != null;
            }

            // Admin users automatically use their facility
            return basicFieldsValid;
        }

        private async Task SignUpAsync()
        {
            if (!CanSignUp())
            {
                await ShowAlertAsync("Validation Error", "Please fill in all required fields.");
                return;
            }

            // Validate email format
            if (!IsValidEmail(Email))
            {
                await ShowAlertAsync("Validation Error", "Please enter a valid email address.");
                return;
            }

            // Validate password match
            if (Password != ConfirmPassword)
            {
                await ShowAlertAsync("Validation Error", "Passwords do not match.");
                return;
            }

            // Validate password strength
            if (Password.Length < 6)
            {
                await ShowAlertAsync("Validation Error", "Password must be at least 6 characters long.");
                return;
            }

            try
            {
                IsBusy = true;

                // Determine facility
                Guid? facilityId;
                if (Constants.Staff?.Role == "SUPER-ADMIN" || Constants.Staff?.Role == "Maame.Dromo.Admin")
                {
                    facilityId = SelectedFacility?.ID;
                }
                else
                {
                    // Admin users use their own facility
                    facilityId = Constants.Staff?.Facility;
                }

                if (facilityId == null)
                {
                    await ShowAlertAsync("Error", "No facility selected or assigned.");
                    return;
                }

                // Hash password
                string hashedPassword = HashPassword(Password);

                // Create new staff
                var staff = new Staff
                {
                    ID = Guid.NewGuid(),
                    Name = Name.Trim(),
                    StaffID = StaffId.Trim().ToUpper(),
                    Email = Email.Trim().ToLower(),
                    Role = Role,
                    Department = Department.Trim(),
                    Password = hashedPassword,
                    IsActive = true,
                    Facility = facilityId,
                    LastLogin = DateTime.Now,
                    CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    DeviceId = DeviceIdentity.GetOrCreateDeviceId(),
                    OriginDeviceId = DeviceIdentity.GetOrCreateDeviceId(),
                    SyncStatus = 0,
                    Version = 1,
                    ServerVersion = 0,
                    Deleted = 0
                };

                // Save to repository
                await _staffRepository.AddAsync(staff);

                // Show success message
                await ShowAlertAsync("Success", $"User '{staff.FacilityName}' has been successfully created!");

                // Navigate back
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Error", $"Failed to create user: {ex.Message}");
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
                await Shell.Current.GoToAsync("..");
            }
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
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
