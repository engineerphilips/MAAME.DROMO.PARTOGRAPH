namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Models
{
    // Medication Administration
    public class MedicationEntry : BasePartographMeasurement
    {
        public string MedicationName { get; set; } = string.Empty;
        public string Dose { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty; // IV, IM, PO, Sublingual, etc.
        public DateTime AdministrationTime { get; set; } = DateTime.Now;
        public string Indication { get; set; } = string.Empty;
        public string PrescribedBy { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public bool AdverseReaction { get; set; }
        public string AdverseReactionDetails { get; set; } = string.Empty;
    }
}
