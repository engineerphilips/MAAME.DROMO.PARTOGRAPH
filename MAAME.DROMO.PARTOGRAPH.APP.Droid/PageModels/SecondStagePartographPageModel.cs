using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Helpers;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class SecondStagePartographPageModel : ObservableObject, IQueryAttributable
    {
        public Partograph? Patient { get; set; }
        private readonly PatientRepository _patientRepository;
        private readonly PartographRepository _partographRepository;
        private readonly ModalErrorHandler _errorHandler;
        private readonly CompanionRepository _companionRepository;
        private readonly PainReliefRepository _painReliefRepository;
        private readonly OralFluidRepository _oralFluidRepository;
        private readonly PostureRepository _postureRepository;
        private readonly FHRRepository _fhrRepository;
        private readonly TemperatureRepository _temperatureRepository;
        private readonly UrineRepository _urineRepository;
        private readonly OxytocinRepository _oxytocinRepository;
        private readonly MedicationEntryRepository _medicationEntryRepository;
        private readonly IVFluidEntryRepository _ivFluidEntryRepository;
        private readonly ContractionRepository _contractionRepository;
        private readonly HeadDescentRepository _headDescentRepository;
        private readonly FetalPositionRepository _fetalPositionRepository;
        private readonly AmnioticFluidRepository _amnioticFluidRepository;
        private readonly CaputRepository _caputRepository;
        private readonly BPRepository _bpRepository;
        private readonly AssessmentRepository _assessmentRepository;
        private readonly PlanRepository _planRepository;
        private readonly BirthOutcomeRepository _birthOutcomeRepository;
        private readonly BabyDetailsRepository _babyDetailsRepository;

        // Services
        private readonly PartographNotesService _notesService;

        // Modal page models
        private readonly CompanionModalPageModel _companionModalPageModel;
        private readonly PainReliefModalPageModel _painReliefModalPageModel;
        private readonly OralFluidModalPageModel _oralFluidModalPageModel;
        private readonly PostureModalPageModel _postureModalPageModel;
        private readonly FetalPositionModalPageModel _fetalPositionModalPageModel;
        private readonly AmnioticFluidModalPageModel _amnioticFluidModalPageModel;
        private readonly CaputModalPageModel _caputModalPageModel;
        private readonly UrineModalPageModel _urineModalPageModel;
        private readonly TemperatureModalPageModel _temperatureModalPageModel;
        private readonly OxytocinModalPageModel _oxytocinModalPageModel;
        private readonly MedicationModalPageModel _medicationModalPageModel;
        private readonly IVFluidModalPageModel _ivFluidModalPageModel;
        private readonly HeadDescentModalPageModel _headDescentModalPageModel;
        private readonly BPPulseModalPageModel _bpPulseModalPageModel;
        private readonly FHRContractionModalPageModel _fHRContractionModalPageModel;
        private readonly PlanModalPageModel _planModalPageModel;
        private readonly AssessmentModalPageModel _assessmentModalPageModel;

        public CompanionModalPageModel CompanionModalPageModel => _companionModalPageModel;
        public PainReliefModalPageModel PainReliefModalPageModel => _painReliefModalPageModel;
        public OralFluidModalPageModel OralFluidModalPageModel => _oralFluidModalPageModel;
        public PostureModalPageModel PostureModalPageModel => _postureModalPageModel;
        public FetalPositionModalPageModel FetalPositionModalPageModel => _fetalPositionModalPageModel;
        public AmnioticFluidModalPageModel AmnioticFluidModalPageModel => _amnioticFluidModalPageModel;
        public CaputModalPageModel CaputModalPageModel => _caputModalPageModel;
        public UrineModalPageModel UrineModalPageModel => _urineModalPageModel;
        public TemperatureModalPageModel TemperatureModalPageModel => _temperatureModalPageModel;
        public OxytocinModalPageModel OxytocinModalPageModel => _oxytocinModalPageModel;
        public MedicationModalPageModel MedicationModalPageModel => _medicationModalPageModel;
        public IVFluidModalPageModel IVFluidModalPageModel => _ivFluidModalPageModel;
        public HeadDescentModalPageModel HeadDescentModalPageModel => _headDescentModalPageModel;
        public BPPulseModalPageModel BPPulseModalPageModel => _bpPulseModalPageModel;
        public FHRContractionModalPageModel FHRContractionModalPageModel => _fHRContractionModalPageModel;
        public PlanModalPageModel PlanModalPageModel => _planModalPageModel;
        public AssessmentModalPageModel AssessmentModalPageModel => _assessmentModalPageModel;

        [ObservableProperty]
        private DateTime _startTime;

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private string _patientInfo = string.Empty;

        [ObservableProperty]
        private string _secondStageDuration = string.Empty;

        [ObservableProperty]
        private DateTime? _lastRecordedTime;

        [ObservableProperty]
        bool _isBusy;

        // Measurement Status Properties
        [ObservableProperty]
        private string _fhrLatestValue = string.Empty;

        [ObservableProperty]
        private string _fhrStatusText = string.Empty;

        [ObservableProperty]
        private string _fhrButtonColor = "LightGray";

        [ObservableProperty]
        private string _contractionLatestValue = string.Empty;

        [ObservableProperty]
        private string _contractionStatusText = string.Empty;

        [ObservableProperty]
        private string _contractionButtonColor = "LightGray";

        [ObservableProperty]
        private string _bpLatestValue = string.Empty;

        [ObservableProperty]
        private string _bpStatusText = string.Empty;

        [ObservableProperty]
        private string _bpButtonColor = "LightGray";

        [ObservableProperty]
        private string _temperatureLatestValue = string.Empty;

        [ObservableProperty]
        private string _temperatureStatusText = string.Empty;

        [ObservableProperty]
        private string _temperatureButtonColor = "LightGray";

        [ObservableProperty]
        private string _urineLatestValue = string.Empty;

        [ObservableProperty]
        private string _urineStatusText = string.Empty;

        [ObservableProperty]
        private string _urineButtonColor = "LightGray";

        [ObservableProperty]
        private string _headDescentLatestValue = string.Empty;

        [ObservableProperty]
        private string _headDescentStatusText = string.Empty;

        [ObservableProperty]
        private string _headDescentButtonColor = "LightGray";

        [ObservableProperty]
        private string _assessmentLatestValue = string.Empty;

        [ObservableProperty]
        private string _assessmentStatusText = string.Empty;

        [ObservableProperty]
        private string _assessmentButtonColor = "LightGray";

        [ObservableProperty]
        private string _companionLatestValue = string.Empty;

        [ObservableProperty]
        private string _companionStatusText = string.Empty;

        [ObservableProperty]
        private string _painReliefLatestValue = string.Empty;

        [ObservableProperty]
        private string _painReliefStatusText = string.Empty;

        [ObservableProperty]
        private string _oralFluidLatestValue = string.Empty;

        [ObservableProperty]
        private string _oralFluidStatusText = string.Empty;

        [ObservableProperty]
        private string _postureLatestValue = string.Empty;

        [ObservableProperty]
        private string _postureStatusText = string.Empty;

        [ObservableProperty]
        private string _amnioticFluidLatestValue = string.Empty;

        [ObservableProperty]
        private string _amnioticFluidStatusText = string.Empty;

        [ObservableProperty]
        private string _fetalPositionLatestValue = string.Empty;

        [ObservableProperty]
        private string _fetalPositionStatusText = string.Empty;

        [ObservableProperty]
        private string _caputLatestValue = string.Empty;

        [ObservableProperty]
        private string _caputStatusText = string.Empty;

        [ObservableProperty]
        private string _oxytocinLatestValue = string.Empty;

        [ObservableProperty]
        private string _oxytocinStatusText = string.Empty;

        [ObservableProperty]
        private string _medicationLatestValue = string.Empty;

        [ObservableProperty]
        private string _medicationStatusText = string.Empty;

        [ObservableProperty]
        private string _ivFluidLatestValue = string.Empty;

        [ObservableProperty]
        private string _ivFluidStatusText = string.Empty;

        [ObservableProperty]
        private string _planLatestValue = string.Empty;

        [ObservableProperty]
        private string _planStatusText = string.Empty;

        [ObservableProperty]
        private string _companionText;

        [ObservableProperty]
        private int _companionCurrentIndex = 0;

        [ObservableProperty]
        private ObservableCollection<BabyDetails> _babies = new();

        // Clinical Notes Properties
        [ObservableProperty]
        private string _clinicalNotes = string.Empty;

        [ObservableProperty]
        private DateTime? _notesGeneratedTime;

        public SecondStagePartographPageModel(
            PatientRepository patientRepository,
            PartographRepository partographRepository,
            CompanionRepository companionRepository,
            PainReliefRepository painReliefRepository,
            OralFluidRepository oralFluidRepository,
            PostureRepository postureRepository,
            FHRRepository fhrRepository,
            TemperatureRepository temperatureRepository,
            UrineRepository urineRepository,
            OxytocinRepository oxytocinRepository,
            MedicationEntryRepository medicationEntryRepository,
            IVFluidEntryRepository ivFluidEntryRepository,
            ContractionRepository contractionRepository,
            HeadDescentRepository headDescentRepository,
            FetalPositionRepository fetalPositionRepository,
            AmnioticFluidRepository amnioticFluidRepository,
            CaputRepository caputRepository,
            BPRepository bpRepository,
            AssessmentRepository assessmentRepository,
            PlanRepository planRepository,
            BirthOutcomeRepository birthOutcomeRepository,
            BabyDetailsRepository babyDetailsRepository,
            ModalErrorHandler errorHandler,
            CompanionModalPageModel companionModalPageModel,
            PainReliefModalPageModel painReliefModalPageModel,
            OralFluidModalPageModel oralFluidModalPageModel,
            PostureModalPageModel postureModalPageModel,
            FetalPositionModalPageModel fetalPositionModalPageModel,
            AmnioticFluidModalPageModel amnioticFluidModalPageModel,
            CaputModalPageModel caputModalPageModel,
            UrineModalPageModel urineModalPageModel,
            TemperatureModalPageModel temperatureModalPageModel,
            OxytocinModalPageModel oxytocinModalPageModel,
            MedicationModalPageModel medicationModalPageModel,
            IVFluidModalPageModel ivFluidModalPageModel,
            HeadDescentModalPageModel headDescentModalPageModel,
            BPPulseModalPageModel bpPulseModalPageModel,
            FHRContractionModalPageModel fHRContractionModalPageModel,
            AssessmentModalPageModel assessmentModalPageModel,
            PlanModalPageModel planModalPageModel,
            PartographNotesService notesService)
        {
            _patientRepository = patientRepository;
            _partographRepository = partographRepository;
            _companionRepository = companionRepository;
            _painReliefRepository = painReliefRepository;
            _oralFluidRepository = oralFluidRepository;
            _postureRepository = postureRepository;
            _fhrRepository = fhrRepository;
            _temperatureRepository = temperatureRepository;
            _urineRepository = urineRepository;
            _oxytocinRepository = oxytocinRepository;
            _medicationEntryRepository = medicationEntryRepository;
            _ivFluidEntryRepository = ivFluidEntryRepository;
            _contractionRepository = contractionRepository;
            _headDescentRepository = headDescentRepository;
            _fetalPositionRepository = fetalPositionRepository;
            _amnioticFluidRepository = amnioticFluidRepository;
            _caputRepository = caputRepository;
            _bpRepository = bpRepository;
            _assessmentRepository = assessmentRepository;
            _planRepository = planRepository;
            _birthOutcomeRepository = birthOutcomeRepository;
            _babyDetailsRepository = babyDetailsRepository;
            _errorHandler = errorHandler;
            _companionModalPageModel = companionModalPageModel;
            _painReliefModalPageModel = painReliefModalPageModel;
            _oralFluidModalPageModel = oralFluidModalPageModel;
            _postureModalPageModel = postureModalPageModel;
            _fetalPositionModalPageModel = fetalPositionModalPageModel;
            _amnioticFluidModalPageModel = amnioticFluidModalPageModel;
            _caputModalPageModel = caputModalPageModel;
            _urineModalPageModel = urineModalPageModel;
            _temperatureModalPageModel = temperatureModalPageModel;
            _oxytocinModalPageModel = oxytocinModalPageModel;
            _medicationModalPageModel = medicationModalPageModel;
            _ivFluidModalPageModel = ivFluidModalPageModel;
            _headDescentModalPageModel = headDescentModalPageModel;
            _bpPulseModalPageModel = bpPulseModalPageModel;
            _fHRContractionModalPageModel = fHRContractionModalPageModel;
            _assessmentModalPageModel = assessmentModalPageModel;
            _planModalPageModel = planModalPageModel;
            _notesService = notesService;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("patientId"))
            {
                Guid? id = Guid.Parse(Convert.ToString(query["patientId"]));
                LoadData(id).FireAndForgetSafeAsync(_errorHandler);
            }
        }

        private async Task<DateTime?> GetEarliestMeasurableTimeAsync()
        {
            if (Patient?.ID == null)
                return null;

            var earliestTimes = new List<DateTime>();
            var latestTimes = new List<DateTime>();

            try
            {
                // Retrieve all measurables and collect their times
                Patient.Companions = await _companionRepository.ListByPatientAsync(Patient.ID);
                if (Patient.Companions.Any())
                {
                    earliestTimes.Add(Patient.Companions.Min(e => e.Time));
                    latestTimes.Add(Patient.Companions.Max(e => e.Time));
                }

                Patient.PainReliefs = await _painReliefRepository.ListByPatientAsync(Patient.ID);
                if (Patient.PainReliefs.Any())
                {
                    earliestTimes.Add(Patient.PainReliefs.Min(e => e.Time));
                    latestTimes.Add(Patient.PainReliefs.Max(e => e.Time));
                }

                Patient.OralFluids = await _oralFluidRepository.ListByPatientAsync(Patient.ID);
                if (Patient.OralFluids.Any())
                {
                    earliestTimes.Add(Patient.OralFluids.Min(e => e.Time));
                    latestTimes.Add(Patient.OralFluids.Max(e => e.Time));
                }

                Patient.Postures = await _postureRepository.ListByPatientAsync(Patient.ID);
                if (Patient.Postures.Any())
                {
                    earliestTimes.Add(Patient.Postures.Min(e => e.Time));
                    latestTimes.Add(Patient.Postures.Max(e => e.Time));
                }

                Patient.Fhrs = await _fhrRepository.ListByPatientAsync(Patient.ID);
                if (Patient.Fhrs.Any())
                {
                    earliestTimes.Add(Patient.Fhrs.Min(e => e.Time));
                    latestTimes.Add(Patient.Fhrs.Max(e => e.Time));
                }

                Patient.Temperatures = await _temperatureRepository.ListByPatientAsync(Patient.ID);
                if (Patient.Temperatures.Any())
                {
                    earliestTimes.Add(Patient.Temperatures.Min(e => e.Time));
                    latestTimes.Add(Patient.Temperatures.Max(e => e.Time));
                }

                Patient.Urines = await _urineRepository.ListByPatientAsync(Patient.ID);
                if (Patient.Urines.Any())
                {
                    earliestTimes.Add(Patient.Urines.Min(e => e.Time));
                    latestTimes.Add(Patient.Urines.Max(e => e.Time));
                }

                Patient.Oxytocins = await _oxytocinRepository.ListByPatientAsync(Patient.ID);
                if (Patient.Oxytocins.Any())
                {
                    earliestTimes.Add(Patient.Oxytocins.Min(e => e.Time));
                    latestTimes.Add(Patient.Oxytocins.Max(e => e.Time));
                }

                Patient.Medications = await _medicationEntryRepository.ListByPatientAsync(Patient.ID);
                if (Patient.Medications.Any())
                {
                    earliestTimes.Add(Patient.Medications.Min(e => e.Time));
                    latestTimes.Add(Patient.Medications.Max(e => e.Time));
                }

                Patient.IVFluids = await _ivFluidEntryRepository.ListByPatientAsync(Patient.ID);
                if (Patient.IVFluids.Any())
                {
                    earliestTimes.Add(Patient.IVFluids.Min(e => e.Time));
                    latestTimes.Add(Patient.IVFluids.Max(e => e.Time));
                }

                Patient.Contractions = await _contractionRepository.ListByPatientAsync(Patient.ID);
                if (Patient.Contractions.Any())
                {
                    earliestTimes.Add(Patient.Contractions.Min(e => e.Time));
                    latestTimes.Add(Patient.Contractions.Max(e => e.Time));
                }

                Patient.HeadDescents = await _headDescentRepository.ListByPatientAsync(Patient.ID);
                if (Patient.HeadDescents.Any())
                    earliestTimes.Add(Patient.HeadDescents.Min(e => e.Time));

                Patient.FetalPositions = await _fetalPositionRepository.ListByPatientAsync(Patient.ID);
                if (Patient.FetalPositions.Any())
                {
                    earliestTimes.Add(Patient.FetalPositions.Min(e => e.Time));
                    latestTimes.Add(Patient.FetalPositions.Max(e => e.Time));
                }

                Patient.AmnioticFluids = await _amnioticFluidRepository.ListByPatientAsync(Patient.ID);
                if (Patient.AmnioticFluids.Any())
                {
                    earliestTimes.Add(Patient.AmnioticFluids.Min(e => e.Time));
                    latestTimes.Add(Patient.AmnioticFluids.Max(e => e.Time));
                }

                Patient.Caputs = await _caputRepository.ListByPatientAsync(Patient.ID);
                if (Patient.Caputs.Any())
                {
                    earliestTimes.Add(Patient.Caputs.Min(e => e.Time));
                    latestTimes.Add(Patient.Caputs.Max(e => e.Time));
                }

                Patient.BPs = await _bpRepository.ListByPatientAsync(Patient.ID);
                if (Patient.BPs.Any())
                {
                    earliestTimes.Add(Patient.BPs.Min(e => e.Time));
                    latestTimes.Add(Patient.BPs.Max(e => e.Time));
                }

                Patient.Assessments = await _assessmentRepository.ListByPatientAsync(Patient.ID);
                if (Patient.Assessments.Any())
                {
                    earliestTimes.Add(Patient.Assessments.Min(e => e.Time));
                    latestTimes.Add(Patient.Assessments.Max(e => e.Time));
                }

                Patient.Plans = await _planRepository.ListByPatientAsync(Patient.ID);
                if (Patient.Plans.Any())
                {
                    earliestTimes.Add(Patient.Plans.Min(e => e.Time));
                    latestTimes.Add(Patient.Plans.Max(e => e.Time));
                }

                if (latestTimes.Any())
                    LastRecordedTime = latestTimes.Max();

                // Return the earliest time among all measurables
                if (earliestTimes.Any())
                    return earliestTimes.Min();
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }

            return DateTime.Now;
        }

        private void UpdateMeasurementStatuses()
        {
            if (Patient == null)
                return;

            // FHR Status
            var latestFHR = Patient.Fhrs?.OrderByDescending(e => e.Time).FirstOrDefault();
            var fhrStatus = MeasurementStatusHelper.CalculateStatus<FHR>(
                latestFHR?.Time,
                latestFHR != null ? $"{latestFHR.Rate} bpm" : "");
            FhrLatestValue = fhrStatus.LatestValue;
            FhrStatusText = fhrStatus.DueStatusText;
            FhrButtonColor = fhrStatus.ButtonColor;

            // Contraction Status
            var latestContraction = Patient.Contractions?.OrderByDescending(e => e.Time).FirstOrDefault();
            var contractionStatus = MeasurementStatusHelper.CalculateStatus<Contraction>(
                latestContraction?.Time,
                latestContraction != null ? $"{latestContraction.FrequencyPer10Min}/10min, {latestContraction.DurationSeconds}s" : "");
            ContractionLatestValue = contractionStatus.LatestValue;
            ContractionStatusText = contractionStatus.DueStatusText;
            ContractionButtonColor = contractionStatus.ButtonColor;

            // BP/Pulse Status
            var latestBP = Patient.BPs?.OrderByDescending(e => e.Time).FirstOrDefault();
            var bpStatus = MeasurementStatusHelper.CalculateStatus<BP>(
                latestBP?.Time,
                latestBP != null ? $"{latestBP.Systolic}/{latestBP.Diastolic}, P:{latestBP.Pulse}" : "");
            BpLatestValue = bpStatus.LatestValue;
            BpStatusText = bpStatus.DueStatusText;
            BpButtonColor = bpStatus.ButtonColor;

            // Temperature Status
            var latestTemp = Patient.Temperatures?.OrderByDescending(e => e.Time).FirstOrDefault();
            var tempStatus = MeasurementStatusHelper.CalculateStatus<Temperature>(
                latestTemp?.Time,
                latestTemp != null ? $"{latestTemp.Rate:F1}°C" : "");
            TemperatureLatestValue = tempStatus.LatestValue;
            TemperatureStatusText = tempStatus.DueStatusText;
            TemperatureButtonColor = tempStatus.ButtonColor;

            // Urine Status
            var latestUrine = Patient.Urines?.OrderByDescending(e => e.Time).FirstOrDefault();
            if (latestUrine != null)
            {
                var urineStatus = MeasurementStatusHelper.CalculateStatus<Urine>(
                latestUrine?.Time,
                latestUrine != null ? $"P:{latestUrine.Protein}, A:{latestUrine.Acetone}" : "");
                UrineLatestValue = urineStatus.LatestValue;
                UrineStatusText = urineStatus.DueStatusText;
                UrineButtonColor = urineStatus.ButtonColor;
            }
            else
            {
                UrineLatestValue = string.Empty;
                UrineStatusText = string.Empty;
                UrineButtonColor = string.Empty;
            }

            // Head Descent Status
            var latestHeadDescent = Patient.HeadDescents?.OrderByDescending(e => e.Time).FirstOrDefault();
            var headDescentStatus = MeasurementStatusHelper.CalculateStatus<HeadDescent>(
                latestHeadDescent?.Time,
                latestHeadDescent != null ? $"Station {latestHeadDescent.Station}" : "");
            HeadDescentLatestValue = headDescentStatus.LatestValue;
            HeadDescentStatusText = headDescentStatus.DueStatusText;
            HeadDescentButtonColor = headDescentStatus.ButtonColor;

            // Assessment Status
            var latestAssessment = Patient.Assessments?.OrderByDescending(e => e.Time).FirstOrDefault();
            var assessmentStatus = MeasurementStatusHelper.CalculateStatus<Assessment>(
                latestAssessment?.Time,
                latestAssessment != null ? latestAssessment.Notes : "");
            AssessmentLatestValue = assessmentStatus.LatestValue;
            AssessmentStatusText = assessmentStatus.DueStatusText;
            AssessmentButtonColor = assessmentStatus.ButtonColor;

            // Companion (not scheduled, just latest)
            var latestCompanion = Patient.Companions?.OrderByDescending(e => e.Time).FirstOrDefault();
            CompanionLatestValue = latestCompanion?.CompanionDisplay ?? "";
            CompanionStatusText = latestCompanion != null ? $"Last: {latestCompanion.Time:HH:mm}, {MeasurementStatusHelper.FormatTimeSince(latestCompanion.Time - DateTime.Now)}" : "";

            // Pain Relief (not scheduled, just latest)
            var latestPainRelief = Patient.PainReliefs?.OrderByDescending(e => e.Time).FirstOrDefault();
            PainReliefLatestValue = latestPainRelief?.PainReliefDisplay ?? "";
            PainReliefStatusText = latestPainRelief != null ? $"Last: {latestPainRelief.Time:HH:mm}, {MeasurementStatusHelper.FormatTimeSince(latestPainRelief.Time - DateTime.Now)}" : "";

            // Oral Fluid (not scheduled, just latest)
            var latestOralFluid = Patient.OralFluids?.OrderByDescending(e => e.Time).FirstOrDefault();
            OralFluidLatestValue = latestOralFluid?.OralFluidDisplay ?? "";
            OralFluidStatusText = latestOralFluid != null ? $"Last: {latestOralFluid.Time:HH:mm}, {MeasurementStatusHelper.FormatTimeSince(latestOralFluid.Time - DateTime.Now)}" : "";

            // Posture (not scheduled, just latest)
            var latestPosture = Patient.Postures?.OrderByDescending(e => e.Time).FirstOrDefault();
            PostureLatestValue = latestPosture?.PostureDisplay ?? "";
            PostureStatusText = latestPosture != null ? $"Last: {latestPosture.Time:HH:mm}, {MeasurementStatusHelper.FormatTimeSince(latestPosture.Time - DateTime.Now)}" : "";

            // Amniotic Fluid (not scheduled, just latest)
            var latestAmnioticFluid = Patient.AmnioticFluids?.OrderByDescending(e => e.Time).FirstOrDefault();
            AmnioticFluidLatestValue = latestAmnioticFluid?.Color ?? "";
            AmnioticFluidStatusText = latestAmnioticFluid != null ? $"Last: {latestAmnioticFluid.Time:HH:mm}, {MeasurementStatusHelper.FormatTimeSince(latestAmnioticFluid.Time - DateTime.Now)}" : "";

            // Fetal Position (not scheduled, just latest)
            var latestFetalPosition = Patient.FetalPositions?.OrderByDescending(e => e.Time).FirstOrDefault();
            FetalPositionLatestValue = latestFetalPosition?.Position ?? "";
            FetalPositionStatusText = latestFetalPosition != null ? $"Last: {latestFetalPosition.Time:HH:mm}, {MeasurementStatusHelper.FormatTimeSince(latestFetalPosition.Time - DateTime.Now)}" : "";

            // Caput (not scheduled, just latest)
            var latestCaput = Patient.Caputs?.OrderByDescending(e => e.Time).FirstOrDefault();
            CaputLatestValue = latestCaput?.Degree ?? "";
            CaputStatusText = latestCaput != null ? $"Last: {latestCaput.Time:HH:mm}, {MeasurementStatusHelper.FormatTimeSince(latestCaput.Time - DateTime.Now)}" : "";

            // Oxytocin (not scheduled, just latest)
            var latestOxytocin = Patient.Oxytocins?.OrderByDescending(e => e.Time).FirstOrDefault();
            OxytocinLatestValue = latestOxytocin != null ? $"{latestOxytocin.DoseMUnitsPerMin} mU/min" : "";
            OxytocinStatusText = latestOxytocin != null ? $"Last: {latestOxytocin.Time:HH:mm}, {MeasurementStatusHelper.FormatTimeSince(latestOxytocin.Time - DateTime.Now)}" : "";

            // Medication (not scheduled, just latest)
            var latestMedication = Patient.Medications?.OrderByDescending(e => e.Time).FirstOrDefault();
            MedicationLatestValue = latestMedication?.MedicationName ?? "";
            MedicationStatusText = latestMedication != null ? $"Last: {latestMedication.Time:HH:mm}, {MeasurementStatusHelper.FormatTimeSince(latestMedication.Time - DateTime.Now)}" : "";

            // IV Fluid (not scheduled, just latest)
            var latestIVFluid = Patient.IVFluids?.OrderByDescending(e => e.Time).FirstOrDefault();
            IvFluidLatestValue = latestIVFluid != null ? $"{latestIVFluid.FluidType}, {latestIVFluid.VolumeInfused}ml" : "";
            IvFluidStatusText = latestIVFluid != null ? $"Last: {latestIVFluid.Time:HH:mm}, {MeasurementStatusHelper.FormatTimeSince(latestIVFluid.Time - DateTime.Now)}" : "";

            // Plan (not scheduled, just latest)
            var latestPlan = Patient.Plans?.OrderByDescending(e => e.Time).FirstOrDefault();
            PlanLatestValue = latestPlan?.Notes ?? "";
            PlanStatusText = latestPlan != null ? $"Last: {latestPlan.Time:HH:mm}, {MeasurementStatusHelper.FormatTimeSince(latestPlan.Time - DateTime.Now)}" : "";
        }

        private async Task LoadData(Guid? patientId)
        {
            try
            {
                IsBusy = true;

                Patient = await _partographRepository.GetAsync(patientId);
                if (Patient == null)
                {
                    _errorHandler.HandleError(new Exception($"Patient with id {patientId} not found."));
                    return;
                }

                PatientName = Patient.Name;
                PatientInfo = Patient.DisplayInfo;

                var birthOutcome = await _birthOutcomeRepository.GetByPartographIdAsync(Patient.ID);
                if (birthOutcome != null)
                    Patient.SecondStageStartTime = birthOutcome.DeliveryTime;

                // Calculate second stage duration
                if (Patient.SecondStageStartTime.HasValue)
                {
                    var duration = DateTime.Now - Patient.SecondStageStartTime.Value;
                    SecondStageDuration = $"{(int)duration.TotalHours}h {duration.Minutes}m";
                }

                // Get the earliest measurable time
                var earliestMeasurableTime = await GetEarliestMeasurableTimeAsync();

                // Set StartTime
                DateTime? newStartTime = null;
                if (Patient.SecondStageStartTime.HasValue)
                {
                    newStartTime = Patient.SecondStageStartTime.Value;
                }
                else if (earliestMeasurableTime.HasValue)
                {
                    newStartTime = earliestMeasurableTime.Value;
                }

                if (newStartTime.HasValue)
                {
                    StartTime = new DateTime(newStartTime.Value.Year, newStartTime.Value.Month,
                        newStartTime.Value.Day, newStartTime.Value.Hour, 0, 0);
                }

                CompanionCurrentIndex = 0;
                CompanionText = Patient?.Companions[CompanionCurrentIndex]?.CompanionDisplay ?? string.Empty;

                // Load babies for this partograph
                await LoadBabiesAsync();

                // Update all measurement statuses
                UpdateMeasurementStatuses();

                // Generate clinical notes
                GenerateClinicalNotes();
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadBabiesAsync()
        {
            try
            {
                if (Patient?.ID == null)
                    return;

                var babies = await _babyDetailsRepository.GetByPartographIdAsync(Patient.ID);
                Babies = new ObservableCollection<BabyDetails>(babies);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
                await AppShell.DisplayToastAsync($"Error loading babies: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ViewBabyDetails(BabyDetails baby)
        {
            if (baby == null || Patient?.ID == null)
                return;

            try
            {
                // Navigate to baby details page for viewing/editing
                var birthOutcome = await _birthOutcomeRepository.GetByPartographIdAsync(Patient.ID);
                if (birthOutcome == null)
                {
                    await AppShell.DisplayToastAsync("Birth outcome not found");
                    return;
                }

                var parameters = new Dictionary<string, object>
                {
                    { "PartographId", Patient.ID.ToString() },
                    { "BirthOutcomeId", birthOutcome.ID.ToString() },
                    { "NumberOfBabies", Babies.Count }
                };

                await Shell.Current.GoToAsync("BabyDetailsPage", parameters);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
                await AppShell.DisplayToastAsync("Failed to navigate to baby details");
            }
        }

        [RelayCommand]
        private async Task UpdateBabyVitals(BabyDetails baby)
        {
            if (baby == null)
                return;

            try
            {
                // Show a prompt to update APGAR and other vital signs
                string apgar1 = baby.Apgar1Min?.ToString() ?? "";
                string apgar5 = baby.Apgar5Min?.ToString() ?? "";
                //string apgar10 = baby.Apgar10Min?.ToString() ?? "";

                string result = await Application.Current.MainPage.DisplayPromptAsync(
                    $"Update Vitals - {baby.BabyTag}",
                    $"Enter APGAR scores (1min, 5min) separated by commas:\nCurrent: {apgar1}, {apgar5}",
                    initialValue: $"{apgar1},{apgar5}",
                    maxLength: 10,
                    keyboard: Keyboard.Numeric);

                if (!string.IsNullOrEmpty(result))
                {
                    var scores = result.Split(',');
                    if (scores.Length >= 2)
                    {
                        baby.Apgar1Min = int.TryParse(scores[0].Trim(), out int a1) ? a1 : baby.Apgar1Min;
                        baby.Apgar5Min = int.TryParse(scores[1].Trim(), out int a5) ? a5 : baby.Apgar5Min;
                        //if (scores.Length >= 3)
                        //{
                        //    baby.Apgar10Min = int.TryParse(scores[2].Trim(), out int a10) ? a10 : baby.Apgar10Min;
                        //}

                        await _babyDetailsRepository.SaveItemAsync(baby);
                        await LoadBabiesAsync();
                        await AppShell.DisplayToastAsync($"Updated vitals for {baby.BabyTag}");
                    }
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
                await AppShell.DisplayToastAsync("Failed to update baby vitals");
            }
        }

        [RelayCommand]
        private async Task Print()
        {
            await AppShell.DisplayToastAsync("Second stage partograph printing feature coming soon");
        }

        [RelayCommand]
        private async Task NavigateToBirthOutcome()
        {
            if (Patient?.ID == null)
            {
                await AppShell.DisplayToastAsync("No patient selected");
                return;
            }

            try
            {
                // Navigate to birth outcome page
                var parameters = new Dictionary<string, object>
                {
                    { "PartographId", Patient.ID.ToString() }
                };

                await Shell.Current.GoToAsync("BirthOutcomePage", parameters);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
                await AppShell.DisplayToastAsync("Failed to navigate to birth outcome page");
            }
        }

        [RelayCommand]
        private async Task NavigateToBabyDetails()
        {
            if (Patient?.ID == null)
            {
                await AppShell.DisplayToastAsync("No patient selected");
                return;
            }

            try
            {
                // Check if birth outcome exists
                var birthOutcome = await _birthOutcomeRepository.GetByPartographIdAsync(Patient.ID);
                if (birthOutcome == null)
                {
                    var shouldNavigate = await Application.Current.MainPage.DisplayAlert(
                        "Birth Outcome Required",
                        "Please record the birth outcome first before adding baby details.",
                        "Go to Birth Outcome", "Cancel");

                    if (shouldNavigate)
                    {
                        await NavigateToBirthOutcome();
                    }
                    return;
                }

                // Navigate to baby details page
                var parameters = new Dictionary<string, object>
                {
                    { "BirthOutcomeId", birthOutcome.ID.ToString() }
                };

                await Shell.Current.GoToAsync("BabyDetailsPage", parameters);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
                await AppShell.DisplayToastAsync("Failed to navigate to baby details page");
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            if (Patient != null)
                await LoadData(Patient.ID);
        }

        // Popup actions and commands (reusing from PartographPageModel)
        public Action? CloseCompanionModalPopup { get; set; }
        public Action? OpenCompanionModalPopup { get; set; }
        public Action? ClosePainReliefModalPopup { get; set; }
        public Action? OpenPainReliefModalPopup { get; set; }
        public Action? CloseOralFluidModalPopup { get; set; }
        public Action? OpenOralFluidModalPopup { get; set; }
        public Action? ClosePostureModalPopup { get; set; }
        public Action? OpenPostureModalPopup { get; set; }
        public Action? CloseFetalPositionModalPopup { get; set; }
        public Action? OpenFetalPositionModalPopup { get; set; }
        public Action? CloseAmnioticFluidModalPopup { get; set; }
        public Action? OpenAmnioticFluidModalPopup { get; set; }
        public Action? CloseCaputModalPopup { get; set; }
        public Action? OpenCaputModalPopup { get; set; }
        public Action? CloseUrineModalPopup { get; set; }
        public Action? OpenUrineModalPopup { get; set; }
        public Action? CloseTemperatureModalPopup { get; set; }
        public Action? OpenTemperatureModalPopup { get; set; }
        public Action? CloseBpPulseModalPopup { get; set; }
        public Action? OpenBpPulseModalPopup { get; set; }
        public Action? CloseMedicationModalPopup { get; set; }
        public Action? OpenMedicationModalPopup { get; set; }
        public Action? CloseIVFluidModalPopup { get; set; }
        public Action? OpenIVFluidModalPopup { get; set; }
        public Action? CloseOxytocinModalPopup { get; set; }
        public Action? OpenOxytocinModalPopup { get; set; }
        public Action? CloseHeadDescentModalPopup { get; set; }
        public Action? OpenHeadDescentModalPopup { get; set; }
        public Action? OpenFHRContractionModalPopup { get; set; }
        public Action? CloseFHRContractionModalPopup { get; set; }
        public Action? OpenAssessmentModalPopup { get; set; }
        public Action? CloseAssessmentModalPopup { get; set; }
        public Action? OpenPlanModalPopup { get; set; }
        public Action? ClosePlanModalPopup { get; set; }

        [RelayCommand]
        public async Task OpenCompanionPopup()
        {
            if (Patient?.ID != null)
            {
                CompanionModalPageModel._patient = Patient;
                CompanionModalPageModel.ClosePopup = () => CloseCompanionModalPopup?.Invoke();
                await CompanionModalPageModel.LoadPatient(Patient.ID);

                OpenCompanionModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private void OnCompanionCycle()
        {
            CompanionCurrentIndex = (CompanionCurrentIndex + 1) % Patient?.Companions.Count ?? 0;
            CompanionText = Patient?.Companions[CompanionCurrentIndex]?.CompanionDisplay ?? string.Empty;
        }

        [RelayCommand]
        private async Task OpenPainReliefPopup()
        {
            if (Patient?.ID != null)
            {
                _painReliefModalPageModel._patient = Patient;
                _painReliefModalPageModel.ClosePopup = () => ClosePainReliefModalPopup?.Invoke();
                await _painReliefModalPageModel.LoadPatient(Patient.ID);
                OpenPainReliefModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenOralFluidPopup()
        {
            if (Patient?.ID != null)
            {
                _oralFluidModalPageModel._patient = Patient;
                _oralFluidModalPageModel.ClosePopup = () => CloseOralFluidModalPopup?.Invoke();
                await _oralFluidModalPageModel.LoadPatient(Patient.ID);
                OpenOralFluidModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenPosturePopup()
        {
            if (Patient?.ID != null)
            {
                _postureModalPageModel._patient = Patient;
                _postureModalPageModel.ClosePopup = () => ClosePostureModalPopup?.Invoke();
                await _postureModalPageModel.LoadPatient(Patient.ID);
                OpenPostureModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenAmnioticFluidPopup()
        {
            if (Patient?.ID != null)
            {
                _amnioticFluidModalPageModel._patient = Patient;
                _amnioticFluidModalPageModel.ClosePopup = () => CloseAmnioticFluidModalPopup?.Invoke();
                await _amnioticFluidModalPageModel.LoadPatient(Patient.ID);
                OpenAmnioticFluidModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenFetalPositionPopup()
        {
            if (Patient?.ID != null)
            {
                _fetalPositionModalPageModel._patient = Patient;
                _fetalPositionModalPageModel.ClosePopup = () => CloseFetalPositionModalPopup?.Invoke();
                await _fetalPositionModalPageModel.LoadPatient(Patient.ID);
                OpenFetalPositionModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenCaputPopup()
        {
            if (Patient?.ID != null)
            {
                _caputModalPageModel._patient = Patient;
                _caputModalPageModel.ClosePopup = () => CloseCaputModalPopup?.Invoke();
                await _caputModalPageModel.LoadPatient(Patient.ID);
                OpenCaputModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenFHRContractionPopup()
        {
            if (Patient?.ID != null)
            {
                _fHRContractionModalPageModel._patient = Patient;
                _fHRContractionModalPageModel.ClosePopup = () => CloseFHRContractionModalPopup?.Invoke();
                await _fHRContractionModalPageModel.LoadPatient(Patient.ID);
                OpenFHRContractionModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenUrinePopup()
        {
            if (Patient?.ID != null)
            {
                _urineModalPageModel._patient = Patient;
                _urineModalPageModel.ClosePopup = () => CloseUrineModalPopup?.Invoke();
                await _urineModalPageModel.LoadPatient(Patient.ID);
                OpenUrineModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenTemperaturePopup()
        {
            if (Patient?.ID != null)
            {
                _temperatureModalPageModel._patient = Patient;
                _temperatureModalPageModel.ClosePopup = () => CloseTemperatureModalPopup?.Invoke();
                await _temperatureModalPageModel.LoadPatient(Patient.ID);
                OpenTemperatureModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenBpPulsePopup()
        {
            if (Patient?.ID != null)
            {
                _bpPulseModalPageModel._patient = Patient;
                _bpPulseModalPageModel.ClosePopup = () => CloseBpPulseModalPopup?.Invoke();
                await _bpPulseModalPageModel.LoadPatient(Patient.ID);
                OpenBpPulseModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenMedicinePopup()
        {
            if (Patient?.ID != null)
            {
                _medicationModalPageModel._patient = Patient;
                _medicationModalPageModel.ClosePopup = () => CloseMedicationModalPopup?.Invoke();
                await _medicationModalPageModel.LoadPatient(Patient.ID);
                OpenMedicationModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenIVFluidsPopup()
        {
            if (Patient?.ID != null)
            {
                _ivFluidModalPageModel._patient = Patient;
                _ivFluidModalPageModel.ClosePopup = () => CloseIVFluidModalPopup?.Invoke();
                await _ivFluidModalPageModel.LoadPatient(Patient.ID);
                OpenIVFluidModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenOxytocinPopup()
        {
            if (Patient?.ID != null)
            {
                _oxytocinModalPageModel._patient = Patient;
                _oxytocinModalPageModel.ClosePopup = () => CloseOxytocinModalPopup?.Invoke();
                await _oxytocinModalPageModel.LoadPatient(Patient.ID);
                OpenOxytocinModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenHeadDescentPopup()
        {
            if (Patient?.ID != null)
            {
                _headDescentModalPageModel._patient = Patient;
                _headDescentModalPageModel.ClosePopup = () => CloseHeadDescentModalPopup?.Invoke();
                await _headDescentModalPageModel.LoadPatient(Patient.ID);
                OpenHeadDescentModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenAssessmentPopup()
        {
            if (Patient?.ID != null)
            {
                _assessmentModalPageModel._patient = Patient;
                _assessmentModalPageModel.ClosePopup = () => CloseAssessmentModalPopup?.Invoke();
                await _assessmentModalPageModel.LoadPatient(Patient.ID);
                OpenAssessmentModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenPlanPopup()
        {
            if (Patient?.ID != null)
            {
                _planModalPageModel._patient = Patient;
                _planModalPageModel.ClosePopup = () => ClosePlanModalPopup?.Invoke();
                await _planModalPageModel.LoadPatient(Patient.ID);
                OpenPlanModalPopup?.Invoke();
            }
        }

        /// <summary>
        /// Refreshes the clinical notes by regenerating them from current partograph data
        /// </summary>
        [RelayCommand]
        private async Task RefreshNotes()
        {
            if (Patient == null)
                return;

            try
            {
                IsBusy = true;

                // Get the current patient details
                var patientDetails = await _patientRepository.GetItemAsync(Patient.PatientID);

                // Generate clinical notes
                ClinicalNotes = _notesService.GenerateClinicalNotes(Patient, patientDetails);
                NotesGeneratedTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleError(ex, "Error generating clinical notes");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Exports the clinical notes to a file or shares them
        /// </summary>
        [RelayCommand]
        private async Task ExportNotes()
        {
            if (string.IsNullOrWhiteSpace(ClinicalNotes))
            {
                await Application.Current.MainPage.DisplayAlert("Export Notes",
                    "No clinical notes available to export. Please refresh notes first.", "OK");
                return;
            }

            try
            {
                IsBusy = true;

                // Get patient name for filename
                var patientDetails = await _patientRepository.GetItemAsync(Patient.PatientID);
                var patientName = patientDetails != null ?
                    $"{patientDetails.FirstName}_{patientDetails.LastName}" : "Patient";
                var fileName = $"Clinical_Notes_SecondStage_{patientName}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                // Create file path in app's documents directory
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var filePath = Path.Combine(documentsPath, fileName);

                // Write notes to file
                await File.WriteAllTextAsync(filePath, ClinicalNotes);

                // Share the file
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Export Clinical Notes",
                    File = new ShareFile(filePath)
                });

                await Application.Current.MainPage.DisplayAlert("Export Successful",
                    $"Clinical notes exported to {fileName}", "OK");
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleError(ex, "Error exporting clinical notes");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Generates clinical notes when measurements are updated
        /// </summary>
        private async void GenerateClinicalNotes()
        {
            if (Patient == null)
                return;

            try
            {
                var patientDetails = await _patientRepository.GetItemAsync(Patient.PatientID);
                ClinicalNotes = _notesService.GenerateClinicalNotes(Patient, patientDetails);
                NotesGeneratedTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                // Silently log error - don't disrupt user workflow
                System.Diagnostics.Debug.WriteLine($"Error generating clinical notes: {ex.Message}");
            }
        }
    }
}
