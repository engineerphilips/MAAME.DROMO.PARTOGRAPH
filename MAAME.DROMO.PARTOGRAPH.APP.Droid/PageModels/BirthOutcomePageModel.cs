using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public class ComplicationItem : ObservableObject
    {
        public string Name { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public partial class BirthOutcomePageModel : ObservableObject, IQueryAttributable
    {
        private readonly BirthOutcomeRepository _birthOutcomeRepository;
        private readonly BabyDetailsRepository _babyDetailsRepository;
        private readonly PatientRepository _patientRepository;
        private readonly PartographRepository _partographRepository;
        private readonly CervixDilatationRepository _cervixDilatationRepository;
        private readonly FHRRepository _fhrRepository;
        private readonly ContractionRepository _contractionRepository;
        private readonly HeadDescentRepository _headDescentRepository;
        private readonly ModalErrorHandler _errorHandler;
        private readonly ILogger<BirthOutcomePageModel> _logger;

        public BirthOutcomePageModel(
            BirthOutcomeRepository birthOutcomeRepository,
            BabyDetailsRepository babyDetailsRepository,
            PatientRepository patientRepository,
            PartographRepository partographRepository,
            CervixDilatationRepository cervixDilatationRepository,
            FHRRepository fhrRepository,
            ContractionRepository contractionRepository,
            HeadDescentRepository headDescentRepository,
            ModalErrorHandler errorHandler,
            ILogger<BirthOutcomePageModel> logger)
        {
            _birthOutcomeRepository = birthOutcomeRepository;
            _babyDetailsRepository = babyDetailsRepository;
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
            _cervixDilatationRepository = cervixDilatationRepository;
            _fhrRepository = fhrRepository;
            _contractionRepository = contractionRepository;
            _headDescentRepository = headDescentRepository;
            _errorHandler = errorHandler;
            _logger = logger;

            // Initialize complications collection
            Complications = new ObservableCollection<ComplicationItem>
            {
                new ComplicationItem { Name = "Postpartum Hemorrhage (PPH)" },
                new ComplicationItem { Name = "Eclampsia" },
                new ComplicationItem { Name = "Septic Shock" },
                new ComplicationItem { Name = "Obstructed Labor" },
                new ComplicationItem { Name = "Ruptured Uterus" }
            };
        }

        [ObservableProperty]
        private Partograph? _partograph;

        [ObservableProperty]
        private string _patientId = string.Empty;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private string _patientInfo = string.Empty;

        [ObservableProperty]
        private string _laborDuration = string.Empty;

        [ObservableProperty]
        private int _currentDilation;

        // Additional labor parameters for outcome summary
        [ObservableProperty]
        private int _latestFHR;

        [ObservableProperty]
        private string _latestFHRDisplay = string.Empty;

        [ObservableProperty]
        private int _latestContractions;

        [ObservableProperty]
        private string _latestContractionsDisplay = string.Empty;

        [ObservableProperty]
        private int _latestHeadDescent;

        [ObservableProperty]
        private string _latestHeadDescentDisplay = string.Empty;

        [ObservableProperty]
        private string _laborStage = string.Empty;

        [ObservableProperty]
        private string _secondStageDuration = string.Empty;

        [ObservableProperty]
        private bool _hasSecondStageDuration;

        //[ObservableProperty]
        //private Patient? _patient;

        [ObservableProperty]
        private BirthOutcome? _birthOutcome;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isEditMode;

        // Maternal Outcome
        //[ObservableProperty]
        private MaternalOutcomeStatus _maternalStatus = MaternalOutcomeStatus.Survived;

        public MaternalOutcomeStatus MaternalStatus
        {
            get => _maternalStatus;
            set
            {
                SetProperty(ref _maternalStatus, value);
                IsDeathFieldsVisible = _maternalStatus == MaternalOutcomeStatus.Died;

                // Update delivery mode options based on maternal status
                UpdateDeliveryModeOptions();

                if (_maternalStatus == MaternalOutcomeStatus.Survived)
                {
                    MaternalDeathTime = null;
                    MaternalDeathCause = string.Empty;
                    MaternalDeathCircumstances = string.Empty;

                    // Reset delivery mode if it was set to NoDelivery
                    if (DeliveryMode == DeliveryMode.NoDelivery)
                    {
                        DeliveryMode = DeliveryMode.SpontaneousVaginal;
                    }
                }
            }
        }

        [ObservableProperty]
        private bool _isDeathFieldsVisible;

        [ObservableProperty]
        private DateTime? _maternalDeathTime;

        [ObservableProperty]
        private string _maternalDeathCause = string.Empty;

        [ObservableProperty]
        private string _maternalDeathCircumstances = string.Empty;

        // Delivery Information
        [ObservableProperty]
        private DeliveryMode _deliveryMode = DeliveryMode.SpontaneousVaginal;

        /// <summary>
        /// Called when DeliveryMode changes. Auto-sets NumberOfBabies to 0 when NoDelivery is selected.
        /// </summary>
        partial void OnDeliveryModeChanged(DeliveryMode value)
        {
            if (value == DeliveryMode.NoDelivery)
            {
                NumberOfBabies = 0;
            }
            else if (NumberOfBabies == 0)
            {
                // Reset to 1 if changing from NoDelivery to another mode
                NumberOfBabies = 1;
            }
        }

        [ObservableProperty]
        private string _deliveryModeDetails = string.Empty;

        //[ObservableProperty]
        //private DateTime _deliveryTime = DateTime.Now;

        [ObservableProperty]
        private DateOnly _deliveryDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _deliveryTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        [ObservableProperty]
        private int _numberOfBabies = 1;

        // Perineal Status
        [ObservableProperty]
        private PerinealStatus _perinealStatus = PerinealStatus.Intact;

        [ObservableProperty]
        private string _perinealDetails = string.Empty;

        // Placental Information

        [ObservableProperty]
        private DateOnly _placentaDeliveryDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _placentaDeliveryTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        [ObservableProperty]
        private bool _placentaComplete = true;

        [ObservableProperty]
        private int _estimatedBloodLoss;

        // Maternal Complications
        [ObservableProperty]
        private string _maternalComplications = string.Empty;

        [ObservableProperty]
        private bool _postpartumHemorrhage;

        [ObservableProperty]
        private bool _eclampsia;

        [ObservableProperty]
        private bool _septicShock;

        [ObservableProperty]
        private bool _obstructedLabor;

        [ObservableProperty]
        private bool _rupturedUterus;

        public ObservableCollection<ComplicationItem> Complications { get; set; }

        // Post-delivery Care
        [ObservableProperty]
        private bool _oxytocinGiven = true;

        [ObservableProperty]
        private bool _antibioticsGiven;

        [ObservableProperty]
        private bool _bloodTransfusionGiven;

        [ObservableProperty]
        private string _notes = string.Empty;

        // Lists for pickers
        public List<MaternalOutcomeStatus> MaternalOutcomeOptions => Enum.GetValues(typeof(MaternalOutcomeStatus)).Cast<MaternalOutcomeStatus>().ToList();

        [ObservableProperty]
        private List<DeliveryMode> _deliveryModeOptions = Enum.GetValues(typeof(DeliveryMode))
            .Cast<DeliveryMode>()
            .Where(d => d != DeliveryMode.NoDelivery) // Exclude NoDelivery by default
            .ToList();

        public List<PerinealStatus> PerinealStatusOptions => Enum.GetValues(typeof(PerinealStatus)).Cast<PerinealStatus>().ToList();

        /// <summary>
        /// Updates the delivery mode options based on maternal status.
        /// Shows "No Delivery" option only when maternal status is "Died".
        /// </summary>
        private void UpdateDeliveryModeOptions()
        {
            if (MaternalStatus == MaternalOutcomeStatus.Died)
            {
                // Include NoDelivery option when mother died
                DeliveryModeOptions = Enum.GetValues(typeof(DeliveryMode))
                    .Cast<DeliveryMode>()
                    .ToList();
            }
            else
            {
                // Exclude NoDelivery option for normal cases
                DeliveryModeOptions = Enum.GetValues(typeof(DeliveryMode))
                    .Cast<DeliveryMode>()
                    .Where(d => d != DeliveryMode.NoDelivery)
                    .ToList();
            }
        }

        public async Task LoadPartographAsync(Guid? partographId)
        {
            try
            {
                IsBusy = true;

                // Load partograph
                Partograph = await _partographRepository.GetAsync(partographId);
                if (Partograph == null)
                {
                    await AppShell.DisplayToastAsync("Failed to load partograph");
                    return;
                }

                //// Load patient
                //Patient = await _patientRepository.GetItemAsync(Partograph.PatientID);

                PatientName = Partograph.Name;
                PatientInfo = Partograph.DisplayInfo;

                // Calculate labor duration
                if (Partograph.LaborStartTime.HasValue)
                {
                    var duration = DateTime.Now - Partograph.LaborStartTime.Value;
                    LaborDuration = $"{(int)duration.TotalHours}h {duration.Minutes}m";
                }

                // Load current labor parameters
                await LoadLaborParametersAsync(partographId);

                // Check if birth outcome already exists
                var existingOutcome = await _birthOutcomeRepository.GetByPartographIdAsync(partographId);
                if (existingOutcome != null)
                {
                    BirthOutcome = existingOutcome;
                    LoadExistingData(existingOutcome);
                    IsEditMode = true;
                }
                else
                {
                    IsEditMode = false;
                    // Pre-fill delivery time from partograph
                    if (Partograph.DeliveryTime.HasValue)
                    {
                        DeliveryDate = DateOnly.FromDateTime(Partograph.DeliveryTime.Value);
                        DeliveryTime = Partograph.DeliveryTime.Value.TimeOfDay;
                         //?? DateTime.Now.TimeOfDay
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading partograph");
                await AppShell.DisplayToastAsync($"Error loading data: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Loads the latest labor parameters (dilation, FHR, contractions, head descent) for display on the outcome page
        /// </summary>
        private async Task LoadLaborParametersAsync(Guid? partographId)
        {
            try
            {
                // Load latest cervical dilation
                var latestDilation = await _cervixDilatationRepository.GetLatestByPatientAsync(partographId);
                if (latestDilation != null)
                {
                    CurrentDilation = latestDilation.DilatationCm;
                }
                else
                {
                    CurrentDilation = 10; // Default to 10 if on outcome page (likely fully dilated)
                }

                // Load latest FHR
                var latestFhr = await _fhrRepository.GetLatestByPatientAsync(partographId);
                if (latestFhr != null)
                {
                    LatestFHR = latestFhr.Rate ?? latestFhr.BaselineRate ?? 0;
                    LatestFHRDisplay = LatestFHR > 0 ? $"{LatestFHR} bpm" : "Not recorded";
                }
                else
                {
                    LatestFHRDisplay = "Not recorded";
                }

                // Load latest contractions
                var latestContraction = await _contractionRepository.GetLatestByPatientAsync(partographId);
                if (latestContraction != null)
                {
                    LatestContractions = latestContraction.FrequencyPer10Min;
                    LatestContractionsDisplay = $"{latestContraction.FrequencyPer10Min} in 10 min";
                }
                else
                {
                    LatestContractionsDisplay = "Not recorded";
                }

                // Load latest head descent
                var latestHeadDescent = await _headDescentRepository.GetLatestByPatientAsync(partographId);
                if (latestHeadDescent != null)
                {
                    LatestHeadDescent = latestHeadDescent.Station;
                    LatestHeadDescentDisplay = !string.IsNullOrEmpty(latestHeadDescent.PalpableAbdominally)
                        ? latestHeadDescent.PalpableAbdominally
                        : $"Station {latestHeadDescent.Station}";
                }
                else
                {
                    LatestHeadDescentDisplay = "Not recorded";
                }

                // Set labor stage display
                if (Partograph != null)
                {
                    LaborStage = Partograph.Status.ToString();

                    // Calculate second stage duration if applicable
                    if (Partograph.SecondStageStartTime.HasValue)
                    {
                        var endTime = Partograph.DeliveryTime ?? DateTime.Now;
                        var secondStageDuration = endTime - Partograph.SecondStageStartTime.Value;
                        SecondStageDuration = $"{(int)secondStageDuration.TotalMinutes} min";
                        HasSecondStageDuration = true;
                    }
                    else
                    {
                        HasSecondStageDuration = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading labor parameters");
                // Continue without failing - these are supplementary data
            }
        }

        private void LoadExistingData(BirthOutcome outcome)
        {
            MaternalStatus = outcome.MaternalStatus;
            MaternalDeathTime = outcome.MaternalDeathTime;
            MaternalDeathCause = outcome.MaternalDeathCause ?? string.Empty;
            MaternalDeathCircumstances = outcome.MaternalDeathCircumstances ?? string.Empty;
            DeliveryMode = outcome.DeliveryMode;
            DeliveryModeDetails = outcome.DeliveryModeDetails ?? string.Empty;
            DeliveryDate = outcome.DeliveryTime.HasValue ? DateOnly.FromDateTime(outcome.DeliveryTime.Value) : DateOnly.FromDateTime(DateTime.Now);
            DeliveryTime = (outcome.DeliveryTime ?? DateTime.Now).TimeOfDay;
            NumberOfBabies = outcome.NumberOfBabies;
            PerinealStatus = outcome.PerinealStatus;
            PerinealDetails = outcome.PerinealDetails ?? string.Empty;
            PlacentaDeliveryDate = outcome.PlacentaDeliveryTime.HasValue ? DateOnly.FromDateTime(outcome.PlacentaDeliveryTime.Value.Date) : DateOnly.FromDateTime(DateTime.Now);
            PlacentaDeliveryTime = outcome.PlacentaDeliveryTime.HasValue ? outcome.PlacentaDeliveryTime.Value.TimeOfDay : TimeSpan.Zero;
            PlacentaComplete = outcome.PlacentaComplete;
            EstimatedBloodLoss = outcome.EstimatedBloodLoss;
            MaternalComplications = outcome.MaternalComplications ?? string.Empty;
            PostpartumHemorrhage = outcome.PostpartumHemorrhage;
            Eclampsia = outcome.Eclampsia;
            SepticShock = outcome.SepticShock;
            ObstructedLabor = outcome.ObstructedLabor;
            RupturedUterus = outcome.RupturedUterus;
            OxytocinGiven = outcome.OxytocinGiven;
            AntibioticsGiven = outcome.AntibioticsGiven;
            BloodTransfusionGiven = outcome.BloodTransfusionGiven;
            Notes = outcome.Notes ?? string.Empty;

            // Sync complications to chip collection
            Complications[0].IsSelected = outcome.PostpartumHemorrhage;
            Complications[1].IsSelected = outcome.Eclampsia;
            Complications[2].IsSelected = outcome.SepticShock;
            Complications[3].IsSelected = outcome.ObstructedLabor;
            Complications[4].IsSelected = outcome.RupturedUterus;
        }

        [RelayCommand]
        private async Task Save()
        {
            if (Partograph == null)
            {
                await AppShell.DisplayToastAsync("Partograph not loaded");
                return;
            }

            if (!ValidateInput())
            {
                return;
            }

            try
            {
                IsBusy = true;

                // Sync complications from chips to properties
                PostpartumHemorrhage = Complications[0].IsSelected;
                Eclampsia = Complications[1].IsSelected;
                SepticShock = Complications[2].IsSelected;
                ObstructedLabor = Complications[3].IsSelected;
                RupturedUterus = Complications[4].IsSelected;

                var outcome = new BirthOutcome
                {
                    ID = BirthOutcome?.ID,
                    PartographID = Partograph.ID,
                    RecordedTime = DateTime.Now,
                    MaternalStatus = MaternalStatus,
                    MaternalDeathTime = MaternalDeathTime,
                    MaternalDeathCause = MaternalDeathCause,
                    MaternalDeathCircumstances = MaternalDeathCircumstances,
                    DeliveryMode = DeliveryMode,
                    DeliveryModeDetails = DeliveryModeDetails,
                    DeliveryTime = new DateTime(DeliveryDate.Year, DeliveryDate.Month, DeliveryDate.Day).Add(DeliveryTime),
                    NumberOfBabies = NumberOfBabies,
                    PerinealStatus = PerinealStatus,
                    PerinealDetails = PerinealDetails,
                    PlacentaDeliveryTime = PlacentaDeliveryDate != null && PlacentaDeliveryTime != null ? new DateTime(PlacentaDeliveryDate.Year, PlacentaDeliveryDate.Month, PlacentaDeliveryDate.Day).Add(PlacentaDeliveryTime) : null,
                    PlacentaComplete = PlacentaComplete,
                    EstimatedBloodLoss = EstimatedBloodLoss,
                    MaternalComplications = MaternalComplications,
                    PostpartumHemorrhage = PostpartumHemorrhage,
                    Eclampsia = Eclampsia,
                    SepticShock = SepticShock,
                    ObstructedLabor = ObstructedLabor,
                    RupturedUterus = RupturedUterus,
                    OxytocinGiven = OxytocinGiven,
                    AntibioticsGiven = AntibioticsGiven,
                    BloodTransfusionGiven = BloodTransfusionGiven,
                    HandlerName = Constants.Staff?.Name ?? string.Empty,
                    Handler = Constants.Staff?.ID,
                    Notes = Notes,
                    DeviceId = DeviceIdentity.GetOrCreateDeviceId(),
                    OriginDeviceId = DeviceIdentity.GetOrCreateDeviceId()
                };

                var result = await _birthOutcomeRepository.SaveItemAsync(outcome);
                if (result != null)
                {
                    await AppShell.DisplayToastAsync("Birth outcome saved successfully");
                    BirthOutcome = outcome;
                    IsEditMode = true;

                    // Update partograph with delivery time and progress to appropriate stage
                    if (Partograph != null)
                    {
                        // Set the delivery time (baby delivery time)
                        Partograph.DeliveryTime = new DateTime(DeliveryDate.Year, DeliveryDate.Month, DeliveryDate.Day).Add(DeliveryTime);

                        // If we're in SecondStage and baby has been delivered, transition to ThirdStage
                        if (Partograph.Status == LaborStatus.SecondStage)
                        {
                            Partograph.Status = LaborStatus.ThirdStage;
                            Partograph.ThirdStageStartTime = Partograph.DeliveryTime;
                            _logger.LogInformation($"Updated partograph {Partograph.ID} status to ThirdStage (baby delivered)");
                        }
                        // If we're already in ThirdStage or later, just update the delivery time
                        else if (Partograph.Status == LaborStatus.FirstStage)
                        {
                            // If still in FirstStage, move to SecondStage first, then to ThirdStage
                            // This handles the case where birth outcome is recorded before proper stage transitions
                            Partograph.Status = LaborStatus.ThirdStage;
                            Partograph.SecondStageStartTime ??= Partograph.DeliveryTime;
                            Partograph.ThirdStageStartTime = Partograph.DeliveryTime;
                            _logger.LogInformation($"Updated partograph {Partograph.ID} status to ThirdStage (from FirstStage - baby delivered)");
                        }

                        await _partographRepository.SaveItemAsync(Partograph);
                    }

                    // Navigate to baby details entry
                    if (NumberOfBabies > 0 && MaternalStatus != MaternalOutcomeStatus.Died)
                    {
                        var shouldContinue = await Application.Current.MainPage.DisplayAlert(
                            "Next Step",
                            $"Do you want to record details for {NumberOfBabies} {(NumberOfBabies > 1 ? "babies" : "baby")}",
                            "Yes", "Later");

                        if (shouldContinue)
                        {
                            await NavigateToBabyDetailsAsync();
                        }
                    }
                }
                else
                {
                    await AppShell.DisplayToastAsync("Failed to save birth outcome");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving birth outcome");
                await AppShell.DisplayToastAsync($"Error saving: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool ValidateInput()
        {
            if (MaternalStatus == MaternalOutcomeStatus.Died && MaternalDeathTime == null)
            {
                Application.Current.MainPage.DisplayAlert("Validation Error", "Please specify time of maternal death", "OK");
                return false;
            }

            // Validate NoDelivery is only used when mother died
            if (DeliveryMode == DeliveryMode.NoDelivery && MaternalStatus != MaternalOutcomeStatus.Died)
            {
                Application.Current.MainPage.DisplayAlert("Validation Error", "No Delivery can only be selected when maternal status is Died", "OK");
                return false;
            }

            // Allow 0 babies only when NoDelivery is selected
            if (DeliveryMode == DeliveryMode.NoDelivery)
            {
                if (NumberOfBabies < 0)
                {
                    Application.Current.MainPage.DisplayAlert("Validation Error", "Number of babies cannot be negative", "OK");
                    return false;
                }
            }
            else if (NumberOfBabies < 1)
            {
                Application.Current.MainPage.DisplayAlert("Validation Error", "Number of babies must be at least 1", "OK");
                return false;
            }

            if (NumberOfBabies > 5)
            {
                Application.Current.MainPage.DisplayAlert("Validation Error", "Number of babies cannot exceed 5", "OK");
                return false;
            }

            if (EstimatedBloodLoss < 0 || EstimatedBloodLoss > 5000)
            {
                Application.Current.MainPage.DisplayAlert("Validation Error", "Estimated blood loss must be between 0 and 5000ml", "OK");
                return false;
            }

            return true;
        }

        [RelayCommand]
        private async Task NavigateToBabyDetailsAsync()
        {
            if (BirthOutcome?.ID == null)
            {
                await AppShell.DisplayToastAsync("Please save birth outcome first");
                return;
            }

            var parameters = new Dictionary<string, object>
            {
                { "BirthOutcomeId", BirthOutcome.ID.ToString() },
                { "PartographId", Partograph.ID.ToString() },
                { "AddNewBaby", "true" } // Signal that we're adding a new baby
            };

            await Shell.Current.GoToAsync("BabyDetailsPage", parameters);
        }

        [RelayCommand]
        private void OnMaternalStatusChanged()
        {
            // Auto-clear death fields if mother survived
            if (MaternalStatus == MaternalOutcomeStatus.Survived)
            {
                MaternalDeathTime = null;
                MaternalDeathCause = string.Empty;
                MaternalDeathCircumstances = string.Empty;
            }
        }

        [RelayCommand]
        private void OnNumberOfBabiesChanged()
        {
            // Ensure number is within valid range
            if (NumberOfBabies < 1) NumberOfBabies = 1;
            if (NumberOfBabies > 5) NumberOfBabies = 5;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("PartographId"))
            {
                Guid id = Guid.Parse(Convert.ToString(query["PartographId"]));
                LoadPartographAsync(id).FireAndForgetSafeAsync(_errorHandler);
                //Refresh().FireAndForgetSafeAsync(_errorHandler);
            }
        }
    }
}
