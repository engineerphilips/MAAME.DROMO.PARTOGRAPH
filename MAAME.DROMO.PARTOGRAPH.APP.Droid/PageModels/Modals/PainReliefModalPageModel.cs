using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public partial class PainReliefModalPageModel : ObservableObject, IQueryAttributable
    {
        public Partograph? _patient;
        private readonly PainReliefRepository _painReliefRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        [ObservableProperty]
        private int _painReliefIndex = -1;

        [ObservableProperty]
        private string _painReliefDisplay = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        // WHO 2020 Enhancements
        [ObservableProperty]
        private int? _painScoreBefore;

        [ObservableProperty]
        private int? _painScoreAfter;

        [ObservableProperty]
        private string _painAssessmentTool = string.Empty;

        [ObservableProperty]
        private string _painReliefMethod = string.Empty;

        [ObservableProperty]
        private string _nonPharmacologicalMethods = string.Empty;

        [ObservableProperty]
        private DateTime? _administeredTime;

        [ObservableProperty]
        private string _administeredBy = string.Empty;

        [ObservableProperty]
        private string _dose = string.Empty;

        [ObservableProperty]
        private string _route = string.Empty;

        [ObservableProperty]
        private string _effectiveness = string.Empty;

        [ObservableProperty]
        private int? _timeToEffectMinutes;

        [ObservableProperty]
        private int? _durationOfEffectHours;

        [ObservableProperty]
        private bool _sideEffects;

        [ObservableProperty]
        private string _sideEffectsDescription = string.Empty;

        [ObservableProperty]
        private bool _continuousMonitoringRequired;

        [ObservableProperty]
        private bool _bladderCareRequired;

        [ObservableProperty]
        private DateTime? _lastTopUpTime;

        [ObservableProperty]
        private int? _topUpCount;

        [ObservableProperty]
        private bool _contraindicationsChecked;

        [ObservableProperty]
        private bool _contraindicationsPresent;

        [ObservableProperty]
        private string _contraindicationDetails = string.Empty;

        [ObservableProperty]
        private bool _informedConsentObtained;

        [ObservableProperty]
        private string _patientPreference = string.Empty;

        [ObservableProperty]
        private string _clinicalAlert = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public Action? ClosePopup { get; set; }

        public PainReliefModalPageModel(PainReliefRepository painReliefRepository, ModalErrorHandler errorHandler)
        {
            _painReliefRepository = painReliefRepository;
            _errorHandler = errorHandler;

            // Set default recorded by from preferences
            RecordedBy = Preferences.Get("StaffName", "Staff");
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("patientId"))
            {
                Guid? patientId = Guid.Parse(Convert.ToString(query["patientId"]));
                LoadPatient(patientId).FireAndForgetSafeAsync(_errorHandler);
            }
        }

        public async Task LoadPatient(Guid? patientId)
        {
            try
            {
                // This would typically load from PatientRepository
                // For now, we'll use the patient ID directly
                PatientName = $"Patient ID: {patientId}";

                // Load last pain relief entry to prefill some values
                var lastEntry = await _painReliefRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    PainReliefDisplay = lastEntry.PainReliefDisplay;
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

                var entry = new PainReliefEntry
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    PainRelief = PainReliefIndex == 0 ? "N" : PainReliefIndex == 1 ? "Y" : PainReliefIndex == 2 ? "D" : null,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID,
                    // WHO 2020 Enhancements
                    PainScoreBefore = PainScoreBefore,
                    PainScoreAfter = PainScoreAfter,
                    PainAssessmentTool = PainAssessmentTool,
                    PainReliefMethod = PainReliefMethod,
                    NonPharmacologicalMethods = NonPharmacologicalMethods,
                    AdministeredTime = AdministeredTime,
                    AdministeredBy = AdministeredBy,
                    Dose = Dose,
                    Route = Route,
                    Effectiveness = Effectiveness,
                    TimeToEffectMinutes = TimeToEffectMinutes,
                    DurationOfEffectHours = DurationOfEffectHours,
                    SideEffects = SideEffects,
                    SideEffectsDescription = SideEffectsDescription,
                    ContinuousMonitoringRequired = ContinuousMonitoringRequired,
                    BladderCareRequired = BladderCareRequired,
                    LastTopUpTime = LastTopUpTime,
                    TopUpCount = TopUpCount ?? 0,
                    ContraindicationsChecked = ContraindicationsChecked,
                    ContraindicationsPresent = ContraindicationsPresent,
                    ContraindicationDetails = ContraindicationDetails,
                    InformedConsentObtained = InformedConsentObtained,
                    PatientPreference = PatientPreference,
                    ClinicalAlert = ClinicalAlert
                };

                if (await _painReliefRepository.SaveItemAsync(entry) != null)
                {
                    await AppShell.DisplayToastAsync("Pain relief assessment saved successfully");

                    // Reset fields to default
                    ResetFields();

                    // Close the popup
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("Pain relief assessment failed to save");
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
            PainReliefIndex = -1;
            Notes = string.Empty;
            // WHO 2020 fields
            PainScoreBefore = null;
            PainScoreAfter = null;
            PainAssessmentTool = string.Empty;
            PainReliefMethod = string.Empty;
            NonPharmacologicalMethods = string.Empty;
            AdministeredTime = null;
            AdministeredBy = string.Empty;
            Dose = string.Empty;
            Route = string.Empty;
            Effectiveness = string.Empty;
            TimeToEffectMinutes = null;
            DurationOfEffectHours = null;
            SideEffects = false;
            SideEffectsDescription = string.Empty;
            ContinuousMonitoringRequired = false;
            BladderCareRequired = false;
            LastTopUpTime = null;
            TopUpCount = null;
            ContraindicationsChecked = false;
            ContraindicationsPresent = false;
            ContraindicationDetails = string.Empty;
            InformedConsentObtained = false;
            PatientPreference = string.Empty;
            ClinicalAlert = string.Empty;
        }
    }
}
