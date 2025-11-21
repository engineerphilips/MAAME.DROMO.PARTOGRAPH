namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Models
{
    // Oxytocin Administration
    public class Oxytocin : BasePartographMeasurement
    {
        //public bool InUse { get; set; }
        public decimal DoseMUnitsPerMin { get; set; }
        public decimal TotalVolumeInfused { get; set; }
        //public DateTime? StartTime { get; set; }
        //public string Indication { get; set; } = string.Empty; // Induction, Augmentation
        //public bool Contraindications { get; set; }
        //public string ContraindicationDetails { get; set; } = string.Empty;
        //public string Response { get; set; } = string.Empty; // Good, Poor, Hyperstimulation
    }
}
