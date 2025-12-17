using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    /// <summary>
    /// Page model for the Placenta Delivery popup - captures placenta delivery details
    /// for Stage 3 → Stage 4 transition (WHO 2020 Guidelines)
    /// </summary>
    public partial class PlacentaDeliveryPopupPageModel : ObservableObject
    {
        public Action? ClosePopup { get; set; }
        public Action<PlacentaDeliveryData>? OnPlacentaDeliveryConfirmed { get; set; }

        [ObservableProperty]
        private DateTime _placentaDeliveryTime = DateTime.Now;

        [ObservableProperty]
        private TimeSpan _placentaDeliveryTimeOfDay = DateTime.Now.TimeOfDay;

        [ObservableProperty]
        private string _thirdStageSummary = string.Empty;

        [ObservableProperty]
        private string _timeSinceDelivery = string.Empty;

        // Placenta completeness
        [ObservableProperty]
        private bool _placentaComplete = true;

        [ObservableProperty]
        private bool _membranesComplete = true;

        [ObservableProperty]
        private string _placentaAppearance = "Normal";

        // Blood loss estimation (WHO categories)
        [ObservableProperty]
        private int _bloodLossIndex;

        [ObservableProperty]
        private string _bloodLossDisplay = "< 250 mL (Normal)";

        [ObservableProperty]
        private int _estimatedBloodLossMl;

        [ObservableProperty]
        private bool _isPPHWarning;

        [ObservableProperty]
        private bool _isPPHCritical;

        // Uterine status
        [ObservableProperty]
        private bool _uterusFirm = true;

        [ObservableProperty]
        private bool _uterusWellContracted = true;

        // Active management
        [ObservableProperty]
        private bool _oxytocinGiven = true; // WHO recommends 10 IU IM

        [ObservableProperty]
        private bool _controlledCordTraction = true;

        [ObservableProperty]
        private bool _uterineMassagePerformed = true;

        // Perineal status
        [ObservableProperty]
        private int _perinealStatusIndex;

        [ObservableProperty]
        private string _perinealStatusDisplay = "Intact";

        [ObservableProperty]
        private bool _episiotomyPerformed;

        [ObservableProperty]
        private bool _suturesRequired;

        [ObservableProperty]
        private string _validationMessage = string.Empty;

        [ObservableProperty]
        private bool _hasValidationError;

        // Blood loss categories (WHO)
        public List<string> BloodLossCategories { get; } = new()
        {
            "< 250 mL (Normal)",
            "250-500 mL (Watch)",
            "500-1000 mL (PPH)",
            "> 1000 mL (Severe PPH)"
        };

        // Perineal status options
        public List<string> PerinealStatusOptions { get; } = new()
        {
            "Intact",
            "1st Degree Tear",
            "2nd Degree Tear",
            "3rd Degree Tear",
            "4th Degree Tear",
            "Episiotomy"
        };

        // Placenta appearance options
        public List<string> PlacentaAppearanceOptions { get; } = new()
        {
            "Normal",
            "Calcified",
            "Infarcted",
            "Abnormal Vessels",
            "Other Abnormality"
        };

        public PlacentaDeliveryPopupPageModel()
        {
            PlacentaDeliveryTime = DateTime.Now.Date;
            PlacentaDeliveryTimeOfDay = DateTime.Now.TimeOfDay;
        }

        /// <summary>
        /// Initializes the popup with third stage information
        /// </summary>
        public void Initialize(DateTime? deliveryTime, DateTime? thirdStageStart)
        {
            PlacentaDeliveryTime = DateTime.Now.Date;
            PlacentaDeliveryTimeOfDay = DateTime.Now.TimeOfDay;

            if (deliveryTime.HasValue)
            {
                var timeSince = DateTime.Now - deliveryTime.Value;
                TimeSinceDelivery = $"Time since baby delivery: {(int)timeSince.TotalMinutes} minutes";
            }

            if (thirdStageStart.HasValue)
            {
                var duration = DateTime.Now - thirdStageStart.Value;
                ThirdStageSummary = $"Third Stage Duration: {(int)duration.TotalMinutes} minutes";

                // Add warning if over 20 minutes
                if (duration.TotalMinutes >= 20)
                {
                    ThirdStageSummary += " (Extended)";
                }
            }

            // Reset values
            PlacentaComplete = true;
            MembranesComplete = true;
            UterusFirm = true;
            UterusWellContracted = true;
            OxytocinGiven = true;
            ControlledCordTraction = true;
            UterineMassagePerformed = true;
            BloodLossIndex = 0;
            PerinealStatusIndex = 0;
            HasValidationError = false;
            ValidationMessage = string.Empty;
        }

        partial void OnBloodLossIndexChanged(int value)
        {
            if (value >= 0 && value < BloodLossCategories.Count)
            {
                BloodLossDisplay = BloodLossCategories[value];

                // Set estimated blood loss
                EstimatedBloodLossMl = value switch
                {
                    0 => 200,
                    1 => 375,
                    2 => 750,
                    3 => 1200,
                    _ => 0
                };

                // Set PPH warnings
                IsPPHWarning = value >= 2; // 500+ mL
                IsPPHCritical = value >= 3; // 1000+ mL
            }
        }

        partial void OnPerinealStatusIndexChanged(int value)
        {
            if (value >= 0 && value < PerinealStatusOptions.Count)
            {
                PerinealStatusDisplay = PerinealStatusOptions[value];
                EpisiotomyPerformed = value == 5;
                SuturesRequired = value >= 1 && value <= 5;
            }
        }

        private bool Validate()
        {
            // Check for incomplete placenta
            if (!PlacentaComplete || !MembranesComplete)
            {
                var confirm = Application.Current?.MainPage?.DisplayAlert(
                    "Incomplete Placenta Warning",
                    "You have indicated the placenta or membranes are incomplete. This requires careful monitoring. Continue?",
                    "Continue", "Go Back").Result ?? false;

                if (!confirm)
                {
                    return false;
                }
            }

            // Check for PPH
            if (IsPPHCritical)
            {
                var confirm = Application.Current?.MainPage?.DisplayAlert(
                    "Severe PPH Warning",
                    "Blood loss >1000mL indicates severe postpartum hemorrhage. Ensure emergency protocols are activated. Continue?",
                    "Continue", "Go Back").Result ?? false;

                if (!confirm)
                {
                    return false;
                }
            }

            // Check uterine status
            if (!UterusFirm || !UterusWellContracted)
            {
                ValidationMessage = "Warning: Uterine atony detected. Ensure active management and monitoring.";
                HasValidationError = true;
                // Still allow to proceed but show warning
            }
            else
            {
                HasValidationError = false;
                ValidationMessage = string.Empty;
            }

            return true;
        }

        [RelayCommand]
        private async Task ConfirmPlacentaDelivery()
        {
            if (!Validate())
                return;

            var fullDeliveryTime = PlacentaDeliveryTime.Date + PlacentaDeliveryTimeOfDay;

            var data = new PlacentaDeliveryData
            {
                PlacentaDeliveryTime = fullDeliveryTime,
                PlacentaComplete = PlacentaComplete,
                MembranesComplete = MembranesComplete,
                PlacentaAppearance = PlacentaAppearance,
                EstimatedBloodLossMl = EstimatedBloodLossMl,
                BloodLossCategory = BloodLossDisplay,
                IsPPH = IsPPHWarning,
                IsSeverePPH = IsPPHCritical,
                UterusFirm = UterusFirm,
                UterusWellContracted = UterusWellContracted,
                OxytocinGiven = OxytocinGiven,
                ControlledCordTraction = ControlledCordTraction,
                UterineMassagePerformed = UterineMassagePerformed,
                PerinealStatus = PerinealStatusDisplay,
                EpisiotomyPerformed = EpisiotomyPerformed,
                SuturesRequired = SuturesRequired
            };

            OnPlacentaDeliveryConfirmed?.Invoke(data);
            ClosePopup?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            ClosePopup?.Invoke();
        }
    }

    /// <summary>
    /// Data class for placenta delivery information
    /// </summary>
    public class PlacentaDeliveryData
    {
        public DateTime PlacentaDeliveryTime { get; set; }
        public bool PlacentaComplete { get; set; }
        public bool MembranesComplete { get; set; }
        public string PlacentaAppearance { get; set; } = string.Empty;
        public int EstimatedBloodLossMl { get; set; }
        public string BloodLossCategory { get; set; } = string.Empty;
        public bool IsPPH { get; set; }
        public bool IsSeverePPH { get; set; }
        public bool UterusFirm { get; set; }
        public bool UterusWellContracted { get; set; }
        public bool OxytocinGiven { get; set; }
        public bool ControlledCordTraction { get; set; }
        public bool UterineMassagePerformed { get; set; }
        public string PerinealStatus { get; set; } = string.Empty;
        public bool EpisiotomyPerformed { get; set; }
        public bool SuturesRequired { get; set; }
    }
}
