using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public class CompletedPatientItem : ObservableObject
    {
        public Partograph Partograph { get; set; }
        public BirthOutcome BirthOutcome { get; set; }
        public List<BabyDetails> Babies { get; set; } = [];

        public string PatientName => Partograph?.Name ?? "";
        public string PatientInfo => Partograph?.DisplayInfo ?? "";
        public DateTime? DeliveryTime => BirthOutcome?.DeliveryTime;
        public string DeliveryTimeDisplay => DeliveryTime?.ToString("dddd dd, MMM yyyy HH:mm") ?? "N/A";
        public string DeliveryModeDisplay => BirthOutcome?.DeliveryMode.ToString() ?? "N/A";
        public int NumberOfBabies => BirthOutcome?.NumberOfBabies ?? 0;
        public string BabiesDisplay => NumberOfBabies == 1 ? "1 baby" : $"{NumberOfBabies} babies";
        public bool HasComplications => BirthOutcome?.PostpartumHemorrhage == true
            || BirthOutcome?.Eclampsia == true
            || BirthOutcome?.SepticShock == true
            || BirthOutcome?.ObstructedLabor == true
            || BirthOutcome?.RupturedUterus == true;
        public string ComplicationsDisplay
        {
            get
            {
                if (!HasComplications) return "No complications";
                var complications = new List<string>();
                if (BirthOutcome?.PostpartumHemorrhage == true) complications.Add("PPH");
                if (BirthOutcome?.Eclampsia == true) complications.Add("Eclampsia");
                if (BirthOutcome?.SepticShock == true) complications.Add("Septic Shock");
                if (BirthOutcome?.ObstructedLabor == true) complications.Add("Obstructed Labor");
                if (BirthOutcome?.RupturedUterus == true) complications.Add("Ruptured Uterus");
                return string.Join(", ", complications);
            }
        }
        public Color ComplicationsColor => HasComplications ? Colors.Red : Colors.Green;
        public string StatusColor => Partograph?.StatusColor.ToString() ?? "#2196F3";
    }

    public partial class CompletedPatientsPageModel : ObservableObject
    {
        private readonly PatientRepository _patientRepository;
        private readonly PartographRepository _partographRepository;
        private readonly BirthOutcomeRepository _birthOutcomeRepository;
        private readonly BabyDetailsRepository _babyDetailsRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private List<CompletedPatientItem> _partographs = [];

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isRefreshing;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;

        private List<CompletedPatientItem> _allPatients = [];

        public CompletedPatientsPageModel(
            PatientRepository patientRepository,
            PartographRepository partographRepository,
            BirthOutcomeRepository birthOutcomeRepository,
            BabyDetailsRepository babyDetailsRepository,
            ModalErrorHandler errorHandler)
        {
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
            _birthOutcomeRepository = birthOutcomeRepository;
            _babyDetailsRepository = babyDetailsRepository;
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
                // Load only completed deliveries (WHO Four-Stage System)
                var partographs = await _partographRepository.ListAsync(LaborStatus.Completed);

                // Load birth outcomes and baby details for each partograph
                var completedPatients = new List<CompletedPatientItem>();
                foreach (var partograph in partographs)
                {
                    var birthOutcome = await _birthOutcomeRepository.GetByPartographIdAsync(partograph.ID);
                    var babies = birthOutcome != null
                        ? await _babyDetailsRepository.GetByBirthOutcomeIdAsync(birthOutcome.ID)
                        : new List<BabyDetails>();

                    completedPatients.Add(new CompletedPatientItem
                    {
                        Partograph = partograph,
                        BirthOutcome = birthOutcome,
                        Babies = babies
                    });
                }

                _allPatients = completedPatients;
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
                    if (!p.Partograph.Patient.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) &&
                        !p.Partograph.Patient.HospitalNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
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
        private Task NavigateToPatient(CompletedPatientItem item)
            => Shell.Current.GoToAsync($"patient?id={item.Partograph.PatientID}");

        [RelayCommand]
        private Task ViewPartograph(CompletedPatientItem item)
            => Shell.Current.GoToAsync($"partograph?id={item.Partograph.ID}");

        [RelayCommand]
        private Task ViewSecondStagePartograph(CompletedPatientItem item)
            => Shell.Current.GoToAsync($"secondstagepartograph?patientId={item.Partograph.ID}");

        [RelayCommand]
        private Task GenerateReport(CompletedPatientItem item)
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
