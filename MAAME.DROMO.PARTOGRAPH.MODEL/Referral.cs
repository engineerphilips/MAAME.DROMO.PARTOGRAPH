using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    /// <summary>
    /// Records referral to higher-level healthcare facility
    /// Based on WHO recommendations for referral and transfer of obstetric cases
    /// </summary>
    public class Referral
    {
        public Guid? ID { get; set; }
        public Guid? PartographID { get; set; }
        public DateTime ReferralTime { get; set; } = DateTime.Now;

        // Referral Type
        public ReferralType ReferralType { get; set; } = ReferralType.Maternal;
        public ReferralUrgency Urgency { get; set; } = ReferralUrgency.Routine;

        // Referring Facility
        public string ReferringFacilityName { get; set; } = string.Empty;
        public string ReferringFacilityLevel { get; set; } = string.Empty; // e.g., "Primary Health Center", "District Hospital"
        public string ReferringPhysician { get; set; } = string.Empty;
        public string ReferringPhysicianContact { get; set; } = string.Empty;

        // Destination Facility
        public string DestinationFacilityName { get; set; } = string.Empty;
        public string DestinationFacilityLevel { get; set; } = string.Empty; // e.g., "Tertiary Hospital", "Specialist Center"
        public string DestinationFacilityContact { get; set; } = string.Empty;
        public string DestinationAddress { get; set; } = string.Empty;
        public bool DestinationNotified { get; set; } = false;
        public DateTime? DestinationNotificationTime { get; set; }
        public string DestinationContactPerson { get; set; } = string.Empty;

        // Reason for Referral - Maternal
        public bool ProlongedLabor { get; set; } = false;
        public bool ObstructedLabor { get; set; } = false;
        public bool FoetalDistress { get; set; } = false;
        public bool AntepartumHemorrhage { get; set; } = false;
        public bool PostpartumHemorrhage { get; set; } = false;
        public bool SeverePreeclampsia { get; set; } = false;
        public bool Eclampsia { get; set; } = false;
        public bool SepticShock { get; set; } = false;
        public bool RupturedUterus { get; set; } = false;
        public bool AbnormalPresentation { get; set; } = false;
        public bool CordProlapse { get; set; } = false;
        public bool PlacentaPrevia { get; set; } = false;
        public bool PlacentalAbruption { get; set; } = false;

        // Reason for Referral - Neonatal
        public bool NeonatalAsphyxia { get; set; } = false;
        public bool PrematurityComplications { get; set; } = false;
        public bool LowBirthWeight { get; set; } = false;
        public bool RespiratoryDistress { get; set; } = false;
        public bool CongenitalAbnormalities { get; set; } = false;
        public bool NeonatalSepsis { get; set; } = false;
        public bool BirthInjuries { get; set; } = false;

        // Additional Reasons
        public bool LackOfResources { get; set; } = false;
        public bool RequiresCaesareanSection { get; set; } = false;
        public bool RequiresBloodTransfusion { get; set; } = false;
        public bool RequiresSpecializedCare { get; set; } = false;
        public string OtherReasons { get; set; } = string.Empty;

        // Primary Diagnosis/Indication
        public string PrimaryDiagnosis { get; set; } = string.Empty;
        public string ClinicalSummary { get; set; } = string.Empty;

        // Maternal Condition at Referral
        public string MaternalCondition { get; set; } = string.Empty;
        public int? MaternalPulse { get; set; }
        public int? MaternalBPSystolic { get; set; }
        public int? MaternalBPDiastolic { get; set; }
        public decimal? MaternalTemperature { get; set; }
        public string MaternalConsciousness { get; set; } = "Alert"; // Alert, Drowsy, Unconscious

        // Fetal/Neonatal Condition at Referral
        public int? FetalHeartRate { get; set; }
        public string FetalCondition { get; set; } = string.Empty;
        public int? NumberOfBabiesBeingReferred { get; set; }
        public string NeonatalCondition { get; set; } = string.Empty;

        // Labor Status at Referral
        public int? CervicalDilationAtReferral { get; set; } // in cm
        public bool MembranesRuptured { get; set; } = false;
        public DateTime? MembraneRuptureTime { get; set; }
        public string LiquorColor { get; set; } = "Clear";

        // Interventions Before Referral
        public string InterventionsPerformed { get; set; } = string.Empty;
        public string MedicationsGiven { get; set; } = string.Empty;
        public string IVFluidsGiven { get; set; } = string.Empty;
        public bool BloodSamplesTaken { get; set; } = false;
        public string InvestigationsPerformed { get; set; } = string.Empty;

        // Transport Details
        public TransportMode TransportMode { get; set; } = TransportMode.Ambulance;
        public string TransportDetails { get; set; } = string.Empty;
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public bool SkillfulAttendantAccompanying { get; set; } = true;
        public string AccompanyingStaffName { get; set; } = string.Empty;
        public string AccompanyingStaffDesignation { get; set; } = string.Empty;

        // Equipment/Supplies Sent
        public bool PartographSent { get; set; } = true;
        public bool IVLineInsitu { get; set; } = false;
        public bool CatheterInsitu { get; set; } = false;
        public bool OxygenProvided { get; set; } = false;
        public string EquipmentSent { get; set; } = string.Empty;

        // Referral Outcome
        public ReferralStatus Status { get; set; } = ReferralStatus.Pending;
        public DateTime? AcceptedTime { get; set; }
        public DateTime? CompletedTime { get; set; }
        public string OutcomeNotes { get; set; } = string.Empty;
        public bool FeedbackReceived { get; set; } = false;
        public string FeedbackDetails { get; set; } = string.Empty;

        // Referral Letter/Form
        public string ReferralLetterPath { get; set; } = string.Empty; // Path to PDF
        public bool ReferralFormGenerated { get; set; } = false;
        public DateTime? FormGenerationTime { get; set; }

        // Recording Information
        public string HandlerName { get; set; } = string.Empty;
        public Guid? Handler { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Navigation Property
        [JsonIgnore]
        public Partograph Partograph { get; set; }

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

        [NotMapped]
        [JsonIgnore]
        public bool HasConflict => SyncStatus == 2;

        [NotMapped]
        [JsonIgnore]
        public bool NeedsSync => SyncStatus == 0;

        [NotMapped]
        [JsonIgnore]
        public List<string> ReferralReasons
        {
            get
            {
                var reasons = new List<string>();
                if (ProlongedLabor) reasons.Add("Prolonged Labor");
                if (ObstructedLabor) reasons.Add("Obstructed Labor");
                if (FoetalDistress) reasons.Add("Foetal Distress");
                if (AntepartumHemorrhage) reasons.Add("Antepartum Hemorrhage");
                if (PostpartumHemorrhage) reasons.Add("Postpartum Hemorrhage");
                if (SeverePreeclampsia) reasons.Add("Severe Preeclampsia");
                if (Eclampsia) reasons.Add("Eclampsia");
                if (SepticShock) reasons.Add("Septic Shock");
                if (RupturedUterus) reasons.Add("Ruptured Uterus");
                if (AbnormalPresentation) reasons.Add("Abnormal Presentation");
                if (CordProlapse) reasons.Add("Cord Prolapse");
                if (PlacentaPrevia) reasons.Add("Placenta Previa");
                if (PlacentalAbruption) reasons.Add("Placental Abruption");
                if (NeonatalAsphyxia) reasons.Add("Neonatal Asphyxia");
                if (PrematurityComplications) reasons.Add("Prematurity Complications");
                if (LowBirthWeight) reasons.Add("Low Birth Weight");
                if (RespiratoryDistress) reasons.Add("Respiratory Distress");
                if (CongenitalAbnormalities) reasons.Add("Congenital Abnormalities");
                if (NeonatalSepsis) reasons.Add("Neonatal Sepsis");
                if (BirthInjuries) reasons.Add("Birth Injuries");
                if (LackOfResources) reasons.Add("Lack of Resources");
                if (RequiresCaesareanSection) reasons.Add("Requires Caesarean Section");
                if (RequiresBloodTransfusion) reasons.Add("Requires Blood Transfusion");
                if (RequiresSpecializedCare) reasons.Add("Requires Specialized Care");
                if (!string.IsNullOrEmpty(OtherReasons)) reasons.Add(OtherReasons);
                return reasons;
            }
        }

        public string CalculateHash()
        {
            var data = $"{ID}|{PartographID}|{ReferralTime}|{Status}";
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }

    public enum ReferralType
    {
        Maternal,
        Neonatal,
        MaternalAndNeonatal
    }

    public enum ReferralUrgency
    {
        Emergency, // Immediate life-threatening
        Urgent, // Within 1-2 hours
        SemiUrgent, // Within 24 hours
        Routine // Scheduled referral
    }

    public enum ReferralStatus
    {
        Pending, // Referral initiated
        Accepted, // Destination facility accepted
        InTransit, // Patient being transported
        Arrived, // Patient arrived at destination
        Completed, // Referral completed
        Declined, // Destination facility declined
        Cancelled // Referral cancelled
    }

    public enum TransportMode
    {
        Ambulance,
        PrivateVehicle,
        PublicTransport,
        MotorcycleAmbulance,
        Boat,
        Helicopter,
        Walking,
        Other
    }
}
