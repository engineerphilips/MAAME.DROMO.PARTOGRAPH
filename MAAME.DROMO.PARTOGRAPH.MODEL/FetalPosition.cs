namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Fetal Position - WHO Labour Care Guide Section 5: Labour Progress
    // WHO 2020: Assess position every 4 hours in first stage, every 30 minutes in second stage
    // Critical for identifying malposition and predicting labor outcome
    public class FetalPosition : BasePartographMeasurement
    {
        // Lie (relationship of fetal long axis to maternal long axis)
        // WHO: Assessed by abdominal palpation - critical for delivery planning
        public string Lie { get; set; } = "Longitudinal"; // Longitudinal, Transverse, Oblique

        // Presentation (presenting part entering pelvis)
        // WHO: Non-vertex presentations require specialist assessment
        public string Presentation { get; set; } = "Vertex"; // Vertex, Breech, Face, Brow, Shoulder, Compound

        // Presenting Part (specific detail for breech presentations)
        public string PresentingPart { get; set; } = string.Empty; // CompleteBreech, FootlingBreech, FrankBreech

        // Position (relationship of denominator to maternal pelvis)
        // Standard notation: LOA, ROA, LOT, ROT, LOP, ROP, etc.
        public string Position { get; set; } = string.Empty; // LOA, ROA, LOP, ROP, LOT, ROT, OA, OP, OT

        // Display helper for UI
        public string? PositionDisplay => Position != null ? Position.ToString() : string.Empty;

        // Variety (position of occiput/sacrum relative to pelvis)
        public string Variety { get; set; } = string.Empty; // Anterior, Posterior, Transverse

        // Flexion/Attitude (degree of flexion of fetal head)
        // WHO: Well-flexed presents smallest diameter
        public string Flexion { get; set; } = "Flexed"; // WellFlexed, Flexed, Deflexed, Extended

        // Engagement (head entering pelvic inlet)
        // WHO: Important milestone - usually occurs before labor in nulliparous
        public bool Engaged { get; set; }

        // Level of Presenting Part (relationship to ischial spines)
        public string Level { get; set; } = string.Empty; // Above, AtSpines, Below

        // Assessment Method
        public string AssessmentMethod { get; set; } = string.Empty; // AbdominalPalpation, VaginalExamination, Both, Ultrasound

        // Rotation Progress (important in second stage)
        public string RotationProgress { get; set; } = string.Empty; // NotRotated, Rotating, CompleteRotation

        // WHO 2020 Alert Status
        public string ClinicalAlert { get; set; } = string.Empty;
        // Persistent OP position with arrest (may need operative delivery)
        // Transverse lie in labor (requires CS)
        // Breech presentation (specialist delivery)
        // Deep transverse arrest
        // Deflexed/Extended positions (larger diameter - may arrest)
    }
}
