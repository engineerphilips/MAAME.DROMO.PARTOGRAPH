using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public class BaselineFHRModalPageModel : INotifyPropertyChanged
    {
        private readonly FHRRepository _repository;
        private readonly ILogger<BaselineFHRModalPageModel> _logger;
        private FHR _currentEntry;
        private Guid? _patientId;

        public BaselineFHRModalPageModel(FHRRepository repository, ILogger<BaselineFHRModalPageModel> logger)
        {
            _repository = repository;
            _logger = logger;
            InitializeCommands();
            InitializeData();
        }

        // Multiple Pregnancy Support
        private bool _isMultiplePregnancy;
        public bool IsMultiplePregnancy
        {
            get => _isMultiplePregnancy;
            set { _isMultiplePregnancy = value; OnPropertyChanged(); }
        }

        private int _numberOfBabies = 1;
        public int NumberOfBabies
        {
            get => _numberOfBabies;
            set
            {
                _numberOfBabies = value;
                OnPropertyChanged();
                IsMultiplePregnancy = value > 1;
                UpdateBabyOptions();
            }
        }

        private int _selectedBabyNumber = 1;
        public int SelectedBabyNumber
        {
            get => _selectedBabyNumber;
            set { _selectedBabyNumber = value; OnPropertyChanged(); UpdateSelectedBabyTag(); }
        }

        private string _selectedBabyTag = "Baby A";
        public string SelectedBabyTag
        {
            get => _selectedBabyTag;
            set { _selectedBabyTag = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> BabyOptions { get; } = new ObservableCollection<string>();

        private void UpdateBabyOptions()
        {
            BabyOptions.Clear();
            for (int i = 1; i <= _numberOfBabies; i++)
            {
                var tag = $"Baby {(char)('A' + i - 1)}";
                BabyOptions.Add(tag);
            }
            if (BabyOptions.Any())
            {
                SelectedBabyNumber = 1;
                SelectedBabyTag = BabyOptions.First();
            }
        }

        private void UpdateSelectedBabyTag()
        {
            if (_selectedBabyNumber >= 1 && _selectedBabyNumber <= BabyOptions.Count)
            {
                SelectedBabyTag = BabyOptions[_selectedBabyNumber - 1];
            }
        }

        // Properties
        private DateTime _recordingDate = DateTime.Now;
        public DateTime RecordingDate
        {
            get => _recordingDate;
            set { _recordingDate = value; OnPropertyChanged(); }
        }

        private TimeSpan _recordingTime = DateTime.Now.TimeOfDay;
        public TimeSpan RecordingTime
        {
            get => _recordingTime;
            set { _recordingTime = value; OnPropertyChanged(); }
        }

        private int _baselineRate = 140;
        public int BaselineRate
        {
            get => _baselineRate;
            set
            {
                _baselineRate = value;
                OnPropertyChanged();
                UpdateBaselineRateStatus();
            }
        }

        private string _baselineRateStatus = "Normal";
        public string BaselineRateStatus
        {
            get => _baselineRateStatus;
            set { _baselineRateStatus = value; OnPropertyChanged(); }
        }

        private Color _baselineRateColor = Colors.Green;
        public Color BaselineRateColor
        {
            get => _baselineRateColor;
            set { _baselineRateColor = value; OnPropertyChanged(); }
        }

        private string _selectedVariability = "Moderate";
        public string SelectedVariability
        {
            get => _selectedVariability;
            set { _selectedVariability = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> VariabilityOptions { get; } = new ObservableCollection<string>
        {
            "Absent", "Minimal", "Moderate", "Marked", "Sinusoidal"
        };

        private bool _hasAccelerations;
        public bool HasAccelerations
        {
            get => _hasAccelerations;
            set { _hasAccelerations = value; OnPropertyChanged(); }
        }

        private string _selectedPattern = "Normal";
        public string SelectedPattern
        {
            get => _selectedPattern;
            set { _selectedPattern = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> PatternOptions { get; } = new ObservableCollection<string>
        {
            "Normal", "Tachycardia", "Bradycardia", "Variable"
        };

        private bool _isIntermittentAuscultation;
        public bool IsIntermittentAuscultation
        {
            get => _isIntermittentAuscultation;
            set { _isIntermittentAuscultation = value; OnPropertyChanged(); }
        }

        private bool _isContinuousCTG;
        public bool IsContinuousCTG
        {
            get => _isContinuousCTG;
            set { _isContinuousCTG = value; OnPropertyChanged(); }
        }

        private bool _isDoppler;
        public bool IsDoppler
        {
            get => _isDoppler;
            set { _isDoppler = value; OnPropertyChanged(); }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        // WHO 2020 Enhanced FHR Assessment Fields

        // Original FHR fields
        private int? _rate;
        public int? Rate
        {
            get => _rate;
            set { _rate = value; OnPropertyChanged(); }
        }

        private string _deceleration = "None";
        public string Deceleration
        {
            get => _deceleration;
            set { _deceleration = value; OnPropertyChanged(); }
        }

        private int _decelerationDurationSeconds;
        public int DecelerationDurationSeconds
        {
            get => _decelerationDurationSeconds;
            set { _decelerationDurationSeconds = value; OnPropertyChanged(); }
        }

        private bool _accelerations;
        public bool Accelerations
        {
            get => _accelerations;
            set { _accelerations = value; OnPropertyChanged(); }
        }

        private string _pattern = string.Empty;
        public string Pattern
        {
            get => _pattern;
            set { _pattern = value; OnPropertyChanged(); }
        }

        private string _monitoringMethod = string.Empty;
        public string MonitoringMethod
        {
            get => _monitoringMethod;
            set { _monitoringMethod = value; OnPropertyChanged(); }
        }

        private string _clinicalAlert = string.Empty;
        public string ClinicalAlert
        {
            get => _clinicalAlert;
            set { _clinicalAlert = value; OnPropertyChanged(); }
        }

        // Detailed Variability Assessment
        private int? _variabilityBpm;
        public int? VariabilityBpm
        {
            get => _variabilityBpm;
            set { _variabilityBpm = value; OnPropertyChanged(); }
        }

        private string _variabilityTrend = string.Empty;
        public string VariabilityTrend
        {
            get => _variabilityTrend;
            set { _variabilityTrend = value; OnPropertyChanged(); }
        }

        private bool _sinusoidalPattern;
        public bool SinusoidalPattern
        {
            get => _sinusoidalPattern;
            set { _sinusoidalPattern = value; OnPropertyChanged(); }
        }

        private bool _saltatorPattern;
        public bool SaltatorPattern
        {
            get => _saltatorPattern;
            set { _saltatorPattern = value; OnPropertyChanged(); }
        }

        // Acceleration Details
        private int? _accelerationCount;
        public int? AccelerationCount
        {
            get => _accelerationCount;
            set { _accelerationCount = value; OnPropertyChanged(); }
        }

        private int? _accelerationPeakBpm;
        public int? AccelerationPeakBpm
        {
            get => _accelerationPeakBpm;
            set { _accelerationPeakBpm = value; OnPropertyChanged(); }
        }

        private int? _accelerationDurationSeconds;
        public int? AccelerationDurationSeconds
        {
            get => _accelerationDurationSeconds;
            set { _accelerationDurationSeconds = value; OnPropertyChanged(); }
        }

        // Deceleration Details
        private int? _decelerationNadirBpm;
        public int? DecelerationNadirBpm
        {
            get => _decelerationNadirBpm;
            set { _decelerationNadirBpm = value; OnPropertyChanged(); }
        }

        private string _decelerationRecovery = string.Empty;
        public string DecelerationRecovery
        {
            get => _decelerationRecovery;
            set { _decelerationRecovery = value; OnPropertyChanged(); }
        }

        private int? _decelerationAmplitudeBpm;
        public int? DecelerationAmplitudeBpm
        {
            get => _decelerationAmplitudeBpm;
            set { _decelerationAmplitudeBpm = value; OnPropertyChanged(); }
        }

        private string _decelerationTiming = string.Empty;
        public string DecelerationTiming
        {
            get => _decelerationTiming;
            set { _decelerationTiming = value; OnPropertyChanged(); }
        }

        // Bradycardia/Tachycardia
        private bool _prolongedBradycardia;
        public bool ProlongedBradycardia
        {
            get => _prolongedBradycardia;
            set { _prolongedBradycardia = value; OnPropertyChanged(); }
        }

        private DateTime? _bradycardiaStartTime;
        public DateTime? BradycardiaStartTime
        {
            get => _bradycardiaStartTime;
            set { _bradycardiaStartTime = value; OnPropertyChanged(); }
        }

        private int? _bradycardiaDurationMinutes;
        public int? BradycardiaDurationMinutes
        {
            get => _bradycardiaDurationMinutes;
            set { _bradycardiaDurationMinutes = value; OnPropertyChanged(); }
        }

        private bool _tachycardia;
        public bool Tachycardia
        {
            get => _tachycardia;
            set { _tachycardia = value; OnPropertyChanged(); }
        }

        private DateTime? _tachycardiaStartTime;
        public DateTime? TachycardiaStartTime
        {
            get => _tachycardiaStartTime;
            set { _tachycardiaStartTime = value; OnPropertyChanged(); }
        }

        private int? _tachycardiaDurationMinutes;
        public int? TachycardiaDurationMinutes
        {
            get => _tachycardiaDurationMinutes;
            set { _tachycardiaDurationMinutes = value; OnPropertyChanged(); }
        }

        // CTG Interpretation
        private string _ctgClassification = string.Empty;
        public string CTGClassification
        {
            get => _ctgClassification;
            set { _ctgClassification = value; OnPropertyChanged(); }
        }

        private bool _reactiveNST;
        public bool ReactiveNST
        {
            get => _reactiveNST;
            set { _reactiveNST = value; OnPropertyChanged(); }
        }

        private DateTime? _lastReactiveTime;
        public DateTime? LastReactiveTime
        {
            get => _lastReactiveTime;
            set { _lastReactiveTime = value; OnPropertyChanged(); }
        }

        // Maternal Context
        private string _maternalPosition = string.Empty;
        public string MaternalPosition
        {
            get => _maternalPosition;
            set { _maternalPosition = value; OnPropertyChanged(); }
        }

        private bool _duringContraction;
        public bool DuringContraction
        {
            get => _duringContraction;
            set { _duringContraction = value; OnPropertyChanged(); }
        }

        private bool _betweenContractions;
        public bool BetweenContractions
        {
            get => _betweenContractions;
            set { _betweenContractions = value; OnPropertyChanged(); }
        }

        // Clinical Response
        private bool _interventionRequired;
        public bool InterventionRequired
        {
            get => _interventionRequired;
            set { _interventionRequired = value; OnPropertyChanged(); }
        }

        private string _interventionTaken = string.Empty;
        public string InterventionTaken
        {
            get => _interventionTaken;
            set { _interventionTaken = value; OnPropertyChanged(); }
        }

        private DateTime? _interventionTime;
        public DateTime? InterventionTime
        {
            get => _interventionTime;
            set { _interventionTime = value; OnPropertyChanged(); }
        }

        private bool _changeInPosition;
        public bool ChangeInPosition
        {
            get => _changeInPosition;
            set { _changeInPosition = value; OnPropertyChanged(); }
        }

        private bool _oxygenAdministered;
        public bool OxygenAdministered
        {
            get => _oxygenAdministered;
            set { _oxygenAdministered = value; OnPropertyChanged(); }
        }

        private bool _ivFluidsIncreased;
        public bool IVFluidsIncreased
        {
            get => _ivFluidsIncreased;
            set { _ivFluidsIncreased = value; OnPropertyChanged(); }
        }

        // Obstetric Emergency Indicators
        private bool _emergencyConsultRequired;
        public bool EmergencyConsultRequired
        {
            get => _emergencyConsultRequired;
            set { _emergencyConsultRequired = value; OnPropertyChanged(); }
        }

        private string _consultReason = string.Empty;
        public string ConsultReason
        {
            get => _consultReason;
            set { _consultReason = value; OnPropertyChanged(); }
        }

        private DateTime? _consultTime;
        public DateTime? ConsultTime
        {
            get => _consultTime;
            set { _consultTime = value; OnPropertyChanged(); }
        }

        private bool _prepareForEmergencyDelivery;
        public bool PrepareForEmergencyDelivery
        {
            get => _prepareForEmergencyDelivery;
            set { _prepareForEmergencyDelivery = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private void InitializeCommands()
        {
            SaveCommand = new Command(async () => await SaveEntry());
            CancelCommand = new Command(async () => await Cancel());
        }

        private void InitializeData()
        {
            // Set default monitoring method
            IsIntermittentAuscultation = true;
        }

        private void UpdateBaselineRateStatus()
        {
            if (BaselineRate < 110)
            {
                BaselineRateStatus = "Bradycardia - Below normal range";
                BaselineRateColor = Colors.Red;
            }
            else if (BaselineRate >= 110 && BaselineRate <= 160)
            {
                BaselineRateStatus = "Normal range (110-160 bpm)";
                BaselineRateColor = Colors.Green;
            }
            else
            {
                BaselineRateStatus = "Tachycardia - Above normal range";
                BaselineRateColor = Colors.Orange;
            }
        }

        private string GetMonitoringMethod()
        {
            if (IsContinuousCTG) return "Continuous CTG";
            if (IsDoppler) return "Doppler";
            return "Intermittent Auscultation";
        }

        private async Task SaveEntry()
        {
            try
            {
                var entry = new FHR
                {
                    PartographID = _patientId,
                    BabyNumber = SelectedBabyNumber,
                    BabyTag = SelectedBabyTag ?? string.Empty,
                    Time = RecordingDate.Date + RecordingTime,
                    HandlerName = await SecureStorage.GetAsync("CurrentUser") ?? "Unknown",
                    //BaselineRate = BaselineRate,
                    //Variability = SelectedVariability,
                    //Accelerations = HasAccelerations,
                    //Pattern = SelectedPattern,
                    //MonitoringMethod = GetMonitoringMethod(),
                    Notes = Notes ?? string.Empty
                };

                if (_currentEntry != null)
                {
                    entry.ID = _currentEntry.ID;
                    //await _repository.UpdateAsync(entry);
                }
                else
                {
                    await _repository.SaveItemAsync(entry);
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving baseline FHR entry");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to save entry", "OK");
            }
        }

        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
