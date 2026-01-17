using MAAME.DROMO.PARTOGRAPH.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

        [ForeignKey("PatientID")]
        public Patient? Patient { get; set; }
        public int Gravida { get; set; } // Number of pregnancies
        public int Parity { get; set; } // Number of births
        public int Abortion { get; set; } // Number of abortions
        public DateTime AdmissionDate { get; set; } = DateTime.Now;
        public DateOnly? ExpectedDeliveryDate { get; set; }
        public DateOnly? LastMenstrualDate { get; set; }

        public string GestationalAge => ExpectedDeliveryDate != null ? new EMPEROR.COMMON.GestationalAge().Age(new DateTime(ExpectedDeliveryDate.Value.Year, ExpectedDeliveryDate.Value.Month, ExpectedDeliveryDate.Value.Day), DateTime.Now, true) : LastMenstrualDate != null ? new EMPEROR.COMMON.GestationalAge().Age(new DateTime(LastMenstrualDate.Value.Year, LastMenstrualDate.Value.Month, LastMenstrualDate.Value.Day), DateTime.Now, false) : string.Empty;

        /// <summary>
        /// Calculates gestational age in weeks and days for precise assessment.
        /// Uses EDD-based calculation: 280 days (40 weeks) minus days until EDD.
        /// Example: If EDD was yesterday, gestational age = 40W1D (281 days).
        /// </summary>
        [JsonIgnore]
        public (int weeks, int days) GestationalWeeksAndDays
        {
            get
            {
                int totalDays = 0;
                var today = DateTime.Today;

                if (ExpectedDeliveryDate != null)
                {
                    var edd = new DateTime(ExpectedDeliveryDate.Value.Year,
                        ExpectedDeliveryDate.Value.Month, ExpectedDeliveryDate.Value.Day);
                    // 280 days = 40 weeks (full term)
                    // Subtract days remaining until EDD to get current gestational age
                    totalDays = 280 - (edd - today).Days;
                }
                else if (LastMenstrualDate != null)
                {
                    var lmp = new DateTime(LastMenstrualDate.Value.Year,
                        LastMenstrualDate.Value.Month, LastMenstrualDate.Value.Day);
                    totalDays = (today - lmp).Days;
                }

                if (totalDays < 0) totalDays = 0;

                int weeks = totalDays / 7;
                int days = totalDays % 7;
                return (weeks, days);
            }
        }

        /// <summary>
        /// Returns the total gestational age in days
        /// </summary>
        [JsonIgnore]
        public int GestationalTotalDays
        {
            get
            {
                var (weeks, days) = GestationalWeeksAndDays;
                return weeks * 7 + days;
            }
        }

        /// <summary>
        /// Determines gestational status based on EGA
        /// - Term: ≤40W
        /// - Prolonged Pregnancy: >40W (40W1D to 40W6D)
        /// - Post Date: ≥41W
        /// - Post Date Critical: >41W5D (SVD not recommended)
        /// </summary>
        [JsonIgnore]
        public string GestationalStatus
        {
            get
            {
                if (ExpectedDeliveryDate == null && LastMenstrualDate == null)
                    return string.Empty;

                int totalDays = GestationalTotalDays;

                // 41W5D = 292 days, 41W = 287 days, 40W = 280 days
                if (totalDays > 292) // > 41W5D
                    return "Post Date - SVD Not Recommended";
                else if (totalDays >= 287) // >= 41W
                    return "Post Date";
                else if (totalDays > 280) // > 40W
                    return "Prolonged Pregnancy";
                else
                    return "Term";
            }
        }

        /// <summary>
        /// Indicates if the pregnancy is post-date (≥41W)
        /// </summary>
        [JsonIgnore]
        public bool IsPostDate => GestationalTotalDays >= 287;

        /// <summary>
        /// Determines if SVD is allowed based on gestational age
        /// SVD is NOT allowed when EGA > 41W5D (292 days)
        /// </summary>
        [JsonIgnore]
        public bool IsSVDAllowed
        {
            get
            {
                if (ExpectedDeliveryDate == null && LastMenstrualDate == null)
                    return true; // Default to allowed if no date info

                return GestationalTotalDays <= 292;
            }
        }

        /// <summary>
        /// Gets the recommended delivery mode based on gestational age
        /// Returns "CS" for EGA > 41W5D, otherwise "SVD"
        /// </summary>
        [JsonIgnore]
        public string RecommendedDeliveryMode => GestationalTotalDays > 292 ? "CS" : "SVD";

        // Labour Information
        public LaborStatus Status { get; set; } = LaborStatus.Pending;

        /// <summary>
        /// Current phase within the first stage of labor (Latent, Active Early, Active Advanced, Transition)
        /// Automatically calculated based on cervical dilation
        /// </summary>
        public FirstStagePhase CurrentPhase { get; set; } = FirstStagePhase.NotDetermined;

        /// <summary>
        /// Gets the display name for the current first stage phase
        /// </summary>
        [JsonIgnore]
        public string CurrentPhaseDisplay => CurrentPhase switch
        {
            FirstStagePhase.NotDetermined => "Not Determined",
            FirstStagePhase.Latent => "Latent Phase (0-4cm)",
            FirstStagePhase.ActiveEarly => "Active Phase - Early (5-7cm)",
            FirstStagePhase.ActiveAdvanced => "Active Phase - Advanced (8-9cm)",
            FirstStagePhase.Transition => "Transition (10cm - Fully Dilated)",
            _ => "Unknown"
        };

        /// <summary>
        /// Gets the color code for the current phase (for UI display)
        /// </summary>
        [JsonIgnore]
        public string CurrentPhaseColor => CurrentPhase switch
        {
            FirstStagePhase.NotDetermined => "#808080", // Gray
            FirstStagePhase.Latent => "#FFFFFF",        // White - Latent phase
            FirstStagePhase.ActiveEarly => "#FFEB3B",   // Yellow - Early active
            FirstStagePhase.ActiveAdvanced => "#FF9800", // Orange - Advanced active
            FirstStagePhase.Transition => "#F44336",     // Red - Transition/Fully dilated
            _ => "#808080"
        };

        /// <summary>
        /// Calculates and returns the appropriate phase based on cervical dilation
        /// </summary>
        public static FirstStagePhase CalculatePhaseFromDilation(int? dilationCm)
        {
            if (!dilationCm.HasValue)
                return FirstStagePhase.NotDetermined;

            return dilationCm.Value switch
            {
                <= 4 => FirstStagePhase.Latent,
                <= 7 => FirstStagePhase.ActiveEarly,
                <= 9 => FirstStagePhase.ActiveAdvanced,
                >= 10 => FirstStagePhase.Transition,
                //_ => FirstStagePhase.NotDetermined
            };
        }

        // Stage Timestamps (WHO Four-Stage System)
        public DateTime? LaborStartTime { get; set; }  // Also serves as FirstStageStartTime
        public DateTime? FirstStageStartTime => LaborStartTime;  // Alias for clarity
        public string LaborStartTimeFormat => LaborStartTime != null ? ElapseTimeCalc.PeriodElapseTimeLower(LaborStartTime.Value, DateTime.Now) : string.Empty;

        public DateTime? SecondStageStartTime { get; set; }
        public string SecondStageTimeFormat => SecondStageStartTime != null ? ElapseTimeCalc.PeriodElapseTimeLower(SecondStageStartTime.Value, DateTime.Now) : string.Empty;

        public DateTime? ThirdStageStartTime { get; set; }
        public string ThirdStageTimeFormat => ThirdStageStartTime != null ? ElapseTimeCalc.PeriodElapseTimeLower(ThirdStageStartTime.Value, DateTime.Now) : string.Empty;

        public DateTime? FourthStageStartTime { get; set; }
        public string FourthStageTimeFormat => FourthStageStartTime != null ? ElapseTimeCalc.PeriodElapseTimeLower(FourthStageStartTime.Value, DateTime.Now) : string.Empty;

        public DateTime? CompletedTime { get; set; }

        public DateTime? RupturedMembraneTime { get; set; }
        public string RupturedMembraneTimeFormat => RupturedMembraneTime != null ? ElapseTimeCalc.PeriodElapseTimeLower(RupturedMembraneTime.Value, DateTime.Now) : string.Empty;

        /// <summary>
        /// Baby delivery time - marks the moment of baby delivery
        /// Note: ThirdStageStartTime should be set separately for placenta delivery phase
        /// </summary>
        public DateTime? DeliveryTime { get; set; }

        /// <summary>
        /// Updates the CurrentPhase based on the latest cervical dilation measurement
        /// </summary>
        public void UpdatePhaseFromDilation()
        {
            var latestDilation = Dilatations?.OrderByDescending(d => d.Time).FirstOrDefault()?.DilatationCm;
            CurrentPhase = CalculatePhaseFromDilation(latestDilation);
        }

        public int? CervicalDilationOnAdmission { get; set; }
        public string MembraneStatus { get; set; } = "Intact";
        public string LiquorStatus { get; set; } = "Clear";

        // Risk Assessment Summary
        public int RiskScore { get; set; } = 0;
        public string RiskLevel { get; set; } = "Low Risk";
        public string RiskColor { get; set; } = "#4CAF50";

        // Risk Factors
        public IEnumerable<PartographDiagnosis> Diagnoses { get; set; } = Enumerable.Empty<PartographDiagnosis>();

        // Risk Factors
        public IEnumerable<PartographRiskFactor> RiskFactors { get; set; } = Enumerable.Empty<PartographRiskFactor>();
        public string Complications { get; set; } = string.Empty;

        // Partographs
        public List<CompanionEntry> Companions { get; set; } = new();
        public List<Contraction> Contractions { get; set; } = new();
        public List<Caput> Caputs { get; set; } = new();
        public List<BP> BPs { get; set; } = new();
        public List<Assessment> Assessments { get; set; } = new();
        public List<Plan> Plans { get; set; } = new();
        public List<BishopScore> BishopScores { get; set; } = new();
        public List<Moulding> Mouldings { get; set; } = new();
        public List<FetalPosition> FetalPositions { get; set; } = new();
        public List<CervixDilatation> Dilatations { get; set; } = new();
        public List<AmnioticFluid> AmnioticFluids { get; set; } = new();
        public List<HeadDescent> HeadDescents { get; set; } = new();
        public List<FHR> Fhrs { get; set; } = new();
        public List<PainReliefEntry> PainReliefs { get; set; } = new();
        public List<PostureEntry> Postures { get; set; } = new();
        public List<Temperature> Temperatures { get; set; } = new();
        public List<Urine> Urines { get; set; } = new();
        public List<OralFluidEntry> OralFluids { get; set; } = new();
        public List<Oxytocin> Oxytocins { get; set; } = new();
        public List<IVFluidEntry> IVFluids { get; set; } = new();
        public List<MedicationEntry> Medications { get; set; } = new();

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
            LaborStatus.FirstStage => "First Stage",
            LaborStatus.SecondStage => "Second Stage",
            LaborStatus.ThirdStage => "Third Stage",
            LaborStatus.FourthStage => "Fourth Stage",
            LaborStatus.Completed => "Completed",
            LaborStatus.Emergency => "Emergency",
            _ => "Unknown"
        };

        [JsonIgnore]
        public Color StatusColor => Status switch
        {
            LaborStatus.Pending => Color.Orange,
            LaborStatus.FirstStage => Color.FromArgb(255, 255, 193, 7),   // #FFC107 Amber - Active labor
            LaborStatus.SecondStage => Color.FromArgb(255, 255, 152, 0),   // #FF9800 Orange - Delivery imminent
            LaborStatus.ThirdStage => Color.FromArgb(255, 33, 150, 243),  // #2196F3 Blue - Placenta delivery
            LaborStatus.FourthStage => Color.FromArgb(255, 156, 39, 176),  // #9C27B0 Purple - Postpartum monitoring
            LaborStatus.Completed => Color.FromArgb(255, 76, 175, 80),   // #4CAF50 Green - All clear
            LaborStatus.Emergency => Color.FromArgb(255, 244, 67, 54),   // #F44336 Red - Emergency
            _ => Color.Gray
        };

        [JsonIgnore]
        public string StatusColorString => Status switch
        {
            LaborStatus.Pending => "Orange",
            LaborStatus.FirstStage => "#FFC107", //Amber - Active labor
            LaborStatus.SecondStage => "#FF9800",   // Orange - Delivery imminent
            LaborStatus.ThirdStage => "#2196F3",  // Blue - Placenta delivery
            LaborStatus.FourthStage => "#9C27B0",  //  Purple - Postpartum monitoring
            LaborStatus.Completed => "#4CAF50",   //  Green - All clear
            LaborStatus.Emergency => "F44336",   // Red - Emergency
            _ => "Gray"
        };

        //public Color StatusColor => Status switch
        //{
        //    LaborStatus.Pending => Color.Orange,
        //    LaborStatus.FirstStage => Color.FromArgb("FFC107"),  // Amber - Active labor
        //    LaborStatus.SecondStage => Color.FromArgb("#FF9800"), // Orange - Delivery imminent
        //    LaborStatus.ThirdStage => Color.FromArgb("#2196F3"),  // Blue - Placenta delivery
        //    LaborStatus.FourthStage => Color.FromArgb("#9C27B0"), // Purple - Postpartum monitoring
        //    LaborStatus.Completed => Color.FromArgb("#4CAF50"),   // Green - All clear
        //    LaborStatus.Emergency => Color.FromArgb("#F44336"),   // Red - Emergency
        //    _ => Color.Gray
        //};

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

        /// <summary>
        /// Calculates a comprehensive hash of all relevant partograph fields for change detection.
        /// This hash is used during sync to detect if the record has been modified.
        /// </summary>
        public string CalculateHash()
        {
            // Include all fields that should trigger a sync when changed
            var dataBuilder = new StringBuilder();
            dataBuilder.Append($"{ID}|");
            dataBuilder.Append($"{PatientID}|");
            dataBuilder.Append($"{Time:O}|");
            dataBuilder.Append($"{Gravida}|");
            dataBuilder.Append($"{Parity}|");
            dataBuilder.Append($"{Abortion}|");
            dataBuilder.Append($"{AdmissionDate:O}|");
            dataBuilder.Append($"{ExpectedDeliveryDate}|");
            dataBuilder.Append($"{LastMenstrualDate}|");
            dataBuilder.Append($"{(int)Status}|");
            dataBuilder.Append($"{(int)CurrentPhase}|");
            dataBuilder.Append($"{LaborStartTime:O}|");
            dataBuilder.Append($"{SecondStageStartTime:O}|");
            dataBuilder.Append($"{ThirdStageStartTime:O}|");
            dataBuilder.Append($"{FourthStageStartTime:O}|");
            dataBuilder.Append($"{CompletedTime:O}|");
            dataBuilder.Append($"{RupturedMembraneTime:O}|");
            dataBuilder.Append($"{DeliveryTime:O}|");
            dataBuilder.Append($"{CervicalDilationOnAdmission}|");
            dataBuilder.Append($"{MembraneStatus}|");
            dataBuilder.Append($"{LiquorStatus}|");
            dataBuilder.Append($"{RiskScore}|");
            dataBuilder.Append($"{RiskLevel}|");
            dataBuilder.Append($"{Complications}|");
            dataBuilder.Append($"{Handler}|");
            dataBuilder.Append($"{UpdatedTime}");

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataBuilder.ToString()));
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Updates the DataHash property with the current hash value.
        /// Call this before saving the record.
        /// </summary>
        public void UpdateDataHash()
        {
            DataHash = CalculateHash();
        }

    }
}
