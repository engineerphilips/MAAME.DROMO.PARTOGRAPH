namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Caput Succedaneum - WHO Labour Care Guide Section 5: Labour Progress
    // WHO 2020: Assess caput on vaginal examination - indicator of labor duration/obstruction
    // Increasing caput suggests prolonged labor or cephalopelvic disproportion
    public class Caput : BasePartographMeasurement
    {
        // Degree of Caput (swelling of scalp)
        public string? Degree { get; set; } // None, Plus1, Plus2, Plus3
        public string? CaputDisplay => Degree != null ? Degree.ToString() : string.Empty;

        // Location (indicates presenting part and position)
        public string Location { get; set; } = string.Empty; // Parietal, Occipital, Frontal, Vertex

        // Size Assessment
        public string Size { get; set; } = string.Empty; // Small, Moderate, Large, Extensive

        // Consistency
        public string Consistency { get; set; } = string.Empty; // Soft, Firm, Boggy

        // Progression (critical - increasing caput is concerning)
        public bool Increasing { get; set; }
        public bool Decreasing { get; set; }
        public bool Stable { get; set; }

        // Progression Rate
        public string ProgressionRate { get; set; } = string.Empty; // Rapid, Gradual, NoChange

        // Time Since First Detected
        public DateTime? FirstDetectedTime { get; set; }
        public int? DurationHours { get; set; }

        // Associated with Moulding
        public bool MouldingPresent { get; set; }
        public string MouldingDegree { get; set; } = string.Empty; // None, Plus1, Plus2, Plus3

        // Clinical Significance (WHO: assess for labor obstruction)
        public bool SuggestsObstruction { get; set; }
        public bool SuggestionProlongedLabor { get; set; }

        // Previous Assessment Comparison
        public string ChangeFromPrevious { get; set; } = string.Empty; // Improved, Worse, Unchanged

        // WHO 2020 Alert Status
        public string ClinicalAlert { get; set; } = string.Empty;
        // +++ caput suggests prolonged labor - review needed
        // Rapidly increasing caput with arrested descent suggests CPD
        // Extensive caput with +3 moulding = high risk of obstruction
    }
}
