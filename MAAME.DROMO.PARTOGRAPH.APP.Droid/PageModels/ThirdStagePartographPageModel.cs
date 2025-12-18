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
    /// Third Stage Partograph Page Model - Manages placenta delivery monitoring
    /// WHO Guidelines: Third stage should not exceed 30 minutes
    /// Enhanced with maternal vitals monitoring, baby listing, and APGAR tracking
    /// </summary>
    public partial class ThirdStagePartographPageModel : ObservableObject, IQueryAttributable
    {
        public Partograph? Patient { get; set; }
        private readonly PatientRepository _patientRepository;
        private readonly PartographRepository _partographRepository;
        private readonly ModalErrorHandler _errorHandler;
        private readonly BPRepository _bpRepository;
        private readonly TemperatureRepository _temperatureRepository;
        private readonly BirthOutcomeRepository _birthOutcomeRepository;
        private readonly BabyDetailsRepository _babyDetailsRepository;
        private readonly LabourTimerService _labourTimerService;
        private readonly PlacentaDeliveryPopupPageModel _placentaDeliveryPopupPageModel;
        private readonly BPPulseModalPageModel _bpPulseModalPageModel;
        private readonly TemperatureModalPageModel _temperatureModalPageModel;

        #region Patient Info Properties

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private string _patientInfo = string.Empty;

        [ObservableProperty]
        private string _thirdStageDuration = string.Empty;

        [ObservableProperty]
        private string _timeSinceBabyDelivery = string.Empty;

        [ObservableProperty]
        private DateTime? _lastRecordedTime;

        [ObservableProperty]
        bool _isBusy;

        #endregion

        #region Placenta Status Properties

        [ObservableProperty]
        private string _placentaStatus = "Awaiting Delivery";

        [ObservableProperty]
        private string _placentaStatusColor = "#FF9800";

        [ObservableProperty]
        private string _bloodLoss = "Not recorded";

        [ObservableProperty]
        private string _bloodLossColor = "#64748B";

        [ObservableProperty]
        private string _uterineStatus = "Monitoring";

        [ObservableProperty]
        private string _uterineStatusColor = "#64748B";

        [ObservableProperty]
        private bool _isPlacentaDelivered;

        [ObservableProperty]
        private string _activeManagementStatus = "Not recorded";

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

        #region APGAR Tracking Properties

        [ObservableProperty]
        private string _apgar1Status = "Due";

        [ObservableProperty]
        private string _apgar1StatusColor = "#FF9800";

        [ObservableProperty]
        private int? _apgar1Score;

        [ObservableProperty]
        private string _apgar5Status = "Pending";

        [ObservableProperty]
        private string _apgar5StatusColor = "#9E9E9E";

        [ObservableProperty]
        private int? _apgar5Score;

        [ObservableProperty]
        private string _apgar10Status = "Pending";

        [ObservableProperty]
        private string _apgar10StatusColor = "#9E9E9E";

        #endregion

        #region Timer Properties

        [ObservableProperty]
        private string _timerDisplay = "00:00";

        [ObservableProperty]
        private bool _isTimerWarning;

        [ObservableProperty]
        private bool _isTimerCritical;

        #endregion

        #region Alert Properties

        [ObservableProperty]
        private string _alertMessage = string.Empty;

        [ObservableProperty]
        private bool _hasAlert;

        [ObservableProperty]
        private string _alertColor = "#FF9800";

        #endregion

        #region Babies Properties

        [ObservableProperty]
        private ObservableCollection<BabyDetails> _babies = new();

        [ObservableProperty]
        private bool _hasBabies;

        [ObservableProperty]
        private int _babyCount;

        [ObservableProperty]
        private BabyDetails? _selectedBaby;

        #endregion

        #region Popup Actions

        public Action? OpenPlacentaDeliveryPopup { get; set; }
        public Action? ClosePlacentaDeliveryPopup { get; set; }
        public Action? OpenBpPulsePopup { get; set; }
        public Action? CloseBpPulsePopup { get; set; }
        public Action? OpenTemperaturePopup { get; set; }
        public Action? CloseTemperaturePopup { get; set; }
        public Action? OpenVitalsTrendPopup { get; set; }
        public Action? CloseVitalsTrendPopup { get; set; }
        public Action? OpenQuickAddBabyPopup { get; set; }
        public Action? CloseQuickAddBabyPopup { get; set; }
        public Action<BabyDetails>? OpenBabyApgarPopup { get; set; }
        public Action? CloseBabyApgarPopup { get; set; }

        #endregion

        #region PageModel References

        public PlacentaDeliveryPopupPageModel PlacentaDeliveryPopupPageModel => _placentaDeliveryPopupPageModel;
        public BPPulseModalPageModel BPPulseModalPageModel => _bpPulseModalPageModel;
        public TemperatureModalPageModel TemperatureModalPageModel => _temperatureModalPageModel;

        #endregion

        public ThirdStagePartographPageModel(
            PatientRepository patientRepository,
            PartographRepository partographRepository,
            ModalErrorHandler errorHandler,
            BPRepository bpRepository,
            TemperatureRepository temperatureRepository,
            BirthOutcomeRepository birthOutcomeRepository,
            BabyDetailsRepository babyDetailsRepository,
            LabourTimerService labourTimerService,
            PlacentaDeliveryPopupPageModel placentaDeliveryPopupPageModel,
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
            _labourTimerService = labourTimerService;
            _placentaDeliveryPopupPageModel = placentaDeliveryPopupPageModel;
            _bpPulseModalPageModel = bpPulseModalPageModel;
            _temperatureModalPageModel = temperatureModalPageModel;

            // Subscribe to timer events
            _labourTimerService.OnTimerUpdate += OnTimerUpdate;
            _labourTimerService.OnApgar1Reminder += OnApgar1Reminder;
            _labourTimerService.OnApgar5Reminder += OnApgar5Reminder;
            _labourTimerService.OnApgar10Reminder += OnApgar10Reminder;
            _labourTimerService.OnPlacentaWarning += OnPlacentaWarning;
            _labourTimerService.OnPlacentaCritical += OnPlacentaCritical;
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

                    // Calculate third stage duration
                    if (Patient.ThirdStageStartTime.HasValue)
                    {
                        var duration = DateTime.Now - Patient.ThirdStageStartTime.Value;
                        ThirdStageDuration = $"{duration.Hours:D2}h {duration.Minutes:D2}m {duration.Seconds:D2}s";
                        UpdateTimerWarnings(duration);
                    }

                    // Calculate time since baby delivery
                    if (Patient.DeliveryTime.HasValue)
                    {
                        var timeSince = DateTime.Now - Patient.DeliveryTime.Value;
                        TimeSinceBabyDelivery = $"{(int)timeSince.TotalMinutes} minutes since delivery";
                    }

                    // Check if placenta already delivered
                    IsPlacentaDelivered = Patient.FourthStageStartTime.HasValue;
                    if (IsPlacentaDelivered)
                    {
                        PlacentaStatus = "Delivered";
                        PlacentaStatusColor = "#4CAF50";
                    }

                    // Load all related data
                    await Task.WhenAll(
                        LoadApgarScores(),
                        LoadBabiesAsync(),
                        LoadLatestVitals(),
                        LoadBirthOutcomeData()
                    );

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

        private async Task LoadApgarScores()
        {
            if (Patient?.ID == null)
                return;

            try
            {
                var babies = await _babyDetailsRepository.GetByPartographIdAsync(Patient.ID);
                var firstBaby = babies.FirstOrDefault();

                if (firstBaby != null)
                {
                    if (firstBaby.Apgar1Min.HasValue)
                    {
                        Apgar1Score = firstBaby.Apgar1Min;
                        Apgar1Status = $"Score: {firstBaby.Apgar1Min}";
                        Apgar1StatusColor = GetApgarColor(firstBaby.Apgar1Min.Value);
                    }

                    if (firstBaby.Apgar5Min.HasValue)
                    {
                        Apgar5Score = firstBaby.Apgar5Min;
                        Apgar5Status = $"Score: {firstBaby.Apgar5Min}";
                        Apgar5StatusColor = GetApgarColor(firstBaby.Apgar5Min.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading APGAR scores: {ex.Message}");
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
                });
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
                await AppShell.DisplayToastAsync($"Error loading babies: {ex.Message}");
            }
        }

        private async Task LoadLatestVitals()
        {
            if (Patient?.ID == null)
                return;

            try
            {
                // Load BP/Pulse history
                var bpList = await _bpRepository.ListByPatientAsync(Patient.ID);
                BpHistory = new ObservableCollection<BP>(bpList.OrderByDescending(b => b.Time).Take(10));

                var latestBp = bpList.OrderByDescending(b => b.Time).FirstOrDefault();
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
                        LastRecordedTime = latestBp.Time;
                    });
                }

                // Load Temperature history
                var tempList = await _temperatureRepository.ListByPatientAsync(Patient.ID);
                TemperatureHistory = new ObservableCollection<Temperature>(tempList.OrderByDescending(t => t.Time).Take(10));

                var latestTemp = tempList.OrderByDescending(t => t.Time).FirstOrDefault();
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

        private async Task LoadBirthOutcomeData()
        {
            if (Patient?.ID == null)
                return;

            try
            {
                var birthOutcome = await _birthOutcomeRepository.GetByPartographIdAsync(Patient.ID);
                if (birthOutcome != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (birthOutcome.PlacentaDeliveryTime.HasValue)
                        {
                            PlacentaStatus = $"Delivered at {birthOutcome.PlacentaDeliveryTime.Value:HH:mm}";
                            PlacentaStatusColor = "#4CAF50";
                            IsPlacentaDelivered = true;
                        }

                        if (birthOutcome.EstimatedBloodLoss.HasValue)
                        {
                            BloodLoss = $"{birthOutcome.EstimatedBloodLoss} mL";
                            BloodLossColor = GetBloodLossColor(birthOutcome.EstimatedBloodLoss.Value);
                        }

                        if (birthOutcome.OxytocinGivenPostDelivery)
                        {
                            ActiveManagementStatus = "Oxytocin given ✓";
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading birth outcome: {ex.Message}");
            }
        }

        #endregion

        #region Status Helper Methods

        private string GetApgarColor(int score)
        {
            if (score >= 7) return "#4CAF50"; // Green - Normal
            if (score >= 4) return "#FF9800"; // Orange - Moderate
            return "#F44336"; // Red - Severe
        }

        private string GetBpStatusText(int systolic, int diastolic)
        {
            if (systolic >= 160 || diastolic >= 110)
                return "⚠️ Severe HTN";
            if (systolic >= 140 || diastolic >= 90)
                return "Elevated";
            if (systolic < 90)
                return "Low";
            return "Normal";
        }

        private string GetBpColor(int systolic, int diastolic)
        {
            if (systolic >= 160 || diastolic >= 110)
                return "#F44336"; // Red
            if (systolic >= 140 || diastolic >= 90)
                return "#FF9800"; // Orange
            if (systolic < 90)
                return "#FF9800"; // Orange
            return "#4CAF50"; // Green
        }

        private string GetPulseStatusText(int pulse)
        {
            if (pulse >= 120) return "⚠️ Tachycardia";
            if (pulse < 60) return "Bradycardia";
            return "Normal";
        }

        private string GetPulseColor(int pulse)
        {
            if (pulse >= 120) return "#F44336"; // Red
            if (pulse < 60) return "#FF9800"; // Orange
            return "#4CAF50"; // Green
        }

        private string GetTemperatureStatusText(float temp)
        {
            if (temp >= 38.5f) return "⚠️ High fever";
            if (temp >= 37.5f) return "Elevated";
            if (temp < 35.0f) return "Hypothermia";
            return "Normal";
        }

        private string GetTemperatureColor(float temp)
        {
            if (temp >= 38.5f) return "#F44336"; // Red
            if (temp >= 37.5f) return "#FF9800"; // Orange
            if (temp < 35.0f) return "#FF9800"; // Orange
            return "#4CAF50"; // Green
        }

        private string GetBloodLossColor(int bloodLoss)
        {
            if (bloodLoss >= 1000) return "#F44336"; // Red - Severe PPH
            if (bloodLoss >= 500) return "#FF9800"; // Orange - PPH warning
            return "#4CAF50"; // Green - Normal
        }

        private void UpdateTimerWarnings(TimeSpan duration)
        {
            IsTimerWarning = duration.TotalMinutes >= 20 && duration.TotalMinutes < 30;
            IsTimerCritical = duration.TotalMinutes >= 30;

            if (IsTimerCritical)
            {
                AlertMessage = "URGENT: Third stage exceeds 30 minutes - Consider manual removal of placenta";
                AlertColor = "#F44336";
                HasAlert = true;
            }
            else if (IsTimerWarning)
            {
                AlertMessage = "Warning: Third stage approaching 30 minutes - Monitor closely";
                AlertColor = "#FF9800";
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
            if (e.CurrentStage == LaborStatus.ThirdStage && e.StageDuration.HasValue)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ThirdStageDuration = e.DurationText;
                    TimerDisplay = $"{(int)e.StageDuration.Value.TotalMinutes:D2}:{e.StageDuration.Value.Seconds:D2}";
                    UpdateTimerWarnings(e.StageDuration.Value);

                    if (e.TimeSinceDelivery.HasValue)
                    {
                        TimeSinceBabyDelivery = $"{(int)e.TimeSinceDelivery.Value.TotalMinutes} min since delivery";
                    }
                });
            }
        }

        private async void OnApgar1Reminder(object? sender, TimerEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (!Apgar1Score.HasValue)
                {
                    Apgar1Status = "DUE NOW!";
                    Apgar1StatusColor = "#F44336";
                }
            });

            await ShowAlert("APGAR 1-Minute Score", e.Message);
        }

        private async void OnApgar5Reminder(object? sender, TimerEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (!Apgar5Score.HasValue)
                {
                    Apgar5Status = "DUE NOW!";
                    Apgar5StatusColor = "#F44336";
                }
            });

            await ShowAlert("APGAR 5-Minute Score", e.Message);
        }

        private async void OnApgar10Reminder(object? sender, TimerEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Apgar10Status = "Due (if indicated)";
                Apgar10StatusColor = "#FF9800";
            });

            // Only show if APGAR 5 was low
            if (Apgar5Score.HasValue && Apgar5Score.Value < 7)
            {
                await ShowAlert("APGAR 10-Minute Score", e.Message);
            }
        }

        private async void OnPlacentaWarning(object? sender, TimerEventArgs e)
        {
            await ShowAlert("Placenta Warning", e.Message);
        }

        private async void OnPlacentaCritical(object? sender, TimerEventArgs e)
        {
            await ShowAlert("URGENT - Retained Placenta Risk", e.Message);
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
                    await LoadLatestVitals();
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
            OpenVitalsTrendPopup?.Invoke();
        }

        [RelayCommand]
        private void Close()
        {
            CloseVitalsTrendPopup?.Invoke();
        }

        #endregion

        #region Placenta Commands

        [RelayCommand]
        private async Task RecordPlacentaDelivery()
        {
            try
            {
                if (Patient == null)
                {
                    await AppShell.DisplayToastAsync("No patient loaded");
                    return;
                }

                // Initialize and show the placenta delivery popup
                _placentaDeliveryPopupPageModel.Initialize(Patient.DeliveryTime, Patient.ThirdStageStartTime);
                _placentaDeliveryPopupPageModel.ClosePopup = () => ClosePlacentaDeliveryPopup?.Invoke();
                _placentaDeliveryPopupPageModel.OnPlacentaDeliveryConfirmed = async (data) =>
                {
                    await ProcessPlacentaDelivery(data);
                };

                OpenPlacentaDeliveryPopup?.Invoke();
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        [RelayCommand]
        private async Task EditPlacentaDetails()
        {
            // Re-open the placenta delivery popup for editing
            await RecordPlacentaDelivery();
        }

        private async Task ProcessPlacentaDelivery(PlacentaDeliveryData data)
        {
            if (Patient?.ID == null)
                return;

            try
            {
                // Update birth outcome with placenta details
                var birthOutcome = await _birthOutcomeRepository.GetByPartographIdAsync(Patient.ID);
                if (birthOutcome != null)
                {
                    birthOutcome.PlacentaDeliveryTime = data.PlacentaDeliveryTime;
                    birthOutcome.PlacentaComplete = data.PlacentaComplete;
                    birthOutcome.EstimatedBloodLoss = data.EstimatedBloodLossMl;
                    birthOutcome.PerinealStatus = data.PerinealStatus switch
                    {
                        "Intact" => PerinealStatus.Intact,
                        "1st Degree Tear" => PerinealStatus.FirstDegreeTear,
                        "2nd Degree Tear" => PerinealStatus.SecondDegreeTear,
                        "3rd Degree Tear" => PerinealStatus.ThirdDegreeTear,
                        "4th Degree Tear" => PerinealStatus.FourthDegreeTear,
                        "Episiotomy" => PerinealStatus.Episiotomy,
                        _ => PerinealStatus.Intact
                    };
                    birthOutcome.OxytocinGivenPostDelivery = data.OxytocinGiven;
                    birthOutcome.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    await _birthOutcomeRepository.SaveItemAsync(birthOutcome);
                }

                // Update UI
                PlacentaStatus = $"Delivered at {data.PlacentaDeliveryTime:HH:mm}";
                PlacentaStatusColor = "#4CAF50";
                BloodLoss = $"{data.EstimatedBloodLossMl} mL";
                BloodLossColor = GetBloodLossColor(data.EstimatedBloodLossMl);
                UterineStatus = data.UterusFirm ? "Firm & Contracted" : "Monitoring - Atony Risk";
                UterineStatusColor = data.UterusFirm ? "#4CAF50" : "#FF9800";
                IsPlacentaDelivered = true;
                ActiveManagementStatus = data.OxytocinGiven ? "Oxytocin given ✓" : "Not recorded";

                await AppShell.DisplayToastAsync("Placenta delivery recorded");

                // If PPH warning, show alert
                if (data.IsPPH)
                {
                    var urgency = data.IsSeverePPH ? "SEVERE PPH" : "PPH Warning";
                    await Application.Current?.MainPage?.DisplayAlert(
                        urgency,
                        $"Blood loss: {data.EstimatedBloodLossMl}mL. Continue monitoring and implement hemorrhage protocol if needed.",
                        "OK");
                }

                // Ask to transition to fourth stage
                var proceed = await Application.Current?.MainPage?.DisplayAlert(
                    "Transition to Fourth Stage",
                    "Placenta delivered. Ready to begin 2-hour postpartum monitoring?",
                    "Yes, Proceed",
                    "Stay Here");

                if (proceed == true)
                {
                    await TransitionToFourthStage();
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
                await AppShell.DisplayToastAsync("Failed to record placenta delivery");
            }
        }

        #endregion

        #region Baby Management Commands

        [RelayCommand]
        private async Task AddBaby()
        {
            if (Patient?.ID == null)
            {
                await AppShell.DisplayToastAsync("No patient selected");
                return;
            }

            try
            {
                // Check if birth outcome exists
                var birthOutcome = await _birthOutcomeRepository.GetByPartographIdAsync(Patient.ID);
                if (birthOutcome == null)
                {
                    var shouldCreate = await Application.Current.MainPage.DisplayAlert(
                        "Birth Outcome Required",
                        "A birth outcome record is needed to add baby details. Would you like to create one?",
                        "Yes",
                        "Cancel");

                    if (shouldCreate)
                    {
                        await NavigateToBirthOutcome();
                    }
                    return;
                }

                // Show action sheet for quick add vs full details
                var choice = await Application.Current.MainPage.DisplayActionSheet(
                    "Add Baby",
                    "Cancel",
                    null,
                    "Quick Entry (Essential Info)",
                    "Full Details Page");

                switch (choice)
                {
                    case "Quick Entry (Essential Info)":
                        await QuickAddBaby(birthOutcome);
                        break;
                    case "Full Details Page":
                        await NavigateToBabyDetails();
                        break;
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        private async Task QuickAddBaby(BirthOutcome birthOutcome)
        {
            try
            {
                // Get baby sex
                var sex = await Application.Current.MainPage.DisplayActionSheet(
                    "Baby Sex",
                    "Cancel",
                    null,
                    "Male", "Female", "Unknown");

                if (sex == "Cancel" || string.IsNullOrEmpty(sex))
                    return;

                // Get baby tag
                var babyNumber = Babies.Count + 1;
                var babyTag = babyNumber == 1 ? "Baby" : $"Baby {(char)('A' + babyNumber - 1)}";

                var tagResult = await Application.Current.MainPage.DisplayPromptAsync(
                    "Baby Tag",
                    "Enter baby identifier (e.g., Baby A, Twin 1)",
                    initialValue: babyTag);

                if (string.IsNullOrEmpty(tagResult))
                    return;

                // Create new baby
                var baby = new BabyDetails
                {
                    ID = Guid.NewGuid(),
                    PartographID = Patient.ID.Value,
                    BirthOutcomeID = birthOutcome.ID,
                    Sex = sex switch
                    {
                        "Male" => BabySex.Male,
                        "Female" => BabySex.Female,
                        _ => BabySex.Unknown
                    },
                    BirthTime = Patient.DeliveryTime ?? DateTime.Now,
                    BabyTag = tagResult,
                    BabyNumber = babyNumber,
                    VitalStatus = BabyVitalStatus.LiveBirth,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID,
                    CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                await _babyDetailsRepository.SaveItemAsync(baby);
                await LoadBabiesAsync();

                await AppShell.DisplayToastAsync($"{tagResult} added successfully");

                // Offer to record APGAR
                var recordApgar = await Application.Current.MainPage.DisplayAlert(
                    "Record APGAR?",
                    $"Would you like to record APGAR scores for {tagResult}?",
                    "Yes",
                    "Later");

                if (recordApgar)
                {
                    await RecordBabyApgar(baby);
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
                await AppShell.DisplayToastAsync("Failed to add baby");
            }
        }

        [RelayCommand]
        private async Task RecordBabyApgar(BabyDetails baby)
        {
            if (baby == null)
                return;

            try
            {
                // Determine which APGAR to record
                var apgarOptions = new List<string>();

                if (!baby.Apgar1Min.HasValue)
                    apgarOptions.Add("APGAR 1-Minute");
                else
                    apgarOptions.Add($"Update APGAR 1 (Current: {baby.Apgar1Min})");

                if (!baby.Apgar5Min.HasValue)
                    apgarOptions.Add("APGAR 5-Minute");
                else
                    apgarOptions.Add($"Update APGAR 5 (Current: {baby.Apgar5Min})");

                // Add APGAR 10 if APGAR 5 < 7
                if (baby.Apgar5Min.HasValue && baby.Apgar5Min.Value < 7)
                {
                    apgarOptions.Add("APGAR 10-Minute");
                }

                var choice = await Application.Current.MainPage.DisplayActionSheet(
                    $"Record APGAR - {baby.BabyTag}",
                    "Cancel",
                    null,
                    apgarOptions.ToArray());

                if (choice == "Cancel" || string.IsNullOrEmpty(choice))
                    return;

                // Determine which APGAR type
                int apgarType = 1;
                if (choice.Contains("5"))
                    apgarType = 5;
                else if (choice.Contains("10"))
                    apgarType = 10;

                // Record APGAR with component scoring
                await RecordApgarWithComponents(baby, apgarType);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        private async Task RecordApgarWithComponents(BabyDetails baby, int apgarType)
        {
            try
            {
                // For simplicity, use quick entry. In full implementation, open APGAR popup
                var scoreResult = await Application.Current.MainPage.DisplayPromptAsync(
                    $"APGAR {apgarType}-Minute Score",
                    "Enter total APGAR score (0-10):",
                    keyboard: Keyboard.Numeric);

                if (string.IsNullOrEmpty(scoreResult) || !int.TryParse(scoreResult, out int score))
                    return;

                if (score < 0 || score > 10)
                {
                    await AppShell.DisplayToastAsync("APGAR score must be between 0 and 10");
                    return;
                }

                // Update baby record
                switch (apgarType)
                {
                    case 1:
                        baby.Apgar1Min = score;
                        break;
                    case 5:
                        baby.Apgar5Min = score;
                        break;
                }

                baby.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                await _babyDetailsRepository.SaveItemAsync(baby);

                // Reload data
                await LoadBabiesAsync();
                await LoadApgarScores();

                var interpretation = score >= 7 ? "Normal" : score >= 4 ? "Moderately Abnormal" : "Severely Abnormal";
                await AppShell.DisplayToastAsync($"APGAR {apgarType}-min: {score} ({interpretation})");

                // If APGAR 5 < 7, remind about APGAR 10
                if (apgarType == 5 && score < 7)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "APGAR 10 Recommended",
                        $"APGAR 5-minute score is {score}. WHO recommends recording APGAR at 10 minutes.",
                        "OK");
                }
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

        [RelayCommand]
        private async Task UpdateBabyVitals(BabyDetails baby)
        {
            if (baby == null)
                return;

            await RecordBabyApgar(baby);
        }

        #endregion

        #region Navigation Commands

        [RelayCommand]
        private async Task TransitionToFourthStage()
        {
            try
            {
                if (Patient == null)
                    return;

                if (!IsPlacentaDelivered)
                {
                    var proceed = await Application.Current?.MainPage?.DisplayAlert(
                        "Placenta Not Recorded",
                        "Placenta delivery has not been recorded. Do you want to record it now?",
                        "Record Placenta",
                        "Skip");

                    if (proceed == true)
                    {
                        await RecordPlacentaDelivery();
                        return;
                    }
                }

                Patient.Status = LaborStatus.FourthStage;
                Patient.FourthStageStartTime = DateTime.Now;
                await _partographRepository.SaveItemAsync(Patient);

                // Update timer service
                _labourTimerService.UpdatePartograph(Patient);

                await AppShell.DisplayToastAsync("Transitioned to Fourth Stage - Begin 2-hour monitoring");
                await Shell.Current.GoToAsync($"fourthpartograph?patientId={Patient.ID}");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

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
                    { "NumberOfBabies", (Babies.Count + 1).ToString() }
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
