using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public partial class TemperatureModalPageModel : ObservableObject
    {
        public Partograph? _patient;
        private readonly TemperatureRepository _temperatureRepository;
        private readonly ModalErrorHandler _errorHandler;

        public TemperatureModalPageModel(TemperatureRepository repository, ModalErrorHandler errorHandler)
        {
            _temperatureRepository = repository;
            _errorHandler = errorHandler;
        }

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        //[ObservableProperty]
        //private float? _rate;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        // WHO 2020 Enhanced Temperature Assessment Fields

        // Original fields
        [ObservableProperty]
        private float? _temperatureCelsius;

        [ObservableProperty]
        private string _measurementSite = "Oral";

        [ObservableProperty]
        private int? _feverDurationHours;

        [ObservableProperty]
        private bool _chillsPresent;

        [ObservableProperty]
        private string _associatedSymptoms = string.Empty;

        [ObservableProperty]
        private bool _repeatedMeasurement;

        [ObservableProperty]
        private string _clinicalAlert = string.Empty;

        // Fever Classification
        [ObservableProperty]
        private string _feverCategory = string.Empty;

        [ObservableProperty]
        private bool _intrapartumFever;

        [ObservableProperty]
        private DateTime? _feverOnsetTime;

        [ObservableProperty]
        private float? _peakTemperature;

        [ObservableProperty]
        private DateTime? _peakTemperatureTime;

        // Repeat Measurements
        [ObservableProperty]
        private float? _secondTemperature;

        [ObservableProperty]
        private DateTime? _secondReadingTime;

        [ObservableProperty]
        private float? _thirdTemperature;

        [ObservableProperty]
        private DateTime? _thirdReadingTime;

        // Infection Screening
        [ObservableProperty]
        private bool _choriamnionitisRisk;

        [ObservableProperty]
        private bool _prolongedRupture;

        [ObservableProperty]
        private int? _hoursSinceRupture;

        [ObservableProperty]
        private bool _maternalTachycardia;

        [ObservableProperty]
        private bool _fetalTachycardia;

        [ObservableProperty]
        private bool _uterineTenderness;

        [ObservableProperty]
        private bool _offensiveLiquor;

        // Associated Symptoms Details
        [ObservableProperty]
        private bool _rigorPresent;

        [ObservableProperty]
        private bool _sweating;

        [ObservableProperty]
        private bool _headache;

        [ObservableProperty]
        private bool _myalgiaArthralgia;

        // Sepsis Screening
        [ObservableProperty]
        private bool _sepsisScreeningDone;

        [ObservableProperty]
        private DateTime? _sepsisScreeningTime;

        [ObservableProperty]
        private string _sepsisRiskLevel = string.Empty;

        [ObservableProperty]
        private bool _qsofaPositive;

        [ObservableProperty]
        private int? _qsofaScore;

        // Clinical Response
        [ObservableProperty]
        private bool _antipyreticsGiven;

        [ObservableProperty]
        private string _antipyreticType = string.Empty;

        [ObservableProperty]
        private DateTime? _antipyreticGivenTime;

        [ObservableProperty]
        private bool _culturesObtained;

        [ObservableProperty]
        private bool _antibioticsStarted;

        [ObservableProperty]
        private DateTime? _antibioticsStartTime;

        [ObservableProperty]
        private bool _ivFluidsGiven;

        [ObservableProperty]
        private bool _coolingMeasures;

        // Monitoring Frequency
        [ObservableProperty]
        private bool _increasedMonitoring;

        [ObservableProperty]
        private int? _monitoringIntervalMinutes;

        public Action? ClosePopup { get; set; }

        internal async Task LoadPatient(Guid? patientId)
        {
            try
            {
                // This would typically load from PatientRepository
                // For now, we'll use the patient ID directly
                PatientName = $"Patient ID: {patientId}";

                // Load last temperature entry to prefill some values
                var lastEntry = await _temperatureRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    TemperatureCelsius = lastEntry.TemperatureCelsius;
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (_patient == null)
            {
                _errorHandler.HandleError(new Exception("Patient information not loaded."));
                return;
            }

            try
            {
                IsBusy = true;

                var entry = new Temperature
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    TemperatureCelsius = TemperatureCelsius ?? 0,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID
                };

                if (await _temperatureRepository.SaveItemAsync(entry) != null)
                {
                    await AppShell.DisplayToastAsync("Temperature assessment saved successfully");

                    // Reset fields to default
                    ResetFields();

                    // Close the popup
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("Temperature assessment failed to save");
                }
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

        [RelayCommand]
        private void Cancel()
        {
            ResetFields();
            ClosePopup?.Invoke();
        }

        private void ResetFields()
        {
            RecordingDate = DateOnly.FromDateTime(DateTime.Now);
            RecordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            TemperatureCelsius = null;
            Notes = string.Empty;
        }
    }
}
