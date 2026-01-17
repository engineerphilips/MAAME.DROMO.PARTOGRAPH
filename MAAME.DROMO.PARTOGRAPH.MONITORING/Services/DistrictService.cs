using MAAME.DROMO.PARTOGRAPH.MONITORING.Data;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class DistrictService : IDistrictService
    {
        private readonly MonitoringDbContext _context;

        public DistrictService(MonitoringDbContext context)
        {
            _context = context;
        }

        public async Task<List<DistrictSummary>> GetDistrictsByRegionAsync(Guid regionId)
        {
            var districts = await _context.Districts
                .Include(d => d.Region)
                .Where(d => d.RegionID == regionId)
                .ToListAsync();

            return await GetDistrictSummariesAsync(districts);
        }

        public async Task<List<DistrictSummary>> GetAllDistrictSummariesAsync()
        {
            var districts = await _context.Districts
                .Include(d => d.Region)
                .ToListAsync();

            return await GetDistrictSummariesAsync(districts);
        }

        private async Task<List<DistrictSummary>> GetDistrictSummariesAsync(List<MAAME.DROMO.PARTOGRAPH.MODEL.District> districts)
        {
            var today = DateTime.UtcNow.Date;
            var summaries = new List<DistrictSummary>();

            foreach (var district in districts)
            {
                var facilityIds = await _context.Facilities
                    .Where(f => f.DistrictID == district.ID)
                    .Select(f => f.ID)
                    .ToListAsync();

                var activeFacilities = await _context.Facilities
                    .Where(f => f.DistrictID == district.ID && f.IsActive)
                    .CountAsync();

                var todayStats = await _context.DailyFacilityStats
                    .Where(s => s.Date >= today && facilityIds.Contains(s.FacilityID))
                    .ToListAsync();

                var monthlyStats = await _context.MonthlyFacilityStats
                    .Where(s => s.Year == today.Year && s.Month == today.Month && facilityIds.Contains(s.FacilityID))
                    .ToListAsync();

                var activeLabors = await _context.Partographs
                    .Where(p => facilityIds.Contains(p.FacilityID) &&
                               (p.LaborStatus == "Active" || p.LaborStatus == "InProgress"))
                    .CountAsync();

                var complications = await _context.ComplicationAnalytics
                    .Where(c => c.OccurrenceDateTime >= today && facilityIds.Contains(c.FacilityID))
                    .CountAsync();

                var referrals = await _context.ReferralAnalytics
                    .Where(r => r.ReferralDateTime >= today && facilityIds.Contains(r.SourceFacilityID))
                    .CountAsync();

                var totalDeliveries = monthlyStats.Sum(s => s.TotalDeliveries);
                var caesareanSections = monthlyStats.Sum(s => s.CaesareanSections);

                summaries.Add(new DistrictSummary
                {
                    ID = district.ID,
                    Name = district.Name,
                    Code = district.Code,
                    Type = district.Type,
                    RegionName = district.Region?.Name ?? district.RegionName,
                    FacilityCount = facilityIds.Count,
                    ActiveFacilities = activeFacilities,
                    DeliveriesToday = todayStats.Sum(s => s.TotalDeliveries),
                    DeliveriesThisMonth = totalDeliveries,
                    ActiveLabors = activeLabors,
                    Complications = complications,
                    Referrals = referrals,
                    CaesareanRate = totalDeliveries > 0 ? (double)caesareanSections / totalDeliveries * 100 : 0,
                    PerformanceStatus = DeterminePerformanceStatus(complications, referrals, activeLabors)
                });
            }

            return summaries.OrderBy(d => d.Name).ToList();
        }

        public async Task<DistrictSummary?> GetDistrictSummaryAsync(Guid districtId)
        {
            var district = await _context.Districts
                .Include(d => d.Region)
                .FirstOrDefaultAsync(d => d.ID == districtId);

            if (district == null) return null;

            var summaries = await GetDistrictSummariesAsync(new List<MAAME.DROMO.PARTOGRAPH.MODEL.District> { district });
            return summaries.FirstOrDefault();
        }

        public async Task<DashboardSummary> GetDistrictDashboardAsync(Guid districtId)
        {
            var today = DateTime.UtcNow.Date;

            var facilityIds = await _context.Facilities
                .Where(f => f.DistrictID == districtId)
                .Select(f => f.ID)
                .ToListAsync();

            var summary = new DashboardSummary
            {
                TotalRegions = 1,
                TotalDistricts = 1,
                TotalFacilities = facilityIds.Count,
                ActiveFacilities = await _context.Facilities.Where(f => f.DistrictID == districtId && f.IsActive).CountAsync()
            };

            var todayStats = await _context.DailyFacilityStats
                .Where(s => s.Date >= today && facilityIds.Contains(s.FacilityID))
                .ToListAsync();

            var monthlyStats = await _context.MonthlyFacilityStats
                .Where(s => s.Year == today.Year && s.Month == today.Month && facilityIds.Contains(s.FacilityID))
                .ToListAsync();

            var yearlyStats = await _context.MonthlyFacilityStats
                .Where(s => s.Year == today.Year && facilityIds.Contains(s.FacilityID))
                .ToListAsync();

            summary.TotalDeliveriesToday = todayStats.Sum(s => s.TotalDeliveries);
            summary.TotalDeliveriesThisMonth = monthlyStats.Sum(s => s.TotalDeliveries);
            summary.TotalDeliveriesThisYear = yearlyStats.Sum(s => s.TotalDeliveries);
            summary.NormalDeliveries = todayStats.Sum(s => s.NormalDeliveries);
            summary.CaesareanSections = todayStats.Sum(s => s.CaesareanSections);
            summary.AssistedDeliveries = todayStats.Sum(s => s.AssistedDeliveries);

            summary.ActiveLabors = await _context.Partographs
                .Where(p => facilityIds.Contains(p.FacilityID) &&
                           (p.LaborStatus == "Active" || p.LaborStatus == "InProgress"))
                .CountAsync();

            summary.HighRiskLabors = await _context.Partographs
                .Where(p => facilityIds.Contains(p.FacilityID) &&
                           (p.LaborStatus == "Active" || p.LaborStatus == "InProgress") &&
                           p.IsHighRisk == true)
                .CountAsync();

            summary.ComplicationsToday = await _context.ComplicationAnalytics
                .Where(c => c.OccurrenceDateTime >= today && facilityIds.Contains(c.FacilityID))
                .CountAsync();

            return summary;
        }

        public async Task<List<TrendData>> GetDistrictDeliveryTrendAsync(Guid districtId, int days = 30)
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-days);

            var facilityIds = await _context.Facilities
                .Where(f => f.DistrictID == districtId)
                .Select(f => f.ID)
                .ToListAsync();

            var stats = await _context.DailyFacilityStats
                .Where(s => s.Date >= startDate && s.Date <= endDate && facilityIds.Contains(s.FacilityID))
                .GroupBy(s => s.Date)
                .Select(g => new TrendData
                {
                    Date = g.Key,
                    Label = g.Key.ToString("MMM dd"),
                    Value = g.Sum(s => s.TotalDeliveries)
                })
                .OrderBy(t => t.Date)
                .ToListAsync();

            return stats;
        }

        private string DeterminePerformanceStatus(int complications, int referrals, int activeLabors)
        {
            if (complications > 5 || referrals > 3)
                return "Critical";
            if (complications > 2 || referrals > 1)
                return "Warning";
            return "Normal";
        }
    }
}
