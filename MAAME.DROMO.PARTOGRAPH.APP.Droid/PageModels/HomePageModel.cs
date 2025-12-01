using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        [ObservableProperty]
        private DashboardStats _dashboardStats = new();

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isRefreshing;

        [ObservableProperty]
        private string _today = DateTime.Now.ToString("dddd, MMM d");

        [ObservableProperty]
        private string _currentShift = GetCurrentShift();

        [ObservableProperty]
        private string _staffName = "Dr. Phil"; // From authentication

        public HomePageModel(PartographRepository partographRepository, ModalErrorHandler errorHandler)
        {
            _partographRepository = partographRepository;
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
                await LoadData();
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
        private Task AddNewPatient()
            => Shell.Current.GoToAsync("newpatient");

        [RelayCommand]
        private async Task HandleEmergency(Partograph patient)
        {
            patient.Status = LaborStatus.Emergency;
            //await _patientRepository.SaveItemAsync(patient);
            await AppShell.DisplaySnackbarAsync($"Emergency alert sent for {patient.Name}");
            await Refresh();
        }
    }
}
