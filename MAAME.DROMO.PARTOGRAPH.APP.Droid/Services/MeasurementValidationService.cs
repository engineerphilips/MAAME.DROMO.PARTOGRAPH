using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Linq;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    public class MeasurementValidationService
    {
        /// <summary>
        /// Validates Fetal Heart Rate measurement
        /// </summary>
        public ValidationResult ValidateFHR(int? rate, string deceleration, Partograph patient)
        {
            var result = new ValidationResult();

            if (!rate.HasValue || rate.Value <= 0)
            {
                result.AddError("FHR", "Fetal heart rate is required and must be greater than 0", "Enter a valid FHR between 110-160 bpm");
                return result;
            }

            // Physiologically impossible values
            if (rate.Value < 60 || rate.Value > 240)
            {
                result.AddError("FHR", $"FHR of {rate.Value} bpm is physiologically impossible", "Please recheck measurement. Normal range: 110-160 bpm");
                return result;
            }

            // Critical values
            if (rate.Value < AlertThresholds.FHR_CRITICAL_MIN)
            {
                result.AddWarning("FHR", $"Severe fetal bradycardia ({rate.Value} bpm)", "Immediate action required. Consider position change, oxygen, and notify obstetrician.");
            }
            else if (rate.Value > AlertThresholds.FHR_CRITICAL_MAX)
            {
                result.AddWarning("FHR", $"Severe fetal tachycardia ({rate.Value} bpm)", "Assess for maternal fever, infection, or fetal distress. Notify obstetrician.");
            }
            // Warning values
            else if (rate.Value < AlertThresholds.FHR_NORMAL_MIN)
            {
                result.AddWarning("FHR", $"Fetal bradycardia ({rate.Value} bpm)", "Monitor closely and recheck in 15 minutes.");
            }
            else if (rate.Value > AlertThresholds.FHR_NORMAL_MAX)
            {
                result.AddWarning("FHR", $"Fetal tachycardia ({rate.Value} bpm)", "Monitor closely and assess maternal vital signs.");
            }

            // Deceleration warnings
            if (!string.IsNullOrEmpty(deceleration) && deceleration != "None")
            {
                if (deceleration.ToLower().Contains("late"))
                {
                    result.AddWarning("Deceleration", "Late decelerations indicate potential uteroplacental insufficiency", "Consider position change, oxygen, IV fluids, and notify obstetrician immediately.");
                }
                else if (deceleration.ToLower().Contains("variable"))
                {
                    result.AddWarning("Deceleration", "Variable decelerations may indicate cord compression", "Try position change and monitor pattern. Notify obstetrician if persistent.");
                }
                else if (deceleration.ToLower().Contains("prolonged"))
                {
                    result.AddWarning("Deceleration", "Prolonged deceleration (>2 minutes) detected", "Check for cord prolapse if membranes ruptured. Immediate obstetric review required.");
                }
            }

            return result;
        }

        /// <summary>
        /// Validates Blood Pressure and Pulse measurement
        /// </summary>
        public ValidationResult ValidateBP(int? systolic, int? diastolic, int? pulse)
        {
            var result = new ValidationResult();

            // Required fields
            if (!systolic.HasValue || systolic.Value <= 0)
            {
                result.AddError("Systolic BP", "Systolic blood pressure is required", "Enter a value between 90-140 mmHg");
            }

            if (!diastolic.HasValue || diastolic.Value <= 0)
            {
                result.AddError("Diastolic BP", "Diastolic blood pressure is required", "Enter a value between 60-90 mmHg");
            }

            if (!pulse.HasValue || pulse.Value <= 0)
            {
                result.AddError("Pulse", "Pulse is required", "Enter a value between 60-100 bpm");
            }

            if (!result.IsValid)
                return result;

            // Physiological impossibilities
            if (systolic.Value > 250 || systolic.Value < 50)
            {
                result.AddError("Systolic BP", $"Systolic BP of {systolic.Value} mmHg is physiologically unlikely", "Please recheck measurement");
            }

            if (diastolic.Value > 150 || diastolic.Value < 30)
            {
                result.AddError("Diastolic BP", $"Diastolic BP of {diastolic.Value} mmHg is physiologically unlikely", "Please recheck measurement");
            }

            // Diastolic must be lower than systolic
            if (diastolic.Value >= systolic.Value)
            {
                result.AddError("Blood Pressure", "Diastolic pressure cannot be equal to or higher than systolic pressure", "Please recheck both measurements");
            }

            if (pulse.Value > 200 || pulse.Value < 30)
            {
                result.AddError("Pulse", $"Pulse of {pulse.Value} bpm is physiologically unlikely", "Please recheck measurement");
            }

            if (!result.IsValid)
                return result;

            // Clinical warnings - Hypertension
            if (systolic.Value >= AlertThresholds.BP_SYSTOLIC_CRITICAL || diastolic.Value >= AlertThresholds.BP_DIASTOLIC_CRITICAL)
            {
                result.AddWarning("Blood Pressure", $"Severe hypertension ({systolic}/{diastolic} mmHg)", "URGENT: Assess for pre-eclampsia. Check for symptoms: headache, visual changes, epigastric pain. Test urine for protein. Notify obstetrician immediately.");
            }
            else if (systolic.Value >= AlertThresholds.BP_SYSTOLIC_WARNING || diastolic.Value >= AlertThresholds.BP_DIASTOLIC_WARNING)
            {
                result.AddWarning("Blood Pressure", $"Elevated blood pressure ({systolic}/{diastolic} mmHg)", "Recheck in 15-30 minutes. Assess for pre-eclampsia symptoms. Consider urine protein testing.");
            }

            // Hypotension
            if (systolic.Value < AlertThresholds.BP_SYSTOLIC_LOW)
            {
                result.AddWarning("Blood Pressure", $"Hypotension ({systolic}/{diastolic} mmHg)", "Assess for bleeding, dehydration, or epidural effect. Consider IV fluid bolus.");
            }

            // Pulse abnormalities
            if (pulse.Value > AlertThresholds.PULSE_WARNING_MAX)
            {
                result.AddWarning("Pulse", $"Tachycardia ({pulse.Value} bpm)", "Assess for dehydration, pain, anxiety, or infection. Monitor closely.");
            }
            else if (pulse.Value < AlertThresholds.PULSE_NORMAL_MIN)
            {
                result.AddWarning("Pulse", $"Bradycardia ({pulse.Value} bpm)", "Assess patient. Consider athletic baseline. Monitor if symptoms present.");
            }

            return result;
        }

        /// <summary>
        /// Validates Temperature measurement
        /// </summary>
        public ValidationResult ValidateTemperature(double? temperature)
        {
            var result = new ValidationResult();

            if (!temperature.HasValue || temperature.Value <= 0)
            {
                result.AddError("Temperature", "Temperature is required and must be greater than 0", "Enter temperature in Celsius (normal: 36.0-37.5°C)");
                return result;
            }

            // Physiologically impossible
            if (temperature.Value < 30 || temperature.Value > 45)
            {
                result.AddError("Temperature", $"Temperature of {temperature.Value:F1}°C is incompatible with life", "Please recheck measurement");
                return result;
            }

            // Critical fever
            if (temperature.Value >= AlertThresholds.TEMP_CRITICAL_MAX)
            {
                result.AddWarning("Temperature", $"High fever ({temperature.Value:F1}°C)", "URGENT: Assess for infection/chorioamnionitis. Consider blood cultures and antibiotics. Notify obstetrician.");
            }
            // Mild fever
            else if (temperature.Value >= AlertThresholds.TEMP_WARNING_MAX)
            {
                result.AddWarning("Temperature", $"Elevated temperature ({temperature.Value:F1}°C)", "Recheck in 30 minutes. Assess for infection. Consider paracetamol if appropriate.");
            }
            // Hypothermia
            else if (temperature.Value < AlertThresholds.TEMP_NORMAL_MIN)
            {
                result.AddWarning("Temperature", $"Low temperature ({temperature.Value:F1}°C)", "Ensure adequate warmth. Assess for shock. Recheck measurement.");
            }

            return result;
        }

        /// <summary>
        /// Validates Cervical Dilatation measurement
        /// </summary>
        public ValidationResult ValidateCervicalDilatation(int? dilatation, Partograph patient)
        {
            var result = new ValidationResult();

            if (!dilatation.HasValue)
            {
                result.AddError("Cervical Dilatation", "Cervical dilatation is required", "Enter value between 0-10 cm");
                return result;
            }

            if (dilatation.Value < 0 || dilatation.Value > 10)
            {
                result.AddError("Cervical Dilatation", $"Cervical dilatation must be between 0-10 cm (entered: {dilatation.Value})", "The cervix cannot dilate beyond 10 cm");
                return result;
            }

            // Check for regression (dilatation decreasing)
            if (patient.Dilatations.Any())
            {
                var previousDilatation = patient.Dilatations.OrderByDescending(d => d.Time).FirstOrDefault();
                if (previousDilatation != null && dilatation.Value < previousDilatation.DilatationCm)
                {
                    result.AddError("Cervical Dilatation", $"Cervical dilatation cannot decrease (Previous: {previousDilatation.DilatationCm}cm, Current: {dilatation.Value}cm)", "This is anatomically impossible. Please verify measurement or enter the correct previous reading.");
                }
                else if (previousDilatation != null)
                {
                    // Check rate of change
                    var timeDiff = DateTime.Now - previousDilatation.Time;
                    var dilatationDiff = dilatation.Value - previousDilatation.DilatationCm;

                    if (timeDiff.TotalHours > 0)
                    {
                        var ratePerHour = dilatationDiff / timeDiff.TotalHours;

                        // Extremely rapid dilatation (>3cm per hour is unusual)
                        if (ratePerHour > 3)
                        {
                            result.AddWarning("Cervical Dilatation", $"Very rapid dilatation ({ratePerHour:F1} cm/hour)", "Please verify measurement. If correct, prepare for imminent delivery.");
                        }
                        // No progress in >4 hours in active labor
                        else if (ratePerHour == 0 && timeDiff.TotalHours >= 4 && previousDilatation.DilatationCm >= 4)
                        {
                            result.AddWarning("Cervical Dilatation", $"No cervical change in {timeDiff.TotalHours:F1} hours", "Labor may not be progressing. Consider augmentation or senior review.");
                        }
                    }
                }
            }

            // WHO 2020: Info about labor stages
            if (dilatation.Value >= 10)
            {
                result.AddInfo("Labor Stage", "Fully dilated - Second stage of labor", "Monitor for urge to push. Prepare for delivery. WHO 2020: Support woman in position of choice.");
            }
            else if (dilatation.Value >= 5)
            {
                result.AddInfo("Labor Stage", "Active labor (WHO 2020)", "Partograph should be started at 5cm with regular contractions. Expect 1cm/hour progress. Monitor FHR and contractions every 30 min, VE every 4 hours.");
            }
            else if (dilatation.Value >= 4 && dilatation.Value < 5)
            {
                result.AddInfo("Labor Stage", "Approaching active labor", "WHO 2020: Active labor begins at 5cm with regular contractions. Continue to monitor and support woman.");
            }

            return result;
        }

        /// <summary>
        /// Validates Contraction measurement
        /// </summary>
        public ValidationResult ValidateContractions(int? frequency, int? duration, Partograph patient)
        {
            var result = new ValidationResult();

            if (!frequency.HasValue || frequency.Value < 0)
            {
                result.AddError("Contraction Frequency", "Contraction frequency is required and cannot be negative", "Enter number of contractions per 10 minutes (typically 3-5)");
            }

            if (!duration.HasValue || duration.Value <= 0)
            {
                result.AddError("Contraction Duration", "Contraction duration is required", "Enter duration in seconds (typically 40-60 seconds)");
            }

            if (!result.IsValid)
                return result;

            // Physiologically unlikely
            if (frequency.Value > 10)
            {
                result.AddWarning("Contraction Frequency", $"Very high frequency ({frequency.Value} per 10 min)", "Please verify count. This frequency is concerning for hyperstimulation.");
            }

            if (duration.Value > 120)
            {
                result.AddWarning("Contraction Duration", $"Very long duration ({duration.Value} seconds)", "Please verify measurement. Prolonged contractions can compromise fetal oxygenation.");
            }

            // Hyperstimulation
            if (frequency.Value > AlertThresholds.CONTRACTION_HYPERSTIMULATION)
            {
                result.AddWarning("Contractions", $"Uterine hyperstimulation ({frequency.Value} per 10 min)", "URGENT: Stop oxytocin if running. Monitor FHR continuously. Notify obstetrician.");
            }

            // Prolonged contractions
            if (duration.Value > AlertThresholds.CONTRACTION_DURATION_MAX)
            {
                result.AddWarning("Contraction Duration", $"Prolonged contractions ({duration.Value} seconds)", "Reduce oxytocin if running. Monitor fetal heart rate closely.");
            }

            // Inadequate contractions in active labor
            if (patient.Dilatations.Any())
            {
                var latestDilatation = patient.Dilatations.OrderByDescending(d => d.Time).FirstOrDefault();
                if (latestDilatation != null && latestDilatation.DilatationCm >= 4 && latestDilatation.DilatationCm < 10 && frequency.Value < 3)
                {
                    result.AddWarning("Contraction Frequency", $"Low frequency in active labor ({frequency.Value} per 10 min)", "Consider augmentation if labor not progressing. Optimal frequency is 3-5 per 10 minutes.");
                }
            }

            return result;
        }

        /// <summary>
        /// Validates Urine output and content
        /// </summary>
        public ValidationResult ValidateUrine(string protein, string acetone, int? volumeMl)
        {
            var result = new ValidationResult();

            // Significant proteinuria
            if (AlertThresholds.URINE_PROTEIN_WARNING.Contains(protein))
            {
                result.AddWarning("Urine Protein", $"Significant proteinuria ({protein})", "Check blood pressure. Assess for pre-eclampsia symptoms. Consider blood tests. Notify obstetrician.");
            }

            // Ketonuria
            if (AlertThresholds.URINE_ACETONE_WARNING.Contains(acetone))
            {
                result.AddWarning("Urine Ketones", $"Ketonuria detected ({acetone})", "Assess hydration and nutritional status. Consider IV dextrose if prolonged labor. Encourage oral fluids if appropriate.");
            }

            // Volume concerns
            if (volumeMl.HasValue)
            {
                if (volumeMl.Value < 30 && volumeMl.Value > 0)
                {
                    result.AddWarning("Urine Volume", $"Low urine output ({volumeMl.Value} ml)", "Assess for dehydration. Ensure adequate fluid intake. Monitor for oliguria.");
                }
                else if (volumeMl.Value == 0)
                {
                    result.AddInfo("Urine Volume", "No urine output recorded", "If patient hasn't voided in >4 hours, assess bladder and consider catheterization.");
                }
            }

            return result;
        }

        /// <summary>
        /// Validates Head Descent measurement
        /// </summary>
        public ValidationResult ValidateHeadDescent(int? station, Partograph patient)
        {
            var result = new ValidationResult();

            if (!station.HasValue)
            {
                result.AddError("Head Descent", "Station is required", "Enter station from -5 to +5 (0 = at ischial spines)");
                return result;
            }

            if (station.Value < -5 || station.Value > 5)
            {
                result.AddError("Head Descent", $"Station must be between -5 and +5 (entered: {station.Value})", "Station 0 is at ischial spines");
                return result;
            }

            // Check for regression (head moving up)
            if (patient.HeadDescents.Any())
            {
                var previousStation = patient.HeadDescents.OrderByDescending(h => h.Time).FirstOrDefault();
                if (previousStation != null && station.Value < previousStation.Station)
                {
                    result.AddWarning("Head Descent", $"Head appears to be moving up (Previous: {previousStation.Station}, Current: {station.Value})", "This is unusual and may indicate poor engagement. Verify measurement and assess for CPD.");
                }
            }

            // Clinical info
            if (station.Value >= 3)
            {
                result.AddInfo("Head Descent", "Head is low - delivery imminent", "Monitor for urge to push. Prepare for delivery.");
            }
            else if (station.Value < -3)
            {
                result.AddInfo("Head Descent", "Head is high - not yet engaged", "Monitor progress. Assess pelvic adequacy if primigravida.");
            }

            return result;
        }

        /// <summary>
        /// Validates time-based measurement entry
        /// </summary>
        public ValidationResult ValidateMeasurementTime(DateTime measurementTime, Partograph patient)
        {
            var result = new ValidationResult();

            // Cannot be in the future
            if (measurementTime > DateTime.Now)
            {
                result.AddError("Time", "Measurement time cannot be in the future", "Please enter the correct time");
            }

            // Cannot be before labor started
            if (patient.LaborStartTime.HasValue && measurementTime < patient.LaborStartTime.Value)
            {
                result.AddWarning("Time", $"Measurement time is before labor start ({patient.LaborStartTime.Value:dd/MM/yyyy HH:mm})", "Verify this is correct");
            }

            // More than 24 hours old
            var hoursSinceMeasurement = (DateTime.Now - measurementTime).TotalHours;
            if (hoursSinceMeasurement > 24)
            {
                result.AddWarning("Time", $"This measurement is from {hoursSinceMeasurement:F0} hours ago", "Confirm this is a retrospective entry");
            }

            return result;
        }
    }
}
