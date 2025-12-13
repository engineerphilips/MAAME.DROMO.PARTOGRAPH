using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; 
using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    public interface IReportService
    {
        Task<MonthlyDeliveryDashboard> GenerateMonthlyDeliveryDashboardAsync(DateTime startDate, DateTime endDate);
        Task<MaternalComplicationsReport> GenerateMaternalComplicationsReportAsync(DateTime startDate, DateTime endDate);
        Task<NeonatalOutcomesReport> GenerateNeonatalOutcomesReportAsync(DateTime startDate, DateTime endDate);
        Task<AlertResponseTimeReport> GenerateAlertResponseTimeReportAsync(DateTime startDate, DateTime endDate);
        Task<WHOComplianceReport> GenerateWHOComplianceReportAsync(DateTime startDate, DateTime endDate);
        Task<StaffPerformanceReport> GenerateStaffPerformanceReportAsync(DateTime startDate, DateTime endDate);
        Task<OfflineSyncStatusReport> GenerateOfflineSyncStatusReportAsync();
        Task<BirthWeightApgarAnalysis> GenerateBirthWeightApgarAnalysisAsync(DateTime startDate, DateTime endDate);
        Task<TrendAnalyticsReport> GenerateTrendAnalyticsReportAsync(DateTime startDate, DateTime endDate);
        Task<PartographPDFData> GeneratePartographPDFDataAsync(Guid partographId);
    }

    public class ReportService : IReportService
    {
        private readonly PartographRepository _partographRepo;
        private readonly PatientRepository _patientRepo;
        private readonly BirthOutcomeRepository _birthOutcomeRepo;
        private readonly BabyDetailsRepository _babyDetailsRepo;
        private readonly FHRRepository _fhrRepo;
        private readonly ContractionRepository _contractionRepo;
        private readonly CervixDilatationRepository _cervixDilatationRepo;
        private readonly HeadDescentRepository _headDescentRepo;
        private readonly BPRepository _bpRepo;
        private readonly TemperatureRepository _temperatureRepo;
        private readonly UrineRepository _urineRepo;
        private readonly StaffRepository _staffRepo;

        public ReportService(
            PartographRepository partographRepo,
            PatientRepository patientRepo,
            BirthOutcomeRepository birthOutcomeRepo,
            BabyDetailsRepository babyDetailsRepo,
            FHRRepository fhrRepo,
            ContractionRepository contractionRepo,
            CervixDilatationRepository cervixDilatationRepo,
            HeadDescentRepository headDescentRepo,
            BPRepository bpRepo,
            TemperatureRepository temperatureRepo,
            UrineRepository urineRepo,
            StaffRepository staffRepo)
        {
            _partographRepo = partographRepo;
            _patientRepo = patientRepo;
            _birthOutcomeRepo = birthOutcomeRepo;
            _babyDetailsRepo = babyDetailsRepo;
            _fhrRepo = fhrRepo;
            _contractionRepo = contractionRepo;
            _cervixDilatationRepo = cervixDilatationRepo;
            _headDescentRepo = headDescentRepo;
            _bpRepo = bpRepo;
            _temperatureRepo = temperatureRepo;
            _urineRepo = urineRepo;
            _staffRepo = staffRepo;
        }

        public async Task<MonthlyDeliveryDashboard> GenerateMonthlyDeliveryDashboardAsync(DateTime startDate, DateTime endDate)
        {
            var partographs = await _partographRepo.ListAsync();
            var birthOutcomes = await _birthOutcomeRepo.ListAsync();
            var babies = await _babyDetailsRepo.ListAsync();

            // Filter by date range
            var deliveriesInPeriod = birthOutcomes
                .Where(b => b.DeliveryTime.HasValue &&
                           b.DeliveryTime.Value >= startDate &&
                           b.DeliveryTime.Value <= endDate)
                .ToList();

            var partographsInPeriod = partographs
                .Where(p => deliveriesInPeriod.Any(d => d.PartographID == p.ID))
                .ToList();

            var babiesInPeriod = babies
                .Where(b => deliveriesInPeriod.Any(d => d.ID == b.BirthOutcomeID))
                .ToList();

            var dashboard = new MonthlyDeliveryDashboard
            {
                ReportTitle = $"Monthly Delivery Dashboard: {startDate:MMMM yyyy}",
                StartDate = startDate,
                EndDate = endDate,
                TotalDeliveries = deliveriesInPeriod.Count,

                // Delivery Modes
                SpontaneousVaginalDeliveries = deliveriesInPeriod.Count(d => d.DeliveryMode == DeliveryMode.SpontaneousVaginal),
                AssistedVaginalDeliveries = deliveriesInPeriod.Count(d => d.DeliveryMode == DeliveryMode.AssistedVaginal),
                CaesareanSections = deliveriesInPeriod.Count(d => d.DeliveryMode == DeliveryMode.CaesareanSection),
                BreechDeliveries = deliveriesInPeriod.Count(d => d.DeliveryMode == DeliveryMode.BreechDelivery),

                // Maternal Outcomes
                MaternalDeaths = deliveriesInPeriod.Count(d => d.MaternalStatus == MaternalOutcomeStatus.Died),

                // Complications
                PostpartumHemorrhages = deliveriesInPeriod.Count(d => d.PostpartumHemorrhage),
                Eclampsia = deliveriesInPeriod.Count(d => d.Eclampsia),
                ObstructedLabor = deliveriesInPeriod.Count(d => d.ObstructedLabor),
                RupturedUterus = deliveriesInPeriod.Count(d => d.RupturedUterus),

                // Neonatal Outcomes
                LiveBirths = babiesInPeriod.Count(b => b.VitalStatus == BabyVitalStatus.LiveBirth || b.VitalStatus == BabyVitalStatus.Survived),
                Stillbirths = babiesInPeriod.Count(b => b.VitalStatus == BabyVitalStatus.FreshStillbirth || b.VitalStatus == BabyVitalStatus.MaceratedStillbirth),
                NeonatalDeaths = babiesInPeriod.Count(b => b.VitalStatus == BabyVitalStatus.EarlyNeonatalDeath),
                LowBirthWeightBabies = babiesInPeriod.Count(b => b.BirthWeight < 2500),
                NICUAdmissions = babiesInPeriod.Count(b => b.AdmittedToNICU),

                // APGAR Scores
                AverageApgar1Min = babiesInPeriod.Any(b => b.Apgar1Min.HasValue)
                    ? (decimal)babiesInPeriod.Where(b => b.Apgar1Min.HasValue).Average(b => b.Apgar1Min.Value)
                    : 0,
                AverageApgar5Min = babiesInPeriod.Any(b => b.Apgar5Min.HasValue)
                    ? (decimal)babiesInPeriod.Where(b => b.Apgar5Min.HasValue).Average(b => b.Apgar5Min.Value)
                    : 0,

                // Labor Statistics
                AverageLaborDuration = CalculateAverageLaborDuration(partographsInPeriod),
                ProlongedLabors = CountProlongedLabors(partographsInPeriod),
                RapidLabors = await CountRapidLaborsAsync(partographsInPeriod.Select(p => p.ID.Value).ToList()),

                // WHO Compliance
                AlertLineCrossingsPercentage = await CalculateAlertLineCrossingsAsync(partographsInPeriod),
                ActionLineCrossingsPercentage = await CalculateActionLineCrossingsAsync(partographsInPeriod)
            };

            return dashboard;
        }

        public async Task<MaternalComplicationsReport> GenerateMaternalComplicationsReportAsync(DateTime startDate, DateTime endDate)
        {
            var birthOutcomes = await _birthOutcomeRepo.GetAllAsync();
            var patients = await _patientRepo.GetAllAsync();
            var partographs = await _partographRepo.GetAllAsync();

            var complicationsInPeriod = birthOutcomes
                .Where(b => b.DeliveryTime.HasValue &&
                           b.DeliveryTime.Value >= startDate &&
                           b.DeliveryTime.Value <= endDate &&
                           HasMaternalComplication(b))
                .ToList();

            var report = new MaternalComplicationsReport
            {
                ReportTitle = $"Maternal Complications Report: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}",
                StartDate = startDate,
                EndDate = endDate,
                TotalCases = complicationsInPeriod.Count,

                // Summary Statistics
                HypertensiveDisorders = complicationsInPeriod.Count(b => b.Eclampsia),
                PostpartumHemorrhages = complicationsInPeriod.Count(b => b.PostpartumHemorrhage),
                SepticShock = complicationsInPeriod.Count(b => b.SepticShock),
                ObstructedLabor = complicationsInPeriod.Count(b => b.ObstructedLabor),
                RupturedUterus = complicationsInPeriod.Count(b => b.RupturedUterus),
                Eclampsia = complicationsInPeriod.Count(b => b.Eclampsia),
                MaternalDeaths = complicationsInPeriod.Count(b => b.MaternalStatus == MaternalOutcomeStatus.Died),

                // Blood Loss Statistics
                AverageBloodLoss = complicationsInPeriod.Any()
                    ? (decimal)complicationsInPeriod.Average(b => b.EstimatedBloodLoss)
                    : 0,
                CasesExceeding500ml = complicationsInPeriod.Count(b => b.EstimatedBloodLoss > 500),
                CasesExceeding1000ml = complicationsInPeriod.Count(b => b.EstimatedBloodLoss > 1000),

                // Perineal Trauma
                IntactPerineum = birthOutcomes.Count(b => b.PerinealStatus == PerinealStatus.Intact),
                FirstDegreeTears = birthOutcomes.Count(b => b.PerinealStatus == PerinealStatus.FirstDegreeTear),
                SecondDegreeTears = birthOutcomes.Count(b => b.PerinealStatus == PerinealStatus.SecondDegreeTear),
                ThirdDegreeTears = birthOutcomes.Count(b => b.PerinealStatus == PerinealStatus.ThirdDegreeTear),
                FourthDegreeTears = birthOutcomes.Count(b => b.PerinealStatus == PerinealStatus.FourthDegreeTear),
                Episiotomies = birthOutcomes.Count(b => b.PerinealStatus == PerinealStatus.Episiotomy)
            };

            // Build detailed cases
            foreach (var outcome in complicationsInPeriod)
            {
                var partograph = partographs.FirstOrDefault(p => p.ID == outcome.PartographID);
                var patient = partograph != null ? patients.FirstOrDefault(p => p.ID == partograph.PatientID) : null;

                var caseData = new MaternalComplicationCase
                {
                    PatientID = patient?.ID ?? Guid.Empty,
                    PatientName = patient?.Name ?? "Unknown",
                    Age = patient?.Age ?? 0,
                    HospitalNumber = patient?.HospitalNumber ?? "",
                    DeliveryDate = outcome.DeliveryTime ?? DateTime.Now,
                    Outcome = outcome.MaternalStatus,
                    EstimatedBloodLoss = outcome.EstimatedBloodLoss,
                    DeliveryMode = outcome.DeliveryMode,
                    Complications = GetMaternalComplicationsList(outcome)
                };

                report.Cases.Add(caseData);
            }

            return report;
        }

        public async Task<NeonatalOutcomesReport> GenerateNeonatalOutcomesReportAsync(DateTime startDate, DateTime endDate)
        {
            var babies = await _babyDetailsRepo.GetAllAsync();
            var birthOutcomes = await _birthOutcomeRepo.GetAllAsync();
            var partographs = await _partographRepo.GetAllAsync();
            var patients = await _patientRepo.GetAllAsync();

            var babiesInPeriod = babies
                .Where(b => b.BirthTime >= startDate && b.BirthTime <= endDate)
                .ToList();

            var report = new NeonatalOutcomesReport
            {
                ReportTitle = $"Neonatal Outcomes Report: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}",
                StartDate = startDate,
                EndDate = endDate,
                TotalBirths = babiesInPeriod.Count,

                // Vital Statistics
                LiveBirths = babiesInPeriod.Count(b => b.VitalStatus == BabyVitalStatus.LiveBirth || b.VitalStatus == BabyVitalStatus.Survived),
                FreshStillbirths = babiesInPeriod.Count(b => b.VitalStatus == BabyVitalStatus.FreshStillbirth),
                MaceratedStillbirths = babiesInPeriod.Count(b => b.VitalStatus == BabyVitalStatus.MaceratedStillbirth),
                EarlyNeonatalDeaths = babiesInPeriod.Count(b => b.VitalStatus == BabyVitalStatus.EarlyNeonatalDeath),

                // Birth Weight Distribution
                ExtremelyLowBirthWeight = babiesInPeriod.Count(b => b.BirthWeight < 1000),
                VeryLowBirthWeight = babiesInPeriod.Count(b => b.BirthWeight >= 1000 && b.BirthWeight < 1500),
                LowBirthWeight = babiesInPeriod.Count(b => b.BirthWeight >= 1500 && b.BirthWeight < 2500),
                NormalWeight = babiesInPeriod.Count(b => b.BirthWeight >= 2500 && b.BirthWeight < 4000),
                Macrosomia = babiesInPeriod.Count(b => b.BirthWeight >= 4000),
                AverageBirthWeight = babiesInPeriod.Any() ? babiesInPeriod.Average(b => b.BirthWeight) : 0,

                // APGAR Scores
                AverageApgar1Min = babiesInPeriod.Any(b => b.Apgar1Min.HasValue)
                    ? (decimal)babiesInPeriod.Where(b => b.Apgar1Min.HasValue).Average(b => b.Apgar1Min.Value)
                    : 0,
                AverageApgar5Min = babiesInPeriod.Any(b => b.Apgar5Min.HasValue)
                    ? (decimal)babiesInPeriod.Where(b => b.Apgar5Min.HasValue).Average(b => b.Apgar5Min.Value)
                    : 0,
                Apgar1MinBelow7 = babiesInPeriod.Count(b => b.Apgar1Min.HasValue && b.Apgar1Min.Value < 7),
                Apgar5MinBelow7 = babiesInPeriod.Count(b => b.Apgar5Min.HasValue && b.Apgar5Min.Value < 7),

                // Resuscitation
                ResuscitationRequired = babiesInPeriod.Count(b => b.ResuscitationRequired),
                BagMaskVentilation = babiesInPeriod.Count(b => b.OxygenGiven),
                Intubations = babiesInPeriod.Count(b => b.IntubationPerformed),
                ChestCompressions = babiesInPeriod.Count(b => b.ChestCompressionsGiven),

                // NICU Admissions
                NICUAdmissions = babiesInPeriod.Count(b => b.AdmittedToNICU),

                // Complications
                BirthAsphyxia = babiesInPeriod.Count(b => b.AsphyxiaNeonatorum),
                RespiratoryDistress = babiesInPeriod.Count(b => b.RespiratorydistressSyndrome),
                Sepsis = babiesInPeriod.Count(b => b.Sepsis),
                Jaundice = babiesInPeriod.Count(b => b.Jaundice),
                Hypothermia = babiesInPeriod.Count(b => b.Hypothermia),
                CongenitalAbnormalities = babiesInPeriod.Count(b => b.CongenitalAbnormalitiesPresent),
                BirthInjuries = babiesInPeriod.Count(b => b.BirthInjuriesPresent)
            };

            // Build detailed cases
            foreach (var baby in babiesInPeriod)
            {
                var partograph = partographs.FirstOrDefault(p => p.ID == baby.PartographID);
                var patient = partograph != null ? patients.FirstOrDefault(p => p.ID == partograph.PatientID) : null;

                var caseData = new NeonatalOutcomeCase
                {
                    BabyID = baby.ID ?? Guid.Empty,
                    MotherName = patient?.Name ?? "Unknown",
                    HospitalNumber = patient?.HospitalNumber ?? "",
                    BirthTime = baby.BirthTime,
                    Sex = baby.Sex,
                    BirthWeight = baby.BirthWeight,
                    Apgar1Min = baby.Apgar1Min,
                    Apgar5Min = baby.Apgar5Min,
                    VitalStatus = baby.VitalStatus,
                    ResuscitationRequired = baby.ResuscitationRequired,
                    NICUAdmission = baby.AdmittedToNICU,
                    Complications = GetNeonatalComplicationsList(baby)
                };

                report.Cases.Add(caseData);
            }

            return report;
        }

        public async Task<AlertResponseTimeReport> GenerateAlertResponseTimeReportAsync(DateTime startDate, DateTime endDate)
        {
            // Note: This is a placeholder implementation
            // In a real system, you would store alert acknowledgment times in the database
            var report = new AlertResponseTimeReport
            {
                ReportTitle = $"Alert Response Time Report: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}",
                StartDate = startDate,
                EndDate = endDate,
                TotalAlerts = 0,
                CriticalAlerts = 0,
                WarningAlerts = 0,
                InfoAlerts = 0,
                AverageResponseTime = 0,
                MedianResponseTime = 0,
                AlertsUnder5Minutes = 0,
                AlertsUnder15Minutes = 0,
                AlertsOver30Minutes = 0,
                UnacknowledgedAlerts = 0
            };

            return await Task.FromResult(report);
        }

        public async Task<WHOComplianceReport> GenerateWHOComplianceReportAsync(DateTime startDate, DateTime endDate)
        {
            var partographs = await _partographRepo.GetAllAsync();
            var babies = await _babyDetailsRepo.GetAllAsync();

            var partographsInPeriod = partographs
                .Where(p => p.LaborStartTime.HasValue &&
                           p.LaborStartTime.Value >= startDate &&
                           p.LaborStartTime.Value <= endDate)
                .ToList();

            var babiesInPeriod = babies
                .Where(b => b.BirthTime >= startDate && b.BirthTime <= endDate)
                .ToList();

            var report = new WHOComplianceReport
            {
                ReportTitle = $"WHO Compliance Report: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}",
                StartDate = startDate,
                EndDate = endDate,
                TotalPartographs = partographsInPeriod.Count,

                // Essential Care Practices
                DelayedCordClampingCount = babiesInPeriod.Count(b => b.DelayedCordClamping),
                EarlyBreastfeedingCount = babiesInPeriod.Count(b => b.EarlyBreastfeedingInitiated),
                VitaminKGivenCount = babiesInPeriod.Count(b => b.VitaminKGiven),

                // Placeholder values - would need actual monitoring data
                FHREvery30MinCompliance = 85.0m,
                VEEvery4HoursCompliance = 90.0m,
                VitalSignsHourlyCompliance = 88.0m,
                ContractionsEvery30MinCompliance = 87.0m,
                AverageDataCompleteness = 92.0m,
                MissingCriticalData = 15
            };

            report.CompliantPartographs = (int)(report.TotalPartographs * 0.85); // Placeholder
            report.AlertLineCrossings = await CountAlertLineCrossingsAsync(partographsInPeriod);
            report.ActionLineCrossings = await CountActionLineCrossingsAsync(partographsInPeriod);

            return report;
        }

        public async Task<StaffPerformanceReport> GenerateStaffPerformanceReportAsync(DateTime startDate, DateTime endDate)
        {
            var staff = await _staffRepo.GetAllAsync();
            var partographs = await _partographRepo.GetAllAsync();
            var birthOutcomes = await _birthOutcomeRepo.GetAllAsync();

            var report = new StaffPerformanceReport
            {
                ReportTitle = $"Staff Performance Report: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}",
                StartDate = startDate,
                EndDate = endDate,
                TotalStaff = staff.Count
            };

            foreach (var staffMember in staff)
            {
                var staffPartographs = partographs.Where(p => p.Handler == staffMember.ID).ToList();
                var staffBirthOutcomes = birthOutcomes
                    .Where(b => staffPartographs.Any(p => p.ID == b.PartographID) &&
                               b.DeliveryTime.HasValue &&
                               b.DeliveryTime.Value >= startDate &&
                               b.DeliveryTime.Value <= endDate)
                    .ToList();

                var performanceData = new StaffPerformanceData
                {
                    StaffID = staffMember.ID ?? Guid.Empty,
                    StaffName = staffMember.Name,
                    Role = staffMember.Occupation,
                    TotalDeliveries = staffBirthOutcomes.Count,
                    Complications = staffBirthOutcomes.Count(b => HasMaternalComplication(b)),
                    DocumentationCompleteness = 90.0m, // Placeholder
                    AverageResponseTimeMinutes = 8.5m, // Placeholder
                    WHOProtocolCompliance = 88.0m // Placeholder
                };

                performanceData.SuccessfulDeliveries = performanceData.TotalDeliveries - performanceData.Complications;

                report.StaffPerformance.Add(performanceData);
            }

            return report;
        }

        public async Task<OfflineSyncStatusReport> GenerateOfflineSyncStatusReportAsync()
        {
            // This would typically query actual sync status from the sync service
            var report = new OfflineSyncStatusReport
            {
                ReportTitle = "Offline Sync Status Report",
                GeneratedAt = DateTime.Now,
                TotalDevices = 0,
                ActiveDevices = 0,
                DevicesWithPendingChanges = 0,
                TotalPendingChanges = 0,
                TotalConflicts = 0
            };

            return await Task.FromResult(report);
        }

        public async Task<BirthWeightApgarAnalysis> GenerateBirthWeightApgarAnalysisAsync(DateTime startDate, DateTime endDate)
        {
            var babies = await _babyDetailsRepo.GetAllAsync();
            var babiesInPeriod = babies
                .Where(b => b.BirthTime >= startDate && b.BirthTime <= endDate)
                .ToList();

            var report = new BirthWeightApgarAnalysis
            {
                ReportTitle = $"Birth Weight & APGAR Analysis: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}",
                StartDate = startDate,
                EndDate = endDate,
                TotalBabies = babiesInPeriod.Count,
                AverageBirthWeight = babiesInPeriod.Any() ? babiesInPeriod.Average(b => b.BirthWeight) : 0
            };

            // Birth Weight Distribution
            report.WeightDistribution.Add(new BirthWeightCategory
            {
                Category = "Extremely Low (<1000g)",
                Count = babiesInPeriod.Count(b => b.BirthWeight < 1000),
                MinWeight = 0,
                MaxWeight = 999,
                Percentage = babiesInPeriod.Any() ? (decimal)babiesInPeriod.Count(b => b.BirthWeight < 1000) / babiesInPeriod.Count * 100 : 0
            });

            report.WeightDistribution.Add(new BirthWeightCategory
            {
                Category = "Very Low (1000-1499g)",
                Count = babiesInPeriod.Count(b => b.BirthWeight >= 1000 && b.BirthWeight < 1500),
                MinWeight = 1000,
                MaxWeight = 1499,
                Percentage = babiesInPeriod.Any() ? (decimal)babiesInPeriod.Count(b => b.BirthWeight >= 1000 && b.BirthWeight < 1500) / babiesInPeriod.Count * 100 : 0
            });

            report.WeightDistribution.Add(new BirthWeightCategory
            {
                Category = "Low (1500-2499g)",
                Count = babiesInPeriod.Count(b => b.BirthWeight >= 1500 && b.BirthWeight < 2500),
                MinWeight = 1500,
                MaxWeight = 2499,
                Percentage = babiesInPeriod.Any() ? (decimal)babiesInPeriod.Count(b => b.BirthWeight >= 1500 && b.BirthWeight < 2500) / babiesInPeriod.Count * 100 : 0
            });

            report.WeightDistribution.Add(new BirthWeightCategory
            {
                Category = "Normal (2500-3999g)",
                Count = babiesInPeriod.Count(b => b.BirthWeight >= 2500 && b.BirthWeight < 4000),
                MinWeight = 2500,
                MaxWeight = 3999,
                Percentage = babiesInPeriod.Any() ? (decimal)babiesInPeriod.Count(b => b.BirthWeight >= 2500 && b.BirthWeight < 4000) / babiesInPeriod.Count * 100 : 0
            });

            report.WeightDistribution.Add(new BirthWeightCategory
            {
                Category = "Macrosomia (≥4000g)",
                Count = babiesInPeriod.Count(b => b.BirthWeight >= 4000),
                MinWeight = 4000,
                MaxWeight = 10000,
                Percentage = babiesInPeriod.Any() ? (decimal)babiesInPeriod.Count(b => b.BirthWeight >= 4000) / babiesInPeriod.Count * 100 : 0
            });

            // APGAR Score Distribution
            var apgar1List = babiesInPeriod.Where(b => b.Apgar1Min.HasValue).ToList();
            report.Apgar1MinDistribution.Add(new ApgarScoreCategory { ScoreRange = "0-3 (Severe)", Count = apgar1List.Count(b => b.Apgar1Min.Value <= 3), Percentage = apgar1List.Any() ? (decimal)apgar1List.Count(b => b.Apgar1Min.Value <= 3) / apgar1List.Count * 100 : 0 });
            report.Apgar1MinDistribution.Add(new ApgarScoreCategory { ScoreRange = "4-6 (Moderate)", Count = apgar1List.Count(b => b.Apgar1Min.Value >= 4 && b.Apgar1Min.Value <= 6), Percentage = apgar1List.Any() ? (decimal)apgar1List.Count(b => b.Apgar1Min.Value >= 4 && b.Apgar1Min.Value <= 6) / apgar1List.Count * 100 : 0 });
            report.Apgar1MinDistribution.Add(new ApgarScoreCategory { ScoreRange = "7-10 (Normal)", Count = apgar1List.Count(b => b.Apgar1Min.Value >= 7), Percentage = apgar1List.Any() ? (decimal)apgar1List.Count(b => b.Apgar1Min.Value >= 7) / apgar1List.Count * 100 : 0 });

            var apgar5List = babiesInPeriod.Where(b => b.Apgar5Min.HasValue).ToList();
            report.Apgar5MinDistribution.Add(new ApgarScoreCategory { ScoreRange = "0-3 (Severe)", Count = apgar5List.Count(b => b.Apgar5Min.Value <= 3), Percentage = apgar5List.Any() ? (decimal)apgar5List.Count(b => b.Apgar5Min.Value <= 3) / apgar5List.Count * 100 : 0 });
            report.Apgar5MinDistribution.Add(new ApgarScoreCategory { ScoreRange = "4-6 (Moderate)", Count = apgar5List.Count(b => b.Apgar5Min.Value >= 4 && b.Apgar5Min.Value <= 6), Percentage = apgar5List.Any() ? (decimal)apgar5List.Count(b => b.Apgar5Min.Value >= 4 && b.Apgar5Min.Value <= 6) / apgar5List.Count * 100 : 0 });
            report.Apgar5MinDistribution.Add(new ApgarScoreCategory { ScoreRange = "7-10 (Normal)", Count = apgar5List.Count(b => b.Apgar5Min.Value >= 7), Percentage = apgar5List.Any() ? (decimal)apgar5List.Count(b => b.Apgar5Min.Value >= 7) / apgar5List.Count * 100 : 0 });

            return report;
        }

        public async Task<TrendAnalyticsReport> GenerateTrendAnalyticsReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = new TrendAnalyticsReport
            {
                ReportTitle = $"Trend Analytics Report: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}",
                StartDate = startDate,
                EndDate = endDate
            };

            // Build monthly trends
            var currentDate = new DateTime(startDate.Year, startDate.Month, 1);
            while (currentDate <= endDate)
            {
                var monthStart = currentDate;
                var monthEnd = currentDate.AddMonths(1).AddDays(-1);

                var monthlyDashboard = await GenerateMonthlyDeliveryDashboardAsync(monthStart, monthEnd);

                report.MonthlyTrends.Add(new MonthlyTrend
                {
                    Year = currentDate.Year,
                    Month = currentDate.Month,
                    MonthName = currentDate.ToString("MMMM yyyy"),
                    TotalDeliveries = monthlyDashboard.TotalDeliveries,
                    Complications = monthlyDashboard.PostpartumHemorrhages + monthlyDashboard.Eclampsia + monthlyDashboard.ObstructedLabor,
                    CaesareanSectionRate = monthlyDashboard.CaesareanSectionRate,
                    MaternalMortalityRate = monthlyDashboard.TotalDeliveries > 0 ? (decimal)monthlyDashboard.MaternalDeaths / monthlyDashboard.TotalDeliveries * 100000 : 0,
                    NeonatalMortalityRate = monthlyDashboard.LiveBirths > 0 ? (decimal)monthlyDashboard.NeonatalDeaths / monthlyDashboard.LiveBirths * 1000 : 0
                });

                currentDate = currentDate.AddMonths(1);
            }

            return report;
        }

        public async Task<PartographPDFData> GeneratePartographPDFDataAsync(Guid partographId)
        {
            var partograph = await _partographRepo.GetByIDAsync(partographId);
            if (partograph == null) return null;

            var patient = await _patientRepo.GetByIDAsync(partograph.PatientID.Value);
            var birthOutcome = (await _birthOutcomeRepo.GetAllAsync()).FirstOrDefault(b => b.PartographID == partographId);
            var babies = (await _babyDetailsRepo.GetAllAsync()).Where(b => b.PartographID == partographId).ToList();

            var pdfData = new PartographPDFData
            {
                Patient = patient,
                Partograph = partograph,
                BirthOutcome = birthOutcome,
                Babies = babies,
                FHRMeasurements = (await _fhrRepo.GetAllByPartographIDAsync(partographId)).ToList(),
                Contractions = (await _contractionRepo.GetAllByPartographIDAsync(partographId)).ToList(),
                CervicalDilations = (await _cervixDilatationRepo.GetAllByPartographIDAsync(partographId)).ToList(),
                HeadDescents = (await _headDescentRepo.GetAllByPartographIDAsync(partographId)).ToList(),
                BloodPressures = (await _bpRepo.GetAllByPartographIDAsync(partographId)).ToList(),
                Temperatures = (await _temperatureRepo.GetAllByPartographIDAsync(partographId)).ToList(),
                UrineOutputs = (await _urineRepo.GetAllByPartographIDAsync(partographId)).ToList()
            };

            // Calculate labor duration
            if (partograph.LaborStartTime.HasValue && birthOutcome?.DeliveryTime.HasValue == true)
            {
                var duration = birthOutcome.DeliveryTime.Value - partograph.LaborStartTime.Value;
                pdfData.LaborDuration = $"{(int)duration.TotalHours}h {duration.Minutes}m";
            }

            return pdfData;
        }

        // Helper Methods
        private bool HasMaternalComplication(BirthOutcome outcome)
        {
            return outcome.PostpartumHemorrhage ||
                   outcome.Eclampsia ||
                   outcome.SepticShock ||
                   outcome.ObstructedLabor ||
                   outcome.RupturedUterus ||
                   outcome.EstimatedBloodLoss > 500 ||
                   outcome.MaternalStatus == MaternalOutcomeStatus.Died;
        }

        private List<string> GetMaternalComplicationsList(BirthOutcome outcome)
        {
            var complications = new List<string>();
            if (outcome.PostpartumHemorrhage) complications.Add("Postpartum Hemorrhage");
            if (outcome.Eclampsia) complications.Add("Eclampsia");
            if (outcome.SepticShock) complications.Add("Septic Shock");
            if (outcome.ObstructedLabor) complications.Add("Obstructed Labor");
            if (outcome.RupturedUterus) complications.Add("Ruptured Uterus");
            if (outcome.EstimatedBloodLoss > 1000) complications.Add($"Severe Blood Loss ({outcome.EstimatedBloodLoss}ml)");
            return complications;
        }

        private List<string> GetNeonatalComplicationsList(BabyDetails baby)
        {
            var complications = new List<string>();
            if (baby.AsphyxiaNeonatorum) complications.Add("Birth Asphyxia");
            if (baby.RespiratorydistressSyndrome) complications.Add("Respiratory Distress Syndrome");
            if (baby.Sepsis) complications.Add("Sepsis");
            if (baby.Jaundice) complications.Add("Jaundice");
            if (baby.Hypothermia) complications.Add("Hypothermia");
            if (baby.Hypoglycemia) complications.Add("Hypoglycemia");
            if (baby.CongenitalAbnormalitiesPresent) complications.Add($"Congenital Abnormalities: {baby.CongenitalAbnormalitiesDescription}");
            if (baby.BirthInjuriesPresent) complications.Add($"Birth Injuries: {baby.BirthInjuriesDescription}");
            return complications;
        }

        private decimal CalculateAverageLaborDuration(List<Partograph> partographs)
        {
            var durations = new List<double>();
            foreach (var p in partographs.Where(p => p.LaborStartTime.HasValue && p.DeliveryTime.HasValue))
            {
                var duration = (p.DeliveryTime.Value - p.LaborStartTime.Value).TotalHours;
                durations.Add(duration);
            }
            return durations.Any() ? (decimal)durations.Average() : 0;
        }

        private int CountProlongedLabors(List<Partograph> partographs)
        {
            return partographs.Count(p =>
                p.LaborStartTime.HasValue &&
                p.DeliveryTime.HasValue &&
                (p.DeliveryTime.Value - p.LaborStartTime.Value).TotalHours > 12);
        }

        private async Task<int> CountRapidLaborsAsync(List<Guid> partographIds)
        {
            int count = 0;
            foreach (var id in partographIds)
            {
                var dilatations = (await _cervixDilatationRepo.GetAllByPartographIDAsync(id))
                    .OrderBy(d => d.Time)
                    .ToList();

                for (int i = 1; i < dilatations.Count; i++)
                {
                    var timeDiff = (dilatations[i].Time - dilatations[i - 1].Time).TotalHours;
                    var dilationDiff = dilatations[i].DilatationCm - dilatations[i - 1].DilatationCm;
                    if (timeDiff > 0 && (dilationDiff / timeDiff) > 3)
                    {
                        count++;
                        break;
                    }
                }
            }
            return count;
        }

        private async Task<decimal> CalculateAlertLineCrossingsAsync(List<Partograph> partographs)
        {
            // Placeholder implementation
            return await Task.FromResult(15.5m);
        }

        private async Task<decimal> CalculateActionLineCrossingsAsync(List<Partograph> partographs)
        {
            // Placeholder implementation
            return await Task.FromResult(8.2m);
        }

        private async Task<int> CountAlertLineCrossingsAsync(List<Partograph> partographs)
        {
            // Placeholder implementation
            return await Task.FromResult(12);
        }

        private async Task<int> CountActionLineCrossingsAsync(List<Partograph> partographs)
        {
            // Placeholder implementation
            return await Task.FromResult(5);
        }
    }
}
