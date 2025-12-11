namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Baseline FHR (every 30 minutes)
    public class FHR : BasePartographMeasurement
    {
        public int? Rate { get; set; } // 110-160 normal
        public string Deceleration { get; set; }
        //public string Variability { get; set; } = string.Empty; // Absent, Minimal, Moderate, Marked
        //public bool Accelerations { get; set; }
        //public string Pattern { get; set; } = string.Empty; // Reassuring, Non-reassuring, Abnormal
        //public string MonitoringMethod { get; set; } = string.Empty; // Intermittent, Continuous CTG, Doppler
    }
}
