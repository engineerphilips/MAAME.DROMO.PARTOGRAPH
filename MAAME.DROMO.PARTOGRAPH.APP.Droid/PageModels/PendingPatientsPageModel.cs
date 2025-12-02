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
    public partial class PendingPatientsPageModel : ObservableObject
    {
        private readonly PartographRepository _patientRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private List<Partograph> _patients = [];

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isRefreshing;

        [ObservableProperty]
        private string _searchText = string.Empty;

        private List<Partograph> _allPatients = [];

        public PendingPatientsPageModel(PartographRepository patientRepository, ModalErrorHandler errorHandler)
        {
            _patientRepository = patientRepository;
            _errorHandler = errorHandler;
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

        private async Task LoadData()
        {
            try
            {
                IsBusy = true;
                _allPatients = await _patientRepository.ListAsync(LaborStatus.Pending);
                FilterPatients();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void FilterPatients()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Patients = _allPatients;
            }
            else
            {
                Patients = _allPatients.Where(p =>
                    p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Patient.HospitalNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterPatients();
        }

        [RelayCommand]
        private async Task Refresh()
        {
            try
            {
                IsRefreshing = true;
                await LoadData();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private Task NavigateToPatient(Partograph partograph)
            => Shell.Current.GoToAsync($"patient?id={partograph.PatientID}");

        [RelayCommand]
        private Task AddNewPatient()
            => Shell.Current.GoToAsync("newpatient");

        [RelayCommand]
        private async Task StartActiveLabor(Partograph partograph)
        {
            try
            {
                // Show confirmation dialog
                var confirm = await Shell.Current.DisplayAlert(
                    "Start Active Labour",
                    $"Are you sure you want to start active labour for {partograph.Name}?",
                    "Yes, Start Labour",
                    "Cancel");

                if (!confirm)
                    return;

                // Update partograph status
                partograph.Status = LaborStatus.Active;
                partograph.LaborStartTime = DateTime.UtcNow;

                // Save to database (repository handles validation and LaborStartTime setting)
                await _patientRepository.SaveItemAsync(partograph);

                // Show success message
                await AppShell.DisplayToastAsync($"Labour started for {partograph.Name}");

                // Reload data to refresh the list
                await LoadData();
            }
            catch (InvalidOperationException ex)
            {
                // Handle specific case where patient already has active partograph
                await Shell.Current.DisplayAlert(
                    "Cannot Start Labour",
                    ex.Message,
                    "OK");
            }
            catch (Exception ex)
            {
                // Handle any other errors
                _errorHandler.HandleError(ex);
            }
        }
    }
}
