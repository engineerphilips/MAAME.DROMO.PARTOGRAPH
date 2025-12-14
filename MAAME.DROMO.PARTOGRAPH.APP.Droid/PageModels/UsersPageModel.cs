using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public class UsersPageModel : INotifyPropertyChanged
    {
        private readonly StaffRepository _staffRepository;
        private readonly FacilityRepository _facilityRepository;
        private bool _isBusy = false;
        private ObservableCollection<Staff> _users = new();
        private int _totalUsers = 0;

        public UsersPageModel(StaffRepository staffRepository, FacilityRepository facilityRepository)
        {
            _staffRepository = staffRepository;
            _facilityRepository = facilityRepository;

            // Initialize commands
            LoadUsersCommand = new Command(async () => await LoadUsersAsync());
            AddUserCommand = new Command(async () => await AddUserAsync());
            RefreshCommand = new Command(async () => await RefreshAsync());
        }

        #region Properties

        public ObservableCollection<Staff> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        public int TotalUsers
        {
            get => _totalUsers;
            set => SetProperty(ref _totalUsers, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // Check if current user can add users
        public bool CanAddUser => Constants.Staff?.Role == "SUPER-ADMIN" ||
                                   Constants.Staff?.Role == "Maame.Dromo.Admin" ||
                                   Constants.Staff?.Role == "ADMIN";

        #endregion

        #region Commands

        public ICommand LoadUsersCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Methods

        public async Task InitializeAsync()
        {
            await LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                Users.Clear();

                // Get all staff from repository
                var allStaff = await _staffRepository.ListAsync();

                // Filter based on user role
                List<Staff> filteredStaff;

                if (Constants.Staff?.Role == "SUPER-ADMIN" || Constants.Staff?.Role == "Maame.Dromo.Admin")
                {
                    // Super and Maame.Dromo.Admin can see all users
                    filteredStaff = allStaff;
                }
                else if (Constants.Staff?.Role == "ADMIN")
                {
                    // Admin can only see users at their facility
                    var adminFacilityId = Constants.Staff.Facility;
                    filteredStaff = allStaff.Where(s => s.Facility == adminFacilityId).ToList();
                }
                else
                {
                    // Other roles should not have access to user management
                    filteredStaff = new List<Staff>();
                }

                // Load facility names for each staff
                var facilities = await _facilityRepository.ListAsync();
                foreach (var staff in filteredStaff)
                {
                    var facility = facilities.FirstOrDefault(f => f.ID == staff.Facility);
                    staff.FacilityName = facility?.Name ?? "Unknown Facility";
                    Users.Add(staff);
                }

                TotalUsers = Users.Count;
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Error", $"Failed to load users: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddUserAsync()
        {
            // Navigate to SignupPage
            await Shell.Current.GoToAsync("signup");
        }

        private async Task RefreshAsync()
        {
            await LoadUsersAsync();
        }

        private static async Task ShowAlertAsync(string title, string message)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, "OK");
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
