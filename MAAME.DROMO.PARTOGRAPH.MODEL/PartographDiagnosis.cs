using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Partograph Diagnosis - WHO Labour Care Guide: Evidence-based clinical diagnosis
    // WHO 2020: Diagnoses should be based on objective findings and guide management
    public class PartographDiagnosis : BasePartographMeasurement
    {
        // Diagnosis Name
        public string Name { get; set; } = string.Empty;

        // Diagnosis Category (WHO Section alignment)
        public string Category { get; set; } = string.Empty; // LaborProgress, FetalWellbeing, MaternalCondition, Complication

        // Diagnosis Type (structured categories based on WHO)
        public string DiagnosisType { get; set; } = string.Empty;
        // Labor Progress: NormalLabor, ProlongedLabor, ArrestedLabor, PrecipitateLabor, FailureToProgress
        // Fetal: FetalDistress, NRFHR, Meconium, MalpresentationMalposition
        // Maternal: PreEclampsia, Hemorrhage, Infection, Exhaustion, DehydrationKetosis
        // Complications: CPD, UterineRupture, CordProlapse, ShoulderDystocia

        // ICD-10 Coding Support
        public string ICDCode { get; set; } = string.Empty;
        public string ICDDescription { get; set; } = string.Empty;

        // Severity
        public string Severity { get; set; } = string.Empty; // Mild, Moderate, Severe, Critical

        // Onset and Duration
        public DateTime? OnsetTime { get; set; }
        public int? DurationHours { get; set; }
        public string OnsetType { get; set; } = string.Empty; // Sudden, Gradual, Insidious

        // Evidence-Based Diagnosis (link to findings)
        public string ClinicalEvidence { get; set; } = string.Empty;
        public string SupportingFindings { get; set; } = string.Empty; // Links to FHR, BP, cervix findings, etc.

        // Linked Measurables (trace diagnosis back to objective data)
        public List<Guid> LinkedMeasurableIDs { get; set; } = new List<Guid>();
        public string LinkedMeasurableTypes { get; set; } = string.Empty; // FHR, BP, CervixDilatation, etc.

        // Diagnosis Status
        public string Status { get; set; } = string.Empty; // Suspected, Confirmed, Ruled Out, Resolved

        // Diagnosed By
        public string DiagnosedBy { get; set; } = string.Empty;
        public string DiagnosedByRole { get; set; } = string.Empty; // Midwife, Doctor, Consultant

        // Confidence Level
        public string ConfidenceLevel { get; set; } = string.Empty; // Low, Medium, High, Definitive

        // Management Plan (associated with diagnosis)
        public string ManagementPlan { get; set; } = string.Empty;
        public string ManagementAction { get; set; } = string.Empty; // Expectant, Intervention, Escalation, EmergencyCS

        // Escalation
        public bool RequiresEscalation { get; set; }
        public string EscalatedTo { get; set; } = string.Empty; // SeniorMidwife, Doctor, Consultant
        public DateTime? EscalationTime { get; set; }

        // Review and Resolution
        public bool RequiresReview { get; set; }
        public DateTime? ReviewTime { get; set; }
        public string ReviewOutcome { get; set; } = string.Empty;
        public DateTime? ResolvedTime { get; set; }

        // Patient Communication
        public bool DiscussedWithPatient { get; set; }
        public bool DiscussedWithCompanion { get; set; }
        public string PatientUnderstanding { get; set; } = string.Empty; // Understands, PartialUnderstanding, DoesNotUnderstand

        // WHO 2020 Alert Status
        public string ClinicalAlert { get; set; } = string.Empty;
    }
}
