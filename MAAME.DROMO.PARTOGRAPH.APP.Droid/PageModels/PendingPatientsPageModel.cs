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
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                IsBusy = true;
                _allPatients = await _patientRepository.ListAsync(LaborStatus.Pending);
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
        private Task NavigateToPatient(Patient patient)
            => Shell.Current.GoToAsync($"patient?id={patient.ID}");

        [RelayCommand]
        private Task AddNewPatient()
            => Shell.Current.GoToAsync("newpatient");

        [RelayCommand]
        private async Task StartActiveLabor(Partograph patient)
        {
            patient.Status = LaborStatus.Active;
            patient.LaborStartTime = DateTime.Now;
            await _patientRepository.SaveItemAsync(patient);
            await AppShell.DisplayToastAsync($"Labor started for {patient.Name}");
            await LoadData();
        }
    }
}
