using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;
using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public class FacilitySelectionPageModel : INotifyPropertyChanged
    {
        private readonly FacilityRepository _facilityRepository;
        private readonly PartographRepository _partographRepository;
        private bool _isBusy = false;
        private Facility? _selectedFacility;
        private ObservableCollection<Facility> _facilities = new();

        public FacilitySelectionPageModel(FacilityRepository facilityRepository, PartographRepository partographRepository)
        {
            _facilityRepository = facilityRepository;
            _partographRepository = partographRepository;

            // Initialize commands
            ContinueCommand = new Command(async () => await ContinueAsync(), () => SelectedFacility != null);
        }

        #region Properties

        public ObservableCollection<Facility> Facilities
        {
            get => _facilities;
            set => SetProperty(ref _facilities, value);
        }

        public Facility? SelectedFacility
        {
            get => _selectedFacility;
            set
            {
                if (SetProperty(ref _selectedFacility, value))
                {
                    ((Command)ContinueCommand).ChangeCanExecute();
                    OnPropertyChanged(nameof(HasSelectedFacility));
                }
            }
        }

        public bool HasSelectedFacility => SelectedFacility != null;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string UserName => Constants.Staff?.Name ?? "User";
        public string UserRole => Constants.Staff?.Role ?? "Admin";

        #endregion

        #region Commands

        public ICommand ContinueCommand { get; }

        #endregion

        #region Methods

        public async Task InitializeAsync()
        {
            await LoadFacilitiesAsync();
        }

        private async Task LoadFacilitiesAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var facilities = await _facilityRepository.GetAllAsync();
                Facilities = new ObservableCollection<Facility>(facilities);
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Error", $"Failed to load facilities: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ContinueAsync()
        {
            if (SelectedFacility == null)
            {
                await ShowAlertAsync("Selection Required", "Please select a facility to continue.");
                return;
            }

            try
            {
                IsBusy = true;
                var facilityName = SelectedFacility.Name;

                // Navigate to AppShell with UI-first loading pattern and progress tracking
                await TransitionLoadingPage.TransitionWithProgressAsync(
                    () => new AppShell(),
                    "Loading Dashboard",
                    ("Configuring facility...", async () =>
                    {
                        // Save selected facility to Constants
                        Constants.SelectedFacility = SelectedFacility;

                        // Save to preferences for persistence
                        Preferences.Set("SelectedFacilityId", SelectedFacility.ID.ToString());
                        Preferences.Set("SelectedFacilityName", facilityName);

                        await Task.Delay(100); // Allow UI update
                    }),
                    ("Loading patient data...", async () =>
                    {
                        // Preload dashboard stats and cache them
                        var stats = await _partographRepository.GetDashboardStatsAsync();
                        Constants.CachedDashboardStats = stats;
                    }),
                    ($"Setting up {facilityName}...", async () =>
                    {
                        // Final preparation
                        await Task.Delay(50);
                    })
                );
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Error", $"Failed to proceed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
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
