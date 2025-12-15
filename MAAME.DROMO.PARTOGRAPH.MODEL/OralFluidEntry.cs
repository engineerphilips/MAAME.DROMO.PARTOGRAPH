namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Oral Fluid and Nutrition - WHO Labour Care Guide Section 4: Care of the Woman
    // WHO 2020: Encourage oral fluids during labor unless contraindicated
    // Routine NBM (nil by mouth) policy is NOT recommended
    public class OralFluidEntry : BasePartographMeasurement
    {
        // Basic Oral Fluid Status (legacy field)
        public string? OralFluid { get; set; }
        public string? OralFluidDisplay => OralFluid != null ? (OralFluid == "Y" ? "Yes" : OralFluid == "N" ? "No" : OralFluid == "D" ? "Declined" : string.Empty) : string.Empty;

        // Fluid Type (WHO: encourage clear fluids)
        public string FluidType { get; set; } = string.Empty; // Water, IceChips, EnergyDrink, Juice, Soup, Tea, IsotonicDrink

        // Amount Consumed
        public int AmountMl { get; set; }
        public int RunningTotalOralIntake { get; set; } // Cumulative oral intake for this labor

        // Tolerance (WHO: document tolerance)
        public bool Tolerated { get; set; } = true;
        public bool Vomiting { get; set; }
        public bool Nausea { get; set; }

        // Vomiting Details (if present)
        public int? VomitingEpisodes { get; set; }
        public string VomitContent { get; set; } = string.Empty; // FluidOnly, Food, Bile, Blood

        // Nutritional Intake (WHO: light diet not contraindicated in low-risk labor)
        public bool FoodOffered { get; set; }
        public bool FoodConsumed { get; set; }
        public string FoodType { get; set; } = string.Empty; // LightSnack, Fruit, Biscuits, Toast

        // NBM Status (WHO: NOT routinely recommended)
        public bool NBM { get; set; }
        public string NBMReason { get; set; } = string.Empty; // PlannedCS, GeneralAnesthesiaRisk, MaternalRequest, PersistentVomiting

        // Restrictions
        public string Restrictions { get; set; } = string.Empty; // None, ClearFluidsOnly, NBM, IceChipsOnly

        // Restriction Reason
        public string RestrictionReason { get; set; } = string.Empty; // AnesthesiaRisk, Vomiting, HighRiskLabor, MaternalChoice

        // Patient Preference
        public bool PatientRequestedFluids { get; set; }
        public bool PatientDeclinedFluids { get; set; }

        // Aspiration Risk Assessment (if NBM instituted)
        public bool AspirationRiskAssessed { get; set; }
        public string AspirationRiskLevel { get; set; } = string.Empty; // Low, Moderate, High

        // WHO 2020 Alert Status
        public string ClinicalAlert { get; set; } = string.Empty;
        // Prolonged NBM without medical indication (WHO: not recommended)
        // Persistent vomiting (may indicate labor complications)
        // Inadequate oral intake with signs of dehydration
    }
}
