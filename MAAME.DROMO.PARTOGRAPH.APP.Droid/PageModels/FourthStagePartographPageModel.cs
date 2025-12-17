using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    /// <summary>
    /// Fourth Stage Partograph Page Model - Manages immediate postpartum monitoring
    /// WHO Guidelines: 2-hour monitoring period with vitals every 15 minutes
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
        private readonly LabourTimerService _labourTimerService;
        private readonly CompletionChecklistPopupPageModel _completionChecklistPopupPageModel;

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

        // Monitoring status
        [ObservableProperty]
        private string _fundalHeight = "At umbilicus";

        [ObservableProperty]
        private string _bleedingStatus = "Normal lochia";

        [ObservableProperty]
        private string _bladderStatus = "Empty";

        [ObservableProperty]
        private string _uterineStatus = "Firm";

        // Vital signs tracking
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

        // Timer display
        [ObservableProperty]
        private string _timerDisplay = "00:00";

        [ObservableProperty]
        private double _progressValue = 0;

        [ObservableProperty]
        private bool _isMonitoringComplete;

        [ObservableProperty]
        private bool _canComplete;

        // Alert display
        [ObservableProperty]
        private string _alertMessage = string.Empty;

        [ObservableProperty]
        private bool _hasAlert;

        [ObservableProperty]
        private string _alertColor = "#2196F3";

        // Latest vitals display
        [ObservableProperty]
        private string _latestBP = "--/--";

        [ObservableProperty]
        private string _latestPulse = "--";

        [ObservableProperty]
        private string _latestTemp = "--";

        // Popup actions
        public Action? OpenCompletionChecklistPopup { get; set; }
        public Action? CloseCompletionChecklistPopup { get; set; }

        public CompletionChecklistPopupPageModel CompletionChecklistPopupPageModel => _completionChecklistPopupPageModel;

        public FourthStagePartographPageModel(
            PatientRepository patientRepository,
            PartographRepository partographRepository,
            ModalErrorHandler errorHandler,
            BPRepository bpRepository,
            TemperatureRepository temperatureRepository,
            BirthOutcomeRepository birthOutcomeRepository,
            BabyDetailsRepository babyDetailsRepository,
            LabourTimerService labourTimerService,
            CompletionChecklistPopupPageModel completionChecklistPopupPageModel)
        {
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
            _errorHandler = errorHandler;
            _bpRepository = bpRepository;
            _temperatureRepository = temperatureRepository;
            _birthOutcomeRepository = birthOutcomeRepository;
            _babyDetailsRepository = babyDetailsRepository;
            _labourTimerService = labourTimerService;
            _completionChecklistPopupPageModel = completionChecklistPopupPageModel;

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

                    // Load vital signs count
                    await LoadVitalSignsData();

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
                    LatestBP = $"{latestBP.Systolic}/{latestBP.Diastolic}";
                    LatestPulse = latestBP.Pulse.ToString() ?? "--";
                    LastVitalSignsTime = latestBP.Time.ToString("HH:mm");

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

                // Get latest temperature
                var temps = await _temperatureRepository.ListByPatientAsync(Patient.ID);
                var latestTemp = temps
                    .Where(t => t.Time >= fourthStageStart)
                    .OrderByDescending(t => t.Time)
                    .FirstOrDefault();

                if (latestTemp != null)
                {
                    LatestTemp = $"{latestTemp.TemperatureCelsius:F1}°C";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading vital signs: {ex.Message}");
            }
        }

        private void UpdateCompletionStatus()
        {
            // Can complete if: 2 hours elapsed AND enough vital signs recorded
            CanComplete = IsMonitoringComplete && VitalSignsRecorded >= 6; // Allow completion with 6+ recordings

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

        [RelayCommand]
        private async Task RecordVitalSigns()
        {
            try
            {
                if (Patient?.ID == null)
                {
                    await AppShell.DisplayToastAsync("No patient loaded");
                    return;
                }

                // Quick input for BP
                string bpResult = await Application.Current?.MainPage?.DisplayPromptAsync(
                    "Blood Pressure",
                    "Enter systolic/diastolic (e.g., 120/80):",
                    placeholder: "120/80",
                    keyboard: Keyboard.Text);

                if (string.IsNullOrEmpty(bpResult))
                    return;

                // Parse BP
                var parts = bpResult.Split('/');
                if (parts.Length != 2 ||
                    !int.TryParse(parts[0].Trim(), out int systolic) ||
                    !int.TryParse(parts[1].Trim(), out int diastolic))
                {
                    await AppShell.DisplayToastAsync("Invalid BP format. Use systolic/diastolic");
                    return;
                }

                // Get pulse
                string pulseResult = await Application.Current?.MainPage?.DisplayPromptAsync(
                    "Pulse",
                    "Enter pulse rate (bpm):",
                    placeholder: "80",
                    keyboard: Keyboard.Numeric);

                int? pulse = null;
                if (!string.IsNullOrEmpty(pulseResult) && int.TryParse(pulseResult, out int parsedPulse))
                {
                    pulse = parsedPulse;
                }

                // Save BP reading
                var bp = new BP
                {
                    ID = Guid.NewGuid(),
                    PartographID = Patient.ID.Value,
                    Systolic = systolic,
                    Diastolic = diastolic,
                    Pulse = pulse ?? 0,
                    Time = DateTime.Now,
                    CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                await _bpRepository.SaveItemAsync(bp);

                // Increment counter and refresh
                VitalSignsRecorded++;
                VitalSignsProgress = $"{VitalSignsRecorded}/{RequiredVitalSigns}";
                VitalSignsProgressValue = Math.Min((double)VitalSignsRecorded / RequiredVitalSigns, 1.0);
                LatestBP = $"{systolic}/{diastolic}";
                LatestPulse = pulse?.ToString() ?? "--";
                LastVitalSignsTime = DateTime.Now.ToString("HH:mm");

                // Calculate next due
                var nextDue = DateTime.Now.AddMinutes(15);
                NextVitalSignsDue = nextDue.ToString("HH:mm");
                IsVitalSignsDue = false;

                UpdateCompletionStatus();

                await AppShell.DisplayToastAsync($"Vital signs recorded ({VitalSignsRecorded}/{RequiredVitalSigns})");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
                await AppShell.DisplayToastAsync("Failed to record vital signs");
            }
        }

        [RelayCommand]
        private async Task UpdateFundalHeight()
        {
            var options = new[] { "At umbilicus", "1 finger below", "2 fingers below", "3 fingers below", "Above umbilicus (concern)" };
            var result = await Application.Current?.MainPage?.DisplayActionSheet(
                "Fundal Height", "Cancel", null, options);

            if (!string.IsNullOrEmpty(result) && result != "Cancel")
            {
                FundalHeight = result;
                await AppShell.DisplayToastAsync("Fundal height updated");
            }
        }

        [RelayCommand]
        private async Task UpdateBleedingStatus()
        {
            var options = new[] { "Normal lochia (rubra)", "Minimal", "Moderate", "Heavy (PPH concern)", "Clots present" };
            var result = await Application.Current?.MainPage?.DisplayActionSheet(
                "Bleeding Status", "Cancel", null, options);

            if (!string.IsNullOrEmpty(result) && result != "Cancel")
            {
                BleedingStatus = result;

                if (result.Contains("Heavy") || result.Contains("PPH"))
                {
                    await Application.Current?.MainPage?.DisplayAlert(
                        "PPH Warning",
                        "Heavy bleeding detected. Assess uterine tone and implement hemorrhage protocol if needed.",
                        "OK");
                }

                await AppShell.DisplayToastAsync("Bleeding status updated");
            }
        }

        [RelayCommand]
        private async Task UpdateBladderStatus()
        {
            var options = new[] { "Empty", "Voided spontaneously", "Palpable bladder", "Catheterized" };
            var result = await Application.Current?.MainPage?.DisplayActionSheet(
                "Bladder Status", "Cancel", null, options);

            if (!string.IsNullOrEmpty(result) && result != "Cancel")
            {
                BladderStatus = result;
                await AppShell.DisplayToastAsync("Bladder status updated");
            }
        }

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
    }
}
