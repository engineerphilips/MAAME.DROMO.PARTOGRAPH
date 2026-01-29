using System;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Models
{
    public class AuditLogEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // WHO performed the action
        public Guid UserId { get; set; }
        public string UserName { get; set; } // De-normalized for historical accuracy
        public string UserRole { get; set; }

        // WHAT entity was affected
        public Guid? PatientId { get; set; }
        public string PatientName { get; set; } // De-normalized
        public Guid? PartographId { get; set; }
        
        // WHAT changed
        public string ActionType { get; set; } // CREATE, UPDATE, DELETE, OVERRIDE
        public string EntityName { get; set; } // e.g., "FetalHeartRate", "Medication"
        public string EntityId { get; set; }   // ID of the specific record changed
        
        // DATA payload
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        
        // WHY (Medico-Legal)
        public string Reason { get; set; }
        public string IpAddress { get; set; }
    }

    public enum AuditActionType
    {
        Create,
        Read, 
        Update,
        Delete,
        Override, // Specifically for bypassing CDS alerts
        Login,
        Logout,
        ViewSensitive
    }
}
