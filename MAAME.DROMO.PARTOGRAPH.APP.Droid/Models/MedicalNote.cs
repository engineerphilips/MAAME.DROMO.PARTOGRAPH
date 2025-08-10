using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Models
{
    public class MedicalNote
    {
        public int ID { get; set; }
        public int PatientID { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public string NoteType { get; set; } = "General"; // General, Alert, Emergency
        public string Content { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsImportant { get; set; }

        [JsonIgnore]
        public Color NoteColor => NoteType switch
        {
            "Alert" => Colors.Orange,
            "Emergency" => Colors.Red,
            _ => Colors.Gray
        };
    }
}
