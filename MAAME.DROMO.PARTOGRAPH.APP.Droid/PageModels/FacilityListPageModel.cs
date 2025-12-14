using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Pages;
using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public class FacilityListPageModel : INotifyPropertyChanged
    {
        private readonly FacilityRepository _facilityRepository;
        private bool _isBusy = false;
        private ObservableCollection<Facility> _facilities = new();

        public FacilityListPageModel(FacilityRepository facilityRepository)
        {
            _facilityRepository = facilityRepository;

            // Initialize commands
            AddFacilityCommand = new Command(async () => await AddFacilityAsync());
        }

        #region Properties

        public ObservableCollection<Facility> Facilities
        {
            get => _facilities;
            set
            {
                if (SetProperty(ref _facilities, value))
                {
                    OnPropertyChanged(nameof(FacilityCountText));
                    OnPropertyChanged(nameof(IsEmpty));
                    OnPropertyChanged(nameof(IsNotEmpty));
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string FacilityCountText => $"{Facilities.Count} {(Facilities.Count == 1 ? "Facility" : "Facilities")} Found";

        public bool IsEmpty => !IsBusy && Facilities.Count == 0;

        public bool IsNotEmpty => !IsBusy && Facilities.Count > 0;

        #endregion

        #region Commands

        public ICommand AddFacilityCommand { get; }

        #endregion

        #region Methods

        public async Task InitializeAsync()
        {
            await LoadFacilitiesAsync();
        }

        public async Task LoadFacilitiesAsync()
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
                OnPropertyChanged(nameof(IsEmpty));
                OnPropertyChanged(nameof(IsNotEmpty));
            }
        }

        private async Task AddFacilityAsync()
        {
            try
            {
                // Navigate to FacilityOnboardingPage
                var onboardingPage = IPlatformApplication.Current!.Services.GetService<FacilityOnboardingPage>();
                if (onboardingPage != null)
                {
                    await Application.Current.MainPage.Navigation.PushAsync(onboardingPage);
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Error", $"Failed to navigate: {ex.Message}");
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
