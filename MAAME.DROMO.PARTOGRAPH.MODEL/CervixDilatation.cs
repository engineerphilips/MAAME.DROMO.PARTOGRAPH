namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Cervical Dilatation - WHO Labour Care Guide Section 5: Labour Progress
    // WHO 2020: Active labor starts at 5cm (not 4cm as in old partograph)
    // Document during vaginal examination
    public class CervixDilatation : BasePartographMeasurement
    {
        // Dilatation (0-10 cm)
        public int DilatationCm { get; set; } // Range 0-10, cannot regress

        // Effacement (0-100%) - Required for Bishop Score
        public int EffacementPercent { get; set; }

        // Cervical Consistency - Required for Bishop Score
        public string Consistency { get; set; } = "Medium"; // Firm, Medium, Soft

        // Cervical Position - Required for Bishop Score
        public string Position { get; set; } = "Mid"; // Posterior, Mid, Anterior

        // Application to Presenting Part
        public bool ApplicationToHead { get; set; }

        // Cervical Edema
        public string CervicalEdema { get; set; } = "None"; // None, Slight, Moderate, Marked

        // Membrane Status
        public string MembraneStatus { get; set; } = string.Empty; // Intact, Bulging, Ruptured

        // Cervical Lip
        public bool CervicalLip { get; set; }

        // Labor Phase Indicator (WHO 2020 definition)
        // Latent: <5cm, Active: ≥5cm
        public string LaborPhase
        {
            get
            {
                if (DilatationCm < 5) return "Latent";
                if (DilatationCm < 10) return "Active";
                return "FullyDilated";
            }
        }
    }
}
