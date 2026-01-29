using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Models
{
    /// <summary>
    /// Extended partograph details with historical data for WHO Labour Care Guide
    /// </summary>
    public class PartographDetailsDto
    {
        public LiveLaborCase CaseInfo { get; set; } = new();
        public LaborCareGuideData GuideData { get; set; } = new();
    }

    /// <summary>
    /// Structured data for WHO Labour Care Guide 2020 Grid
    /// </summary>
    public class LaborCareGuideData
    {
        // Axis
        public List<DateTime> TimePoints { get; set; } = new(); // 30-min intervals
        public DateTime StartTime { get; set; }

        // Section 1: Supportive Care
        public List<CategoricalPoint> Companion { get; set; } = new(); // Yes/No
        public List<CategoricalPoint> PainRelief { get; set; } = new(); // Yes/No
        public List<CategoricalPoint> OralFluid { get; set; } = new(); // Yes/No
        public List<CategoricalPoint> Posture { get; set; } = new(); // Supine, Mobile, etc.

        // Section 2: Care of the Baby
        public List<MeasurementPoint> BaselineFHR { get; set; } = new();
        public List<CategoricalPoint> FHRDeceleration { get; set; } = new(); // None, Early, Late, Variable
        public List<CategoricalPoint> AmnioticFluid { get; set; } = new(); // I, C, M, B, A
        public List<CategoricalPoint> FetalPosition { get; set; } = new(); // OA, OP, etc.
        public List<CategoricalPoint> Caput { get; set; } = new(); // 0, +, ++, +++
        public List<CategoricalPoint> Moulding { get; set; } = new(); // 0, +, ++, +++

        // Section 3: Care of the Woman
        public List<MeasurementPoint> Pulse { get; set; } = new();
        public List<MeasurementPoint> SystolicBP { get; set; } = new();
        public List<MeasurementPoint> DiastolicBP { get; set; } = new();
        public List<MeasurementPoint> Temperature { get; set; } = new();
        public List<CategoricalPoint> UrineProtein { get; set; } = new(); 
        public List<CategoricalPoint> UrineAcetone { get; set; } = new();
        public List<MeasurementPoint> UrineVolume { get; set; } = new();

        // Section 4: Labour Progress
        public List<MeasurementPoint> ContractionFrequency { get; set; } = new(); // per 10 min
        public List<MeasurementPoint> ContractionDuration { get; set; } = new(); // seconds
        public List<MeasurementPoint> Dilatation { get; set; } = new();
        public List<MeasurementPoint> Descent { get; set; } = new();

        // Section 5: Medication
        public List<MeasurementPoint> Oxytocin { get; set; } = new(); // U/L, drops/min
        public List<MedicationEntry> Medicines { get; set; } = new();
        public List<MeasurementPoint> IVFluids { get; set; } = new();

        // Section 6: Shared Decision Making
        public List<CategoricalPoint> Assessment { get; set; } = new(); // Normal Progress, etc.
        public List<CategoricalPoint> Plan { get; set; } = new(); // Continue monitoring, etc.
        public List<CategoricalPoint> Initials { get; set; } = new();
        
        // Alerts & Notes tied to timeline
        public List<TimelineAlert> Alerts { get; set; } = new();
    }

    public class MeasurementPoint
    {
        public DateTime Time { get; set; }
        public double Value { get; set; }
        public bool IsAlert { get; set; }
    }

    public class CategoricalPoint
    {
        public DateTime Time { get; set; }
        public string Value { get; set; } = string.Empty;
        public bool IsAlert { get; set; }
        public string AlertType { get; set; } = string.Empty; // e.g. "High", "Low", "Abnormal"
    }

    public class MedicationEntry
    {
        public DateTime Time { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Dose { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
    }

    public class TimelineAlert
    {
        public DateTime Time { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Info";
    }
}
