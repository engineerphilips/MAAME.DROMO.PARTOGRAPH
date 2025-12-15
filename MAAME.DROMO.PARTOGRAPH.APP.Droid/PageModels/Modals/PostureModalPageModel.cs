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
    public partial class PostureModalPageModel : ObservableObject
    {
        public Partograph? _patient;
        private readonly PostureRepository _postureRepository;
        private readonly ModalErrorHandler _errorHandler;

        public PostureModalPageModel(PostureRepository repository, ModalErrorHandler errorHandler)
        {
            _postureRepository = repository;
            _errorHandler = errorHandler;
            // Set default for PatientChoice
            PatientChoice = true;
        }

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        [ObservableProperty]
        private int _postureIndex = -1;

        [ObservableProperty]
        private string _postureDisplay = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        // WHO 2020 Enhancements
        [ObservableProperty]
        private string _postureCategory = string.Empty;

        [ObservableProperty]
        private DateTime? _startTime;

        [ObservableProperty]
        private DateTime? _endTime;

        [ObservableProperty]
        private int? _durationMinutes;

        [ObservableProperty]
        private string _reason = string.Empty;

        [ObservableProperty]
        private string _effectOnLabor = string.Empty;

        [ObservableProperty]
        private string _effectOnPain = string.Empty;

        [ObservableProperty]
        private string _effectOnContractions = string.Empty;

        [ObservableProperty]
        private bool _patientChoice = true;

        [ObservableProperty]
        private bool _medicallyIndicated;

        [ObservableProperty]
        private bool _mobileAndActive;

        [ObservableProperty]
        private bool _restrictedMobility;

        [ObservableProperty]
        private string _mobilityRestriction = string.Empty;

        [ObservableProperty]
        private string _supportEquipment = string.Empty;

        [ObservableProperty]
        private string _clinicalAlert = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public Action? ClosePopup { get; set; }

        internal async Task LoadPatient(Guid? patientId)
        {
            try
            {
                // This would typically load from PatientRepository
                // For now, we'll use the patient ID directly
                PatientName = $"Patient ID: {patientId}";

                // Load last pain relief entry to prefill some values
                var lastEntry = await _postureRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    PostureDisplay = lastEntry.PostureDisplay;
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

                var entry = new PostureEntry
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    Posture = PostureIndex == 0 ? "N" : PostureIndex == 1 ? "Y" : PostureIndex == 2 ? "D" : string.Empty,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID,
                    // WHO 2020 Enhancements
                    PostureCategory = PostureCategory,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    DurationMinutes = DurationMinutes,
                    Reason = Reason,
                    EffectOnLabor = EffectOnLabor,
                    EffectOnPain = EffectOnPain,
                    EffectOnContractions = EffectOnContractions,
                    PatientChoice = PatientChoice,
                    MedicallyIndicated = MedicallyIndicated,
                    MobileAndActive = MobileAndActive,
                    RestrictedMobility = RestrictedMobility,
                    MobilityRestriction = MobilityRestriction,
                    SupportEquipment = SupportEquipment,
                    ClinicalAlert = ClinicalAlert
                };

                if (await _postureRepository.SaveItemAsync(entry) != null)
                {
                    await AppShell.DisplayToastAsync("Posture assessment saved successfully");

                    // Reset fields to default
                    ResetFields();

                    // Close the popup
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("Posture assessment failed to save");
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
            PostureIndex = -1;
            Notes = string.Empty;
            // WHO 2020 fields
            PostureCategory = string.Empty;
            StartTime = null;
            EndTime = null;
            DurationMinutes = null;
            Reason = string.Empty;
            EffectOnLabor = string.Empty;
            EffectOnPain = string.Empty;
            EffectOnContractions = string.Empty;
            PatientChoice = true;
            MedicallyIndicated = false;
            MobileAndActive = false;
            RestrictedMobility = false;
            MobilityRestriction = string.Empty;
            SupportEquipment = string.Empty;
            ClinicalAlert = string.Empty;
        }
    }
}
