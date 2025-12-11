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

        // Enhanced metrics
        public double AvgDeliveryTime { get; set; } // Average delivery time in hours
        public double AvgActiveLaborTime { get; set; } // Average time in active labor
        public int ProlongedLaborCount { get; set; } // Patients in labor > 12 hours
        public int HighRiskCount { get; set; } // High-risk patients count
        public int OverdueChecksCount { get; set; } // Patients overdue for checks

        // Critical patients list (top 5 requiring attention)
        public List<CriticalPatientInfo> CriticalPatients { get; set; } = [];

        // Real-time alerts
        public List<PatientAlert> ActiveAlerts { get; set; } = [];

        // Admission trends
        public int AdmissionsToday { get; set; }
        public int AdmissionsThisShift { get; set; }
    }

    public class LaborProgressData
    {
        public string PatientName { get; set; } = string.Empty;
        public int HoursInLabor { get; set; }
        public int CervicalDilation { get; set; }
        public LaborStatus Status { get; set; }
        public DateTime LastUpdate { get; set; }
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
