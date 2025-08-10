using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Models
{
    public class PartographEntry
    {
        public int ID { get; set; }
        public int PatientID { get; set; }
        public DateTime RecordedTime { get; set; } = DateTime.Now;

        // Cervical Progress
        public int CervicalDilation { get; set; } // 0-10 cm
        public string DescentOfHead { get; set; } = string.Empty; // Station -3 to +3

        // Contractions
        public int ContractionsPerTenMinutes { get; set; }
        public int ContractionDuration { get; set; } // in seconds
        public string ContractionStrength { get; set; } = "Moderate";

        // Fetal Heart Rate
        public int FetalHeartRate { get; set; } // beats per minute

        // Maternal Observations
        public string LiquorStatus { get; set; } = "Clear";
        public string Moulding { get; set; } = "None";
        public string Caput { get; set; } = "None";

        // Medical Interventions
        public string MedicationsGiven { get; set; } = string.Empty;
        public string OxytocinUnits { get; set; } = string.Empty;
        public string IVFluids { get; set; } = string.Empty;

        public string RecordedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
