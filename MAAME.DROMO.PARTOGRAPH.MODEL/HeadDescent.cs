namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Head Descent - WHO Labour Care Guide Section 5: Labour Progress
    // WHO 2020: Assess descent every 4 hours in first stage, every 30 minutes in second stage
    // Critical for diagnosing labor dystocia and cephalopelvic disproportion
    public class HeadDescent : BasePartographMeasurement
    {
        // Station (relative to ischial spines)
        // Range: -5 to +5 cm or -3 to +3 (depending on system)
        // 0 = at ischial spines, negative = above, positive = below
        public int Station { get; set; }

        // Palpable Abdominally (WHO/UK standard - assessed by abdominal palpation)
        // 5/5 = fully palpable, 0/5 = not palpable (fully descended)
        // Engagement occurs at 2/5 or less palpable
        public string PalpableAbdominally { get; set; } = string.Empty; // 5/5, 4/5, 3/5, 2/5, 1/5, 0/5

        // Engagement Status (head entering pelvis - critical milestone)
        // WHO: Engagement typically occurs when <2/5 palpable or station 0/below
        public bool Engaged { get; set; }

        // Synclitism (relationship of sagittal suture to pelvis on VE)
        // Asynclitism may indicate cephalopelvic disproportion
        public string Synclitism { get; set; } = "Normal"; // Normal, AnteriorAsynclitic, PosteriorAsynclitic

        // Flexion (affects presenting diameter and descent)
        // Well flexed = smallest diameter, extended = larger diameter (may arrest)
        public string Flexion { get; set; } = "Flexed"; // Extended, Deflexed, Flexed, WellFlexed

        // Second Stage Indicators (WHO: monitor every 30 minutes in second stage)
        public bool VisibleAtIntroitus { get; set; }
        public bool Crowning { get; set; }

        // Rotation (position of occiput)
        public string Rotation { get; set; } = string.Empty; // OA, OT, OP, LOA, ROA, LOT, ROT, LOP, ROP

        // Descent Progress Assessment
        public string DescentRate { get; set; } = string.Empty; // Normal, Slow, Arrested, Rapid

        // Clinical Decision Support
        public bool DescentRegression { get; set; } // Abnormal - head should not ascend after descent

        // WHO 2020 Alert Status
        public string ClinicalAlert { get; set; } = string.Empty;
        // Lack of descent in second stage for 1 hour (nulliparous) or 30 min (multiparous)
        // Persistent OP position with arrest
        // Asynclitism with no descent suggests CPD
    }
}
