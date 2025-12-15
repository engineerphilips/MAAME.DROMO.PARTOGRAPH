using System;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Assessment (every 4 hours or as needed)
    public class Assessment : BasePartographMeasurement
    {
        // Labor Progress Assessment
        public string LaborProgress { get; set; } = string.Empty; // Normal, Slow, Rapid, Arrested
        public string LaborPhase { get; set; } = string.Empty; // Latent, ActivePhase1, ActivePhase2, SecondStage, ThirdStage
        public string PartographLine { get; set; } = string.Empty; // BelowActionLine, OnActionLine, AboveActionLine

        // Fetal Wellbeing Assessment
        public string FetalWellbeing { get; set; } = string.Empty; // Reassuring, NonReassuring, Abnormal

        // Maternal Condition Assessment
        public string MaternalCondition { get; set; } = string.Empty; // Stable, Fatigued, Exhausted, Compromised

        // Risk Factors and Complications
        public string RiskFactors { get; set; } = string.Empty;
        public string Complications { get; set; } = string.Empty;

        // Expected Delivery Mode
        public string ExpectedDelivery { get; set; } = string.Empty; // NormalVaginal, Instrumental, Cesarean

        // Intervention Requirements
        public bool RequiresIntervention { get; set; }
        public string InterventionRequired { get; set; } = string.Empty;
        public string UrgencyLevel { get; set; } = string.Empty; // Routine, Review, Urgent, Emergency

        // Senior Review
        public bool SeniorReviewRequired { get; set; }
        public bool ConsultantInformed { get; set; }

        // Assessment Type and Follow-up
        public string AssessmentType { get; set; } = string.Empty; // Routine, Triggered, Emergency
        public DateTime? NextAssessment { get; set; }

        // Clinical Decision
        public string ClinicalDecision { get; set; } = string.Empty; // ContinueLabor, AugmentLabor, OperativeDelivery, CSection
    }
}
