namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Models
{
    // Pain Relief Management
    [SQLite.Table("Tbl_PainRelief")]
    public class PainReliefEntry : BasePartographMeasurement
    {
        public char? PainRelief { get; set; }
        public string? PainReliefDisplay => PainRelief != null ? PainRelief.ToString() : string.Empty;

        //public string PainReliefMethod { get; set; } = string.Empty; // None, Epidural, Pethidine, Gas & Air, etc.
        //public DateTime? AdministeredTime { get; set; }
        //public string Dose { get; set; } = string.Empty;
        //public string Effectiveness { get; set; } = string.Empty; // Poor, Fair, Good, Excellent
        //public bool SideEffects { get; set; }
        //public string SideEffectsDescription { get; set; } = string.Empty;
    }
}
