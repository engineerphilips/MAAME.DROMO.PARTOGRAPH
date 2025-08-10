using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Models
{
    // Base class for all partograph measurements
    public abstract class BasePartographMeasurement
    {
        public int ID { get; set; }
        public int PatientID { get; set; }
        public DateTime RecordedTime { get; set; } = DateTime.Now;
        public string RecordedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    // Companion Support
    public class CompanionEntry : BasePartographMeasurement
    {
        public bool HasCompanion { get; set; }
        public string CompanionName { get; set; } = string.Empty;
        public string CompanionRelationship { get; set; } = string.Empty; // Partner, Mother, Sister, etc.
        public bool CompanionPresent { get; set; }
        public string CompanionSupport { get; set; } = string.Empty; // Emotional, Physical, etc.
    }

    // Pain Relief Management
    public class PainReliefEntry : BasePartographMeasurement
    {
        public string PainLevel { get; set; } = "0"; // 0-10 scale
        public string PainReliefMethod { get; set; } = string.Empty; // None, Epidural, Pethidine, Gas & Air, etc.
        public DateTime? AdministeredTime { get; set; }
        public string Dose { get; set; } = string.Empty;
        public string Effectiveness { get; set; } = string.Empty; // Poor, Fair, Good, Excellent
        public bool SideEffects { get; set; }
        public string SideEffectsDescription { get; set; } = string.Empty;
    }

    // Oral Fluid Intake
    public class OralFluidEntry : BasePartographMeasurement
    {
        public string FluidType { get; set; } = string.Empty; // Water, Ice chips, Energy drink, etc.
        public int AmountMl { get; set; }
        public bool Tolerated { get; set; }
        public bool Vomiting { get; set; }
        public string Restrictions { get; set; } = string.Empty;
    }

    // Maternal Posture
    public class PostureEntry : BasePartographMeasurement
    {
        public string Position { get; set; } = string.Empty; // Upright, Left lateral, Right lateral, Supine, etc.
        public bool Mobilizing { get; set; }
        public string MobilityLevel { get; set; } = string.Empty; // Full, Limited, Bed rest
        public bool UsingBirthBall { get; set; }
        public bool UsingBirthPool { get; set; }
        public string ComfortMeasures { get; set; } = string.Empty;
    }

    // Baseline FHR (every 30 minutes)
    public class BaselineFHREntry : BasePartographMeasurement
    {
        public int BaselineRate { get; set; } // 110-160 normal
        public string Variability { get; set; } = string.Empty; // Absent, Minimal, Moderate, Marked
        public bool Accelerations { get; set; }
        public string Pattern { get; set; } = string.Empty; // Reassuring, Non-reassuring, Abnormal
        public string MonitoringMethod { get; set; } = string.Empty; // Intermittent, Continuous CTG, Doppler
    }

    // FHR Deceleration (every 30 minutes)
    public class FHRDecelerationEntry : BasePartographMeasurement
    {
        public bool DecelerationsPresent { get; set; }
        public string DecelerationType { get; set; } = string.Empty; // Early, Late, Variable, Prolonged
        public string Severity { get; set; } = string.Empty; // Mild, Moderate, Severe
        public int Duration { get; set; } // in seconds
        public string Recovery { get; set; } = string.Empty; // Quick, Slow, Poor
        public bool RequiresAction { get; set; }
        public string ActionTaken { get; set; } = string.Empty;
    }

    // Amniotic Fluid
    public class AmnioticFluidEntry : BasePartographMeasurement
    {
        public string Color { get; set; } = "Clear"; // Clear, Straw, Green, Brown, Blood-stained
        public string Consistency { get; set; } = "Normal"; // Normal, Thick, Tenacious
        public string Amount { get; set; } = "Normal"; // Normal, Reduced (oligohydramnios), Excessive (polyhydramnios)
        public string Odor { get; set; } = "None"; // None, Offensive, Fishy
        public bool MeconiumStaining { get; set; }
        public string MeconiumGrade { get; set; } = string.Empty; // Grade I, II, III
    }

    // Fetal Position
    public class FetalPositionEntry : BasePartographMeasurement
    {
        public string Position { get; set; } = string.Empty; // LOA, ROA, LOP, ROP, etc.
        public string Presentation { get; set; } = "Vertex"; // Vertex, Breech, Transverse
        public string Lie { get; set; } = "Longitudinal"; // Longitudinal, Transverse, Oblique
        public bool Engaged { get; set; }
        public string Flexion { get; set; } = "Flexed"; // Flexed, Deflexed, Extended
    }

    // Caput Succedaneum
    public class CaputEntry : BasePartographMeasurement
    {
        public string Degree { get; set; } = "None"; // None, +, ++, +++
        public string Location { get; set; } = string.Empty; // Parietal, Occipital, etc.
        public bool Increasing { get; set; }
        public string Consistency { get; set; } = string.Empty; // Soft, Firm, Hard
    }

    // Moulding
    public class MouldingEntry : BasePartographMeasurement
    {
        public string Degree { get; set; } = "None"; // None, +, ++, +++
        public bool SuturesOverlapping { get; set; }
        public string Location { get; set; } = string.Empty; // Sagittal, Coronal, Lambdoid
        public bool Reducing { get; set; }
    }

    // Enhanced Vital Signs (every hour)
    public class EnhancedVitalSignEntry : BasePartographMeasurement
    {
        public int PulseRate { get; set; }
        public string PulseRhythm { get; set; } = "Regular"; // Regular, Irregular
        public string PulseVolume { get; set; } = "Normal"; // Weak, Normal, Bounding
        public int SystolicBP { get; set; }
        public int DiastolicBP { get; set; }
        public string BPPosition { get; set; } = "Left arm"; // Left arm, Right arm
        public decimal Temperature { get; set; }
        public string TemperatureRoute { get; set; } = "Oral"; // Oral, Axillary, Tympanic
        public int RespiratoryRate { get; set; }
        public string RespiratoryPattern { get; set; } = "Normal"; // Normal, Shallow, Deep, Irregular
    }

    // Urine Output (every hour)
    public class UrineEntry : BasePartographMeasurement
    {
        public int OutputMl { get; set; }
        public string Color { get; set; } = "Yellow"; // Clear, Yellow, Dark yellow, Brown
        public string Protein { get; set; } = "Nil"; // Nil, Trace, +, ++, +++
        public string Glucose { get; set; } = "Nil"; // Nil, Trace, +, ++, +++
        public string Ketones { get; set; } = "Nil"; // Nil, Trace, +, ++, +++
        public string SpecificGravity { get; set; } = string.Empty;
        public bool Catheterized { get; set; }
        public DateTime? LastVoided { get; set; }
    }

    // Contractions (every 30 minutes)
    public class ContractionEntry : BasePartographMeasurement
    {
        public int FrequencyPer10Min { get; set; }
        public int DurationSeconds { get; set; }
        public string Strength { get; set; } = "Moderate"; // Mild, Moderate, Strong
        public string Regularity { get; set; } = "Regular"; // Regular, Irregular
        public bool PalpableAtRest { get; set; }
        public string EffectOnCervix { get; set; } = string.Empty; // Effacing, Dilating, No change
        public bool Coordinated { get; set; }
    }

    // Cervical Dilatation
    public class CervixDilatationEntry : BasePartographMeasurement
    {
        public int DilatationCm { get; set; }
        public int EffacementPercent { get; set; }
        public string Consistency { get; set; } = "Soft"; // Firm, Soft, Very soft
        public string Position { get; set; } = "Central"; // Posterior, Central, Anterior
        public bool ApplicationToHead { get; set; }
        public string CervicalEdema { get; set; } = "None"; // None, Slight, Moderate, Marked
    }

    // Head Descent
    public class HeadDescentEntry : BasePartographMeasurement
    {
        public string Station { get; set; } = "0"; // -3 to +3
        public bool Engaged { get; set; }
        public string Synclitism { get; set; } = "Normal"; // Normal, Asynclitic anterior, Asynclitic posterior
        public string Flexion { get; set; } = "Flexed"; // Extended, Deflexed, Flexed, Well flexed
        public bool VisibleAtIntroitus { get; set; }
        public bool Crowning { get; set; }
    }

    // Oxytocin Administration
    public class OxytocinEntry : BasePartographMeasurement
    {
        public bool InUse { get; set; }
        public decimal DoseMUnitsPerMin { get; set; }
        public decimal TotalVolumeInfused { get; set; }
        public DateTime? StartTime { get; set; }
        public string Indication { get; set; } = string.Empty; // Induction, Augmentation
        public bool Contraindications { get; set; }
        public string ContraindicationDetails { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty; // Good, Poor, Hyperstimulation
    }

    // Medication Administration
    public class MedicationEntry : BasePartographMeasurement
    {
        public string MedicationName { get; set; } = string.Empty;
        public string Dose { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty; // IV, IM, PO, Sublingual, etc.
        public DateTime AdministrationTime { get; set; } = DateTime.Now;
        public string Indication { get; set; } = string.Empty;
        public string PrescribedBy { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public bool AdverseReaction { get; set; }
        public string AdverseReactionDetails { get; set; } = string.Empty;
    }

    // IV Fluid Management
    public class IVFluidEntry : BasePartographMeasurement
    {
        public string FluidType { get; set; } = string.Empty; // Normal saline, Hartmann's, Dextrose, etc.
        public int VolumeInfused { get; set; }
        public string Rate { get; set; } = string.Empty; // ml/hr
        public DateTime? StartTime { get; set; }
        public string Additives { get; set; } = string.Empty; // KCl, Syntocinon, etc.
        public string IVSite { get; set; } = string.Empty; // Left hand, Right forearm, etc.
        public bool SiteHealthy { get; set; }
        public string SiteCondition { get; set; } = string.Empty; // Clean, Inflamed, Swollen, etc.
    }

    // Assessment and Plan
    public class AssessmentPlanEntry : BasePartographMeasurement
    {
        public string LaborProgress { get; set; } = string.Empty; // Normal, Delayed, Rapid
        public string FetalWellbeing { get; set; } = string.Empty; // Satisfactory, Concerning, Compromised
        public string MaternalCondition { get; set; } = string.Empty; // Stable, Concerned, Critical
        public string RiskFactors { get; set; } = string.Empty;
        public string Complications { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string ExpectedDelivery { get; set; } = string.Empty; // Normal vaginal, Instrumental, Cesarean
        public bool RequiresIntervention { get; set; }
        public string InterventionRequired { get; set; } = string.Empty;
        public DateTime? NextAssessment { get; set; }
        public string AssessedBy { get; set; } = string.Empty;
    }

    // Measurement Schedule Helper
    public static class MeasurementSchedule
    {
        public static readonly Dictionary<Type, TimeSpan> ScheduleIntervals = new()
        {
            { typeof(BaselineFHREntry), TimeSpan.FromMinutes(30) },
            { typeof(FHRDecelerationEntry), TimeSpan.FromMinutes(30) },
            { typeof(ContractionEntry), TimeSpan.FromMinutes(30) },
            { typeof(EnhancedVitalSignEntry), TimeSpan.FromHours(1) },
            { typeof(UrineEntry), TimeSpan.FromHours(1) },
            { typeof(CervixDilatationEntry), TimeSpan.FromHours(4) }, // Or as needed
            { typeof(HeadDescentEntry), TimeSpan.FromHours(4) }, // Or as needed
            { typeof(AssessmentPlanEntry), TimeSpan.FromHours(4) }
        };

        public static bool IsDue(Type measurementType, DateTime lastRecorded)
        {
            if (!ScheduleIntervals.TryGetValue(measurementType, out var interval))
                return false;

            return DateTime.Now - lastRecorded >= interval;
        }

        public static DateTime GetNextDueTime(Type measurementType, DateTime lastRecorded)
        {
            if (!ScheduleIntervals.TryGetValue(measurementType, out var interval))
                return DateTime.Now;

            return lastRecorded.Add(interval);
        }
    }
}
