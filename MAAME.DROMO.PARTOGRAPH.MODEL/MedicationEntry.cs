using System;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Medication Administration - WHO Labour Care Guide Section 6: Medication
    // Document all medications administered during labour
    public class MedicationEntry : BasePartographMeasurement
    {
        // Medication Details
        public string MedicationName { get; set; } = string.Empty;
        public string Dose { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty; // PO, IV, IM, SC, Sublingual, PR, Topical, Inhalation

        // Administration (mandatory for drug audit and patient safety)
        public DateTime AdministrationTime { get; set; } = DateTime.Now;
        public string AdministeredBy { get; set; } = string.Empty; // Who gave the medication
        public string WitnessedBy { get; set; } = string.Empty; // For controlled drugs - dual signature

        // Prescription
        public string Indication { get; set; } = string.Empty; // Why medication was given
        public string PrescribedBy { get; set; } = string.Empty; // Must be authorized prescriber

        // Patient Response
        public string Response { get; set; } = string.Empty; // Effective, PartialEffect, NoEffect, AdverseReaction
        public DateTime? ResponseTime { get; set; }

        // Adverse Reactions (critical for patient safety)
        public bool AdverseReaction { get; set; }
        public string AdverseReactionType { get; set; } = string.Empty; // Allergic, SideEffect, Overdose
        public string AdverseReactionSeverity { get; set; } = string.Empty; // Mild, Moderate, Severe, LifeThreatening
        public string AdverseReactionDetails { get; set; } = string.Empty;

        // Patient Refusal
        public bool RefusedByPatient { get; set; }
        public string RefusalReason { get; set; } = string.Empty;

        // Clinical Alert
        public string ClinicalAlert { get; set; } = string.Empty; // Auto-populated for adverse reactions
    }
}
