using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    /// <summary>
    /// Page model for the Delivery Moment popup - captures essential delivery details
    /// for Stage 2 → Stage 3 transition (WHO 2020 Guidelines)
    /// </summary>
    public partial class DeliveryMomentPopupPageModel : ObservableObject
    {
        public Action? ClosePopup { get; set; }
        public Action<DeliveryMomentData>? OnDeliveryConfirmed { get; set; }

        [ObservableProperty]
        private DateTime _deliveryTime = DateTime.Now;

        [ObservableProperty]
        private TimeSpan _deliveryTimeOfDay = DateTime.Now.TimeOfDay;

        [ObservableProperty]
        private string _babySex = string.Empty;

        [ObservableProperty]
        private bool _isMale;

        [ObservableProperty]
        private bool _isFemale;

        [ObservableProperty]
        private bool _sexUndetermined;

        [ObservableProperty]
        private bool _immediateCry = true;

        [ObservableProperty]
        private bool _cordClampedDelayed = true; // WHO recommends delayed clamping

        [ObservableProperty]
        private bool _skinToSkinInitiated = true; // WHO recommends immediate skin-to-skin

        [ObservableProperty]
        private string _deliveryModeDisplay = "Spontaneous Vaginal";

        [ObservableProperty]
        private int _deliveryModeIndex;

        [ObservableProperty]
        private string _secondStageSummary = string.Empty;

        [ObservableProperty]
        private bool _isMultipleBirth;

        [ObservableProperty]
        private int _babyNumber = 1;

        [ObservableProperty]
        private int _totalBabies = 1;

        [ObservableProperty]
        private string _validationMessage = string.Empty;

        [ObservableProperty]
        private bool _hasValidationError;

        // Delivery mode options
        public List<string> DeliveryModes { get; } = new()
        {
            "Spontaneous Vaginal",
            "Assisted (Vacuum)",
            "Assisted (Forceps)",
            "Breech Vaginal",
            "Emergency C-Section",
            "Elective C-Section"
        };

        public DeliveryMomentPopupPageModel()
        {
            // Default to current time
            DeliveryTime = DateTime.Now.Date;
            DeliveryTimeOfDay = DateTime.Now.TimeOfDay;
        }

        /// <summary>
        /// Initializes the popup with second stage summary
        /// </summary>
        public void Initialize(TimeSpan? secondStageDuration, DateTime? secondStageStart)
        {
            if (secondStageDuration.HasValue && secondStageStart.HasValue)
            {
                SecondStageSummary = $"Second Stage Duration: {(int)secondStageDuration.Value.TotalHours}h {secondStageDuration.Value.Minutes}m\n" +
                                    $"Started: {secondStageStart.Value:HH:mm}";
            }

            // Reset values
            DeliveryTime = DateTime.Now.Date;
            DeliveryTimeOfDay = DateTime.Now.TimeOfDay;
            ImmediateCry = true;
            CordClampedDelayed = true;
            SkinToSkinInitiated = true;
            HasValidationError = false;
            ValidationMessage = string.Empty;
            BabySex = string.Empty;
            IsMale = false;
            IsFemale = false;
            SexUndetermined = false;
        }

        partial void OnIsMaleChanged(bool value)
        {
            if (value)
            {
                IsFemale = false;
                SexUndetermined = false;
                BabySex = "Male";
            }
        }

        partial void OnIsFemaleChanged(bool value)
        {
            if (value)
            {
                IsMale = false;
                SexUndetermined = false;
                BabySex = "Female";
            }
        }

        partial void OnSexUndeterminedChanged(bool value)
        {
            if (value)
            {
                IsMale = false;
                IsFemale = false;
                BabySex = "Undetermined";
            }
        }

        partial void OnDeliveryModeIndexChanged(int value)
        {
            if (value >= 0 && value < DeliveryModes.Count)
            {
                DeliveryModeDisplay = DeliveryModes[value];
            }
        }

        [RelayCommand]
        private void SelectMale()
        {
            IsMale = true;
        }

        [RelayCommand]
        private void SelectFemale()
        {
            IsFemale = true;
        }

        [RelayCommand]
        private void SelectUndetermined()
        {
            SexUndetermined = true;
        }

        private bool Validate()
        {
            if (string.IsNullOrEmpty(BabySex))
            {
                ValidationMessage = "Please select baby's sex";
                HasValidationError = true;
                return false;
            }

            var fullDeliveryTime = DeliveryTime.Date + DeliveryTimeOfDay;
            if (fullDeliveryTime > DateTime.Now.AddMinutes(5))
            {
                ValidationMessage = "Delivery time cannot be in the future";
                HasValidationError = true;
                return false;
            }

            HasValidationError = false;
            ValidationMessage = string.Empty;
            return true;
        }

        [RelayCommand]
        private void ConfirmDelivery()
        {
            if (!Validate())
                return;

            var fullDeliveryTime = DeliveryTime.Date + DeliveryTimeOfDay;

            var data = new DeliveryMomentData
            {
                DeliveryTime = fullDeliveryTime,
                BabySex = BabySex,
                ImmediateCry = ImmediateCry,
                CordClampedDelayed = CordClampedDelayed,
                SkinToSkinInitiated = SkinToSkinInitiated,
                DeliveryMode = DeliveryModeDisplay,
                IsMultipleBirth = IsMultipleBirth,
                BabyNumber = BabyNumber,
                TotalBabies = TotalBabies
            };

            OnDeliveryConfirmed?.Invoke(data);
            ClosePopup?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            ClosePopup?.Invoke();
        }
    }

    /// <summary>
    /// Data class for delivery moment information
    /// </summary>
    public class DeliveryMomentData
    {
        public DateTime DeliveryTime { get; set; }
        public string BabySex { get; set; } = string.Empty;
        public bool ImmediateCry { get; set; }
        public bool CordClampedDelayed { get; set; }
        public bool SkinToSkinInitiated { get; set; }
        public string DeliveryMode { get; set; } = string.Empty;
        public bool IsMultipleBirth { get; set; }
        public int BabyNumber { get; set; }
        public int TotalBabies { get; set; }
    }
}
