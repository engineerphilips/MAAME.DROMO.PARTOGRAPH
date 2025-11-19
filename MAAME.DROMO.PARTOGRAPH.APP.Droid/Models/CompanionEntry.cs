namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Models
{
    // Companion Support
    [SQLite.Table("Tbl_Companion")]
    public class CompanionEntry : BasePartographMeasurement
    {
        public char? Companion { get; set; }
        public string? CompanionDisplay => Companion != null ? Companion.ToString() : string.Empty;
    }
}
