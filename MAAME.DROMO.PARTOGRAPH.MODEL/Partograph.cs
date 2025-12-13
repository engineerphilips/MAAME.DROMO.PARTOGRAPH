using MAAME.DROMO.PARTOGRAPH.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public string GestationalAge => ExpectedDeliveryDate != null ? new EMPEROR.COMMON.GestationalAge().Age(DateTime.Now, new DateTime(ExpectedDeliveryDate.Value.Year, ExpectedDeliveryDate.Value.Month, ExpectedDeliveryDate.Value.Day), true) : LastMenstrualDate != null ? new EMPEROR.COMMON.GestationalAge().Age(new DateTime(LastMenstrualDate.Value.Year, LastMenstrualDate.Value.Month, LastMenstrualDate.Value.Day), DateTime.Now, false) : string.Empty;

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
        public IEnumerable<PartographDiagnosis> Diagnoses { get; set; } = Enumerable.Empty<PartographDiagnosis>();

        // Risk Factors
        public IEnumerable<PartographRiskFactor> RiskFactors { get; set; } = Enumerable.Empty<PartographRiskFactor>();
        public string Complications { get; set; } = string.Empty;

        // Partographs
        public List<CompanionEntry> Companions { get; set; }
        public List<Contraction> Contractions { get; set; }
        public List<Caput> Caputs { get; set; }
        public List<BP> BPs { get; set; }
        public List<Assessment> Assessments { get; set; }
        public List<Plan> Plans { get; set; }
        public List<Moulding> Mouldings { get; set; }
        public List<FetalPosition> FetalPositions { get; set; }
        public List<CervixDilatation> Dilatations { get; set; }
        public List<AmnioticFluid> AmnioticFluids { get; set; }
        public List<HeadDescent> HeadDescents { get; set; }
        public List<FHR> Fhrs { get; set; }
        public List<PainReliefEntry> PainReliefs { get; set; }
        public List<PostureEntry> Postures { get; set; }
        public List<Temperature> Temperatures { get; set; }
        public List<Urine> Urines { get; set; }
        public List<OralFluidEntry> OralFluids { get; set; }
        public List<Oxytocin> Oxytocins { get; set; }
        public List<IVFluidEntry> IVFluids { get; set; }
        public List<MedicationEntry> Medications { get; set; }
        
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
            LaborStatus.Pending => Color.Orange,
            LaborStatus.Active => Color.Green,
            LaborStatus.Completed => Color.Blue,
            LaborStatus.Emergency => Color.Red,
            _ => Color.Gray
        };

        [JsonIgnore]
        public string DisplayInfo => $"G{Gravida}P{Parity} • {GestationalAge} • {Patient?.Age}yrs";
        [JsonIgnore]
        public string Name => Patient?.Name;
        [JsonIgnore]
        public string HospitalNumber => Patient?.HospitalNumber;

        // Current/Latest measurements
        [JsonIgnore]
        public int? CurrentDilatation => Dilatations?.OrderByDescending(d => d.Time).FirstOrDefault()?.DilatationCm;
        [JsonIgnore]
        public int? CurrentHeadDescent => HeadDescents?.OrderByDescending(h => h.Time).FirstOrDefault()?.Station;
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

        public DateTime? SecondStageStartTime { get; set; }

        public string CalculateHash()
        {
            var data = $"{ID}|{Handler}";
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }

    }
}
