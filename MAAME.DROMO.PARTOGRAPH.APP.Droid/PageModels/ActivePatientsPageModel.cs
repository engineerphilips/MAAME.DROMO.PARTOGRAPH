using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class ActivePatientsPageModel : ObservableObject
    {
        private readonly PatientRepository _patientRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private List<Patient> _patients = [];

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isRefreshing;

        [ObservableProperty]
        private string _searchText = string.Empty;

        private List<Patient> _allPatients = [];

        public ActivePatientsPageModel(PatientRepository patientRepository, ModalErrorHandler errorHandler)
        {
            _patientRepository = patientRepository;
            _errorHandler = errorHandler;
        }

        [RelayCommand]
        private async Task Appearing()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                IsBusy = true;
                _allPatients = await _patientRepository.ListAsync(LaborStatus.Active);

                // Calculate hours in labor for each patient
                foreach (var patient in _allPatients)
                {
                    if (patient.LaborStartTime.HasValue)
                    {
                        var hoursInLabor = (DateTime.Now - patient.LaborStartTime.Value).TotalHours;
                        // You can add this as a property to display
                    }
                }

                FilterPatients();
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
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
                    p.HospitalNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
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
        private Task NavigateToPatient(Patient patient)
            => Shell.Current.GoToAsync($"patient?id={patient.ID}");

        [RelayCommand]
        private Task NavigateToPartograph(Patient patient)
            => Shell.Current.GoToAsync($"partograph?id={patient.ID}");

        [RelayCommand]
        private Task AddPartographEntry(Patient patient)
            => Shell.Current.GoToAsync($"partographentry?patientId={patient.ID}");

        [RelayCommand]
        private async Task CompleteDelivery(Patient patient)
        {
            patient.Status = LaborStatus.Completed;
            patient.DeliveryTime = DateTime.Now;
            await _patientRepository.SaveItemAsync(patient);
            await AppShell.DisplayToastAsync($"Delivery completed for {patient.Name}");
            await LoadData();
        }

        [RelayCommand]
        private async Task MarkAsEmergency(Patient patient)
        {
            patient.Status = LaborStatus.Emergency;
            await _patientRepository.SaveItemAsync(patient);
            await AppShell.DisplaySnackbarAsync($"Emergency alert sent for {patient.Name}");
            await LoadData();
        }
    }
}
