namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Models
{
    // Baseline FHR (every 30 minutes)
    public class Temperature : BasePartographMeasurement
    {
        public float? Rate { get; set; }
    }
}
