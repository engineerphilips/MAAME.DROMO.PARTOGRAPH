using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    [QueryProperty(nameof(PartographId), "PartographId")]
    public partial class ReferralPageModel : ObservableObject
    {
        private readonly ReferralRepository _referralRepository;
        private readonly PartographRepository _partographRepository;
        private readonly PatientRepository _patientRepository;
        private readonly BirthOutcomeRepository _birthOutcomeRepository;
        private readonly BabyDetailsRepository _babyDetailsRepository;
        private readonly ILogger _logger;

        public ReferralPageModel(
            ReferralRepository referralRepository,
            PartographRepository partographRepository,
            PatientRepository patientRepository,
            BirthOutcomeRepository birthOutcomeRepository,
            BabyDetailsRepository babyDetailsRepository,
            ILogger<ReferralPageModel> logger)
        {
            _referralRepository = referralRepository;
            _partographRepository = partographRepository;
            _patientRepository = patientRepository;
            _birthOutcomeRepository = birthOutcomeRepository;
            _babyDetailsRepository = babyDetailsRepository;
            _logger = logger;
        }

        [ObservableProperty]
        private Guid? _partographId;

        [ObservableProperty]
        private Partograph? _partograph;

        [ObservableProperty]
        private Patient? _patient;

        [ObservableProperty]
        private bool _isBusy;

        // Basic Information
        [ObservableProperty]
        private DateTime _referralTime = DateTime.Now;

        [ObservableProperty]
        private ReferralType _referralType = ReferralType.Maternal;

        [ObservableProperty]
        private ReferralUrgency _urgency = ReferralUrgency.Emergency;

        // Referring Facility
        [ObservableProperty]
        private string _referringFacilityName = string.Empty;

        [ObservableProperty]
        private string _referringFacilityLevel = string.Empty;

        [ObservableProperty]
        private string _referringPhysician = string.Empty;

        [ObservableProperty]
        private string _referringPhysicianContact = string.Empty;

        // Destination Facility
        [ObservableProperty]
        private string _destinationFacilityName = string.Empty;

        [ObservableProperty]
        private string _destinationFacilityLevel = string.Empty;

        [ObservableProperty]
        private string _destinationFacilityContact = string.Empty;

        [ObservableProperty]
        private string _destinationAddress = string.Empty;

        [ObservableProperty]
        private bool _destinationNotified;

        // Maternal Reasons
        [ObservableProperty]
        private bool _prolongedLabor;

        [ObservableProperty]
        private bool _obstructedLabor;

        [ObservableProperty]
        private bool _foetalDistress;

        [ObservableProperty]
        private bool _antepartumHemorrhage;

        [ObservableProperty]
        private bool _postpartumHemorrhage;

        [ObservableProperty]
        private bool _severePreeclampsia;

        [ObservableProperty]
        private bool _eclampsia;

        [ObservableProperty]
        private bool _septicShock;

        [ObservableProperty]
        private bool _rupturedUterus;

        [ObservableProperty]
        private bool _abnormalPresentation;

        [ObservableProperty]
        private bool _cordProlapse;

        // Neonatal Reasons
        [ObservableProperty]
        private bool _neonatalAsphyxia;

        [ObservableProperty]
        private bool _prematurityComplications;

        [ObservableProperty]
        private bool _lowBirthWeight;

        [ObservableProperty]
        private bool _respiratoryDistress;

        [ObservableProperty]
        private bool _congenitalAbnormalities;

        [ObservableProperty]
        private bool _neonatalSepsis;

        // Other Reasons
        [ObservableProperty]
        private bool _lackOfResources;

        [ObservableProperty]
        private bool _requiresCaesareanSection;

        [ObservableProperty]
        private bool _requiresBloodTransfusion;

        [ObservableProperty]
        private bool _requiresSpecializedCare;

        [ObservableProperty]
        private string _otherReasons = string.Empty;

        [ObservableProperty]
        private string _primaryDiagnosis = string.Empty;

        [ObservableProperty]
        private string _clinicalSummary = string.Empty;

        // Maternal Condition at Referral
        [ObservableProperty]
        private string _maternalCondition = string.Empty;

        [ObservableProperty]
        private int? _maternalPulse;

        [ObservableProperty]
        private int? _maternalBPSystolic;

        [ObservableProperty]
        private int? _maternalBPDiastolic;

        [ObservableProperty]
        private decimal? _maternalTemperature;

        [ObservableProperty]
        private string _maternalConsciousness = "Alert";

        // Fetal/Neonatal Condition
        [ObservableProperty]
        private int? _fetalHeartRate;

        [ObservableProperty]
        private string _fetalCondition = string.Empty;

        [ObservableProperty]
        private int? _numberOfBabiesBeingReferred;

        [ObservableProperty]
        private string _neonatalCondition = string.Empty;

        // Labor Status
        [ObservableProperty]
        private int? _cervicalDilationAtReferral;

        [ObservableProperty]
        private bool _membranesRuptured;

        [ObservableProperty]
        private string _liquorColor = "Clear";

        // Interventions
        [ObservableProperty]
        private string _interventionsPerformed = string.Empty;

        [ObservableProperty]
        private string _medicationsGiven = string.Empty;

        [ObservableProperty]
        private string _ivFluidsGiven = string.Empty;

        [ObservableProperty]
        private bool _bloodSamplesTaken;

        // Transport
        [ObservableProperty]
        private TransportMode _transportMode = TransportMode.Ambulance;

        [ObservableProperty]
        private string _transportDetails = string.Empty;

        [ObservableProperty]
        private bool _skillfulAttendantAccompanying = true;

        [ObservableProperty]
        private string _accompanyingStaffName = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        // Lists for pickers
        public List<ReferralType> ReferralTypeOptions => Enum.GetValues(typeof(ReferralType)).Cast<ReferralType>().ToList();
        public List<ReferralUrgency> UrgencyOptions => Enum.GetValues(typeof(ReferralUrgency)).Cast<ReferralUrgency>().ToList();
        public List<TransportMode> TransportModeOptions => Enum.GetValues(typeof(TransportMode)).Cast<TransportMode>().ToList();

        partial void OnPartographIdChanged(Guid? value)
        {
            if (value.HasValue)
            {
                Task.Run(() => LoadPartographAsync());
            }
        }

        private async Task LoadPartographAsync()
        {
            try
            {
                IsBusy = true;

                Partograph = await _partographRepository.GetCurrentPartographAsync(PartographId);
                if (Partograph == null)
                {
                    await AppShell.DisplayToastAsync("Failed to load partograph");
                    return;
                }

                //Patient = await _patientRepository.GetItemAsync(Partograph.PatientID);

                // Pre-fill referring facility from staff info
                ReferringFacilityName = Constants.Staff?.FacilityName ?? string.Empty;
                ReferringPhysician = Constants.Staff?.Name ?? string.Empty;
                //ReferringPhysicianContact = Constants.Staff?.PhoneNumber ?? string.Empty;

                // Pre-fill some maternal condition from latest readings
                await PreFillMaternalConditionAsync();
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

        private async Task PreFillMaternalConditionAsync()
        {
            if (Partograph == null) return;

            try
            {
                var bpRepo = new BPRepository(Microsoft.Extensions.Logging.Abstractions.NullLogger<BPRepository>.Instance);
                var tempRepo = new TemperatureRepository(Microsoft.Extensions.Logging.Abstractions.NullLogger<TemperatureRepository>.Instance);
                var fhrRepo = new FHRRepository(Microsoft.Extensions.Logging.Abstractions.NullLogger<FHRRepository>.Instance);
                var dilationRepo = new CervixDilatationRepository(Microsoft.Extensions.Logging.Abstractions.NullLogger<CervixDilatationRepository>.Instance);

                var latestBP = await bpRepo.GetLatestByPatientAsync(Partograph.ID);
                if (latestBP != null)
                {
                    MaternalPulse = latestBP.Pulse;
                    MaternalBPSystolic = latestBP.Systolic;
                    MaternalBPDiastolic = latestBP.Diastolic;
                }

                var latestTemp = await tempRepo.GetLatestByPatientAsync(Partograph.ID);
                if (latestTemp != null)
                {
                    MaternalTemperature = (decimal) (latestTemp?.TemperatureCelsius ?? 0);
                }

                var latestFHR = await fhrRepo.GetLatestByPatientAsync(Partograph.ID);
                if (latestFHR != null)
                {
                    FetalHeartRate = latestFHR.Rate;
                }

                var latestDilation = await dilationRepo.GetLatestByPatientAsync(Partograph.ID);
                if (latestDilation != null)
                {
                    CervicalDilationAtReferral = latestDilation.DilatationCm;
                }

                MembranesRuptured = Partograph.MembraneStatus == "Ruptured";
                LiquorColor = Partograph.LiquorStatus;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not pre-fill maternal condition");
            }
        }

        [RelayCommand]
        private async Task SaveAndGenerateReferralFormAsync()
        {
            if (!ValidateInput())
            {
                return;
            }

            try
            {
                IsBusy = true;

                var referral = CreateReferralFromForm();
                var result = await _referralRepository.SaveItemAsync(referral);

                if (result != null)
                {
                    await AppShell.DisplayToastAsync("Referral saved successfully");

                    // Generate referral form/letter
                    var shouldGenerate = await Application.Current.MainPage.DisplayAlert(
                        "Generate Referral Form",
                        "Would you like to generate a referral form PDF?",
                        "Yes", "No");

                    if (shouldGenerate)
                    {
                        await GenerateReferralPDFAsync(referral);
                    }

                    // Navigate back
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await AppShell.DisplayToastAsync("Failed to save referral");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving referral");
                await AppShell.DisplayToastAsync($"Error saving: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private Referral CreateReferralFromForm()
        {
            return new Referral
            {
                ID = Guid.NewGuid(),
                PartographID = PartographId,
                ReferralTime = ReferralTime,
                ReferralType = ReferralType,
                Urgency = Urgency,
                ReferringFacilityName = ReferringFacilityName,
                ReferringFacilityLevel = ReferringFacilityLevel,
                ReferringPhysician = ReferringPhysician,
                ReferringPhysicianContact = ReferringPhysicianContact,
                DestinationFacilityName = DestinationFacilityName,
                DestinationFacilityLevel = DestinationFacilityLevel,
                DestinationFacilityContact = DestinationFacilityContact,
                DestinationAddress = DestinationAddress,
                DestinationNotified = DestinationNotified,
                ProlongedLabor = ProlongedLabor,
                ObstructedLabor = ObstructedLabor,
                FoetalDistress = FoetalDistress,
                AntepartumHemorrhage = AntepartumHemorrhage,
                PostpartumHemorrhage = PostpartumHemorrhage,
                SeverePreeclampsia = SeverePreeclampsia,
                Eclampsia = Eclampsia,
                SepticShock = SepticShock,
                RupturedUterus = RupturedUterus,
                AbnormalPresentation = AbnormalPresentation,
                CordProlapse = CordProlapse,
                NeonatalAsphyxia = NeonatalAsphyxia,
                PrematurityComplications = PrematurityComplications,
                LowBirthWeight = LowBirthWeight,
                RespiratoryDistress = RespiratoryDistress,
                CongenitalAbnormalities = CongenitalAbnormalities,
                NeonatalSepsis = NeonatalSepsis,
                LackOfResources = LackOfResources,
                RequiresCaesareanSection = RequiresCaesareanSection,
                RequiresBloodTransfusion = RequiresBloodTransfusion,
                RequiresSpecializedCare = RequiresSpecializedCare,
                OtherReasons = OtherReasons,
                PrimaryDiagnosis = PrimaryDiagnosis,
                ClinicalSummary = ClinicalSummary,
                MaternalCondition = MaternalCondition,
                MaternalPulse = MaternalPulse,
                MaternalBPSystolic = MaternalBPSystolic,
                MaternalBPDiastolic = MaternalBPDiastolic,
                MaternalTemperature = MaternalTemperature,
                MaternalConsciousness = MaternalConsciousness,
                FetalHeartRate = FetalHeartRate,
                FetalCondition = FetalCondition,
                NumberOfBabiesBeingReferred = NumberOfBabiesBeingReferred,
                NeonatalCondition = NeonatalCondition,
                CervicalDilationAtReferral = CervicalDilationAtReferral,
                MembranesRuptured = MembranesRuptured,
                LiquorColor = LiquorColor,
                InterventionsPerformed = InterventionsPerformed,
                MedicationsGiven = MedicationsGiven,
                IVFluidsGiven = IvFluidsGiven,
                BloodSamplesTaken = BloodSamplesTaken,
                TransportMode = TransportMode,
                TransportDetails = TransportDetails,
                SkillfulAttendantAccompanying = SkillfulAttendantAccompanying,
                AccompanyingStaffName = AccompanyingStaffName,
                PartographSent = true,
                Status = ReferralStatus.Pending,
                HandlerName = Constants.Staff?.Name ?? string.Empty,
                Handler = Constants.Staff?.ID,
                Notes = Notes,
                DeviceId = DeviceIdentity.GetOrCreateDeviceId(),
                OriginDeviceId = DeviceIdentity.GetOrCreateDeviceId()
            };
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(DestinationFacilityName))
            {
                Application.Current.MainPage.DisplayAlert("Validation Error", "Please specify destination facility", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(PrimaryDiagnosis))
            {
                Application.Current.MainPage.DisplayAlert("Validation Error", "Please specify primary diagnosis for referral", "OK");
                return false;
            }

            return true;
        }

        private async Task GenerateReferralPDFAsync(Referral referral)
        {
            try
            {
                await AppShell.DisplayToastAsync("Generating referral form...");

                // Create referral summary
                var summary = GenerateReferralSummary(referral);

                // Display summary (in a real implementation, this would generate a PDF)
                await Application.Current.MainPage.DisplayAlert(
                    "Referral Form",
                    summary,
                    "OK");

                await AppShell.DisplayToastAsync("Referral form generated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating referral PDF");
                await AppShell.DisplayToastAsync("Error generating referral form");
            }
        }

        private string GenerateReferralSummary(Referral referral)
        {
            var summary = $@"
MATERNAL REFERRAL FORM

Date/Time: {referral.ReferralTime:dd MMM yyyy HH:mm}
Urgency: {referral.Urgency}

PATIENT INFORMATION
Name: {Patient?.Name}
Hospital Number: {Patient?.HospitalNumber}
Age: {Patient?.Age} years
Gravida/Parity: G{Partograph?.Gravida}P{Partograph?.Parity}

REFERRING FACILITY
{referral.ReferringFacilityName}
Physician: {referral.ReferringPhysician}
Contact: {referral.ReferringPhysicianContact}

DESTINATION FACILITY
{referral.DestinationFacilityName}
{referral.DestinationAddress}
Contact: {referral.DestinationFacilityContact}

PRIMARY DIAGNOSIS
{referral.PrimaryDiagnosis}

CLINICAL SUMMARY
{referral.ClinicalSummary}

REASONS FOR REFERRAL
{string.Join(", ", referral.ReferralReasons)}

MATERNAL CONDITION
Pulse: {referral.MaternalPulse} bpm
BP: {referral.MaternalBPSystolic}/{referral.MaternalBPDiastolic} mmHg
Temperature: {referral.MaternalTemperature}°C
Consciousness: {referral.MaternalConsciousness}

LABOR STATUS
Cervical Dilation: {referral.CervicalDilationAtReferral} cm
Membranes: {(referral.MembranesRuptured ? "Ruptured" : "Intact")}
Liquor: {referral.LiquorColor}

INTERVENTIONS PERFORMED
{referral.InterventionsPerformed}

MEDICATIONS GIVEN
{referral.MedicationsGiven}

TRANSPORT
Mode: {referral.TransportMode}
Skilled Attendant: {(referral.SkillfulAttendantAccompanying ? "Yes" : "No")}

Referring Physician Signature: _______________
Date: {DateTime.Now:dd MMM yyyy}
";
            return summary;
        }
    }
}
