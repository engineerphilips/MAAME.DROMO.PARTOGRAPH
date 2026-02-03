namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    /// <summary>
    /// Shift handover report model
    /// </summary>
    public class ShiftHandoverReport
    {
        public string ShiftId { get; set; } = string.Empty;
        public DateTime ShiftStart { get; set; }
        public DateTime ShiftEnd { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public Guid? FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;

        // Summary stats
        public int TotalActivePatients { get; set; }
        public int TotalAlertsGenerated { get; set; }
        public int AlertsAcknowledged { get; set; }
        public int AlertsMissed { get; set; }
        public int MeasurementsCompleted { get; set; }
        public double CompliancePercentage { get; set; }

        // Pending items for next shift
        public List<AlertHistoryRecord> PendingAlerts { get; set; } = new();
        public List<PatientAttentionItem> PatientsRequiringAttention { get; set; } = new();
        public List<OverdueMeasurement> OverdueMeasurements { get; set; } = new();

        // Completed items this shift
        public List<AlertHistoryRecord> ResolvedAlerts { get; set; } = new();
    }
}
