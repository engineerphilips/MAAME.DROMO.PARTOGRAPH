namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Models
{
    // Baseline FHR (every 30 minutes)
    public class BP : BasePartographMeasurement
    {
        public int? Systolic { get; set; }
        public int? Diastolic { get; set; }
        public int? Pulse { get; set; }
    }
}
