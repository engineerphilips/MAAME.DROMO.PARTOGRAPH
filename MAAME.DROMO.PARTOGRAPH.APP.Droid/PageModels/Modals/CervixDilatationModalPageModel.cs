using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public partial class CervixDilatationModalPageModel : ObservableObject
    {
        public Partograph? _patient;
        private readonly CervixDilatationRepository _cervixDilatationRepository;
        private readonly PartographRepository _partographRepository;
        private readonly StageProgressionService _stageProgressionService;
        private readonly ModalErrorHandler _errorHandler;

        public Action? ClosePopup { get; set; }

        /// <summary>
        /// Event raised when stage should be progressed to SecondStage
        /// </summary>
        public event EventHandler<Guid>? OnProgressToSecondStage;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

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

        /// <summary>
        /// The minimum allowed dilatation value based on previous measurements.
        /// Labor should not regress - dilatation can only increase or stay the same.
        /// </summary>
        [ObservableProperty]
        private int _minimumDilatation;

        /// <summary>
        /// Validation message shown when user tries to enter invalid value
        /// </summary>
        [ObservableProperty]
        private string _validationMessage = string.Empty;

        /// <summary>
        /// Whether the validation message should be shown
        /// </summary>
        [ObservableProperty]
        private bool _showValidationMessage;

        private int _dilatation;

        public int Dilatation
        {
            get => _dilatation;
            set
            {
                // Validate that dilatation doesn't decrease (labor progression rule)
                if (value < MinimumDilatation && value >= 0)
                {
                    ValidationMessage = $"Dilatation cannot decrease. Last recorded value was {MinimumDilatation} cm. Labor should only progress forward.";
                    ShowValidationMessage = true;
                    return;
                }
                ShowValidationMessage = false;
                ValidationMessage = string.Empty;
                SetProperty(ref _dilatation, value);
            }
        }

        [ObservableProperty]
        private string _dilatationDisplay = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        // Advanced Fields Toggle
        [ObservableProperty]
        private bool _showAdvancedFields = false;

        // Segmented Control Indices
        [ObservableProperty]
        private int _consistencyIndex = -1;

        [ObservableProperty]
        private int _positionIndex = -1;

        // WHO 2020 Enhanced Cervical Dilatation Assessment Fields

        // Original fields (DilatationCm already exists above)
        [ObservableProperty]
        private int _effacementPercent;

        [ObservableProperty]
        private string _consistency = "Medium";

        [ObservableProperty]
        private string _position = "Mid";

        [ObservableProperty]
        private bool _applicationToHead;

        [ObservableProperty]
        private string _cervicalEdema = "None";

        [ObservableProperty]
        private string _membraneStatus = string.Empty;

        [ObservableProperty]
        private bool _cervicalLip;

        [ObservableProperty]
        private string _clinicalAlert = string.Empty;

        // Progress Tracking
        [ObservableProperty]
        private decimal? _dilatationRateCmPerHour;

        [ObservableProperty]
        private string _progressionRate = string.Empty;

        [ObservableProperty]
        private bool _crossedActionLine;

        [ObservableProperty]
        private bool _crossedAlertLine;

        [ObservableProperty]
        private DateTime? _actionLineCrossedTime;

        // Cervical Length
        [ObservableProperty]
        private decimal? _cervicalLengthCm;

        // Examination Details
        [ObservableProperty]
        private string _examinerName = string.Empty;

        [ObservableProperty]
        private int? _examDurationMinutes;

        [ObservableProperty]
        private bool _difficultExam;

        [ObservableProperty]
        private string _examDifficulty = string.Empty;

        // Cervical Features
        [ObservableProperty]
        private string _cervicalThickness = string.Empty;

        [ObservableProperty]
        private bool _anteriorCervicalLip;

        [ObservableProperty]
        private bool _posteriorCervicalLip;

        [ObservableProperty]
        private string _cervicalDilatationPattern = string.Empty;

        // Presenting Part Relationship
        [ObservableProperty]
        private int? _stationRelativeToPelvicSpines;

        [ObservableProperty]
        private string _presentingPartPosition = string.Empty;

        [ObservableProperty]
        private bool _presentingPartWellApplied;

        // Membrane Assessment
        [ObservableProperty]
        private bool _membranesBulging;

        [ObservableProperty]
        private bool _forewatersPresent;

        [ObservableProperty]
        private bool _hindwatersPresent;

        // Clinical Alerts
        [ObservableProperty]
        private bool _prolongedLatentPhase;

        [ObservableProperty]
        private bool _protractedActivePhase;

        [ObservableProperty]
        private bool _arrestedDilatation;

        [ObservableProperty]
        private bool _precipitousLabor;

        public CervixDilatationModalPageModel(
            CervixDilatationRepository cervixDilatationRepository,
            PartographRepository partographRepository,
            StageProgressionService stageProgressionService,
            ModalErrorHandler errorHandler)
        {
            _cervixDilatationRepository = cervixDilatationRepository;
            _partographRepository = partographRepository;
            _stageProgressionService = stageProgressionService;
            _errorHandler = errorHandler;

            // Set default recorded by from preferences
            RecordedBy = Preferences.Get("StaffName", "Staff");
        }

        //public void ApplyQueryAttributes(IDictionary<string, object> query)
        //{
        //    if (query.ContainsKey("patientId"))
        //    {
        //        Guid? patientId = Guid.Parse(Convert.ToString(query["patientId"]));
        //        LoadPatient(patientId).FireAndForgetSafeAsync(_errorHandler);
        //    }
        //}

        private string _stageDescription = "Labour not started";
        public string StageDescription
        {
            get => _stageDescription;
            set { _stageDescription = value; OnPropertyChanged(); }
        }

        private Color _dilatationColor = Colors.Gray;
        public Color DilatationColor
        {
            get => _dilatationColor;
            set { _dilatationColor = value; OnPropertyChanged(); }
        }

        private bool _fullyDilated;
        public bool FullyDilated
        {
            get => _fullyDilated;
            set
            {
                _fullyDilated = value;
                if (value) Dilatation = 10;
                OnPropertyChanged();
            }
        }

        private Color _progressColor = Colors.Blue;
        public Color ProgressColor
        {
            get => _progressColor;
            set { _progressColor = value; OnPropertyChanged(); }
        }

        private string _progressMessage = "Monitor progress";
        public string ProgressMessage
        {
            get => _progressMessage;
            set { _progressMessage = value; OnPropertyChanged(); }
        }

        private void InitializeData()
        {
            UpdateDilatationDisplay();
            UpdateProgressMessage();
        }

        private void UpdateDilatationDisplay()
        {
            DilatationDisplay = $"{Dilatation:F1} cm";

            if (Dilatation == 10)
            {
                StageDescription = "Fully dilated - Second stage";
                DilatationColor = Colors.Green;
                FullyDilated = true;
            }
            else if (Dilatation >= 4)
            {
                StageDescription = "Active labor";
                DilatationColor = Colors.Orange;
            }
            else if (Dilatation > 0)
            {
                StageDescription = "Latent phase";
                DilatationColor = Colors.Blue;
            }
            else
            {
                StageDescription = "Labour not started";
                DilatationColor = Colors.Gray;
            }
        }

        private void UpdateProgressMessage()
        {
            // Expected dilatation rate is 1cm/hour in active labor
            if (Dilatation >= 10)
            {
                ProgressColor = Colors.Green;
                ProgressMessage = "✓ Fully dilated - Ready for second stage";
            }
            else if (Dilatation >= 4)
            {
                ProgressColor = Colors.Green;
                ProgressMessage = "Good progress - Active labor";
            }
            else if (Dilatation > 0)
            {
                ProgressColor = Colors.Blue;
                ProgressMessage = "Latent phase - Monitor progress";
            }
            else
            {
                ProgressColor = Colors.Gray;
                ProgressMessage = "Cervix not dilated";
            }

            // Auto-calculate labor progress alerts
            AutoCalculateLaborProgress();
        }

        /// <summary>
        /// Auto-calculate labor progress alerts based on dilatation and rate
        /// </summary>
        private void AutoCalculateLaborProgress()
        {
            // Calculate dilatation rate if available
            if (DilatationRateCmPerHour.HasValue)
            {
                if (DilatationRateCmPerHour < 1.0m && Dilatation >= 4)
                {
                    // Protracted active phase (<1 cm/hour in active labor)
                    ProtractedActivePhase = true;
                    ProgressionRate = "Slow (<1 cm/hour)";
                }
                else if (DilatationRateCmPerHour == 0 && Dilatation >= 4)
                {
                    // Arrested dilatation (no progress)
                    ArrestedDilatation = true;
                    ProgressionRate = "Arrested (No progress)";
                }
                else if (DilatationRateCmPerHour >= 1.0m)
                {
                    ProtractedActivePhase = false;
                    ArrestedDilatation = false;
                    ProgressionRate = "Normal (≥1 cm/hour)";
                }
            }

            // Auto-flag prolonged latent phase if dilatation <4cm for extended time
            if (Dilatation < 4 && Dilatation > 0)
            {
                // This would need historical data to determine if it's truly prolonged
                // For now, we just note it's in latent phase
                ProlongedLatentPhase = false; // Would need time tracking
            }
        }

        public async Task LoadPatient(Guid? patientId)
        {

            try
            {
                // This would typically load from PatientRepository
                // For now, we'll use the patient ID directly
                PatientName = $"Patient ID: {patientId}";

                // Load measurement history
                await LoadMeasurementHistory(patientId);

                // Load last entry to set minimum dilatation (labor progression rule)
                var lastEntry = await _cervixDilatationRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    // Set minimum dilatation - labor should not regress
                    MinimumDilatation = lastEntry.DilatationCm;
                    // Pre-fill with last known value
                    _dilatation = lastEntry.DilatationCm;
                    OnPropertyChanged(nameof(Dilatation));
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
                var historyEntries = await _cervixDilatationRepository.ListByPatientAsync(patientId);

                MeasurementHistory.Clear();

                foreach (var entry in historyEntries.Take(10)) // Show last 10 entries
                {
                    var historyItem = new MeasurementHistoryItem(
                        $"{entry.DilatationCm} cm",
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

            if (Dilatation < 0)
            {
                _errorHandler.HandleError(new Exception("Dilatation status is not set."));
                return;
            }

            try
            {
                IsBusy = true;

                var entry = new CervixDilatation
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    DilatationCm = Dilatation,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID
                };

                if (await _cervixDilatationRepository.SaveItemAsync(entry) != null)
                {
                    await AppShell.DisplayToastAsync("Dilatation assessment saved successfully");

                    // Check if dilation is 10 and should auto-progress to SecondStage
                    if (Dilatation >= 10 && _patient.ID.HasValue)
                    {
                        await CheckAndProgressToSecondStageAsync(_patient.ID.Value);
                    }

                    // Reset fields to default
                    ResetFields();

                    // Close the popup
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("Head descent assessment failed to save");
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

        /// <summary>
        /// Checks if the partograph is in FirstStage and automatically progresses to SecondStage when dilation is 10
        /// </summary>
        private async Task CheckAndProgressToSecondStageAsync(Guid partographId)
        {
            try
            {
                // Reload the partograph to get current status
                var partograph = await _partographRepository.GetAsync(partographId);
                if (partograph == null) return;

                // Only auto-progress if currently in FirstStage or Pending
                if (partograph.Status == LaborStatus.FirstStage || partograph.Status == LaborStatus.Pending)
                {
                    // Use StageProgressionService to check and progress
                    var (shouldProgress, nextStage, reason) = _stageProgressionService.CheckAutomaticProgression(partograph);

                    if (shouldProgress && nextStage == LaborStatus.SecondStage)
                    {
                        var (success, message) = await _stageProgressionService.ProgressToNextStage(partograph, LaborStatus.SecondStage);

                        if (success)
                        {
                            await AppShell.DisplayToastAsync("Full dilation (10cm) - Automatically progressed to Second Stage");

                            // Notify listeners about the stage change
                            OnProgressToSecondStage?.Invoke(this, partographId);

                            // Ask user if they want to navigate to Second Stage page
                            var navigate = await Application.Current.MainPage.DisplayAlert(
                                "Second Stage Started",
                                "Cervix is fully dilated (10cm). The labor has progressed to Second Stage. Would you like to go to the Second Stage monitoring page?",
                                "Yes, Go to Second Stage",
                                "Stay Here");

                            if (navigate)
                            {
                                await Shell.Current.GoToAsync($"secondpartograph?id={partographId}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail the main save operation
                System.Diagnostics.Debug.WriteLine($"Error checking stage progression: {ex.Message}");
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
            Dilatation = -1;
            Notes = string.Empty;
        }
    }
}
