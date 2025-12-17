using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public partial class MouldingModalPageModel : ObservableObject
    {
        public Partograph? _patient;
        private readonly MouldingRepository _mouldingRepository;
        private readonly ModalErrorHandler _errorHandler;

        public MouldingModalPageModel(MouldingRepository repository, ModalErrorHandler errorHandler)
        {
            _mouldingRepository = repository;
            _errorHandler = errorHandler;
        }

        /// <summary>
        /// Collection of measurement history items for display
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<MeasurementHistoryItem> _measurementHistory = new();

        /// <summary>
        /// Indicates whether there is any history to display
        /// </summary>
        [ObservableProperty]
        private bool _hasHistory;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        [ObservableProperty]
        private int _degreeIndex = -1;

        [ObservableProperty]
        private string _degreeDisplay = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        // WHO 2020 Enhancements
        [ObservableProperty]
        private bool _suturesOverlapping;

        [ObservableProperty]
        private bool _reducible;

        [ObservableProperty]
        private string _location = string.Empty;

        [ObservableProperty]
        private bool _sagittalSuture;

        [ObservableProperty]
        private bool _coronalSuture;

        [ObservableProperty]
        private bool _lambdoidSuture;

        [ObservableProperty]
        private string _severity = string.Empty;

        [ObservableProperty]
        private bool _increasing;

        [ObservableProperty]
        private bool _reducing;

        [ObservableProperty]
        private bool _stable;

        [ObservableProperty]
        private string _progressionRate = string.Empty;

        [ObservableProperty]
        private DateTime? _firstDetectedTime;

        [ObservableProperty]
        private int? _durationHours;

        [ObservableProperty]
        private bool _caputPresent;

        [ObservableProperty]
        private string _caputDegree = string.Empty;

        [ObservableProperty]
        private bool _suggestsObstruction;

        [ObservableProperty]
        private bool _suggestsCPD;

        [ObservableProperty]
        private string _changeFromPrevious = string.Empty;

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

                // Load measurement history
                await LoadMeasurementHistory(patientId);

                // Load last moulding entry to prefill some values
                var lastEntry = await _mouldingRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    DegreeIndex = lastEntry.Degree == "0" ? 0 : lastEntry.Degree == "+" ? 1 : lastEntry.Degree == "++" ? 2 : lastEntry.Degree == "+++" ? 3 : -1;
                    DegreeDisplay = lastEntry.DegreeDisplay;
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
        }

        /// <summary>
        /// Loads the measurement history for display in the modal.
        /// Shows values, date/time (time only if today), and who recorded it.
        /// </summary>
        private async Task LoadMeasurementHistory(Guid? patientId)
        {
            try
            {
                var historyEntries = await _mouldingRepository.ListByPatientAsync(patientId);

                MeasurementHistory.Clear();

                foreach (var entry in historyEntries.Take(10)) // Show last 10 entries
                {
                    var historyItem = new MeasurementHistoryItem(
                        entry.Degree ?? "N/A",
                        entry.Time,
                        entry.HandlerName ?? "Unknown"
                    );
                    MeasurementHistory.Add(historyItem);
                }

                HasHistory = MeasurementHistory.Count > 0;
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

                var entry = new Moulding
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    Degree = DegreeIndex == 0 ? "0" : DegreeIndex == 1 ? "+" : DegreeIndex == 2 ? "++" : DegreeIndex == 3 ? "+++" : string.Empty,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID,
                    // WHO 2020 Enhancements
                    SuturesOverlapping = SuturesOverlapping,
                    Reducible = Reducible,
                    Location = Location,
                    SagittalSuture = SagittalSuture,
                    CoronalSuture = CoronalSuture,
                    LambdoidSuture = LambdoidSuture,
                    Severity = Severity,
                    Increasing = Increasing,
                    Reducing = Reducing,
                    Stable = Stable,
                    ProgressionRate = ProgressionRate,
                    FirstDetectedTime = FirstDetectedTime,
                    DurationHours = DurationHours,
                    CaputPresent = CaputPresent,
                    CaputDegree = CaputDegree,
                    SuggestsObstruction = SuggestsObstruction,
                    SuggestsCPD = SuggestsCPD,
                    ChangeFromPrevious = ChangeFromPrevious,
                    ClinicalAlert = ClinicalAlert
                };

                if (await _mouldingRepository.SaveItemAsync(entry) != null)
                {
                    await AppShell.DisplayToastAsync("Moulding assessment saved successfully");

                    // Reset fields to default
                    ResetFields();

                    // Close the popup
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("Moulding assessment failed to save");
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
            SuturesOverlapping = false;
            Reducible = false;
            Location = string.Empty;
            SagittalSuture = false;
            CoronalSuture = false;
            LambdoidSuture = false;
            Severity = string.Empty;
            Increasing = false;
            Reducing = false;
            Stable = false;
            ProgressionRate = string.Empty;
            FirstDetectedTime = null;
            DurationHours = null;
            CaputPresent = false;
            CaputDegree = string.Empty;
            SuggestsObstruction = false;
            SuggestsCPD = false;
            ChangeFromPrevious = string.Empty;
            ClinicalAlert = string.Empty;
        }
    }
}
