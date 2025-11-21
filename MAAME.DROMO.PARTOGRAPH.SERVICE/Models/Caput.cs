namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Models
{
    // Caput Succedaneum
    public class Caput : BasePartographMeasurement
    {
        public string? Degree { get; set; }
        public string? CaputDisplay => Degree != null ? Degree.ToString() : string.Empty;
        //public string Degree { get; set; } = "None"; // None, +, ++, +++
        //public string Location { get; set; } = string.Empty; // Parietal, Occipital, etc.
        //public bool Increasing { get; set; }
        //public string Consistency { get; set; } = string.Empty; // Soft, Firm, Hard
    }
}
