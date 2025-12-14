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
        /// Analyzes labor progression against WHO Labour Care Guide 2020 alert/action lines
        /// Based on WHO 2020 guidelines: Active labor starts at 5cm, alert line crosses at 1cm/hour
        /// </summary>
        private List<ClinicalAlert> AnalyzeLaborProgression(Partograph patient)
        {
            var alerts = new List<ClinicalAlert>();

            if (!patient.Dilatations.Any() || !patient.LaborStartTime.HasValue)
                return alerts;

            var dilatations = patient.Dilatations.OrderBy(d => d.Time).ToList();
            var latestDilatation = dilatations.Last();
            var actualDilatation = latestDilatation.DilatationCm;

            // WHO 2020: Active labor starts at 5cm with regular contractions
            var activeLaborEntry = dilatations.FirstOrDefault(d => d.DilatationCm >= AlertThresholds.ACTIVE_LABOR_START_CM);
            if (activeLaborEntry == null)
            {
                // Patient not yet in active labor
                if (actualDilatation >= 4 && actualDilatation < 5)
                {
                    alerts.Add(new ClinicalAlert
                    {
                        Severity = AlertSeverity.Info,
                        Category = AlertCategory.Labor,
                        Title = "Approaching Active Labor",
                        Message = $"Cervix is {actualDilatation}cm dilated. Active labour begins at 5cm with regular contractions.",
                        RecommendedActions = new List<string>
                        {
                            "Continue to monitor progress",
                            "Ensure woman is comfortable and supported",
                            "Assess contraction pattern",
                            "Prepare to start partograph when 5cm reached"
                        },
                        MeasurementType = "Cervical Dilatation",
                        CurrentValue = $"{actualDilatation} cm",
                        ExpectedRange = "Active labour starts at 5cm"
                    });
                }
                return alerts;
            }

            var hoursSinceActiveLaborStart = (latestDilatation.Time - activeLaborEntry.Time).TotalHours;

            // WHO 2020: Alert line - 1cm per hour from 5cm (reaches 10cm in 5 hours)
            var alertLineExpected = AlertThresholds.ALERT_LINE_START_CM +
                                   (hoursSinceActiveLaborStart * AlertThresholds.DILATATION_RATE_EXPECTED);
            if (alertLineExpected > 10) alertLineExpected = 10;

            // WHO 2020: Action line - 4 hours to the right of alert line
            var actionLineExpected = AlertThresholds.ALERT_LINE_START_CM +
                                    (Math.Max(0, hoursSinceActiveLaborStart - AlertThresholds.ACTION_LINE_OFFSET_HOURS) *
                                     AlertThresholds.DILATATION_RATE_EXPECTED);
            if (actionLineExpected > 10) actionLineExpected = 10;

            // Check if labor has crossed the action line (Critical)
            if (actualDilatation < actionLineExpected && hoursSinceActiveLaborStart > AlertThresholds.ACTION_LINE_OFFSET_HOURS)
            {
                alerts.Add(new ClinicalAlert
                {
                    Severity = AlertSeverity.Critical,
                    Category = AlertCategory.Labor,
                    Title = "Labour Progress: Action Line Crossed",
                    Message = $"Cervical dilatation has crossed the action line. Current: {actualDilatation}cm, Expected: ≥{actionLineExpected:F1}cm after {hoursSinceActiveLaborStart:F1} hours in active labor.",
                    RecommendedActions = new List<string>
                    {
                        "URGENT: Notify senior obstetrician immediately",
                        "Assess for cephalopelvic disproportion (CPD)",
                        "Review for malpresentation or malposition",
                        "Consider augmentation with oxytocin if membranes ruptured and no contraindications",
                        "Artificial rupture of membranes if not done and no contraindications",
                        "Prepare for potential operative delivery",
                        "Ensure woman is well-hydrated and has adequate pain relief",
                        "Monitor fetal and maternal wellbeing closely"
                    },
                    MeasurementType = "Cervical Dilatation",
                    CurrentValue = $"{actualDilatation} cm",
                    ExpectedRange = $"≥{actionLineExpected:F1} cm (Action line)"
                });
            }
            // Check if labor has crossed the alert line (Warning)
            else if (actualDilatation < alertLineExpected && hoursSinceActiveLaborStart > 1)
            {
                alerts.Add(new ClinicalAlert
                {
                    Severity = AlertSeverity.Warning,
                    Category = AlertCategory.Labor,
                    Title = "Labour Progress: Alert Line Crossed",
                    Message = $"Labour is progressing slower than expected. Current: {actualDilatation}cm, Expected: ≥{alertLineExpected:F1}cm after {hoursSinceActiveLaborStart:F1} hours in active labor.",
                    RecommendedActions = new List<string>
                    {
                        "Assess contraction pattern (should be 3-5 in 10 minutes, each lasting 40 seconds)",
                        "Assess for obstructed labour signs: severe pain, maternal distress, bladder distension",
                        "Ensure woman is well-hydrated (IV fluids if needed)",
                        "Encourage upright positions and mobility if appropriate",
                        "Consider artificial rupture of membranes if appropriate and no contraindications",
                        "Review pain relief options",
                        "Monitor closely - if crosses action line, escalate immediately"
                    },
                    MeasurementType = "Cervical Dilatation",
                    CurrentValue = $"{actualDilatation} cm",
                    ExpectedRange = $"≥{alertLineExpected:F1} cm (Alert line)"
                });
            }

            // WHO 2020: Prolonged active labor (>12 hours from 5cm)
            if (hoursSinceActiveLaborStart > 12 && actualDilatation < 10)
            {
                alerts.Add(new ClinicalAlert
                {
                    Severity = AlertSeverity.Critical,
                    Category = AlertCategory.Labor,
                    Title = "Prolonged Active Labor",
                    Message = $"Active labour has exceeded 12 hours ({hoursSinceActiveLaborStart:F1} hours since 5cm). Current dilatation: {actualDilatation}cm.",
                    RecommendedActions = new List<string>
                    {
                        "URGENT: Senior obstetrician review required",
                        "Assess for obstructed labor",
                        "Assess maternal exhaustion and hydration status",
                        "Review for fetal distress",
                        "Consider operative delivery (cesarean section or instrumental delivery)",
                        "Ensure continuous maternal and fetal monitoring",
                        "Provide emotional support and keep woman informed"
                    },
                    MeasurementType = "Labour Duration",
                    CurrentValue = $"{hoursSinceActiveLaborStart:F1} hours in active labor",
                    ExpectedRange = "<12 hours from 5cm to full dilatation"
                });
            }

            // Good progress - informational
            if (actualDilatation >= alertLineExpected && actualDilatation < 10 && hoursSinceActiveLaborStart > 0)
            {
                alerts.Add(new ClinicalAlert
                {
                    Severity = AlertSeverity.Info,
                    Category = AlertCategory.Labor,
                    Title = "Normal Labour Progress",
                    Message = $"Labour is progressing normally. Current: {actualDilatation}cm after {hoursSinceActiveLaborStart:F1} hours in active labor.",
                    RecommendedActions = new List<string>
                    {
                        "Continue supportive care",
                        "Monitor according to WHO 2020 schedule (FHR and contractions every 30 min, VE every 4 hours)",
                        "Encourage oral fluids and light food if desired",
                        "Provide continuous emotional support",
                        "Encourage mobility and upright positions"
                    },
                    MeasurementType = "Cervical Dilatation",
                    CurrentValue = $"{actualDilatation} cm",
                    ExpectedRange = "On or ahead of alert line"
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

            // Check for deceleration patterns - WHO 2020 Labour Care Guide
            if (!string.IsNullOrEmpty(latestFHR.Deceleration) &&
                latestFHR.Deceleration != "None" &&
                latestFHR.Deceleration != "No")
            {
                var decelerationType = latestFHR.Deceleration.ToLower();
                AlertSeverity severity;
                string message;
                List<string> actions;

                // Classify according to WHO 2020 guidelines
                if (decelerationType.Contains("late"))
                {
                    // Late decelerations - CRITICAL: uteroplacental insufficiency
                    severity = AlertSeverity.Critical;
                    message = "Late decelerations detected. These indicate uteroplacental insufficiency and fetal hypoxia.";
                    actions = new List<string>
                    {
                        "URGENT: Immediate action required",
                        "Change maternal position (left lateral)",
                        "Discontinue oxytocin if running",
                        "Administer oxygen to mother (8-10 L/min via face mask)",
                        "IV fluid bolus if hypotensive",
                        "Check maternal blood pressure",
                        "Notify obstetrician immediately",
                        "Prepare for possible expedited delivery"
                    };
                }
                else if (decelerationType.Contains("prolonged"))
                {
                    // Prolonged decelerations (>2 min) - CRITICAL: immediate evaluation
                    severity = AlertSeverity.Critical;
                    message = "Prolonged deceleration detected (>2 minutes). Requires immediate evaluation for cord prolapse or other acute events.";
                    actions = new List<string>
                    {
                        "URGENT: Immediate obstetric review required",
                        "Check for cord prolapse if membranes ruptured",
                        "Change maternal position",
                        "Discontinue oxytocin if running",
                        "Administer oxygen to mother",
                        "Vaginal examination to check for cord prolapse",
                        "Prepare for emergency delivery if deceleration persists"
                    };
                }
                else if (decelerationType.Contains("variable"))
                {
                    // Variable decelerations - WARNING: cord compression
                    severity = AlertSeverity.Warning;
                    message = "Variable decelerations detected. These may indicate umbilical cord compression.";
                    actions = new List<string>
                    {
                        "Change maternal position (left or right lateral)",
                        "Monitor pattern - assess if persistent or worsening",
                        "Discontinue oxytocin if running and decelerations severe",
                        "Administer oxygen if decelerations deep or prolonged",
                        "Notify obstetrician if pattern worsens or persists",
                        "Consider amnioinfusion if available and appropriate"
                    };
                }
                else if (decelerationType.Contains("early"))
                {
                    // Early decelerations - INFO: benign head compression
                    severity = AlertSeverity.Info;
                    message = "Early decelerations detected. These are typically benign and caused by fetal head compression.";
                    actions = new List<string>
                    {
                        "Continue routine monitoring",
                        "Early decelerations are usually benign",
                        "Ensure decelerations are truly 'early' (mirror contractions)",
                        "Monitor for any change in pattern"
                    };
                }
                else
                {
                    // Unspecified deceleration type
                    severity = AlertSeverity.Warning;
                    message = $"{latestFHR.Deceleration} decelerations observed. Pattern requires evaluation.";
                    actions = new List<string>
                    {
                        "Assess deceleration pattern carefully",
                        "Determine deceleration type (Early/Late/Variable/Prolonged)",
                        "Change maternal position",
                        "Monitor closely and notify obstetrician if uncertain"
                    };
                }

                alerts.Add(new ClinicalAlert
                {
                    Severity = severity,
                    Category = AlertCategory.Fetal,
                    Title = $"FHR Deceleration: {latestFHR.Deceleration}",
                    Message = message,
                    RecommendedActions = actions,
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
        /// Analyzes contraction patterns for abnormalities (WHO Labour Care Guide 2020)
        /// </summary>
        private List<ClinicalAlert> AnalyzeContractions(Partograph patient)
        {
            var alerts = new List<ClinicalAlert>();

            if (!patient.Contractions.Any())
                return alerts;

            var latestContraction = patient.Contractions.OrderByDescending(c => c.Time).First();

            // WHO 2020: Tachysystole (>5 contractions per 10 minutes)
            if (latestContraction.FrequencyPer10Min > AlertThresholds.CONTRACTION_NORMAL_MAX)
            {
                alerts.Add(new ClinicalAlert
                {
                    Severity = AlertSeverity.Critical,
                    Category = AlertCategory.Labor,
                    Title = "Uterine Tachysystole Detected",
                    Message = $"Contractions: {latestContraction.FrequencyPer10Min} in 10 minutes - More than 5 contractions per 10 minutes (tachysystole).",
                    RecommendedActions = new List<string>
                    {
                        "URGENT: Stop oxytocin infusion immediately if running",
                        "Monitor fetal heart rate continuously",
                        "Change woman's position to left lateral",
                        "Administer oxygen to woman",
                        "Consider tocolysis if fetal heart rate abnormalities present",
                        "Notify senior obstetrician immediately",
                        "Assess for signs of uterine rupture (severe continuous pain, maternal shock)"
                    },
                    MeasurementType = "Contraction Frequency",
                    CurrentValue = $"{latestContraction.FrequencyPer10Min} per 10 min",
                    ExpectedRange = $"3-5 per 10 min (WHO 2020)"
                });
            }

            // WHO 2020: Inadequate contractions in active labor
            if (latestContraction.FrequencyPer10Min < AlertThresholds.CONTRACTION_NORMAL_MIN && patient.Dilatations.Any())
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
                            "Assess labour progress",
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

            if (recentFluids.Count == 0 && patient.Status == LaborStatus.FirstStage && patient.Status == LaborStatus.SecondStage && patient.Status == LaborStatus.ThirdStage)
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
