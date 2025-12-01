using MAAME.DROMO.PARTOGRAPH.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    public class Partograph
    {
        public Guid? ID { get; set; }
        public Guid? PatientID { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public Patient? Patient { get; set; }
        public int Gravida { get; set; } // Number of pregnancies
        public int Parity { get; set; } // Number of births
        public int Abortion { get; set; } // Number of abortions
        public DateTime AdmissionDate { get; set; } = DateTime.Now;
        public DateOnly? ExpectedDeliveryDate { get; set; }
        public DateOnly? LastMenstrualDate { get; set; }
        
        public string GestationalAge { get; set; } = string.Empty;

        // Labour Information
        public LaborStatus Status { get; set; } = LaborStatus.Pending;
        public DateTime? LaborStartTime { get; set; }
        public string LaborStartTimeFormat => LaborStartTime != null ? ElapseTimeCalc.PeriodElapseTimeLower(LaborStartTime.Value, DateTime.Now) : string.Empty;
        public DateTime? RupturedMembraneTime { get; set; }
        public string RupturedMembraneTimeFormat => RupturedMembraneTime != null ? ElapseTimeCalc.PeriodElapseTimeLower(RupturedMembraneTime.Value, DateTime.Now) : string.Empty;
        public DateTime? DeliveryTime { get; set; }
        public int? CervicalDilationOnAdmission { get; set; }
        public string MembraneStatus { get; set; } = "Intact";
        public string LiquorStatus { get; set; } = "Clear";

        // Risk Factors
        public string RiskFactors { get; set; } = string.Empty;
        public string Complications { get; set; } = string.Empty;

        // Partographs
        public IEnumerable<Contraction> Contractions { get; set; }
        public IEnumerable<Caput> Caputs { get; set; }
        public IEnumerable<BP> BPs { get; set; }
        public IEnumerable<Moulding> Mouldings { get; set; }
        public IEnumerable<FetalPosition> FetalPositions { get; set; }
        public IEnumerable<CervixDilatation> Dilatations { get; set; }
        public IEnumerable<AmnioticFluid> AmnioticFluids { get; set; }
        public IEnumerable<HeadDescent> HeadDescents { get; set; }
        public IEnumerable<FHR> Fhrs { get; set; }
        public IEnumerable<PainReliefEntry> PainReliefs { get; set; }
        public IEnumerable<PostureEntry> Postures { get; set; }
        public IEnumerable<Temperature> Temperatures { get; set; }
        public IEnumerable<Urine> Urines { get; set; }
        public IEnumerable<OralFluidEntry> OralFluids { get; set; }
        public IEnumerable<Oxytocin> Oxytocins { get; set; }
        public IEnumerable<IVFluidEntry> IVFluids { get; set; }

        //// Cervical Progress
        //public int CervicalDilation { get; set; } // 0-10 cm
        //public string DescentOfHead { get; set; } = string.Empty; // Station -3 to +3

        //// Contractions
        //public int ContractionsPerTenMinutes { get; set; }
        //public int ContractionDuration { get; set; } // in seconds
        //public string ContractionStrength { get; set; } = "Moderate";

        //// Fetal Heart Rate
        //public int FetalHeartRate { get; set; } // beats per minute

        //// Maternal Observations
        //public string LiquorStatus { get; set; } = "Clear";
        //public string Moulding { get; set; } = "None";
        //public string Caput { get; set; } = "None";

        //// Medical Interventions
        //public string MedicationsGiven { get; set; } = string.Empty;
        //public string OxytocinUnits { get; set; } = string.Empty;
        //public string IVFluids { get; set; } = string.Empty;

        //public string RecordedBy { get; set; } = string.Empty;
        //public string Notes { get; set; } = string.Empty;

        //User
        public string HandlerName { get; set; } = string.Empty;
        public Guid? Handler { get; set; }

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
        public string DisplayInfo => $"G{Gravida}P{Parity} • {Patient?.Age}yrs • {GestationalAge}";
        [JsonIgnore]
        public string Name => Patient?.Name;
        [JsonIgnore]
        public string HospitalNumber => Patient?.HospitalNumber;
        // Sync columns
        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
        public long? DeletedTime { get; set; }

        public string DeviceId { get; set; }
        public string OriginDeviceId { get; set; }
        public int SyncStatus { get; set; }  // 0=pending, 1=synced, 2=conflict

        public int Version { get; set; }
        public int ServerVersion { get; set; }

        public int Deleted { get; set; }
        public string ConflictData { get; set; }
        public string DataHash { get; set; }

        [JsonIgnore]
        public bool HasConflict => SyncStatus == 2;

        [JsonIgnore]
        public bool NeedsSync => SyncStatus == 0;

        public string CalculateHash()
        {
            var data = $"{AdmissionDate}|{Handler}";
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }

    }
}
