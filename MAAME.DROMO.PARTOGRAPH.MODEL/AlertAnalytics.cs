namespace MAAME.DROMO.PARTOGRAPH.MODEL
{
    /// <summary>
    /// Analytics model for alert compliance tracking
    /// </summary>
    public class AlertAnalytics
    {
        public int TotalAlerts { get; set; }
        public int AcknowledgedAlerts { get; set; }
        public int MissedAlerts { get; set; }
        public int EscalatedAlerts { get; set; }
        public double AverageResponseTimeMinutes { get; set; }
        public double CompliancePercentage { get; set; }
        public Dictionary<string, int> AlertsByType { get; set; } = new();
        public Dictionary<string, int> AlertsBySeverity { get; set; } = new();
        public Dictionary<string, double> ComplianceByMeasurementType { get; set; } = new();
        public List<HourlyAlertCount> AlertsByHour { get; set; } = new();
        public List<AlertHistoryRecord> RecentAlerts { get; set; } = new();

        // Alias properties for ViewModel compatibility
        public int AcknowledgedCount => AcknowledgedAlerts;
        public int MissedCount => MissedAlerts;
        public Dictionary<string, int> ByType => AlertsByType;
        public Dictionary<string, int> BySeverity => AlertsBySeverity;
        public Dictionary<int, int> ByHour => AlertsByHour.ToDictionary(h => h.Hour, h => h.Count);
    }
}
