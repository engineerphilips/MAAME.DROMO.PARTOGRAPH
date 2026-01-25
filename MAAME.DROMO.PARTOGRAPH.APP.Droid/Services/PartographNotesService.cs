using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    /// <summary>
    /// Generates dynamic clinical notes based on partograph measurements
    /// Following WHO 2020 guidelines for labor monitoring documentation
    /// </summary>
    public class PartographNotesService
    {
        private readonly ILogger<PartographNotesService> _logger;
        private readonly PartographRepository _partographRepository;
        private readonly PatientRepository _patientRepository;
        private readonly FHRRepository _fhrRepository;
        private readonly ContractionRepository _contractionRepository;
        private readonly CervixDilatationRepository _cervixDilatationRepository;
        private readonly HeadDescentRepository _headDescentRepository;
        private readonly BPRepository _bpRepository;
        private readonly TemperatureRepository _temperatureRepository;
        private readonly AmnioticFluidRepository _amnioticFluidRepository;
        private readonly FetalPositionRepository _fetalPositionRepository;
        private readonly CaputRepository _caputRepository;
        private readonly MouldingRepository _mouldingRepository;
        private readonly OxytocinRepository _oxytocinRepository;
        private readonly MedicationEntryRepository _medicationEntryRepository;
        private readonly AssessmentRepository _assessmentRepository;
        private readonly PlanRepository _planRepository;
        private readonly CompanionRepository _companionRepository;
        private readonly PostureRepository _postureRepository;
        private readonly PainReliefRepository _painReliefRepository;

        public PartographNotesService(
            ILogger<PartographNotesService> logger,
            PartographRepository partographRepository,
            PatientRepository patientRepository,
            FHRRepository fhrRepository,
            ContractionRepository contractionRepository,
            CervixDilatationRepository cervixDilatationRepository,
            HeadDescentRepository headDescentRepository,
            BPRepository bpRepository,
            TemperatureRepository temperatureRepository,
            AmnioticFluidRepository amnioticFluidRepository,
            FetalPositionRepository fetalPositionRepository,
            CaputRepository caputRepository,
            MouldingRepository mouldingRepository,
            OxytocinRepository oxytocinRepository,
            MedicationEntryRepository medicationEntryRepository,
            AssessmentRepository assessmentRepository,
            PlanRepository planRepository,
            CompanionRepository companionRepository,
            PostureRepository postureRepository,
            PainReliefRepository painReliefRepository)
        {
            _logger = logger;
            _partographRepository = partographRepository;
            _patientRepository = patientRepository;
            _fhrRepository = fhrRepository;
            _contractionRepository = contractionRepository;
            _cervixDilatationRepository = cervixDilatationRepository;
            _headDescentRepository = headDescentRepository;
            _bpRepository = bpRepository;
            _temperatureRepository = temperatureRepository;
            _amnioticFluidRepository = amnioticFluidRepository;
            _fetalPositionRepository = fetalPositionRepository;
            _caputRepository = caputRepository;
            _mouldingRepository = mouldingRepository;
            _oxytocinRepository = oxytocinRepository;
            _medicationEntryRepository = medicationEntryRepository;
            _assessmentRepository = assessmentRepository;
            _planRepository = planRepository;
            _companionRepository = companionRepository;
            _postureRepository = postureRepository;
            _painReliefRepository = painReliefRepository;
        }

        /// <summary>
        /// Generates comprehensive clinical notes for a partograph by loading all data from repositories
        /// </summary>
        /// <param name="partographId">The ID of the partograph</param>
        /// <returns>Generated clinical notes as a formatted string</returns>
        public async Task<string> GenerateFullPartographNotes(Guid partographId)
        {
            try
            {
                // Load the partograph
                var partograph = await _partographRepository.GetAsync(partographId);
                if (partograph == null)
                {
                    _logger.LogWarning("Partograph not found for ID: {PartographId}", partographId);
                    return "No partograph data available.";
                }

                // Load the patient
                Patient? patient = null;
                if (partograph.PatientID.HasValue)
                {
                    patient = await _patientRepository.GetAsync(partograph.PatientID.Value);
                }

                // Load all measurements in parallel for efficiency
                var fhrTask = _fhrRepository.ListByPatientAsync(partographId);
                var contractionTask = _contractionRepository.ListByPatientAsync(partographId);
                var dilatationTask = _cervixDilatationRepository.ListByPatientAsync(partographId);
                var descentTask = _headDescentRepository.ListByPatientAsync(partographId);
                var bpTask = _bpRepository.ListByPatientAsync(partographId);
                var tempTask = _temperatureRepository.ListByPatientAsync(partographId);
                var amnioticTask = _amnioticFluidRepository.ListByPatientAsync(partographId);
                var positionTask = _fetalPositionRepository.ListByPatientAsync(partographId);
                var caputTask = _caputRepository.ListByPatientAsync(partographId);
                var mouldingTask = _mouldingRepository.ListByPatientAsync(partographId);
                var oxytocinTask = _oxytocinRepository.ListByPatientAsync(partographId);
                var medicationTask = _medicationEntryRepository.ListByPatientAsync(partographId);
                var assessmentTask = _assessmentRepository.ListByPatientAsync(partographId);
                var planTask = _planRepository.ListByPatientAsync(partographId);
                var companionTask = _companionRepository.ListByPatientAsync(partographId);
                var postureTask = _postureRepository.ListByPatientAsync(partographId);
                var painReliefTask = _painReliefRepository.ListByPatientAsync(partographId);

                await Task.WhenAll(
                    fhrTask, contractionTask, dilatationTask, descentTask,
                    bpTask, tempTask, amnioticTask, positionTask,
                    caputTask, mouldingTask, oxytocinTask, medicationTask,
                    assessmentTask, planTask, companionTask, postureTask, painReliefTask);

                // Populate the partograph with loaded measurements
                partograph.Fhrs = fhrTask.Result.ToList();
                partograph.Contractions = contractionTask.Result.ToList();
                partograph.Dilatations = dilatationTask.Result.ToList();
                partograph.HeadDescents = descentTask.Result.ToList();
                partograph.BPs = bpTask.Result.ToList();
                partograph.Temperatures = tempTask.Result.ToList();
                partograph.AmnioticFluids = amnioticTask.Result.ToList();
                partograph.FetalPositions = positionTask.Result.ToList();
                partograph.Caputs = caputTask.Result.ToList();
                partograph.Mouldings = mouldingTask.Result.ToList();
                partograph.Oxytocins = oxytocinTask.Result.ToList();
                partograph.Medications = medicationTask.Result.ToList();
                partograph.Assessments = assessmentTask.Result.ToList();
                partograph.Plans = planTask.Result.ToList();
                partograph.Companions = companionTask.Result.ToList();
                partograph.Postures = postureTask.Result.ToList();
                partograph.PainReliefs = painReliefTask.Result.ToList();

                // Generate the clinical notes using the existing method
                return GenerateClinicalNotes(partograph, patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating full partograph notes for ID: {PartographId}", partographId);
                return "Error generating clinical notes.";
            }
        }

        /// <summary>
        /// Generates comprehensive clinical notes for the entire partograph
        /// </summary>
        public string GenerateClinicalNotes(Partograph partograph, Patient patient)
        {
            if (partograph == null)
                return "No partograph data available.";

            var notes = new StringBuilder();

            // Get all measurement time points
            var timePoints = GetDistinctTimePoints(partograph);

            if (!timePoints.Any())
            {
                return "No measurements recorded yet.";
            }

            // Generate notes for each time point (sorted chronologically)
            var sortedTimePoints = timePoints.OrderBy(t => t).ToList();
            foreach (var timePoint in sortedTimePoints)
            {
                var timeNote = GenerateNoteForTimePoint(partograph, patient, timePoint, timePoint == sortedTimePoints.First());
                if (!string.IsNullOrWhiteSpace(timeNote))
                {
                    notes.AppendLine(timeNote);
                    notes.AppendLine();
                }
            }

            // Add delivery information if available
            if (partograph.DeliveryTime.HasValue)
            {
                notes.AppendLine(GenerateDeliveryNote(partograph));
                notes.AppendLine();
            }

            return notes.ToString().TrimEnd();
        }

        /// <summary>
        /// Generates a clinical note for a specific time point
        /// </summary>
        private string GenerateNoteForTimePoint(Partograph partograph, Patient patient, DateTime timePoint, bool isAdmission)
        {
            var note = new StringBuilder();

            // Header with date and time
            note.AppendLine($"Date: {timePoint:MMMM dd, yyyy} Time: {timePoint:HH:mm}");

            // Admission vs subsequent observation
            if (isAdmission)
            {
                note.Append(GenerateAdmissionNote(partograph, patient, timePoint));
            }
            else
            {
                note.Append(GenerateObservationNote(partograph, patient, timePoint));
            }

            // Add clinical actions/decisions
            var actions = GetClinicalActions(partograph, timePoint);
            if (!string.IsNullOrWhiteSpace(actions))
            {
                note.AppendLine(actions);
            }

            return note.ToString();
        }

        /// <summary>
        /// Generates admission note (first observation)
        /// </summary>
        private string GenerateAdmissionNote(Partograph partograph, Patient patient, DateTime timePoint)
        {
            var note = new StringBuilder();
            var patientName = patient != null ? $"{patient.FirstName} {patient.LastName}" : "the patient";

            note.Append($"At the time of admission, {patientName} presented with ");

            // Contractions
            var contraction = GetMeasurementAtTime(partograph.Contractions, timePoint);
            if (contraction != null)
            {
                var intensity = DetermineContractionIntensity(contraction.DurationSeconds);
                note.Append($"{contraction.FrequencyPer10Min} uterine contractions every 10 minutes, " +
                           $"of {intensity} intensity, and lasting {contraction.DurationSeconds} seconds.\n");
            }
            else
            {
                note.Append("no recorded contractions.\n");
            }

            // Vaginal examination findings
            var dilation = GetMeasurementAtTime(partograph.Dilatations, timePoint);
            var descent = GetMeasurementAtTime(partograph.HeadDescents, timePoint);
            var position = GetMeasurementAtTime(partograph.FetalPositions, timePoint);

            if (dilation != null || descent != null || position != null)
            {
                note.Append("Vaginal examination shows ");

                if (dilation != null)
                {
                    note.Append($"{dilation.DilatationCm} cm cervical dilatation");
                }

                if (position != null)
                {
                    note.Append(dilation != null ? ", " : "");
                    note.Append($"{GetPositionDescription(position.Position)} presentation");
                }

                if (descent != null)
                {
                    note.Append(". ");
                    note.Append($"Fetal descent is {descent.Station}");
                }

                note.AppendLine(".");
            }

            // Assessment
            note.Append(GetGeneralAssessment(partograph, timePoint, patientName));

            return note.ToString();
        }

        /// <summary>
        /// Generates observation note for subsequent time points
        /// </summary>
        private string GenerateObservationNote(Partograph partograph, Patient patient, DateTime timePoint)
        {
            var note = new StringBuilder();
            var patientName = patient != null ? $"{patient.FirstName} {patient.LastName}" : "the patient";

            // Check for complaints or status changes
            var complaints = GetPatientComplaints(partograph, timePoint);
            if (!string.IsNullOrWhiteSpace(complaints))
            {
                note.AppendLine(complaints);
            }

            // Vital signs
            var vitals = GetVitalSigns(partograph, timePoint, patientName);
            if (!string.IsNullOrWhiteSpace(vitals))
            {
                note.Append(vitals);
            }

            // Fetal observations
            var fetalObs = GetFetalObservations(partograph, timePoint);
            if (!string.IsNullOrWhiteSpace(fetalObs))
            {
                note.Append(fetalObs);
            }

            // Labor progress
            var laborProgress = GetLaborProgress(partograph, timePoint);
            if (!string.IsNullOrWhiteSpace(laborProgress))
            {
                note.Append(laborProgress);
            }

            return note.ToString();
        }

        /// <summary>
        /// Gets vital signs narrative
        /// </summary>
        private string GetVitalSigns(Partograph partograph, DateTime timePoint, string patientName)
        {
            var vitals = new List<string>();

            var bp = GetMeasurementAtTime(partograph.BPs, timePoint);
            if (bp != null)
            {
                vitals.Add($"blood pressure {bp.Systolic}/{bp.Diastolic} mmHg");
                if (bp.Pulse > 0)
                {
                    vitals.Add($"heart rate {bp.Pulse} bpm");
                }
            }

            var temp = GetMeasurementAtTime(partograph.Temperatures, timePoint);
            if (temp != null)
            {
                vitals.Add($"temperature {temp.TemperatureCelsius}°C");
            }

            if (vitals.Any())
            {
                return $"Vitals are {string.Join(", ", vitals)}. ";
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets fetal observations narrative
        /// </summary>
        private string GetFetalObservations(Partograph partograph, DateTime timePoint)
        {
            var obs = new StringBuilder();

            var fhr = GetMeasurementAtTime(partograph.Fhrs, timePoint);
            if (fhr != null)
            {
                obs.Append($"FHR is {fhr.Rate} bpm");
                if (!string.IsNullOrWhiteSpace(fhr.Deceleration) && fhr.Deceleration != "None")
                {
                    obs.Append($" with {fhr.Deceleration.ToLower()} decelerations");
                }
                obs.Append(". ");
            }

            var fluid = GetMeasurementAtTime(partograph.AmnioticFluids, timePoint);
            if (fluid != null && !string.IsNullOrWhiteSpace(fluid.Color))
            {
                obs.Append($"Amniotic fluid shows {fluid.Color.ToLower()}. ");
            }

            return obs.ToString();
        }

        /// <summary>
        /// Gets labor progress narrative
        /// </summary>
        private string GetLaborProgress(Partograph partograph, DateTime timePoint)
        {
            var progress = new StringBuilder();

            var contraction = GetMeasurementAtTime(partograph.Contractions, timePoint);
            if (contraction != null)
            {
                var intensity = DetermineContractionIntensityAdjective(contraction.DurationSeconds);
                progress.Append($"Patient maintains {contraction.FrequencyPer10Min} ");
                if (!string.IsNullOrWhiteSpace(intensity))
                {
                    progress.Append($"{intensity} ");
                }
                progress.Append($"uterine contractions in 10 minutes, lasting {contraction.DurationSeconds} seconds each. ");
            }

            var descent = GetMeasurementAtTime(partograph.HeadDescents, timePoint);
            if (descent != null)
            {
                progress.Append($"Fetal descent is {descent.Station}. ");
            }

            var dilation = GetMeasurementAtTime(partograph.Dilatations, timePoint);
            if (dilation != null)
            {
                progress.Append($"Cervical dilatation is {dilation.DilatationCm} cm");

                var position = GetMeasurementAtTime(partograph.FetalPositions, timePoint);
                if (position != null)
                {
                    progress.Append($" and the fetal position is {position.Position}");
                }
                progress.Append(". ");
            }

            var moulding = GetMeasurementAtTime(partograph.Mouldings, timePoint);
            if (moulding != null && !string.IsNullOrWhiteSpace(moulding.DegreeDisplay))
            {
                progress.Append($"Moulding: {moulding.DegreeDisplay}. ");
            }

            var caput = GetMeasurementAtTime(partograph.Caputs, timePoint);
            if (caput != null && !string.IsNullOrWhiteSpace(caput.CaputDisplay))
            {
                progress.Append($"Caput: {caput.CaputDisplay}. ");
            }

            return progress.ToString();
        }

        /// <summary>
        /// Gets clinical actions taken at this time point
        /// </summary>
        private string GetClinicalActions(Partograph partograph, DateTime timePoint)
        {
            var actions = new StringBuilder();

            // Check for medication/interventions
            var oxytocin = GetMeasurementAtTime(partograph.Oxytocins, timePoint);
            if (oxytocin != null)
            {
                actions.AppendLine($"Oxytocin administered: {oxytocin.DoseMUnitsPerMin} drops/minute.");
            }

            var medication = GetMeasurementAtTime(partograph.Medications, timePoint);
            if (medication != null && !string.IsNullOrWhiteSpace(medication.MedicationName))
            {
                actions.AppendLine($"Medication administered: {medication.MedicationName}" +
                    (!string.IsNullOrWhiteSpace(medication.Dose) ? $" ({medication.Dose})" : "") + ".");
            }

            // Check for care support
            var companion = GetMeasurementAtTime(partograph.Companions, timePoint);
            if (companion != null && companion.Companion == "Y")
            {
                actions.AppendLine($"Companion present: {companion.CompanionDisplay ?? "family member"}.");
            }

            var painRelief = GetMeasurementAtTime(partograph.PainReliefs, timePoint);
            if (painRelief != null && !string.IsNullOrWhiteSpace(painRelief.PainReliefDisplay))
            {
                actions.AppendLine($"Pain relief: {painRelief.PainReliefDisplay}.");
            }

            var posture = GetMeasurementAtTime(partograph.Postures, timePoint);
            if (posture != null && !string.IsNullOrWhiteSpace(posture.PostureDisplay))
            {
                actions.AppendLine($"Maternal position: {posture.PostureDisplay.ToLower()}.");
            }

            // Check for assessment/plan
            var assessment = GetMeasurementAtTime(partograph.Assessments, timePoint);
            if (assessment != null && !string.IsNullOrWhiteSpace(assessment.Notes))
            {
                actions.AppendLine($"Assessment: {assessment.Notes}");
            }

            var plan = GetMeasurementAtTime(partograph.Plans, timePoint);
            if (plan != null && !string.IsNullOrWhiteSpace(plan.Notes))
            {
                actions.AppendLine($"Plan: {plan.Notes}");
            }

            return actions.ToString().TrimEnd();
        }

        /// <summary>
        /// Gets patient complaints if any
        /// </summary>
        private string GetPatientComplaints(Partograph partograph, DateTime timePoint)
        {
            // Check assessments for complaints
            var assessment = GetMeasurementAtTime(partograph.Assessments, timePoint);
            if (assessment != null && !string.IsNullOrWhiteSpace(assessment.Notes))
            {
                if (assessment.Notes.ToLower().Contains("complain") ||
                    assessment.Notes.ToLower().Contains("pain") ||
                    assessment.Notes.ToLower().Contains("discomfort"))
                {
                    return assessment.Notes;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets general assessment for admission
        /// </summary>
        private string GetGeneralAssessment(Partograph partograph, DateTime timePoint, string patientName)
        {
            var assessment = new StringBuilder();

            // Check if parameters are normal
            var fhr = GetMeasurementAtTime(partograph.Fhrs, timePoint);
            var bp = GetMeasurementAtTime(partograph.BPs, timePoint);
            var temp = GetMeasurementAtTime(partograph.Temperatures, timePoint);

            bool parametersNormal = true;
            if (fhr != null && (fhr.Rate < 110 || fhr.Rate > 160)) parametersNormal = false;
            if (bp != null && (bp.Systolic > 140 || bp.Diastolic > 90)) parametersNormal = false;
            if (temp != null && (temp.TemperatureCelsius < 36.5 || temp.TemperatureCelsius > 37.5)) parametersNormal = false;

            if (parametersNormal)
            {
                assessment.AppendLine($"Given that all other clinical parameters are normal and that {patientName} is coping with the labour, " +
                    "the midwife assesses the number and duration of uterine contractions half-hourly.");
                assessment.AppendLine("Unnecessary vaginal examinations are avoided and vaginal examinations are only performed after 4 hours.");
            }

            return assessment.ToString();
        }

        /// <summary>
        /// Generates delivery note
        /// </summary>
        private string GenerateDeliveryNote(Partograph partograph)
        {
            if (!partograph.DeliveryTime.HasValue)
                return string.Empty;

            var deliveryMode = partograph.Status == LaborStatus.Completed ? "vaginally" : "via caesarean section";
            return $"Childbirth takes place {deliveryMode} at {partograph.DeliveryTime.Value:HH:mm}.";
        }

        /// <summary>
        /// Gets all distinct time points from all measurements
        /// </summary>
        private List<DateTime> GetDistinctTimePoints(Partograph partograph)
        {
            var times = new HashSet<DateTime>();

            // Add times from all measurement types
            AddTimes(times, partograph.Contractions);
            AddTimes(times, partograph.Dilatations);
            AddTimes(times, partograph.HeadDescents);
            AddTimes(times, partograph.Fhrs);
            AddTimes(times, partograph.BPs);
            AddTimes(times, partograph.Temperatures);
            AddTimes(times, partograph.AmnioticFluids);
            AddTimes(times, partograph.FetalPositions);
            AddTimes(times, partograph.Mouldings);
            AddTimes(times, partograph.Caputs);
            AddTimes(times, partograph.Oxytocins);
            AddTimes(times, partograph.Medications);
            AddTimes(times, partograph.Companions);
            AddTimes(times, partograph.PainReliefs);
            AddTimes(times, partograph.Postures);
            AddTimes(times, partograph.Assessments);
            AddTimes(times, partograph.Plans);

            return times.ToList();
        }

        /// <summary>
        /// Helper to add times from a collection
        /// </summary>
        private void AddTimes<T>(HashSet<DateTime> times, ICollection<T> measurements) where T : BasePartographMeasurement
        {
            if (measurements != null)
            {
                foreach (var m in measurements)
                {
                    times.Add(m.Time);
                }
            }
        }

        /// <summary>
        /// Gets measurement closest to the specified time point
        /// </summary>
        private T GetMeasurementAtTime<T>(ICollection<T> measurements, DateTime timePoint) where T : BasePartographMeasurement
        {
            if (measurements == null || !measurements.Any())
                return null;

            // Get measurements within 1 minute of the time point
            return measurements
                .Where(m => Math.Abs((m.Time - timePoint).TotalMinutes) < 1)
                .OrderBy(m => Math.Abs((m.Time - timePoint).Ticks))
                .FirstOrDefault();
        }

        /// <summary>
        /// Determines contraction intensity from duration
        /// </summary>
        private string DetermineContractionIntensity(int durationSeconds)
        {
            if (durationSeconds < 30) return "mild";
            if (durationSeconds < 45) return "moderate";
            return "strong";
        }

        /// <summary>
        /// Determines contraction intensity adjective
        /// </summary>
        private string DetermineContractionIntensityAdjective(int durationSeconds)
        {
            if (durationSeconds >= 45) return "strong";
            return string.Empty;
        }

        /// <summary>
        /// Gets position description
        /// </summary>
        private string GetPositionDescription(string position)
        {
            if (string.IsNullOrWhiteSpace(position))
                return "cephalic";

            if (position.ToLower().Contains("cephalic") || position.ToLower().StartsWith("o"))
                return "cephalic";
            if (position.ToLower().Contains("breech"))
                return "breech";

            return position.ToLower();
        }
    }
}
