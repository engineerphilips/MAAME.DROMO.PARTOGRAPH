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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input; 

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    public partial class PartographPageModel : ObservableObject, IQueryAttributable
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
        private readonly CervixDilatationRepository _cervixDilatationRepository;
        private readonly ContractionRepository _contractionRepository;
        private readonly HeadDescentRepository _headDescentRepository;
        private readonly FetalPositionRepository _fetalPositionRepository;
        private readonly AmnioticFluidRepository _amnioticFluidRepository;
        private readonly CaputRepository _caputRepository;
        private readonly MouldingRepository _mouldingRepository;
        private readonly BPRepository _bpRepository;
        private readonly AssessmentRepository _assessmentRepository;
        private readonly PlanRepository _planRepository;
        private readonly BishopScoreRepository _bishopScoreRepository;

        // Alert and Validation Services
        private readonly AlertEngine _alertEngine;
        private readonly MeasurementValidationService _validationService;
        private readonly PartographNotesService _notesService;
        private readonly IPartographPdfService _pdfService;

        // Modal page models
        private readonly CompanionModalPageModel _companionModalPageModel;
        private readonly PainReliefModalPageModel _painReliefModalPageModel;
        private readonly OralFluidModalPageModel _oralFluidModalPageModel;
        private readonly PostureModalPageModel _postureModalPageModel;
        private readonly FetalPositionModalPageModel _fetalPositionModalPageModel;
        private readonly AmnioticFluidModalPageModel _amnioticFluidModalPageModel;
        private readonly CaputModalPageModel _caputModalPageModel;
        private readonly MouldingModalPageModel _mouldingModalPageModel;
        private readonly UrineModalPageModel _urineModalPageModel;
        private readonly TemperatureModalPageModel _temperatureModalPageModel;
        private readonly OxytocinModalPageModel _oxytocinModalPageModel;
        private readonly MedicationModalPageModel _medicationModalPageModel;
        private readonly IVFluidModalPageModel _ivFluidModalPageModel;
        private readonly HeadDescentModalPageModel _headDescentModalPageModel;
        private readonly CervixDilatationModalPageModel _cervixDilatationModalPageModel;
        private readonly BPPulseModalPageModel _bpPulseModalPageModel;
        private readonly FHRContractionModalPageModel _fHRContractionModalPageModel;
        private readonly PlanModalPageModel _planModalPageModel;
        private readonly AssessmentModalPageModel _assessmentModalPageModel;
        private readonly BishopScorePopupPageModel _bishopScorePopupPageModel;

        public CompanionModalPageModel CompanionModalPageModel => _companionModalPageModel;
        public PainReliefModalPageModel PainReliefModalPageModel => _painReliefModalPageModel;
        public OralFluidModalPageModel OralFluidModalPageModel => _oralFluidModalPageModel;
        public PostureModalPageModel PostureModalPageModel => _postureModalPageModel;
        public FetalPositionModalPageModel FetalPositionModalPageModel => _fetalPositionModalPageModel;
        public AmnioticFluidModalPageModel AmnioticFluidModalPageModel => _amnioticFluidModalPageModel;
        public CaputModalPageModel CaputModalPageModel => _caputModalPageModel;
        public MouldingModalPageModel MouldingModalPageModel => _mouldingModalPageModel;
        public UrineModalPageModel UrineModalPageModel => _urineModalPageModel;
        public TemperatureModalPageModel TemperatureModalPageModel => _temperatureModalPageModel;
        public OxytocinModalPageModel OxytocinModalPageModel => _oxytocinModalPageModel;
        public MedicationModalPageModel MedicationModalPageModel => _medicationModalPageModel;
        public IVFluidModalPageModel IVFluidModalPageModel => _ivFluidModalPageModel;
        public HeadDescentModalPageModel HeadDescentModalPageModel => _headDescentModalPageModel;
        public CervixDilatationModalPageModel CervixDilatationModalPageModel => _cervixDilatationModalPageModel;
        public BPPulseModalPageModel BPPulseModalPageModel => _bpPulseModalPageModel;
        public FHRContractionModalPageModel FHRContractionModalPageModel => _fHRContractionModalPageModel;
        public PlanModalPageModel PlanModalPageModel => _planModalPageModel;
        public AssessmentModalPageModel AssessmentModalPageModel => _assessmentModalPageModel;
        public BishopScorePopupPageModel BishopScorePopupPageModel => _bishopScorePopupPageModel;

        //[ObservableProperty]
        //private ObservableCollection<EnhancedTimeSlotViewModel> _timeSlots = new ();

        [ObservableProperty]
        private DateTime _startTime; // = DateTime.Today.AddHours(6);

        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private string _patientInfo = string.Empty;

        [ObservableProperty]
        private string _laborDuration = string.Empty;

        [ObservableProperty]
        private DateTime? _lastRecordedTime;

        [ObservableProperty]
        private int _currentDilation;

        //[ObservableProperty]
        //private ObservableCollection<Partograph> _partographEntries = new();

        //[ObservableProperty]
        //private ObservableCollection<ChartDataPoint> _cervicalDilationData = new();

        //[ObservableProperty]
        //private ObservableCollection<ChartDataPoint> _fetalHeartRateData = new();

        //[ObservableProperty]
        //private ObservableCollection<ChartDataPoint> _contractionsData = new();

        //[ObservableProperty]
        //private ObservableCollection<ChartDataPoint> _alertLineData = new();

        //[ObservableProperty]
        //private ObservableCollection<ChartDataPoint> _actionLineData = new();

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
        private string _fhrDecelerationLatestValue = string.Empty;

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
        private string _cervixDilatationLatestValue = string.Empty;

        [ObservableProperty]
        private string _cervixDilatationStatusText = string.Empty;

        [ObservableProperty]
        private string _cervixDilatationButtonColor = "LightGray";

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
        private string _mouldingLatestValue = string.Empty;

        [ObservableProperty]
        private string _mouldingStatusText = string.Empty;

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

        // Bishop Score Properties
        [ObservableProperty]
        private string _bishopScoreButtonColor = "LightGray";

        [ObservableProperty]
        private string _bishopScoreLatestValue = string.Empty;

        [ObservableProperty]
        private string _bishopScoreInterpretation = string.Empty;

        [ObservableProperty]
        private string _bishopScoreNotes = string.Empty;

        [ObservableProperty]
        private string _bishopScoreTotalDisplay = "-";

        [ObservableProperty]
        private string _bishopScoreStatusColor = "#808080";

        // Alert Properties
        [ObservableProperty]
        private ObservableCollection<ClinicalAlert> _activeAlerts = new ObservableCollection<ClinicalAlert>();

        [ObservableProperty]
        private bool _hasAlerts = false;

        [ObservableProperty]
        private int _criticalAlertCount = 0;

        [ObservableProperty]
        private int _warningAlertCount = 0;

        [ObservableProperty]
        private string _alertSummary = string.Empty;

        // Clinical Notes Properties
        [ObservableProperty]
        private string _clinicalNotes = string.Empty;

        [ObservableProperty]
        private DateTime? _notesGeneratedTime;

        //[ObservableProperty]
        //private ObservableCollection<TimeSlots> _chartinghours;

        public PartographPageModel(PatientRepository patientRepository,
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
            CervixDilatationRepository cervixDilatationRepository,
            ContractionRepository contractionRepository,
            HeadDescentRepository headDescentRepository,
            FetalPositionRepository fetalPositionRepository,
            AmnioticFluidRepository amnioticFluidRepository,
            CaputRepository caputRepository,
            MouldingRepository mouldingRepository,
            BPRepository bpRepository,
            AssessmentRepository assessmentRepository,
            PlanRepository planRepository,
            BishopScoreRepository bishopScoreRepository,
            ModalErrorHandler errorHandler,
            CompanionModalPageModel companionModalPageModel,
            PainReliefModalPageModel painReliefModalPageModel,
            OralFluidModalPageModel oralFluidModalPageModel,
            PostureModalPageModel postureModalPageModel,
            FetalPositionModalPageModel fetalPositionModalPageModel,
            AmnioticFluidModalPageModel amnioticFluidModalPageModel,
            CaputModalPageModel caputModalPageModel,
            MouldingModalPageModel mouldingModalPageModel,
            UrineModalPageModel urineModalPageModel,
            TemperatureModalPageModel temperatureModalPageModel,
            OxytocinModalPageModel oxytocinModalPageModel,
            MedicationModalPageModel medicationModalPageModel,
            IVFluidModalPageModel ivFluidModalPageModel,
            HeadDescentModalPageModel headDescentModalPageModel,
            CervixDilatationModalPageModel cervixDilatationModalPageModel,
            BPPulseModalPageModel bpPulseModalPageModel,
            FHRContractionModalPageModel fHRContractionModalPageModel,
            AssessmentModalPageModel assessmentModalPageModel,
            PlanModalPageModel planModalPageModel,
            BishopScorePopupPageModel bishopScorePopupPageModel,
            PartographNotesService notesService,
            IPartographPdfService pdfService)
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
            _cervixDilatationRepository = cervixDilatationRepository;
            _contractionRepository = contractionRepository;
            _headDescentRepository = headDescentRepository;
            _fetalPositionRepository = fetalPositionRepository;
            _amnioticFluidRepository = amnioticFluidRepository;
            _caputRepository = caputRepository;
            _mouldingRepository = mouldingRepository;
            _bpRepository = bpRepository;
            _assessmentRepository = assessmentRepository;
            _planRepository = planRepository;
            _bishopScoreRepository = bishopScoreRepository;
            _errorHandler = errorHandler;
            _companionModalPageModel = companionModalPageModel;
            _painReliefModalPageModel = painReliefModalPageModel;
            _oralFluidModalPageModel = oralFluidModalPageModel;
            _postureModalPageModel = postureModalPageModel;
            _fetalPositionModalPageModel = fetalPositionModalPageModel;
            _amnioticFluidModalPageModel = amnioticFluidModalPageModel;
            _caputModalPageModel = caputModalPageModel;
            _mouldingModalPageModel = mouldingModalPageModel;
            _urineModalPageModel = urineModalPageModel;
            _temperatureModalPageModel = temperatureModalPageModel;
            _oxytocinModalPageModel = oxytocinModalPageModel;
            _medicationModalPageModel = medicationModalPageModel;
            _ivFluidModalPageModel = ivFluidModalPageModel;
            _headDescentModalPageModel = headDescentModalPageModel;
            _cervixDilatationModalPageModel = cervixDilatationModalPageModel;
            _bpPulseModalPageModel = bpPulseModalPageModel;
            _fHRContractionModalPageModel = fHRContractionModalPageModel;
            _assessmentModalPageModel = assessmentModalPageModel;
            _planModalPageModel = planModalPageModel;
            _bishopScorePopupPageModel = bishopScorePopupPageModel;

            // Initialize alert engine and validation service
            _alertEngine = new AlertEngine();
            _validationService = new MeasurementValidationService();
            _notesService = notesService;
            _pdfService = pdfService;

            // Subscribe to alert events
            _alertEngine.AlertTriggered += OnAlertTriggered;
            _alertEngine.AlertCleared += OnAlertCleared;

            //Chartinghours = new ObservableCollection<TimeSlots>();
            //TimeSlots = new ObservableCollection<EnhancedTimeSlotViewModel>();

            var tasks = new Task[1];
            tasks[0] = GenerateInitialTimeSlots();
            Task.WhenAny(tasks);
        }

        /// <summary>
        /// Event handler for when an alert is triggered
        /// </summary>
        private void OnAlertTriggered(object? sender, ClinicalAlert alert)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ActiveAlerts.Add(alert);
                UpdateAlertSummary();
            });
        }

        /// <summary>
        /// Event handler for when an alert is cleared
        /// </summary>
        private void OnAlertCleared(object? sender, Guid alertId)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var alert = ActiveAlerts.FirstOrDefault(a => a.Id == alertId);
                if (alert != null)
                {
                    ActiveAlerts.Remove(alert);
                    UpdateAlertSummary();
                }
            });
        }

        /// <summary>
        /// Updates alert summary counts and text
        /// </summary>
        private void UpdateAlertSummary()
        {
            HasAlerts = ActiveAlerts.Any();
            CriticalAlertCount = ActiveAlerts.Count(a => a.Severity == AlertSeverity.Critical);
            WarningAlertCount = ActiveAlerts.Count(a => a.Severity == AlertSeverity.Warning);

            if (!HasAlerts)
            {
                //AlertSummary = "No active alerts";
                AlertSummary = "";
            }
            else
            {
                var parts = new List<string>();
                if (CriticalAlertCount > 0)
                    parts.Add($"{CriticalAlertCount} critical");
                if (WarningAlertCount > 0)
                    parts.Add($"{WarningAlertCount} warning");

                AlertSummary = string.Join(", ", parts);
            }
        }

        /// <summary>
        /// Analyzes patient data and generates clinical alerts
        /// </summary>
        private void AnalyzeAndGenerateAlerts()
        {
            if (Patient == null)
                return;

            try
            {
                // Analyze patient and get alerts
                var alerts = _alertEngine.AnalyzePatient(Patient);

                // Update UI
                ActiveAlerts.Clear();
                foreach (var alert in alerts)
                {
                    ActiveAlerts.Add(alert);
                }

                UpdateAlertSummary();
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
        }

        /// <summary>
        /// Acknowledges an alert
        /// </summary>
        [RelayCommand]
        private void AcknowledgeAlert(ClinicalAlert alert)
        {
            if (alert != null)
            {
                _alertEngine.AcknowledgeAlert(alert.Id, "User"); // TODO: Use actual user name
                alert.IsAcknowledged = true;
                OnPropertyChanged(nameof(ActiveAlerts));
            }
        }

        /// <summary>
        /// Clears all acknowledged alerts
        /// </summary>
        [RelayCommand]
        private void ClearAcknowledgedAlerts()
        {
            _alertEngine.ClearAcknowledgedAlerts();
            var acknowledgedAlerts = ActiveAlerts.Where(a => a.IsAcknowledged).ToList();
            foreach (var alert in acknowledgedAlerts)
            {
                ActiveAlerts.Remove(alert);
            }
            UpdateAlertSummary();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("patientId"))
            {
                Guid? id = Guid.Parse(Convert.ToString(query["patientId"]));
                LoadData(id).FireAndForgetSafeAsync(_errorHandler);
            }
        }

        private async Task GenerateInitialTimeSlots()
        {
            StartTime = await GetEarliestMeasurableTimeAsync() ?? DateTime.Today;
        }

        //private async Task GenerateInitialTimeSlots()
        //{
        //    TimeSlots.Clear();
        //    Chartinghours.Clear();
        //    DateTime date;
        //    StartTime = await GetEarliestMeasurableTimeAsync() ?? DateTime.Today;

        //    date = new DateTime (StartTime.Year, StartTime.Month, StartTime.Day, StartTime.Hour, 0, 0);

        //    for (int i = 0; i < 12; i++)
        //    {
        //        var currentTime = StartTime.AddHours(i);
        //        var timeSlot = new EnhancedTimeSlotViewModel(currentTime, i + 1)
        //        {
        //            Companion = CompanionType.,
        //            OralFluid = OralFluidType.,
        //            PainRelief = PainReliefType.,
        //            Posture = PostureType., 

        //        };

        //        //timeSlot.DataChanged += OnTimeSlotDataChanged;
        //        TimeSlots.Add(timeSlot);
        //        Chartinghours.Add(new TimeSlots() { Id = i, Slot = date.AddHours(i) });
        //    }

        //    //var x = Chartinghours?.Count ?? 0;

        //    //if (TimeSlots.Any())
        //    //    RegenerateTimeSlots();
        //}

        //private void RegenerateTimeSlots()
        //{
        //    foreach (var time in TimeSlots)
        //    {
        //        var x = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
        //        var y = x.AddHours(1);
        //        if (time.Time >= x && time.Time < y)
        //        {
        //            time.Companion = CompanionType.Yes;
        //        }
        //        else
        //            time.Companion = CompanionType.;
        //    }
        //}

        //private void RegenerateTimeSlots()
        //{
        //    var existingData = TimeSlots.ToDictionary(ts => ts.Time, ts => ts.GetData());
        //    TimeSlots.Clear();

        //    for (int i = 0; i < existingData.Count; i++)
        //    {
        //        var currentTime = StartTime.AddHours(i);
        //        var timeSlot = new EnhancedTimeSlotViewModel(currentTime, i + 1);
        //        timeSlot.DataChanged += OnTimeSlotDataChanged;

        //        if (existingData.ContainsKey(currentTime))
        //        {
        //            timeSlot.LoadData(existingData[currentTime]);
        //        }

        //        TimeSlots.Add(timeSlot);
        //    }
        //}

        //[ObservableProperty]
        //private List<Partograph> _companions = new();
        //[ObservableProperty]
        //private List<Partograph> _painReliefs = new();
        //[ObservableProperty]
        //private List<Partograph> _oralFluids = new();
        //[ObservableProperty]
        //private List<Partograph> _postures = new();
        //[ObservableProperty]
        //private List<Partograph> _fhrs = new();
        //[ObservableProperty]
        //private List<Partograph> _temperatures = new();
        //[ObservableProperty]
        //private List<Partograph> _urines = new();
        //[ObservableProperty]
        //private List<Partograph> _bps = new();
        //[ObservableProperty]
        //private List<Partograph> _oxytocins = new();
        //[ObservableProperty]
        //private List<Partograph> _ivfluids = new();
        //[ObservableProperty]
        //private List<Partograph> _cervixDilatations = new();
        //[ObservableProperty]
        //private List<Partograph> _contractions = new();
        //[ObservableProperty]
        //private List<Partograph> _headDescents = new();
        //[ObservableProperty]
        //private List<Partograph> _cervixDilatation = new();

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

                Patient.Dilatations = await _cervixDilatationRepository.ListByPatientAsync(Patient.ID);
                if (Patient.Dilatations.Any())
                {
                    earliestTimes.Add(Patient.Dilatations.Min(e => e.Time));
                    latestTimes.Add(Patient.Dilatations.Max(e => e.Time));
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

                Patient.Mouldings = await _mouldingRepository.ListByPatientAsync(Patient.ID);
                if (Patient.Mouldings.Any())
                {
                    earliestTimes.Add(Patient.Mouldings.Min(e => e.Time));
                    latestTimes.Add(Patient.Mouldings.Max(e => e.Time));
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

                Patient.BishopScores = await _bishopScoreRepository.ListByPatientAsync(Patient.ID);
                if (Patient.BishopScores.Any())
                {
                    earliestTimes.Add(Patient.BishopScores.Min(e => e.Time));
                    latestTimes.Add(Patient.BishopScores.Max(e => e.Time));
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

        /// <summary>
        /// Formats deceleration type to abbreviated form
        /// </summary>
        private string FormatDecelerationType(string decelerationType)
        {
            if (string.IsNullOrEmpty(decelerationType))
                return string.Empty;

            switch (decelerationType.ToLower())
            {
                case "early":
                    return "Early/E";
                case "late":
                    return "Late/L";
                case "variable":
                    return "Variable/V";
                case "prolonged":
                    return "Prolonged/P";
                case "no":
                    return "No";
                default:
                    return decelerationType;
            }
        }

        /// <summary>
        /// Updates the status of all measurables with latest values and due times
        /// </summary>
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

            // FHR Deceleration Status
            if (latestFHR != null && !string.IsNullOrEmpty(latestFHR.Deceleration))
            {
                FhrDecelerationLatestValue = FormatDecelerationType(latestFHR.Deceleration);
            }
            else
            {
                FhrDecelerationLatestValue = string.Empty;
            }

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

            // Cervix Dilatation Status
            var latestDilatation = Patient.Dilatations?.OrderByDescending(e => e.Time).FirstOrDefault();
            var dilatationStatus = MeasurementStatusHelper.CalculateStatus<CervixDilatation>(
                latestDilatation?.Time,
                latestDilatation != null ? $"{latestDilatation.DilatationCm} cm" : "");
            CervixDilatationLatestValue = dilatationStatus.LatestValue;
            CervixDilatationStatusText = dilatationStatus.DueStatusText;
            CervixDilatationButtonColor = dilatationStatus.ButtonColor;

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

            // Moulding (not scheduled, just latest)
            var latestMoulding = Patient.Mouldings?.OrderByDescending(e => e.Time).FirstOrDefault();
            MouldingLatestValue = latestMoulding?.DegreeDisplay ?? "";
            MouldingStatusText = latestMoulding != null ? $"Last: {latestMoulding.Time:HH:mm}" : "";

            // Oxytocin (not scheduled, just latest)
            var latestOxytocin = Patient.Oxytocins?.OrderByDescending(e => e.Time).FirstOrDefault();
            OxytocinLatestValue = latestOxytocin != null ? $"{latestOxytocin.DoseMUnitsPerMin} mU/min" : "";
            OxytocinStatusText = latestOxytocin != null ? $"Last: {latestOxytocin.Time:HH:mm}, {MeasurementStatusHelper.FormatTimeSince(latestOxytocin.Time - DateTime.Now)}" : "";

            // Medication (not scheduled, just latest)
            var latestMedication = Patient.Medications?.OrderByDescending(e => e.Time).FirstOrDefault();
            MedicationLatestValue = $"{latestMedication?.MedicationName} {latestMedication?.Dose}" ?? "";
            MedicationStatusText = latestMedication != null ? $"Last: {latestMedication.Time:HH:mm}, {MeasurementStatusHelper.FormatTimeSince(latestMedication.Time - DateTime.Now)}" : "";

            // IV Fluid (not scheduled, just latest)
            var latestIVFluid = Patient.IVFluids?.OrderByDescending(e => e.Time).FirstOrDefault();
            IvFluidLatestValue = latestIVFluid != null ? $"{latestIVFluid.FluidType}, {latestIVFluid.VolumeInfused}ml" : "";
            IvFluidStatusText = latestIVFluid != null ? $"Last: {latestIVFluid.Time:HH:mm}, {MeasurementStatusHelper.FormatTimeSince(latestIVFluid.Time - DateTime.Now)}" : "";

            // Plan (not scheduled, just latest)
            var latestPlan = Patient.Plans?.OrderByDescending(e => e.Time).FirstOrDefault();
            PlanLatestValue = latestPlan?.Notes ?? "";
            PlanStatusText = latestPlan != null ? $"Last: {latestPlan.Time:HH:mm}, {MeasurementStatusHelper.FormatTimeSince(latestPlan.Time - DateTime.Now)}" : "";

            // Bishop Score (not scheduled, just latest)
            var latestBishopScore = Patient.BishopScores?.OrderByDescending(e => e.Time).FirstOrDefault();
            if (latestBishopScore != null)
            {
                BishopScoreTotalDisplay = latestBishopScore.TotalScore.ToString();
                BishopScoreLatestValue = latestBishopScore.Time != default
                    ? $"Last recorded: {latestBishopScore.Time:HH:mm}"
                    : "No score recorded";
                BishopScoreInterpretation = latestBishopScore.Interpretation ?? "";
                BishopScoreNotes = latestBishopScore.Notes ?? "";

                // Color based on score favorability
                if (latestBishopScore.TotalScore >= 9)
                {
                    BishopScoreStatusColor = "#4CAF50"; // Green - Favorable
                    BishopScoreButtonColor = "#E8F5E9"; // Light green background
                }
                else if (latestBishopScore.TotalScore >= 6)
                {
                    BishopScoreStatusColor = "#FF9800"; // Orange - Moderately favorable
                    BishopScoreButtonColor = "#FFF3E0"; // Light orange background
                }
                else
                {
                    BishopScoreStatusColor = "#F44336"; // Red - Unfavorable
                    BishopScoreButtonColor = "#FFEBEE"; // Light red background
                }
            }
            else
            {
                BishopScoreTotalDisplay = "-";
                BishopScoreLatestValue = "";
                BishopScoreInterpretation = "";
                BishopScoreNotes = "";
                BishopScoreStatusColor = "#808080"; // Gray - No data
                BishopScoreButtonColor = "LightGray";
            }
        }

        //private void LoadMeasurablesFromDatabase()
        //{
        //    if (_patient?.ID == null)
        //        return;

        //    try
        //    {
        //        // Map measurables to TimeSlots based on their time
        //        foreach (var timeSlot in TimeSlots)
        //        {
        //            // Define the time window for this slot (current hour)
        //            var slotStartTime = new DateTime(timeSlot.Time.Year, timeSlot.Time.Month, timeSlot.Time.Day, timeSlot.Time.Hour, 0, 0);
        //            var slotEndTime = slotStartTime.AddHours(1);

        //            // Find companion entry for this time slot
        //            var companionEntry = _patient.Companions?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (companionEntry != null && !string.IsNullOrEmpty(companionEntry.CompanionDisplay))
        //            {
        //                if (Enum.TryParse<CompanionType>(companionEntry.CompanionDisplay, true, out var companionType))
        //                {
        //                    timeSlot.Companion = companionType;
        //                }
        //            }

        //            // Find pain relief entry for this time slot
        //            var painReliefEntry = _patient.PainReliefs?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (painReliefEntry != null && !string.IsNullOrEmpty(painReliefEntry.PainReliefDisplay))
        //            {
        //                if (Enum.TryParse<PainReliefType>(painReliefEntry.PainReliefDisplay, true, out var painReliefType))
        //                {
        //                    timeSlot.PainRelief = painReliefType;
        //                }
        //            }

        //            // Find oral fluid entry for this time slot
        //            var oralFluidEntry = _patient.OralFluids?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (oralFluidEntry != null && !string.IsNullOrEmpty(oralFluidEntry.OralFluidDisplay))
        //            {
        //                if (Enum.TryParse<OralFluidType>(oralFluidEntry.OralFluidDisplay, true, out var oralFluidType))
        //                {
        //                    timeSlot.OralFluid = oralFluidType;
        //                }
        //            }

        //            // Find posture entry for this time slot
        //            var postureEntry = _patient.Postures?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (postureEntry != null && !string.IsNullOrEmpty(postureEntry.PostureDisplay))
        //            {
        //                if (Enum.TryParse<PostureType>(postureEntry.PostureDisplay, true, out var postureType))
        //                {
        //                    timeSlot.Posture = postureType;
        //                }
        //            }

        //            // Find FHR entry for this time slot
        //            var fhrEntry = _patient.Fhrs?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (fhrEntry != null && fhrEntry.Rate.HasValue)
        //            {
        //                timeSlot.BaselineFHR = fhrEntry.Rate.Value;
        //            }

        //            // Find temperature entry for this time slot
        //            var temperatureEntry = _patient.Temperatures?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (temperatureEntry != null)
        //            {
        //                timeSlot.Temperature = temperatureEntry.Rate;
        //            }

        //            // Find urine entry for this time slot
        //            var urineEntry = _patient.Urines?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (urineEntry != null)
        //            {
        //                if (!string.IsNullOrEmpty(urineEntry.Protein))
        //                {
        //                    timeSlot.UrineProtein = urineEntry.Protein;
        //                }
        //                if (!string.IsNullOrEmpty(urineEntry.Acetone))
        //                {
        //                    timeSlot.UrineAcetone = urineEntry.Acetone;
        //                }
        //            }

        //            // Find oxytocin entry for this time slot
        //            var oxytocinEntry = _patient.Oxytocins?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (oxytocinEntry != null)
        //            {
        //                timeSlot.OxytocinDose = oxytocinEntry.DoseMUnitsPerMin;
        //                timeSlot.OxytocinVolume = oxytocinEntry.TotalVolumeInfused;
        //            }

        //            // Find IV fluid entry for this time slot
        //            var ivFluidEntry = _patient.IVFluids?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (ivFluidEntry != null)
        //            {
        //                if (!string.IsNullOrEmpty(ivFluidEntry.FluidType))
        //                {
        //                    timeSlot.IVFluidType = ivFluidEntry.FluidType;
        //                }
        //                timeSlot.IVFluidVolume = ivFluidEntry.VolumeInfused;
        //                if (!string.IsNullOrEmpty(ivFluidEntry.Rate))
        //                {
        //                    timeSlot.IVFluidRate = ivFluidEntry.Rate;
        //                }
        //            }

        //            // Find cervix dilatation entry for this time slot
        //            var dilatationEntry = _patient.Dilatations?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (dilatationEntry != null)
        //            {
        //                timeSlot.CervixDilatation = dilatationEntry.DilatationCm;
        //            }

        //            // Find contraction entry for this time slot
        //            var contractionEntry = _patient.Contractions?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (contractionEntry != null)
        //            {
        //                timeSlot.ContractionFrequency = contractionEntry.FrequencyPer10Min;
        //                timeSlot.ContractionDuration = contractionEntry.DurationSeconds;
        //            }

        //            // Find head descent entry for this time slot
        //            var headDescentEntry = _patient.HeadDescents?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (headDescentEntry != null)
        //            {
        //                timeSlot.HeadDescentStation = headDescentEntry.Station;
        //            }

        //            // Find fetal position entry for this time slot
        //            var fetalPositionEntry = _patient.FetalPositions?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (fetalPositionEntry != null && !string.IsNullOrEmpty(fetalPositionEntry.Position))
        //            {
        //                timeSlot.FetalPosition = fetalPositionEntry.Position;
        //            }

        //            // Find amniotic fluid entry for this time slot
        //            var amnioticFluidEntry = _patient.AmnioticFluids?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (amnioticFluidEntry != null && !string.IsNullOrEmpty(amnioticFluidEntry.Color))
        //            {
        //                timeSlot.AmnioticFluidColor = amnioticFluidEntry.Color;
        //            }

        //            // Find caput entry for this time slot
        //            var caputEntry = _patient.Caputs?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (caputEntry != null && !string.IsNullOrEmpty(caputEntry.Degree))
        //            {
        //                timeSlot.CaputDegree = caputEntry.Degree;
        //            }

        //            // Find moulding entry for this time slot
        //            var mouldingEntry = _patient.Mouldings?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (mouldingEntry != null)
        //            {
        //                timeSlot.MouldingDegree = mouldingEntry.Degree;
        //            }

        //            // Find BP entry for this time slot
        //            var bpEntry = _patient.BPs?.FirstOrDefault(e =>
        //                e.Time >= slotStartTime && e.Time < slotEndTime);
        //            if (bpEntry != null)
        //            {
        //                timeSlot.BPSystolic = bpEntry.Systolic;
        //                timeSlot.BPDiastolic = bpEntry.Diastolic;
        //                timeSlot.Pulse = bpEntry.Pulse;
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        _errorHandler.HandleError(e);
        //    }
        //}

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

                // Calculate labor duration
                if (Patient.LaborStartTime.HasValue)
                {
                    var duration = DateTime.Now - Patient.LaborStartTime.Value;
                    LaborDuration = $"{(int)duration.TotalHours}h {duration.Minutes}m";
                }

                // Get the earliest measurable time to set as start time
                var earliestMeasurableTime = await GetEarliestMeasurableTimeAsync();

                // Set StartTime to the earliest of: LaborStartTime or earliest measurable
                DateTime? newStartTime = null;
                if (Patient.LaborStartTime.HasValue)
                {
                    newStartTime = Patient.LaborStartTime.Value;
                }
                else if (earliestMeasurableTime.HasValue)
                {
                    newStartTime = earliestMeasurableTime.Value;
                }

                // If we have a new start time, floor it to the hour and regenerate time slots
                if (newStartTime.HasValue)
                {
                    StartTime = new DateTime(newStartTime.Value.Year, newStartTime.Value.Month,
                        newStartTime.Value.Day, newStartTime.Value.Hour, 0, 0);

                    await GenerateInitialTimeSlots();
                }

                //var companions = await _companionRepository.ListByPatientAsync(patientId);
                //// Load partograph entries
                //var entries = await _partographRepository.ListByPatientAsync(patientId);
                //PartographEntries = new ObservableCollection<Partograph>(entries.OrderBy(e => e.Time));

                //if (entries.Any())
                //{
                //    LastRecordedTime = entries.Max(e => e.Time);
                //    var latestEntry = entries.OrderByDescending(e => e.Time).FirstOrDefault();
                //    //CurrentDilation = latestEntry.CervicalDilation;
                //}

                if (Patient.Dilatations.Any())
                    CurrentDilation = Patient.Dilatations?.OrderByDescending(e => e.Time)?.FirstOrDefault()?.DilatationCm ?? 0;
                else
                    CurrentDilation = 0;

                //if (_patient.Companions.Any())
                //    CompanionDescription = _patient.Companions?.OrderByDescending(e => e.Time)?.FirstOrDefault()?.CompanionDisplay ?? string.Empty;
                //else
                //    CompanionDescription = string.Empty;                

                //// Load measurables from database and populate TimeSlots
                //LoadMeasurablesFromDatabase();

                // Prepare chart data
                //PrepareChartData();
                //CalculateAlertActionLines();

                CompanionCurrentIndex = 0;
                CompanionText = Patient?.Companions[CompanionCurrentIndex]?.CompanionDisplay ?? string.Empty;

                // Update all measurement statuses with latest values and due times
                UpdateMeasurementStatuses();

                // Analyze patient data and generate clinical alerts
                AnalyzeAndGenerateAlerts();

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

        //private void PrepareChartData()
        //{
        //    if (!PartographEntries.Any()) return;

        //    var baseTime = _patient?.LaborStartTime ?? PartographEntries.First().Time;

        //    //// Cervical Dilation Data
        //    //var dilationData = new ObservableCollection<ChartDataPoint>();
        //    //foreach (var entry in PartographEntries)
        //    //{
        //    //    dilationData.Add(new ChartDataPoint
        //    //    {
        //    //        Time = entry.Time,
        //    //        Value = entry.CervicalDilation
        //    //    });
        //    //}

        //    //CervicalDilationData = dilationData;

        //    //// Fetal Heart Rate Data
        //    //var fhrData = new ObservableCollection<ChartDataPoint>();
        //    //foreach (var entry in PartographEntries.Where(e => e.FetalHeartRate > 0))
        //    //{
        //    //    fhrData.Add(new ChartDataPoint
        //    //    {
        //    //        Time = entry.RecordedTime,
        //    //        Value = entry.FetalHeartRate
        //    //    });
        //    //}
        //    //FetalHeartRateData = fhrData;

        //    //// Contractions Data
        //    //var contractionsData = new ObservableCollection<ChartDataPoint>();
        //    //foreach (var entry in PartographEntries)
        //    //{
        //    //    contractionsData.Add(new ChartDataPoint
        //    //    {
        //    //        Time = entry.Time,
        //    //        Value = entry.ContractionsPerTenMinutes
        //    //    });
        //    //}
        //    //ContractionsData = contractionsData;
        //}

        //private void CalculateAlertActionLines()
        //{
        //    if (_patient?.LaborStartTime == null) return;

        //    var startTime = _patient.LaborStartTime.Value;
        //    var alertLine = new ObservableCollection<ChartDataPoint>();
        //    var actionLine = new ObservableCollection<ChartDataPoint>();

        //    //// Alert line: Expected progress of 1cm/hour from 4cm
        //    //// Starting from when patient reached 4cm dilation
        //    //var fourCmEntry = PartographEntries.FirstOrDefault(e => e.CervicalDilation >= 4);
        //    //if (fourCmEntry != null)
        //    //{
        //    //    var fourCmTime = fourCmEntry.RecordedTime;

        //    //    // Alert line - normal progress
        //    //    for (int i = 4; i <= 10; i++)
        //    //    {
        //    //        alertLine.Add(new ChartDataPoint
        //    //        {
        //    //            Time = fourCmTime.AddHours(i - 4),
        //    //            Value = i
        //    //        });
        //    //    }

        //    //    // Action line - 2 hours behind alert line
        //    //    for (int i = 4; i <= 10; i++)
        //    //    {
        //    //        actionLine.Add(new ChartDataPoint
        //    //        {
        //    //            Time = fourCmTime.AddHours(i - 4 + 2),
        //    //            Value = i
        //    //        });
        //    //    }
        //    //}

        //    //AlertLineData = alertLine;
        //    //ActionLineData = actionLine;
        //}

        [RelayCommand]
        private Task AddEntry()
            => Shell.Current.GoToAsync($"partographentry?patientId={Patient?.ID}");

        [RelayCommand]
        private async Task Print()
        {
            await AppShell.DisplayToastAsync("Partograph printing feature coming soon");
        }

        [RelayCommand]
        private Task ViewCharts()
            => Shell.Current.GoToAsync($"///partographchart?patientId={Patient?.ID}");

        //public Task NavigateToPartograph(Partograph patient) => Shell.Current.GoToAsync($"partograph?patientId={patient.ID}");
        [RelayCommand]
        private async Task SecondStage()
        {
            if (Patient?.ID == null)
            {
                await AppShell.DisplayToastAsync("No patient selected");
                return;
            }

            // Check if patient has reached appropriate stage for birth outcome recording
            if (CurrentDilation < 10 && Patient.Status != LaborStatus.Completed && Patient.Status != LaborStatus.SecondStage)
            {
                var shouldContinue = await Application.Current.MainPage.DisplayAlert(
                    "Confirm Promotion",
                    $"Current dilation is {CurrentDilation}cm. Are you sure you want to proceed to birth outcome recording?",
                    "Yes", "No");

                if (!shouldContinue)
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
        private async Task NavigateToReferral()
        {
            if (Patient?.ID == null)
            {
                await AppShell.DisplayToastAsync("No patient selected");
                return;
            }

            try
            {
                // Navigate to referral page
                var parameters = new Dictionary<string, object>
                {
                    { "PartographId", Patient.ID }
                };

                await Shell.Current.GoToAsync("ReferralPage", parameters);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
                await AppShell.DisplayToastAsync("Failed to navigate to referral page");
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            if (Patient != null)
                await LoadData(Patient.ID);
        }

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
        public Action? CloseMouldingModalPopup { get; set; }
        public Action? OpenMouldingModalPopup { get; set; }
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
        public Action? CloseCervixDilatationModalPopup { get; set; }
        public Action? OpenCervixDilatationModalPopup { get; set; }
        public Action? OpenFHRContractionModalPopup { get; set; }
        public Action? CloseFHRContractionModalPopup { get; set; }
        public Action? OpenAssessmentModalPopup { get; set; }
        public Action? CloseAssessmentModalPopup { get; set; }
        public Action? OpenPlanModalPopup { get; set; }
        public Action? ClosePlanModalPopup { get; set; }
        public Action? OpenBishopScoreModalPopup { get; set; }
        public Action? CloseBishopScoreModalPopup { get; set; }

        // Popup Open Commands
        [RelayCommand]
        public void OpenBishopScorePopup()
        {
            if (Patient?.ID != null)
            {
                // Pre-populate Bishop Score with latest measurable data
                var latestDilatation = Patient.Dilatations?.OrderByDescending(d => d.Time).FirstOrDefault();

                int dilationCm = latestDilatation?.DilatationCm ?? 0;

                // Initialize with available data (other fields default to 0/initial values)
                _bishopScorePopupPageModel.InitializeWithData(
                    dilationCm: dilationCm,
                    effacementPercent: 0,  // Not currently tracked in CervixDilatation
                    station: "-3",         // Default value
                    consistency: "Firm",   // Default value
                    position: "Posterior"  // Default value
                );

                _bishopScorePopupPageModel.ClosePopup = () => CloseBishopScoreModalPopup?.Invoke();

                // Subscribe to save event to refresh data after save
                _bishopScorePopupPageModel.OnScoreSaved = async (score) =>
                {
                    // Save to repository here if needed
                    await RefreshCommand.ExecuteAsync(null);
                };

                OpenBishopScoreModalPopup?.Invoke();
            }
        }

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

        [ObservableProperty]
        private string _companionText;

        [ObservableProperty]
        private int _companionCurrentIndex = 0;
        //private readonly List<string> _companionInfoList;
        //_patient.Companions?.OrderByDescending(e => e.Time)?.FirstOrDefault()?.CompanionDisplay ?? string.Empty

        [RelayCommand]
        private void OnCompanionCycle()
        {
            CompanionCurrentIndex = (CompanionCurrentIndex + 1) % Patient?.Companions.Count ?? 0;
            //_companionCurrentIndex = (_companionCurrentIndex + 1) % _companionInfoList.Count;
            //CompanionText = _companionInfoList[_companionCurrentIndex];
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
                //IsPainReliefPopupOpen = true;
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
        private async Task OpenMedicationPopup()
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
        private async Task OpenIVFluidPopup()
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
        private async Task OpenCervixDilatationPopup()
        {
            if (Patient?.ID != null)
            {
                _cervixDilatationModalPageModel._patient = Patient;
                _cervixDilatationModalPageModel.ClosePopup = () => CloseCervixDilatationModalPopup?.Invoke();
                await _cervixDilatationModalPageModel.LoadPatient(Patient.ID);
                OpenCervixDilatationModalPopup?.Invoke();
            }
        }

        [RelayCommand]
        private async Task OpenMouldingPopup()
        {
            if (Patient?.ID != null)
            {
                _mouldingModalPageModel._patient = Patient;
                _mouldingModalPageModel.ClosePopup = () => CloseMouldingModalPopup?.Invoke();
                await _mouldingModalPageModel.LoadPatient(Patient.ID);
                OpenMouldingModalPopup?.Invoke();
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

        //[RelayCommand]
        //private async Task OpenBishopScorePopup()
        //{
        //    if (Patient?.ID != null)
        //    {
        //        //_planModalPageModel._patient = Patient;
        //        //_planModalPageModel.ClosePopup = () => ClosePlanModalPopup?.Invoke();
        //        //await _planModalPageModel.LoadPatient(Patient.ID);
        //        OpenBishopScoreModalPopup?.Invoke();
        //    }
        //}

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
                var patientDetails = await _patientRepository.GetAsync(Patient.PatientID);

                // Generate clinical notes
                ClinicalNotes = _notesService.GenerateClinicalNotes(Patient, patientDetails);
                NotesGeneratedTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
                //"Error generating clinical notes"
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
                var patientDetails = await _patientRepository.GetAsync(Patient.PatientID);
                var patientName = patientDetails != null ?
                    $"{patientDetails.FirstName}_{patientDetails.LastName}" : "Patient";
                var fileName = $"Clinical_Notes_{patientName}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

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
                _errorHandler.HandleError(ex);
                //, "Error exporting clinical notes"
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
                var patientDetails = await _patientRepository.GetAsync(Patient.PatientID);
                ClinicalNotes = _notesService.GenerateClinicalNotes(Patient, patientDetails);
                NotesGeneratedTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                // Silently log error - don't disrupt user workflow
                System.Diagnostics.Debug.WriteLine($"Error generating clinical notes: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates and downloads partograph PDF report
        /// </summary>
        [RelayCommand]
        private async Task PrintPartograph()
        {
            if (Patient?.ID == null)
            {
                await AppShell.DisplayToastAsync("No partograph loaded");
                return;
            }

            try
            {
                IsBusy = true;
                await AppShell.DisplayToastAsync("Generating partograph PDF...");

                var filePath = await _pdfService.GenerateAndSavePartographPdfAsync(
                    Patient.ID.Value,
                    PatientName);

                await AppShell.DisplayToastAsync($"PDF saved to: {filePath}");

                // Optionally, open the PDF
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
                await AppShell.DisplayToastAsync($"Failed to generate PDF: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    //public class ChartDataPoint
    //{
    //    public DateTime Time { get; set; }
    //    public double Value { get; set; }
    //}
}
