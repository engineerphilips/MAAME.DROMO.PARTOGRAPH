using System;
using System.Collections.Generic;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    public enum AlertSeverity
    {
        Info,       // Informational
        Warning,    // Requires attention
        Critical    // Immediate action required
    }

    public enum AlertCategory
    {
        Labor,      // Labor progression issues
        Fetal,      // Fetal wellbeing concerns
        Maternal,   // Maternal vital signs
        Hydration,  // Fluid intake concerns
        General     // General information
    }

    public class ClinicalAlert
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public AlertSeverity Severity { get; set; }
        public AlertCategory Category { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<string> RecommendedActions { get; set; } = new List<string>();
        public DateTime TriggeredAt { get; set; } = DateTime.Now;
        public bool IsAcknowledged { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string AcknowledgedBy { get; set; } = string.Empty;

        // Alert specific data
        public string MeasurementType { get; set; } = string.Empty;
        public string CurrentValue { get; set; } = string.Empty;
        public string ExpectedRange { get; set; } = string.Empty;

        // UI Properties
        public string SeverityIcon => Severity switch
        {
            AlertSeverity.Critical => "🚨",
            AlertSeverity.Warning => "⚠️",
            AlertSeverity.Info => "ℹ️",
            _ => "•"
        };

        public string SeverityColor => Severity switch
        {
            AlertSeverity.Critical => "#EF5350",  // Red
            AlertSeverity.Warning => "#FF9800",   // Orange
            AlertSeverity.Info => "#2196F3",      // Blue
            _ => "#9E9E9E"
        };

        public string CategoryIcon => Category switch
        {
            AlertCategory.Labor => "📊",
            AlertCategory.Fetal => "👶",
            AlertCategory.Maternal => "🩺",
            AlertCategory.Hydration => "💧",
            _ => "📋"
        };
    }

    public class AlertThresholds
    {
        // Fetal Heart Rate thresholds
        public const int FHR_NORMAL_MIN = 110;
        public const int FHR_NORMAL_MAX = 160;
        public const int FHR_CRITICAL_MIN = 100;
        public const int FHR_CRITICAL_MAX = 180;

        // Maternal Blood Pressure thresholds
        public const int BP_SYSTOLIC_WARNING = 140;
        public const int BP_SYSTOLIC_CRITICAL = 160;
        public const int BP_DIASTOLIC_WARNING = 90;
        public const int BP_DIASTOLIC_CRITICAL = 110;
        public const int BP_SYSTOLIC_LOW = 90;
        public const int BP_DIASTOLIC_LOW = 60;

        // Maternal Pulse thresholds
        public const int PULSE_NORMAL_MIN = 60;
        public const int PULSE_NORMAL_MAX = 100;
        public const int PULSE_WARNING_MAX = 120;

        // Temperature thresholds (Celsius)
        public const double TEMP_NORMAL_MIN = 36.0;
        public const double TEMP_NORMAL_MAX = 37.5;
        public const double TEMP_WARNING_MAX = 38.0;
        public const double TEMP_CRITICAL_MAX = 38.5;

        // Contraction thresholds
        public const int CONTRACTION_NORMAL_MAX = 5;
        public const int CONTRACTION_HYPERSTIMULATION = 6; // Per 10 minutes
        public const int CONTRACTION_DURATION_MIN = 20;    // Seconds
        public const int CONTRACTION_DURATION_MAX = 90;

        // Labor progression thresholds
        public const double DILATATION_RATE_PRIMIPARA = 1.0;   // cm per hour
        public const double DILATATION_RATE_MULTIPARA = 1.5;   // cm per hour
        public const int ALERT_LINE_HOURS = 4;  // Hours from 4cm to 10cm
        public const int ACTION_LINE_HOURS = 6; // 2 hours behind alert line

        // Urine protein/acetone alert levels
        public static readonly string[] URINE_PROTEIN_WARNING = { "++", "+++", "++++" };
        public static readonly string[] URINE_ACETONE_WARNING = { "++", "+++", "++++" };
    }
}
