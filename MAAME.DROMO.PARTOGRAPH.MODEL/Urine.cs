namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    // Urine Output (every hour)
    public class Urine : BasePartographMeasurement
    {
        public string Protein { get; set; } = "Nil"; // Nil, Trace, +, ++, +++
        public string Acetone { get; set; } = "Nil"; // Nil, Trace, +, ++, +++

        //public int OutputMl { get; set; }
        //public string Color { get; set; } = "Yellow"; // Clear, Yellow, Dark yellow, Brown
        //public string Protein { get; set; } = "Nil"; // Nil, Trace, +, ++, +++
        //public string Glucose { get; set; } = "Nil"; // Nil, Trace, +, ++, +++
        //public string Ketones { get; set; } = "Nil"; // Nil, Trace, +, ++, +++
        //public string SpecificGravity { get; set; } = string.Empty;
        //public bool Catheterized { get; set; }
        //public DateTime? LastVoided { get; set; }
    }
}
