using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Medical Notes - WHO Labour Care Guide: Documentation across all 7 sections
    // WHO 2020: Maintain clear, contemporaneous clinical notes
    // Link notes to specific measurables and clinical decisions
    public class MedicalNote : BasePartographMeasurement
    {
        // Note Type (structured categories)
        public string NoteType { get; set; } = "General"; // General, Alert, Emergency, ClinicalDecision, Handover, Escalation

        // Content
        public string Content { get; set; } = string.Empty;

        // Author Details
        public string CreatedBy { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Midwife, Doctor, Consultant, Anesthetist

        // Priority/Urgency
        public bool IsImportant { get; set; }
        public string UrgencyLevel { get; set; } = string.Empty; // Routine, Review, Urgent, Emergency

        // Clinical Category (WHO Section mapping)
        public string ClinicalCategory { get; set; } = string.Empty; // FetalWellbeing, MaternalCondition, LaborProgress, Medication, SupportiveCare, SharedDecision

        // WHO Section Reference
        public string WHOSection { get; set; } = string.Empty; // Section1-7

        // Linking to Specific Measurables (trace note back to measurement)
        public string LinkedMeasurableType { get; set; } = string.Empty; // FHR, BP, CervixDilatation, etc.
        public Guid? LinkedMeasurableID { get; set; }
        public DateTime? LinkedMeasurableTime { get; set; }

        // Review and Follow-up
        public bool RequiresReview { get; set; }
        public bool RequiresFollowUp { get; set; }
        public DateTime? ReviewedTime { get; set; }
        public string ReviewedBy { get; set; } = string.Empty;
        public string ReviewOutcome { get; set; } = string.Empty;

        // Escalation Tracking
        public bool Escalated { get; set; }
        public string EscalatedTo { get; set; } = string.Empty; // SeniorMidwife, Doctor, Consultant, Anesthetist
        public DateTime? EscalationTime { get; set; }
        public string EscalationReason { get; set; } = string.Empty;

        // Handover/Communication
        public bool IncludeInHandover { get; set; }
        public bool CommunicatedToPatient { get; set; }
        public bool CommunicatedToCompanion { get; set; }

        // Attachment/Reference
        public string AttachmentPath { get; set; } = string.Empty;
        public string ReferenceDocument { get; set; } = string.Empty;

        // Clinical Alert Status
        public string ClinicalAlert { get; set; } = string.Empty;

        //[JsonIgnore]
        //public Color NoteColor => NoteType switch
        //{
        //    "Alert" => Colors.Orange,
        //    "Emergency" => Colors.Red,
        //    "Escalation" => Colors.DarkOrange,
        //    "ClinicalDecision" => Colors.Blue,
        //    _ => Colors.Gray
        //};
    }
}
