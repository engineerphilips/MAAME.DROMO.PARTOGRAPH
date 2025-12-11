using MAAME.DROMO.PARTOGRAPH.MODEL;
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

        // Birth Outcome and Referral Entities
        public DbSet<BirthOutcome> BirthOutcomes { get; set; }
        public DbSet<BabyDetails> BabyDetails { get; set; }
        public DbSet<Referral> Referrals { get; set; }

        // Supporting Entities
        //public DbSet<VitalSign> VitalSigns { get; set; }
        //public DbSet<Project> Projects { get; set; }
        //public DbSet<ProjectTask> ProjectTasks { get; set; }
        //public DbSet<Category> Categories { get; set; }
        //public DbSet<Tag> Tags { get; set; }
        //public DbSet<ProjectsTags> ProjectsTags { get; set; }

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

                //entity.HasMany(p => p.MedicalNotes)
                //    .WithOne()
                //    .HasForeignKey(m => m.PartographID)
                //    .OnDelete(DeleteBehavior.Cascade);
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

            // Configure VitalSign
            //modelBuilder.Entity<VitalSign>(entity =>
            //{
            //    entity.HasKey(e => e.ID);
            //    entity.HasIndex(e => e.PatientID);
            //    entity.HasIndex(e => e.Time);
            //});

            // Configure Project entities
            //modelBuilder.Entity<Project>(entity =>
            //{
            //    entity.HasKey(e => e.ID);
            //    entity.Property(e => e.Name).HasMaxLength(200);
            //});

            //modelBuilder.Entity<ProjectTask>(entity =>
            //{
            //    entity.HasKey(e => e.ID);
            //    entity.HasIndex(e => e.ProjectID);
            //});

            //modelBuilder.Entity<Category>(entity =>
            //{
            //    entity.HasKey(e => e.ID);
            //    entity.Property(e => e.Name).HasMaxLength(100);
            //});

            //modelBuilder.Entity<Tag>(entity =>
            //{
            //    entity.HasKey(e => e.ID);
            //    entity.Property(e => e.Name).HasMaxLength(50);
            //    entity.HasIndex(e => e.Name).IsUnique();
            //});

            //modelBuilder.Entity<ProjectsTags>(entity =>
            //{
            //    entity.HasKey(e => e.ID);
            //    entity.HasIndex(e => new { e.ProjectID, e.TagID }).IsUnique();
            //});
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
    }
}
