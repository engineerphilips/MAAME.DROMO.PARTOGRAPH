using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class PatientsPageModel : ObservableObject
    {
        private readonly PatientRepository _patientRepository;
        private readonly ModalErrorHandler _errorHandler;
        private readonly IDataLoadingService _dataLoadingService;
        private bool _isInitialLoad = true;

        [ObservableProperty]
        private List<Patient> _patients = [];

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isRefreshing;

        [ObservableProperty]
        private string _searchText = string.Empty;

        private List<Patient> _allPatients = [];

        public PatientsPageModel(
            PatientRepository patientRepository,
            ModalErrorHandler errorHandler,
            IDataLoadingService dataLoadingService)
        {
            _patientRepository = patientRepository;
            _errorHandler = errorHandler;
            _dataLoadingService = dataLoadingService;
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
                    await LoadDataWithProgress();
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

        private async Task LoadDataWithProgress()
        {
            await _dataLoadingService.LoadDataWithProgressAsync(
                "Loading Patients",
                ("patient records", async () =>
                {
                    _allPatients = await _patientRepository.ListAsync() ?? [];
                }),
                ("filtering data", async () =>
                {
                    await Task.Run(() =>
                    {
                        // Filter by facility if necessary
                        var facilityId = Data.Constants.GetFacilityForFiltering();
                        if (facilityId != null)
                        {
                            _allPatients = _allPatients.Where(p => p.Handler == facilityId || p.Handler == Data.Constants.Staff?.ID).ToList();
                        }

                        // Filter out deleted patients
                        _allPatients = _allPatients.Where(p => p.Deleted == 0).ToList();

                        FilterPatients();
                    });
                })
            );
        }

        private async Task LoadData()
        {
            try
            {
                IsBusy = true;

                // Load all patients
                _allPatients = await _patientRepository.ListAsync() ?? [];

                // Filter by facility if necessary
                var facilityId = Data.Constants.GetFacilityForFiltering();
                if (facilityId != null)
                {
                    _allPatients = _allPatients.Where(p => p.Handler == facilityId || p.Handler == Data.Constants.Staff?.ID).ToList();
                }

                // Filter out deleted patients
                _allPatients = _allPatients.Where(p => p.Deleted == 0).ToList();

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
        private Task NavigateToPatientHub(Patient patient)
            => Shell.Current.GoToAsync($"patienthub?patientId={patient.ID}");

        [RelayCommand]
        private Task AddNewPatient()
            => Shell.Current.GoToAsync("patient");
    }
}
