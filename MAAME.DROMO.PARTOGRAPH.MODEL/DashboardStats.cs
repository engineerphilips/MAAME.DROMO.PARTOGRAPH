using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    public class DashboardStats
    {
        public int TotalPatients { get; set; }
        public int PendingLabor { get; set; }
        public int ActiveLabor { get; set; }
        public int CompletedToday { get; set; }
        public int EmergencyCases { get; set; }
        public List<LaborProgressData> RecentProgress { get; set; } = [];

        // Labor Stage Breakdown (WHO Four-Stage System)
        public int FirstStageCount { get; set; }
        public int SecondStageCount { get; set; }
        public int ThirdStageCount { get; set; }
        public int FourthStageCount { get; set; }

        // Phase Breakdown (First Stage phases)
        public int LatentPhaseCount { get; set; }
        public int ActiveEarlyPhaseCount { get; set; }
        public int ActiveAdvancedPhaseCount { get; set; }
        public int TransitionPhaseCount { get; set; }

        // Delivery Statistics
        public int VaginalDeliveriesToday { get; set; }
        public int CsectionDeliveriesToday { get; set; }
        public int AssistedDeliveriesToday { get; set; }

        // Mother & Baby Outcomes
        public int HealthyMothersToday { get; set; }
        public int HealthyBabiestoday { get; set; }
        public int NICUAdmissionsToday { get; set; }

        // Enhanced metrics
        public double AvgDeliveryTime { get; set; } // Average delivery time in hours
        public double AvgActiveLaborTime { get; set; } // Average time in active labor
        public double AvgFirstStageHours { get; set; } // Average first stage duration
        public double AvgSecondStageMinutes { get; set; } // Average second stage duration
        public int ProlongedLaborCount { get; set; } // Patients in labor > 12 hours
        public int HighRiskCount { get; set; } // High-risk patients count
        public int OverdueChecksCount { get; set; } // Patients overdue for checks

        // Quick Stats
        public int TotalDeliveriesToday { get; set; }
        public int AbnormalFHRCount { get; set; } // Patients with abnormal FHR
        public int MeconiumStainedCount { get; set; } // Patients with meconium-stained liquor

        // Critical patients list (top 5 requiring attention)
        public List<CriticalPatientInfo> CriticalPatients { get; set; } = [];

        // Real-time alerts
        public List<PatientAlert> ActiveAlerts { get; set; } = [];

        // Admission trends
        public int AdmissionsToday { get; set; }
        public int AdmissionsThisShift { get; set; }

        // Shift handover data
        public List<ShiftHandoverItem> ShiftHandoverItems { get; set; } = [];

        // WHO compliance metrics
        public WHOComplianceMetrics ComplianceMetrics { get; set; } = new();

        // Recent activity
        public List<RecentActivityItem> RecentActivities { get; set; } = [];

        // Resource utilization
        public ResourceUtilization Resources { get; set; } = new();

        // Labor progress trends (hourly admissions for today)
        public List<HourlyAdmission> AdmissionTrends { get; set; } = [];
    }

    public class LaborProgressData
    {
        public string PatientName { get; set; } = string.Empty;
        public int HoursInLabor { get; set; }
        public int CervicalDilation { get; set; }
        public LaborStatus Status { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public class ShiftHandoverItem
    {
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string HospitalNumber { get; set; } = string.Empty;
        public DateTime AdmissionTime { get; set; }
        public LaborStatus Status { get; set; }
        public string KeyNotes { get; set; } = string.Empty;
        public int HoursInLabor { get; set; }
        public bool RequiresHandover { get; set; }
    }

    public class WHOComplianceMetrics
    {
        public int TotalActiveLabors { get; set; }
        public int AlertLineCrossings { get; set; }
        public int ActionLineCrossings { get; set; }
        public int PartographsCompleted { get; set; }
        public int PartographsMissingData { get; set; }
        public double ComplianceRate { get; set; }
        public int OnTimeAssessments { get; set; }
        public int LateAssessments { get; set; }
    }

    public class RecentActivityItem
    {
        public Guid ActivityId { get; set; }
        public ActivityType Type { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
        public Guid? PatientId { get; set; }
    }

    public class ResourceUtilization
    {
        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int AvailableBeds { get; set; }
        public double OccupancyRate { get; set; }
        public int TotalStaff { get; set; }
        public int ActivePatients { get; set; }
        public double StaffToPatientRatio { get; set; }
    }

    public class HourlyAdmission
    {
        public int Hour { get; set; }
        public string HourLabel { get; set; } = string.Empty;
        public int AdmissionCount { get; set; }
    }

    public enum ActivityType
    {
        Admission,
        Delivery,
        EmergencyEscalation,
        StatusChange,
        Assessment,
        Intervention
    }

    public class CriticalPatientInfo
    {
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string HospitalNumber { get; set; } = string.Empty;
        public string ReasonForConcern { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public int HoursInLabor { get; set; }
        public int? CurrentDilation { get; set; }
        public int? LastFetalHeartRate { get; set; }
        public DateTime LastCheckTime { get; set; }
        public string TimeInLabor { get; set; } = string.Empty;
        public bool IsOverdueCheck { get; set; }
        public LaborStatus Status { get; set; }
    }

    public class PatientAlert
    {
        public Guid AlertId { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string AlertMessage { get; set; } = string.Empty;
        public AlertType Type { get; set; }
        public AlertSeverity Severity { get; set; }
        public DateTime AlertTime { get; set; }
        public bool IsAcknowledged { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }

    public enum AlertType
    {
        FetalHeartRate,
        ProlongedLabor,
        ActionLineCrossed,
        AlertLineCrossed,
        OverdueAssessment,
        AbnormalVitals,
        RupturedMembranes,
        HighRisk,
        Emergency
    }

    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical,
        Emergency
    }
}
