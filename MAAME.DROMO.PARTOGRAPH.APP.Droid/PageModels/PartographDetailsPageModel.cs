using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using System.Collections.ObjectModel;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels
{
    #region View Models for Display

    public class BabyDetailViewModel
    {
        public string BabyLabel { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public string BirthWeight { get; set; } = string.Empty;
        public string ApgarScores { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty;
        public Color OutcomeColor { get; set; } = Colors.Green;
    }

    public class MeasurementEntryViewModel
    {
        public DateTime Time { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string StatusText { get; set; } = "Normal";
        public Color StatusColor { get; set; } = Color.FromArgb("#10B981");
    }

    public class MedicalNoteViewModel
    {
        public DateTime Time { get; set; }
        public string NoteType { get; set; } = "General";
        public string Content { get; set; } = string.Empty;
        public string Handler { get; set; } = string.Empty;
        public bool IsImportant { get; set; }
        public Color TypeColor { get; set; } = Color.FromArgb("#6B7280");
        public Color BorderColor { get; set; } = Color.FromArgb("#E5E7EB");
    }

    public class AssessmentPlanViewModel
    {
        public DateTime Time { get; set; }
        public string Assessment { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string Handler { get; set; } = string.Empty;
    }

    public class MedicationViewModel
    {
        public DateTime Time { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    #endregion

    public partial class PartographDetailsPageModel : ObservableObject, IQueryAttributable
    {
        private readonly PartographRepository _partographRepository;
        private readonly PatientRepository _patientRepository;
        private readonly BirthOutcomeRepository _birthOutcomeRepository;
        private readonly BabyDetailsRepository _babyDetailsRepository;
        //private readonly MedicalNoteRepository _medicalNoteRepository;
        private readonly FHRRepository _fhrRepository;
        private readonly ContractionRepository _contractionRepository;
        private readonly CervixDilatationRepository _cervixDilatationRepository;
        private readonly HeadDescentRepository _headDescentRepository;
        private readonly BPRepository _bpRepository;
        private readonly TemperatureRepository _temperatureRepository;
        private readonly UrineRepository _urineRepository;
        private readonly AmnioticFluidRepository _amnioticFluidRepository;
        private readonly FetalPositionRepository _fetalPositionRepository;
        private readonly CaputRepository _caputRepository;
        private readonly MouldingRepository _mouldingRepository;
        private readonly OxytocinRepository _oxytocinRepository;
        private readonly MedicationEntryRepository _medicationEntryRepository;
        private readonly IVFluidEntryRepository _ivFluidEntryRepository;
        private readonly AssessmentRepository _assessmentRepository;
        private readonly PlanRepository _planRepository;
        private readonly CompanionRepository _companionRepository;
        private readonly PostureRepository _postureRepository;
        private readonly OralFluidRepository _oralFluidRepository;
        private readonly PainReliefRepository _painReliefRepository;
        private readonly PartographNotesService _notesService;
        private readonly IPartographPdfService _pdfService;
        private readonly ModalErrorHandler _errorHandler;

        private Partograph? _partograph;
        
        private BirthOutcome? _birthOutcome;
        private Guid? _partographId;

        #region Observable Properties

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isRefreshing;

        // Patient Info
        [ObservableProperty]
        private string _patientName = string.Empty;

        [ObservableProperty]
        private string _patientInfo = string.Empty;

        // Labour Summary
        [ObservableProperty]
        private string _totalLaborDuration = "-";

        [ObservableProperty]
        private string _deliveryMode = "-";

        [ObservableProperty]
        private string _numberOfBabies = "0";

        [ObservableProperty]
        private DateTime? _deliveryTime;

        // Timeline
        [ObservableProperty]
        private DateTime? _laborStartTime;

        [ObservableProperty]
        private DateTime? _secondStageStartTime;

        [ObservableProperty]
        private DateTime? _thirdStageStartTime;

        [ObservableProperty]
        private DateTime? _fourthStageStartTime;

        // Birth Outcome
        [ObservableProperty]
        private BirthOutcome? _birthOutcomeData;

        public BirthOutcome? BirthOutcome => _birthOutcomeData;

        [ObservableProperty]
        private string _maternalOutcome = "Survived";

        [ObservableProperty]
        private string _perineumStatus = "-";

        [ObservableProperty]
        private int _estimatedBloodLoss;

        [ObservableProperty]
        private string _placentaStatus = "-";

        [ObservableProperty]
        private bool _hasComplications;

        [ObservableProperty]
        private string _complicationsText = string.Empty;

        [ObservableProperty]
        private bool _hasInterventions;

        [ObservableProperty]
        private string _interventionsText = string.Empty;

        // Baby Details
        [ObservableProperty]
        private bool _hasBabyDetails;

        [ObservableProperty]
        private ObservableCollection<BabyDetailViewModel> _babyDetailsList = [];

        // Measurements
        [ObservableProperty]
        private ObservableCollection<MeasurementEntryViewModel> _measurementEntries = [];

        [ObservableProperty]
        private int _totalMeasurements;

        [ObservableProperty]
        private bool _hasMoreMeasurements;

        // Medical Notes
        [ObservableProperty]
        private ObservableCollection<MedicalNoteViewModel> _medicalNotes = [];

        [ObservableProperty]
        private int _totalNotes;

        // Assessment & Plan
        [ObservableProperty]
        private ObservableCollection<AssessmentPlanViewModel> _assessmentPlanEntries = [];

        // Medications
        [ObservableProperty]
        private ObservableCollection<MedicationViewModel> _medicationEntries = [];

        // Clinical Summary
        [ObservableProperty]
        private string _clinicalNotesSummary = string.Empty;

        #endregion

        public PartographDetailsPageModel(
            PartographRepository partographRepository,
            PatientRepository patientRepository,
            BirthOutcomeRepository birthOutcomeRepository,
            BabyDetailsRepository babyDetailsRepository, 
            FHRRepository fhrRepository,
            ContractionRepository contractionRepository,
            CervixDilatationRepository cervixDilatationRepository,
            HeadDescentRepository headDescentRepository,
            BPRepository bpRepository,
            TemperatureRepository temperatureRepository,
            UrineRepository urineRepository,
            AmnioticFluidRepository amnioticFluidRepository,
            FetalPositionRepository fetalPositionRepository,
            CaputRepository caputRepository,
            MouldingRepository mouldingRepository,
            OxytocinRepository oxytocinRepository,
            MedicationEntryRepository medicationEntryRepository,
            IVFluidEntryRepository ivFluidEntryRepository,
            AssessmentRepository assessmentRepository,
            PlanRepository planRepository,
            CompanionRepository companionRepository,
            PostureRepository postureRepository,
            OralFluidRepository oralFluidRepository,
            PainReliefRepository painReliefRepository,
            PartographNotesService notesService,
            IPartographPdfService pdfService,
            ModalErrorHandler errorHandler)
        {
            _partographRepository = partographRepository;
            _patientRepository = patientRepository;
            _birthOutcomeRepository = birthOutcomeRepository;
            _babyDetailsRepository = babyDetailsRepository;
            //_medicalNoteRepository = medicalNoteRepository;
            _fhrRepository = fhrRepository;
            _contractionRepository = contractionRepository;
            _cervixDilatationRepository = cervixDilatationRepository;
            _headDescentRepository = headDescentRepository;
            _bpRepository = bpRepository;
            _temperatureRepository = temperatureRepository;
            _urineRepository = urineRepository;
            _amnioticFluidRepository = amnioticFluidRepository;
            _fetalPositionRepository = fetalPositionRepository;
            _caputRepository = caputRepository;
            _mouldingRepository = mouldingRepository;
            _oxytocinRepository = oxytocinRepository;
            _medicationEntryRepository = medicationEntryRepository;
            _ivFluidEntryRepository = ivFluidEntryRepository;
            _assessmentRepository = assessmentRepository;
            _planRepository = planRepository;
            _companionRepository = companionRepository;
            _postureRepository = postureRepository;
            _oralFluidRepository = oralFluidRepository;
            _painReliefRepository = painReliefRepository;
            _notesService = notesService;
            _pdfService = pdfService;
            _errorHandler = errorHandler;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("id", out var idObj) && idObj != null)
            {
                if (Guid.TryParse(idObj.ToString(), out var id))
                {
                    _partographId = id;
                    _ = LoadDataAsync();
                }
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            try
            {
                IsRefreshing = true;
                await LoadDataAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task LoadDataAsync()
        {
            if (_partographId == null) return;

            try
            {
                IsBusy = true;

                // Load partograph
                _partograph = await _partographRepository.GetAsync(_partographId.Value);
                if (_partograph == null) return;

                // Load patient
                if (_partograph.PatientID.HasValue)
                {
                    var patient = await _patientRepository.GetAsync(_partograph.PatientID.Value);
                    if (patient != null)
                    {
                        _partograph.Patient = patient;
                    }
                }

                // Set patient info
                PatientName = _partograph.Name ?? "Unknown Patient";
                PatientInfo = _partograph.DisplayInfo ?? "";

                // Set timeline
                LaborStartTime = _partograph.LaborStartTime;
                SecondStageStartTime = _partograph.SecondStageStartTime;
                ThirdStageStartTime = _partograph.ThirdStageStartTime;
                FourthStageStartTime = _partograph.FourthStageStartTime;

                // Calculate total labor duration
                if (_partograph.LaborStartTime.HasValue && _partograph.FourthStageStartTime.HasValue)
                {
                    var duration = _partograph.FourthStageStartTime.Value - _partograph.LaborStartTime.Value;
                    TotalLaborDuration = FormatDuration(duration);
                }

                // Load birth outcome
                _birthOutcome = await _birthOutcomeRepository.GetByPartographIdAsync(_partographId.Value);
                BirthOutcomeData = _birthOutcome;

                if (_birthOutcome != null)
                {
                    DeliveryTime = _birthOutcome.DeliveryTime;
                    DeliveryMode = _birthOutcome.DeliveryMode.ToString();
                    NumberOfBabies = _birthOutcome.NumberOfBabies.ToString();
                    MaternalOutcome = _birthOutcome.MaternalStatus == MaternalOutcomeStatus.Survived ? "Survived" : "Deceased";
                    PerineumStatus = _birthOutcome.PerinealStatus.ToString() ?? "Not recorded";
                    EstimatedBloodLoss = _birthOutcome.EstimatedBloodLoss;
                    PlacentaStatus = _birthOutcome.PlacentaComplete ? "Complete" : "Incomplete";

                    // Complications
                    var complications = new List<string>();
                    if (_birthOutcome.PostpartumHemorrhage) complications.Add("Postpartum Hemorrhage");
                    if (_birthOutcome.Eclampsia) complications.Add("Eclampsia");
                    if (_birthOutcome.SepticShock) complications.Add("Septic Shock");
                    if (_birthOutcome.ObstructedLabor) complications.Add("Obstructed Labor");
                    if (_birthOutcome.RupturedUterus) complications.Add("Ruptured Uterus");
                    HasComplications = complications.Count > 0;
                    ComplicationsText = string.Join(", ", complications);

                    // Interventions
                    var interventions = new List<string>();
                    if (_birthOutcome.OxytocinGiven) interventions.Add("Oxytocin");
                    if (_birthOutcome.AntibioticsGiven) interventions.Add("Antibiotics");
                    if (_birthOutcome.BloodTransfusionGiven) interventions.Add("Blood Transfusion");
                    HasInterventions = interventions.Count > 0;
                    InterventionsText = string.Join(", ", interventions);

                    // Load baby details
                    var babies = await _babyDetailsRepository.GetByBirthOutcomeIdAsync(_birthOutcome.ID!.Value);
                    HasBabyDetails = babies.Count > 0;
                    BabyDetailsList.Clear();
                    int babyNum = 1;
                    foreach (var baby in babies)
                    {
                        BabyDetailsList.Add(new BabyDetailViewModel
                        {
                            BabyLabel = babies.Count > 1 ? $"Baby {babyNum}" : "Baby",
                            Sex = baby.Sex.ToString() ?? "Unknown",
                            BirthWeight = baby.BirthWeight == 0m ? $"{baby.BirthWeight}" : "N/A",
                            ApgarScores = $"{baby.Apgar1Min ?? 0}/{baby.Apgar5Min ?? 0}",
                            Outcome = !string.IsNullOrWhiteSpace(baby.DeathCause) ? (baby.ResuscitationRequired ? "Alive (Resuscitated)" : "Alive") : "Stillborn",
                            OutcomeColor = !string.IsNullOrWhiteSpace(baby.DeathCause) ? Colors.Green : Colors.Red
                        });
                        babyNum++;
                    }
                }

                // Load all measurements in parallel
                await LoadMeasurementsAsync();

                // Load medical notes
                //await LoadMedicalNotesAsync();

                // Load assessments and plans
                await LoadAssessmentPlansAsync();

                // Load medications
                await LoadMedicationsAsync();

                // Generate clinical summary
                await GenerateClinicalSummaryAsync();
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadMeasurementsAsync()
        {
            if (_partographId == null) return;

            var allMeasurements = new List<MeasurementEntryViewModel>();

            // Load FHR
            var fhrEntries = await _fhrRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in fhrEntries)
            {
                allMeasurements.Add(new MeasurementEntryViewModel
                {
                    Time = entry.Time,
                    Category = "Fetal Heart Rate",
                    Value = $"{entry.Rate} bpm",
                    Notes = entry.Notes ?? "",
                    StatusText = GetFhrStatus(entry.Rate ?? 0),
                    StatusColor = GetFhrStatusColor(entry.Rate ?? 0)
                });
            }

            // Load Contractions
            var contractions = await _contractionRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in contractions)
            {
                allMeasurements.Add(new MeasurementEntryViewModel
                {
                    Time = entry.Time,
                    Category = "Contractions",
                    Value = $"{entry.FrequencyPer10Min}/10min, {entry.DurationSeconds}s duration",
                    Notes = entry.Notes ?? "",
                    StatusText = "Recorded",
                    StatusColor = Color.FromArgb("#3B82F6")
                });
            }

            // Load Cervix Dilation
            var dilations = await _cervixDilatationRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in dilations)
            {
                allMeasurements.Add(new MeasurementEntryViewModel
                {
                    Time = entry.Time,
                    Category = "Cervical Dilation",
                    Value = $"{entry.DilatationCm} cm",
                    Notes = entry.Notes ?? "",
                    StatusText = "Recorded",
                    StatusColor = Color.FromArgb("#8B5CF6")
                });
            }

            // Load Head Descent
            var descents = await _headDescentRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in descents)
            {
                allMeasurements.Add(new MeasurementEntryViewModel
                {
                    Time = entry.Time,
                    Category = "Head Descent",
                    Value = entry.DescentRate.ToString(),
                    Notes = entry.Notes ?? "",
                    StatusText = "Recorded",
                    StatusColor = Color.FromArgb("#6366F1")
                });
            }

            // Load BP
            var bpEntries = await _bpRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in bpEntries)
            {
                allMeasurements.Add(new MeasurementEntryViewModel
                {
                    Time = entry.Time,
                    Category = "Blood Pressure",
                    Value = $"{entry.Systolic}/{entry.Diastolic} mmHg, Pulse: {entry.Pulse} bpm",
                    Notes = entry.Notes ?? "",
                    StatusText = GetBpStatus(entry.Systolic, entry.Diastolic),
                    StatusColor = GetBpStatusColor(entry.Systolic, entry.Diastolic)
                });
            }

            // Load Temperature
            var temps = await _temperatureRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in temps)
            {
                allMeasurements.Add(new MeasurementEntryViewModel
                {
                    Time = entry.Time,
                    Category = "Temperature",
                    Value = $"{entry.TemperatureCelsius:F1}°C",
                    Notes = entry.Notes ?? "",
                    StatusText = GetTempStatus(entry.TemperatureCelsius),
                    StatusColor = GetTempStatusColor(entry.TemperatureCelsius)
                });
            }

            // Load Urine
            var urineEntries = await _urineRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in urineEntries)
            {
                allMeasurements.Add(new MeasurementEntryViewModel
                {
                    Time = entry.Time,
                    Category = "Urine",
                    Value = $"Protein: {entry.Protein}, Acetone: {entry.Ketones}",
                    Notes = entry.Notes ?? "",
                    StatusText = "Recorded",
                    StatusColor = Color.FromArgb("#F59E0B")
                });
            }

            // Load Amniotic Fluid
            var amnioticEntries = await _amnioticFluidRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in amnioticEntries)
            {
                allMeasurements.Add(new MeasurementEntryViewModel
                {
                    Time = entry.Time,
                    Category = "Amniotic Fluid",
                    Value = entry.Color.ToString(),
                    Notes = entry.Notes ?? "",
                    StatusText = GetAmnioticStatus(entry.Color),
                    StatusColor = GetAmnioticStatusColor(entry.Color)
                });
            }

            // Load Fetal Position
            var positions = await _fetalPositionRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in positions)
            {
                allMeasurements.Add(new MeasurementEntryViewModel
                {
                    Time = entry.Time,
                    Category = "Fetal Position",
                    Value = entry.Position.ToString(),
                    Notes = entry.Notes ?? "",
                    StatusText = "Recorded",
                    StatusColor = Color.FromArgb("#14B8A6")
                });
            }

            // Load Caput
            var caputEntries = await _caputRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in caputEntries)
            {
                allMeasurements.Add(new MeasurementEntryViewModel
                {
                    Time = entry.Time,
                    Category = "Caput",
                    Value = entry.Degree.ToString(),
                    Notes = entry.Notes ?? "",
                    StatusText = "Recorded",
                    StatusColor = Color.FromArgb("#EC4899")
                });
            }

            // Load Moulding
            var mouldingEntries = await _mouldingRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in mouldingEntries)
            {
                allMeasurements.Add(new MeasurementEntryViewModel
                {
                    Time = entry.Time,
                    Category = "Moulding",
                    Value = entry.Degree.ToString(),
                    Notes = entry.Notes ?? "",
                    StatusText = "Recorded",
                    StatusColor = Color.FromArgb("#F97316")
                });
            }

            // Load Companion
            var companions = await _companionRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in companions)
            {
                allMeasurements.Add(new MeasurementEntryViewModel
                {
                    Time = entry.Time,
                    Category = "Birth Companion",
                    Value = entry.CompanionPresent ? $"Present ({entry.CompanionType})" : "Not Present",
                    Notes = entry.Notes ?? "",
                    StatusText = entry.CompanionPresent ? "Present" : "Absent",
                    StatusColor = entry.CompanionPresent ? Color.FromArgb("#10B981") : Color.FromArgb("#6B7280")
                });
            }

            // Load Posture
            var postures = await _postureRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in postures)
            {
                allMeasurements.Add(new MeasurementEntryViewModel
                {
                    Time = entry.Time,
                    Category = "Maternal Posture",
                    Value = entry.Posture.ToString(),
                    Notes = entry.Notes ?? "",
                    StatusText = "Recorded",
                    StatusColor = Color.FromArgb("#06B6D4")
                });
            }

            // Load Oral Fluid
            var oralFluids = await _oralFluidRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in oralFluids)
            {
                allMeasurements.Add(new MeasurementEntryViewModel
                {
                    Time = entry.Time,
                    Category = "Oral Fluid",
                    Value = entry.FluidType.ToString(),
                    Notes = entry.Notes ?? "",
                    StatusText = "Recorded",
                    StatusColor = Color.FromArgb("#0EA5E9")
                });
            }

            // Load Pain Relief
            var painReliefs = await _painReliefRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in painReliefs)
            {
                allMeasurements.Add(new MeasurementEntryViewModel
                {
                    Time = entry.Time,
                    Category = "Pain Relief",
                    Value = entry.PainRelief.ToString(),
                    Notes = entry.Notes ?? "",
                    StatusText = "Given",
                    StatusColor = Color.FromArgb("#A855F7")
                });
            }

            // Sort by time descending and take first 20 for display
            var sortedMeasurements = allMeasurements.OrderByDescending(m => m.Time).ToList();
            TotalMeasurements = sortedMeasurements.Count;
            HasMoreMeasurements = sortedMeasurements.Count > 20;

            MeasurementEntries.Clear();
            foreach (var measurement in sortedMeasurements.Take(20))
            {
                MeasurementEntries.Add(measurement);
            }
        }

        //private async Task LoadMedicalNotesAsync()
        //{
        //    if (_partographId == null) return;

        //    var notes = await _medicalNoteRepository.ListAsync(_partographId.Value);
        //    TotalNotes = notes.Count;

        //    MedicalNotes.Clear();
        //    foreach (var note in notes.OrderByDescending(n => n.Time))
        //    {
        //        MedicalNotes.Add(new MedicalNoteViewModel
        //        {
        //            Time = note.Time,
        //            NoteType = note.NoteType ?? "General",
        //            Content = note.Notes ?? note.Content ?? "",
        //            Handler = note.HandlerName ?? "Unknown",
        //            IsImportant = note.IsImportant,
        //            TypeColor = GetNoteTypeColor(note.NoteType),
        //            BorderColor = note.IsImportant ? Color.FromArgb("#EF4444") : Color.FromArgb("#E5E7EB")
        //        });
        //    }
        //}

        private async Task LoadAssessmentPlansAsync()
        {
            if (_partographId == null) return;

            var assessments = await _assessmentRepository.ListByPatientAsync(_partographId.Value);
            var plans = await _planRepository.ListByPatientAsync(_partographId.Value);

            // Combine assessments and plans by time
            var combined = new Dictionary<DateTime, AssessmentPlanViewModel>();

            foreach (var assessment in assessments)
            {
                var roundedTime = new DateTime(assessment.Time.Year, assessment.Time.Month, assessment.Time.Day,
                    assessment.Time.Hour, assessment.Time.Minute, 0);

                if (!combined.ContainsKey(roundedTime))
                {
                    combined[roundedTime] = new AssessmentPlanViewModel
                    {
                        Time = roundedTime,
                        Handler = assessment.HandlerName ?? "Unknown"
                    };
                }

                var assessmentText = new List<string>();
                if (!string.IsNullOrEmpty(assessment.LaborProgress))
                    assessmentText.Add($"Labor Progress: {assessment.LaborProgress}");
                if (!string.IsNullOrEmpty(assessment.FetalWellbeing))
                    assessmentText.Add($"Fetal Wellbeing: {assessment.FetalWellbeing}");
                if (!string.IsNullOrEmpty(assessment.MaternalCondition))
                    assessmentText.Add($"Maternal Condition: {assessment.MaternalCondition}");

                combined[roundedTime].Assessment = string.Join("\n", assessmentText);
            }

            foreach (var plan in plans)
            {
                var roundedTime = new DateTime(plan.Time.Year, plan.Time.Month, plan.Time.Day,
                    plan.Time.Hour, plan.Time.Minute, 0);

                if (!combined.ContainsKey(roundedTime))
                {
                    combined[roundedTime] = new AssessmentPlanViewModel
                    {
                        Time = roundedTime,
                        Handler = plan.HandlerName ?? "Unknown"
                    };
                }

                combined[roundedTime].Plan = plan.Notes ?? "";
            }

            AssessmentPlanEntries.Clear();
            foreach (var entry in combined.Values.OrderByDescending(e => e.Time))
            {
                AssessmentPlanEntries.Add(entry);
            }
        }

        private async Task LoadMedicationsAsync()
        {
            if (_partographId == null) return;

            MedicationEntries.Clear();

            // Load Oxytocin
            var oxytocinEntries = await _oxytocinRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in oxytocinEntries)
            {
                MedicationEntries.Add(new MedicationViewModel
                {
                    Time = entry.Time,
                    Name = "Oxytocin",
                    Details = $"Dose: {entry.TotalVolumeInfused} U/L, Rate: {entry.DoseMUnitsPerMin} drops/min"
                });
            }

            // Load Medications
            var medications = await _medicationEntryRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in medications)
            {
                MedicationEntries.Add(new MedicationViewModel
                {
                    Time = entry.Time,
                    Name = entry.MedicationName ?? "Unknown",
                    Details = $"Dose: {entry.Dose}, Route: {entry.Route}"
                });
            }

            // Load IV Fluids
            var ivFluids = await _ivFluidEntryRepository.ListByPatientAsync(_partographId.Value);
            foreach (var entry in ivFluids)
            {
                MedicationEntries.Add(new MedicationViewModel
                {
                    Time = entry.Time,
                    Name = $"IV Fluid: {entry.FluidType}",
                    Details = $"Volume: {entry.VolumeInfused}mL, Rate: {entry.RateMlPerHour}mL/hr"
                });
            }

            // Sort by time
            var sorted = MedicationEntries.OrderByDescending(m => m.Time).ToList();
            MedicationEntries.Clear();
            foreach (var med in sorted)
            {
                MedicationEntries.Add(med);
            }
        }

        private async Task GenerateClinicalSummaryAsync()
        {
            if (_partograph == null || _partographId == null) return;

            try
            {
                ClinicalNotesSummary = await _notesService.GenerateFullPartographNotes(_partographId.Value);
            }
            catch
            {
                ClinicalNotesSummary = "";
            }
        }

        #region Helper Methods

        private static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
            {
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            }
            return $"{duration.Minutes}m";
        }

        private static string GetFhrStatus(int heartRate)
        {
            if (heartRate < 110) return "Low";
            if (heartRate > 160) return "High";
            return "Normal";
        }

        private static Color GetFhrStatusColor(int heartRate)
        {
            if (heartRate < 110 || heartRate > 160) return Color.FromArgb("#EF4444");
            return Color.FromArgb("#10B981");
        }

        private static string GetBpStatus(int systolic, int diastolic)
        {
            if (systolic >= 140 || diastolic >= 90) return "High";
            if (systolic < 90 || diastolic < 60) return "Low";
            return "Normal";
        }

        private static Color GetBpStatusColor(int systolic, int diastolic)
        {
            if (systolic >= 140 || diastolic >= 90) return Color.FromArgb("#EF4444");
            if (systolic < 90 || diastolic < 60) return Color.FromArgb("#F59E0B");
            return Color.FromArgb("#10B981");
        }

        private static string GetTempStatus(double celsius)
        {
            if (celsius >= 37.5) return "Fever";
            if (celsius < 36.0) return "Low";
            return "Normal";
        }

        private static Color GetTempStatusColor(double celsius)
        {
            if (celsius >= 37.5) return Color.FromArgb("#EF4444");
            if (celsius < 36.0) return Color.FromArgb("#F59E0B");
            return Color.FromArgb("#10B981");
        }

        private static string GetAmnioticStatus(string color)
        {
            return color switch
            {
                "Clear" => "Normal",
                "Meconium" => "Alert",
                "Bloody" => "Alert",
                _ => "Recorded"
            };
        }

        private static Color GetAmnioticStatusColor(string color)
        {
            return color switch
            {
                "Clear" => Color.FromArgb("#10B981"),
                "Meconium" => Color.FromArgb("#EF4444"),
                "Bloody" => Color.FromArgb("#EF4444"),
                _ => Color.FromArgb("#F59E0B")
            };
        }

        private static Color GetNoteTypeColor(string? noteType)
        {
            return noteType?.ToLower() switch
            {
                "alert" => Color.FromArgb("#EF4444"),
                "emergency" => Color.FromArgb("#DC2626"),
                "warning" => Color.FromArgb("#F59E0B"),
                _ => Color.FromArgb("#6B7280")
            };
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task PrintPartograph()
        {
            if (_partographId == null) return;

            try
            {
                IsBusy = true;
                await AppShell.DisplayToastAsync("Generating PDF...");

                var filePath = await _pdfService.GenerateAndSavePartographPdfAsync(
                    _partographId.Value,
                    PatientName);

                await AppShell.DisplayToastAsync($"PDF saved to: {filePath}");

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

        [RelayCommand]
        private async Task ExportPartograph()
        {
            await AppShell.DisplayToastAsync("Export feature coming soon");
        }

        [RelayCommand]
        private async Task ViewAllMeasurements()
        {
            await AppShell.DisplayToastAsync("Showing all measurements...");
            // This could navigate to a detailed measurements page in the future
        }

        #endregion
    }
}
