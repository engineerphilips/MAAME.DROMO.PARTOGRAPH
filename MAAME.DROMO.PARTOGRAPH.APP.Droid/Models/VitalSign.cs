using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Models
{
    public class VitalSign
    {
        public Guid? ID { get; set; }
        public Guid? PatientID { get; set; }
        public DateTime RecordedTime { get; set; } = DateTime.Now;

        public int SystolicBP { get; set; }
        public int DiastolicBP { get; set; }
        public decimal Temperature { get; set; } // Celsius
        public int PulseRate { get; set; }
        public int RespiratoryRate { get; set; }
        public string UrineOutput { get; set; } = string.Empty;
        public string UrineProtein { get; set; } = "Nil";
        public string UrineAcetone { get; set; } = "Nil";

        public string RecordedBy { get; set; } = string.Empty;

        [JsonIgnore]
        public string BPDisplay => $"{SystolicBP}/{DiastolicBP}";

        [JsonIgnore]
        public bool IsAbnormal =>
            SystolicBP > 140 || DiastolicBP > 90 ||
            Temperature > 37.5m || PulseRate > 100;
    }
}
