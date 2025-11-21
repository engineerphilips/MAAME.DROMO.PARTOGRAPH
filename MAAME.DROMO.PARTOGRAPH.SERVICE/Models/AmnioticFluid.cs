namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Models
{
    //// FHR Deceleration (every 30 minutes)
    //public class FHRDecelerationEntry : BasePartographMeasurement
    //{
    //    public bool DecelerationsPresent { get; set; }
    //    public string DecelerationType { get; set; } = string.Empty; // Early, Late, Variable, Prolonged
    //    public string Severity { get; set; } = string.Empty; // Mild, Moderate, Severe
    //    public int Duration { get; set; } // in seconds
    //    public string Recovery { get; set; } = string.Empty; // Quick, Slow, Poor
    //    public bool RequiresAction { get; set; }
    //    public string ActionTaken { get; set; } = string.Empty;
    //}

    // Amniotic Fluid
    public class AmnioticFluid : BasePartographMeasurement
    {
        //public string Color { get; set; } = "Clear"; // Clear, Straw, Green, Brown, Blood-stained
        //public string Consistency { get; set; } = "Normal"; // Normal, Thick, Tenacious
        //public string Amount { get; set; } = "Normal"; // Normal, Reduced (oligohydramnios), Excessive (polyhydramnios)
        //public string Odor { get; set; } = "None"; // None, Offensive, Fishy
        //public bool MeconiumStaining { get; set; }
        //public string MeconiumGrade { get; set; } = string.Empty; // Grade I, II, III

        public string Color { get; set; } = string.Empty; // LOA, ROA, LOP, ROP, etc.
        public string? AmnioticFluidDisplay => Color != null ? Color.ToString() : string.Empty;
    }
}
