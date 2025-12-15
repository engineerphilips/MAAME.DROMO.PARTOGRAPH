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
    public partial class CompanionModalPageModel : ObservableObject
    {
        public Partograph? _patient;
        private readonly CompanionRepository _companionRepository;
        private readonly ModalErrorHandler _errorHandler;

        public Action? ClosePopup { get; set; }

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        [ObservableProperty]
        private int _companionIndex = -1;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        [ObservableProperty]
        private string _companionDisplay = string.Empty;

        // WHO 2020 Enhancements
        [ObservableProperty]
        private bool _companionPresent;

        [ObservableProperty]
        private string _companionType = string.Empty;

        [ObservableProperty]
        private int _numberOfCompanions;

        [ObservableProperty]
        private string _companionName = string.Empty;

        [ObservableProperty]
        private string _companionRelationship = string.Empty;

        [ObservableProperty]
        private DateTime? _arrivalTime;

        [ObservableProperty]
        private DateTime? _departureTime;

        [ObservableProperty]
        private int? _durationMinutes;

        [ObservableProperty]
        private bool _continuousPresence;

        [ObservableProperty]
        private string _participationLevel = string.Empty;

        [ObservableProperty]
        private string _supportActivities = string.Empty;

        [ObservableProperty]
        private bool _patientRequestedCompanion;

        [ObservableProperty]
        private bool _patientDeclinedCompanion;

        [ObservableProperty]
        private string _reasonForNoCompanion = string.Empty;

        [ObservableProperty]
        private bool _staffOrientedCompanion;

        [ObservableProperty]
        private bool _companionInvolvedInDecisions;

        [ObservableProperty]
        private bool _languageBarrier;

        [ObservableProperty]
        private bool _interpreterRequired;

        [ObservableProperty]
        private bool _culturalPractices;

        [ObservableProperty]
        private string _culturalPracticesDetails = string.Empty;

        [ObservableProperty]
        private string _clinicalAlert = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public CompanionModalPageModel(CompanionRepository companionRepository, ModalErrorHandler errorHandler)
        {
            _companionRepository = companionRepository;
            _errorHandler = errorHandler;
            RecordedBy = Preferences.Get("StaffName", "Staff");
        }

        public async Task LoadPatient(Guid? patientId)
        {
            try
            {
                PatientName = $"Patient ID: {patientId}";
                var lastEntry = await _companionRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    CompanionDisplay = lastEntry?.CompanionDisplay;
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

            if (CompanionIndex < 0)
            {
                _errorHandler.HandleError(new Exception("Companion status is not selected."));
                return;
            }

            try
            {
                IsBusy = true;

                var entry = new CompanionEntry
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    Companion = CompanionIndex == 0 ? "N" : CompanionIndex == 1 ? "Y" : CompanionIndex == 2 ? "D" : null,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID,
                    // WHO 2020 Enhancements
                    CompanionPresent = CompanionPresent,
                    CompanionType = CompanionType,
                    NumberOfCompanions = NumberOfCompanions,
                    CompanionName = CompanionName,
                    CompanionRelationship = CompanionRelationship,
                    ArrivalTime = ArrivalTime,
                    DepartureTime = DepartureTime,
                    DurationMinutes = DurationMinutes,
                    ContinuousPresence = ContinuousPresence,
                    ParticipationLevel = ParticipationLevel,
                    SupportActivities = SupportActivities,
                    PatientRequestedCompanion = PatientRequestedCompanion,
                    PatientDeclinedCompanion = PatientDeclinedCompanion,
                    ReasonForNoCompanion = ReasonForNoCompanion,
                    StaffOrientedCompanion = StaffOrientedCompanion,
                    CompanionInvolvedInDecisions = CompanionInvolvedInDecisions,
                    LanguageBarrier = LanguageBarrier,
                    InterpreterRequired = InterpreterRequired,
                    CulturalPractices = CulturalPractices,
                    CulturalPracticesDetails = CulturalPracticesDetails,
                    ClinicalAlert = ClinicalAlert
                };

                if (await _companionRepository.SaveItemAsync(entry) != null)
                {
                    await AppShell.DisplayToastAsync("Companion assessment saved successfully");
                    ResetFields();
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("Companion assessment failed to save");
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
            CompanionIndex = -1;
            Notes = string.Empty;
            // WHO 2020 fields
            CompanionPresent = false;
            CompanionType = string.Empty;
            NumberOfCompanions = 0;
            CompanionName = string.Empty;
            CompanionRelationship = string.Empty;
            ArrivalTime = null;
            DepartureTime = null;
            DurationMinutes = null;
            ContinuousPresence = false;
            ParticipationLevel = string.Empty;
            SupportActivities = string.Empty;
            PatientRequestedCompanion = false;
            PatientDeclinedCompanion = false;
            ReasonForNoCompanion = string.Empty;
            StaffOrientedCompanion = false;
            CompanionInvolvedInDecisions = false;
            LanguageBarrier = false;
            InterpreterRequired = false;
            CulturalPractices = false;
            CulturalPracticesDetails = string.Empty;
            ClinicalAlert = string.Empty;
        }
    }
}
