using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Models
{
    public class Patient
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string HospitalNumber { get; set; } = string.Empty;
        public int Age { get; set; }
        public int Gravidity { get; set; } // Number of pregnancies
        public int Parity { get; set; } // Number of births
        public DateTime AdmissionDate { get; set; } = DateTime.Now;
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string BloodGroup { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string EmergencyContact { get; set; } = string.Empty;

        // Labor Information
        public LaborStatus Status { get; set; } = LaborStatus.Pending;
        public DateTime? LaborStartTime { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public int? CervicalDilationOnAdmission { get; set; }
        public string MembraneStatus { get; set; } = "Intact";
        public string LiquorStatus { get; set; } = "Clear";

        // Risk Factors
        public string RiskFactors { get; set; } = string.Empty;
        public string Complications { get; set; } = string.Empty;

        // Navigation Properties
        public List<PartographEntry> PartographEntries { get; set; } = [];
        public List<VitalSign> VitalSigns { get; set; } = [];
        public List<MedicalNote> MedicalNotes { get; set; } = [];

        [JsonIgnore]
        public string StatusDisplay => Status switch
        {
            LaborStatus.Pending => "Pre-Labor",
            LaborStatus.Active => "Active Labor",
            LaborStatus.Completed => "Delivered",
            LaborStatus.Emergency => "Emergency",
            _ => "Unknown"
        };

        [JsonIgnore]
        public Color StatusColor => Status switch
        {
            LaborStatus.Pending => Colors.Orange,
            LaborStatus.Active => Colors.Green,
            LaborStatus.Completed => Colors.Blue,
            LaborStatus.Emergency => Colors.Red,
            _ => Colors.Gray
        };

        [JsonIgnore]
        public string DisplayInfo => $"G{Gravidity}P{Parity} • {Age}yrs • {HospitalNumber}";
    }

    public enum LaborStatus
    {
        Pending,    // Not in active labor
        Active,     // In active labor
        Completed,  // Delivered
        Emergency   // Requires immediate attention
    }
}
