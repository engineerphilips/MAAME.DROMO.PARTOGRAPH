using System;
using System.Collections.Generic;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public interface ICDSService
    {
        /// <summary>
        /// Evaluates current labor data against WHO rules and returns a list of active alerts.
        /// </summary>
        List<ClinicalAlert> EvaluateCase(LiveLaborCase laborCase, LaborCareGuideData recentData);
        
        /// <summary>
        /// Gets a specific guideline by code.
        /// </summary>
        ClinicalGuideline GetGuideline(string code);
    }

    public class CDSService : ICDSService
    {
        public List<ClinicalAlert> EvaluateCase(LiveLaborCase laborCase, LaborCareGuideData recentData)
        {
            var alerts = new List<ClinicalAlert>();

            if (laborCase == null || recentData == null) return alerts;

            // 1. Fetal Heart Rate Rules (WHO: 110-160 normal)
            if (laborCase.LatestFHR < 110)
            {
                alerts.Add(new ClinicalAlert
                {
                    PartographId = laborCase.PartographId,
                    Severity = ClinicalAlertSeverity.Critical,
                    Category = ClinicalAlertCategory.Fetal,
                    Title = "Fetal Bradycardia",
                    Message = $"Current FHR is {laborCase.LatestFHR} bpm (Limit: < 110).",
                    Recommendation = "Rule out maternal hypotension. Reposition mother (Left Lateral). Administer Oxygen if indicated.",
                    GuidelineReference = "WHO LCG 2020: Section 2"
                });
            }
            else if (laborCase.LatestFHR > 160)
            {
                alerts.Add(new ClinicalAlert
                {
                    PartographId = laborCase.PartographId,
                    Severity = ClinicalAlertSeverity.Warning,
                    Category = ClinicalAlertCategory.Fetal,
                    Title = "Fetal Tachycardia",
                    Message = $"Current FHR is {laborCase.LatestFHR} bpm (Limit: > 160).",
                    Recommendation = "Check maternal temperature (Chorioamnionitis risk). Ensure hydration.",
                    GuidelineReference = "WHO LCG 2020: Section 2"
                });
            }

            // 2. Maternal Blood Pressure Rules (Severe Pre-eclampsia check)
            if (laborCase.LatestSystolicBP >= 160 || laborCase.LatestDiastolicBP >= 110)
            {
                alerts.Add(new ClinicalAlert
                {
                    PartographId = laborCase.PartographId,
                    Severity = ClinicalAlertSeverity.Emergency,
                    Category = ClinicalAlertCategory.Maternal,
                    Title = "Severe Hypertension",
                    Message = $"BP {laborCase.LatestSystolicBP}/{laborCase.LatestDiastolicBP} is critical.",
                    Recommendation = "Immediately alert Obstetrician. Prepare Magnesium Sulfate loading dose per protocol.",
                    GuidelineReference = "WHO Pre-eclampsia Protocol"
                });
            }
            else if (laborCase.LatestSystolicBP >= 140 || laborCase.LatestDiastolicBP >= 90)
            {
                alerts.Add(new ClinicalAlert
                {
                    PartographId = laborCase.PartographId,
                    Severity = ClinicalAlertSeverity.Warning,
                    Category = ClinicalAlertCategory.Maternal,
                    Title = "Hypertension",
                    Message = $"BP {laborCase.LatestSystolicBP}/{laborCase.LatestDiastolicBP} is elevated.",
                    Recommendation = "Repeat BP in 15 minutes. Monitor for proteinurea.",
                    GuidelineReference = "WHO LCG 2020: Section 3"
                });
            }

            // 3. Maternal Temperature (Sepsis check)
            if (laborCase.LatestTemperature > 38.0m)
            {
                alerts.Add(new ClinicalAlert
                {
                    PartographId = laborCase.PartographId,
                    Severity = ClinicalAlertSeverity.Critical,
                    Category = ClinicalAlertCategory.Maternal,
                    Title = "Maternal Pyrexia",
                    Message = $"Temperature {laborCase.LatestTemperature}°C indicates possible infection.",
                    Recommendation = "Start IV Fluids. Check FHR for tachycardia. Consider antibiotics.",
                    GuidelineReference = "WHO Sepsis Protocol"
                });
            }

            // 4. Labor Progress (Protracted Active Phase)
            // Rule: If dilation < 1cm/hour in active phase (>5cm)
            if (laborCase.CurrentDilatation >= 5 && laborCase.LaborStage == "First Stage" && laborCase.LastAssessmentTime.HasValue)
            {
                // Simple heuristic for demo: If last exam was > 4 hours ago
                var hoursSinceExam = (DateTime.UtcNow - laborCase.LastAssessmentTime.Value).TotalHours;
                if (hoursSinceExam > 4)
                {
                     alerts.Add(new ClinicalAlert
                    {
                        PartographId = laborCase.PartographId,
                        Severity = ClinicalAlertSeverity.Warning,
                        Category = ClinicalAlertCategory.LaborProgress,
                        Title = "Overdue Assessment",
                        Message = $"Last vaginal exam was {hoursSinceExam:F1} hours ago.",
                        Recommendation = "Perform Vaginal Exam to assess progress.",
                        GuidelineReference = "WHO LCG 2020: Section 4"
                    });
                }
            }

            return alerts;
        }

        public ClinicalGuideline GetGuideline(string code)
        {
            // In a real app, this would query a database of guidelines
            return new ClinicalGuideline 
            { 
                Code = code, 
                Title = "WHO Intrapartum Care 2020", 
                Source = "WHO", 
                Url = "https://who.int/reproductivehealth" 
            };
        }
    }
}
