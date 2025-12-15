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
    public partial class CaputModalPageModel : ObservableObject, IQueryAttributable
    {
        public Partograph? _patient;
        private readonly CaputRepository _caputRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        [ObservableProperty]
        private int _degreeIndex = -1;

        [ObservableProperty]
        private string _caputDisplay = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        // WHO 2020 Enhancements
        [ObservableProperty]
        private string _location = string.Empty;

        [ObservableProperty]
        private string _size = string.Empty;

        [ObservableProperty]
        private string _consistency = string.Empty;

        [ObservableProperty]
        private bool _increasing;

        [ObservableProperty]
        private bool _decreasing;

        [ObservableProperty]
        private bool _stable;

        [ObservableProperty]
        private string _progressionRate = string.Empty;

        [ObservableProperty]
        private DateTime? _firstDetectedTime;

        [ObservableProperty]
        private int? _durationHours;

        [ObservableProperty]
        private bool _mouldingPresent;

        [ObservableProperty]
        private string _mouldingDegree = string.Empty;

        [ObservableProperty]
        private bool _suggestsObstruction;

        [ObservableProperty]
        private bool _suggestionProlongedLabor;

        [ObservableProperty]
        private string _changeFromPrevious = string.Empty;

        [ObservableProperty]
        private string _clinicalAlert = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public Action? ClosePopup { get; set; }

        public CaputModalPageModel(CaputRepository caputRepository, ModalErrorHandler errorHandler)
        {
            _caputRepository = caputRepository;
            _errorHandler = errorHandler;

            // Set default recorded by from preferences
            RecordedBy = Preferences.Get("StaffName", "Staff");
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("patientId"))
            {
                Guid? patientId = Guid.Parse(query["patientId"].ToString());
                LoadPatient(patientId).FireAndForgetSafeAsync(_errorHandler);
            }
        }

        internal async Task LoadPatient(Guid? patientId)
        {
            try
            {
                // This would typically load from PatientRepository
                // For now, we'll use the patient ID directly
                PatientName = $"Patient ID: {patientId}";

                // Load last pain relief entry to prefill some values
                var lastEntry = await _caputRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    CaputDisplay = lastEntry.CaputDisplay;
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

                var entry = new Caput
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    Degree = DegreeIndex == 0 ? "0" : DegreeIndex == 1 ? "+" : DegreeIndex == 2 ? "++" : DegreeIndex == 3 ? "+++" : null,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID,
                    // WHO 2020 Enhancements
                    Location = Location,
                    Size = Size,
                    Consistency = Consistency,
                    Increasing = Increasing,
                    Decreasing = Decreasing,
                    Stable = Stable,
                    ProgressionRate = ProgressionRate,
                    FirstDetectedTime = FirstDetectedTime,
                    DurationHours = DurationHours,
                    MouldingPresent = MouldingPresent,
                    MouldingDegree = MouldingDegree,
                    SuggestsObstruction = SuggestsObstruction,
                    SuggestionProlongedLabor = SuggestionProlongedLabor,
                    ChangeFromPrevious = ChangeFromPrevious,
                    ClinicalAlert = ClinicalAlert
                };

                if (await _caputRepository.SaveItemAsync(entry) != null)
                {
                    await AppShell.DisplayToastAsync("Caput assessment saved successfully");

                    // Reset fields to default
                    ResetFields();

                    // Close the popup
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("Caput assessment failed to save");
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
            DegreeIndex = -1;
            Notes = string.Empty;
            // WHO 2020 fields
            Location = string.Empty;
            Size = string.Empty;
            Consistency = string.Empty;
            Increasing = false;
            Decreasing = false;
            Stable = false;
            ProgressionRate = string.Empty;
            FirstDetectedTime = null;
            DurationHours = null;
            MouldingPresent = false;
            MouldingDegree = string.Empty;
            SuggestsObstruction = false;
            SuggestionProlongedLabor = false;
            ChangeFromPrevious = string.Empty;
            ClinicalAlert = string.Empty;
        }
    }
}
