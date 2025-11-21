namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Models
{
    // Cervical Dilatation
    public class CervixDilatation : BasePartographMeasurement
    {
        public int DilatationCm { get; set; }
        //public int EffacementPercent { get; set; }
        //public string Consistency { get; set; } = "Soft"; // Firm, Soft, Very soft
        //public string Position { get; set; } = "Central"; // Posterior, Central, Anterior
        //public bool ApplicationToHead { get; set; }
        //public string CervicalEdema { get; set; } = "None"; // None, Slight, Moderate, Marked
    }
}
