namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Maternal Posture
    public class PostureEntry : BasePartographMeasurement
    {
        public string? Posture { get; set; }
        public string? PostureDisplay => Posture != null ? Posture.ToString() : string.Empty;

    }
}
