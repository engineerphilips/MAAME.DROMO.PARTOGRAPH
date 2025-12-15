using System;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Management Plan - WHO Labour Care Guide Section 7: Shared Decision-Making
    // Record plan following assessment, each time clinical assessment of woman's and baby's wellbeing is completed
    public class Plan : BasePartographMeasurement
    {
        // Management Strategy
        public string ManagementPlan { get; set; } = string.Empty; // ExpectantManagement, AugmentLabor, OperativeDelivery, CSection

        // Expected Delivery Method
        public string ExpectedDeliveryMethod { get; set; } = string.Empty; // SVD, InstrumentalAssisted, LSCS, EmergencyCS

        // Interventions Planned
        public string InterventionsPlanned { get; set; } = string.Empty;
        public string PainReliefPlan { get; set; } = string.Empty;
        public string AugmentationPlan { get; set; } = string.Empty;

        // Monitoring Plan
        public bool ContinuousMonitoringRequired { get; set; }
        public DateTime? NextReviewTime { get; set; }

        // Transfer and Escalation
        public bool TransferRequired { get; set; }
        public string TransferDestination { get; set; } = string.Empty;
        public string TeamMembersInvolved { get; set; } = string.Empty; // Consultant, Pediatrics, Anesthesia

        // Patient Consent and Agreement (WHO LCG: continuous communication with woman and companion)
        public string ConsentRequired { get; set; } = string.Empty; // None, Verbal, Written
        public bool PatientInformed { get; set; }
        public string PatientAgreement { get; set; } = string.Empty; // Agrees, Declines, UndecidedNeedsMoreInfo
        public string PatientConcerns { get; set; } = string.Empty;
    }
}
