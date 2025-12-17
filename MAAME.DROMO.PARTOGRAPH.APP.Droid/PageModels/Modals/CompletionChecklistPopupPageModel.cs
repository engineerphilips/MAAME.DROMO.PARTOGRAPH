using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    /// <summary>
    /// Page model for the Completion Checklist popup - ensures all requirements are met
    /// before marking delivery as complete (WHO 2020 Guidelines)
    /// </summary>
    public partial class CompletionChecklistPopupPageModel : ObservableObject
    {
        public Action? ClosePopup { get; set; }
        public Action<CompletionChecklistData>? OnCompletionConfirmed { get; set; }

        [ObservableProperty]
        private string _fourthStageSummary = string.Empty;

        [ObservableProperty]
        private string _totalLabourDuration = string.Empty;

        [ObservableProperty]
        private int _vitalSignsRecorded;

        [ObservableProperty]
        private int _requiredVitalSigns = 8; // Every 15 min for 2 hours

        [ObservableProperty]
        private string _vitalSignsStatus = string.Empty;

        [ObservableProperty]
        private bool _vitalSignsComplete;

        // Maternal Checklist
        [ObservableProperty]
        private bool _vitalsStable = true;

        [ObservableProperty]
        private bool _bleedingControlled = true;

        [ObservableProperty]
        private bool _uterusFirm = true;

        [ObservableProperty]
        private bool _bladderEmptied = true;

        [ObservableProperty]
        private bool _mobilityAssessed = true;

        [ObservableProperty]
        private bool _painManaged = true;

        // Newborn Checklist
        [ObservableProperty]
        private bool _breastfeedingInitiated = true;

        [ObservableProperty]
        private bool _babyExaminationComplete = true;

        [ObservableProperty]
        private bool _vitaminKGiven = true;

        [ObservableProperty]
        private bool _eyeProphylaxisGiven = true;

        [ObservableProperty]
        private bool _babyIdentificationComplete = true;

        [ObservableProperty]
        private bool _birthNotificationComplete;

        // Documentation Checklist
        [ObservableProperty]
        private bool _partographComplete = true;

        [ObservableProperty]
        private bool _birthOutcomeRecorded = true;

        [ObservableProperty]
        private bool _babyDetailsRecorded = true;

        [ObservableProperty]
        private bool _apgarScoresRecorded = true;

        // Discharge Readiness
        [ObservableProperty]
        private string _dischargeDestination = "Postnatal Ward";

        [ObservableProperty]
        private int _dischargeDestinationIndex;

        [ObservableProperty]
        private string _handoverNotes = string.Empty;

        // Validation
        [ObservableProperty]
        private bool _canComplete;

        [ObservableProperty]
        private string _validationMessage = string.Empty;

        [ObservableProperty]
        private bool _hasValidationError;

        [ObservableProperty]
        private int _checklistProgress;

        [ObservableProperty]
        private string _checklistProgressText = string.Empty;

        // Discharge destinations
        public List<string> DischargeDestinations { get; } = new()
        {
            "Postnatal Ward",
            "Mother-Baby Unit",
            "High Dependency Unit",
            "Home (Early Discharge)",
            "NICU (Baby Only)"
        };

        public CompletionChecklistPopupPageModel()
        {
            UpdateChecklistProgress();
        }

        /// <summary>
        /// Initializes the popup with fourth stage information
        /// </summary>
        public void Initialize(Partograph partograph, int vitalSignsCount)
        {
            VitalSignsRecorded = vitalSignsCount;
            VitalSignsComplete = vitalSignsCount >= RequiredVitalSigns;
            VitalSignsStatus = $"{vitalSignsCount}/{RequiredVitalSigns} recordings";

            if (partograph.FourthStageStartTime.HasValue)
            {
                var duration = DateTime.Now - partograph.FourthStageStartTime.Value;
                FourthStageSummary = $"Fourth Stage Duration: {(int)duration.TotalHours}h {duration.Minutes}m";
            }

            // Calculate total labour duration
            if (partograph.LaborStartTime.HasValue)
            {
                var totalDuration = DateTime.Now - partograph.LaborStartTime.Value;
                TotalLabourDuration = $"Total Labour Duration: {(int)totalDuration.TotalHours}h {totalDuration.Minutes}m";
            }

            // Reset checklist items
            VitalsStable = true;
            BleedingControlled = true;
            UterusFirm = true;
            BladderEmptied = true;
            MobilityAssessed = true;
            PainManaged = true;
            BreastfeedingInitiated = true;
            BabyExaminationComplete = true;
            VitaminKGiven = true;
            EyeProphylaxisGiven = true;
            BabyIdentificationComplete = true;
            PartographComplete = true;
            BirthOutcomeRecorded = true;
            BabyDetailsRecorded = true;
            ApgarScoresRecorded = true;
            DischargeDestinationIndex = 0;
            HandoverNotes = string.Empty;

            UpdateChecklistProgress();
        }

        partial void OnVitalsStableChanged(bool value) => UpdateChecklistProgress();
        partial void OnBleedingControlledChanged(bool value) => UpdateChecklistProgress();
        partial void OnUterusFirmChanged(bool value) => UpdateChecklistProgress();
        partial void OnBladderEmptiedChanged(bool value) => UpdateChecklistProgress();
        partial void OnMobilityAssessedChanged(bool value) => UpdateChecklistProgress();
        partial void OnPainManagedChanged(bool value) => UpdateChecklistProgress();
        partial void OnBreastfeedingInitiatedChanged(bool value) => UpdateChecklistProgress();
        partial void OnBabyExaminationCompleteChanged(bool value) => UpdateChecklistProgress();
        partial void OnVitaminKGivenChanged(bool value) => UpdateChecklistProgress();
        partial void OnEyeProphylaxisGivenChanged(bool value) => UpdateChecklistProgress();
        partial void OnBabyIdentificationCompleteChanged(bool value) => UpdateChecklistProgress();
        partial void OnBirthNotificationCompleteChanged(bool value) => UpdateChecklistProgress();
        partial void OnPartographCompleteChanged(bool value) => UpdateChecklistProgress();
        partial void OnBirthOutcomeRecordedChanged(bool value) => UpdateChecklistProgress();
        partial void OnBabyDetailsRecordedChanged(bool value) => UpdateChecklistProgress();
        partial void OnApgarScoresRecordedChanged(bool value) => UpdateChecklistProgress();

        partial void OnDischargeDestinationIndexChanged(int value)
        {
            if (value >= 0 && value < DischargeDestinations.Count)
            {
                DischargeDestination = DischargeDestinations[value];
            }
        }

        private void UpdateChecklistProgress()
        {
            // Count completed items
            var maternalItems = new[] { VitalsStable, BleedingControlled, UterusFirm, BladderEmptied, MobilityAssessed, PainManaged };
            var newbornItems = new[] { BreastfeedingInitiated, BabyExaminationComplete, VitaminKGiven, EyeProphylaxisGiven, BabyIdentificationComplete, BirthNotificationComplete };
            var documentationItems = new[] { PartographComplete, BirthOutcomeRecorded, BabyDetailsRecorded, ApgarScoresRecorded };

            var totalItems = maternalItems.Length + newbornItems.Length + documentationItems.Length;
            var completedItems = maternalItems.Count(x => x) + newbornItems.Count(x => x) + documentationItems.Count(x => x);

            ChecklistProgress = (int)((completedItems * 100.0) / totalItems);
            ChecklistProgressText = $"{completedItems}/{totalItems} items complete";

            // Critical items that must be checked
            var criticalItems = VitalsStable && BleedingControlled && UterusFirm && BabyExaminationComplete;
            CanComplete = criticalItems && ChecklistProgress >= 75; // Allow completion if 75%+ and all critical items done

            if (!criticalItems)
            {
                HasValidationError = true;
                ValidationMessage = "Critical items incomplete: Ensure vitals stable, bleeding controlled, uterus firm, and baby examined.";
            }
            else if (ChecklistProgress < 75)
            {
                HasValidationError = true;
                ValidationMessage = $"Complete at least 75% of checklist items ({ChecklistProgress}% complete)";
            }
            else
            {
                HasValidationError = false;
                ValidationMessage = string.Empty;
            }
        }

        [RelayCommand]
        private async Task ConfirmCompletion()
        {
            if (!CanComplete)
            {
                await Application.Current?.MainPage?.DisplayAlert(
                    "Cannot Complete",
                    ValidationMessage,
                    "OK");
                return;
            }

            var confirm = await Application.Current?.MainPage?.DisplayAlert(
                "Complete Delivery",
                "Are you sure you want to mark this delivery as complete and transfer the patient?",
                "Yes, Complete",
                "Cancel");

            if (confirm != true)
                return;

            var data = new CompletionChecklistData
            {
                CompletionTime = DateTime.Now,
                VitalSignsRecorded = VitalSignsRecorded,
                VitalsStable = VitalsStable,
                BleedingControlled = BleedingControlled,
                UterusFirm = UterusFirm,
                BladderEmptied = BladderEmptied,
                MobilityAssessed = MobilityAssessed,
                PainManaged = PainManaged,
                BreastfeedingInitiated = BreastfeedingInitiated,
                BabyExaminationComplete = BabyExaminationComplete,
                VitaminKGiven = VitaminKGiven,
                EyeProphylaxisGiven = EyeProphylaxisGiven,
                BabyIdentificationComplete = BabyIdentificationComplete,
                BirthNotificationComplete = BirthNotificationComplete,
                PartographComplete = PartographComplete,
                BirthOutcomeRecorded = BirthOutcomeRecorded,
                BabyDetailsRecorded = BabyDetailsRecorded,
                ApgarScoresRecorded = ApgarScoresRecorded,
                DischargeDestination = DischargeDestination,
                HandoverNotes = HandoverNotes,
                ChecklistCompletionPercentage = ChecklistProgress
            };

            OnCompletionConfirmed?.Invoke(data);
            ClosePopup?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            ClosePopup?.Invoke();
        }
    }

    /// <summary>
    /// Data class for completion checklist information
    /// </summary>
    public class CompletionChecklistData
    {
        public DateTime CompletionTime { get; set; }
        public int VitalSignsRecorded { get; set; }

        // Maternal
        public bool VitalsStable { get; set; }
        public bool BleedingControlled { get; set; }
        public bool UterusFirm { get; set; }
        public bool BladderEmptied { get; set; }
        public bool MobilityAssessed { get; set; }
        public bool PainManaged { get; set; }

        // Newborn
        public bool BreastfeedingInitiated { get; set; }
        public bool BabyExaminationComplete { get; set; }
        public bool VitaminKGiven { get; set; }
        public bool EyeProphylaxisGiven { get; set; }
        public bool BabyIdentificationComplete { get; set; }
        public bool BirthNotificationComplete { get; set; }

        // Documentation
        public bool PartographComplete { get; set; }
        public bool BirthOutcomeRecorded { get; set; }
        public bool BabyDetailsRecorded { get; set; }
        public bool ApgarScoresRecorded { get; set; }

        // Discharge
        public string DischargeDestination { get; set; } = string.Empty;
        public string HandoverNotes { get; set; } = string.Empty;
        public int ChecklistCompletionPercentage { get; set; }
    }
}
