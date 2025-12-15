namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
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

        // WHO 2020 Enhanced Amniotic Fluid Assessment
        // Membrane Rupture Details
        public string RuptureMethod { get; set; } = string.Empty; // SROM, AROM, PROM, PPROM
        public string RuptureLocation { get; set; } = string.Empty; // HighLeak, Forewaters, Hindwaters
        public bool ConfirmedRupture { get; set; }
        public string ConfirmationMethod { get; set; } = string.Empty; // Visual, Nitrazine, Ferning, Speculum

        // Fluid Volume Assessment
        public string FluidVolume { get; set; } = string.Empty; // Normal, Oligohydramnios, Polyhydramnios
        public int? EstimatedVolumeMl { get; set; }
        public bool PoolingInVagina { get; set; }

        // Meconium Details
        public DateTime? MeconiumFirstNotedTime { get; set; }
        public bool MeconiumThickParticulate { get; set; }
        public bool NeonatalTeamAlerted { get; set; }
        public DateTime? NeonatalTeamAlertTime { get; set; }

        // Chorioamnionitis Risk
        public bool ProlongedRupture { get; set; } // >18-24 hours
        public int? HoursSinceRupture { get; set; }
        public bool MaternalFever { get; set; }
        public bool MaternalTachycardia { get; set; }
        public bool FetalTachycardia { get; set; }
        public bool UterineTenderness { get; set; }

        // Blood-Stained Fluid
        public string BloodSource { get; set; } = string.Empty; // Show, Abruption, Vasa Previa, Traumatic
        public bool ActiveBleeding { get; set; }
        public string BleedingAmount { get; set; } = string.Empty; // Spotting, Light, Moderate, Heavy

        // Cord Considerations
        public bool CordProlapse { get; set; }
        public bool CordPresentation { get; set; }
        public DateTime? CordComplicationTime { get; set; }

        // Clinical Actions
        public bool AntibioticsIndicated { get; set; }
        public bool AmnioinfusionConsidered { get; set; }
        public bool ExpeditedDeliveryNeeded { get; set; }
    }
}
