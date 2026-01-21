using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Data
{
    /// <summary>
    /// Robson Classification Data Seeder
    /// Classifies existing completed deliveries using the WHO Robson Classification System (2017)
    /// Reference: WHO Robson Classification: Implementation Manual (ISBN 978-92-4-151319-7)
    /// </summary>
    public class RobsonDataSeeder
    {
        private readonly PartographDbContext _context;
        private readonly ILogger<RobsonDataSeeder> _logger;

        public RobsonDataSeeder(PartographDbContext context, ILogger<RobsonDataSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Classify all existing completed deliveries that haven't been classified yet
        /// </summary>
        public async Task ClassifyExistingDeliveriesAsync()
        {
            try
            {
                _logger.LogInformation("Starting Robson classification of existing deliveries...");

                // Get completed partographs without classification
                var unclassifiedPartographs = await _context.Partographs
                    .Include(p => p.Patient)
                    .Include(p => p.FetalPositions)
                    .Where(p => p.Status == LaborStatus.Completed)
                    .Where(p => !_context.RobsonClassifications.Any(r => r.PartographID == p.ID))
                    .ToListAsync();

                _logger.LogInformation($"Found {unclassifiedPartographs.Count} unclassified completed deliveries");

                if (unclassifiedPartographs.Count == 0)
                {
                    _logger.LogInformation("No deliveries to classify.");
                    return;
                }

                int classifiedCount = 0;
                int failedCount = 0;

                foreach (var partograph in unclassifiedPartographs)
                {
                    try
                    {
                        var classification = await ClassifyPartographAsync(partograph);
                        if (classification != null)
                        {
                            _context.RobsonClassifications.Add(classification);
                            classifiedCount++;
                        }
                        else
                        {
                            failedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to classify partograph {partograph.ID}");
                        failedCount++;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Robson classification complete: {classifiedCount} classified, {failedCount} failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Robson classification seeding");
                throw;
            }
        }

        /// <summary>
        /// Classify a single partograph and return the classification
        /// </summary>
        private async Task<RobsonClassification?> ClassifyPartographAsync(Partograph partograph)
        {
            if (partograph.ID == null) return null;

            // Get birth outcome for delivery mode
            var birthOutcome = await _context.BirthOutcomes
                .FirstOrDefaultAsync(b => b.PartographID == partograph.ID);

            var babyDetails = await _context.BabyDetails
                .Where(b => b.PartographID == partograph.ID)
                .ToListAsync();

            var now = DateTime.Now;
            var nowTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var classification = new RobsonClassification
            {
                ID = Guid.NewGuid(),
                PartographID = partograph.ID,
                ClassifiedAt = partograph.DeliveryTime ?? now,

                // Classification criteria from partograph data
                Parity = partograph.Parity,
                HasPreviousCesarean = partograph.Patient?.HasPreviousCSection ?? false,
                NumberOfPreviousCesareans = partograph.Patient?.NumberOfPreviousCsections ?? 0,
                GestationalAgeWeeks = GetGestationalWeeks(partograph),
                LaborOnset = partograph.LaborOnset,
                FetalPresentation = GetFetalPresentation(partograph),
                NumberOfFetuses = GetNumberOfFetuses(birthOutcome, babyDetails),
                DeliveryMode = GetDeliveryMode(birthOutcome),

                // Recording info
                ClassifiedBy = "System (Auto)",
                IsAutoClassified = true,
                ValidationStatus = ClassificationValidationStatus.Validated,
                Notes = "Auto-classified during system initialization",

                // Sync columns
                CreatedTime = nowTimestamp,
                UpdatedTime = nowTimestamp,
                DeviceId = "SERVER",
                OriginDeviceId = "SERVER",
                SyncStatus = 1, // Synced
                Version = 1,
                ServerVersion = 1,
                Deleted = 0,
                ConflictData = string.Empty,
                DataHash = string.Empty
            };

            // Calculate the Robson group
            classification.CalculateRobsonGroup();

            // Set CS indication if cesarean
            if (classification.IsCesareanSection && birthOutcome != null)
            {
                classification.CesareanIndication = GetCSIndication(birthOutcome, partograph);
                classification.CesareanType = DetermineCSType(partograph, birthOutcome);
            }

            // Update hash
            classification.UpdateDataHash();

            return classification;
        }

        /// <summary>
        /// Get gestational age in weeks from partograph
        /// </summary>
        private int GetGestationalWeeks(Partograph partograph)
        {
            if (partograph.ExpectedDeliveryDate.HasValue)
            {
                // Calculate from EDD (40 weeks from LMP)
                var deliveryDate = partograph.DeliveryTime ?? DateTime.Now;
                var eddDate = partograph.ExpectedDeliveryDate.Value.ToDateTime(TimeOnly.MinValue);
                var daysToEdd = (eddDate - deliveryDate).Days;
                var weeksAtDelivery = 40 - (daysToEdd / 7);

                // Ensure reasonable range
                return Math.Clamp(weeksAtDelivery, 22, 45);
            }

            if (partograph.LastMenstrualDate.HasValue)
            {
                // Calculate from LMP
                var deliveryDate = partograph.DeliveryTime ?? DateTime.Now;
                var lmpDate = partograph.LastMenstrualDate.Value.ToDateTime(TimeOnly.MinValue);
                var daysSinceLmp = (deliveryDate - lmpDate).Days;
                var weeks = daysSinceLmp / 7;

                return Math.Clamp(weeks, 22, 45);
            }

            // Default to term if no data
            return 40;
        }

        /// <summary>
        /// Get fetal presentation from partograph
        /// </summary>
        private FetalPresentationType GetFetalPresentation(Partograph partograph)
        {
            var latestPosition = partograph.FetalPositions?
                .OrderByDescending(f => f.Time)
                .FirstOrDefault();

            if (latestPosition == null) return FetalPresentationType.Cephalic;

            return (latestPosition.Presentation?.ToLower()) switch
            {
                "breech" => FetalPresentationType.Breech,
                "completebreech" => FetalPresentationType.Breech,
                "frankbreech" => FetalPresentationType.Breech,
                "footlingbreech" => FetalPresentationType.Breech,
                "transverse" => FetalPresentationType.Transverse,
                "shoulder" => FetalPresentationType.Transverse,
                "oblique" => FetalPresentationType.Oblique,
                "face" => FetalPresentationType.Cephalic, // Face is still cephalic
                "brow" => FetalPresentationType.Cephalic, // Brow is still cephalic
                "vertex" => FetalPresentationType.Cephalic,
                _ => FetalPresentationType.Cephalic
            };
        }

        /// <summary>
        /// Get number of fetuses
        /// </summary>
        private int GetNumberOfFetuses(BirthOutcome? birthOutcome, List<BabyDetails> babyDetails)
        {
            if (birthOutcome?.NumberOfBabies > 0)
                return birthOutcome.NumberOfBabies;

            if (babyDetails.Count > 0)
                return babyDetails.Count;

            return 1; // Default to singleton
        }

        /// <summary>
        /// Get delivery mode from birth outcome
        /// </summary>
        private DeliveryMode GetDeliveryMode(BirthOutcome? outcome)
        {
            if (outcome == null) return DeliveryMode.SpontaneousVaginal;

            return (outcome?.DeliveryMode.ToString().ToLower()) switch
            {
                "caesareansection" => DeliveryMode.CaesareanSection,
                "cesarean" => DeliveryMode.CaesareanSection,
                "cs" => DeliveryMode.CaesareanSection,
                "c-section" => DeliveryMode.CaesareanSection,
                "assistedvaginal" => DeliveryMode.AssistedVaginal,
                "vacuum" => DeliveryMode.AssistedVaginal,
                "forceps" => DeliveryMode.AssistedVaginal,
                "instrumental" => DeliveryMode.AssistedVaginal,
                "breechdelivery" => DeliveryMode.BreechDelivery,
                "breech" => DeliveryMode.BreechDelivery,
                "spontaneousvaginal" => DeliveryMode.SpontaneousVaginal,
                "svd" => DeliveryMode.SpontaneousVaginal,
                "normal" => DeliveryMode.SpontaneousVaginal,
                "vaginal" => DeliveryMode.SpontaneousVaginal,
                _ => DeliveryMode.SpontaneousVaginal
            };
        }

        /// <summary>
        /// Get CS indication from birth outcome and partograph data
        /// </summary>
        private string GetCSIndication(BirthOutcome birthOutcome, Partograph partograph)
        {
            var indications = new List<string>();

            // Check for common indications in diagnosis or notes
            if (!string.IsNullOrEmpty(birthOutcome.DeliveryModeDetails))
            {
                indications.Add(birthOutcome.DeliveryModeDetails);
            }

            // Check partograph complications
            if (!string.IsNullOrEmpty(partograph.Complications))
            {
                indications.Add(partograph.Complications);
            }

            // Check for common patterns
            if (partograph.Patient?.HasPreviousCSection == true)
            {
                indications.Add("Previous cesarean section");
            }

            if (indications.Count == 0)
            {
                return "Not specified";
            }

            return string.Join("; ", indications.Distinct());
        }

        /// <summary>
        /// Determine if CS was emergency or elective
        /// </summary>
        private CesareanType? DetermineCSType(Partograph partograph, BirthOutcome birthOutcome)
        {
            // If labor was entered (status progressed beyond Pending), likely emergency
            if (partograph.Status != LaborStatus.Pending && partograph.LaborStartTime.HasValue)
            {
                return CesareanType.Emergency;
            }

            // If LaborOnset is CesareanBeforeLabor, it's elective
            if (partograph.LaborOnset == LaborOnsetType.CesareanBeforeLabor)
            {
                return CesareanType.Elective;
            }

            // Check delivery mode details for clues
            var details = birthOutcome.DeliveryModeDetails?.ToLower() ?? "";
            if (details.Contains("emergency") || details.Contains("urgent"))
            {
                return CesareanType.Emergency;
            }
            if (details.Contains("elective") || details.Contains("planned") || details.Contains("scheduled"))
            {
                return CesareanType.Elective;
            }

            // Default to emergency if labor started
            if (partograph.LaborStartTime.HasValue)
            {
                return CesareanType.Emergency;
            }

            return null;
        }

        /// <summary>
        /// Generate sample Robson data for testing/demo purposes
        /// </summary>
        public async Task GenerateSampleDataAsync(int count = 100)
        {
            _logger.LogInformation($"Generating {count} sample Robson classifications...");

            var random = new Random(42); // Fixed seed for reproducibility
            var now = DateTime.Now;
            var nowTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var classifications = new List<RobsonClassification>();

            // Distribution based on typical Robson group sizes
            var groupDistribution = new Dictionary<RobsonGroup, (int count, decimal csRate)>
            {
                { RobsonGroup.Group1, (35, 0.08m) },   // 35%, 8% CS rate
                { RobsonGroup.Group2, (10, 0.35m) },   // 10%, 35% CS rate
                { RobsonGroup.Group3, (30, 0.02m) },   // 30%, 2% CS rate
                { RobsonGroup.Group4, (5, 0.18m) },    // 5%, 18% CS rate
                { RobsonGroup.Group5, (8, 0.70m) },    // 8%, 70% CS rate
                { RobsonGroup.Group6, (2, 0.85m) },    // 2%, 85% CS rate
                { RobsonGroup.Group7, (1, 0.80m) },    // 1%, 80% CS rate
                { RobsonGroup.Group8, (2, 0.55m) },    // 2%, 55% CS rate
                { RobsonGroup.Group9, (1, 1.00m) },    // 1%, 100% CS rate
                { RobsonGroup.Group10, (6, 0.25m) }    // 6%, 25% CS rate
            };

            foreach (var kvp in groupDistribution)
            {
                var group = kvp.Key;
                var groupCount = (int)(count * kvp.Value.count / 100.0);
                var csRate = kvp.Value.csRate;

                for (int i = 0; i < groupCount; i++)
                {
                    var isCS = random.NextDouble() < (double)csRate;
                    var daysAgo = random.Next(1, 365);
                    var classifiedAt = now.AddDays(-daysAgo);

                    var classification = CreateSampleClassification(
                        group, isCS, classifiedAt, random, nowTimestamp);

                    classifications.Add(classification);
                }
            }

            _context.RobsonClassifications.AddRange(classifications);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Generated {classifications.Count} sample Robson classifications");
        }

        private RobsonClassification CreateSampleClassification(
            RobsonGroup group,
            bool isCesarean,
            DateTime classifiedAt,
            Random random,
            long nowTimestamp)
        {
            var (parity, hasPrevCS, gestWeeks, laborOnset, presentation, numFetuses) =
                GetSampleDataForGroup(group, random);

            var classification = new RobsonClassification
            {
                ID = Guid.NewGuid(),
                PartographID = null, // Sample data without partograph link
                ClassifiedAt = classifiedAt,

                Parity = parity,
                HasPreviousCesarean = hasPrevCS,
                NumberOfPreviousCesareans = hasPrevCS ? random.Next(1, 3) : 0,
                GestationalAgeWeeks = gestWeeks,
                LaborOnset = laborOnset,
                FetalPresentation = presentation,
                NumberOfFetuses = numFetuses,

                Group = group,
                DeliveryMode = isCesarean ? DeliveryMode.CaesareanSection : DeliveryMode.SpontaneousVaginal,
                CesareanIndication = isCesarean ? GetSampleCSIndication(group, random) : string.Empty,
                CesareanType = isCesarean ? (random.Next(2) == 0 ? CesareanType.Emergency : CesareanType.Elective) : null,

                ClassifiedBy = "Sample Data Generator",
                IsAutoClassified = true,
                ValidationStatus = ClassificationValidationStatus.Validated,
                Notes = "Sample data for demonstration",

                CreatedTime = nowTimestamp,
                UpdatedTime = nowTimestamp,
                DeviceId = "SAMPLE",
                OriginDeviceId = "SAMPLE",
                SyncStatus = 1,
                Version = 1,
                ServerVersion = 1,
                Deleted = 0,
                ConflictData = string.Empty,
                DataHash = string.Empty
            };

            classification.UpdateDataHash();
            return classification;
        }

        private (int parity, bool hasPrevCS, int gestWeeks, LaborOnsetType laborOnset,
            FetalPresentationType presentation, int numFetuses) GetSampleDataForGroup(RobsonGroup group, Random random)
        {
            return group switch
            {
                RobsonGroup.Group1 => (0, false, random.Next(37, 42), LaborOnsetType.Spontaneous, FetalPresentationType.Cephalic, 1),
                RobsonGroup.Group2 => (0, false, random.Next(37, 42),
                    random.Next(2) == 0 ? LaborOnsetType.Induced : LaborOnsetType.CesareanBeforeLabor,
                    FetalPresentationType.Cephalic, 1),
                RobsonGroup.Group3 => (random.Next(1, 5), false, random.Next(37, 42), LaborOnsetType.Spontaneous, FetalPresentationType.Cephalic, 1),
                RobsonGroup.Group4 => (random.Next(1, 5), false, random.Next(37, 42),
                    random.Next(2) == 0 ? LaborOnsetType.Induced : LaborOnsetType.CesareanBeforeLabor,
                    FetalPresentationType.Cephalic, 1),
                RobsonGroup.Group5 => (random.Next(1, 5), true, random.Next(37, 42),
                    (LaborOnsetType)random.Next(3), FetalPresentationType.Cephalic, 1),
                RobsonGroup.Group6 => (0, random.Next(2) == 0, random.Next(37, 42),
                    (LaborOnsetType)random.Next(3), FetalPresentationType.Breech, 1),
                RobsonGroup.Group7 => (random.Next(1, 5), random.Next(2) == 0, random.Next(37, 42),
                    (LaborOnsetType)random.Next(3), FetalPresentationType.Breech, 1),
                RobsonGroup.Group8 => (random.Next(0, 5), random.Next(2) == 0, random.Next(37, 42),
                    (LaborOnsetType)random.Next(3), FetalPresentationType.Cephalic, random.Next(2, 4)),
                RobsonGroup.Group9 => (random.Next(0, 5), random.Next(2) == 0, random.Next(37, 42),
                    (LaborOnsetType)random.Next(3),
                    random.Next(2) == 0 ? FetalPresentationType.Transverse : FetalPresentationType.Oblique, 1),
                RobsonGroup.Group10 => (random.Next(0, 5), random.Next(2) == 0, random.Next(28, 37),
                    (LaborOnsetType)random.Next(3), FetalPresentationType.Cephalic, 1),
                _ => (0, false, 40, LaborOnsetType.Spontaneous, FetalPresentationType.Cephalic, 1)
            };
        }

        private string GetSampleCSIndication(RobsonGroup group, Random random)
        {
            var indications = group switch
            {
                RobsonGroup.Group1 => new[] { "Fetal distress", "Failure to progress", "Failed induction", "Cephalopelvic disproportion" },
                RobsonGroup.Group2 => new[] { "Failed induction", "Fetal distress", "Maternal request", "Elective" },
                RobsonGroup.Group3 => new[] { "Fetal distress", "Uterine rupture risk", "Placental abruption" },
                RobsonGroup.Group4 => new[] { "Failed induction", "Fetal distress", "Previous uterine surgery" },
                RobsonGroup.Group5 => new[] { "Previous cesarean", "Patient choice ERCS", "Declined VBAC", "Uterine rupture risk" },
                RobsonGroup.Group6 => new[] { "Breech presentation", "Failed ECV", "Patient choice" },
                RobsonGroup.Group7 => new[] { "Breech presentation", "Previous CS with breech", "Failed ECV" },
                RobsonGroup.Group8 => new[] { "Multiple pregnancy", "Fetal distress", "Malpresentation" },
                RobsonGroup.Group9 => new[] { "Transverse/oblique lie", "Cord prolapse risk", "Placenta previa" },
                RobsonGroup.Group10 => new[] { "Preterm with complications", "Fetal distress", "IUGR" },
                _ => new[] { "Not specified" }
            };

            return indications[random.Next(indications.Length)];
        }
    }
}
