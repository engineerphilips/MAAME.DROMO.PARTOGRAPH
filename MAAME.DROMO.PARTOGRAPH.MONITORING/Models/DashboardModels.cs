namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Models
{
    /// <summary>
    /// Summary statistics for dashboard display
    /// </summary>
    public class DashboardSummary
    {
        public int TotalRegions { get; set; }
        public int TotalDistricts { get; set; }
        public int TotalFacilities { get; set; }
        public int ActiveFacilities { get; set; }

        // Delivery statistics
        public int TotalDeliveriesToday { get; set; }
        public int TotalDeliveriesThisMonth { get; set; }
        public int TotalDeliveriesThisYear { get; set; }

        // Active labor monitoring
        public int ActiveLabors { get; set; }
        public int HighRiskLabors { get; set; }

        // Outcomes
        public int NormalDeliveries { get; set; }
        public int CaesareanSections { get; set; }
        public int AssistedDeliveries { get; set; }

        // Complications
        public int ComplicationsToday { get; set; }
        public int ReferralsToday { get; set; }

        // Mortality (This month)
        public int MaternalDeaths { get; set; }
        public int NeonatalDeaths { get; set; }
        public int Stillbirths { get; set; }
    }

    /// <summary>
    /// Region summary for national dashboard
    /// </summary>
    public class RegionSummary
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int DistrictCount { get; set; }
        public int FacilityCount { get; set; }
        public int DeliveriesToday { get; set; }
        public int DeliveriesThisMonth { get; set; }
        public int ActiveLabors { get; set; }
        public int Complications { get; set; }
        public int Referrals { get; set; }
        public double CaesareanRate { get; set; }
        public string PerformanceStatus { get; set; } = "Normal"; // Normal, Warning, Critical
    }

    /// <summary>
    /// District summary for regional dashboard
    /// </summary>
    public class DistrictSummary
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public int FacilityCount { get; set; }
        public int ActiveFacilities { get; set; }
        public int DeliveriesToday { get; set; }
        public int DeliveriesThisMonth { get; set; }
        public int ActiveLabors { get; set; }
        public int Complications { get; set; }
        public int Referrals { get; set; }
        public double CaesareanRate { get; set; }
        public string PerformanceStatus { get; set; } = "Normal";
    }

    /// <summary>
    /// Facility summary for district dashboard
    /// </summary>
    public class FacilitySummary
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int DeliveriesToday { get; set; }
        public int DeliveriesThisMonth { get; set; }
        public int ActiveLabors { get; set; }
        public int StaffCount { get; set; }
        public DateTime? LastActivityTime { get; set; }
        public string PerformanceStatus { get; set; } = "Normal";
    }

    /// <summary>
    /// Trend data for charts
    /// </summary>
    public class TrendData
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Delivery mode distribution
    /// </summary>
    public class DeliveryModeDistribution
    {
        public int NormalVaginal { get; set; }
        public int AssistedVaginal { get; set; }
        public int ElectiveCaesarean { get; set; }
        public int EmergencyCaesarean { get; set; }
        public int Total => NormalVaginal + AssistedVaginal + ElectiveCaesarean + EmergencyCaesarean;

        public double NormalVaginalPercent => Total > 0 ? (double)NormalVaginal / Total * 100 : 0;
        public double CaesareanPercent => Total > 0 ? (double)(ElectiveCaesarean + EmergencyCaesarean) / Total * 100 : 0;
    }

    /// <summary>
    /// Alert for dashboard
    /// </summary>
    public class DashboardAlert
    {
        public Guid ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Info"; // Info, Warning, Critical
        public string Category { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }

    /// <summary>
    /// Filter options for data queries
    /// </summary>
    public class DashboardFilter
    {
        public Guid? RegionID { get; set; }
        public Guid? DistrictID { get; set; }
        public Guid? FacilityID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Period { get; set; } // Today, Week, Month, Year, Custom
    }
}
