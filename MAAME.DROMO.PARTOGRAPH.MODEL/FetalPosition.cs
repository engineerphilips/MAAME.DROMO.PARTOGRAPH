namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Fetal Position
    public class FetalPosition : BasePartographMeasurement
    {
        public string Position { get; set; } = string.Empty; // LOA, ROA, LOP, ROP, etc.
        public string? PositionDisplay => Position != null ? Position.ToString() : string.Empty;
        //public string Presentation { get; set; } = "Vertex"; // Vertex, Breech, Transverse
        //public string Lie { get; set; } = "Longitudinal"; // Longitudinal, Transverse, Oblique
        //public bool Engaged { get; set; }
        //public string Flexion { get; set; } = "Flexed"; // Flexed, Deflexed, Extended
    }
}
