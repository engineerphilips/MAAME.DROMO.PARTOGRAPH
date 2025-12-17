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
    /// Third Stage Partograph Page Model - Manages placenta delivery monitoring
    /// WHO Guidelines: Third stage should not exceed 30 minutes
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

        [ObservableProperty]
        private string _placentaStatus = "Awaiting Delivery";

        [ObservableProperty]
        private string _placentaStatusColor = "#FF9800";

        [ObservableProperty]
        private string _bloodLoss = "0 mL";

        [ObservableProperty]
        private string _uterineStatus = "Monitoring";

        // APGAR tracking
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

        // Timer display
        [ObservableProperty]
        private string _timerDisplay = "00:00";

        [ObservableProperty]
        private bool _isTimerWarning;

        [ObservableProperty]
        private bool _isTimerCritical;

        [ObservableProperty]
        private bool _isPlacentaDelivered;

        // Alert message
        [ObservableProperty]
        private string _alertMessage = string.Empty;

        [ObservableProperty]
        private bool _hasAlert;

        [ObservableProperty]
        private string _alertColor = "#FF9800";

        // Popup actions
        public Action? OpenPlacentaDeliveryPopup { get; set; }
        public Action? ClosePlacentaDeliveryPopup { get; set; }

        public PlacentaDeliveryPopupPageModel PlacentaDeliveryPopupPageModel => _placentaDeliveryPopupPageModel;

        public ThirdStagePartographPageModel(
            PatientRepository patientRepository,
            PartographRepository partographRepository,
            ModalErrorHandler errorHandler,
            BPRepository bpRepository,
            TemperatureRepository temperatureRepository,
            BirthOutcomeRepository birthOutcomeRepository,
            BabyDetailsRepository babyDetailsRepository,
            LabourTimerService labourTimerService,
            PlacentaDeliveryPopupPageModel placentaDeliveryPopupPageModel)
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

        private async Task LoadPatientData(Guid patientId)
        {
            try
            {
                IsBusy = true;
                Patient = await _partographRepository.GetCurrentPartographAsync(patientId);

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

                    // Load APGAR scores from baby details
                    await LoadApgarScores();

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

        private string GetApgarColor(int score)
        {
            if (score >= 7) return "#4CAF50"; // Green - Normal
            if (score >= 4) return "#FF9800"; // Orange - Moderate
            return "#F44336"; // Red - Severe
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
                        "Intact" => BirthOutcome.PerinealStatusType.Intact,
                        "1st Degree Tear" => BirthOutcome.PerinealStatusType.FirstDegreeTear,
                        "2nd Degree Tear" => BirthOutcome.PerinealStatusType.SecondDegreeTear,
                        "3rd Degree Tear" => BirthOutcome.PerinealStatusType.ThirdDegreeTear,
                        "4th Degree Tear" => BirthOutcome.PerinealStatusType.FourthDegreeTear,
                        "Episiotomy" => BirthOutcome.PerinealStatusType.Episiotomy,
                        _ => BirthOutcome.PerinealStatusType.Intact
                    };
                    birthOutcome.OxytocinGivenPostDelivery = data.OxytocinGiven;
                    birthOutcome.UpdatedAt = DateTime.UtcNow;

                    // Set complication if PPH detected
                    if (data.IsSeverePPH)
                    {
                        birthOutcome.MaternalComplication = BirthOutcome.MaternalComplicationType.PPH;
                    }

                    await _birthOutcomeRepository.SaveItemAsync(birthOutcome);
                }

                // Update UI
                PlacentaStatus = $"Delivered at {data.PlacentaDeliveryTime:HH:mm}";
                PlacentaStatusColor = "#4CAF50";
                BloodLoss = $"{data.EstimatedBloodLossMl} mL";
                UterineStatus = data.UterusFirm ? "Firm & Contracted" : "Monitoring - Atony Risk";
                IsPlacentaDelivered = true;

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
        private async Task RecordApgar1()
        {
            if (Patient?.ID == null)
                return;

            try
            {
                // Navigate to baby details to record APGAR
                var birthOutcome = await _birthOutcomeRepository.GetByPartographIdAsync(Patient.ID);
                if (birthOutcome == null)
                {
                    await AppShell.DisplayToastAsync("Please record birth outcome first");
                    return;
                }

                var parameters = new Dictionary<string, object>
                {
                    { "PartographId", Patient.ID.ToString() },
                    { "BirthOutcomeId", birthOutcome.ID.ToString() },
                    { "NumberOfBabies", "1" },
                    { "FocusApgar", "1" }
                };

                await Shell.Current.GoToAsync("BabyDetailsPage", parameters);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        [RelayCommand]
        private async Task RecordApgar5()
        {
            if (Patient?.ID == null)
                return;

            try
            {
                var birthOutcome = await _birthOutcomeRepository.GetByPartographIdAsync(Patient.ID);
                if (birthOutcome == null)
                {
                    await AppShell.DisplayToastAsync("Please record birth outcome first");
                    return;
                }

                var parameters = new Dictionary<string, object>
                {
                    { "PartographId", Patient.ID.ToString() },
                    { "BirthOutcomeId", birthOutcome.ID.ToString() },
                    { "NumberOfBabies", "1" },
                    { "FocusApgar", "5" }
                };

                await Shell.Current.GoToAsync("BabyDetailsPage", parameters);
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
