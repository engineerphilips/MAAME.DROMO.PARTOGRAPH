using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class HomePageModel : ObservableObject
    {
        private readonly PartographRepository _partographRepository;
        private readonly ModalErrorHandler _errorHandler;
        private readonly IDataLoadingService _dataLoadingService;
        private bool _isInitialLoad = true;

        [ObservableProperty]
        private DashboardStats _dashboardStats = new();

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isRefreshing;

        [ObservableProperty]
        private DateTime _today = DateTime.Now;

        [ObservableProperty]
        private string _currentShift = GetCurrentShift();

        [ObservableProperty]
        private string _staffName = "Dr. Phil"; // From authentication

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private bool _hasAlerts;

        [ObservableProperty]
        private bool _hasCriticalPatients;

        public HomePageModel(
            PartographRepository partographRepository,
            ModalErrorHandler errorHandler,
            IDataLoadingService dataLoadingService)
        {
            _partographRepository = partographRepository;
            _errorHandler = errorHandler;
            _dataLoadingService = dataLoadingService;
        }

        partial void OnDashboardStatsChanged(DashboardStats value)
        {
            HasAlerts = value.ActiveAlerts.Any();
            HasCriticalPatients = value.CriticalPatients.Any();
        }

        private static string GetCurrentShift()
        {
            var hour = DateTime.Now.Hour;
            if (hour >= 7 && hour < 15) return "Morning Shift";
            if (hour >= 15 && hour < 23) return "Evening Shift";
            return "Night Shift";
        }

        private async Task LoadDataWithProgress()
        {
            await _dataLoadingService.LoadDataWithProgressAsync(
                "Loading Dashboard",
                ("dashboard statistics", async () =>
                {
                    DashboardStats = await _partographRepository.GetDashboardStatsAsync();
                })
            );
        }

        private async Task LoadData()
        {
            try
            {
                IsBusy = true;

                // Load only dashboard statistics - lightweight and fast
                DashboardStats = await _partographRepository.GetDashboardStatsAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            try
            {
                IsRefreshing = true;
                await LoadData();
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task Appearing()
        {
            try
            {
                // UI renders first, then data loads with progress
                if (_isInitialLoad)
                {
                    _isInitialLoad = false;

                    // Check for cached dashboard stats (preloaded during facility selection transition)
                    if (Constants.CachedDashboardStats != null)
                    {
                        // Use cached data - no loading overlay needed
                        DashboardStats = Constants.CachedDashboardStats;
                        // Clear cache after use
                        Constants.CachedDashboardStats = null;
                    }
                    else
                    {
                        // Fallback: load with progress if no cached data
                        await LoadDataWithProgress();
                    }
                }
                else
                {
                    await LoadData();
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
        }

        [RelayCommand]
        private Task NavigateToPatient(Partograph patient)
            => Shell.Current.GoToAsync($"patient?id={patient.ID}");

        [RelayCommand]
        private Task NavigateToPendingPatients()
            => Shell.Current.GoToAsync("pendingpatients");

        [RelayCommand]
        private Task NavigateToActivePatients()
            => Shell.Current.GoToAsync("activepatients");

        [RelayCommand]
        private Task NavigateToCompletedPatients()
            => Shell.Current.GoToAsync("completedpatients");

        [RelayCommand]
        private Task AddNewPatient() => Shell.Current.GoToAsync("patient");

        [RelayCommand]
        private async Task HandleEmergency(Partograph patient)
        {
            patient.Status = LaborStatus.Emergency;
            //await _patientRepository.SaveItemAsync(patient);
            await AppShell.DisplaySnackbarAsync($"Emergency alert sent for {patient.Name}");
            await Refresh();
        }

        [RelayCommand]
        private async Task NavigateToEmergencyPatients()
        {
            // Navigate to filtered view showing only emergency patients
            await Shell.Current.GoToAsync("activepatients?filter=emergency");
        }

        [RelayCommand]
        private async Task NavigateToCriticalPatient(CriticalPatientInfo patient)
        {
            if (patient?.PatientId != null)
            {
                await Shell.Current.GoToAsync($"partograph?patientId={patient.PatientId}");
            }
        }

        [RelayCommand]
        private async Task NavigateToAlertPatient(PatientAlert alert)
        {
            if (alert?.PatientId != null)
            {
                var status = await _partographRepository.GetStatusAsync(alert.PatientId);
                // Navigate based on labor stage
                var route = status switch
                {
                    LaborStatus.SecondStage => "secondpartograph",
                    LaborStatus.ThirdStage => "thirdpartograph",
                    LaborStatus.FourthStage => "fourthpartograph",
                    _ => "partograph" // FirstStage, Pending, or any other status defaults to first stage partograph
                };

                await Shell.Current.GoToAsync($"{route}?patientId={alert.PatientId.ToString()}");
                //await Shell.Current.GoToAsync($"partograph?patientId={alert.PatientId}");
            }
        }

        [RelayCommand]
        private async Task PerformQuickSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return;

            try
            {
                // Navigate to active patients with search query
                await Shell.Current.GoToAsync($"activepatients?search={Uri.EscapeDataString(SearchQuery)}");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        [RelayCommand]
        private async Task NavigatedTo()
        {
            try
            {
                await LoadData();
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
        }
    }
}
