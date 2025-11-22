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
    public partial class ActivePatientsPageModel : ObservableObject
    {
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

        private List<Partograph> _allPartographs = [];

        public ActivePatientsPageModel(PartographRepository partographRepository, ModalErrorHandler errorHandler)
        {
            _partographRepository = partographRepository;
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
                _allPartographs = await _partographRepository.ListAsync(LaborStatus.Active);

                // Calculate hours in labor for each patient
                foreach (var patient in _allPartographs)
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
                Partographs = _allPartographs;
            }
            else
            {
                Partographs = _allPartographs.Where(p =>
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

        public Partograph? BindToPatient(Guid? id) => Partographs?.FirstOrDefault(p => p.ID == id);

        [RelayCommand]
        private Task NavigateToPatient(Partograph patient)
            => Shell.Current.GoToAsync($"patient?id={patient.ID}");

        [RelayCommand]
        public Task NavigateToPartograph(Partograph patient) => Shell.Current.GoToAsync($"partograph?patientId={patient.ID}");

        [RelayCommand]
        private Task AddPartographEntry(Partograph patient)
            => Shell.Current.GoToAsync($"partographentry?patientId={patient.ID}");

        [RelayCommand]
        private async Task CompleteDelivery(Partograph patient)
        {
            patient.Status = LaborStatus.Completed;
            patient.DeliveryTime = DateTime.Now;
            await _partographRepository.SaveItemAsync(patient);
            await AppShell.DisplayToastAsync($"Delivery completed for {patient.Name}");
            await LoadData();
        }

        [RelayCommand]
        private async Task MarkAsEmergency(Partograph patient)
        {
            patient.Status = LaborStatus.Emergency;
            await _partographRepository.SaveItemAsync(patient);
            await AppShell.DisplaySnackbarAsync($"Emergency alert sent for {patient.Name}");
            await LoadData();
        }
    }
}
