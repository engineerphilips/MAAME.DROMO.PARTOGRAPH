namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Oral Fluid Intake
    public class OralFluidEntry : BasePartographMeasurement
    {
        public char? OralFluid { get; set; }
        public string? OralFluidDisplay => OralFluid != null ? OralFluid.ToString() : string.Empty;

        //public string FluidType { get; set; } = string.Empty; // Water, Ice chips, Energy drink, etc.
        //public int AmountMl { get; set; }
        //public bool Tolerated { get; set; }
        //public bool Vomiting { get; set; }
        //public string Restrictions { get; set; } = string.Empty;
    }
}
