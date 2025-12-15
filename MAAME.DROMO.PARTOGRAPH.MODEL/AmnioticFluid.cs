namespace MAAME.DROMO.PARTOGRAPH.MODEL
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

    // Amniotic Fluid - WHO Labour Care Guide Section 5: Labour Progress / Section 3: Care of Baby
    // Assessed during vaginal examination or when membranes rupture
    // Meconium staining requires neonatal team alert
    public class AmnioticFluid : BasePartographMeasurement
    {
        // Amniotic Fluid Color (critical for fetal wellbeing assessment)
        public string Color { get; set; } = "Clear"; // Clear, Straw, Green, Brown, BloodStained, PortWine

        // Meconium Staining (WHO: important for neonatal resuscitation planning)
        public bool MeconiumStaining { get; set; }
        public string MeconiumGrade { get; set; } = string.Empty; // Grade1Thin, Grade2Moderate, Grade3Thick

        // Consistency
        public string Consistency { get; set; } = "Normal"; // Normal, Thick, Tenacious

        // Amount (if visible during examination)
        public string Amount { get; set; } = "Normal"; // Normal, Reduced, Excessive

        // Odor (offensive odor indicates chorioamnionitis)
        public string Odor { get; set; } = "None"; // None, Normal, Offensive

        // Rupture Status and Timing
        public string RuptureStatus { get; set; } = "Intact"; // Intact, SROM, AROM, Unknown
        public DateTime? RuptureTime { get; set; }

        // Duration Since Rupture (calculated)
        public TimeSpan? DurationSinceRupture
        {
            get
            {
                if (RuptureTime.HasValue)
                {
                    return DateTime.Now - RuptureTime.Value;
                }
                return null;
            }
        }

        // Display Property
        public string? AmnioticFluidDisplay => Color != null ? Color.ToString() : string.Empty;

        // Clinical Significance
        public string ClinicalAlert { get; set; } = string.Empty;
        // Grade 2-3 meconium triggers neonatal team notification
        // Offensive odor suggests chorioamnionitis
        // Blood-stained may indicate abruption
    }
}
