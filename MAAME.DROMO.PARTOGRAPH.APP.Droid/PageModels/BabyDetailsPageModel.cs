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
    [QueryProperty(nameof(BirthOutcomeId), "BirthOutcomeId")]
    [QueryProperty(nameof(PartographId), "PartographId")]
    [QueryProperty(nameof(NumberOfBabies), "NumberOfBabies")]
    public partial class BabyDetailsPageModel : ObservableObject
    {
        private readonly BabyDetailsRepository _babyDetailsRepository;
        private readonly BirthOutcomeRepository _birthOutcomeRepository;
        private readonly PartographRepository _partographRepository;
        private readonly ILogger _logger;

        public BabyDetailsPageModel(
            BabyDetailsRepository babyDetailsRepository,
            BirthOutcomeRepository birthOutcomeRepository,
            PartographRepository partographRepository,
            ILogger<BabyDetailsPageModel> logger)
        {
            _babyDetailsRepository = babyDetailsRepository;
            _birthOutcomeRepository = birthOutcomeRepository;
            _partographRepository = partographRepository;
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

        // Baby Identification
        [ObservableProperty]
        private int _babyNumber = 1;

        [ObservableProperty]
        private string _babyTag = string.Empty;

        [ObservableProperty]
        private DateTime _birthTime = DateTime.Now;

        [ObservableProperty]
        private BabySex _sex = BabySex.Unknown;

        // Vital Status
        [ObservableProperty]
        private BabyVitalStatus _vitalStatus = BabyVitalStatus.LiveBirth;

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

        [ObservableProperty]
        private int? _apgar10Min;

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
        [ObservableProperty]
        private bool _skinToSkinContact = true;

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

        // Clinical Status
        [ObservableProperty]
        private bool _breathing = true;

        [ObservableProperty]
        private bool _crying = true;

        [ObservableProperty]
        private bool _goodMuscleTone = true;

        [ObservableProperty]
        private SkinColor _skinColor = SkinColor.Pink;

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

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;

                BirthOutcome = await _birthOutcomeRepository.GetByPartographIdAsync(PartographId);
                Partograph = await _partographRepository.GetCurrentPartographAsync(PartographId);

                IsMultipleBirth = NumberOfBabies > 1;

                // Load existing baby details if any
                var existingBabies = await _babyDetailsRepository.GetByPartographIdAsync(PartographId);
                Babies = new ObservableCollection<BabyDetails>(existingBabies);

                if (Babies.Count > 0)
                {
                    LoadBabyDetails(Babies[0]);
                }
                else
                {
                    // Initialize first baby
                    BabyNumber = 1;
                    BabyTag = NumberOfBabies > 1 ? "Baby A" : "Baby";
                    if (BirthOutcome?.DeliveryTime.HasValue == true)
                    {
                        BirthTime = BirthOutcome.DeliveryTime.Value;
                    }
                }
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
        private async Task SaveCurrentBabyAsync()
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

                    // Add to collection if not already there
                    var existingBaby = Babies.FirstOrDefault(b => b.BabyNumber == BabyNumber);
                    if (existingBaby != null)
                    {
                        Babies.Remove(existingBaby);
                    }
                    Babies.Add(baby);

                    // Move to next baby or complete
                    if (CurrentBabyIndex < NumberOfBabies - 1)
                    {
                        var shouldContinue = await Application.Current.MainPage.DisplayAlert(
                            "Next Baby",
                            $"Do you want to record details for the next baby?",
                            "Yes", "Done");

                        if (shouldContinue)
                        {
                            CurrentBabyIndex++;
                            LoadNextBaby();
                        }
                        else
                        {
                            await CompleteEntryAsync();
                        }
                    }
                    else
                    {
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
            BabyNumber = CurrentBabyIndex + 1;
            BabyTag = GetBabyTag(BabyNumber);

            // Check if this baby was already recorded
            var existingBaby = Babies.FirstOrDefault(b => b.BabyNumber == BabyNumber);
            if (existingBaby != null)
            {
                LoadBabyDetails(existingBaby);
            }
            else
            {
                ResetFields();
            }
        }

        private void LoadBabyDetails(BabyDetails baby)
        {
            BabyNumber = baby.BabyNumber;
            BabyTag = baby.BabyTag;
            BirthTime = baby.BirthTime;
            Sex = baby.Sex;
            VitalStatus = baby.VitalStatus;
            DeathTime = baby.DeathTime;
            DeathCause = baby.DeathCause ?? string.Empty;
            StillbirthMacerated = baby.StillbirthMacerated;
            BirthWeight = baby.BirthWeight;
            Length = baby.Length;
            HeadCircumference = baby.HeadCircumference;
            ChestCircumference = baby.ChestCircumference;
            Apgar1Min = baby.Apgar1Min;
            Apgar5Min = baby.Apgar5Min;
            Apgar10Min = baby.Apgar10Min;
            ResuscitationRequired = baby.ResuscitationRequired;
            ResuscitationSteps = baby.ResuscitationSteps ?? string.Empty;
            ResuscitationDuration = baby.ResuscitationDuration;
            OxygenGiven = baby.OxygenGiven;
            IntubationPerformed = baby.IntubationPerformed;
            ChestCompressionsGiven = baby.ChestCompressionsGiven;
            MedicationsGiven = baby.MedicationsGiven;
            MedicationDetails = baby.MedicationDetails ?? string.Empty;
            SkinToSkinContact = baby.SkinToSkinContact;
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
            Breathing = baby.Breathing;
            Crying = baby.Crying;
            GoodMuscleTone = baby.GoodMuscleTone;
            SkinColor = baby.SkinColor;
            RequiresSpecialCare = baby.RequiresSpecialCare;
            SpecialCareReason = baby.SpecialCareReason ?? string.Empty;
            AdmittedToNICU = baby.AdmittedToNICU;
            NicuAdmissionTime = baby.NICUAdmissionTime;
            FeedingMethod = baby.FeedingMethod;
            Notes = baby.Notes ?? string.Empty;
        }

        private BabyDetails CreateBabyDetailsFromForm()
        {
            var baby = new BabyDetails
            {
                ID = CurrentBaby?.ID ?? Guid.NewGuid(),
                PartographID = PartographId,
                BirthOutcomeID = BirthOutcomeId,
                BabyNumber = BabyNumber,
                BabyTag = BabyTag,
                BirthTime = BirthTime,
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
                Apgar10Min = Apgar10Min,
                ResuscitationRequired = ResuscitationRequired,
                ResuscitationSteps = ResuscitationSteps,
                ResuscitationDuration = ResuscitationDuration,
                OxygenGiven = OxygenGiven,
                IntubationPerformed = IntubationPerformed,
                ChestCompressionsGiven = ChestCompressionsGiven,
                MedicationsGiven = MedicationsGiven,
                MedicationDetails = MedicationDetails,
                SkinToSkinContact = SkinToSkinContact,
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
                Breathing = Breathing,
                Crying = Crying,
                GoodMuscleTone = GoodMuscleTone,
                SkinColor = SkinColor,
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
            string[] tags = { "Baby A", "Baby B", "Baby C", "Baby D", "Baby E" };
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
            BirthTime = BirthOutcome?.DeliveryTime ?? DateTime.Now;
            Sex = BabySex.Unknown;
            VitalStatus = BabyVitalStatus.LiveBirth;
            DeathTime = null;
            DeathCause = string.Empty;
            StillbirthMacerated = false;
            BirthWeight = 0;
            Length = 0;
            HeadCircumference = 0;
            ChestCircumference = 0;
            Apgar1Min = null;
            Apgar5Min = null;
            Apgar10Min = null;
            ResuscitationRequired = false;
            ResuscitationSteps = string.Empty;
            SkinToSkinContact = true;
            EarlyBreastfeedingInitiated = true;
            DelayedCordClamping = true;
            VitaminKGiven = true;
            EyeProphylaxisGiven = true;
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
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        private void OnVitalStatusChanged()
        {
            if (VitalStatus == BabyVitalStatus.LiveBirth || VitalStatus == BabyVitalStatus.Survived)
            {
                DeathTime = null;
                DeathCause = string.Empty;
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
    }
}
