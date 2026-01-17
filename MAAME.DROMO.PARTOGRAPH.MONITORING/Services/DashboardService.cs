using MAAME.DROMO.PARTOGRAPH.MONITORING.Data;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly MonitoringDbContext _context;

        public DashboardService(MonitoringDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummary> GetDashboardSummaryAsync(DashboardFilter? filter = null)
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var yearStart = new DateTime(today.Year, 1, 1);

            var todayUnix = new DateTimeOffset(today).ToUnixTimeSeconds();
            var monthStartUnix = new DateTimeOffset(monthStart).ToUnixTimeSeconds();
            var yearStartUnix = new DateTimeOffset(yearStart).ToUnixTimeSeconds();

            var summary = new DashboardSummary
            {
                TotalRegions = await _context.Regions.CountAsync(),
                TotalDistricts = await _context.Districts.CountAsync(),
                TotalFacilities = await _context.Facilities.CountAsync(),
                ActiveFacilities = await _context.Facilities.Where(f => f.IsActive).CountAsync()
            };

            // Build facility query based on filter
            IQueryable<MAAME.DROMO.PARTOGRAPH.MODEL.Facility> facilityQuery = _context.Facilities;
            if (filter?.RegionID.HasValue == true)
            {
                facilityQuery = facilityQuery.Where(f => f.RegionID == filter.RegionID);
            }
            if (filter?.DistrictID.HasValue == true)
            {
                facilityQuery = facilityQuery.Where(f => f.DistrictID == filter.DistrictID);
            }

            var facilityIds = await facilityQuery.Select(f => f.ID).ToListAsync();

            // Get delivery statistics from DailyFacilityStats
            var todayStats = await _context.DailyFacilityStats
                .Where(s => s.Date >= today && facilityIds.Contains(s.FacilityID))
                .ToListAsync();

            summary.TotalDeliveriesToday = todayStats.Sum(s => s.TotalDeliveries);
            summary.NormalDeliveries = todayStats.Sum(s => s.NormalDeliveries);
            summary.CaesareanSections = todayStats.Sum(s => s.CaesareanSections);
            summary.AssistedDeliveries = todayStats.Sum(s => s.AssistedDeliveries);

            // Monthly statistics
            var monthlyStats = await _context.MonthlyFacilityStats
                .Where(s => s.Year == today.Year && s.Month == today.Month && facilityIds.Contains(s.FacilityID))
                .ToListAsync();

            summary.TotalDeliveriesThisMonth = monthlyStats.Sum(s => s.TotalDeliveries);

            // Year statistics
            var yearlyStats = await _context.MonthlyFacilityStats
                .Where(s => s.Year == today.Year && facilityIds.Contains(s.FacilityID))
                .ToListAsync();

            summary.TotalDeliveriesThisYear = yearlyStats.Sum(s => s.TotalDeliveries);

            // Active labors (partographs with LaborStatus = Active or InProgress)
            summary.ActiveLabors = await _context.Partographs
                .Where(p => facilityIds.Contains(p.FacilityID) &&
                           (p.LaborStatus == "Active" || p.LaborStatus == "InProgress"))
                .CountAsync();

            // High risk labors
            summary.HighRiskLabors = await _context.Partographs
                .Where(p => facilityIds.Contains(p.FacilityID) &&
                           (p.LaborStatus == "Active" || p.LaborStatus == "InProgress") &&
                           p.IsHighRisk == true)
                .CountAsync();

            // Complications today
            summary.ComplicationsToday = await _context.ComplicationAnalytics
                .Where(c => c.OccurrenceDateTime >= today &&
                           facilityIds.Contains(c.FacilityID))
                .CountAsync();

            // Referrals today
            summary.ReferralsToday = await _context.Referrals
                .Where(r => r.ReferralTime >= todayUnix &&
                           facilityIds.Contains(r.SourceFacilityID ?? Guid.Empty))
                .CountAsync();

            // Mortality this month
            summary.MaternalDeaths = await _context.MaternalMortalityRecords
                .Where(m => m.Year == today.Year && m.Month == today.Month &&
                           facilityIds.Contains(m.FacilityID))
                .CountAsync();

            summary.NeonatalDeaths = await _context.NeonatalOutcomeRecords
                .Where(n => n.Year == today.Year && n.Month == today.Month &&
                           n.OutcomeType == "Death" &&
                           facilityIds.Contains(n.FacilityID))
                .CountAsync();

            summary.Stillbirths = await _context.BirthOutcomes
                .Where(b => b.RecordedTime >= monthStartUnix &&
                           b.FetalStatus == "Stillbirth")
                .CountAsync();

            return summary;
        }

        public async Task<List<TrendData>> GetDeliveryTrendAsync(DashboardFilter? filter = null)
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-30);

            var query = _context.DailyFacilityStats
                .Where(s => s.Date >= startDate && s.Date <= endDate);

            if (filter?.RegionID.HasValue == true)
            {
                var facilityIds = await _context.Facilities
                    .Where(f => f.RegionID == filter.RegionID)
                    .Select(f => f.ID)
                    .ToListAsync();
                query = query.Where(s => facilityIds.Contains(s.FacilityID));
            }

            if (filter?.DistrictID.HasValue == true)
            {
                var facilityIds = await _context.Facilities
                    .Where(f => f.DistrictID == filter.DistrictID)
                    .Select(f => f.ID)
                    .ToListAsync();
                query = query.Where(s => facilityIds.Contains(s.FacilityID));
            }

            var stats = await query
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

        public async Task<DeliveryModeDistribution> GetDeliveryModeDistributionAsync(DashboardFilter? filter = null)
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var query = _context.MonthlyFacilityStats
                .Where(s => s.Year == today.Year && s.Month == today.Month);

            if (filter?.RegionID.HasValue == true)
            {
                var facilityIds = await _context.Facilities
                    .Where(f => f.RegionID == filter.RegionID)
                    .Select(f => f.ID)
                    .ToListAsync();
                query = query.Where(s => facilityIds.Contains(s.FacilityID));
            }

            if (filter?.DistrictID.HasValue == true)
            {
                var facilityIds = await _context.Facilities
                    .Where(f => f.DistrictID == filter.DistrictID)
                    .Select(f => f.ID)
                    .ToListAsync();
                query = query.Where(s => facilityIds.Contains(s.FacilityID));
            }

            var stats = await query.ToListAsync();

            return new DeliveryModeDistribution
            {
                NormalVaginal = stats.Sum(s => s.NormalDeliveries),
                AssistedVaginal = stats.Sum(s => s.AssistedDeliveries),
                ElectiveCaesarean = stats.Sum(s => s.ElectiveCaesareans),
                EmergencyCaesarean = stats.Sum(s => s.EmergencyCaesareans)
            };
        }

        public async Task<List<DashboardAlert>> GetActiveAlertsAsync(DashboardFilter? filter = null)
        {
            var query = _context.AlertSummaries
                .Where(a => !a.Resolved)
                .OrderByDescending(a => a.AlertDateTime);

            if (filter?.RegionID.HasValue == true)
            {
                var facilityIds = await _context.Facilities
                    .Where(f => f.RegionID == filter.RegionID)
                    .Select(f => f.ID)
                    .ToListAsync();
                query = (IOrderedQueryable<MAAME.DROMO.PARTOGRAPH.MODEL.AlertSummary>)query.Where(a => facilityIds.Contains(a.FacilityID));
            }

            if (filter?.DistrictID.HasValue == true)
            {
                var facilityIds = await _context.Facilities
                    .Where(f => f.DistrictID == filter.DistrictID)
                    .Select(f => f.ID)
                    .ToListAsync();
                query = (IOrderedQueryable<MAAME.DROMO.PARTOGRAPH.MODEL.AlertSummary>)query.Where(a => facilityIds.Contains(a.FacilityID));
            }

            var alerts = await query.Take(50).ToListAsync();

            return alerts.Select(a => new DashboardAlert
            {
                ID = a.ID,
                Title = a.AlertType ?? "Alert",
                Message = a.Description ?? "",
                Severity = a.AlertSeverity ?? "Info",
                Category = a.AlertType ?? "General",
                FacilityName = a.FacilityName ?? "",
                CreatedAt = a.AlertDateTime,
                IsResolved = a.Resolved
            }).ToList();
        }

        public async Task<List<TrendData>> GetComplicationTrendAsync(DashboardFilter? filter = null)
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-30);

            var query = _context.ComplicationAnalytics
                .Where(c => c.OccurrenceDateTime >= startDate && c.OccurrenceDateTime <= endDate);

            if (filter?.RegionID.HasValue == true)
            {
                var facilityIds = await _context.Facilities
                    .Where(f => f.RegionID == filter.RegionID)
                    .Select(f => f.ID)
                    .ToListAsync();
                query = query.Where(c => facilityIds.Contains(c.FacilityID));
            }

            if (filter?.DistrictID.HasValue == true)
            {
                var facilityIds = await _context.Facilities
                    .Where(f => f.DistrictID == filter.DistrictID)
                    .Select(f => f.ID)
                    .ToListAsync();
                query = query.Where(c => facilityIds.Contains(c.FacilityID));
            }

            var stats = await query
                .GroupBy(c => c.OccurrenceDateTime.Date)
                .Select(g => new TrendData
                {
                    Date = g.Key,
                    Label = g.Key.ToString("MMM dd"),
                    Value = g.Count()
                })
                .OrderBy(t => t.Date)
                .ToListAsync();

            return stats;
        }
    }
}
