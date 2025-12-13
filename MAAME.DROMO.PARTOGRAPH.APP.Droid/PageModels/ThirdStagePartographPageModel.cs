using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class ThirdStagePartographPageModel : ObservableObject, IQueryAttributable
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
        private string _thirdStageDuration = string.Empty;

        [ObservableProperty]
        private DateTime? _lastRecordedTime;

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        private string _placentaStatus = "Awaiting Delivery";

        [ObservableProperty]
        private string _bloodLoss = "0 mL";

        [ObservableProperty]
        private string _uterineStatus = "Monitoring";

        public ThirdStagePartographPageModel(
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

                    if (Patient.ThirdStageStartTime.HasValue)
                    {
                        var duration = DateTime.Now - Patient.ThirdStageStartTime.Value;
                        ThirdStageDuration = $"{duration.Hours:D2}h {duration.Minutes:D2}m";
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
        private async Task RecordPlacentaDelivery()
        {
            try
            {
                if (Patient != null)
                {
                    Patient.DeliveryTime = DateTime.Now;
                    await _partographRepository.SaveItemAsync(Patient);
                    PlacentaStatus = $"Delivered at {DateTime.Now:HH:mm}";
                    await AppShell.DisplayToastAsync("Placenta delivery recorded");
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        [RelayCommand]
        private async Task TransitionToFourthStage()
        {
            try
            {
                if (Patient != null)
                {
                    Patient.Status = LaborStatus.FourthStage;
                    Patient.FourthStageStartTime = DateTime.Now;
                    await _partographRepository.SaveItemAsync(Patient);
                    await AppShell.DisplayToastAsync("Transitioned to Fourth Stage");
                    await Shell.Current.GoToAsync($"fourthpartograph?patientId={Patient.ID}");
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
