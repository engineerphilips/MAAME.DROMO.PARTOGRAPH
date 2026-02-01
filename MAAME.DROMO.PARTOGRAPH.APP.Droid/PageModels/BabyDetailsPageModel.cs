using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    //[QueryProperty(nameof(BirthOutcomeId), "BirthOutcomeId")]
    //[QueryProperty(nameof(PartographId), "PartographId")]
    //[QueryProperty(nameof(NumberOfBabies), "NumberOfBabies")]
    public partial class BabyDetailsPageModel : ObservableObject, IQueryAttributable
    {
        private readonly BabyDetailsRepository _babyDetailsRepository;
        private readonly BirthOutcomeRepository _birthOutcomeRepository;
        private readonly PartographRepository _partographRepository;
        private readonly ModalErrorHandler _errorHandler;
        private readonly ILogger _logger;

        public BabyDetailsPageModel(
            BabyDetailsRepository babyDetailsRepository,
            BirthOutcomeRepository birthOutcomeRepository,
            PartographRepository partographRepository,
            ModalErrorHandler errorHandler,
            ILogger<BabyDetailsPageModel> logger)
        {
            _babyDetailsRepository = babyDetailsRepository;
            _birthOutcomeRepository = birthOutcomeRepository;
            _partographRepository = partographRepository;
            _errorHandler = errorHandler;
            _logger = logger;
        }

        [ObservableProperty]
        private Guid? _birthOutcomeId;

        [ObservableProperty]
        private Guid? _partographId;

        [ObservableProperty]
        private int _numberOfBabies = 1;

        [ObservableProperty]
        private int _currentBabyIndex = 0;

        [ObservableProperty]
        private Partograph? _partograph;

        [ObservableProperty]
        private BirthOutcome? _birthOutcome;

        [ObservableProperty]
        private ObservableCollection<BabyDetails> _babies = new();

        [ObservableProperty]
        private BabyDetails? _currentBaby;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isMultipleBirth;

        [ObservableProperty]
        private bool _isAddingNewBaby;

        // Baby Identification
        [ObservableProperty]
        private int _babyNumber = 1;

        [ObservableProperty]
        private string _babyTag = string.Empty;

        //[ObservableProperty]
        //private DateTime _birthTime = DateTime.Now;

        [ObservableProperty]
        private DateOnly _birthDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private TimeSpan _birthTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);


        [ObservableProperty]
        private BabySex _sex = BabySex.Unknown;

        // Vital Status
        //[ObservableProperty]
        private BabyVitalStatus _vitalStatus = BabyVitalStatus.LiveBirth;

        public BabyVitalStatus VitalStatus
        {
            get => _vitalStatus;
            set
            {
                SetProperty(ref _vitalStatus, value);
                ShowDeathFields = _vitalStatus != BabyVitalStatus.LiveBirth && _vitalStatus != BabyVitalStatus.Survived;
                //OnPropertyChanged(nameof(AbortionVisibility));
                if (_vitalStatus != BabyVitalStatus.LiveBirth || _vitalStatus != BabyVitalStatus.Survived)
                {
                    DeathTime = null;
                    DeathCause = string.Empty;
                    //MaternalDeathCircumstances = string.Empty;
                }
            }
        }

        [ObservableProperty]
        private DateTime? _deathTime;

        [ObservableProperty]
        private string _deathCause = string.Empty;

        [ObservableProperty]
        private bool _stillbirthMacerated;

        // Anthropometric Measurements
        [ObservableProperty]
        private decimal _birthWeight;

        [ObservableProperty]
        private decimal _length;

        [ObservableProperty]
        private decimal _headCircumference;

        [ObservableProperty]
        private decimal _chestCircumference;

        // APGAR Scores
        [ObservableProperty]
        private int? _apgar1Min;

        [ObservableProperty]
        private int? _apgar5Min;

        //[ObservableProperty]
        //private int? _apgar10Min;

        // APGAR 1-Minute Component Scores
        [ObservableProperty]
        private int? _apgar1HeartRate;

        [ObservableProperty]
        private int? _apgar1RespiratoryEffort;

        [ObservableProperty]
        private int? _apgar1MuscleTone;

        [ObservableProperty]
        private int? _apgar1ReflexIrritability;

        [ObservableProperty]
        private int? _apgar1Color;

        // APGAR 5-Minute Component Scores
        [ObservableProperty]
        private int? _apgar5HeartRate;

        [ObservableProperty]
        private int? _apgar5RespiratoryEffort;

        [ObservableProperty]
        private int? _apgar5MuscleTone;

        [ObservableProperty]
        private int? _apgar5ReflexIrritability;

        [ObservableProperty]
        private int? _apgar5Color;

        // APGAR Display Properties
        [ObservableProperty]
        private string _apgar1MinDisplay = "—";

        [ObservableProperty]
        private string _apgar5MinDisplay = "—";

        [ObservableProperty]
        private string _apgar1StatusText = "Tap to score";

        [ObservableProperty]
        private string _apgar5StatusText = "Tap to score";

        [ObservableProperty]
        private Color _apgar1StatusColor = Colors.Gray;

        [ObservableProperty]
        private Color _apgar5StatusColor = Colors.Gray;

        [ObservableProperty]
        private Color _apgar1ButtonColor = Color.FromArgb("#F5F5F5");

        [ObservableProperty]
        private Color _apgar5ButtonColor = Color.FromArgb("#F5F5F5");

        [ObservableProperty]
        private Color _apgar1BorderColor = Color.FromArgb("#E0E0E0");

        [ObservableProperty]
        private Color _apgar5BorderColor = Color.FromArgb("#E0E0E0");

        // Resuscitation
        [ObservableProperty]
        private bool _resuscitationRequired;

        [ObservableProperty]
        private string _resuscitationSteps = string.Empty;

        [ObservableProperty]
        private int? _resuscitationDuration;

        [ObservableProperty]
        private bool _oxygenGiven;

        [ObservableProperty]
        private bool _intubationPerformed;

        [ObservableProperty]
        private bool _chestCompressionsGiven;

        [ObservableProperty]
        private bool _medicationsGiven;

        [ObservableProperty]
        private string _medicationDetails = string.Empty;

        // Immediate Newborn Care (WHO 2020)
        //[ObservableProperty]
        //private bool _skinToSkinContact = true;

        [ObservableProperty]
        private bool _earlyBreastfeedingInitiated = true;

        [ObservableProperty]
        private bool _delayedCordClamping = true;

        [ObservableProperty]
        private int? _cordClampingTime;

        [ObservableProperty]
        private bool _vitaminKGiven = true;

        [ObservableProperty]
        private bool _eyeProphylaxisGiven = true;

        [ObservableProperty]
        private bool _hepatitisBVaccineGiven;

        [ObservableProperty]
        private decimal? _firstTemperature;

        [ObservableProperty]
        private bool _kangarooMotherCare;

        // Congenital Issues
        [ObservableProperty]
        private bool _congenitalAbnormalitiesPresent;

        [ObservableProperty]
        private string _congenitalAbnormalitiesDescription = string.Empty;

        [ObservableProperty]
        private bool _birthInjuriesPresent;

        [ObservableProperty]
        private string _birthInjuriesDescription = string.Empty;

        //// Clinical Status
        //[ObservableProperty]
        //private bool _breathing = true;

        //[ObservableProperty]
        //private bool _crying = true;

        //[ObservableProperty]
        //private bool _goodMuscleTone = true;

        //[ObservableProperty]
        //private SkinColor _skinColor = SkinColor.Pink;

        // Special Care
        [ObservableProperty]
        private bool _requiresSpecialCare;

        [ObservableProperty]
        private string _specialCareReason = string.Empty;

        [ObservableProperty]
        private bool _admittedToNICU;

        [ObservableProperty]
        private DateTime? _nicuAdmissionTime;
        
        [ObservableProperty]
        private FeedingMethod _feedingMethod = FeedingMethod.Breastfeeding;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private bool _showDeathFields = false;

        public string BabyCountDisplay => $"Baby {BabyNumber} of {NumberOfBabies}";

        // Lists for pickers
        public List<BabySex> SexOptions => Enum.GetValues(typeof(BabySex)).Cast<BabySex>().ToList();
        public List<BabyVitalStatus> VitalStatusOptions => Enum.GetValues(typeof(BabyVitalStatus)).Cast<BabyVitalStatus>().ToList();
        public List<SkinColor> SkinColorOptions => Enum.GetValues(typeof(SkinColor)).Cast<SkinColor>().ToList();
        public List<FeedingMethod> FeedingMethodOptions => Enum.GetValues(typeof(FeedingMethod)).Cast<FeedingMethod>().ToList();

        partial void OnBirthOutcomeIdChanged(Guid? value)
        {
            if (value.HasValue)
            {
                Task.Run(() => LoadDataAsync());
            }
        }

        partial void OnBabyNumberChanged(int value)
        {
            OnPropertyChanged(nameof(BabyCountDisplay));
        }

        partial void OnNumberOfBabiesChanged(int value)
        {
            OnPropertyChanged(nameof(BabyCountDisplay));
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                Babies = new ObservableCollection<BabyDetails>();

                BirthOutcome = await _birthOutcomeRepository.GetByPartographIdAsync(PartographId);
                Partograph = await _partographRepository.GetAsync(PartographId);

                if (BirthOutcome != null)
                    NumberOfBabies = BirthOutcome.NumberOfBabies;

                IsMultipleBirth = NumberOfBabies > 1;

                // Load existing baby details if any
                var existingBabies = await _babyDetailsRepository.GetByPartographIdAsync(PartographId);
                Babies = new ObservableCollection<BabyDetails>(existingBabies.OrderBy(b => b.BabyNumber));

                // Check if all babies have been recorded
                bool allBabiesRecorded = Babies.Count >= NumberOfBabies;

                // Determine if we're adding a new baby or editing existing ones
                if (IsAddingNewBaby)
                {
                    // Check if we can add more babies
                    if (allBabiesRecorded)
                    {
                        // Cannot add more babies - show message and load first baby for editing
                        await AppShell.DisplayToastAsync($"All {NumberOfBabies} {(NumberOfBabies == 1 ? "baby" : "babies")} already recorded");
                        _logger.LogInformation($"All {NumberOfBabies} babies already recorded - loading first for editing");

                        CurrentBabyIndex = 0;
                        var firstBaby = Babies.FirstOrDefault();
                        if (firstBaby != null)
                        {
                            CurrentBaby = firstBaby;
                            LoadBabyDetails(firstBaby);
                        }
                    }
                    else
                    {
                        // Can add new baby - find the next available baby number
                        int nextBabyNumber = Babies.Count > 0 ? Babies.Max(b => b.BabyNumber) + 1 : 1;

                        // Ensure we don't exceed the allowed number
                        if (nextBabyNumber > NumberOfBabies)
                        {
                            nextBabyNumber = NumberOfBabies;
                        }

                        // Reset the index to reflect we're adding a new baby
                        CurrentBabyIndex = nextBabyNumber - 1;
                        BabyNumber = nextBabyNumber;
                        BabyTag = GetBabyTag(nextBabyNumber);

                        // Reset all fields for new baby entry
                        ResetFields();

                        // Set the baby number and tag after reset (reset clears CurrentBaby)
                        BabyNumber = nextBabyNumber;
                        BabyTag = GetBabyTag(nextBabyNumber);

                        // Pre-fill birth time from delivery time
                        if (BirthOutcome?.DeliveryTime.HasValue == true)
                        {
                            BirthDate = DateOnly.FromDateTime(BirthOutcome.DeliveryTime.Value);
                            BirthTime = BirthOutcome.DeliveryTime.Value.TimeOfDay;
                        }

                        _logger.LogInformation($"Initialized form for new baby #{nextBabyNumber} of {NumberOfBabies}");
                    }
                }
                else if (Babies.Count == 0)
                {
                    // No babies recorded yet - initialize for first baby
                    BabyNumber = 1;
                    BabyTag = GetBabyTag(1);
                    ResetFields();
                    BabyNumber = 1;
                    BabyTag = GetBabyTag(1);

                    if (BirthOutcome?.DeliveryTime.HasValue == true)
                    {
                        BirthDate = DateOnly.FromDateTime(BirthOutcome.DeliveryTime.Value);
                        BirthTime = BirthOutcome.DeliveryTime.Value.TimeOfDay;
                    }

                    _logger.LogInformation($"Initialized form for first baby of {NumberOfBabies}");
                }
                else
                {
                    // Not explicitly adding new - check if we have all babies recorded
                    if (Babies.Count < NumberOfBabies)
                    {
                        // There are unrecorded babies - start with the next one
                        int nextBabyNumber = Babies.Count + 1;
                        CurrentBabyIndex = nextBabyNumber - 1;
                        BabyNumber = nextBabyNumber;
                        BabyTag = GetBabyTag(nextBabyNumber);

                        // Reset fields for new baby
                        ResetFields();
                        BabyNumber = nextBabyNumber;
                        BabyTag = GetBabyTag(nextBabyNumber);

                        if (BirthOutcome?.DeliveryTime.HasValue == true)
                        {
                            BirthDate = DateOnly.FromDateTime(BirthOutcome.DeliveryTime.Value);
                            BirthTime = BirthOutcome.DeliveryTime.Value.TimeOfDay;
                        }

                        _logger.LogInformation($"Continuing with unrecorded baby #{nextBabyNumber} of {NumberOfBabies}");
                    }
                    else
                    {
                        // All babies recorded - load the first one for viewing/editing
                        CurrentBabyIndex = 0;
                        var firstBaby = Babies.FirstOrDefault();
                        if (firstBaby != null)
                        {
                            CurrentBaby = firstBaby;
                            LoadBabyDetails(firstBaby);
                            _logger.LogInformation($"Loaded existing baby #{firstBaby.BabyNumber} for editing");
                        }
                    }
                }

                // Update the display
                OnPropertyChanged(nameof(BabyCountDisplay));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data");
                await AppShell.DisplayToastAsync($"Error loading data: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SaveCurrentBaby()
        {
            if (!ValidateInput())
            {
                return;
            }

            try
            {
                IsBusy = true;

                var baby = CreateBabyDetailsFromForm();
                var result = await _babyDetailsRepository.SaveItemAsync(baby);

                if (result != null)
                {
                    await AppShell.DisplayToastAsync($"{BabyTag} details saved successfully");
                    _logger.LogInformation($"Saved baby #{BabyNumber} (ID: {result.ID})");

                    // Update the collection with the saved baby
                    var existingBaby = Babies.FirstOrDefault(b => b.BabyNumber == BabyNumber);
                    if (existingBaby != null)
                    {
                        var index = Babies.IndexOf(existingBaby);
                        Babies[index] = result;
                    }
                    else
                    {
                        Babies.Add(result);
                    }

                    // Update CurrentBaby to the saved result (with ID populated)
                    CurrentBaby = result;

                    // Calculate how many babies are left to record
                    int recordedBabiesCount = Babies.Count;
                    int babiesRemaining = NumberOfBabies - recordedBabiesCount;

                    // Check if there are more babies to record
                    if (babiesRemaining > 0)
                    {
                        var shouldContinue = await Application.Current.MainPage.DisplayAlert(
                            "Next Baby",
                            $"{babiesRemaining} more {(babiesRemaining == 1 ? "baby" : "babies")} to record. Do you want to continue?",
                            "Yes", "Done");

                        if (shouldContinue)
                        {
                            // Move to next baby
                            CurrentBabyIndex = recordedBabiesCount; // Next index (0-based)
                            LoadNextBaby();
                        }
                        else
                        {
                            await CompleteEntryAsync();
                        }
                    }
                    else
                    {
                        // All babies recorded
                        await CompleteEntryAsync();
                    }
                }
                else
                {
                    await AppShell.DisplayToastAsync("Failed to save baby details");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving baby details");
                await AppShell.DisplayToastAsync($"Error saving: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void LoadNextBaby()
        {
            // Calculate the next baby number (CurrentBabyIndex is 0-based, BabyNumber is 1-based)
            BabyNumber = CurrentBabyIndex + 1;
            BabyTag = GetBabyTag(BabyNumber);

            // Check if this baby was already recorded
            var existingBaby = Babies.FirstOrDefault(b => b.BabyNumber == BabyNumber);
            if (existingBaby != null)
            {
                // Loading an existing baby for editing
                CurrentBaby = existingBaby;
                LoadBabyDetails(existingBaby);
                _logger.LogInformation($"Loaded existing baby #{BabyNumber} for editing");
            }
            else
            {
                // New baby - reset all fields completely
                ResetFields();

                // Restore the baby number and tag after reset
                BabyNumber = CurrentBabyIndex + 1;
                BabyTag = GetBabyTag(BabyNumber);

                // Pre-fill birth time from delivery time
                if (BirthOutcome?.DeliveryTime.HasValue == true)
                {
                    BirthDate = DateOnly.FromDateTime(BirthOutcome.DeliveryTime.Value);
                    BirthTime = BirthOutcome.DeliveryTime.Value.TimeOfDay;
                }

                _logger.LogInformation($"Initialized form for new baby #{BabyNumber}");
            }

            // Update display
            OnPropertyChanged(nameof(BabyCountDisplay));
        }

        private void LoadBabyDetails(BabyDetails baby)
        {
            // Set CurrentBaby so we know we're editing an existing baby
            CurrentBaby = baby;

            BabyNumber = baby.BabyNumber;
            BabyTag = baby.BabyTag;
            BirthDate = baby.BirthTime != null ? DateOnly.FromDateTime(baby.BirthTime) : DateOnly.FromDateTime(DateTime.Now);
            BirthTime = baby.BirthTime != null ? baby.BirthTime.TimeOfDay : DateTime.Now.TimeOfDay;
            Sex = baby.Sex;
            VitalStatus = baby.VitalStatus;
            ShowDeathFields = VitalStatus != BabyVitalStatus.LiveBirth && VitalStatus != BabyVitalStatus.Survived;
            DeathTime = baby.DeathTime;
            DeathCause = baby.DeathCause ?? string.Empty;
            StillbirthMacerated = baby.StillbirthMacerated;
            BirthWeight = baby.BirthWeight;
            Length = baby.Length;
            HeadCircumference = baby.HeadCircumference;
            ChestCircumference = baby.ChestCircumference;
            Apgar1Min = baby.Apgar1Min;
            Apgar5Min = baby.Apgar5Min;
            //Apgar10Min = baby.Apgar10Min;
            Apgar1HeartRate = baby.Apgar1HeartRate;
            Apgar1RespiratoryEffort = baby.Apgar1RespiratoryEffort;
            Apgar1MuscleTone = baby.Apgar1MuscleTone;
            Apgar1ReflexIrritability = baby.Apgar1ReflexIrritability;
            Apgar1Color = baby.Apgar1Color;
            Apgar5HeartRate = baby.Apgar5HeartRate;
            Apgar5RespiratoryEffort = baby.Apgar5RespiratoryEffort;
            Apgar5MuscleTone = baby.Apgar5MuscleTone;
            Apgar5ReflexIrritability = baby.Apgar5ReflexIrritability;
            Apgar5Color = baby.Apgar5Color;
            UpdateApgar1Display();
            UpdateApgar5Display();
            ResuscitationRequired = baby.ResuscitationRequired;
            ResuscitationSteps = baby.ResuscitationSteps ?? string.Empty;
            ResuscitationDuration = baby.ResuscitationDuration;
            OxygenGiven = baby.OxygenGiven;
            IntubationPerformed = baby.IntubationPerformed;
            ChestCompressionsGiven = baby.ChestCompressionsGiven;
            MedicationsGiven = baby.MedicationsGiven;
            MedicationDetails = baby.MedicationDetails ?? string.Empty;
            //SkinToSkinContact = baby.SkinToSkinContact;
            EarlyBreastfeedingInitiated = baby.EarlyBreastfeedingInitiated;
            DelayedCordClamping = baby.DelayedCordClamping;
            CordClampingTime = baby.CordClampingTime;
            VitaminKGiven = baby.VitaminKGiven;
            EyeProphylaxisGiven = baby.EyeProphylaxisGiven;
            HepatitisBVaccineGiven = baby.HepatitisBVaccineGiven;
            FirstTemperature = baby.FirstTemperature;
            KangarooMotherCare = baby.KangarooMotherCare;
            CongenitalAbnormalitiesPresent = baby.CongenitalAbnormalitiesPresent;
            CongenitalAbnormalitiesDescription = baby.CongenitalAbnormalitiesDescription ?? string.Empty;
            BirthInjuriesPresent = baby.BirthInjuriesPresent;
            BirthInjuriesDescription = baby.BirthInjuriesDescription ?? string.Empty;
            //Breathing = baby.Breathing;
            //Crying = baby.Crying;
            //GoodMuscleTone = baby.GoodMuscleTone;
            //SkinColor = baby.SkinColor;
            RequiresSpecialCare = baby.RequiresSpecialCare;
            SpecialCareReason = baby.SpecialCareReason ?? string.Empty;
            AdmittedToNICU = baby.AdmittedToNICU;
            NicuAdmissionTime = baby.NICUAdmissionTime;
            FeedingMethod = baby.FeedingMethod;
            Notes = baby.Notes ?? string.Empty;
        }

        private BabyDetails CreateBabyDetailsFromForm()
        {
            // Check if we're editing an existing baby by looking for a baby with the same BabyNumber
            // CurrentBaby is only set when we're explicitly editing an existing baby
            var existingBabyId = CurrentBaby?.ID;

            // If CurrentBaby is null, check if there's an existing baby with this BabyNumber in our collection
            // This handles cases where we navigate back to edit an already-saved baby
            if (existingBabyId == null)
            {
                var existingInCollection = Babies.FirstOrDefault(b => b.BabyNumber == BabyNumber);
                existingBabyId = existingInCollection?.ID;
            }

            var baby = new BabyDetails
            {
                ID = existingBabyId, // Will be null for new babies, causing a new record to be created
                PartographID = PartographId,
                BirthOutcomeID = BirthOutcomeId,
                BabyNumber = BabyNumber,
                BabyTag = BabyTag,
                BirthTime = BirthDate != null && BirthTime != null ? new DateTime(BirthDate.Year, BirthDate.Month, BirthDate.Day).Add(BirthTime) : DateTime.Now,
                Sex = Sex,
                VitalStatus = VitalStatus,
                DeathTime = DeathTime,
                DeathCause = DeathCause,
                StillbirthMacerated = StillbirthMacerated,
                BirthWeight = BirthWeight,
                Length = Length,
                HeadCircumference = HeadCircumference,
                ChestCircumference = ChestCircumference,
                Apgar1Min = Apgar1Min,
                Apgar5Min = Apgar5Min,
                Apgar1HeartRate = Apgar1HeartRate,
                Apgar1RespiratoryEffort = Apgar1RespiratoryEffort,
                Apgar1MuscleTone = Apgar1MuscleTone,
                Apgar1ReflexIrritability = Apgar1ReflexIrritability,
                Apgar1Color = Apgar1Color,
                Apgar5HeartRate = Apgar5HeartRate,
                Apgar5RespiratoryEffort = Apgar5RespiratoryEffort,
                Apgar5MuscleTone = Apgar5MuscleTone,
                Apgar5ReflexIrritability = Apgar5ReflexIrritability,
                Apgar5Color = Apgar5Color,
                ResuscitationRequired = ResuscitationRequired,
                ResuscitationSteps = ResuscitationSteps,
                ResuscitationDuration = ResuscitationDuration,
                OxygenGiven = OxygenGiven,
                IntubationPerformed = IntubationPerformed,
                ChestCompressionsGiven = ChestCompressionsGiven,
                MedicationsGiven = MedicationsGiven,
                MedicationDetails = MedicationDetails,
                EarlyBreastfeedingInitiated = EarlyBreastfeedingInitiated,
                DelayedCordClamping = DelayedCordClamping,
                CordClampingTime = CordClampingTime,
                VitaminKGiven = VitaminKGiven,
                EyeProphylaxisGiven = EyeProphylaxisGiven,
                HepatitisBVaccineGiven = HepatitisBVaccineGiven,
                FirstTemperature = FirstTemperature,
                KangarooMotherCare = KangarooMotherCare,
                WeightClassification = GetWeightClassification(BirthWeight),
                GestationalClassification = GestationalAgeClassification.Term,
                CongenitalAbnormalitiesPresent = CongenitalAbnormalitiesPresent,
                CongenitalAbnormalitiesDescription = CongenitalAbnormalitiesDescription,
                BirthInjuriesPresent = BirthInjuriesPresent,
                BirthInjuriesDescription = BirthInjuriesDescription,
                RequiresSpecialCare = RequiresSpecialCare,
                SpecialCareReason = SpecialCareReason,
                AdmittedToNICU = AdmittedToNICU,
                NICUAdmissionTime = NicuAdmissionTime,
                FeedingMethod = FeedingMethod,
                HandlerName = Constants.Staff?.Name ?? string.Empty,
                Handler = Constants.Staff?.ID,
                Notes = Notes,
                DeviceId = DeviceIdentity.GetOrCreateDeviceId(),
                OriginDeviceId = DeviceIdentity.GetOrCreateDeviceId()
            };

            return baby;
        }

        private BirthWeightClassification GetWeightClassification(decimal weight)
        {
            if (weight < 1000) return BirthWeightClassification.ExtremelyLowBirthWeight;
            if (weight < 1500) return BirthWeightClassification.VeryLowBirthWeight;
            if (weight < 2500) return BirthWeightClassification.LowBirthWeight;
            if (weight >= 4000) return BirthWeightClassification.Macrosomia;
            return BirthWeightClassification.Normal;
        }

        private string GetBabyTag(int number)
        {
            if (NumberOfBabies == 1) return "Baby";
            string[] tags = { "Baby 1", "Baby 2", "Baby 3", "Baby 4", "Baby 5" };
            return number > 0 && number <= tags.Length ? tags[number - 1] : $"Baby {number}";
        }

        private bool ValidateInput()
        {
            if (BirthWeight <= 0 || BirthWeight > 10000)
            {
                Application.Current.MainPage.DisplayAlert("Validation Error", "Birth weight must be between 0 and 10000 grams", "OK");
                return false;
            }

            if (Apgar1Min.HasValue && (Apgar1Min < 0 || Apgar1Min > 10))
            {
                Application.Current.MainPage.DisplayAlert("Validation Error", "APGAR score must be between 0 and 10", "OK");
                return false;
            }

            if (Apgar5Min.HasValue && (Apgar5Min < 0 || Apgar5Min > 10))
            {
                Application.Current.MainPage.DisplayAlert("Validation Error", "APGAR score must be between 0 and 10", "OK");
                return false;
            }

            if (VitalStatus != BabyVitalStatus.LiveBirth && VitalStatus != BabyVitalStatus.Survived && DeathTime == null)
            {
                Application.Current.MainPage.DisplayAlert("Validation Error", "Please specify time of death", "OK");
                return false;
            }

            return true;
        }

        private void ResetFields()
        {
            // Clear the current baby reference - we're starting fresh
            CurrentBaby = null;

            // Basic identification
            BirthDate = DateOnly.FromDateTime((BirthOutcome?.DeliveryTime ?? DateTime.Now));
            BirthTime = (BirthOutcome?.DeliveryTime ?? DateTime.Now).TimeOfDay;
            Sex = BabySex.Unknown;

            // Vital status
            VitalStatus = BabyVitalStatus.LiveBirth;
            ShowDeathFields = false;
            DeathTime = null;
            DeathCause = string.Empty;
            StillbirthMacerated = false;

            // Anthropometric measurements
            BirthWeight = 0;
            Length = 0;
            HeadCircumference = 0;
            ChestCircumference = 0;

            // APGAR total scores
            Apgar1Min = null;
            Apgar5Min = null;

            // APGAR 1-minute component scores
            Apgar1HeartRate = null;
            Apgar1RespiratoryEffort = null;
            Apgar1MuscleTone = null;
            Apgar1ReflexIrritability = null;
            Apgar1Color = null;

            // APGAR 5-minute component scores
            Apgar5HeartRate = null;
            Apgar5RespiratoryEffort = null;
            Apgar5MuscleTone = null;
            Apgar5ReflexIrritability = null;
            Apgar5Color = null;

            // Reset APGAR display
            UpdateApgar1Display();
            UpdateApgar5Display();

            // Resuscitation
            ResuscitationRequired = false;
            ResuscitationSteps = string.Empty;
            ResuscitationDuration = null;
            OxygenGiven = false;
            IntubationPerformed = false;
            ChestCompressionsGiven = false;
            MedicationsGiven = false;
            MedicationDetails = string.Empty;

            // Immediate newborn care
            EarlyBreastfeedingInitiated = true;
            DelayedCordClamping = true;
            CordClampingTime = null;
            VitaminKGiven = true;
            EyeProphylaxisGiven = true;
            HepatitisBVaccineGiven = false;
            FirstTemperature = null;
            KangarooMotherCare = false;

            // Clinical assessment
            CongenitalAbnormalitiesPresent = false;
            CongenitalAbnormalitiesDescription = string.Empty;
            BirthInjuriesPresent = false;
            BirthInjuriesDescription = string.Empty;

            // Special care
            RequiresSpecialCare = false;
            SpecialCareReason = string.Empty;
            AdmittedToNICU = false;
            NicuAdmissionTime = null;
            FeedingMethod = FeedingMethod.Breastfeeding;

            // Notes
            Notes = string.Empty;
        }

        private async Task CompleteEntryAsync()
        {
            var result = await Application.Current.MainPage.DisplayAlert(
                "Complete",
                "All baby details recorded. What would you like to do next?",
                "Back to Partograph", "Stay Here");

            if (result)
            {
                //await Shell.Current.GoToAsync("..");

                await Shell.Current.GoToAsync($"thirdpartograph?patientId={Partograph.ID.ToString()}");
            }
        }

        [RelayCommand]
        private void OnVitalStatusChanged()
        {
            // Show death fields only when status is NOT LiveBirth or Survived
            ShowDeathFields = VitalStatus != BabyVitalStatus.LiveBirth && VitalStatus != BabyVitalStatus.Survived;

            if (VitalStatus == BabyVitalStatus.LiveBirth || VitalStatus == BabyVitalStatus.Survived)
            {
                DeathTime = null;
                DeathCause = string.Empty;
                StillbirthMacerated = false;
            }
        }

        [RelayCommand]
        private void OnResuscitationRequiredChanged()
        {
            if (!ResuscitationRequired)
            {
                ResuscitationSteps = string.Empty;
                ResuscitationDuration = null;
                OxygenGiven = false;
                IntubationPerformed = false;
                ChestCompressionsGiven = false;
                MedicationsGiven = false;
                MedicationDetails = string.Empty;
            }
        }

        [RelayCommand]
        private async Task ShowApgar1Popup()
        {
            try
            {
                var popup = new Pages.Modals.Apgar1Popup()
                {
                    WidthRequest = 312,
                    HeightRequest = 600,
                    HeaderHeight = 50
                };
                var viewModel = new PageModels.Modals.Apgar1PopupPageModel();

                // Load existing scores if available
                viewModel.LoadExistingScores(Apgar1HeartRate, Apgar1RespiratoryEffort,
                    Apgar1MuscleTone, Apgar1ReflexIrritability, Apgar1Color);

                // Set up callbacks
                viewModel.ClosePopup = () =>
                {
                    popup.IsOpen = false;
                };

                viewModel.OnScoreSaved = (totalScore, heartRate, respiratory, muscleTone, reflex, color) =>
                {
                    Apgar1Min = totalScore;
                    Apgar1HeartRate = heartRate;
                    Apgar1RespiratoryEffort = respiratory;
                    Apgar1MuscleTone = muscleTone;
                    Apgar1ReflexIrritability = reflex;
                    Apgar1Color = color;
                    UpdateApgar1Display();
                };

                popup.BindingContext = viewModel;
                popup.IsOpen = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing APGAR 1 popup");
                await AppShell.DisplayToastAsync("Error opening APGAR 1 score form");
            }
        }

        [RelayCommand]
        private async Task ShowApgar5Popup()
        {
            try
            {
                var popup = new Pages.Modals.Apgar5Popup()
                {
                    WidthRequest = 312,
                    HeightRequest = 600,
                    HeaderHeight = 50
                };
                var viewModel = new PageModels.Modals.Apgar5PopupPageModel();

                // Load existing scores if available
                viewModel.LoadExistingScores(Apgar5HeartRate, Apgar5RespiratoryEffort,
                    Apgar5MuscleTone, Apgar5ReflexIrritability, Apgar5Color);

                // Set up callbacks
                viewModel.ClosePopup = () =>
                {
                    popup.IsOpen = false;
                };

                viewModel.OnScoreSaved = (totalScore, heartRate, respiratory, muscleTone, reflex, color) =>
                {
                    Apgar5Min = totalScore;
                    Apgar5HeartRate = heartRate;
                    Apgar5RespiratoryEffort = respiratory;
                    Apgar5MuscleTone = muscleTone;
                    Apgar5ReflexIrritability = reflex;
                    Apgar5Color = color;
                    UpdateApgar5Display();
                };

                popup.BindingContext = viewModel;
                popup.IsOpen = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing APGAR 5 popup");
                await AppShell.DisplayToastAsync("Error opening APGAR 5 score form");
            }
        }

        private void UpdateApgar1Display()
        {
            if (Apgar1Min.HasValue)
            {
                Apgar1MinDisplay = Apgar1Min.Value.ToString();

                // Update status text and color based on WHO 2020 guidelines
                if (Apgar1Min.Value >= 7)
                {
                    Apgar1StatusText = "Normal";
                    Apgar1StatusColor = Colors.Green;
                    Apgar1ButtonColor = Color.FromArgb("#E8F5E9"); // Light green
                    Apgar1BorderColor = Colors.Green;
                }
                else if (Apgar1Min.Value >= 4)
                {
                    Apgar1StatusText = "Moderately Abnormal";
                    Apgar1StatusColor = Colors.Orange;
                    Apgar1ButtonColor = Color.FromArgb("#FFF3E0"); // Light orange
                    Apgar1BorderColor = Colors.Orange;
                }
                else
                {
                    Apgar1StatusText = "Severely Abnormal";
                    Apgar1StatusColor = Colors.Red;
                    Apgar1ButtonColor = Color.FromArgb("#FFEBEE"); // Light red
                    Apgar1BorderColor = Colors.Red;
                }
            }
            else
            {
                Apgar1MinDisplay = "—";
                Apgar1StatusText = "Tap to score";
                Apgar1StatusColor = Colors.Gray;
                Apgar1ButtonColor = Color.FromArgb("#F5F5F5");
                Apgar1BorderColor = Color.FromArgb("#E0E0E0");
            }
        }

        private void UpdateApgar5Display()
        {
            if (Apgar5Min.HasValue)
            {
                Apgar5MinDisplay = Apgar5Min.Value.ToString();

                // Update status text and color based on WHO 2020 guidelines
                if (Apgar5Min.Value >= 7)
                {
                    Apgar5StatusText = "Normal";
                    Apgar5StatusColor = Colors.Green;
                    Apgar5ButtonColor = Color.FromArgb("#E8F5E9"); // Light green
                    Apgar5BorderColor = Colors.Green;
                }
                else if (Apgar5Min.Value >= 4)
                {
                    Apgar5StatusText = "Moderately Abnormal";
                    Apgar5StatusColor = Colors.Orange;
                    Apgar5ButtonColor = Color.FromArgb("#FFF3E0"); // Light orange
                    Apgar5BorderColor = Colors.Orange;
                }
                else
                {
                    Apgar5StatusText = "Severely Abnormal";
                    Apgar5StatusColor = Colors.Red;
                    Apgar5ButtonColor = Color.FromArgb("#FFEBEE"); // Light red
                    Apgar5BorderColor = Colors.Red;
                }
            }
            else
            {
                Apgar5MinDisplay = "—";
                Apgar5StatusText = "Tap to score";
                Apgar5StatusColor = Colors.Gray;
                Apgar5ButtonColor = Color.FromArgb("#F5F5F5");
                Apgar5BorderColor = Color.FromArgb("#E0E0E0");
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // Check if we're explicitly adding a new baby
            if (query.ContainsKey("AddNewBaby"))
            {
                IsAddingNewBaby = bool.TryParse(query["AddNewBaby"].ToString(), out var addNew) && addNew;
            }
            else
            {
                // Default to adding new baby if coming from navigation
                IsAddingNewBaby = true;
            }

            if (query.ContainsKey("BirthOutcomeId"))
            {
                BirthOutcomeId = Guid.Parse(query["BirthOutcomeId"].ToString());
            }

            if (query.ContainsKey("PartographId"))
            {
                PartographId = Guid.Parse(query["PartographId"].ToString());
                LoadDataAsync().FireAndForgetSafeAsync(_errorHandler);
            }
        }
    }
}
