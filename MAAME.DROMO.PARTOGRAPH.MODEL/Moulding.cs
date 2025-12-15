namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Moulding - WHO Labour Care Guide Section 5: Labour Progress
    // WHO 2020: Assess moulding on vaginal examination - indicator of head compression
    // Severe moulding (+++) suggests cephalopelvic disproportion
    public class Moulding : BasePartographMeasurement
    {
        // Degree of Moulding (overlapping of skull bones)
        // 0 or None: Sutures separated, bones easily felt
        // +: Sutures apposed (touching)
        // ++: Sutures overlapping but reducible
        // +++: Sutures overlapping and not reducible (concerning - suggests CPD)
        public string Degree { get; set; } // None, Plus1, Plus2, Plus3
        public string? DegreeDisplay => Degree != null ? Degree.ToString() : string.Empty;

        // Suture Overlap Status
        public bool SuturesOverlapping { get; set; }
        public bool Reducible { get; set; } // Can overlapping bones be separated?

        // Location (which sutures affected)
        public string Location { get; set; } = string.Empty; // Sagittal, Coronal, Lambdoid, Multiple

        // Affected Sutures (detailed assessment)
        public bool SagittalSuture { get; set; }
        public bool CoronalSuture { get; set; }
        public bool LambdoidSuture { get; set; }

        // Severity Assessment
        public string Severity { get; set; } = string.Empty; // Mild, Moderate, Severe, Extreme

        // Progression (critical - increasing moulding is concerning)
        public bool Increasing { get; set; }
        public bool Reducing { get; set; }
        public bool Stable { get; set; }

        // Progression Rate
        public string ProgressionRate { get; set; } = string.Empty; // Rapid, Gradual, NoChange

        // Time Since First Detected
        public DateTime? FirstDetectedTime { get; set; }
        public int? DurationHours { get; set; }

        // Associated with Caput
        public bool CaputPresent { get; set; }
        public string CaputDegree { get; set; } = string.Empty; // None, Plus1, Plus2, Plus3

        // Clinical Significance (WHO: assess for labor obstruction)
        public bool SuggestsObstruction { get; set; }
        public bool SuggestsCPD { get; set; }

        // Previous Assessment Comparison
        public string ChangeFromPrevious { get; set; } = string.Empty; // Improved, Worse, Unchanged

        // WHO 2020 Alert Status
        public string ClinicalAlert { get; set; } = string.Empty;
        // +++ moulding is CRITICAL - suggests severe CPD - senior review required
        // Increasing moulding with arrested descent = likely obstructed labor
        // +++ moulding + +++ caput = high likelihood of CS required
    }
}
