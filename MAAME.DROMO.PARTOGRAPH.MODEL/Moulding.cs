namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Moulding
    public class Moulding : BasePartographMeasurement
    {
        //public string Degree { get; set; } = "None"; // None, +, ++, +++
        public int Degree { get; set; }
        public string? DegreeDisplay => Degree > -1 ? Degree.ToString() : string.Empty;
        //public bool SuturesOverlapping { get; set; }
        //public string Location { get; set; } = string.Empty; // Sagittal, Coronal, Lambdoid
        //public bool Reducing { get; set; }
    }
}
