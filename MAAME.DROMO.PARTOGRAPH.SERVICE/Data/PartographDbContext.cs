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
        public DbSet<Assessment> AssessmentPlans { get; set; }
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
                entity.HasKey(e => e.ID);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.HospitalNumber).HasMaxLength(50);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);

                // Configure indexes for sync
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
                entity.HasIndex(e => e.ServerVersion);
                entity.HasIndex(e => e.DeviceId);

                // Navigation properties
                entity.HasMany(p => p.PartographEntries)
                    .WithOne()
                    .HasForeignKey(p => p.PatientID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Partograph
            modelBuilder.Entity<Partograph>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);

                // Configure indexes for sync
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
                entity.HasIndex(e => e.ServerVersion);
                entity.HasIndex(e => e.PatientID);

                // Foreign key to Patient
                entity.HasOne<Patient>()
                    .WithMany(p => p.PartographEntries)
                    .HasForeignKey(p => p.PatientID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Staff
            modelBuilder.Entity<Staff>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.FacilityName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StaffID).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);

                // Configure indexes
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.StaffID);
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
            });

            // Configure Facility
            modelBuilder.Entity<Facility>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Code).HasMaxLength(50);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);

                // Configure indexes
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
                entity.HasIndex(e => e.Region);
            });

            // Configure base measurement properties for all measurement types
            ConfigureMeasurement<FHR>(modelBuilder);
            ConfigureMeasurement<Contraction>(modelBuilder);
            ConfigureMeasurement<CervixDilatation>(modelBuilder);
            ConfigureMeasurement<HeadDescent>(modelBuilder);
            ConfigureMeasurement<BP>(modelBuilder);
            ConfigureMeasurement<Temperature>(modelBuilder);
            ConfigureMeasurement<AmnioticFluid>(modelBuilder);
            ConfigureMeasurement<Urine>(modelBuilder);
            ConfigureMeasurement<Caput>(modelBuilder);
            ConfigureMeasurement<Moulding>(modelBuilder);
            ConfigureMeasurement<FetalPosition>(modelBuilder);
            ConfigureMeasurement<PainReliefEntry>(modelBuilder);
            ConfigureMeasurement<PostureEntry>(modelBuilder);
            ConfigureMeasurement<OralFluidEntry>(modelBuilder);
            ConfigureMeasurement<IVFluidEntry>(modelBuilder);
            ConfigureMeasurement<MedicationEntry>(modelBuilder);
            ConfigureMeasurement<Oxytocin>(modelBuilder);
            ConfigureMeasurement<CompanionEntry>(modelBuilder);
            ConfigureMeasurement<Assessment>(modelBuilder);
            ConfigureMeasurement<MedicalNote>(modelBuilder);

            // Configure extended measurement types
            ConfigureMeasurement<FourthStageVitals>(modelBuilder);
            ConfigureMeasurement<BishopScore>(modelBuilder);
            ConfigureMeasurement<PartographDiagnosis>(modelBuilder);
            ConfigureMeasurement<PartographRiskFactor>(modelBuilder);
            ConfigureMeasurement<Plan>(modelBuilder);

            // Configure BirthOutcome
            modelBuilder.Entity<BirthOutcome>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);

                // Configure indexes
                entity.HasIndex(e => e.PartographID);
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
                entity.HasIndex(e => e.ServerVersion);
                entity.HasIndex(e => e.RecordedTime);

                // Foreign key to Partograph
                entity.HasOne(e => e.Partograph)
                    .WithMany()
                    .HasForeignKey(e => e.PartographID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure BabyDetails
            modelBuilder.Entity<BabyDetails>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);

                // Configure indexes
                entity.HasIndex(e => e.PartographID);
                entity.HasIndex(e => e.BirthOutcomeID);
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
                entity.HasIndex(e => e.ServerVersion);
                entity.HasIndex(e => e.BirthTime);

                // Foreign key to Partograph
                entity.HasOne(e => e.Partograph)
                    .WithMany()
                    .HasForeignKey(e => e.PartographID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Foreign key to BirthOutcome
                entity.HasOne(e => e.BirthOutcome)
                    .WithMany()
                    .HasForeignKey(e => e.BirthOutcomeID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Referral
            modelBuilder.Entity<Referral>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);

                // Configure indexes
                entity.HasIndex(e => e.PartographID);
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
                entity.HasIndex(e => e.ServerVersion);
                entity.HasIndex(e => e.ReferralTime);
                entity.HasIndex(e => e.Status);

                // Foreign key to Partograph
                entity.HasOne(e => e.Partograph)
                    .WithMany()
                    .HasForeignKey(e => e.PartographID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Analytics Tables
            ConfigureAnalyticsTables(modelBuilder);
        }

        private void ConfigureMeasurement<T>(ModelBuilder modelBuilder) where T : BasePartographMeasurement
        {
            modelBuilder.Entity<T>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.DeviceId).HasMaxLength(100);
                entity.Property(e => e.OriginDeviceId).HasMaxLength(100);

                // Configure indexes for sync and queries
                entity.HasIndex(e => e.PartographID);
                entity.HasIndex(e => e.UpdatedTime);
                entity.HasIndex(e => e.SyncStatus);
                entity.HasIndex(e => e.ServerVersion);
                entity.HasIndex(e => e.Time);

                // Foreign key to Partograph
                entity.HasOne<Partograph>()
                    .WithMany()
                    .HasForeignKey(e => e.PartographID)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureAnalyticsTables(ModelBuilder modelBuilder)
        {
            // DailyFacilityStats
            modelBuilder.Entity<DailyFacilityStats>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.FacilityID);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => new { e.Year, e.Month, e.Day });
                entity.HasIndex(e => new { e.FacilityID, e.Date }).IsUnique();
            });

            // MonthlyFacilityStats
            modelBuilder.Entity<MonthlyFacilityStats>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.FacilityID);
                entity.HasIndex(e => new { e.Year, e.Month });
                entity.HasIndex(e => new { e.FacilityID, e.Year, e.Month }).IsUnique();
            });

            // DeliveryOutcomeSummary
            modelBuilder.Entity<DeliveryOutcomeSummary>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.PartographID);
                entity.HasIndex(e => e.PatientID);
                entity.HasIndex(e => e.FacilityID);
                entity.HasIndex(e => e.DeliveryTime);
                entity.HasIndex(e => e.DeliveryMode);
            });

            // MaternalMortalityRecord
            modelBuilder.Entity<MaternalMortalityRecord>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.FacilityID);
                entity.HasIndex(e => e.DeathDateTime);
                entity.HasIndex(e => new { e.Year, e.Month });
                entity.HasIndex(e => e.DirectObstetricCause);
            });

            // NeonatalOutcomeRecord
            modelBuilder.Entity<NeonatalOutcomeRecord>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.FacilityID);
                entity.HasIndex(e => e.BirthDateTime);
                entity.HasIndex(e => e.OutcomeType);
                entity.HasIndex(e => new { e.Year, e.Month });
            });

            // ComplicationAnalytics
            modelBuilder.Entity<ComplicationAnalytics>(entity =>
            {
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
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.PartographID);
                entity.HasIndex(e => e.FacilityID);
                entity.HasIndex(e => e.LaborStartTime);
                entity.HasIndex(e => e.LaborProgressPattern);
            });

            // FacilityPerformanceSnapshot
            modelBuilder.Entity<FacilityPerformanceSnapshot>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.FacilityID);
                entity.HasIndex(e => e.SnapshotDate);
                entity.HasIndex(e => e.PeriodType);
                entity.HasIndex(e => e.Region);
            });

            // AlertSummary
            modelBuilder.Entity<AlertSummary>(entity =>
            {
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
