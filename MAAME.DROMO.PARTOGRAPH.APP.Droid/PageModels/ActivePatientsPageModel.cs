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
        private readonly CervixDilatationRepository _cervixDilatationRepository;
        private readonly HeadDescentRepository _headDescentRepository;
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

        public ActivePatientsPageModel(PartographRepository partographRepository, CervixDilatationRepository cervixDilatationRepository, HeadDescentRepository headDescentRepository, ModalErrorHandler errorHandler)
        {
            _partographRepository = partographRepository;
            _cervixDilatationRepository = cervixDilatationRepository;
            _headDescentRepository = headDescentRepository;
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

                // Load all active labor stages (WHO Four-Stage System)
                var firstStage = await _partographRepository.ListAsync(LaborStatus.FirstStage);
                var secondStage = await _partographRepository.ListAsync(LaborStatus.SecondStage);
                var thirdStage = await _partographRepository.ListAsync(LaborStatus.ThirdStage);
                var fourthStage = await _partographRepository.ListAsync(LaborStatus.FourthStage);
                var emergency = await _partographRepository.ListAsync(LaborStatus.Emergency);

                // Combine all active stages
                _allPartographs = new List<Partograph>();
                _allPartographs.AddRange(firstStage ?? []);
                _allPartographs.AddRange(secondStage ?? []);
                _allPartographs.AddRange(thirdStage ?? []);
                _allPartographs.AddRange(fourthStage ?? []);
                _allPartographs.AddRange(emergency ?? []);

                if (_allPartographs?.Count > 0)
                {
                    foreach (var item in _allPartographs)
                    {
                        item.Dilatations = await _cervixDilatationRepository.ListByPatientAsync(item.ID);
                        item.HeadDescents = await _headDescentRepository.ListByPatientAsync(item.ID);
                    }
                }

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
        public Task NavigateToPartograph(Partograph patient) => Shell.Current.GoToAsync($"partograph?patientId={patient.ID.ToString()}");

        [RelayCommand]
        private Task AddPartographEntry(Partograph patient)
            => Shell.Current.GoToAsync($"partographentry?patientId={patient.ID}");

        [RelayCommand]
        private async Task CompleteDelivery(Partograph partograph)
        {
            try
            {
                // Show confirmation dialog
                var confirm = await Shell.Current.DisplayAlert(
                    "Complete Delivery",
                    $"Are you sure you want to mark delivery as completed for {partograph.Name}?",
                    "Yes, Complete",
                    "Cancel");

                if (!confirm)
                    return;

                // Update partograph status
                partograph.Status = LaborStatus.Completed;
                partograph.DeliveryTime = DateTime.UtcNow;

                // Save to database (repository handles DeliveryTime setting)
                await _partographRepository.SaveItemAsync(partograph);

                // Show success message
                await AppShell.DisplayToastAsync($"Delivery completed for {partograph.Name}");

                // Reload data to refresh the list
                await LoadData();
            }
            catch (Exception ex)
            {
                // Handle errors
                _errorHandler.HandleError(ex);
            }
        }

        [RelayCommand]
        private async Task MarkAsEmergency(Partograph partograph)
        {
            try
            {
                // Show confirmation dialog
                var confirm = await Shell.Current.DisplayAlert(
                    "Mark as Emergency",
                    $"Are you sure you want to mark {partograph.Name} as an emergency?",
                    "Yes, Emergency",
                    "Cancel");

                if (!confirm)
                    return;

                // Update partograph status
                partograph.Status = LaborStatus.Emergency;

                // Save to database
                await _partographRepository.SaveItemAsync(partograph);

                // Show success message
                await AppShell.DisplaySnackbarAsync($"Emergency alert sent for {partograph.Name}");

                // Reload data to refresh the list
                await LoadData();
            }
            catch (Exception ex)
            {
                // Handle errors
                _errorHandler.HandleError(ex);
            }
        }
    }
}
