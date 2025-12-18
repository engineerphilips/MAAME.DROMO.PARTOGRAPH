using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    /// <summary>
    /// Fourth Stage Partograph Page Model - Manages immediate postpartum monitoring
    /// WHO Guidelines: 2-hour monitoring period with vitals every 15 minutes
    /// Enhanced with comprehensive maternal vitals, fourth stage specific assessments, and trend tracking
    /// </summary>
    public partial class FourthStagePartographPageModel : ObservableObject, IQueryAttributable
    {
        public Partograph? Patient { get; set; }
        private readonly PatientRepository _patientRepository;
        private readonly PartographRepository _partographRepository;
        private readonly ModalErrorHandler _errorHandler;
        private readonly BPRepository _bpRepository;
        private readonly TemperatureRepository _temperatureRepository;
        private readonly BirthOutcomeRepository _birthOutcomeRepository;
        private readonly BabyDetailsRepository _babyDetailsRepository;
        private readonly FourthStageVitalsRepository _fourthStageVitalsRepository;
        private readonly LabourTimerService _labourTimerService;
        private readonly CompletionChecklistPopupPageModel _completionChecklistPopupPageModel;
        private readonly BPPulseModalPageModel _bpPulseModalPageModel;
        private readonly TemperatureModalPageModel _temperatureModalPageModel;

        #region Patient Info Properties

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private string _patientInfo = string.Empty;

        [ObservableProperty]
        private string _fourthStageDuration = string.Empty;

        [ObservableProperty]
        private string _remainingTime = string.Empty;

        [ObservableProperty]
        private DateTime? _lastRecordedTime;

        [ObservableProperty]
        bool _isBusy;

        #endregion

        #region Maternal Vitals Properties

        // BP/Pulse
        [ObservableProperty]
        private string _bpLatestValue = "--/-- mmHg";

        [ObservableProperty]
        private string _bpStatusText = "No reading";

        [ObservableProperty]
        private string _bpButtonColor = "#64748B";

        [ObservableProperty]
        private string _pulseLatestValue = "-- bpm";

        [ObservableProperty]
        private string _pulseStatusText = string.Empty;

        [ObservableProperty]
        private string _pulseButtonColor = "#64748B";

        // Temperature
        [ObservableProperty]
        private string _temperatureLatestValue = "--°C";

        [ObservableProperty]
        private string _temperatureStatusText = "No reading";

        [ObservableProperty]
        private string _temperatureButtonColor = "#64748B";

        // Vitals history for trends
        [ObservableProperty]
        private ObservableCollection<BP> _bpHistory = new();

        [ObservableProperty]
        private ObservableCollection<Temperature> _temperatureHistory = new();

        #endregion

        #region Fourth Stage Assessment Properties

        // Fundal Height
        [ObservableProperty]
        private string _fundalHeight = "At umbilicus";

        [ObservableProperty]
        private string _fundalHeightColor = "#4CAF50";

        [ObservableProperty]
        private string _fundalHeightStatus = "Normal";

        // Bleeding Status
        [ObservableProperty]
        private string _bleedingStatus = "Normal lochia";

        [ObservableProperty]
        private string _bleedingStatusColor = "#4CAF50";

        [ObservableProperty]
        private string _estimatedBloodLoss = "Not recorded";

        // Bladder Status
        [ObservableProperty]
        private string _bladderStatus = "Empty";

        [ObservableProperty]
        private string _bladderStatusColor = "#4CAF50";

        // Uterine Status
        [ObservableProperty]
        private string _uterineStatus = "Firm";

        [ObservableProperty]
        private string _uterineStatusColor = "#4CAF50";

        #endregion

        #region Vital Signs Tracking Properties

        [ObservableProperty]
        private int _vitalSignsRecorded = 0;

        [ObservableProperty]
        private int _requiredVitalSigns = 8;

        [ObservableProperty]
        private string _vitalSignsProgress = "0/8";

        [ObservableProperty]
        private double _vitalSignsProgressValue = 0;

        [ObservableProperty]
        private string _lastVitalSignsTime = "Not recorded";

        [ObservableProperty]
        private string _nextVitalSignsDue = "Now";

        [ObservableProperty]
        private bool _isVitalSignsDue;

        #endregion

        #region Timer Properties

        [ObservableProperty]
        private string _timerDisplay = "00:00";

        [ObservableProperty]
        private double _progressValue = 0;

        [ObservableProperty]
        private bool _isMonitoringComplete;

        [ObservableProperty]
        private bool _canComplete;

        #endregion

        #region Alert Properties

        [ObservableProperty]
        private string _alertMessage = string.Empty;

        [ObservableProperty]
        private bool _hasAlert;

        [ObservableProperty]
        private string _alertColor = "#2196F3";

        #endregion

        #region Mother-Baby Bonding Properties

        [ObservableProperty]
        private bool _skinToSkinInitiated;

        [ObservableProperty]
        private bool _breastfeedingInitiated;

        [ObservableProperty]
        private bool _babyVitalsStable = true;

        [ObservableProperty]
        private ObservableCollection<BabyDetails> _babies = new();

        [ObservableProperty]
        private bool _hasBabies;

        [ObservableProperty]
        private int _babyCount;

        #endregion

        #region Popup Actions

        public Action? OpenCompletionChecklistPopup { get; set; }
        public Action? CloseCompletionChecklistPopup { get; set; }
        public Action? OpenBpPulsePopup { get; set; }
        public Action? CloseBpPulsePopup { get; set; }
        public Action? OpenTemperaturePopup { get; set; }
        public Action? CloseTemperaturePopup { get; set; }
        public Action? OpenVitalsTrendPopup { get; set; }
        public Action? CloseVitalsTrendPopup { get; set; }
        public Action? OpenTemperatureTrendPopup { get; set; }
        public Action? CloseTemperatureTrendPopup { get; set; }
        public Action? OpenFourthStageAssessmentPopup { get; set; }
        public Action? CloseFourthStageAssessmentPopup { get; set; }

        #endregion

        #region PageModel References

        public CompletionChecklistPopupPageModel CompletionChecklistPopupPageModel => _completionChecklistPopupPageModel;
        public BPPulseModalPageModel BPPulseModalPageModel => _bpPulseModalPageModel;
        public TemperatureModalPageModel TemperatureModalPageModel => _temperatureModalPageModel;

        #endregion

        public FourthStagePartographPageModel(
            PatientRepository patientRepository,
            PartographRepository partographRepository,
            ModalErrorHandler errorHandler,
            BPRepository bpRepository,
            TemperatureRepository temperatureRepository,
            BirthOutcomeRepository birthOutcomeRepository,
            BabyDetailsRepository babyDetailsRepository,
            FourthStageVitalsRepository fourthStageVitalsRepository,
            LabourTimerService labourTimerService,
            CompletionChecklistPopupPageModel completionChecklistPopupPageModel,
            BPPulseModalPageModel bpPulseModalPageModel,
            TemperatureModalPageModel temperatureModalPageModel)
        {
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
            _errorHandler = errorHandler;
            _bpRepository = bpRepository;
            _temperatureRepository = temperatureRepository;
            _birthOutcomeRepository = birthOutcomeRepository;
            _babyDetailsRepository = babyDetailsRepository;
            _fourthStageVitalsRepository = fourthStageVitalsRepository;
            _labourTimerService = labourTimerService;
            _completionChecklistPopupPageModel = completionChecklistPopupPageModel;
            _bpPulseModalPageModel = bpPulseModalPageModel;
            _temperatureModalPageModel = temperatureModalPageModel;

            // Subscribe to timer events
            _labourTimerService.OnTimerUpdate += OnTimerUpdate;
            _labourTimerService.OnVitalSignsReminder += OnVitalSignsReminder;
            _labourTimerService.OnFourthStageComplete += OnFourthStageComplete;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("patientId"))
            {
                var patientId = Guid.Parse(query["patientId"].ToString());
                Task.Run(async () => await LoadPatientData(patientId));
            }
        }

        #region Data Loading Methods

        private async Task LoadPatientData(Guid patientId)
        {
            try
            {
                IsBusy = true;
                Patient = await _partographRepository.GetAsync(patientId);

                if (Patient?.Patient != null)
                {
                    PatientName = Patient.Name;
                    PatientInfo = Patient.DisplayInfo;

                    // Calculate fourth stage duration
                    if (Patient.FourthStageStartTime.HasValue)
                    {
                        var duration = DateTime.Now - Patient.FourthStageStartTime.Value;
                        FourthStageDuration = $"{duration.Hours:D2}h {duration.Minutes:D2}m";

                        // Calculate remaining time
                        var remaining = TimeSpan.FromHours(2) - duration;
                        if (remaining.TotalSeconds > 0)
                        {
                            RemainingTime = $"{(int)remaining.TotalMinutes} min remaining";
                            ProgressValue = Math.Min(duration.TotalMinutes / 120.0, 1.0);
                        }
                        else
                        {
                            RemainingTime = "Monitoring complete";
                            ProgressValue = 1.0;
                            IsMonitoringComplete = true;
                        }
                    }

                    // Load all related data in parallel
                    await Task.WhenAll(
                        LoadVitalSignsData(),
                        LoadLatestVitals(),
                        LoadFourthStageAssessments(),
                        LoadBabiesAsync()
                    );

                    // Update completion eligibility
                    UpdateCompletionStatus();

                    // Start timer monitoring
                    _labourTimerService.StartMonitoring(Patient);
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadVitalSignsData()
        {
            if (Patient?.ID == null)
                return;

            try
            {
                // Count BP readings since fourth stage started
                var bpReadings = await _bpRepository.ListByPatientAsync(Patient.ID);
                var fourthStageStart = Patient.FourthStageStartTime ?? DateTime.Now;

                var fourthStageBPs = bpReadings
                    .Where(bp => bp.Time >= fourthStageStart)
                    .OrderByDescending(bp => bp.Time)
                    .ToList();

                VitalSignsRecorded = fourthStageBPs.Count;
                VitalSignsProgress = $"{VitalSignsRecorded}/{RequiredVitalSigns}";
                VitalSignsProgressValue = Math.Min((double)VitalSignsRecorded / RequiredVitalSigns, 1.0);

                // Update latest vitals display
                var latestBP = fourthStageBPs.FirstOrDefault();
                if (latestBP != null)
                {
                    LastVitalSignsTime = latestBP.Time.ToString("HH:mm");
                    LastRecordedTime = latestBP.Time;

                    // Calculate next due time (15 min intervals)
                    var nextDue = latestBP.Time.AddMinutes(15);
                    if (nextDue <= DateTime.Now)
                    {
                        NextVitalSignsDue = "Due Now";
                        IsVitalSignsDue = true;
                    }
                    else
                    {
                        NextVitalSignsDue = nextDue.ToString("HH:mm");
                        IsVitalSignsDue = false;
                    }
                }
                else
                {
                    NextVitalSignsDue = "Record now";
                    IsVitalSignsDue = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading vital signs: {ex.Message}");
            }
        }

        private async Task LoadLatestVitals()
        {
            if (Patient?.ID == null)
                return;

            try
            {
                var fourthStageStart = Patient.FourthStageStartTime ?? DateTime.Now;

                // Load BP/Pulse history
                var bpList = await _bpRepository.ListByPatientAsync(Patient.ID);
                var fourthStageBPs = bpList
                    .Where(bp => bp.Time >= fourthStageStart)
                    .OrderByDescending(b => b.Time)
                    .Take(10)
                    .ToList();

                BpHistory = new ObservableCollection<BP>(fourthStageBPs);

                var latestBp = fourthStageBPs.FirstOrDefault();
                if (latestBp != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        BpLatestValue = $"{latestBp.Systolic}/{latestBp.Diastolic} mmHg";
                        PulseLatestValue = $"{latestBp.Pulse} bpm";
                        BpStatusText = GetBpStatusText(latestBp.Systolic, latestBp.Diastolic);
                        BpButtonColor = GetBpColor(latestBp.Systolic, latestBp.Diastolic);
                        PulseStatusText = GetPulseStatusText(latestBp.Pulse);
                        PulseButtonColor = GetPulseColor(latestBp.Pulse);
                    });
                }

                // Load Temperature history
                var tempList = await _temperatureRepository.ListByPatientAsync(Patient.ID);
                var fourthStageTemps = tempList
                    .Where(t => t.Time >= fourthStageStart)
                    .OrderByDescending(t => t.Time)
                    .Take(10)
                    .ToList();

                TemperatureHistory = new ObservableCollection<Temperature>(fourthStageTemps);

                var latestTemp = fourthStageTemps.FirstOrDefault();
                if (latestTemp != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        TemperatureLatestValue = $"{latestTemp.TemperatureCelsius:F1}°C";
                        TemperatureStatusText = GetTemperatureStatusText(latestTemp.TemperatureCelsius);
                        TemperatureButtonColor = GetTemperatureColor(latestTemp.TemperatureCelsius);
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading vitals: {ex.Message}");
            }
        }

        private async Task LoadFourthStageAssessments()
        {
            if (Patient?.ID == null)
                return;

            try
            {
                var fourthStageStart = Patient.FourthStageStartTime ?? DateTime.Now;
                var assessments = await _fourthStageVitalsRepository.ListByPatientAsync(Patient.ID);
                var latestAssessment = assessments
                    .Where(a => a.Time >= fourthStageStart)
                    .OrderByDescending(a => a.Time)
                    .FirstOrDefault();

                if (latestAssessment != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Fundal Height
                        FundalHeight = GetFundalHeightDisplayText(latestAssessment.FundalHeight);
                        FundalHeightColor = GetFundalHeightColor(latestAssessment.FundalHeight);
                        FundalHeightStatus = GetFundalHeightStatusText(latestAssessment.FundalHeight);

                        // Bleeding
                        BleedingStatus = GetBleedingDisplayText(latestAssessment.BleedingStatus);
                        BleedingStatusColor = GetBleedingColor(latestAssessment.BleedingStatus);
                        if (latestAssessment.EstimatedBloodLossMl.HasValue)
                        {
                            EstimatedBloodLoss = $"{latestAssessment.EstimatedBloodLossMl} mL";
                        }

                        // Bladder
                        BladderStatus = GetBladderDisplayText(latestAssessment.BladderStatus);
                        BladderStatusColor = GetBladderColor(latestAssessment.BladderStatus);

                        // Uterine
                        UterineStatus = GetUterineDisplayText(latestAssessment.UterineStatus);
                        UterineStatusColor = GetUterineColor(latestAssessment.UterineStatus);

                        // Bonding
                        SkinToSkinInitiated = latestAssessment.SkinToSkinContact;
                        BreastfeedingInitiated = latestAssessment.BreastfeedingInitiated;
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading fourth stage assessments: {ex.Message}");
            }
        }

        private async Task LoadBabiesAsync()
        {
            if (Patient?.ID == null)
                return;

            try
            {
                var babies = await _babyDetailsRepository.GetByPartographIdAsync(Patient.ID);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Babies = new ObservableCollection<BabyDetails>(babies);
                    HasBabies = Babies.Count > 0;
                    BabyCount = Babies.Count;
                    BabyVitalsStable = babies.All(b => b.VitalStatus == BabyVitalStatus.LiveBirth);
                });
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        #endregion

        #region Status Helper Methods

        private string GetBpStatusText(int systolic, int diastolic)
        {
            if (systolic >= 160 || diastolic >= 110)
                return "⚠️ Severe HTN";
            if (systolic >= 140 || diastolic >= 90)
                return "Elevated";
            if (systolic < 90)
                return "⚠️ Low - PPH risk";
            return "Normal";
        }

        private string GetBpColor(int systolic, int diastolic)
        {
            if (systolic >= 160 || diastolic >= 110)
                return "#F44336"; // Red
            if (systolic >= 140 || diastolic >= 90)
                return "#FF9800"; // Orange
            if (systolic < 90)
                return "#F44336"; // Red - hypotension is serious postpartum
            return "#4CAF50"; // Green
        }

        private string GetPulseStatusText(int pulse)
        {
            if (pulse >= 120) return "⚠️ Tachycardia - PPH?";
            if (pulse >= 100) return "Elevated";
            if (pulse < 60) return "Bradycardia";
            return "Normal";
        }

        private string GetPulseColor(int pulse)
        {
            if (pulse >= 120) return "#F44336"; // Red
            if (pulse >= 100) return "#FF9800"; // Orange
            if (pulse < 60) return "#FF9800"; // Orange
            return "#4CAF50"; // Green
        }

        private string GetTemperatureStatusText(float temp)
        {
            if (temp >= 38.5f) return "⚠️ High fever";
            if (temp >= 37.5f) return "Elevated";
            if (temp < 35.0f) return "⚠️ Hypothermia";
            return "Normal";
        }

        private string GetTemperatureColor(float temp)
        {
            if (temp >= 38.5f) return "#F44336"; // Red
            if (temp >= 37.5f) return "#FF9800"; // Orange
            if (temp < 35.0f) return "#FF9800"; // Orange
            return "#4CAF50"; // Green
        }

        private string GetFundalHeightDisplayText(FundalHeightStatus status) => status switch
        {
            FundalHeightStatus.AtUmbilicus => "At umbilicus",
            FundalHeightStatus.OneFingerBelow => "1 finger below",
            FundalHeightStatus.TwoFingersBelow => "2 fingers below",
            FundalHeightStatus.ThreeFingersBelow => "3 fingers below",
            FundalHeightStatus.AboveUmbilicus => "Above umbilicus",
            FundalHeightStatus.NotPalpable => "Not palpable",
            _ => "Not recorded"
        };

        private string GetFundalHeightColor(FundalHeightStatus status) => status switch
        {
            FundalHeightStatus.AtUmbilicus => "#4CAF50",
            FundalHeightStatus.OneFingerBelow => "#4CAF50",
            FundalHeightStatus.TwoFingersBelow => "#4CAF50",
            FundalHeightStatus.ThreeFingersBelow => "#4CAF50",
            FundalHeightStatus.AboveUmbilicus => "#F44336", // Concern
            FundalHeightStatus.NotPalpable => "#F44336",
            _ => "#64748B"
        };

        private string GetFundalHeightStatusText(FundalHeightStatus status) => status switch
        {
            FundalHeightStatus.AboveUmbilicus => "⚠️ Check for atony/full bladder",
            FundalHeightStatus.NotPalpable => "⚠️ Urgent assessment needed",
            _ => "Normal"
        };

        private string GetBleedingDisplayText(BleedingStatus status) => status switch
        {
            BleedingStatus.NormalLochia => "Normal lochia",
            BleedingStatus.Minimal => "Minimal",
            BleedingStatus.Moderate => "Moderate",
            BleedingStatus.Heavy => "Heavy",
            BleedingStatus.Excessive => "Excessive - PPH",
            BleedingStatus.Clots => "Clots present",
            _ => "Not recorded"
        };

        private string GetBleedingColor(BleedingStatus status) => status switch
        {
            BleedingStatus.NormalLochia => "#4CAF50",
            BleedingStatus.Minimal => "#4CAF50",
            BleedingStatus.Moderate => "#FF9800",
            BleedingStatus.Heavy => "#F44336",
            BleedingStatus.Excessive => "#F44336",
            BleedingStatus.Clots => "#FF9800",
            _ => "#64748B"
        };

        private string GetBladderDisplayText(BladderStatus status) => status switch
        {
            BladderStatus.Empty => "Empty",
            BladderStatus.VoidedSpontaneously => "Voided spontaneously",
            BladderStatus.Palpable => "Palpable",
            BladderStatus.Distended => "Distended",
            BladderStatus.Catheterized => "Catheterized",
            _ => "Not recorded"
        };

        private string GetBladderColor(BladderStatus status) => status switch
        {
            BladderStatus.Empty => "#4CAF50",
            BladderStatus.VoidedSpontaneously => "#4CAF50",
            BladderStatus.Catheterized => "#4CAF50",
            BladderStatus.Palpable => "#FF9800",
            BladderStatus.Distended => "#F44336",
            _ => "#64748B"
        };

        private string GetUterineDisplayText(UterineStatus status) => status switch
        {
            UterineStatus.Firm => "Firm",
            UterineStatus.ModeratelyFirm => "Moderately firm",
            UterineStatus.Soft => "Soft",
            UterineStatus.Boggy => "Boggy - Atony",
            UterineStatus.NotPalpable => "Not palpable",
            _ => "Not recorded"
        };

        private string GetUterineColor(UterineStatus status) => status switch
        {
            UterineStatus.Firm => "#4CAF50",
            UterineStatus.ModeratelyFirm => "#4CAF50",
            UterineStatus.Soft => "#FF9800",
            UterineStatus.Boggy => "#F44336",
            UterineStatus.NotPalpable => "#F44336",
            _ => "#64748B"
        };

        private void UpdateCompletionStatus()
        {
            // Can complete if: 2 hours elapsed AND enough vital signs recorded
            CanComplete = IsMonitoringComplete && VitalSignsRecorded >= 6;

            if (IsMonitoringComplete && !CanComplete)
            {
                AlertMessage = $"Record at least {6 - VitalSignsRecorded} more vital signs before completing";
                AlertColor = "#FF9800";
                HasAlert = true;
            }
            else if (IsMonitoringComplete && CanComplete)
            {
                AlertMessage = "Monitoring period complete - Ready to discharge";
                AlertColor = "#4CAF50";
                HasAlert = true;
            }
            else
            {
                HasAlert = false;
            }
        }

        #endregion

        #region Timer Event Handlers

        private void OnTimerUpdate(object? sender, StageTimerUpdateEventArgs e)
        {
            if (e.CurrentStage == LaborStatus.FourthStage && e.StageDuration.HasValue)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    FourthStageDuration = e.DurationText;

                    if (e.RemainingTime.HasValue)
                    {
                        RemainingTime = e.RemainingTimeText;
                        ProgressValue = 1.0 - (e.RemainingTime.Value.TotalMinutes / 120.0);
                    }

                    if (e.RemainingTime?.TotalSeconds <= 0)
                    {
                        IsMonitoringComplete = true;
                        RemainingTime = "Complete";
                    }

                    TimerDisplay = $"{(int)e.StageDuration.Value.TotalMinutes:D2}:{e.StageDuration.Value.Seconds:D2}";
                    UpdateCompletionStatus();
                });
            }
        }

        private async void OnVitalSignsReminder(object? sender, TimerEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsVitalSignsDue = true;
                NextVitalSignsDue = "Due Now";
            });

            await ShowAlert("Vital Signs Due", e.Message);
        }

        private async void OnFourthStageComplete(object? sender, TimerEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsMonitoringComplete = true;
                UpdateCompletionStatus();
            });

            await ShowAlert("Monitoring Complete", e.Message);
        }

        private async Task ShowAlert(string title, string message)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current?.MainPage?.DisplayAlert(title, message, "OK");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing alert: {ex.Message}");
            }
        }

        #endregion

        #region Vitals Commands

        [RelayCommand]
        private async Task RecordBpPulse()
        {
            try
            {
                if (Patient == null)
                {
                    await AppShell.DisplayToastAsync("No patient loaded");
                    return;
                }

                _bpPulseModalPageModel._patient = Patient;
                await _bpPulseModalPageModel.LoadPatient(Patient.ID);
                _bpPulseModalPageModel.ClosePopup = async () =>
                {
                    CloseBpPulsePopup?.Invoke();
                    await LoadVitalSignsData();
                    await LoadLatestVitals();
                    UpdateCompletionStatus();
                };

                OpenBpPulsePopup?.Invoke();
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        [RelayCommand]
        private async Task RecordTemperature()
        {
            try
            {
                if (Patient == null)
                {
                    await AppShell.DisplayToastAsync("No patient loaded");
                    return;
                }

                _temperatureModalPageModel._patient = Patient;
                await _temperatureModalPageModel.LoadPatient(Patient.ID);
                _temperatureModalPageModel.ClosePopup = async () =>
                {
                    CloseTemperaturePopup?.Invoke();
                    await LoadLatestVitals();
                };

                OpenTemperaturePopup?.Invoke();
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        [RelayCommand]
        private void ShowBpTrend()
        {
            OpenVitalsTrendPopup?.Invoke();
        }

        [RelayCommand]
        private void ShowTemperatureTrend()
        {
            OpenTemperatureTrendPopup?.Invoke();
        }

        [RelayCommand]
        private void CloseTrend()
        {
            CloseVitalsTrendPopup?.Invoke();
        }

        [RelayCommand]
        private void CloseTemperatureTrend()
        {
            CloseTemperatureTrendPopup?.Invoke();
        }

        #endregion

        #region Fourth Stage Assessment Commands

        [RelayCommand]
        private async Task UpdateFundalHeight()
        {
            var options = new[] { "At umbilicus", "1 finger below", "2 fingers below", "3 fingers below", "Above umbilicus (concern)" };
            var result = await Application.Current?.MainPage?.DisplayActionSheet(
                "Fundal Height", "Cancel", null, options);

            if (!string.IsNullOrEmpty(result) && result != "Cancel")
            {
                FundalHeight = result;
                var status = result switch
                {
                    "At umbilicus" => FundalHeightStatus.AtUmbilicus,
                    "1 finger below" => FundalHeightStatus.OneFingerBelow,
                    "2 fingers below" => FundalHeightStatus.TwoFingersBelow,
                    "3 fingers below" => FundalHeightStatus.ThreeFingersBelow,
                    "Above umbilicus (concern)" => FundalHeightStatus.AboveUmbilicus,
                    _ => FundalHeightStatus.AtUmbilicus
                };

                FundalHeightColor = GetFundalHeightColor(status);
                FundalHeightStatus = GetFundalHeightStatusText(status);

                await SaveFourthStageAssessment();
                await AppShell.DisplayToastAsync("Fundal height updated");
            }
        }

        [RelayCommand]
        private async Task UpdateBleedingStatus()
        {
            var options = new[] { "Normal lochia (rubra)", "Minimal", "Moderate", "Heavy (PPH concern)", "Excessive - PPH", "Clots present" };
            var result = await Application.Current?.MainPage?.DisplayActionSheet(
                "Bleeding Status", "Cancel", null, options);

            if (!string.IsNullOrEmpty(result) && result != "Cancel")
            {
                BleedingStatus = result;
                var status = result switch
                {
                    "Normal lochia (rubra)" => MODEL.BleedingStatus.NormalLochia,
                    "Minimal" => MODEL.BleedingStatus.Minimal,
                    "Moderate" => MODEL.BleedingStatus.Moderate,
                    "Heavy (PPH concern)" => MODEL.BleedingStatus.Heavy,
                    "Excessive - PPH" => MODEL.BleedingStatus.Excessive,
                    "Clots present" => MODEL.BleedingStatus.Clots,
                    _ => MODEL.BleedingStatus.NormalLochia
                };

                BleedingStatusColor = GetBleedingColor(status);

                if (result.Contains("Heavy") || result.Contains("PPH") || result.Contains("Excessive"))
                {
                    await Application.Current?.MainPage?.DisplayAlert(
                        "PPH Warning",
                        "Heavy bleeding detected. Assess uterine tone and implement hemorrhage protocol if needed.",
                        "OK");
                }

                await SaveFourthStageAssessment();
                await AppShell.DisplayToastAsync("Bleeding status updated");
            }
        }

        [RelayCommand]
        private async Task UpdateBladderStatus()
        {
            var options = new[] { "Empty", "Voided spontaneously", "Palpable bladder", "Distended", "Catheterized" };
            var result = await Application.Current?.MainPage?.DisplayActionSheet(
                "Bladder Status", "Cancel", null, options);

            if (!string.IsNullOrEmpty(result) && result != "Cancel")
            {
                BladderStatus = result;
                var status = result switch
                {
                    "Empty" => MODEL.BladderStatus.Empty,
                    "Voided spontaneously" => MODEL.BladderStatus.VoidedSpontaneously,
                    "Palpable bladder" => MODEL.BladderStatus.Palpable,
                    "Distended" => MODEL.BladderStatus.Distended,
                    "Catheterized" => MODEL.BladderStatus.Catheterized,
                    _ => MODEL.BladderStatus.Empty
                };

                BladderStatusColor = GetBladderColor(status);

                if (result.Contains("Palpable") || result.Contains("Distended"))
                {
                    await Application.Current?.MainPage?.DisplayAlert(
                        "Bladder Alert",
                        "Full bladder may prevent uterine contraction. Consider encouraging voiding or catheterization.",
                        "OK");
                }

                await SaveFourthStageAssessment();
                await AppShell.DisplayToastAsync("Bladder status updated");
            }
        }

        [RelayCommand]
        private async Task UpdateUterineStatus()
        {
            var options = new[] { "Firm", "Moderately firm", "Soft", "Boggy - Atony risk" };
            var result = await Application.Current?.MainPage?.DisplayActionSheet(
                "Uterine Status", "Cancel", null, options);

            if (!string.IsNullOrEmpty(result) && result != "Cancel")
            {
                UterineStatus = result;
                var status = result switch
                {
                    "Firm" => MODEL.UterineStatus.Firm,
                    "Moderately firm" => MODEL.UterineStatus.ModeratelyFirm,
                    "Soft" => MODEL.UterineStatus.Soft,
                    "Boggy - Atony risk" => MODEL.UterineStatus.Boggy,
                    _ => MODEL.UterineStatus.Firm
                };

                UterineStatusColor = GetUterineColor(status);

                if (result.Contains("Soft") || result.Contains("Boggy"))
                {
                    await Application.Current?.MainPage?.DisplayAlert(
                        "Uterine Atony Alert",
                        "Uterus is not well contracted. Perform fundal massage and consider uterotonics.",
                        "OK");
                }

                await SaveFourthStageAssessment();
                await AppShell.DisplayToastAsync("Uterine status updated");
            }
        }

        private async Task SaveFourthStageAssessment()
        {
            if (Patient?.ID == null)
                return;

            try
            {
                var assessment = new FourthStageVitals
                {
                    ID = Guid.NewGuid(),
                    PartographID = Patient.ID.Value,
                    Time = DateTime.Now,
                    Handler = Constants.Staff?.ID,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    FundalHeight = ParseFundalHeight(FundalHeight),
                    BleedingStatus = ParseBleedingStatus(BleedingStatus),
                    BladderStatus = ParseBladderStatus(BladderStatus),
                    UterineStatus = ParseUterineStatus(UterineStatus),
                    SkinToSkinContact = SkinToSkinInitiated,
                    BreastfeedingInitiated = BreastfeedingInitiated,
                    CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                await _fourthStageVitalsRepository.SaveItemAsync(assessment);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving fourth stage assessment: {ex.Message}");
            }
        }

        private FundalHeightStatus ParseFundalHeight(string text) => text switch
        {
            "At umbilicus" => FundalHeightStatus.AtUmbilicus,
            "1 finger below" => FundalHeightStatus.OneFingerBelow,
            "2 fingers below" => FundalHeightStatus.TwoFingersBelow,
            "3 fingers below" => FundalHeightStatus.ThreeFingersBelow,
            _ when text.Contains("Above") => FundalHeightStatus.AboveUmbilicus,
            _ => FundalHeightStatus.AtUmbilicus
        };

        private BleedingStatus ParseBleedingStatus(string text) => text switch
        {
            _ when text.Contains("Normal") => MODEL.BleedingStatus.NormalLochia,
            "Minimal" => MODEL.BleedingStatus.Minimal,
            "Moderate" => MODEL.BleedingStatus.Moderate,
            _ when text.Contains("Heavy") => MODEL.BleedingStatus.Heavy,
            _ when text.Contains("Excessive") || text.Contains("PPH") => MODEL.BleedingStatus.Excessive,
            _ when text.Contains("Clots") => MODEL.BleedingStatus.Clots,
            _ => MODEL.BleedingStatus.NormalLochia
        };

        private BladderStatus ParseBladderStatus(string text) => text switch
        {
            "Empty" => MODEL.BladderStatus.Empty,
            _ when text.Contains("Voided") => MODEL.BladderStatus.VoidedSpontaneously,
            _ when text.Contains("Palpable") => MODEL.BladderStatus.Palpable,
            _ when text.Contains("Distended") => MODEL.BladderStatus.Distended,
            _ when text.Contains("Catheter") => MODEL.BladderStatus.Catheterized,
            _ => MODEL.BladderStatus.Empty
        };

        private UterineStatus ParseUterineStatus(string text) => text switch
        {
            "Firm" => MODEL.UterineStatus.Firm,
            _ when text.Contains("Moderately") => MODEL.UterineStatus.ModeratelyFirm,
            "Soft" => MODEL.UterineStatus.Soft,
            _ when text.Contains("Boggy") => MODEL.UterineStatus.Boggy,
            _ => MODEL.UterineStatus.Firm
        };

        #endregion

        #region Completion Commands

        [RelayCommand]
        private async Task CompleteDelivery()
        {
            try
            {
                if (Patient == null)
                {
                    await AppShell.DisplayToastAsync("No patient loaded");
                    return;
                }

                // Initialize and show the completion checklist popup
                _completionChecklistPopupPageModel.Initialize(Patient, VitalSignsRecorded);
                _completionChecklistPopupPageModel.ClosePopup = () => CloseCompletionChecklistPopup?.Invoke();
                _completionChecklistPopupPageModel.OnCompletionConfirmed = async (data) =>
                {
                    await ProcessCompletion(data);
                };

                OpenCompletionChecklistPopup?.Invoke();
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        private async Task ProcessCompletion(CompletionChecklistData data)
        {
            if (Patient?.ID == null)
                return;

            try
            {
                // Update partograph status
                Patient.Status = LaborStatus.Completed;
                Patient.CompletedTime = data.CompletionTime;
                await _partographRepository.SaveItemAsync(Patient);

                // Stop timer monitoring
                _labourTimerService.StopMonitoring();

                await AppShell.DisplayToastAsync("Delivery completed successfully!");

                // Show summary
                await Application.Current?.MainPage?.DisplayAlert(
                    "Delivery Completed",
                    $"Patient transferred to: {data.DischargeDestination}\n" +
                    $"Checklist: {data.ChecklistCompletionPercentage}% complete\n" +
                    $"Vital signs recorded: {data.VitalSignsRecorded}",
                    "OK");

                // Navigate back to active patients
                await Shell.Current.GoToAsync("//activepatients");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
                await AppShell.DisplayToastAsync("Failed to complete delivery");
            }
        }

        #endregion

        #region Navigation Commands

        [RelayCommand]
        private async Task NavigateToBirthOutcome()
        {
            if (Patient?.ID == null)
                return;

            var parameters = new Dictionary<string, object>
            {
                { "PartographId", Patient.ID.ToString() }
            };

            await Shell.Current.GoToAsync("BirthOutcomePage", parameters);
        }

        [RelayCommand]
        private async Task NavigateToBabyDetails()
        {
            if (Patient?.ID == null)
                return;

            try
            {
                var birthOutcome = await _birthOutcomeRepository.GetByPartographIdAsync(Patient.ID);
                if (birthOutcome == null)
                {
                    await AppShell.DisplayToastAsync("Please record birth outcome first");
                    await NavigateToBirthOutcome();
                    return;
                }

                var parameters = new Dictionary<string, object>
                {
                    { "PartographId", Patient.ID.ToString() },
                    { "BirthOutcomeId", birthOutcome.ID.ToString() },
                    { "NumberOfBabies", "1" }
                };

                await Shell.Current.GoToAsync("BabyDetailsPage", parameters);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        [RelayCommand]
        private async Task ViewBabyDetails(BabyDetails baby)
        {
            if (baby == null || Patient?.ID == null)
                return;

            try
            {
                var birthOutcome = await _birthOutcomeRepository.GetByPartographIdAsync(Patient.ID);
                if (birthOutcome == null)
                {
                    await AppShell.DisplayToastAsync("Birth outcome not found");
                    return;
                }

                var parameters = new Dictionary<string, object>
                {
                    { "PartographId", Patient.ID.ToString() },
                    { "BirthOutcomeId", birthOutcome.ID.ToString() },
                    { "BabyId", baby.ID.ToString() }
                };

                await Shell.Current.GoToAsync("BabyDetailsPage", parameters);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        #endregion

        #region Other Commands

        [RelayCommand]
        private async Task Refresh()
        {
            if (Patient?.ID != null)
            {
                await LoadPatientData(Patient.ID.Value);
            }
        }

        [RelayCommand]
        private async Task Print()
        {
            await AppShell.DisplayToastAsync("Print functionality coming soon");
        }

        #endregion
    }
}
