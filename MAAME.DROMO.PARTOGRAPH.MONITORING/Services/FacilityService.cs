using MAAME.DROMO.PARTOGRAPH.MONITORING.Data;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class FacilityService : IFacilityService
    {
        private readonly MonitoringDbContext _context;

        public FacilityService(MonitoringDbContext context)
        {
            _context = context;
        }

        public async Task<List<FacilitySummary>> GetFacilitiesByDistrictAsync(Guid districtId)
        {
            var facilities = await _context.Facilities
                .Include(f => f.District)
                .Where(f => f.DistrictID == districtId)
                .ToListAsync();

            return await GetFacilitySummariesAsync(facilities);
        }

        public async Task<List<FacilitySummary>> GetAllFacilitySummariesAsync()
        {
            var facilities = await _context.Facilities
                .Include(f => f.District)
                .ToListAsync();

            return await GetFacilitySummariesAsync(facilities);
        }

        private async Task<List<FacilitySummary>> GetFacilitySummariesAsync(List<MAAME.DROMO.PARTOGRAPH.MODEL.Facility> facilities)
        {
            var today = DateTime.UtcNow.Date;
            var summaries = new List<FacilitySummary>();

            foreach (var facility in facilities)
            {
                if (!facility.ID.HasValue) continue;

                var facilityId = facility.ID.Value;

                var todayStats = await _context.DailyFacilityStats
                    .FirstOrDefaultAsync(s => s.Date >= today && s.FacilityID == facilityId);

                var monthlyStats = await _context.MonthlyFacilityStats
                    .FirstOrDefaultAsync(s => s.Year == today.Year && s.Month == today.Month && s.FacilityID == facilityId);

                var activeLabors = await _context.Partographs
                    .Where(p => p.FacilityID == facilityId &&
                               (p.LaborStatus == "Active" || p.LaborStatus == "InProgress"))
                    .CountAsync();

                var staffCount = await _context.Staff
                    .Where(s => s.Facility == facilityId || s.FacilityID == facilityId)
                    .CountAsync();

                var lastActivity = await _context.Partographs
                    .Where(p => p.FacilityID == facilityId)
                    .OrderByDescending(p => p.UpdatedTime)
                    .Select(p => p.UpdatedTime)
                    .FirstOrDefaultAsync();

                summaries.Add(new FacilitySummary
                {
                    ID = facilityId,
                    Name = facility.Name,
                    Code = facility.Code,
                    Type = facility.Type,
                    Level = facility.Level,
                    DistrictName = facility.District?.Name ?? facility.DistrictName,
                    IsActive = facility.IsActive,
                    DeliveriesToday = todayStats?.TotalDeliveries ?? 0,
                    DeliveriesThisMonth = monthlyStats?.TotalDeliveries ?? 0,
                    ActiveLabors = activeLabors,
                    StaffCount = staffCount,
                    LastActivityTime = lastActivity > 0 ? DateTimeOffset.FromUnixTimeSeconds(lastActivity).DateTime : null,
                    PerformanceStatus = DeterminePerformanceStatus(facility.IsActive, activeLabors, lastActivity)
                });
            }

            return summaries.OrderBy(f => f.Name).ToList();
        }

        public async Task<FacilitySummary?> GetFacilitySummaryAsync(Guid facilityId)
        {
            var facility = await _context.Facilities
                .Include(f => f.District)
                .FirstOrDefaultAsync(f => f.ID == facilityId);

            if (facility == null) return null;

            var summaries = await GetFacilitySummariesAsync(new List<MAAME.DROMO.PARTOGRAPH.MODEL.Facility> { facility });
            return summaries.FirstOrDefault();
        }

        public async Task<DashboardSummary> GetFacilityDashboardAsync(Guid facilityId)
        {
            var today = DateTime.UtcNow.Date;

            var summary = new DashboardSummary
            {
                TotalRegions = 1,
                TotalDistricts = 1,
                TotalFacilities = 1,
                ActiveFacilities = await _context.Facilities.AnyAsync(f => f.ID == facilityId && f.IsActive) ? 1 : 0
            };

            var todayStats = await _context.DailyFacilityStats
                .FirstOrDefaultAsync(s => s.Date >= today && s.FacilityID == facilityId);

            var monthlyStats = await _context.MonthlyFacilityStats
                .FirstOrDefaultAsync(s => s.Year == today.Year && s.Month == today.Month && s.FacilityID == facilityId);

            var yearlyStats = await _context.MonthlyFacilityStats
                .Where(s => s.Year == today.Year && s.FacilityID == facilityId)
                .ToListAsync();

            summary.TotalDeliveriesToday = todayStats?.TotalDeliveries ?? 0;
            summary.TotalDeliveriesThisMonth = monthlyStats?.TotalDeliveries ?? 0;
            summary.TotalDeliveriesThisYear = yearlyStats.Sum(s => s.TotalDeliveries);
            summary.NormalDeliveries = todayStats?.NormalDeliveries ?? 0;
            summary.CaesareanSections = todayStats?.CaesareanSections ?? 0;
            summary.AssistedDeliveries = todayStats?.AssistedDeliveries ?? 0;

            summary.ActiveLabors = await _context.Partographs
                .Where(p => p.FacilityID == facilityId &&
                           (p.LaborStatus == "Active" || p.LaborStatus == "InProgress"))
                .CountAsync();

            summary.HighRiskLabors = await _context.Partographs
                .Where(p => p.FacilityID == facilityId &&
                           (p.LaborStatus == "Active" || p.LaborStatus == "InProgress") &&
                           p.IsHighRisk == true)
                .CountAsync();

            summary.ComplicationsToday = await _context.ComplicationAnalytics
                .Where(c => c.OccurrenceDateTime >= today && c.FacilityID == facilityId)
                .CountAsync();

            return summary;
        }

        private string DeterminePerformanceStatus(bool isActive, int activeLabors, long lastActivity)
        {
            if (!isActive)
                return "Inactive";

            var lastActivityTime = lastActivity > 0
                ? DateTimeOffset.FromUnixTimeSeconds(lastActivity).DateTime
                : DateTime.MinValue;

            if (lastActivityTime < DateTime.UtcNow.AddDays(-7))
                return "Warning"; // No activity in a week

            return "Normal";
        }
    }
}
