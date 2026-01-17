using MAAME.DROMO.PARTOGRAPH.MODEL;
using MAAME.DROMO.PARTOGRAPH.SERVICE.Models;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Data
{
    public class PartographDbContext : DbContext
    {
        public PartographDbContext(DbContextOptions<PartographDbContext> options)
            : base(options)
        {
        }

        // Core Entities
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Partograph> Partographs { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Facility> Facilities { get; set; }

        // Partograph Measurements
        public DbSet<FHR> FHRs { get; set; }
        public DbSet<Contraction> Contractions { get; set; }
        public DbSet<CervixDilatation> CervixDilatations { get; set; }
        public DbSet<HeadDescent> HeadDescents { get; set; }
        public DbSet<BP> BPs { get; set; }
        public DbSet<Temperature> Temperatures { get; set; }
        public DbSet<AmnioticFluid> AmnioticFluids { get; set; }
        public DbSet<Urine> Urines { get; set; }
        public DbSet<Caput> Caputs { get; set; }
        public DbSet<Moulding> Mouldings { get; set; }
        public DbSet<FetalPosition> FetalPositions { get; set; }
        public DbSet<PainReliefEntry> PainReliefs { get; set; }
        public DbSet<PostureEntry> Postures { get; set; }
        public DbSet<OralFluidEntry> OralFluids { get; set; }
        public DbSet<IVFluidEntry> IVFluids { get; set; }
        public DbSet<MedicationEntry> Medications { get; set; }
        public DbSet<Oxytocin> Oxytocins { get; set; }
        public DbSet<CompanionEntry> Companions { get; set; }
        public DbSet<Assessment> Assessments { get; set; } 
        public DbSet<MedicalNote> MedicalNotes { get; set; }

        // Extended Partograph Measurements
        public DbSet<FourthStageVitals> FourthStageVitals { get; set; }
        public DbSet<BishopScore> BishopScores { get; set; }
        public DbSet<PartographDiagnosis> PartographDiagnoses { get; set; }
        public DbSet<PartographRiskFactor> PartographRiskFactors { get; set; }
        public DbSet<Plan> Plans { get; set; }

        // Birth Outcome and Referral Entities
        public DbSet<BirthOutcome> BirthOutcomes { get; set; }
        public DbSet<BabyDetails> BabyDetails { get; set; }
        public DbSet<Referral> Referrals { get; set; }

        // Analytics Tables (for external web application interface)
        public DbSet<DailyFacilityStats> DailyFacilityStats { get; set; }
        public DbSet<MonthlyFacilityStats> MonthlyFacilityStats { get; set; }
        public DbSet<DeliveryOutcomeSummary> DeliveryOutcomeSummaries { get; set; }
        public DbSet<MaternalMortalityRecord> MaternalMortalityRecords { get; set; }
        public DbSet<NeonatalOutcomeRecord> NeonatalOutcomeRecords { get; set; }
        public DbSet<ComplicationAnalytics> ComplicationAnalytics { get; set; }
        public DbSet<ReferralAnalytics> ReferralAnalytics { get; set; }
        public DbSet<LaborProgressAnalytics> LaborProgressAnalytics { get; set; }
        public DbSet<FacilityPerformanceSnapshot> FacilityPerformanceSnapshots { get; set; }
        public DbSet<AlertSummary> AlertSummaries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Patient
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("Tbl_Patient");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.HospitalNumber).HasMaxLength(50);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);
                entity.Property(e => e.Deleted).HasDefaultValue(0);

                // Configure indexes for sync
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
                entity.HasIndex(e => e.ServerVersion);
                entity.HasIndex(e => e.DeviceId);

                // Note: Relationship configured from Partograph side to avoid duplicate configuration
            });

            // Configure Partograph
            modelBuilder.Entity<Partograph>(entity =>
            {
                entity.ToTable("Tbl_Partograph");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);
                entity.Property(e => e.Deleted).HasDefaultValue(0);

                // Configure indexes for sync
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
                entity.HasIndex(e => e.ServerVersion);
                entity.HasIndex(e => e.PatientID);

                // Foreign key to Patient - explicit principal key to avoid shadow property
                entity.HasOne(p => p.Patient)
                    .WithMany(p => p.PartographEntries)
                    .HasForeignKey(p => p.PatientID)
                    .HasPrincipalKey(p => p.ID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Staff
            modelBuilder.Entity<Staff>(entity =>
            {
                entity.ToTable("Tbl_Staff");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.FacilityName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StaffID).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);
                entity.Property(e => e.Deleted).HasDefaultValue(0);

                // Configure indexes
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.StaffID);
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
            });

            // Configure Facility
            modelBuilder.Entity<Facility>(entity =>
            {
                entity.ToTable("Tbl_Facility");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Code).HasMaxLength(50);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);
                entity.Property(e => e.Deleted).HasDefaultValue(0);

                // Configure indexes
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
                entity.HasIndex(e => e.Region);
            });

            // Configure measurement types with proper navigation property references
            // FHR - links to Partograph.Fhrs
            ConfigureMeasurementWithNavigation<FHR>(modelBuilder, "Tbl_FHR", p => p.Fhrs);

            // Contraction - links to Partograph.Contractions
            ConfigureMeasurementWithNavigation<Contraction>(modelBuilder, "Tbl_Contraction", p => p.Contractions);
            // Add decimal precision for Contraction
            modelBuilder.Entity<Contraction>().Property(e => e.AverageIntervalMinutes).HasPrecision(10, 2);

            // CervixDilatation - links to Partograph.Dilatations
            ConfigureMeasurementWithNavigation<CervixDilatation>(modelBuilder, "Tbl_CervixDilatation", p => p.Dilatations);
            // Add decimal precision for CervixDilatation
            modelBuilder.Entity<CervixDilatation>().Property(e => e.CervicalLengthCm).HasPrecision(5, 2);
            modelBuilder.Entity<CervixDilatation>().Property(e => e.DilatationRateCmPerHour).HasPrecision(5, 2);

            // HeadDescent - links to Partograph.HeadDescents
            ConfigureMeasurementWithNavigation<HeadDescent>(modelBuilder, "Tbl_HeadDescent", p => p.HeadDescents);

            // BP - links to Partograph.BPs
            ConfigureMeasurementWithNavigation<BP>(modelBuilder, "Tbl_BP", p => p.BPs);

            // Temperature - links to Partograph.Temperatures
            ConfigureMeasurementWithNavigation<Temperature>(modelBuilder, "Tbl_Temperature", p => p.Temperatures);

            // AmnioticFluid - links to Partograph.AmnioticFluids
            ConfigureMeasurementWithNavigation<AmnioticFluid>(modelBuilder, "Tbl_AmnioticFluid", p => p.AmnioticFluids);

            // Urine - links to Partograph.Urines
            ConfigureMeasurementWithNavigation<Urine>(modelBuilder, "Tbl_Urine", p => p.Urines);
            // Add decimal precision for Urine
            modelBuilder.Entity<Urine>().Property(e => e.HourlyOutputRate).HasPrecision(10, 2);

            // Caput - links to Partograph.Caputs
            ConfigureMeasurementWithNavigation<Caput>(modelBuilder, "Tbl_Caput", p => p.Caputs);

            // Moulding - links to Partograph.Mouldings
            ConfigureMeasurementWithNavigation<Moulding>(modelBuilder, "Tbl_Moulding", p => p.Mouldings);

            // FetalPosition - links to Partograph.FetalPositions
            ConfigureMeasurementWithNavigation<FetalPosition>(modelBuilder, "Tbl_FetalPosition", p => p.FetalPositions);

            // PainReliefEntry - links to Partograph.PainReliefs
            ConfigureMeasurementWithNavigation<PainReliefEntry>(modelBuilder, "Tbl_PainReliefEntry", p => p.PainReliefs);

            // PostureEntry - links to Partograph.Postures
            ConfigureMeasurementWithNavigation<PostureEntry>(modelBuilder, "Tbl_Posture", p => p.Postures);

            // OralFluidEntry - links to Partograph.OralFluids
            ConfigureMeasurementWithNavigation<OralFluidEntry>(modelBuilder, "Tbl_OralFluid", p => p.OralFluids);

            // IVFluidEntry - links to Partograph.IVFluids
            ConfigureMeasurementWithNavigation<IVFluidEntry>(modelBuilder, "Tbl_IVFluid", p => p.IVFluids);
            // Add decimal precision for IVFluidEntry
            modelBuilder.Entity<IVFluidEntry>().Property(e => e.RateMlPerHour).HasPrecision(10, 2);

            // MedicationEntry - links to Partograph.Medications
            ConfigureMeasurementWithNavigation<MedicationEntry>(modelBuilder, "Tbl_Medication", p => p.Medications);

            // Oxytocin - links to Partograph.Oxytocins
            ConfigureMeasurementWithNavigation<Oxytocin>(modelBuilder, "Tbl_Oxytocin", p => p.Oxytocins);
            // Add decimal precision for Oxytocin
            modelBuilder.Entity<Oxytocin>().Property(e => e.ConcentrationMUnitsPerMl).HasPrecision(10, 4);
            modelBuilder.Entity<Oxytocin>().Property(e => e.DoseMUnitsPerMin).HasPrecision(10, 4);
            modelBuilder.Entity<Oxytocin>().Property(e => e.InfusionRateMlPerHour).HasPrecision(10, 2);
            modelBuilder.Entity<Oxytocin>().Property(e => e.TotalVolumeInfused).HasPrecision(10, 2);

            // CompanionEntry - links to Partograph.Companions
            ConfigureMeasurementWithNavigation<CompanionEntry>(modelBuilder, "Tbl_Companion", p => p.Companions);

            // Assessment - links to Partograph.Assessments
            ConfigureMeasurementWithNavigation<Assessment>(modelBuilder, "Tbl_Assessment", p => p.Assessments);

            // MedicalNote - no navigation property on Partograph, use basic configuration
            ConfigureMeasurement<MedicalNote>(modelBuilder, "Tbl_MedicalNote");

            // Configure extended measurement types
            ConfigureMeasurement<FourthStageVitals>(modelBuilder, "Tbl_FourthStageVitals");

            // BishopScore - links to Partograph.BishopScores
            ConfigureMeasurementWithNavigation<BishopScore>(modelBuilder, "Tbl_BishopScore", p => p.BishopScores);

            // PartographDiagnosis - links to Partograph.Diagnoses (IEnumerable)
            ConfigureMeasurement<PartographDiagnosis>(modelBuilder, "Tbl_PartographDiagnosis");
            // Ignore the navigation property as it's IEnumerable, not a proper collection
            modelBuilder.Entity<Partograph>().Ignore(p => p.Diagnoses);

            // PartographRiskFactor - links to Partograph.RiskFactors (IEnumerable)
            ConfigureMeasurement<PartographRiskFactor>(modelBuilder, "Tbl_PartographRiskFactor");
            // Ignore the navigation property as it's IEnumerable, not a proper collection
            modelBuilder.Entity<Partograph>().Ignore(p => p.RiskFactors);

            // Plan - links to Partograph.Plans
            ConfigureMeasurementWithNavigation<Plan>(modelBuilder, "Tbl_Plan", p => p.Plans);

            // Configure BirthOutcome
            modelBuilder.Entity<BirthOutcome>(entity =>
            {
                entity.ToTable("Tbl_BirthOutcome");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);
                entity.Property(e => e.Deleted).HasDefaultValue(0);

                // Configure indexes
                entity.HasIndex(e => e.PartographID);
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
                entity.HasIndex(e => e.ServerVersion);
                entity.HasIndex(e => e.RecordedTime);

                // Foreign key to Partograph - explicit principal key to avoid shadow property
                entity.HasOne(e => e.Partograph)
                    .WithMany()
                    .HasForeignKey(e => e.PartographID)
                    .HasPrincipalKey(p => p.ID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure BabyDetails
            modelBuilder.Entity<BabyDetails>(entity =>
            {
                entity.ToTable("Tbl_BabyDetails");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);
                entity.Property(e => e.Deleted).HasDefaultValue(0);

                // Configure decimal precision for BabyDetails
                entity.Property(e => e.BirthWeight).HasPrecision(10, 2);
                entity.Property(e => e.Length).HasPrecision(5, 2);
                entity.Property(e => e.HeadCircumference).HasPrecision(5, 2);
                entity.Property(e => e.ChestCircumference).HasPrecision(5, 2);
                entity.Property(e => e.FirstTemperature).HasPrecision(4, 1);

                // Configure indexes
                entity.HasIndex(e => e.PartographID);
                entity.HasIndex(e => e.BirthOutcomeID);
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
                entity.HasIndex(e => e.ServerVersion);
                entity.HasIndex(e => e.BirthTime);

                // Foreign key to Partograph - explicit principal key to avoid shadow property
                entity.HasOne(e => e.Partograph)
                    .WithMany()
                    .HasForeignKey(e => e.PartographID)
                    .HasPrincipalKey(p => p.ID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Foreign key to BirthOutcome - explicit principal key to avoid shadow property
                entity.HasOne(e => e.BirthOutcome)
                    .WithMany()
                    .HasForeignKey(e => e.BirthOutcomeID)
                    .HasPrincipalKey(b => b.ID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Referral
            modelBuilder.Entity<Referral>(entity =>
            {
                entity.ToTable("Tbl_Referral");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);
                entity.Property(e => e.Deleted).HasDefaultValue(0);

                // Configure decimal precision for Referral
                entity.Property(e => e.MaternalTemperature).HasPrecision(4, 1);

                // Configure indexes
                entity.HasIndex(e => e.PartographID);
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
                entity.HasIndex(e => e.ServerVersion);
                entity.HasIndex(e => e.ReferralTime);
                entity.HasIndex(e => e.Status);

                // Foreign key to Partograph - explicit principal key to avoid shadow property
                entity.HasOne(e => e.Partograph)
                    .WithMany()
                    .HasForeignKey(e => e.PartographID)
                    .HasPrincipalKey(p => p.ID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Analytics Tables
            ConfigureAnalyticsTables(modelBuilder);

            // Configure Global Query Filters for Soft Deletes
            // These filters automatically exclude soft-deleted records from all queries
            // Use .IgnoreQueryFilters() to include deleted records when needed
            ConfigureSoftDeleteFilters(modelBuilder);

            // Ignore types that are defined but not intended for database persistence
            // EnhancedVitalSignEntry is defined in BasePartographMeasurement.cs but not used as a DbSet
            modelBuilder.Ignore<EnhancedVitalSignEntry>();
        }

        /// <summary>
        /// Configures global query filters to automatically exclude soft-deleted records.
        /// This ensures that deleted records are never accidentally returned in queries.
        /// To include deleted records, use .IgnoreQueryFilters() on the query.
        /// </summary>
        private void ConfigureSoftDeleteFilters(ModelBuilder modelBuilder)
        {
            // Core entities
            modelBuilder.Entity<Patient>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Partograph>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Staff>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Facility>().HasQueryFilter(e => e.Deleted == 0);

            // Partograph Measurements
            modelBuilder.Entity<FHR>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Contraction>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<CervixDilatation>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<HeadDescent>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<BP>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Temperature>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<AmnioticFluid>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Urine>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Caput>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Moulding>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<FetalPosition>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<PainReliefEntry>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<PostureEntry>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<OralFluidEntry>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<IVFluidEntry>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<MedicationEntry>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Oxytocin>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<CompanionEntry>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Assessment>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<MedicalNote>().HasQueryFilter(e => e.Deleted == 0);

            // Extended Partograph Measurements
            modelBuilder.Entity<FourthStageVitals>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<BishopScore>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<PartographDiagnosis>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<PartographRiskFactor>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Plan>().HasQueryFilter(e => e.Deleted == 0);

            // Birth Outcome and Referral Entities
            modelBuilder.Entity<BirthOutcome>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<BabyDetails>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Referral>().HasQueryFilter(e => e.Deleted == 0);
        }

        private void ConfigureMeasurement<T>(ModelBuilder modelBuilder, string tableName) where T : BasePartographMeasurement
        {
            modelBuilder.Entity<T>(entity =>
            {
                entity.ToTable(tableName);
                entity.HasKey(e => e.ID);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);
                entity.Property(e => e.Deleted).HasDefaultValue(0);

                // Configure indexes for sync and queries
                entity.HasIndex(e => e.PartographID);
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
                entity.HasIndex(e => e.ServerVersion);
                entity.HasIndex(e => e.Time);

                // Foreign key to Partograph - explicit principal key to avoid shadow property
                entity.HasOne<Partograph>()
                    .WithMany()
                    .HasForeignKey(e => e.PartographID)
                    .HasPrincipalKey(p => p.ID)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureMeasurementWithNavigation<T>(ModelBuilder modelBuilder, string tableName, System.Linq.Expressions.Expression<Func<Partograph, IEnumerable<T>?>> navigationProperty) where T : BasePartographMeasurement
        {
            modelBuilder.Entity<T>(entity =>
            {
                entity.ToTable(tableName);
                entity.HasKey(e => e.ID);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);
                entity.Property(e => e.Deleted).HasDefaultValue(0);

                // Configure indexes for sync and queries
                entity.HasIndex(e => e.PartographID);
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
                entity.HasIndex(e => e.ServerVersion);
                entity.HasIndex(e => e.Time);

                // Foreign key to Partograph - explicit principal key to avoid shadow property
                entity.HasOne<Partograph>()
                    .WithMany(navigationProperty)
                    .HasForeignKey(e => e.PartographID)
                    .HasPrincipalKey(p => p.ID)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureAnalyticsTables(ModelBuilder modelBuilder)
        {
            // DailyFacilityStats
            modelBuilder.Entity<DailyFacilityStats>(entity =>
            {
                entity.ToTable("Tbl_DailyFacilityStats");
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.FacilityID);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => new { e.Year, e.Month, e.Day });
                entity.HasIndex(e => new { e.FacilityID, e.Date }).IsUnique();
            });

            // MonthlyFacilityStats
            modelBuilder.Entity<MonthlyFacilityStats>(entity =>
            {
                entity.ToTable("Tbl_MonthlyFacilityStats");
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.FacilityID);
                entity.HasIndex(e => new { e.Year, e.Month });
                entity.HasIndex(e => new { e.FacilityID, e.Year, e.Month }).IsUnique();
            });

            // DeliveryOutcomeSummary
            modelBuilder.Entity<DeliveryOutcomeSummary>(entity =>
            {
                entity.ToTable("Tbl_DeliveryOutcomeSummary");
                entity.HasKey(e => e.ID);

                // Configure decimal precision for BirthWeight
                entity.Property(e => e.BirthWeight).HasPrecision(10, 2);

                entity.HasIndex(e => e.PartographID);
                entity.HasIndex(e => e.PatientID);
                entity.HasIndex(e => e.FacilityID);
                entity.HasIndex(e => e.DeliveryTime);
                entity.HasIndex(e => e.DeliveryMode);
            });

            // MaternalMortalityRecord
            modelBuilder.Entity<MaternalMortalityRecord>(entity =>
            {
                entity.ToTable("Tbl_MaternalMortalityRecord");
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.FacilityID);
                entity.HasIndex(e => e.DeathDateTime);
                entity.HasIndex(e => new { e.Year, e.Month });
                entity.HasIndex(e => e.DirectObstetricCause);
            });

            // NeonatalOutcomeRecord
            modelBuilder.Entity<NeonatalOutcomeRecord>(entity =>
            {
                entity.ToTable("Tbl_NeonatalOutcomeRecord");
                entity.HasKey(e => e.ID);

                // Configure decimal precision for BirthWeight
                entity.Property(e => e.BirthWeight).HasPrecision(10, 2);

                entity.HasIndex(e => e.FacilityID);
                entity.HasIndex(e => e.BirthDateTime);
                entity.HasIndex(e => e.OutcomeType);
                entity.HasIndex(e => new { e.Year, e.Month });
            });

            // ComplicationAnalytics
            modelBuilder.Entity<ComplicationAnalytics>(entity =>
            {
                entity.ToTable("Tbl_ComplicationAnalytics");
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.PartographID);
                entity.HasIndex(e => e.FacilityID);
                entity.HasIndex(e => e.OccurrenceDateTime);
                entity.HasIndex(e => e.ComplicationType);
                entity.HasIndex(e => e.Severity);
            });

            // ReferralAnalytics
            modelBuilder.Entity<ReferralAnalytics>(entity =>
            {
                entity.ToTable("Tbl_ReferralAnalytics");
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.ReferralID);
                entity.HasIndex(e => e.SourceFacilityID);
                entity.HasIndex(e => e.DestinationFacilityID);
                entity.HasIndex(e => e.ReferralDateTime);
                entity.HasIndex(e => e.Urgency);
            });

            // LaborProgressAnalytics
            modelBuilder.Entity<LaborProgressAnalytics>(entity =>
            {
                entity.ToTable("Tbl_LaborProgressAnalytics");
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.PartographID);
                entity.HasIndex(e => e.FacilityID);
                entity.HasIndex(e => e.LaborStartTime);
                entity.HasIndex(e => e.LaborProgressPattern);
            });

            // FacilityPerformanceSnapshot
            modelBuilder.Entity<FacilityPerformanceSnapshot>(entity =>
            {
                entity.ToTable("Tbl_FacilityPerformanceSnapshot");
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.FacilityID);
                entity.HasIndex(e => e.SnapshotDate);
                entity.HasIndex(e => e.PeriodType);
                entity.HasIndex(e => e.Region);
            });

            // AlertSummary
            modelBuilder.Entity<AlertSummary>(entity =>
            {
                entity.ToTable("Tbl_AlertSummary");
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.FacilityID);
                entity.HasIndex(e => e.PartographID);
                entity.HasIndex(e => e.AlertDateTime);
                entity.HasIndex(e => e.AlertSeverity);
                entity.HasIndex(e => e.Resolved);
            });
        }
    }
}
