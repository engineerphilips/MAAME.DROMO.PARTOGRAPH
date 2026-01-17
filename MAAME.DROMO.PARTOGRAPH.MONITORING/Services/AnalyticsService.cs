using MAAME.DROMO.PARTOGRAPH.MONITORING.Data;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly MonitoringDbContext _context;

        public AnalyticsService(MonitoringDbContext context)
        {
            _context = context;
        }

        public async Task<List<DeliveryOutcomeStats>> GetDeliveryOutcomeStatsAsync(DashboardFilter? filter = null)
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var query = _context.DeliveryOutcomeSummaries
                .Where(d => d.DeliveryTime >= monthStart);

            if (filter?.RegionID.HasValue == true)
            {
                var facilityIds = await _context.Facilities
                    .Where(f => f.RegionID == filter.RegionID)
                    .Select(f => f.ID)
                    .ToListAsync();
                query = query.Where(d => facilityIds.Contains(d.FacilityID));
            }

            if (filter?.DistrictID.HasValue == true)
            {
                var facilityIds = await _context.Facilities
                    .Where(f => f.DistrictID == filter.DistrictID)
                    .Select(f => f.ID)
                    .ToListAsync();
                query = query.Where(d => facilityIds.Contains(d.FacilityID));
            }

            var outcomes = await query
                .GroupBy(d => d.DeliveryMode)
                .Select(g => new { Mode = g.Key, Count = g.Count() })
                .ToListAsync();

            var total = outcomes.Sum(o => o.Count);

            return outcomes.Select(o => new DeliveryOutcomeStats
            {
                Category = o.Mode ?? "Unknown",
                Count = o.Count,
                Percentage = total > 0 ? (double)o.Count / total * 100 : 0
            }).ToList();
        }

        public async Task<List<ComplicationStats>> GetComplicationStatsAsync(DashboardFilter? filter = null)
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var query = _context.ComplicationAnalytics
                .Where(c => c.OccurrenceDateTime >= monthStart);

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

            var complications = await query
                .GroupBy(c => new { c.ComplicationType, c.Severity })
                .Select(g => new ComplicationStats
                {
                    ComplicationType = g.Key.ComplicationType ?? "Unknown",
                    Severity = g.Key.Severity ?? "Unknown",
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Count)
                .ToListAsync();

            return complications;
        }

        public async Task<List<ReferralStats>> GetReferralStatsAsync(DashboardFilter? filter = null)
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthStartUnix = new DateTimeOffset(monthStart).ToUnixTimeSeconds();

            var query = _context.Referrals
                .Where(r => r.ReferralTime >= monthStartUnix);

            if (filter?.RegionID.HasValue == true)
            {
                var facilityIds = await _context.Facilities
                    .Where(f => f.RegionID == filter.RegionID)
                    .Select(f => f.ID)
                    .ToListAsync();
                query = query.Where(r => facilityIds.Contains(r.SourceFacilityID ?? Guid.Empty));
            }

            if (filter?.DistrictID.HasValue == true)
            {
                var facilityIds = await _context.Facilities
                    .Where(f => f.DistrictID == filter.DistrictID)
                    .Select(f => f.ID)
                    .ToListAsync();
                query = query.Where(r => facilityIds.Contains(r.SourceFacilityID ?? Guid.Empty));
            }

            var referrals = await query
                .GroupBy(r => r.PrimaryReason)
                .Select(g => new
                {
                    Reason = g.Key,
                    Count = g.Count(),
                    Completed = g.Count(r => r.Status == "Completed"),
                    Pending = g.Count(r => r.Status == "Pending" || r.Status == "InTransit")
                })
                .ToListAsync();

            return referrals.Select(r => new ReferralStats
            {
                Reason = r.Reason ?? "Unspecified",
                Count = r.Count,
                Completed = r.Completed,
                Pending = r.Pending
            }).OrderByDescending(r => r.Count).ToList();
        }

        public async Task<MortalityStats> GetMortalityStatsAsync(DashboardFilter? filter = null)
        {
            var today = DateTime.UtcNow.Date;
            var yearStart = new DateTime(today.Year, 1, 1);

            IQueryable<Guid?> facilityIdsQuery = null;

            if (filter?.RegionID.HasValue == true)
            {
                facilityIdsQuery = _context.Facilities
                    .Where(f => f.RegionID == filter.RegionID)
                    .Select(f => f.ID);
            }
            else if (filter?.DistrictID.HasValue == true)
            {
                facilityIdsQuery = _context.Facilities
                    .Where(f => f.DistrictID == filter.DistrictID)
                    .Select(f => f.ID);
            }

            var maternalDeathsQuery = _context.MaternalMortalityRecords
                .Where(m => m.Year == today.Year);

            var neonatalOutcomesQuery = _context.NeonatalOutcomeRecords
                .Where(n => n.Year == today.Year);

            if (facilityIdsQuery != null)
            {
                var facilityIds = await facilityIdsQuery.ToListAsync();
                maternalDeathsQuery = maternalDeathsQuery.Where(m => facilityIds.Contains(m.FacilityID));
                neonatalOutcomesQuery = neonatalOutcomesQuery.Where(n => facilityIds.Contains(n.FacilityID));
            }

            var maternalDeaths = await maternalDeathsQuery.CountAsync();
            var neonatalDeaths = await neonatalOutcomesQuery.CountAsync(n => n.OutcomeType == "Death");
            var earlyNeonatalDeaths = await neonatalOutcomesQuery.CountAsync(n => n.OutcomeType == "EarlyNeonatalDeath");
            var stillbirths = await neonatalOutcomesQuery.CountAsync(n => n.OutcomeType == "Stillbirth");

            // Get total deliveries for rate calculation
            var monthlyStatsQuery = _context.MonthlyFacilityStats
                .Where(m => m.Year == today.Year);

            if (facilityIdsQuery != null)
            {
                var facilityIds = await facilityIdsQuery.ToListAsync();
                monthlyStatsQuery = monthlyStatsQuery.Where(m => facilityIds.Contains(m.FacilityID));
            }

            var totalDeliveries = await monthlyStatsQuery.SumAsync(m => m.TotalDeliveries);
            var liveBirths = totalDeliveries - stillbirths;

            return new MortalityStats
            {
                MaternalDeaths = maternalDeaths,
                NeonatalDeaths = neonatalDeaths,
                EarlyNeonatalDeaths = earlyNeonatalDeaths,
                Stillbirths = stillbirths,
                TotalDeliveries = totalDeliveries,
                MaternalMortalityRatio = liveBirths > 0 ? (double)maternalDeaths / liveBirths * 100000 : 0,
                NeonatalMortalityRate = liveBirths > 0 ? (double)neonatalDeaths / liveBirths * 1000 : 0,
                StillbirthRate = totalDeliveries > 0 ? (double)stillbirths / totalDeliveries * 1000 : 0
            };
        }

        public async Task<List<FacilityPerformanceData>> GetFacilityPerformanceAsync(DashboardFilter? filter = null)
        {
            var today = DateTime.UtcNow.Date;

            var facilitiesQuery = _context.Facilities.AsQueryable();

            if (filter?.RegionID.HasValue == true)
            {
                facilitiesQuery = facilitiesQuery.Where(f => f.RegionID == filter.RegionID);
            }

            if (filter?.DistrictID.HasValue == true)
            {
                facilitiesQuery = facilitiesQuery.Where(f => f.DistrictID == filter.DistrictID);
            }

            var facilities = await facilitiesQuery
                .Include(f => f.District)
                .ToListAsync();

            var performances = new List<FacilityPerformanceData>();

            foreach (var facility in facilities)
            {
                if (!facility.ID.HasValue) continue;

                var monthlyStats = await _context.MonthlyFacilityStats
                    .FirstOrDefaultAsync(m => m.Year == today.Year && m.Month == today.Month && m.FacilityID == facility.ID.Value);

                if (monthlyStats == null) continue;

                var totalDeliveries = monthlyStats.TotalDeliveries;
                var caesareans = monthlyStats.CaesareanSections;
                var complications = await _context.ComplicationAnalytics
                    .CountAsync(c => c.FacilityID == facility.ID.Value && c.OccurrenceDateTime.Month == today.Month);

                var referrals = await _context.ReferralAnalytics
                    .CountAsync(r => r.SourceFacilityID == facility.ID.Value && r.ReferralDateTime.Month == today.Month);

                var caesareanRate = totalDeliveries > 0 ? (double)caesareans / totalDeliveries * 100 : 0;
                var complicationRate = totalDeliveries > 0 ? (double)complications / totalDeliveries * 100 : 0;
                var referralRate = totalDeliveries > 0 ? (double)referrals / totalDeliveries * 100 : 0;

                performances.Add(new FacilityPerformanceData
                {
                    FacilityID = facility.ID.Value,
                    FacilityName = facility.Name,
                    DistrictName = facility.District?.Name ?? facility.DistrictName,
                    TotalDeliveries = totalDeliveries,
                    CaesareanRate = caesareanRate,
                    ComplicationRate = complicationRate,
                    ReferralRate = referralRate,
                    PerformanceGrade = CalculateGrade(caesareanRate, complicationRate)
                });
            }

            return performances.OrderByDescending(p => p.TotalDeliveries).ToList();
        }

        private string CalculateGrade(double caesareanRate, double complicationRate)
        {
            // WHO recommends C-section rate between 10-15%
            // Lower complication rate is better
            var score = 100.0;

            // Penalize C-section rate outside optimal range
            if (caesareanRate < 10)
                score -= (10 - caesareanRate) * 2;
            else if (caesareanRate > 15)
                score -= (caesareanRate - 15) * 3;

            // Penalize high complication rate
            score -= complicationRate * 5;

            return score switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "F"
            };
        }
    }
}
