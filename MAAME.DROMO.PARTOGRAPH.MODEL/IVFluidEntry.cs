using System;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // IV Fluid Management
    public class IVFluidEntry : BasePartographMeasurement
    {
        public string FluidType { get; set; } = string.Empty; // Normal saline, Hartmann's, Dextrose, etc.
        public int VolumeInfused { get; set; }
        public string Rate { get; set; } = string.Empty; // ml/hr
        //public DateTime? StartTime { get; set; }
        //public string Additives { get; set; } = string.Empty; // KCl, Syntocinon, etc.
        //public string IVSite { get; set; } = string.Empty; // Left hand, Right forearm, etc.
        //public bool SiteHealthy { get; set; }
        //public string SiteCondition { get; set; } = string.Empty; // Clean, Inflamed, Swollen, etc.
    }
}
