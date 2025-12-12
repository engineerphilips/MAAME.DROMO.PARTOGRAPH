using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class CompletedPatientsPageModel : ObservableObject
    {
        private readonly PatientRepository _patientRepository;
        private readonly PartographRepository _partographRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private List<Partograph> _partographs = [];

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isRefreshing;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;

        private List<Partograph> _allPatients = [];

        public CompletedPatientsPageModel(PatientRepository patientRepository, PartographRepository partographRepository, ModalErrorHandler errorHandler)
        {
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
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
                _allPatients = await _partographRepository.ListAsync(LaborStatus.SecondStage);
                FilterPatients();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void FilterPatients()
        {
            //&& SelectedDate == DateTime.MinValue
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Partographs = _allPatients.OrderByDescending(p => p.DeliveryTime).ToList();
                return;
            }

            var filtered = _allPatients.Where(p =>
            {
                // Filter by search text
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    if (!p.Patient.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) &&
                        !p.Patient.HospitalNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                // Filter by selected date
                if (SelectedDate != DateTime.MinValue && p.DeliveryTime?.Date != SelectedDate.Date)
                {
                    return false;
                }

                return true;
            });

            Partographs = filtered.OrderByDescending(p => p.DeliveryTime).ToList();
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterPatients();
        }

        partial void OnSelectedDateChanged(DateTime value)
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
        private Task ViewPartograph(Patient patient)
            => Shell.Current.GoToAsync($"partograph?id={patient.ID}");

        [RelayCommand]
        private Task GenerateReport(Patient patient)
        {
            // Generate delivery report
            return AppShell.DisplayToastAsync("Delivery report generation coming soon");
        }

        [RelayCommand]
        private async Task ExportData()
        {
            // Export completed cases data
            await AppShell.DisplayToastAsync("Data export feature coming soon");
        }
    }
}
