namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    public class PainReliefEntry : BasePartographMeasurement
    {
        public string? PainRelief { get; set; }
        public string? PainReliefDisplay => PainRelief != null ? PainRelief.ToString() : string.Empty;

        //public string PainReliefMethod { get; set; } = string.Empty; // None, Epidural, Pethidine, Gas & Air, etc.
        //public DateTime? AdministeredTime { get; set; }
        //public string Dose { get; set; } = string.Empty;
        //public string Effectiveness { get; set; } = string.Empty; // Poor, Fair, Good, Excellent
        //public bool SideEffects { get; set; }
        //public string SideEffectsDescription { get; set; } = string.Empty;
    }
}
