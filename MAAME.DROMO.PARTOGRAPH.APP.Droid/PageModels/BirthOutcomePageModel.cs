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
        private readonly ModalErrorHandler _errorHandler;
        private readonly ILogger<BirthOutcomePageModel> _logger;

        public BirthOutcomePageModel(
            BirthOutcomeRepository birthOutcomeRepository,
            BabyDetailsRepository babyDetailsRepository,
            PatientRepository patientRepository,
            PartographRepository partographRepository,
            ModalErrorHandler errorHandler,
            ILogger<BirthOutcomePageModel> logger)
        {
            _birthOutcomeRepository = birthOutcomeRepository;
            _babyDetailsRepository = babyDetailsRepository;
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
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
                //OnPropertyChanged(nameof(AbortionVisibility));
                if (_maternalStatus == MaternalOutcomeStatus.Survived)
                {
                    MaternalDeathTime = null;
                    MaternalDeathCause = string.Empty;
                    MaternalDeathCircumstances = string.Empty;
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

        [ObservableProperty]
        private string _deliveryModeDetails = string.Empty;

        [ObservableProperty]
        private DateTime _deliveryTime = DateTime.Now;

        [ObservableProperty]
        private int _numberOfBabies = 1;

        // Perineal Status
        [ObservableProperty]
        private PerinealStatus _perinealStatus = PerinealStatus.Intact;

        [ObservableProperty]
        private string _perinealDetails = string.Empty;

        // Placental Information
        [ObservableProperty]
        private DateTime? _placentaDeliveryTime;

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
        public List<DeliveryMode> DeliveryModeOptions => Enum.GetValues(typeof(DeliveryMode)).Cast<DeliveryMode>().ToList();
        public List<PerinealStatus> PerinealStatusOptions => Enum.GetValues(typeof(PerinealStatus)).Cast<PerinealStatus>().ToList();

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
                        DeliveryTime = Partograph.DeliveryTime.Value;
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

        private void LoadExistingData(BirthOutcome outcome)
        {
            MaternalStatus = outcome.MaternalStatus;
            MaternalDeathTime = outcome.MaternalDeathTime;
            MaternalDeathCause = outcome.MaternalDeathCause ?? string.Empty;
            MaternalDeathCircumstances = outcome.MaternalDeathCircumstances ?? string.Empty;
            DeliveryMode = outcome.DeliveryMode;
            DeliveryModeDetails = outcome.DeliveryModeDetails ?? string.Empty;
            DeliveryTime = outcome.DeliveryTime ?? DateTime.Now;
            NumberOfBabies = outcome.NumberOfBabies;
            PerinealStatus = outcome.PerinealStatus;
            PerinealDetails = outcome.PerinealDetails ?? string.Empty;
            PlacentaDeliveryTime = outcome.PlacentaDeliveryTime;
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
        private async Task SaveAsync()
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
                    ID = BirthOutcome?.ID ?? Guid.NewGuid(),
                    PartographID = Partograph.ID,
                    RecordedTime = DateTime.Now,
                    MaternalStatus = MaternalStatus,
                    MaternalDeathTime = MaternalDeathTime,
                    MaternalDeathCause = MaternalDeathCause,
                    MaternalDeathCircumstances = MaternalDeathCircumstances,
                    DeliveryMode = DeliveryMode,
                    DeliveryModeDetails = DeliveryModeDetails,
                    DeliveryTime = DeliveryTime,
                    NumberOfBabies = NumberOfBabies,
                    PerinealStatus = PerinealStatus,
                    PerinealDetails = PerinealDetails,
                    PlacentaDeliveryTime = PlacentaDeliveryTime,
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

                    // Navigate to baby details entry
                    if (NumberOfBabies > 0 && MaternalStatus != MaternalOutcomeStatus.Died)
                    {
                        var shouldContinue = await Application.Current.MainPage.DisplayAlert(
                            "Next Step",
                            $"Do you want to record details for {NumberOfBabies} {(NumberOfBabies > 1 ? "baby" : "babies")}?",
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

            if (NumberOfBabies < 1)
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
                { "BirthOutcomeId", BirthOutcome.ID },
                { "PartographId", Partograph.ID },
                { "NumberOfBabies", NumberOfBabies }
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
