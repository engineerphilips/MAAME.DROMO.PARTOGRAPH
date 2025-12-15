using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    public partial class BPPulseModalPageModel : ObservableObject
    {
        public Partograph? _patient;
        private readonly BPRepository _bPPulseRepository;
        private readonly ModalErrorHandler _errorHandler;

        public Action? ClosePopup { get; set; }

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private DateOnly _recordingDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _recordingTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        private void InitializeData()
        {
            UpdatePulseStatus();
            UpdateBPStatus();
        }

        private int? _pulse = 80;
        public int? Pulse
        {
            get => _pulse;
            set
            {
                _pulse = value;
                UpdatePulseStatus();
            }
        }

        private int? _systolic = 120;
        public int? Systolic
        {
            get => _systolic;
            set
            {
                _systolic = value;
                OnPropertyChanged();
                UpdateBPStatus();
            }
        }

        private int? _diastolic = 80;
        public int? Diastolic
        {
            get => _diastolic;
            set
            {
                _diastolic = value;
                OnPropertyChanged();
                UpdateBPStatus();
            }
        }

        private Color _pulseColor = Colors.Green;
        public Color PulseColor
        {
            get => _pulseColor;
            set { _pulseColor = value; OnPropertyChanged(); }
        }

        private string _pulseStatus = "Normal";
        public string PulseStatus
        {
            get => _pulseStatus;
            set { _pulseStatus = value; OnPropertyChanged(); }
        }

        private Color _bpColor = Colors.Green;
        public Color BPColor
        {
            get => _bpColor;
            set { _bpColor = value; }
        }

        private string _bpStatus = "Normal";
        public string BPStatus
        {
            get => _bpStatus;
            set { _bpStatus = value; }
        }

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        // Advanced Fields Toggle
        [ObservableProperty]
        private bool _showAdvancedFields = false;

        // WHO 2020 Enhanced BP Assessment Fields

        // Original fields (already have Systolic, Diastolic, Pulse above)
        [ObservableProperty]
        private string _maternalPosition = "Sitting";

        [ObservableProperty]
        private string _cuffSize = "Standard";

        [ObservableProperty]
        private bool _repeatMeasurement;

        [ObservableProperty]
        private bool _irregularPulse;

        [ObservableProperty]
        private string _clinicalAlert = string.Empty;

        // Hypertension Classification
        [ObservableProperty]
        private string _bpCategory = string.Empty;

        [ObservableProperty]
        private bool _severeHypertension;

        [ObservableProperty]
        private bool _preeclampsiaRange;

        [ObservableProperty]
        private DateTime? _firstElevatedBPTime;

        [ObservableProperty]
        private int? _consecutiveElevatedReadings;

        // Repeat Measurements
        [ObservableProperty]
        private int? _secondSystolic;

        [ObservableProperty]
        private int? _secondDiastolic;

        [ObservableProperty]
        private DateTime? _secondReadingTime;

        [ObservableProperty]
        private int? _thirdSystolic;

        [ObservableProperty]
        private int? _thirdDiastolic;

        [ObservableProperty]
        private DateTime? _thirdReadingTime;

        // Pulse Details
        [ObservableProperty]
        private string _pulseRhythm = string.Empty;

        [ObservableProperty]
        private string _pulseVolume = string.Empty;

        [ObservableProperty]
        private string _pulseCharacter = string.Empty;

        [ObservableProperty]
        private bool _pulseDeficit;

        // Hypotension Assessment
        [ObservableProperty]
        private bool _hypotension;

        [ObservableProperty]
        private string _hypotensionCause = string.Empty;

        [ObservableProperty]
        private bool _posturalHypotension;

        [ObservableProperty]
        private int? _posturalDrop;

        // Pre-eclampsia Screening
        [ObservableProperty]
        private bool _newOnsetHypertension;

        [ObservableProperty]
        private bool _knownHypertension;

        [ObservableProperty]
        private bool _onAntihypertensives;

        [ObservableProperty]
        private string _antihypertensiveMedication = string.Empty;

        [ObservableProperty]
        private DateTime? _lastAntihypertensiveDose;

        // Associated Symptoms
        [ObservableProperty]
        private bool _headache;

        [ObservableProperty]
        private bool _visualDisturbances;

        [ObservableProperty]
        private bool _epigastricPain;

        [ObservableProperty]
        private bool _hyperreflexia;

        [ObservableProperty]
        private bool _edema;

        // Clinical Response
        [ObservableProperty]
        private bool _emergencyProtocolActivated;

        [ObservableProperty]
        private bool _antihypertensiveGiven;

        [ObservableProperty]
        private DateTime? _antihypertensiveGivenTime;

        [ObservableProperty]
        private bool _magnesiumSulfateGiven;

        [ObservableProperty]
        private bool _ivFluidsGiven;

        [ObservableProperty]
        private bool _positionChanged;

        public BPPulseModalPageModel(BPRepository bPRepository, ModalErrorHandler errorHandler)
        {
            _bPPulseRepository = bPRepository;
            _errorHandler = errorHandler;

            // Set default recorded by from preferences
            RecordedBy = Preferences.Get("StaffName", "Staff");
        }

        private void UpdatePulseStatus()
        {
            if (Pulse < 60)
            {
                PulseColor = Colors.Orange;
                PulseStatus = "Bradycardia - Low pulse";
            }
            else if (Pulse >= 60 && Pulse <= 100)
            {
                PulseColor = Colors.Green;
                PulseStatus = "Normal pulse (60-100 bpm)";
            }
            else if (Pulse > 100 && Pulse <= 120)
            {
                PulseColor = Colors.Orange;
                PulseStatus = "Tachycardia - Monitor closely";
            }
            else
            {
                PulseColor = Colors.Red;
                PulseStatus = "⚠️ Severe tachycardia - Requires attention";
            }
        }

        private void UpdateBPStatus()
        {
            // Check for hypertension (pre-eclampsia threshold)
            if (Systolic >= 140 || Diastolic >= 90)
            {
                if (Systolic >= 160 || Diastolic >= 110)
                {
                    BPColor = Colors.Red;
                    BPStatus = "⚠️ Severe hypertension - Immediate action required";

                    // Auto-flag severe hypertension
                    SevereHypertension = true;
                    PreeclampsiaRange = true;
                    BpCategory = "Severe Hypertension (≥160/110)";

                    // Set first elevated BP time if not already set
                    if (!FirstElevatedBPTime.HasValue)
                    {
                        FirstElevatedBPTime = DateTime.Now;
                        ConsecutiveElevatedReadings = 1;
                    }
                    else
                    {
                        ConsecutiveElevatedReadings = (ConsecutiveElevatedReadings ?? 0) + 1;
                    }
                }
                else
                {
                    BPColor = Colors.Orange;
                    BPStatus = "Mild hypertension - Monitor closely";

                    SevereHypertension = false;
                    PreeclampsiaRange = true;
                    BpCategory = "Mild Hypertension (140-159/90-109)";

                    if (!FirstElevatedBPTime.HasValue)
                    {
                        FirstElevatedBPTime = DateTime.Now;
                        ConsecutiveElevatedReadings = 1;
                    }
                }
            }
            else if (Systolic < 90 || Diastolic < 60)
            {
                BPColor = Colors.Orange;
                BPStatus = "Hypotension - Monitor closely";

                SevereHypertension = false;
                PreeclampsiaRange = false;
                Hypotension = true;
                BpCategory = "Hypotension (<90/60)";
            }
            else
            {
                BPColor = Colors.Green;
                BPStatus = "Normal blood pressure";

                SevereHypertension = false;
                PreeclampsiaRange = false;
                Hypotension = false;
                BpCategory = "Normal (90-139/60-89)";
                FirstElevatedBPTime = null;
                ConsecutiveElevatedReadings = null;
            }
        }

        //public void ApplyQueryAttributes(IDictionary<string, object> query)
        //{
        //    if (query.ContainsKey("patientId"))
        //    {
        //        Guid? patientId = Guid.Parse(Convert.ToString(query["patientId"]));
        //        LoadPatient(patientId).FireAndForgetSafeAsync(_errorHandler);
        //    }
        //}

        public async Task LoadPatient(Guid? patientId)
        {

            try
            {
                // This would typically load from PatientRepository
                // For now, we'll use the patient ID directly
                PatientName = $"Patient ID: {patientId}";

                // Load last pain relief entry to prefill some values
                var lastEntry = await _bPPulseRepository.GetLatestByPatientAsync(patientId);
                if (lastEntry != null)
                {
                    Pulse = lastEntry.Pulse != 0 ? lastEntry.Pulse : null;
                    Systolic = lastEntry.Pulse != 0 ? lastEntry.Systolic : null;
                    Systolic = lastEntry.Pulse != 0 ? lastEntry.Diastolic : null;
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

            if (Pulse < 0)
            {
                _errorHandler.HandleError(new Exception("Pulse is not set."));
                return;
            }

            if (Systolic < 0)
            {
                _errorHandler.HandleError(new Exception("Systolic blood pressure is not set."));
                return;
            }

            if (Diastolic < 0)
            {
                _errorHandler.HandleError(new Exception("Diastolic blood pressure is not set."));
                return;
            }

            try
            {
                IsBusy = true;

                var entry = new BP
                {
                    PartographID = _patient.ID,
                    Time = new DateTime(RecordingDate.Year, RecordingDate.Month, RecordingDate.Day).Add(RecordingTime),
                    Pulse = Pulse ?? 0,
                    Systolic = Systolic ?? 0,
                    Diastolic = Diastolic ?? 0,
                    Notes = Notes,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID
                };

                if (await _bPPulseRepository.SaveItemAsync(entry) != null)
                {
                    await AppShell.DisplayToastAsync("BP/Pulse assessment saved successfully");

                    // Reset fields to default
                    ResetFields();

                    // Close the popup
                    ClosePopup?.Invoke();
                }
                else
                {
                    await AppShell.DisplayToastAsync("BP/Pulse assessment failed to save");
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
            Pulse = null;
            Systolic = null;
            Diastolic = null;
            Notes = string.Empty;
        }
    }
}
