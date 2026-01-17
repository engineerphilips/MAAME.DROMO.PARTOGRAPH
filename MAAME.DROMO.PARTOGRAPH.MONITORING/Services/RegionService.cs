using MAAME.DROMO.PARTOGRAPH.MONITORING.Data;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class RegionService : IRegionService
    {
        private readonly MonitoringDbContext _context;

        public RegionService(MonitoringDbContext context)
        {
            _context = context;
        }

        public async Task<List<RegionSummary>> GetAllRegionSummariesAsync()
        {
            var regions = await _context.Regions
                .Include(r => r.Districts)
                .ToListAsync();

            var today = DateTime.UtcNow.Date;
            var summaries = new List<RegionSummary>();

            foreach (var region in regions)
            {
                var facilityIds = await _context.Facilities
                    .Where(f => f.RegionID == region.ID)
                    .Select(f => f.ID)
                    .ToListAsync();

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

                summaries.Add(new RegionSummary
                {
                    ID = region.ID,
                    Name = region.Name,
                    Code = region.Code,
                    DistrictCount = region.Districts?.Count ?? 0,
                    FacilityCount = facilityIds.Count,
                    DeliveriesToday = todayStats.Sum(s => s.TotalDeliveries),
                    DeliveriesThisMonth = totalDeliveries,
                    ActiveLabors = activeLabors,
                    Complications = complications,
                    Referrals = referrals,
                    CaesareanRate = totalDeliveries > 0 ? (double)caesareanSections / totalDeliveries * 100 : 0,
                    PerformanceStatus = DeterminePerformanceStatus(complications, referrals, activeLabors)
                });
            }

            return summaries.OrderBy(r => r.Name).ToList();
        }

        public async Task<RegionSummary?> GetRegionSummaryAsync(Guid regionId)
        {
            var region = await _context.Regions
                .Include(r => r.Districts)
                .FirstOrDefaultAsync(r => r.ID == regionId);

            if (region == null) return null;

            var today = DateTime.UtcNow.Date;
            var facilityIds = await _context.Facilities
                .Where(f => f.RegionID == regionId)
                .Select(f => f.ID)
                .ToListAsync();

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

            return new RegionSummary
            {
                ID = region.ID,
                Name = region.Name,
                Code = region.Code,
                DistrictCount = region.Districts?.Count ?? 0,
                FacilityCount = facilityIds.Count,
                DeliveriesToday = todayStats.Sum(s => s.TotalDeliveries),
                DeliveriesThisMonth = totalDeliveries,
                ActiveLabors = activeLabors,
                Complications = complications,
                Referrals = referrals,
                CaesareanRate = totalDeliveries > 0 ? (double)caesareanSections / totalDeliveries * 100 : 0,
                PerformanceStatus = DeterminePerformanceStatus(complications, referrals, activeLabors)
            };
        }

        public async Task<DashboardSummary> GetRegionDashboardAsync(Guid regionId)
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var facilityIds = await _context.Facilities
                .Where(f => f.RegionID == regionId)
                .Select(f => f.ID)
                .ToListAsync();

            var summary = new DashboardSummary
            {
                TotalRegions = 1,
                TotalDistricts = await _context.Districts.Where(d => d.RegionID == regionId).CountAsync(),
                TotalFacilities = facilityIds.Count,
                ActiveFacilities = await _context.Facilities.Where(f => f.RegionID == regionId && f.IsActive).CountAsync()
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

        public async Task<List<TrendData>> GetRegionDeliveryTrendAsync(Guid regionId, int days = 30)
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-days);

            var facilityIds = await _context.Facilities
                .Where(f => f.RegionID == regionId)
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
            // Simple heuristic - can be refined based on actual requirements
            if (complications > 10 || referrals > 5)
                return "Critical";
            if (complications > 5 || referrals > 2)
                return "Warning";
            return "Normal";
        }
    }
}
