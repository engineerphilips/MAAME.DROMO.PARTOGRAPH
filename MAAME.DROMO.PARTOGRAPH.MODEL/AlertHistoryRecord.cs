namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    /// <summary>
    /// Model for persisted alert history records
    /// </summary>
    public class AlertHistoryRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? PartographId { get; set; }
        public Guid? PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty; // "MeasurementDue" or "ClinicalAlert"
        public string MeasurementType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // Critical, Warning, Info
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? AcknowledgedAt { get; set; }
        public string AcknowledgedBy { get; set; } = string.Empty;
        public DateTime? ResolvedAt { get; set; }
        public string ResolvedBy { get; set; } = string.Empty;
        public int EscalationLevel { get; set; } = 0;
        public DateTime? EscalatedAt { get; set; }
        public int ResponseTimeMinutes { get; set; }
        public bool IsMissed { get; set; }
        public string ShiftId { get; set; } = string.Empty;
        public Guid? FacilityId { get; set; }

        // Calculated properties
        public bool IsAcknowledged => AcknowledgedAt.HasValue;
        public bool IsResolved => ResolvedAt.HasValue;
        public bool IsEscalated => EscalationLevel > 0;

        public string SeverityColor => Severity switch
        {
            "Critical" => "#EF5350",
            "Warning" => "#FF9800",
            "Info" => "#2196F3",
            _ => "#9E9E9E"
        };

        public string SeverityIcon => Severity switch
        {
            "Critical" => "🚨",
            "Warning" => "⚠️",
            "Info" => "ℹ️",
            _ => "🔔"
        };

        // UI Display properties
        public bool HasStatus => IsAcknowledged || IsMissed || IsEscalated;

        public string StatusDisplay
        {
            get
            {
                if (IsMissed) return "MISSED";
                if (IsAcknowledged) return "✓ ACK";
                if (IsEscalated) return $"L{EscalationLevel}";
                return "";
            }
        }

        public string StatusColor
        {
            get
            {
                if (IsMissed) return "#C62828";
                if (IsAcknowledged) return "#4CAF50";
                if (IsEscalated) return "#E65100";
                return "#9E9E9E";
            }
        }
    }
}
