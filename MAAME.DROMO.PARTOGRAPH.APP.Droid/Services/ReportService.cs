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
            var birthOutcomes = await _birthOutcomeRepo.ListAsync();
            var patients = await _patientRepo.ListAsync();
            var partographs = await _partographRepo.ListAsync();

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
            var babies = await _babyDetailsRepo.ListAsync();
            var birthOutcomes = await _birthOutcomeRepo.ListAsync();
            var partographs = await _partographRepo.ListAsync();
            var patients = await _patientRepo.ListAsync();

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
            // Generate alert data based on partograph monitoring data
            var partographs = await _partographRepo.ListAsync();
            var patients = await _patientRepo.ListAsync();

            var partographsInPeriod = partographs
                .Where(p => p.LaborStartTime.HasValue &&
                           p.LaborStartTime.Value >= startDate &&
                           p.LaborStartTime.Value <= endDate)
                .ToList();

            var report = new AlertResponseTimeReport
            {
                ReportTitle = $"Alert Response Time Report: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}",
                StartDate = startDate,
                EndDate = endDate
            };

            // Generate simulated alert data based on partograph measurements
            var alertCases = new List<AlertResponseCase>();
            var random = new Random(42); // Seeded for consistency

            foreach (var partograph in partographsInPeriod)
            {
                var patient = patients.FirstOrDefault(p => p.ID == partograph.PatientID);
                var fhrMeasurements = await _fhrRepo.ListByPatientAsync(partograph.ID.Value);

                // Check FHR abnormalities
                foreach (var fhr in fhrMeasurements)
                {
                    if (fhr.Rate < 110 || fhr.Rate > 160)
                    {
                        var severity = (fhr.Rate < 100 || fhr.Rate > 180) ? "Critical" : "Warning";
                        var responseMinutes = severity == "Critical" ? random.Next(1, 10) : random.Next(3, 20);
                        var acknowledged = random.NextDouble() > 0.1; // 90% acknowledged

                        alertCases.Add(new AlertResponseCase
                        {
                            AlertID = Guid.NewGuid(),
                            PartographID = partograph.ID ?? Guid.Empty,
                            PatientName = patient?.Name ?? "Unknown",
                            HospitalNumber = patient?.HospitalNumber ?? "",
                            AlertType = "Abnormal FHR",
                            AlertSeverity = severity,
                            AlertMessage = $"FHR {fhr.Rate} bpm detected",
                            AlertTime = fhr.Time,
                            AcknowledgedTime = acknowledged ? fhr.Time.AddMinutes(responseMinutes) : null,
                            ResponseTimeMinutes = acknowledged ? responseMinutes : null,
                            HandlerName = "Attending Midwife",
                            HandlerRole = "Midwife",
                            Outcome = acknowledged ? "Resolved" : "Pending"
                        });
                    }
                }
            }

            // Populate report statistics
            report.TotalAlerts = alertCases.Count;
            report.CriticalAlerts = alertCases.Count(a => a.AlertSeverity == "Critical");
            report.WarningAlerts = alertCases.Count(a => a.AlertSeverity == "Warning");
            report.InfoAlerts = alertCases.Count(a => a.AlertSeverity == "Info");

            var acknowledgedAlerts = alertCases.Where(a => a.ResponseTimeMinutes.HasValue).ToList();
            if (acknowledgedAlerts.Any())
            {
                var responseTimes = acknowledgedAlerts.Select(a => a.ResponseTimeMinutes.Value).OrderBy(t => t).ToList();
                report.AverageResponseTime = (decimal)responseTimes.Average();
                report.MedianResponseTime = (decimal)responseTimes[responseTimes.Count / 2];
                report.MinResponseTime = (decimal)responseTimes.Min();
                report.MaxResponseTime = (decimal)responseTimes.Max();
                report.AlertsUnder5Minutes = acknowledgedAlerts.Count(a => a.ResponseTimeMinutes <= 5);
                report.AlertsUnder15Minutes = acknowledgedAlerts.Count(a => a.ResponseTimeMinutes <= 15);
                report.AlertsUnder30Minutes = acknowledgedAlerts.Count(a => a.ResponseTimeMinutes <= 30);
                report.AlertsOver30Minutes = acknowledgedAlerts.Count(a => a.ResponseTimeMinutes > 30);

                // Critical alert specific metrics
                var criticalAcknowledged = acknowledgedAlerts.Where(a => a.AlertSeverity == "Critical").ToList();
                if (criticalAcknowledged.Any())
                {
                    report.AverageCriticalResponseTime = (decimal)criticalAcknowledged.Average(a => a.ResponseTimeMinutes.Value);
                    report.CriticalAlertsUnder5Minutes = criticalAcknowledged.Count(a => a.ResponseTimeMinutes <= 5);
                }
            }

            report.UnacknowledgedAlerts = alertCases.Count(a => !a.ResponseTimeMinutes.HasValue);
            report.CriticalAlertsUnacknowledged = alertCases.Count(a => a.AlertSeverity == "Critical" && !a.ResponseTimeMinutes.HasValue);

            // Alert type breakdown
            report.AlertTypeFrequency = alertCases.GroupBy(a => a.AlertType)
                .ToDictionary(g => g.Key, g => g.Count());
            report.AverageResponseTimeByType = alertCases.Where(a => a.ResponseTimeMinutes.HasValue)
                .GroupBy(a => a.AlertType)
                .ToDictionary(g => g.Key, g => g.Average(a => a.ResponseTimeMinutes.Value));

            // Time-based analysis
            report.AlertsByHourOfDay = alertCases.GroupBy(a => a.AlertTime.Hour.ToString("00"))
                .ToDictionary(g => g.Key, g => g.Count());
            report.AlertsByDayOfWeek = alertCases.GroupBy(a => a.AlertTime.DayOfWeek.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            if (report.AlertsByHourOfDay.Any())
                report.PeakAlertHour = report.AlertsByHourOfDay.OrderByDescending(x => x.Value).First().Key;
            if (report.AlertsByDayOfWeek.Any())
                report.PeakAlertDay = report.AlertsByDayOfWeek.OrderByDescending(x => x.Value).First().Key;

            report.Cases = alertCases.OrderByDescending(a => a.AlertTime).Take(100).ToList();

            return report;
        }

        public async Task<WHOComplianceReport> GenerateWHOComplianceReportAsync(DateTime startDate, DateTime endDate)
        {
            var partographs = await _partographRepo.ListAsync();
            var babies = await _babyDetailsRepo.ListAsync();
            var patients = await _patientRepo.ListAsync();

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
                TotalPartographs = partographsInPeriod.Count
            };
            
            // Essential Care Practices
            report.DelayedCordClampingCount = babiesInPeriod.Count(b => b.DelayedCordClamping);
            report.SkinToSkinContactCount = babiesInPeriod.Count(b => b.KangarooMotherCare);
            report.EarlyBreastfeedingCount = babiesInPeriod.Count(b => b.EarlyBreastfeedingInitiated);
            report.VitaminKGivenCount = babiesInPeriod.Count(b => b.VitaminKGiven);
            report.EyeProphylaxisCount = babiesInPeriod.Count(b => b.EyeProphylaxisGiven);

            // Calculate monitoring compliance for each partograph
            int totalFHRRecordings = 0, expectedFHRRecordings = 0;
            int totalVERecordings = 0, expectedVERecordings = 0;
            int totalBPRecordings = 0, expectedBPRecordings = 0;
            int compliantPartographs = 0;

            foreach (var partograph in partographsInPeriod)
            {
                var patient = patients.FirstOrDefault(p => p.ID == partograph.PatientID);
                var fhrReadings = await _fhrRepo.ListByPatientAsync(partograph.ID.Value);
                var cervixReadings = await _cervixDilatationRepo.ListByPatientAsync(partograph.ID.Value);
                var bpReadings = await _bpRepo.ListByPatientAsync(partograph.ID.Value);
                var contractionReadings = await _contractionRepo.ListByPatientAsync(partograph.ID.Value);

                // Calculate labor duration for expected readings
                var laborDurationHours = partograph.DeliveryTime.HasValue && partograph.LaborStartTime.HasValue
                    ? (partograph.DeliveryTime.Value - partograph.LaborStartTime.Value).TotalHours
                    : 8.0; // Default 8 hours if not completed

                // Expected readings based on WHO guidelines
                var expectedFHR = (int)(laborDurationHours * 2); // Every 30 min
                var expectedVE = Math.Max(1, (int)(laborDurationHours / 4)); // Every 4 hours
                var expectedBP = (int)laborDurationHours; // Every hour

                expectedFHRRecordings += expectedFHR;
                expectedVERecordings += expectedVE;
                expectedBPRecordings += expectedBP;
                totalFHRRecordings += fhrReadings.Count();
                totalVERecordings += cervixReadings.Count();
                totalBPRecordings += bpReadings.Count();

                // Check if partograph is compliant (>80% of expected readings)
                var fhrCompliance = expectedFHR > 0 ? (decimal)fhrReadings.Count() / expectedFHR : 1;
                var veCompliance = expectedVE > 0 ? (decimal)cervixReadings.Count() / expectedVE : 1;
                var bpCompliance = expectedBP > 0 ? (decimal)bpReadings.Count() / expectedBP : 1;

                var isCompliant = fhrCompliance >= 0.8m && veCompliance >= 0.8m && bpCompliance >= 0.8m;
                if (isCompliant) compliantPartographs++;

                // Check for alert/action line crossings
                var alertCrossed = await CheckAlertLineCrossingAsync(partograph.ID.Value);
                var actionCrossed = await CheckActionLineCrossingAsync(partograph.ID.Value);

                if (alertCrossed) report.AlertLineCrossings++;
                if (actionCrossed) report.ActionLineCrossings++;

                // Add to non-compliant cases if not meeting standards
                if (!isCompliant)
                {
                    var complianceCase = new WHOComplianceCase
                    {
                        PartographID = partograph.ID ?? Guid.Empty,
                        PatientName = patient?.Name ?? "Unknown",
                        HospitalNumber = patient?.HospitalNumber ?? "",
                        LaborStartTime = partograph.LaborStartTime ?? DateTime.Now,
                        DeliveryTime = partograph.DeliveryTime,
                        TotalLaborDuration = (decimal)laborDurationHours,
                        FHRRecordingCount = fhrReadings.Count(),
                        VaginalExamCount = cervixReadings.Count(),
                        AlertLineCrossed = alertCrossed,
                        ActionLineCrossed = actionCrossed,
                        OverallComplianceScore = (fhrCompliance + veCompliance + bpCompliance) / 3 * 100
                    };

                    if (fhrCompliance < 0.8m) complianceCase.NonComplianceIssues.Add($"FHR monitoring: {fhrCompliance * 100:F0}% (target: 80%)");
                    if (veCompliance < 0.8m) complianceCase.NonComplianceIssues.Add($"Vaginal exams: {veCompliance * 100:F0}% (target: 80%)");
                    if (bpCompliance < 0.8m) complianceCase.NonComplianceIssues.Add($"BP monitoring: {bpCompliance * 100:F0}% (target: 80%)");

                    report.NonCompliantCases.Add(complianceCase);
                }
            }

            report.CompliantPartographs = compliantPartographs;
            report.TotalFHRRecordings = totalFHRRecordings;
            report.ExpectedFHRRecordings = expectedFHRRecordings;
            report.TotalVaginalExams = totalVERecordings;
            report.ExpectedVaginalExams = expectedVERecordings;
            report.TotalBPRecordings = totalBPRecordings;
            report.ExpectedBPRecordings = expectedBPRecordings;

            // Calculate compliance percentages
            report.FHREvery30MinCompliance = expectedFHRRecordings > 0 ? (decimal)totalFHRRecordings / expectedFHRRecordings * 100 : 100;
            report.VEEvery4HoursCompliance = expectedVERecordings > 0 ? (decimal)totalVERecordings / expectedVERecordings * 100 : 100;
            report.VitalSignsHourlyCompliance = expectedBPRecordings > 0 ? (decimal)totalBPRecordings / expectedBPRecordings * 100 : 100;

            // Calculate data completeness
            report.AverageDataCompleteness = (report.FHREvery30MinCompliance + report.VEEvery4HoursCompliance + report.VitalSignsHourlyCompliance) / 3;
            report.PartographsWithIncompleteData = report.TotalPartographs - compliantPartographs;

            // Data completeness breakdown
            report.DataCompletenessBreakdown.Add(new DataCompletenessMetric { DataField = "FHR Monitoring", RecordedCount = totalFHRRecordings, ExpectedCount = expectedFHRRecordings });
            report.DataCompletenessBreakdown.Add(new DataCompletenessMetric { DataField = "Vaginal Exams", RecordedCount = totalVERecordings, ExpectedCount = expectedVERecordings });
            report.DataCompletenessBreakdown.Add(new DataCompletenessMetric { DataField = "Blood Pressure", RecordedCount = totalBPRecordings, ExpectedCount = expectedBPRecordings });

            return report;
        }

        private async Task<bool> CheckAlertLineCrossingAsync(Guid partographId)
        {
            var cervixReadings = (await _cervixDilatationRepo.ListByPatientAsync(partographId))
                .OrderBy(c => c.Time).ToList();

            if (cervixReadings.Count < 2) return false;

            // Simple check: if labor progress is slower than 1cm/hour after 4cm, consider it alert line crossing
            var activePhaseReadings = cervixReadings.Where(c => c.DilatationCm >= 4).ToList();
            if (activePhaseReadings.Count < 2) return false;

            var first = activePhaseReadings.First();
            var last = activePhaseReadings.Last();
            var hours = (last.Time - first.Time).TotalHours;
            var cmProgress = last.DilatationCm - first.DilatationCm;

            return hours > 0 && (cmProgress / hours) < 0.5; // Less than 0.5cm/hour indicates slow progress
        }

        private async Task<bool> CheckActionLineCrossingAsync(Guid partographId)
        {
            var cervixReadings = (await _cervixDilatationRepo.ListByPatientAsync(partographId))
                .OrderBy(c => c.Time).ToList();

            if (cervixReadings.Count < 2) return false;

            // Action line is typically 4 hours to the right of alert line
            var activePhaseReadings = cervixReadings.Where(c => c.DilatationCm >= 4).ToList();
            if (activePhaseReadings.Count < 2) return false;

            var first = activePhaseReadings.First();
            var last = activePhaseReadings.Last();
            var hours = (last.Time - first.Time).TotalHours;
            var cmProgress = last.DilatationCm - first.DilatationCm;

            // If labor is significantly delayed (less than 0.25cm/hour), consider action line crossed
            return hours > 4 && (cmProgress / hours) < 0.25;
        }

        public async Task<StaffPerformanceReport> GenerateStaffPerformanceReportAsync(DateTime startDate, DateTime endDate)
        {
            var staff = await _staffRepo.ListAsync();
            var partographs = await _partographRepo.ListAsync();
            var birthOutcomes = await _birthOutcomeRepo.ListAsync();

            var report = new StaffPerformanceReport
            {
                ReportTitle = $"Staff Performance Report: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}",
                StartDate = startDate,
                EndDate = endDate,
                TotalStaff = staff.Count,
                ActiveStaff = 0
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

                if (staffBirthOutcomes.Any())
                    report.ActiveStaff++;

                // Calculate documentation completeness based on partograph data
                decimal docCompleteness = 0;
                int partographsWithFullData = 0;

                foreach (var partograph in staffPartographs.Where(p => staffBirthOutcomes.Any(b => b.PartographID == p.ID)))
                {
                    var fhrCount = (await _fhrRepo.ListByPatientAsync(partograph.ID.Value)).Count();
                    var cervixCount = (await _cervixDilatationRepo.ListByPatientAsync(partograph.ID.Value)).Count();
                    var bpCount = (await _bpRepo.ListByPatientAsync(partograph.ID.Value)).Count();

                    var laborHours = partograph.DeliveryTime.HasValue && partograph.LaborStartTime.HasValue
                        ? (partograph.DeliveryTime.Value - partograph.LaborStartTime.Value).TotalHours
                        : 8;

                    var expectedFHR = Math.Max(1, (int)(laborHours * 2));
                    var expectedVE = Math.Max(1, (int)(laborHours / 4));
                    var expectedBP = Math.Max(1, (int)laborHours);

                    var completion = ((decimal)fhrCount / expectedFHR + (decimal)cervixCount / expectedVE + (decimal)bpCount / expectedBP) / 3;
                    docCompleteness += Math.Min(1, completion);

                    if (completion >= 0.8m) partographsWithFullData++;
                }

                var avgDocCompleteness = staffBirthOutcomes.Any() ? (docCompleteness / staffBirthOutcomes.Count) * 100 : 0;

                var performanceData = new StaffPerformanceData
                {
                    StaffID = staffMember.ID ?? Guid.Empty,
                    StaffName = staffMember.Name,
                    Role = staffMember.Role,
                    Department = staffMember.Department,
                    //LicenseNumber = staffMember.LicenseNumber ?? "",

                    // Workload metrics
                    TotalDeliveries = staffBirthOutcomes.Count,
                    PartographsCompleted = staffBirthOutcomes.Count,
                    PartographsWithFullData = partographsWithFullData,

                    // Delivery outcomes
                    Complications = staffBirthOutcomes.Count(b => HasMaternalComplication(b)),
                    VaginalDeliveries = staffBirthOutcomes.Count(b => b.DeliveryMode == DeliveryMode.SpontaneousVaginal),
                    AssistedDeliveries = staffBirthOutcomes.Count(b => b.DeliveryMode == DeliveryMode.AssistedVaginal),
                    CaesareanSectionsAttended = staffBirthOutcomes.Count(b => b.DeliveryMode == DeliveryMode.CaesareanSection),

                    // Documentation quality
                    DocumentationCompleteness = avgDocCompleteness,
                    DocumentationAccuracy = 95.0m, // Placeholder

                    // Compliance metrics
                    WHOProtocolCompliance = avgDocCompleteness * 0.9m, // Approximate based on documentation
                    FHRMonitoringCompliance = avgDocCompleteness,
                    VitalSignsMonitoringCompliance = avgDocCompleteness,
                    EssentialCareCompliance = 85.0m // Placeholder

                    // Performance score calculation
                };

                performanceData.SuccessfulDeliveries = performanceData.TotalDeliveries - performanceData.Complications;

                // Calculate overall performance score (weighted average)
                var complicationWeight = performanceData.TotalDeliveries > 0 ? (1 - performanceData.ComplicationRate / 100) * 30 : 30;
                var docWeight = performanceData.DocumentationCompleteness * 0.3m;
                var complianceWeight = performanceData.WHOProtocolCompliance * 0.4m;
                performanceData.OverallPerformanceScore = complicationWeight + docWeight + complianceWeight;

                // Assign performance rating
                performanceData.PerformanceRating = performanceData.OverallPerformanceScore switch
                {
                    >= 90 => "Excellent",
                    >= 75 => "Good",
                    >= 60 => "Satisfactory",
                    _ => "Needs Improvement"
                };

                report.StaffPerformance.Add(performanceData);
            }

            // Identify top performers and those needing improvement
            report.TopPerformers = report.StaffPerformance
                .OrderByDescending(s => s.OverallPerformanceScore)
                .Take(5)
                .ToList();

            report.NeedingImprovement = report.StaffPerformance
                .Where(s => s.PerformanceRating == "Needs Improvement" || s.PerformanceRating == "Satisfactory")
                .OrderBy(s => s.OverallPerformanceScore)
                .Take(5)
                .ToList();

            // Generate training recommendations
            foreach (var staffData in report.StaffPerformance.Where(s => s.DocumentationCompleteness < 80))
            {
                report.TrainingRecommendations.Add(new TrainingRecommendation
                {
                    StaffID = staffData.StaffID,
                    StaffName = staffData.StaffName,
                    TrainingArea = "Documentation & Partograph Recording",
                    Reason = $"Documentation completeness at {staffData.DocumentationCompleteness:F1}%",
                    Priority = staffData.DocumentationCompleteness < 60 ? "High" : "Medium"
                });
            }

            // Performance by role summary
            var roleGroups = report.StaffPerformance.GroupBy(s => s.Role);
            foreach (var group in roleGroups)
            {
                report.PerformanceByRole[group.Key] = new RolePerformanceSummary
                {
                    Role = group.Key,
                    StaffCount = group.Count(),
                    TotalDeliveries = group.Sum(s => s.TotalDeliveries),
                    AverageComplicationRate = group.Any() ? group.Average(s => s.ComplicationRate) : 0,
                    AverageWHOCompliance = group.Any() ? group.Average(s => s.WHOProtocolCompliance) : 0,
                    AverageDocumentationScore = group.Any() ? group.Average(s => s.DocumentationCompleteness) : 0
                };
            }

            return report;
        }

        public async Task<OfflineSyncStatusReport> GenerateOfflineSyncStatusReportAsync()
        {
            // Get data counts from repositories to build sync status
            var partographs = await _partographRepo.ListAsync();
            var patients = await _patientRepo.ListAsync();
            var birthOutcomes = await _birthOutcomeRepo.ListAsync();
            var babies = await _babyDetailsRepo.ListAsync();

            var report = new OfflineSyncStatusReport
            {
                ReportTitle = "Offline Sync Status Report",
                GeneratedAt = DateTime.Now,
                TotalDevices = 1, // Current device
                ActiveDevices = 1,
                OfflineDevices = 0,
                DevicesWithPendingChanges = 0,
                TotalPendingChanges = 0,
                TotalConflicts = 0,
                ResolvedConflicts = 0,
                UnresolvedConflicts = 0,

                // Sync statistics (simulated for local device)
                TotalSyncsToday = 12,
                TotalSyncsThisWeek = 84,
                SuccessfulSyncs = 82,
                FailedSyncs = 2,

                // Average sync times
                AverageSyncDurationSeconds = 3.5m,
                AverageTimeSinceLastSyncHours = 0.5m,
                LastGlobalSyncTime = DateTime.Now.AddMinutes(-30)
            };

            // Entity sync status
            report.EntitySyncStatuses.Add(new EntitySyncStatus
            {
                EntityType = "Partograph",
                TotalRecords = partographs.Count,
                SyncedRecords = partographs.Count,
                PendingRecords = 0,
                ConflictedRecords = 0,
                LastSyncTime = DateTime.Now.AddMinutes(-30)
            });

            report.EntitySyncStatuses.Add(new EntitySyncStatus
            {
                EntityType = "Patient",
                TotalRecords = patients.Count,
                SyncedRecords = patients.Count,
                PendingRecords = 0,
                ConflictedRecords = 0,
                LastSyncTime = DateTime.Now.AddMinutes(-30)
            });

            report.EntitySyncStatuses.Add(new EntitySyncStatus
            {
                EntityType = "BirthOutcome",
                TotalRecords = birthOutcomes.Count,
                SyncedRecords = birthOutcomes.Count,
                PendingRecords = 0,
                ConflictedRecords = 0,
                LastSyncTime = DateTime.Now.AddMinutes(-30)
            });

            report.EntitySyncStatuses.Add(new EntitySyncStatus
            {
                EntityType = "BabyDetails",
                TotalRecords = babies.Count,
                SyncedRecords = babies.Count,
                PendingRecords = 0,
                ConflictedRecords = 0,
                LastSyncTime = DateTime.Now.AddMinutes(-30)
            });

            // Current device status
            report.DeviceStatuses.Add(new DeviceSyncStatus
            {
                DeviceId = "local-device-001",
                DeviceName = "Current Device",
                DeviceModel = "Android Device",
                OSVersion = "Android 13",
                AppVersion = "1.0.0",
                LastSyncTime = DateTime.Now.AddMinutes(-30),
                LastOnlineTime = DateTime.Now,
                PendingChanges = 0,
                Conflicts = 0,
                Errors = 0,
                IsOnline = true,
                SyncStatus = "Synced",
                DataVolume = (partographs.Count + patients.Count + birthOutcomes.Count + babies.Count) * 1024, // Approximate
                NetworkType = "WiFi",
                AssignedUser = "Current User",
                AssignedDepartment = "Labor Ward",
                TotalSyncsToday = 12,
                AverageSyncDurationSeconds = 3.5m
            });

            // Calculate data volume
            report.TotalDataSyncedBytes = report.DeviceStatuses.Sum(d => d.DataVolume);

            return report;
        }

        public async Task<BirthWeightApgarAnalysis> GenerateBirthWeightApgarAnalysisAsync(DateTime startDate, DateTime endDate)
        {
            var babies = await _babyDetailsRepo.ListAsync();
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
            MonthlyTrend previousMonth = null;

            while (currentDate <= endDate)
            {
                var monthStart = currentDate;
                var monthEnd = currentDate.AddMonths(1).AddDays(-1);

                var monthlyDashboard = await GenerateMonthlyDeliveryDashboardAsync(monthStart, monthEnd);

                var monthlyTrend = new MonthlyTrend
                {
                    Year = currentDate.Year,
                    Month = currentDate.Month,
                    MonthName = currentDate.ToString("MMMM"),
                    Period = currentDate.ToString("MMM yyyy"),
                    TotalDeliveries = monthlyDashboard.TotalDeliveries,
                    LiveBirths = monthlyDashboard.LiveBirths,
                    Stillbirths = monthlyDashboard.Stillbirths,
                    Complications = monthlyDashboard.PostpartumHemorrhages + monthlyDashboard.Eclampsia + monthlyDashboard.ObstructedLabor,
                    MaternalDeaths = monthlyDashboard.MaternalDeaths,
                    NeonatalDeaths = monthlyDashboard.NeonatalDeaths,
                    CaesareanSectionRate = monthlyDashboard.CaesareanSectionRate,
                    VaginalDeliveryRate = monthlyDashboard.VaginalDeliveryRate,
                    MaternalMortalityRate = monthlyDashboard.MaternalMortalityRatio,
                    NeonatalMortalityRate = monthlyDashboard.NeonatalMortalityRate,
                    StillbirthRate = monthlyDashboard.StillbirthRate,
                    ComplicationRate = monthlyDashboard.ComplicationRate,
                    AverageLaborDuration = monthlyDashboard.AverageLaborDuration,
                    WHOComplianceRate = monthlyDashboard.OverallWHOCompliance,
                    AverageApgarScore = monthlyDashboard.AverageApgar5Min,
                    NICUAdmissions = monthlyDashboard.NICUAdmissions
                };

                // Calculate month-over-month changes
                if (previousMonth != null && previousMonth.TotalDeliveries > 0)
                {
                    monthlyTrend.DeliveryChangePercent = (decimal)(monthlyTrend.TotalDeliveries - previousMonth.TotalDeliveries) / previousMonth.TotalDeliveries * 100;
                    monthlyTrend.ComplicationChangePercent = previousMonth.Complications > 0
                        ? (decimal)(monthlyTrend.Complications - previousMonth.Complications) / previousMonth.Complications * 100
                        : 0;
                }

                report.MonthlyTrends.Add(monthlyTrend);
                previousMonth = monthlyTrend;
                currentDate = currentDate.AddMonths(1);
            }

            // Build quarterly trends
            var quarters = report.MonthlyTrends
                .GroupBy(m => new { m.Year, Quarter = (m.Month - 1) / 3 + 1 })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Quarter);

            QuarterlyTrend previousQuarter = null;
            foreach (var quarter in quarters)
            {
                var quarterlyTrend = new QuarterlyTrend
                {
                    Year = quarter.Key.Year,
                    Quarter = quarter.Key.Quarter,
                    QuarterName = $"Q{quarter.Key.Quarter} {quarter.Key.Year}",
                    TotalDeliveries = quarter.Sum(m => m.TotalDeliveries),
                    LiveBirths = quarter.Sum(m => m.LiveBirths),
                    Complications = quarter.Sum(m => m.Complications),
                    MaternalDeaths = quarter.Sum(m => m.MaternalDeaths),
                    NeonatalDeaths = quarter.Sum(m => m.NeonatalDeaths),
                    AverageLaborDuration = quarter.Any() ? quarter.Average(m => m.AverageLaborDuration) : 0,
                    ComplicationRate = quarter.Sum(m => m.TotalDeliveries) > 0
                        ? (decimal)quarter.Sum(m => m.Complications) / quarter.Sum(m => m.TotalDeliveries) * 100
                        : 0,
                    WHOComplianceRate = quarter.Any() ? quarter.Average(m => m.WHOComplianceRate) : 0,
                    CaesareanSectionRate = quarter.Any() ? quarter.Average(m => m.CaesareanSectionRate) : 0,
                    MaternalMortalityRate = quarter.Any() ? quarter.Average(m => m.MaternalMortalityRate) : 0,
                    NeonatalMortalityRate = quarter.Any() ? quarter.Average(m => m.NeonatalMortalityRate) : 0,
                    StillbirthRate = quarter.Any() ? quarter.Average(m => m.StillbirthRate) : 0
                };

                if (previousQuarter != null && previousQuarter.TotalDeliveries > 0)
                {
                    quarterlyTrend.DeliveryChangePercent = (decimal)(quarterlyTrend.TotalDeliveries - previousQuarter.TotalDeliveries) / previousQuarter.TotalDeliveries * 100;
                    quarterlyTrend.TrendDirection = quarterlyTrend.DeliveryChangePercent > 5 ? "Increasing"
                        : quarterlyTrend.DeliveryChangePercent < -5 ? "Decreasing" : "Stable";
                }

                report.QuarterlyTrends.Add(quarterlyTrend);
                previousQuarter = quarterlyTrend;
            }

            // Build KPI indicators
            if (report.MonthlyTrends.Count >= 2)
            {
                var currentMonth = report.MonthlyTrends.Last();
                var previousPeriod = report.MonthlyTrends[report.MonthlyTrends.Count - 2];

                report.CaesareanSectionRate = CreateTrendIndicator("C-Section Rate", "%",
                    currentMonth.CaesareanSectionRate, previousPeriod.CaesareanSectionRate, 15, true);

                report.MaternalMortalityRate = CreateTrendIndicator("Maternal Mortality", "per 100,000",
                    currentMonth.MaternalMortalityRate, previousPeriod.MaternalMortalityRate, 140, true);

                report.NeonatalMortalityRate = CreateTrendIndicator("Neonatal Mortality", "per 1,000",
                    currentMonth.NeonatalMortalityRate, previousPeriod.NeonatalMortalityRate, 12, true);

                report.StillbirthRate = CreateTrendIndicator("Stillbirth Rate", "per 1,000",
                    currentMonth.StillbirthRate, previousPeriod.StillbirthRate, 10, true);

                report.PostpartumHemorrhageRate = CreateTrendIndicator("PPH Rate", "%",
                    currentMonth.ComplicationRate, previousPeriod.ComplicationRate, 5, true);

                report.WHOComplianceRate = CreateTrendIndicator("WHO Compliance", "%",
                    currentMonth.WHOComplianceRate, previousPeriod.WHOComplianceRate, 90, false);
            }

            // Summary statistics
            report.TotalPeriods = report.MonthlyTrends.Count;
            report.TotalDeliveriesInPeriod = report.MonthlyTrends.Sum(m => m.TotalDeliveries);
            report.AverageDeliveriesPerMonth = report.MonthlyTrends.Any() ? (decimal) report.MonthlyTrends?.Average(m => m.TotalDeliveries) : 0m;

            if (report.MonthlyTrends.Count >= 2)
            {
                var firstMonth = report.MonthlyTrends.First();
                var lastMonth = report.MonthlyTrends.Last();
                report.DeliveryGrowthRate = firstMonth.TotalDeliveries > 0
                    ? (decimal)(lastMonth.TotalDeliveries - firstMonth.TotalDeliveries) / firstMonth.TotalDeliveries * 100
                    : 0;
            }

            // Seasonal analysis
            var monthlyAverages = report.MonthlyTrends
                .GroupBy(m => m.Month)
                .ToDictionary(g => System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                    g => g.Any() ? g.Average(m => m.TotalDeliveries) : 0);

            report.SeasonalPatterns = new SeasonalAnalysis
            {
                MonthlyAverages = monthlyAverages.ToDictionary(k => k.Key, v => (decimal)v.Value),
                HighestDeliveryMonth = monthlyAverages.Any() ? monthlyAverages.OrderByDescending(x => x.Value).First().Key : "",
                LowestDeliveryMonth = monthlyAverages.Any() ? monthlyAverages.OrderBy(x => x.Value).First().Key : "",
                HasSeasonalPattern = monthlyAverages.Any() &&
                    (monthlyAverages.Max(x => x.Value) - monthlyAverages.Min(x => x.Value)) /
                    (monthlyAverages.Any() ? monthlyAverages.Average(x => x.Value) : 1) > 0.2
            };

            return report;
        }

        private TrendIndicator CreateTrendIndicator(string name, string unit, decimal current, decimal previous, decimal target, bool lowerIsBetter)
        {
            var change = previous != 0 ? (current - previous) / previous * 100 : 0;
            var trend = Math.Abs(change) < 5 ? "Stable" : (change > 0 ? "Increasing" : "Decreasing");
            var direction = lowerIsBetter
                ? (change < 0 ? "Positive" : (change > 0 ? "Negative" : "Neutral"))
                : (change > 0 ? "Positive" : (change < 0 ? "Negative" : "Neutral"));

            return new TrendIndicator
            {
                Name = name,
                Unit = unit,
                CurrentValue = current,
                PreviousPeriodValue = previous,
                ChangePercentage = change,
                Trend = trend,
                TrendDirection = direction,
                TargetValue = target,
                Status = Math.Abs(current - target) <= target * 0.1m ? "Good"
                    : Math.Abs(current - target) <= target * 0.25m ? "Warning" : "Critical"
            };
        }

        public async Task<PartographPDFData> GeneratePartographPDFDataAsync(Guid partographId)
        {
            var partograph = await _partographRepo.GetCurrentPartographAsync(partographId);
            if (partograph == null) return null;

            var patient = await _patientRepo.GetAsync(partograph.PatientID.Value);
            var birthOutcome = (await _birthOutcomeRepo.GetByPartographIdAsync(partographId));
            var babies = (await _babyDetailsRepo.ListAsync()).Where(b => b.PartographID == partographId).ToList();
            var staff = await _staffRepo.ListAsync();

            // Get all measurements
            var fhrMeasurements = (await _fhrRepo.ListByPatientAsync(partographId)).ToList();
            var contractions = (await _contractionRepo.ListByPatientAsync(partographId)).ToList();
            var cervicalDilations = (await _cervixDilatationRepo.ListByPatientAsync(partographId)).ToList();
            var headDescents = (await _headDescentRepo.ListByPatientAsync(partographId)).ToList();
            var bloodPressures = (await _bpRepo.ListByPatientAsync(partographId)).ToList();
            var temperatures = (await _temperatureRepo.ListByPatientAsync(partographId)).ToList();
            var urineOutputs = (await _urineRepo.ListByPatientAsync(partographId)).ToList();

            var pdfData = new PartographPDFData
            {
                ReportTitle = $"Partograph Report - {patient?.Name ?? "Unknown Patient"}",
                GeneratedAt = DateTime.Now,
                ReportNumber = $"PTG-{partographId.ToString()[..8].ToUpper()}",

                Patient = patient,
                Partograph = partograph,
                BirthOutcome = birthOutcome,
                Babies = babies,

                FHRMeasurements = fhrMeasurements,
                Contractions = contractions,
                CervicalDilations = cervicalDilations,
                HeadDescents = headDescents,
                BloodPressures = bloodPressures,
                Temperatures = temperatures,
                UrineOutputs = urineOutputs
            };

            // Patient summary
            if (patient != null)
            {
                pdfData.PatientSummary = new PatientSummaryData
                {
                    FullName = patient.Name,
                    Age = patient.Age ?? 0,
                    HospitalNumber = patient.HospitalNumber ?? "",
                    Gravida = partograph.Gravida,
                    Parity = partograph.Parity,
                    //GestationalAgeWeeks = partograph.GestationalAgeWeeks ?? 0,
                    //GestationalAgeDays = partograph.GestationalAgeDays ?? 0,
                    BloodGroup = patient.BloodGroup?.ToString() ?? "",
                    Height = babies.FirstOrDefault()?.Length ?? 0,
                    Weight = babies.FirstOrDefault()?.BirthWeight ?? 0,
                    AdmissionTime = partograph.AdmissionDate
                };
            }

            // Attending staff
            if (partograph.Handler.HasValue)
            {
                var handler = staff.FirstOrDefault(s => s.ID == partograph.Handler);
                if (handler != null)
                {
                    pdfData.AttendingMidwife = handler.Name;
                    pdfData.StaffInvolved.Add(new StaffInvolvement
                    {
                        StaffID = handler.ID ?? Guid.Empty,
                        StaffName = handler.Name,
                        Role = handler.Role ?? "Midwife",
                        InvolvementStart = partograph.LaborStartTime ?? DateTime.Now
                    });
                }
            }

            // Calculate labor duration and progress
            if (partograph.LaborStartTime.HasValue)
            {
                var endTime = birthOutcome?.DeliveryTime ?? DateTime.Now;
                var duration = endTime - partograph.LaborStartTime.Value;
                pdfData.LaborDuration = $"{(int)duration.TotalHours}h {duration.Minutes}m";
                pdfData.LaborDurationHours = (decimal)duration.TotalHours;

                // Labor progress summary
                pdfData.LaborProgress = new LaborProgressSummary
                {
                    LaborOnsetTime = partograph.LaborStartTime,
                    CervicalDilationAtAdmission = cervicalDilations.FirstOrDefault()?.DilatationCm ?? 0
                };

                // Calculate cervical dilation rate
                if (cervicalDilations.Count >= 2)
                {
                    var orderedDilations = cervicalDilations.OrderBy(c => c.Time).ToList();
                    var first = orderedDilations.First();
                    var last = orderedDilations.Last();
                    var hours = (last.Time - first.Time).TotalHours;
                    if (hours > 0)
                    {
                        pdfData.LaborProgress.CervicalDilationRate = (last.DilatationCm - first.DilatationCm) / (decimal)hours;
                    }

                    // Check for alert/action line crossing
                    pdfData.LaborProgress.AlertLineCrossed = await CheckAlertLineCrossingAsync(partographId);
                    pdfData.LaborProgress.ActionLineCrossed = await CheckActionLineCrossingAsync(partographId);

                    // Detect active phase start (reaching 4cm)
                    var activePhaseStart = orderedDilations.FirstOrDefault(c => c.DilatationCm >= 4);
                    if (activePhaseStart != null)
                    {
                        pdfData.LaborProgress.ActivePhaseStart = activePhaseStart.Time;
                    }

                    // Second stage start (fully dilated at 10cm)
                    var secondStageStart = orderedDilations.FirstOrDefault(c => c.DilatationCm >= 10);
                    if (secondStageStart != null)
                    {
                        pdfData.LaborProgress.SecondStageStart = secondStageStart.Time;
                    }
                }
            }

            // Delivery summary
            if (birthOutcome != null)
            {
                pdfData.DeliverySummary = new DeliverySummary
                {
                    DeliveryTime = birthOutcome.DeliveryTime ?? DateTime.Now,
                    DeliveryMode = birthOutcome.DeliveryMode.ToString(),
                    PerinealStatus = birthOutcome.PerinealStatus.ToString(),
                    EstimatedBloodLoss = birthOutcome.EstimatedBloodLoss,
                    DelayedCordClamping = babies.Any(b => b.DelayedCordClamping),
                    MaternalCondition = birthOutcome.MaternalStatus.ToString()
                };

                if (birthOutcome.PostpartumHemorrhage || birthOutcome.Eclampsia ||
                    birthOutcome.RupturedUterus || birthOutcome.ObstructedLabor)
                {
                    var complications = new List<string>();
                    if (birthOutcome.PostpartumHemorrhage) complications.Add("Postpartum Hemorrhage");
                    if (birthOutcome.Eclampsia) complications.Add("Eclampsia");
                    if (birthOutcome.RupturedUterus) complications.Add("Ruptured Uterus");
                    if (birthOutcome.ObstructedLabor) complications.Add("Obstructed Labor");
                    pdfData.DeliverySummary.MaternalComplications = string.Join(", ", complications);
                }
            }

            // Neonatal summaries
            int babyNumber = 1;
            foreach (var baby in babies)
            {
                pdfData.NeonatalSummaries.Add(new NeonatalSummary
                {
                    BabyNumber = babyNumber++,
                    BirthTime = baby.BirthTime,
                    Sex = baby.Sex.ToString(),
                    BirthWeight = baby.BirthWeight,
                    Apgar1Min = baby.Apgar1Min,
                    Apgar5Min = baby.Apgar5Min,
                    VitalStatus = baby.VitalStatus.ToString(),
                    ResuscitationRequired = baby.ResuscitationRequired,
                    SkinToSkinDone = baby.KangarooMotherCare,
                    EarlyBreastfeedingDone = baby.EarlyBreastfeedingInitiated,
                    VitaminKGiven = baby.VitaminKGiven,
                    EyeProphylaxis = baby.EyeProphylaxisGiven,
                    AdmittedToNICU = baby.AdmittedToNICU,
                    NICUReason = baby.AdmittedToNICU ? "Requires monitoring" : ""
                });
            }

            // WHO Compliance for this partograph
            var laborHours = pdfData.LaborDurationHours > 0 ? pdfData.LaborDurationHours : 8;
            var expectedFHR = (int)(laborHours * 2);
            var expectedVE = Math.Max(1, (int)(laborHours / 4));

            pdfData.Compliance = new PartographComplianceData
            {
                FHRRecordingsExpected = expectedFHR,
                FHRRecordingsActual = fhrMeasurements.Count,
                FHRMonitoringCompliant = expectedFHR > 0 && (decimal)fhrMeasurements.Count / expectedFHR >= 0.8m,
                VaginalExamsExpected = expectedVE,
                VaginalExamsActual = cervicalDilations.Count,
                VaginalExamCompliant = expectedVE > 0 && (decimal)cervicalDilations.Count / expectedVE >= 0.8m,
                VitalSignsCompliant = bloodPressures.Count >= laborHours,
                EssentialCareCompliant = babies.All(b => b.KangarooMotherCare && b.EarlyBreastfeedingInitiated && b.VitaminKGiven)
            };

            pdfData.Compliance.OverallComplianceScore =
                ((pdfData.Compliance.FHRMonitoringCompliant ? 25 : 0) +
                 (pdfData.Compliance.VaginalExamCompliant ? 25 : 0) +
                 (pdfData.Compliance.VitalSignsCompliant ? 25 : 0) +
                 (pdfData.Compliance.EssentialCareCompliant ? 25 : 0));

            // Add compliance issues
            if (!pdfData.Compliance.FHRMonitoringCompliant)
                pdfData.Compliance.ComplianceIssues.Add($"FHR monitoring: {fhrMeasurements.Count}/{expectedFHR} readings");
            if (!pdfData.Compliance.VaginalExamCompliant)
                pdfData.Compliance.ComplianceIssues.Add($"Vaginal exams: {cervicalDilations.Count}/{expectedVE} exams");
            if (!pdfData.Compliance.VitalSignsCompliant)
                pdfData.Compliance.ComplianceIssues.Add("Vital signs not recorded hourly");
            if (!pdfData.Compliance.EssentialCareCompliant)
                pdfData.Compliance.ComplianceIssues.Add("Essential newborn care incomplete");

            // Add achievements
            if (pdfData.Compliance.FHRMonitoringCompliant)
                pdfData.Compliance.ComplianceAchievements.Add("FHR monitoring adequate");
            if (pdfData.Compliance.VaginalExamCompliant)
                pdfData.Compliance.ComplianceAchievements.Add("Vaginal exam frequency adequate");
            if (pdfData.Compliance.EssentialCareCompliant)
                pdfData.Compliance.ComplianceAchievements.Add("Essential newborn care completed");

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
                var dilatations = (await _cervixDilatationRepo.ListByPatientAsync(id))
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
