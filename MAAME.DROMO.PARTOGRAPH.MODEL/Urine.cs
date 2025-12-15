namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Urine Output and Assessment - WHO Labour Care Guide Section 4: Care of the Woman
    // WHO recommends documenting urine output and characteristics during labour
    public class Urine : BasePartographMeasurement
    {
        // Urine Output (critical for fluid balance and renal function)
        public int OutputMl { get; set; }

        // Urine Color
        public string Color { get; set; } = "Yellow"; // Clear, Yellow, DarkYellow, Brown, Red

        // Protein (pre-eclampsia screening)
        public string Protein { get; set; } = "Nil"; // Nil, Trace, Plus1, Plus2, Plus3

        // Ketones (maternal exhaustion/dehydration indicator)
        public string Ketones { get; set; } = "Nil"; // Nil, Trace, Plus1, Plus2, Plus3

        // Glucose
        public string Glucose { get; set; } = "Nil"; // Nil, Trace, Plus1, Plus2, Plus3

        // Specific Gravity (concentration)
        public string SpecificGravity { get; set; } = string.Empty;

        // Voiding Method
        public string VoidingMethod { get; set; } = "Spontaneous"; // Spontaneous, Catheterized, Suprapubic

        // Bladder Assessment
        public bool BladderPalpable { get; set; }
        public DateTime? LastVoided { get; set; }

        // Clinical Alerts
        public string ClinicalAlert { get; set; } = string.Empty;
        // Protein ≥Plus2 suggests pre-eclampsia screening needed
        // Ketones ≥Plus2 indicates prolonged labor/dehydration
        // Output <30ml/hour for 2+ hours = oliguria (renal concern)
    }
}
