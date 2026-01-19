using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummary> GetDashboardSummaryAsync(DashboardFilter? filter = null);
        Task<List<TrendData>> GetDeliveryTrendAsync(DashboardFilter? filter = null);
        Task<DeliveryModeDistribution> GetDeliveryModeDistributionAsync(DashboardFilter? filter = null);
        Task<List<DashboardAlert>> GetActiveAlertsAsync(DashboardFilter? filter = null);
        Task<List<TrendData>> GetComplicationTrendAsync(DashboardFilter? filter = null);
    }

    public interface IRegionService
    {
        Task<List<RegionSummary>> GetAllRegionSummariesAsync();
        Task<RegionSummary?> GetRegionSummaryAsync(Guid regionId);
        Task<DashboardSummary> GetRegionDashboardAsync(Guid regionId);
        Task<List<TrendData>> GetRegionDeliveryTrendAsync(Guid regionId, int days = 30);
    }

    public interface IDistrictService
    {
        Task<List<DistrictSummary>> GetDistrictsByRegionAsync(Guid regionId);
        Task<List<DistrictSummary>> GetAllDistrictSummariesAsync();
        Task<DistrictSummary?> GetDistrictSummaryAsync(Guid districtId);
        Task<DashboardSummary> GetDistrictDashboardAsync(Guid districtId);
        Task<List<TrendData>> GetDistrictDeliveryTrendAsync(Guid districtId, int days = 30);
    }

    public interface IFacilityService
    {
        Task<List<FacilitySummary>> GetFacilitiesByDistrictAsync(Guid districtId);
        Task<List<FacilitySummary>> GetFacilitiesByRegionAsync(Guid regionId);
        Task<List<FacilitySummary>> GetAllFacilitySummariesAsync();
        Task<FacilitySummary?> GetFacilitySummaryAsync(Guid facilityId);
        Task<DashboardSummary> GetFacilityDashboardAsync(Guid facilityId);
        Task<(bool Success, string Message, Guid? FacilityId)> CreateFacilityAsync(FacilityOnboardingRequest request);
    }

    /// <summary>
    /// Request model for facility onboarding
    /// </summary>
    public class FacilityOnboardingRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = "Hospital"; // Hospital, Clinic, Health Center
        public string Level { get; set; } = "Primary"; // Primary, Secondary, Tertiary
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid RegionId { get; set; }
        public Guid DistrictId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? GHPostGPS { get; set; }
    }

    public interface IAnalyticsService
    {
        Task<List<DeliveryOutcomeStats>> GetDeliveryOutcomeStatsAsync(DashboardFilter? filter = null);
        Task<List<ComplicationStats>> GetComplicationStatsAsync(DashboardFilter? filter = null);
        Task<List<ReferralStats>> GetReferralStatsAsync(DashboardFilter? filter = null);
        Task<MortalityStats> GetMortalityStatsAsync(DashboardFilter? filter = null);
        Task<List<FacilityPerformanceData>> GetFacilityPerformanceAsync(DashboardFilter? filter = null);
    }

    // Additional analytics models
    public class DeliveryOutcomeStats
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class ComplicationStats
    {
        public string ComplicationType { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Severity { get; set; } = string.Empty;
    }

    public class ReferralStats
    {
        public string Reason { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Completed { get; set; }
        public int Pending { get; set; }
    }

    public class MortalityStats
    {
        public int MaternalDeaths { get; set; }
        public int NeonatalDeaths { get; set; }
        public int Stillbirths { get; set; }
        public int EarlyNeonatalDeaths { get; set; }
        public int TotalDeliveries { get; set; }
        public double MaternalMortalityRatio { get; set; } // Per 100,000 live births
        public double NeonatalMortalityRate { get; set; } // Per 1,000 live births
        public double StillbirthRate { get; set; } // Per 1,000 total births
    }

    public class FacilityPerformanceData
    {
        public Guid FacilityID { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public int TotalDeliveries { get; set; }
        public double CaesareanRate { get; set; }
        public double ComplicationRate { get; set; }
        public double ReferralRate { get; set; }
        public string PerformanceGrade { get; set; } = "B"; // A, B, C, D, F
    }
}
