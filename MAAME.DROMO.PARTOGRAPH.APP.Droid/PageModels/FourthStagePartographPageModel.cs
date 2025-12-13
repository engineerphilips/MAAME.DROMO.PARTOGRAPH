using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class FourthStagePartographPageModel : ObservableObject, IQueryAttributable
    {
        public Partograph? Patient { get; set; }
        private readonly PatientRepository _patientRepository;
        private readonly PartographRepository _partographRepository;
        private readonly ModalErrorHandler _errorHandler;
        private readonly BPRepository _bpRepository;
        private readonly TemperatureRepository _temperatureRepository;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private string _patientInfo = string.Empty;

        [ObservableProperty]
        private string _fourthStageDuration = string.Empty;

        [ObservableProperty]
        private DateTime? _lastRecordedTime;

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        private string _fundalHeight = "At umbilicus";

        [ObservableProperty]
        private string _bleedingStatus = "Normal lochia";

        [ObservableProperty]
        private string _bladderStatus = "Empty";

        [ObservableProperty]
        private int _vitalSignsRecorded = 0;

        public FourthStagePartographPageModel(
            PatientRepository patientRepository,
            PartographRepository partographRepository,
            ModalErrorHandler errorHandler,
            BPRepository bpRepository,
            TemperatureRepository temperatureRepository)
        {
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
            _errorHandler = errorHandler;
            _bpRepository = bpRepository;
            _temperatureRepository = temperatureRepository;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("patientId"))
            {
                var patientId = Guid.Parse(query["patientId"].ToString());
                Task.Run(async () => await LoadPatientData(patientId));
            }
        }

        private async Task LoadPatientData(Guid patientId)
        {
            try
            {
                IsBusy = true;
                Patient = await _partographRepository.GetItemAsync(patientId);

                if (Patient?.Patient != null)
                {
                    PatientName = Patient.Name;
                    PatientInfo = Patient.DisplayInfo;

                    if (Patient.FourthStageStartTime.HasValue)
                    {
                        var duration = DateTime.Now - Patient.FourthStageStartTime.Value;
                        FourthStageDuration = $"{duration.Hours:D2}h {duration.Minutes:D2}m";
                    }
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RecordVitalSigns()
        {
            try
            {
                VitalSignsRecorded++;
                await AppShell.DisplayToastAsync($"Vital signs recorded ({VitalSignsRecorded}/8 required)");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        [RelayCommand]
        private async Task CompleteDelivery()
        {
            try
            {
                if (Patient != null)
                {
                    var confirm = await Shell.Current.DisplayAlert(
                        "Complete Delivery",
                        "Are you sure you want to mark the delivery as completed and move patient to postpartum ward?",
                        "Yes, Complete",
                        "Cancel");

                    if (confirm)
                    {
                        Patient.Status = LaborStatus.Completed;
                        Patient.CompletedTime = DateTime.Now;
                        await _partographRepository.SaveItemAsync(Patient);
                        await AppShell.DisplayToastAsync("Delivery completed successfully");
                        await Shell.Current.GoToAsync("//activepatients");
                    }
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        [RelayCommand]
        private async Task Print()
        {
            await AppShell.DisplayToastAsync("Print functionality coming soon");
        }
    }
}
