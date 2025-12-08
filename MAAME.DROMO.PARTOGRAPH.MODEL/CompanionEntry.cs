namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    public class CompanionEntry : BasePartographMeasurement
    {
        public string? Companion { get; set; }
        public string? CompanionDisplay => Companion != null ? (Companion == "Y" ? "Yes" : Companion == "N" ? "No" : Companion == "D" ? "Declined" : string.Empty) : string.Empty;
    }
}
