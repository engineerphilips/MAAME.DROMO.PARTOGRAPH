using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    // ContractionsModalPageModel
    public partial class FHRContractionModalPageModel : ObservableObject, IQueryAttributable
    {
        private readonly ContractionRepository _contractionRepository;
        private readonly FHRRepository _fHRRepository;
        private readonly FHRPatternAnalysisService _fhrAnalysisService;
        private readonly ILogger<FHRContractionModalPageModel> _logger;
        public Partograph? _patient;

        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        private int _frequencyPer10Min;
        public int FrequencyPer10Min
        {
            get => _frequencyPer10Min;
            set
            {
                SetProperty(ref _frequencyPer10Min, value);
                AutoCalculateContractionPatterns();
            }
        }

        [ObservableProperty]
        private int _durationSeconds;

        private int _rate;
        public int Rate
        {
            get => _rate;
            set
            {
                SetProperty(ref _rate, value);
                AutoCalculateFHRPatterns();
            }
        }

        [ObservableProperty]
        private string _contractionDisplay = string.Empty;

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
        private int _fHRRangeIndex = -1;

        [ObservableProperty]
        private int _frequencyRangeIndex = -1;

        [ObservableProperty]
        private int _durationRangeIndex = -1;

        // WHO 2020 Enhanced Contraction Assessment Fields

        // Original Contraction fields
        [ObservableProperty]
        private string _strength = "Moderate";

        [ObservableProperty]
        private string _regularity = "Regular";

        [ObservableProperty]
        private bool _palpableAtRest;

        [ObservableProperty]
        private bool _coordinated = true;

        [ObservableProperty]
        private string _effectOnCervix = string.Empty;

        [ObservableProperty]
        private int? _intensityMmHg;

        [ObservableProperty]
        private string _contractionClinicalAlert = string.Empty;

        // Pattern Analysis
        [ObservableProperty]
        private string _contractionPattern = string.Empty;

        [ObservableProperty]
        private bool _tachysystole;

        [ObservableProperty]
        private bool _hyperstimulation;

        [ObservableProperty]
        private DateTime? _tachysystoleStartTime;

        [ObservableProperty]
        private int? _tachysystoleDurationMinutes;

        // Intensity Details
        [ObservableProperty]
        private string _intensityAssessment = string.Empty;

        [ObservableProperty]
        private bool _indentableDuringContraction;

        [ObservableProperty]
        private bool _uterusRelaxesBetweenContractions = true;

        [ObservableProperty]
        private int? _relaxationTimeSeconds;

        // IUPC Measurements
        [ObservableProperty]
        private int? _restingToneMmHg;

        [ObservableProperty]
        private int? _peakPressureMmHg;

        [ObservableProperty]
        private int? _montevideUnits;

        // Duration Patterns
        [ObservableProperty]
        private int? _shortestDurationSeconds;

        [ObservableProperty]
        private int? _longestDurationSeconds;

        [ObservableProperty]
        private int? _averageDurationSeconds;

        [ObservableProperty]
        private bool _prolongedContractions;

        [ObservableProperty]
        private int? _prolongedContractionCount;

        // Frequency Patterns
        [ObservableProperty]
        private string _frequencyTrend = string.Empty;

        [ObservableProperty]
        private bool _irregularFrequency;

        [ObservableProperty]
        private decimal? _averageIntervalMinutes;

        // Maternal Response
        [ObservableProperty]
        private string _maternalCopingLevel = string.Empty;

        [ObservableProperty]
        private bool _maternalExhaustion;

        [ObservableProperty]
        private string _painLocation = string.Empty;

        // Oxytocin Relationship
        [ObservableProperty]
        private bool _onOxytocin;

        [ObservableProperty]
        private string _oxytocinEffect = string.Empty;

        [ObservableProperty]
        private bool _oxytocinAdjustmentNeeded;

        [ObservableProperty]
        private string _suggestedOxytocinAction = string.Empty;

        // Clinical Management
        [ObservableProperty]
        private bool _contractionInterventionRequired;

        [ObservableProperty]
        private string _contractionInterventionTaken = string.Empty;

        [ObservableProperty]
        private DateTime? _contractionInterventionTime;

        [ObservableProperty]
        private bool _oxytocinStopped;

        [ObservableProperty]
        private bool _oxytocinReduced;

        [ObservableProperty]
        private bool _tocolyticsGiven;

        // Safety Alerts
        [ObservableProperty]
        private bool _hypertonicUterus;

        [ObservableProperty]
        private bool _uterineRuptureRisk;

        [ObservableProperty]
        private bool _fhrCompromise;

        [ObservableProperty]
        private bool _contractionEmergencyConsultRequired;

        // WHO 2020 Enhanced FHR Assessment Fields

        // Original FHR fields (Rate already exists above)
        [ObservableProperty]
        private string _deceleration = "None";

        [ObservableProperty]
        private int _decelerationDurationSeconds;

        [ObservableProperty]
        private string _variability = string.Empty;

        [ObservableProperty]
        private bool _accelerations;

        [ObservableProperty]
        private string _pattern = string.Empty;

        [ObservableProperty]
        private string _monitoringMethod = string.Empty;

        [ObservableProperty]
        private int? _baselineRate;

        [ObservableProperty]
        private string _fhrClinicalAlert = string.Empty;

        // Detailed Variability Assessment
        [ObservableProperty]
        private int? _variabilityBpm;

        [ObservableProperty]
        private string _variabilityTrend = string.Empty;

        [ObservableProperty]
        private bool _sinusoidalPattern;

        [ObservableProperty]
        private bool _saltatorPattern;

        // Acceleration Details
        [ObservableProperty]
        private int? _accelerationCount;

        [ObservableProperty]
        private int? _accelerationPeakBpm;

        [ObservableProperty]
        private int? _accelerationDurationSeconds;

        // Deceleration Details
        [ObservableProperty]
        private int? _decelerationNadirBpm;

        [ObservableProperty]
        private string _decelerationRecovery = string.Empty;

        [ObservableProperty]
        private int? _decelerationAmplitudeBpm;

        [ObservableProperty]
        private string _decelerationTiming = string.Empty;

        // Bradycardia/Tachycardia
        [ObservableProperty]
        private bool _prolongedBradycardia;

        [ObservableProperty]
        private DateTime? _bradycardiaStartTime;

        [ObservableProperty]
        private int? _bradycardiaDurationMinutes;

        [ObservableProperty]
        private bool _tachycardia;

        [ObservableProperty]
        private DateTime? _tachycardiaStartTime;

        [ObservableProperty]
        private int? _tachycardiaDurationMinutes;

        // CTG Interpretation
        [ObservableProperty]
        private string _ctgClassification = string.Empty;

        [ObservableProperty]
        private bool _reactiveNST;

        [ObservableProperty]
        private DateTime? _lastReactiveTime;

        // Maternal Context
        [ObservableProperty]
        private string _maternalPosition = string.Empty;

        [ObservableProperty]
        private bool _duringContraction;

        [ObservableProperty]
        private bool _betweenContractions;

        // Clinical Response
        [ObservableProperty]
        private bool _fhrInterventionRequired;

        [ObservableProperty]
        private string _fhrInterventionTaken = string.Empty;

        [ObservableProperty]
        private DateTime? _fhrInterventionTime;

        [ObservableProperty]
        private bool _changeInPosition;

        [ObservableProperty]
        private bool _oxygenAdministered;

        [ObservableProperty]
        private bool _ivFluidsIncreased;

        // Obstetric Emergency Indicators
        [ObservableProperty]
        private bool _emergencyConsultRequired;

        [ObservableProperty]
        private string _consultReason = string.Empty;

        [ObservableProperty]
        private DateTime? _consultTime;

        [ObservableProperty]
        private bool _prepareForEmergencyDelivery;

        public Action? ClosePopup { get; set; }

        public FHRContractionModalPageModel(ContractionRepository contractionRepository, FHRRepository fHRRepository, FHRPatternAnalysisService fhrAnalysisService, ModalErrorHandler errorHandler)
        {
            _contractionRepository = contractionRepository;
            _fHRRepository = fHRRepository;
            _fhrAnalysisService = fhrAnalysisService;
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
                var lastContractionEntry = await _contractionRepository.GetLatestByPatientAsync(patientId);
                if (lastContractionEntry != null)
                {
                    //ContractionDisplay = lastContractionEntry.FrequencyPer10Min;
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

                var contractionEntry = new Contraction
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    FrequencyPer10Min = FrequencyPer10Min,
                    DurationSeconds = DurationSeconds,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID
                };

                var fhrEntry = new FHR
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    Rate = Rate,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID
                };

                // ===== AUTOMATIC FHR DECELERATION DETECTION =====
                // Analyze FHR pattern based on current reading and recent history
                try
                {
                    // Get recent FHR readings for this patient (last 5 readings)
                    var fHRs = await _fHRRepository.ListByPatientAsync(_patient.ID);
                    var recentFHRs = fHRs?.OrderByDescending(f => f.Time).Take(5).ToList();

                    // Get recent contractions (within last hour)
                    var allContractions = await _contractionRepository.ListByPatientAsync(_patient.ID);
                    var recentContractions = allContractions?
                        .Where(c => (fhrEntry.Time - c.Time).TotalHours <= 1)
                        .OrderByDescending(c => c.Time)
                        .ToList() ?? new List<Contraction>();

                    // Analyze the pattern and detect deceleration type
                    var analysisResult = _fhrAnalysisService.AnalyzeFHRPattern(
                        recentFHRs?.ToList() ?? new List<FHR>(),
                        recentContractions,
                        fhrEntry
                    );

                    // Set the automatically detected deceleration
                    fhrEntry.Deceleration = analysisResult.DetectedDeceleration;

                    // Add analysis details to notes if confidence is reasonable
                    if (analysisResult.Confidence >= 0.5)
                    {
                        var detectionNote = $"\n[AUTO-DETECTED: {analysisResult.DetectedDeceleration} (Confidence: {analysisResult.Confidence:P0})]\n{analysisResult.Reason}";
                        if (analysisResult.Severity == "Critical" || analysisResult.Severity == "Warning")
                        {
                            detectionNote += $"\n⚠️ SEVERITY: {analysisResult.Severity}";
                        }
                        fhrEntry.Notes = string.IsNullOrEmpty(fhrEntry.Notes)
                            ? detectionNote.Trim()
                            : $"{fhrEntry.Notes}\n{detectionNote}";
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the save operation
                    _logger?.LogError(ex, "Error during automatic FHR deceleration detection");
                    // If auto-detection fails, set to "No" as fallback
                    fhrEntry.Deceleration = fhrEntry.Deceleration ?? "No";
                }
                // ===== END AUTOMATIC DETECTION =====

                var contraction = await _contractionRepository.SaveItemAsync(contractionEntry) != null;
                var fhr = await _fHRRepository.SaveItemAsync(fhrEntry) != null;

                if (contraction || fhr)
                {
                    await AppShell.DisplayToastAsync($"{(contraction && fhr ? "Contraction & FHR" : contraction ? "Contraction" : "FHR")} assessment saved successfully");

                    // Reset fields to default
                    ResetFields();

                    // Close the popup
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("Assessment failed to save");
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
            Rate = 0;
            FrequencyPer10Min = 0;
            DurationSeconds = 0;
            Notes = string.Empty;
        }

        /// <summary>
        /// Auto-calculate FHR pattern indicators based on heart rate
        /// </summary>
        private void AutoCalculateFHRPatterns()
        {
            // Auto-detect tachycardia (>160 bpm)
            if (Rate > 160)
            {
                Tachycardia = true;
                if (!TachycardiaStartTime.HasValue)
                {
                    TachycardiaStartTime = DateTime.Now;
                }
            }
            else
            {
                Tachycardia = false;
                TachycardiaStartTime = null;
                TachycardiaDurationMinutes = null;
            }

            // Auto-detect prolonged bradycardia (<110 bpm)
            if (Rate < 110 && Rate > 0)
            {
                ProlongedBradycardia = true;
                if (!BradycardiaStartTime.HasValue)
                {
                    BradycardiaStartTime = DateTime.Now;
                }
            }
            else
            {
                ProlongedBradycardia = false;
                BradycardiaStartTime = null;
                BradycardiaDurationMinutes = null;
            }

            // Set baseline rate for normal FHR (110-160 bpm)
            if (Rate >= 110 && Rate <= 160)
            {
                BaselineRate = Rate;
            }

            // Auto-flag FHR compromise if abnormal patterns detected
            FhrCompromise = ProlongedBradycardia || Tachycardia || SinusoidalPattern;
        }

        /// <summary>
        /// Auto-calculate contraction pattern indicators
        /// </summary>
        private void AutoCalculateContractionPatterns()
        {
            // Auto-detect tachysystole (>5 contractions per 10 minutes)
            Tachysystole = FrequencyPer10Min > 5;

            if (Tachysystole)
            {
                if (!TachysystoleStartTime.HasValue)
                {
                    TachysystoleStartTime = DateTime.Now;
                }

                // Auto-detect hyperstimulation (tachysystole + abnormal FHR)
                Hyperstimulation = Tachysystole && (ProlongedBradycardia || Tachycardia || FhrCompromise);

                // Suggest oxytocin adjustment if on oxytocin
                if (OnOxytocin)
                {
                    OxytocinAdjustmentNeeded = true;
                    SuggestedOxytocinAction = Hyperstimulation
                        ? "STOP oxytocin immediately - Hyperstimulation detected"
                        : "Reduce oxytocin - Tachysystole detected";
                }
            }
            else
            {
                Tachysystole = false;
                TachysystoleStartTime = null;
                TachysystoleDurationMinutes = null;
                Hyperstimulation = false;
            }

            // Auto-flag intervention required for critical patterns
            ContractionInterventionRequired = Hyperstimulation || Tachysystole;
            EmergencyConsultRequired = Hyperstimulation || (Tachysystole && FhrCompromise);
        }
    }
}
