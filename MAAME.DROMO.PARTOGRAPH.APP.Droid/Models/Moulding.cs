namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Models
{
    // Moulding
    public class Moulding : BasePartographMeasurement
    {
        public string Degree { get; set; } = "None"; // None, +, ++, +++
        public string? MouldingDisplay => Degree != null ? Degree.ToString() : string.Empty;
        //public bool SuturesOverlapping { get; set; }
        //public string Location { get; set; } = string.Empty; // Sagittal, Coronal, Lambdoid
        //public bool Reducing { get; set; }
    }
}
