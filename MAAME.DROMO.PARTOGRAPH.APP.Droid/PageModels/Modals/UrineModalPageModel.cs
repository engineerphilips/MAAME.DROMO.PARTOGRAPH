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
    public partial class UrineModalPageModel : ObservableObject
    {
        public Partograph? _patient;
        private readonly UrineRepository _urineRepository;
        private readonly ModalErrorHandler _errorHandler;

        public UrineModalPageModel(UrineRepository repository, ModalErrorHandler errorHandler)
        {
            _urineRepository = repository;
            _errorHandler = errorHandler;
        }

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        [ObservableProperty]
        private int _proteinIndex = -1;

        [ObservableProperty]
        private int _acetoneIndex = -1;

        [ObservableProperty]
        private string _proteinDisplay = string.Empty;

        [ObservableProperty]
        private string _acetoneDisplay = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        // Advanced Fields Toggle
        [ObservableProperty]
        private bool _showAdvancedFields = false;

        // WHO 2020 Enhanced Urine Assessment Fields

        // Original fields
        private int _outputMl;
        public int OutputMl
        {
            get => _outputMl;
            set
            {
                SetProperty(ref _outputMl, value);
                AutoCalculateUrineFields();
            }
        }

        [ObservableProperty]
        private string _color = "Yellow";

        [ObservableProperty]
        private string _protein = "Nil";

        [ObservableProperty]
        private string _ketones = "Nil";

        [ObservableProperty]
        private string _glucose = "Nil";

        [ObservableProperty]
        private string _specificGravity = string.Empty;

        [ObservableProperty]
        private string _voidingMethod = "Spontaneous";

        [ObservableProperty]
        private bool _bladderPalpable;

        [ObservableProperty]
        private DateTime? _lastVoided;

        [ObservableProperty]
        private string _clinicalAlert = string.Empty;

        // Volume and Pattern
        [ObservableProperty]
        private DateTime? _voidingTime;

        [ObservableProperty]
        private int? _timeSinceLastVoidMinutes;

        [ObservableProperty]
        private int? _cumulativeOutputMl;

        [ObservableProperty]
        private decimal? _hourlyOutputRate;

        [ObservableProperty]
        private bool _oliguria;

        [ObservableProperty]
        private bool _anuria;

        [ObservableProperty]
        private int? _consecutiveOliguriaHours;

        // Appearance
        [ObservableProperty]
        private string _clarity = string.Empty;

        [ObservableProperty]
        private bool _hematuria;

        [ObservableProperty]
        private bool _concentrated;

        [ObservableProperty]
        private bool _dilute;

        [ObservableProperty]
        private string _odor = string.Empty;

        // Dipstick Results
        [ObservableProperty]
        private string _bloodDipstick = "Nil";

        [ObservableProperty]
        private string _leukocytesDipstick = "Nil";

        [ObservableProperty]
        private string _nitritesDipstick = "Nil";

        [ObservableProperty]
        private float? _phLevel;

        // Bladder Management
        [ObservableProperty]
        private bool _bladderFullness;

        [ObservableProperty]
        private string _bladderFullnessLevel = string.Empty;

        [ObservableProperty]
        private bool _difficultVoiding;

        [ObservableProperty]
        private bool _urinaryRetention;

        [ObservableProperty]
        private bool _catheterizationIndicated;

        [ObservableProperty]
        private DateTime? _lastCatheterizationTime;

        [ObservableProperty]
        private string _catheterType = string.Empty;

        // Pre-eclampsia Monitoring
        [ObservableProperty]
        private bool _proteinuriaNewOnset;

        [ObservableProperty]
        private bool _proteinuriaWorsening;

        [ObservableProperty]
        private DateTime? _firstProteinDetectedTime;

        [ObservableProperty]
        private bool _laboratorySampleSent;

        [ObservableProperty]
        private string _proteinCreatinineRatio = string.Empty;

        // Dehydration/Ketosis Assessment
        [ObservableProperty]
        private bool _signsOfDehydration;

        [ObservableProperty]
        private bool _prolongedLabor;

        [ObservableProperty]
        private bool _increasedKetoneTrend;

        [ObservableProperty]
        private DateTime? _firstKetoneDetectedTime;

        // Fluid Balance
        [ObservableProperty]
        private int? _totalOralIntakeMl;

        [ObservableProperty]
        private int? _totalIVIntakeMl;

        [ObservableProperty]
        private int? _fluidBalanceMl;

        // Clinical Response
        [ObservableProperty]
        private bool _encourageOralFluids;

        [ObservableProperty]
        private bool _ivFluidsStarted;

        [ObservableProperty]
        private bool _catheterizationPerformed;

        [ObservableProperty]
        private bool _nephrologyConsultRequired;

        public Action? ClosePopup { get; set; }

        internal async Task LoadPatient(Guid? patientId)
        {
            try
            {
                // Reset fields to default values when opening the modal
                ResetFields();

                // This would typically load from PatientRepository
                // For now, we'll use the patient ID directly
                PatientName = $"Patient ID: {patientId}";

                // Note: We no longer prefill values so users start with fresh inputs
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

                var entry = new Urine
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    Protein = GetProteinValue(ProteinIndex),
                    Ketones = GetKetonesValue(AcetoneIndex),
                    OutputMl = OutputMl,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID
                };

                if (await _urineRepository.SaveItemAsync(entry) != null)
                {
                    await AppShell.DisplayToastAsync("Urine assessment saved successfully");

                    // Reset fields to default
                    ResetFields();

                    // Close the popup
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("Urine assessment failed to save");
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
            ProteinIndex = -1;
            AcetoneIndex = -1;
            OutputMl = 0;
            Notes = string.Empty;
        }

        /// <summary>
        /// Convert UI index to MODEL Protein value
        /// UI: P- (0), P (1), P1+ (2), P2+ (3), P3+ (4)
        /// MODEL: Nil, Trace, Plus1, Plus2, Plus3
        /// </summary>
        private string? GetProteinValue(int index)
        {
            return index switch
            {
                0 => "Nil",      // P-
                1 => "Trace",    // P
                2 => "Plus1",    // P1+
                3 => "Plus2",    // P2+
                4 => "Plus3",    // P3+
                _ => null
            };
        }

        /// <summary>
        /// Convert UI index to MODEL Ketones value
        /// UI: A- (0), A (1), A1+ (2), A2+ (3), A3+ (4)
        /// MODEL: Nil, Trace, Plus1, Plus2, Plus3
        /// </summary>
        private string? GetKetonesValue(int index)
        {
            return index switch
            {
                0 => "Nil",      // A-
                1 => "Trace",    // A
                2 => "Plus1",    // A1+
                3 => "Plus2",    // A2+
                4 => "Plus3",    // A3+
                _ => null
            };
        }

        /// <summary>
        /// Auto-calculate urine-related fields based on output volume
        /// </summary>
        private void AutoCalculateUrineFields()
        {
            // Calculate hourly output rate
            if (TimeSinceLastVoidMinutes.HasValue && TimeSinceLastVoidMinutes > 0)
            {
                HourlyOutputRate = (decimal)OutputMl / ((decimal)TimeSinceLastVoidMinutes.Value / 60);
            }

            // Auto-detect oliguria (<30 ml/hour)
            if (HourlyOutputRate.HasValue)
            {
                Oliguria = HourlyOutputRate < 30;
            }
            else if (OutputMl > 0 && OutputMl < 30)
            {
                // If no time interval, assume this is hourly output
                Oliguria = true;
            }

            // Auto-detect anuria (no urine output)
            Anuria = OutputMl == 0;

            // Track consecutive oliguria hours if oliguria detected
            if (Oliguria && !ConsecutiveOliguriaHours.HasValue)
            {
                ConsecutiveOliguriaHours = 1;
            }
            else if (!Oliguria)
            {
                ConsecutiveOliguriaHours = null;
            }
        }
    }
}
