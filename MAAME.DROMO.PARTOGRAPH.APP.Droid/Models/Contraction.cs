namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Models
{
    // Contractions (every 30 minutes)
    public class Contraction : BasePartographMeasurement
    {
        public int FrequencyPer10Min { get; set; }
        public int DurationSeconds { get; set; }
        //public string Strength { get; set; } = "Moderate"; // Mild, Moderate, Strong
        //public string Regularity { get; set; } = "Regular"; // Regular, Irregular
        //public bool PalpableAtRest { get; set; }
        //public string EffectOnCervix { get; set; } = string.Empty; // Effacing, Dilating, No change
        //public bool Coordinated { get; set; }
    }
}
