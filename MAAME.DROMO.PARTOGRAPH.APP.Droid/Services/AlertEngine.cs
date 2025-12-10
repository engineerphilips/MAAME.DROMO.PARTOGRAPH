using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    public class AlertEngine
    {
        private readonly List<ClinicalAlert> _activeAlerts = new List<ClinicalAlert>();

        public event EventHandler<ClinicalAlert>? AlertTriggered;
        public event EventHandler<Guid>? AlertCleared;

        /// <summary>
        /// Analyzes all patient measurements and generates clinical alerts
        /// </summary>
        public List<ClinicalAlert> AnalyzePatient(Partograph patient)
        {
            var newAlerts = new List<ClinicalAlert>();

            if (patient == null)
                return newAlerts;

            // Analyze labor progression
            newAlerts.AddRange(AnalyzeLaborProgression(patient));

            // Analyze fetal wellbeing
            newAlerts.AddRange(AnalyzeFetalWellbeing(patient));

            // Analyze maternal vital signs
            newAlerts.AddRange(AnalyzeMaternalVitals(patient));

            // Analyze contractions
            newAlerts.AddRange(AnalyzeContractions(patient));

            // Analyze hydration
            newAlerts.AddRange(AnalyzeHydration(patient));

            // Update active alerts list
            UpdateActiveAlerts(newAlerts);

            return newAlerts;
        }

        /// <summary>
        /// Analyzes labor progression against WHO alert/action lines
        /// </summary>
        private List<ClinicalAlert> AnalyzeLaborProgression(Partograph patient)
        {
            var alerts = new List<ClinicalAlert>();

            if (!patient.Dilatations.Any() || !patient.LaborStartTime.HasValue)
                return alerts;

            var dilatations = patient.Dilatations.OrderBy(d => d.Time).ToList();
            var latestDilatation = dilatations.Last();

            // Find when patient reached 4cm (active labor start)
            var fourCmEntry = dilatations.FirstOrDefault(d => d.DilatationCm >= 4);
            if (fourCmEntry == null)
                return alerts;

            var hoursSinceFourCm = (latestDilatation.Time - fourCmEntry.Time).TotalHours;
            var expectedDilatation = 4 + hoursSinceFourCm; // 1cm per hour expected
            var actualDilatation = latestDilatation.DilatationCm;

            // Alert line - 4 hours from 4cm to 10cm (1.5cm/hr)
            var alertLineExpected = 4 + (hoursSinceFourCm * 1.5);

            // Action line - 2 hours behind alert line
            var actionLineExpected = 4 + Math.Max(0, (hoursSinceFourCm - 2) * 1.5);

            if (actualDilatation < actionLineExpected && hoursSinceFourCm > 2)
            {
                // Crossing action line - CRITICAL
                alerts.Add(new ClinicalAlert
                {
                    Severity = AlertSeverity.Critical,
                    Category = AlertCategory.Labor,
                    Title = "Labor Progression: Action Line Crossed",
                    Message = $"Cervical dilatation is progressing slower than expected. Current: {actualDilatation}cm, Expected: {actionLineExpected:F1}cm.",
                    RecommendedActions = new List<string>
                    {
                        "Assess for cephalopelvic disproportion (CPD)",
                        "Consider augmentation with oxytocin if no contraindications",
                        "Inform senior obstetrician",
                        "Review for need of operative delivery",
                        "Ensure adequate hydration and pain relief"
                    },
                    MeasurementType = "Cervical Dilatation",
                    CurrentValue = $"{actualDilatation} cm",
                    ExpectedRange = $">{actionLineExpected:F1} cm at this time"
                });
            }
            else if (actualDilatation < alertLineExpected && hoursSinceFourCm > 1)
            {
                // Crossing alert line - WARNING
                alerts.Add(new ClinicalAlert
                {
                    Severity = AlertSeverity.Warning,
                    Category = AlertCategory.Labor,
                    Title = "Labor Progression: Alert Line Crossed",
                    Message = $"Labor is progressing slower than optimal. Current: {actualDilatation}cm, Expected: {alertLineExpected:F1}cm.",
                    RecommendedActions = new List<string>
                    {
                        "Assess for adequate contractions (3-5 in 10 minutes)",
                        "Ensure adequate hydration",
                        "Optimize maternal position and mobility",
                        "Review pain relief adequacy",
                        "Monitor closely for further delays"
                    },
                    MeasurementType = "Cervical Dilatation",
                    CurrentValue = $"{actualDilatation} cm",
                    ExpectedRange = $">{alertLineExpected:F1} cm at this time"
                });
            }

            // Check for prolonged labor (>12 hours in active phase)
            if (hoursSinceFourCm > 12 && actualDilatation < 10)
            {
                alerts.Add(new ClinicalAlert
                {
                    Severity = AlertSeverity.Critical,
                    Category = AlertCategory.Labor,
                    Title = "Prolonged Labor",
                    Message = $"Active labor has exceeded 12 hours ({hoursSinceFourCm:F1} hours). Current dilatation: {actualDilatation}cm.",
                    RecommendedActions = new List<string>
                    {
                        "Senior obstetrician review required",
                        "Assess maternal exhaustion",
                        "Consider operative delivery",
                        "Review augmentation strategy",
                        "Monitor maternal and fetal wellbeing closely"
                    },
                    MeasurementType = "Labor Duration",
                    CurrentValue = $"{hoursSinceFourCm:F1} hours",
                    ExpectedRange = "<12 hours in active phase"
                });
            }

            return alerts;
        }

        /// <summary>
        /// Analyzes fetal heart rate for abnormal patterns
        /// </summary>
        private List<ClinicalAlert> AnalyzeFetalWellbeing(Partograph patient)
        {
            var alerts = new List<ClinicalAlert>();

            if (!patient.Fhrs.Any())
                return alerts;

            var latestFHR = patient.Fhrs.OrderByDescending(f => f.Time).First();

            // Critical bradycardia or tachycardia
            if (latestFHR.Rate < AlertThresholds.FHR_CRITICAL_MIN ||
                latestFHR.Rate > AlertThresholds.FHR_CRITICAL_MAX)
            {
                string condition = latestFHR.Rate < AlertThresholds.FHR_CRITICAL_MIN ? "Bradycardia" : "Tachycardia";

                alerts.Add(new ClinicalAlert
                {
                    Severity = AlertSeverity.Critical,
                    Category = AlertCategory.Fetal,
                    Title = $"Fetal Heart Rate: Severe {condition}",
                    Message = $"FHR is {latestFHR.Rate} bpm - outside critical range ({AlertThresholds.FHR_CRITICAL_MIN}-{AlertThresholds.FHR_CRITICAL_MAX} bpm).",
                    RecommendedActions = new List<string>
                    {
                        "Immediate senior obstetrician notification",
                        "Change maternal position (left lateral)",
                        "Administer oxygen to mother",
                        "Check for cord prolapse if membranes ruptured",
                        "Prepare for potential emergency delivery",
                        "Continuous FHR monitoring"
                    },
                    MeasurementType = "Fetal Heart Rate",
                    CurrentValue = $"{latestFHR.Rate} bpm",
                    ExpectedRange = $"{AlertThresholds.FHR_NORMAL_MIN}-{AlertThresholds.FHR_NORMAL_MAX} bpm"
                });
            }
            // Warning range
            else if (latestFHR.Rate < AlertThresholds.FHR_NORMAL_MIN ||
                     latestFHR.Rate > AlertThresholds.FHR_NORMAL_MAX)
            {
                string condition = latestFHR.Rate < AlertThresholds.FHR_NORMAL_MIN ? "Bradycardia" : "Tachycardia";

                alerts.Add(new ClinicalAlert
                {
                    Severity = AlertSeverity.Warning,
                    Category = AlertCategory.Fetal,
                    Title = $"Fetal Heart Rate: {condition}",
                    Message = $"FHR is {latestFHR.Rate} bpm - outside normal range.",
                    RecommendedActions = new List<string>
                    {
                        "Recheck FHR in 15 minutes",
                        "Assess maternal vital signs",
                        "Ensure adequate maternal hydration",
                        "Consider maternal position change",
                        "Inform attending physician if persists"
                    },
                    MeasurementType = "Fetal Heart Rate",
                    CurrentValue = $"{latestFHR.Rate} bpm",
                    ExpectedRange = $"{AlertThresholds.FHR_NORMAL_MIN}-{AlertThresholds.FHR_NORMAL_MAX} bpm"
                });
            }

            // Check for deceleration patterns
            if (!string.IsNullOrEmpty(latestFHR.Deceleration) && latestFHR.Deceleration != "None")
            {
                var severity = latestFHR.Deceleration.ToLower().Contains("late") ?
                    AlertSeverity.Critical : AlertSeverity.Warning;

                alerts.Add(new ClinicalAlert
                {
                    Severity = severity,
                    Category = AlertCategory.Fetal,
                    Title = $"FHR Deceleration Detected: {latestFHR.Deceleration}",
                    Message = $"{latestFHR.Deceleration} decelerations observed. This may indicate fetal compromise.",
                    RecommendedActions = new List<string>
                    {
                        latestFHR.Deceleration.ToLower().Contains("late") ?
                            "URGENT: Late decelerations suggest uteroplacental insufficiency" :
                            "Variable decelerations may indicate cord compression",
                        "Change maternal position",
                        "Discontinue oxytocin if running",
                        "Administer oxygen to mother",
                        "Prepare for possible operative delivery",
                        "Senior obstetrician review"
                    },
                    MeasurementType = "FHR Pattern",
                    CurrentValue = latestFHR.Deceleration,
                    ExpectedRange = "No decelerations"
                });
            }

            return alerts;
        }

        /// <summary>
        /// Analyzes maternal vital signs for abnormalities
        /// </summary>
        private List<ClinicalAlert> AnalyzeMaternalVitals(Partograph patient)
        {
            var alerts = new List<ClinicalAlert>();

            // Blood Pressure Analysis
            if (patient.BPs.Any())
            {
                var latestBP = patient.BPs.OrderByDescending(b => b.Time).First();

                // Hypertension
                if (latestBP.Systolic >= AlertThresholds.BP_SYSTOLIC_CRITICAL ||
                    latestBP.Diastolic >= AlertThresholds.BP_DIASTOLIC_CRITICAL)
                {
                    alerts.Add(new ClinicalAlert
                    {
                        Severity = AlertSeverity.Critical,
                        Category = AlertCategory.Maternal,
                        Title = "Severe Hypertension Detected",
                        Message = $"BP: {latestBP.Systolic}/{latestBP.Diastolic} mmHg - Critical elevation.",
                        RecommendedActions = new List<string>
                        {
                            "URGENT: Assess for pre-eclampsia/eclampsia",
                            "Check for headache, visual disturbances, epigastric pain",
                            "Test urine for protein",
                            "Consider antihypertensive medication",
                            "Senior obstetrician notification",
                            "Prepare for potential emergency delivery",
                            "Monitor continuously"
                        },
                        MeasurementType = "Blood Pressure",
                        CurrentValue = $"{latestBP.Systolic}/{latestBP.Diastolic} mmHg",
                        ExpectedRange = $"<{AlertThresholds.BP_SYSTOLIC_CRITICAL}/{AlertThresholds.BP_DIASTOLIC_CRITICAL} mmHg"
                    });
                }
                else if (latestBP.Systolic >= AlertThresholds.BP_SYSTOLIC_WARNING ||
                         latestBP.Diastolic >= AlertThresholds.BP_DIASTOLIC_WARNING)
                {
                    alerts.Add(new ClinicalAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = AlertCategory.Maternal,
                        Title = "Elevated Blood Pressure",
                        Message = $"BP: {latestBP.Systolic}/{latestBP.Diastolic} mmHg - Above normal range.",
                        RecommendedActions = new List<string>
                        {
                            "Recheck BP in 15-30 minutes",
                            "Assess for pre-eclampsia symptoms",
                            "Test urine for protein",
                            "Inform attending physician",
                            "Consider more frequent monitoring"
                        },
                        MeasurementType = "Blood Pressure",
                        CurrentValue = $"{latestBP.Systolic}/{latestBP.Diastolic} mmHg",
                        ExpectedRange = $"<{AlertThresholds.BP_SYSTOLIC_WARNING}/{AlertThresholds.BP_DIASTOLIC_WARNING} mmHg"
                    });
                }

                // Hypotension
                if (latestBP.Systolic < AlertThresholds.BP_SYSTOLIC_LOW)
                {
                    alerts.Add(new ClinicalAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = AlertCategory.Maternal,
                        Title = "Hypotension Detected",
                        Message = $"BP: {latestBP.Systolic}/{latestBP.Diastolic} mmHg - Low systolic pressure.",
                        RecommendedActions = new List<string>
                        {
                            "Assess for hemorrhage or shock",
                            "Check maternal hydration status",
                            "Consider IV fluid bolus",
                            "Review epidural if in use",
                            "Monitor maternal consciousness and symptoms"
                        },
                        MeasurementType = "Blood Pressure",
                        CurrentValue = $"{latestBP.Systolic}/{latestBP.Diastolic} mmHg",
                        ExpectedRange = $">{AlertThresholds.BP_SYSTOLIC_LOW} mmHg systolic"
                    });
                }

                // Pulse
                if (latestBP.Pulse > AlertThresholds.PULSE_WARNING_MAX)
                {
                    alerts.Add(new ClinicalAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = AlertCategory.Maternal,
                        Title = "Maternal Tachycardia",
                        Message = $"Pulse: {latestBP.Pulse} bpm - Elevated.",
                        RecommendedActions = new List<string>
                        {
                            "Assess for maternal distress or anxiety",
                            "Check for dehydration",
                            "Review for signs of infection",
                            "Assess pain management adequacy",
                            "Consider fluid replacement"
                        },
                        MeasurementType = "Pulse",
                        CurrentValue = $"{latestBP.Pulse} bpm",
                        ExpectedRange = $"{AlertThresholds.PULSE_NORMAL_MIN}-{AlertThresholds.PULSE_NORMAL_MAX} bpm"
                    });
                }
            }

            // Temperature Analysis
            if (patient.Temperatures.Any())
            {
                var latestTemp = patient.Temperatures.OrderByDescending(t => t.Time).First();

                if (latestTemp.Rate >= AlertThresholds.TEMP_CRITICAL_MAX)
                {
                    alerts.Add(new ClinicalAlert
                    {
                        Severity = AlertSeverity.Critical,
                        Category = AlertCategory.Maternal,
                        Title = "High Fever Detected",
                        Message = $"Temperature: {latestTemp.Rate:F1}°C - Significant pyrexia.",
                        RecommendedActions = new List<string>
                        {
                            "Assess for chorioamnionitis (if membranes ruptured)",
                            "Check for other infection sources",
                            "Consider blood cultures",
                            "Initiate broad-spectrum antibiotics",
                            "Senior obstetrician review",
                            "Monitor fetal heart rate closely"
                        },
                        MeasurementType = "Temperature",
                        CurrentValue = $"{latestTemp.Rate:F1}°C",
                        ExpectedRange = $"<{AlertThresholds.TEMP_WARNING_MAX}°C"
                    });
                }
                else if (latestTemp.Rate >= AlertThresholds.TEMP_WARNING_MAX)
                {
                    alerts.Add(new ClinicalAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = AlertCategory.Maternal,
                        Title = "Elevated Temperature",
                        Message = $"Temperature: {latestTemp.Rate:F1}°C - Above normal.",
                        RecommendedActions = new List<string>
                        {
                            "Recheck temperature in 30 minutes",
                            "Assess for infection signs",
                            "Review time since membrane rupture",
                            "Consider paracetamol for comfort",
                            "Inform attending physician"
                        },
                        MeasurementType = "Temperature",
                        CurrentValue = $"{latestTemp.Rate:F1}°C",
                        ExpectedRange = $"{AlertThresholds.TEMP_NORMAL_MIN}-{AlertThresholds.TEMP_NORMAL_MAX}°C"
                    });
                }
                else if (latestTemp.Rate < AlertThresholds.TEMP_NORMAL_MIN)
                {
                    alerts.Add(new ClinicalAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = AlertCategory.Maternal,
                        Title = "Low Temperature",
                        Message = $"Temperature: {latestTemp.Rate:F1}°C - Below normal.",
                        RecommendedActions = new List<string>
                        {
                            "Ensure adequate room temperature",
                            "Provide warm blankets",
                            "Assess for shock",
                            "Recheck temperature"
                        },
                        MeasurementType = "Temperature",
                        CurrentValue = $"{latestTemp.Rate:F1}°C",
                        ExpectedRange = $">{AlertThresholds.TEMP_NORMAL_MIN}°C"
                    });
                }
            }

            // Urine Analysis
            if (patient.Urines.Any())
            {
                var latestUrine = patient.Urines.OrderByDescending(u => u.Time).First();

                if (AlertThresholds.URINE_PROTEIN_WARNING.Contains(latestUrine.Protein))
                {
                    alerts.Add(new ClinicalAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = AlertCategory.Maternal,
                        Title = "Significant Proteinuria",
                        Message = $"Urine protein: {latestUrine.Protein} - May indicate pre-eclampsia.",
                        RecommendedActions = new List<string>
                        {
                            "Assess blood pressure trending",
                            "Check for pre-eclampsia symptoms",
                            "Consider blood tests (LFTs, FBC, uric acid)",
                            "Inform obstetrician",
                            "Increase monitoring frequency"
                        },
                        MeasurementType = "Urine Protein",
                        CurrentValue = latestUrine.Protein,
                        ExpectedRange = "Negative or trace"
                    });
                }

                if (AlertThresholds.URINE_ACETONE_WARNING.Contains(latestUrine.Acetone))
                {
                    alerts.Add(new ClinicalAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = AlertCategory.Maternal,
                        Title = "Ketonuria Detected",
                        Message = $"Urine acetone: {latestUrine.Acetone} - May indicate dehydration or starvation ketosis.",
                        RecommendedActions = new List<string>
                        {
                            "Assess hydration status",
                            "Review oral/IV fluid intake",
                            "Consider IV dextrose if prolonged labor",
                            "Encourage oral fluids if appropriate",
                            "Monitor for diabetic ketoacidosis if diabetic"
                        },
                        MeasurementType = "Urine Acetone",
                        CurrentValue = latestUrine.Acetone,
                        ExpectedRange = "Negative"
                    });
                }
            }

            return alerts;
        }

        /// <summary>
        /// Analyzes contraction patterns for abnormalities
        /// </summary>
        private List<ClinicalAlert> AnalyzeContractions(Partograph patient)
        {
            var alerts = new List<ClinicalAlert>();

            if (!patient.Contractions.Any())
                return alerts;

            var latestContraction = patient.Contractions.OrderByDescending(c => c.Time).First();

            // Hyperstimulation
            if (latestContraction.FrequencyPer10Min > AlertThresholds.CONTRACTION_HYPERSTIMULATION)
            {
                alerts.Add(new ClinicalAlert
                {
                    Severity = AlertSeverity.Critical,
                    Category = AlertCategory.Labor,
                    Title = "Uterine Hyperstimulation",
                    Message = $"Contractions: {latestContraction.FrequencyPer10Min} in 10 minutes - Excessive frequency.",
                    RecommendedActions = new List<string>
                    {
                        "URGENT: Discontinue oxytocin immediately if running",
                        "Monitor fetal heart rate continuously",
                        "Change maternal position",
                        "Consider tocolytic if fetal distress",
                        "Inform senior obstetrician",
                        "Assess for uterine rupture risk"
                    },
                    MeasurementType = "Contraction Frequency",
                    CurrentValue = $"{latestContraction.FrequencyPer10Min} per 10 min",
                    ExpectedRange = $"≤{AlertThresholds.CONTRACTION_NORMAL_MAX} per 10 min"
                });
            }

            // Inadequate contractions in active labor
            if (latestContraction.FrequencyPer10Min < 3 && patient.Dilatations.Any())
            {
                var latestDilatation = patient.Dilatations.OrderByDescending(d => d.Time).FirstOrDefault();
                if (latestDilatation != null && latestDilatation.DilatationCm >= 4 && latestDilatation.DilatationCm < 10)
                {
                    alerts.Add(new ClinicalAlert
                    {
                        Severity = AlertSeverity.Warning,
                        Category = AlertCategory.Labor,
                        Title = "Inadequate Contractions",
                        Message = $"Contractions: {latestContraction.FrequencyPer10Min} in 10 minutes - Below optimal for active labor.",
                        RecommendedActions = new List<string>
                        {
                            "Assess labor progress",
                            "Consider augmentation with oxytocin if appropriate",
                            "Ensure adequate hydration",
                            "Encourage mobilization and position changes",
                            "Review for cephalopelvic disproportion"
                        },
                        MeasurementType = "Contraction Frequency",
                        CurrentValue = $"{latestContraction.FrequencyPer10Min} per 10 min",
                        ExpectedRange = "3-5 per 10 min in active labor"
                    });
                }
            }

            // Prolonged contractions
            if (latestContraction.DurationSeconds > AlertThresholds.CONTRACTION_DURATION_MAX)
            {
                alerts.Add(new ClinicalAlert
                {
                    Severity = AlertSeverity.Warning,
                    Category = AlertCategory.Labor,
                    Title = "Prolonged Contractions",
                    Message = $"Contraction duration: {latestContraction.DurationSeconds} seconds - Longer than optimal.",
                    RecommendedActions = new List<string>
                    {
                        "Reduce oxytocin rate if running",
                        "Monitor fetal heart rate closely",
                        "Assess maternal comfort",
                        "Ensure adequate rest periods between contractions"
                    },
                    MeasurementType = "Contraction Duration",
                    CurrentValue = $"{latestContraction.DurationSeconds} seconds",
                    ExpectedRange = $"{AlertThresholds.CONTRACTION_DURATION_MIN}-{AlertThresholds.CONTRACTION_DURATION_MAX} seconds"
                });
            }

            return alerts;
        }

        /// <summary>
        /// Analyzes hydration status
        /// </summary>
        private List<ClinicalAlert> AnalyzeHydration(Partograph patient)
        {
            var alerts = new List<ClinicalAlert>();

            if (!patient.OralFluids.Any())
                return alerts;

            var recentFluids = patient.OralFluids.Where(f => f.Time > DateTime.Now.AddHours(-4)).ToList();

            if (recentFluids.Count == 0 && patient.Status == LaborStatus.Active)
            {
                alerts.Add(new ClinicalAlert
                {
                    Severity = AlertSeverity.Warning,
                    Category = AlertCategory.Hydration,
                    Title = "No Oral Fluids Recorded Recently",
                    Message = "No oral fluid intake recorded in the last 4 hours.",
                    RecommendedActions = new List<string>
                    {
                        "Encourage oral fluids if appropriate",
                        "Consider IV hydration if prolonged labor",
                        "Assess for nausea/vomiting",
                        "Monitor for dehydration signs",
                        "Check urine output and ketones"
                    },
                    MeasurementType = "Oral Fluid Intake",
                    CurrentValue = "None in 4 hours",
                    ExpectedRange = "Regular intake during labor"
                });
            }

            return alerts;
        }

        /// <summary>
        /// Updates the active alerts list and triggers events
        /// </summary>
        private void UpdateActiveAlerts(List<ClinicalAlert> newAlerts)
        {
            // Remove alerts that are no longer active
            var alertsToRemove = _activeAlerts.Where(a =>
                !newAlerts.Any(na => na.Title == a.Title && na.MeasurementType == a.MeasurementType))
                .ToList();

            foreach (var alert in alertsToRemove)
            {
                _activeAlerts.Remove(alert);
                AlertCleared?.Invoke(this, alert.Id);
            }

            // Add new alerts
            foreach (var newAlert in newAlerts)
            {
                var existing = _activeAlerts.FirstOrDefault(a =>
                    a.Title == newAlert.Title && a.MeasurementType == newAlert.MeasurementType);

                if (existing == null)
                {
                    _activeAlerts.Add(newAlert);
                    AlertTriggered?.Invoke(this, newAlert);
                }
            }
        }

        /// <summary>
        /// Gets all active alerts
        /// </summary>
        public List<ClinicalAlert> GetActiveAlerts() => _activeAlerts.ToList();

        /// <summary>
        /// Acknowledges an alert
        /// </summary>
        public void AcknowledgeAlert(Guid alertId, string acknowledgedBy)
        {
            var alert = _activeAlerts.FirstOrDefault(a => a.Id == alertId);
            if (alert != null)
            {
                alert.IsAcknowledged = true;
                alert.AcknowledgedAt = DateTime.Now;
                alert.AcknowledgedBy = acknowledgedBy;
            }
        }

        /// <summary>
        /// Clears all acknowledged alerts
        /// </summary>
        public void ClearAcknowledgedAlerts()
        {
            _activeAlerts.RemoveAll(a => a.IsAcknowledged);
        }
    }
}
