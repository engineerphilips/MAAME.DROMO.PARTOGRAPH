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

    /// <summary>
    /// Alert thresholds based on WHO Labour Care Guide 2020
    /// ISBN 978-92-4-001756-6
    /// </summary>
    public class AlertThresholds
    {
        // WHO 2020: Fetal Heart Rate thresholds (Page 37-38)
        // Abnormal FHR: <110 or >160 bpm
        public const int FHR_NORMAL_MIN = 110;
        public const int FHR_NORMAL_MAX = 160;
        public const int FHR_CRITICAL_MIN = 100;  // Severe bradycardia
        public const int FHR_CRITICAL_MAX = 180;  // Severe tachycardia

        // WHO 2020: Maternal Blood Pressure thresholds (Page 41-42)
        // Severe hypertension: Systolic ≥160 or Diastolic ≥110 mmHg
        // Hypertension: Systolic ≥140 or Diastolic ≥90 mmHg
        public const int BP_SYSTOLIC_WARNING = 140;
        public const int BP_SYSTOLIC_CRITICAL = 160;  // Severe hypertension
        public const int BP_DIASTOLIC_WARNING = 90;
        public const int BP_DIASTOLIC_CRITICAL = 110; // Severe hypertension
        public const int BP_SYSTOLIC_LOW = 90;        // Hypotension
        public const int BP_DIASTOLIC_LOW = 60;

        // WHO 2020: Maternal Pulse thresholds (Page 41)
        // Tachycardia: >100 bpm
        public const int PULSE_NORMAL_MIN = 60;
        public const int PULSE_NORMAL_MAX = 100;
        public const int PULSE_WARNING_MAX = 120;  // Maternal tachycardia concern

        // WHO 2020: Temperature thresholds (Page 42)
        // Fever: ≥38°C
        public const double TEMP_NORMAL_MIN = 36.0;
        public const double TEMP_NORMAL_MAX = 37.4;
        public const double TEMP_WARNING_MAX = 38.0;   // Fever threshold
        public const double TEMP_CRITICAL_MAX = 38.5;  // High fever

        // WHO 2020: Contraction thresholds (Page 36-37)
        // Normal: 3-5 contractions per 10 minutes in active labor
        // Tachysystole: >5 contractions per 10 minutes
        public const int CONTRACTION_NORMAL_MIN = 3;
        public const int CONTRACTION_NORMAL_MAX = 5;
        public const int CONTRACTION_TACHYSYSTOLE = 6; // >5 per 10 minutes
        public const int CONTRACTION_DURATION_MIN = 20;    // Seconds
        public const int CONTRACTION_DURATION_MAX = 60;    // Typical maximum

        // WHO 2020: Labor progression thresholds (Page 29-35)
        // Active labor starts at 5cm with regular contractions
        // Alert line: 1cm/hour from 5cm (reaches 10cm in 5 hours)
        // Action line: 4 hours to the right of alert line
        public const int ACTIVE_LABOR_START_CM = 5;        // Active labor threshold
        public const double DILATATION_RATE_EXPECTED = 1.0; // cm per hour (for all women)
        public const int ALERT_LINE_START_CM = 5;          // Alert line starts at 5cm
        public const int ALERT_LINE_HOURS = 5;             // Hours from 5cm to 10cm
        public const int ACTION_LINE_OFFSET_HOURS = 4;    // Action line is 4 hours right of alert

        // WHO 2020: Urine protein/acetone alert levels (Page 42-43)
        // Proteinuria ≥2+ should be investigated
        public static readonly string[] URINE_PROTEIN_WARNING = { "++", "+++", "++++" };
        public static readonly string[] URINE_ACETONE_WARNING = { "++", "+++", "++++" };

        // WHO 2020: Monitoring intervals
        public const int FHR_MONITORING_INTERVAL_MINUTES = 30;      // Every 30 minutes in active labor
        public const int BP_MONITORING_INTERVAL_MINUTES = 240;      // Every 4 hours
        public const int TEMP_MONITORING_INTERVAL_MINUTES = 240;    // Every 4 hours
        public const int CONTRACTION_MONITORING_INTERVAL_MINUTES = 30; // Every 30 minutes
        public const int VAGINAL_EXAM_INTERVAL_HOURS = 4;          // Every 4 hours in active labor
    }
}
