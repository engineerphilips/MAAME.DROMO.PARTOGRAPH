using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
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
        private readonly PatientRepository _patientRepository;
        private readonly ModalErrorHandler _errorHandler;
        private bool _isNavigatedTo;
        private bool _dataLoaded;

        [ObservableProperty]
        private DashboardStats _dashboardStats = new();

        [ObservableProperty]
        private List<Patient> _recentActivePatients = [];

        [ObservableProperty]
        private List<Patient> _emergencyPatients = [];

        [ObservableProperty]
        private List<CategoryChartData> _laborStageData = [];

        [ObservableProperty]
        private List<Brush> _laborStageColors = [];

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isRefreshing;

        [ObservableProperty]
        private string _today = DateTime.Now.ToString("dddd, MMM d");

        [ObservableProperty]
        private string _currentShift = GetCurrentShift();

        [ObservableProperty]
        private string _staffName = "Dr. Sarah Johnson"; // From authentication

        public HomePageModel(PatientRepository patientRepository, ModalErrorHandler errorHandler)
        {
            _patientRepository = patientRepository;
            _errorHandler = errorHandler;
        }

        private static string GetCurrentShift()
        {
            var hour = DateTime.Now.Hour;
            if (hour >= 7 && hour < 15) return "Morning Shift";
            if (hour >= 15 && hour < 23) return "Evening Shift";
            return "Night Shift";
        }

        private async Task LoadData()
        {
            try
            {
                IsBusy = true;

                // Load dashboard statistics
                DashboardStats = await _patientRepository.GetDashboardStatsAsync();

                // Load recent active patients
                var activePatients = await _patientRepository.ListAsync(LaborStatus.Active);
                RecentActivePatients = activePatients.Take(5).ToList();

                // Load emergency patients
                EmergencyPatients = await _patientRepository.ListAsync(LaborStatus.Emergency);

                // Prepare chart data
                var chartData = new List<CategoryChartData>
                {
                    new("Pre-Labor", DashboardStats.PendingLabor),
                    new("Active Labor", DashboardStats.ActiveLabor),
                    new("Delivered Today", DashboardStats.CompletedToday),
                    new("Emergency", DashboardStats.EmergencyCases)
                };

                var chartColors = new List<Brush>
                {
                    new SolidColorBrush(Colors.Orange),
                    new SolidColorBrush(Colors.Green),
                    new SolidColorBrush(Colors.Blue),
                    new SolidColorBrush(Colors.Red)
                };

                LaborStageData = chartData;
                LaborStageColors = chartColors;
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
        private void NavigatedTo() => _isNavigatedTo = true;

        [RelayCommand]
        private void NavigatedFrom() => _isNavigatedTo = false;

        [RelayCommand]
        private async Task Appearing()
        {
            if (!_dataLoaded)
            {
                _dataLoaded = true;
                await LoadData();
            }
            else if (!_isNavigatedTo)
            {
                await Refresh();
            }
        }

        [RelayCommand]
        private Task NavigateToPatient(Patient patient)
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
        private Task AddNewPatient()
            => Shell.Current.GoToAsync("newpatient");

        [RelayCommand]
        private async Task HandleEmergency(Patient patient)
        {
            patient.Status = LaborStatus.Emergency;
            await _patientRepository.SaveItemAsync(patient);
            await AppShell.DisplaySnackbarAsync($"Emergency alert sent for {patient.Name}");
            await Refresh();
        }
    }
}
