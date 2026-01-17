using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.EntityFrameworkCore;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Data
{
    public class MonitoringDbContext : DbContext
    {
        public MonitoringDbContext(DbContextOptions<MonitoringDbContext> options)
            : base(options)
        {
        }

        // Administrative hierarchy
        public DbSet<Region> Regions { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<MonitoringUser> MonitoringUsers { get; set; }

        // Core clinical data (read-only for monitoring)
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Partograph> Partographs { get; set; }
        public DbSet<Staff> Staff { get; set; }

        // Analytics data
        public DbSet<DailyFacilityStats> DailyFacilityStats { get; set; }
        public DbSet<MonthlyFacilityStats> MonthlyFacilityStats { get; set; }
        public DbSet<DeliveryOutcomeSummary> DeliveryOutcomeSummaries { get; set; }
        public DbSet<MaternalMortalityRecord> MaternalMortalityRecords { get; set; }
        public DbSet<NeonatalOutcomeRecord> NeonatalOutcomeRecords { get; set; }
        public DbSet<ComplicationAnalytics> ComplicationAnalytics { get; set; }
        public DbSet<ReferralAnalytics> ReferralAnalytics { get; set; }
        public DbSet<FacilityPerformanceSnapshot> FacilityPerformanceSnapshots { get; set; }
        public DbSet<AlertSummary> AlertSummaries { get; set; }

        // Birth outcomes and referrals
        public DbSet<BirthOutcome> BirthOutcomes { get; set; }
        public DbSet<BabyDetails> BabyDetails { get; set; }
        public DbSet<Referral> Referrals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Region
            modelBuilder.Entity<Region>(entity =>
            {
                entity.ToTable("Tbl_Region");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).HasMaxLength(20);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.Name);

                entity.HasMany(r => r.Districts)
                    .WithOne(d => d.Region)
                    .HasForeignKey(d => d.RegionID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure District
            modelBuilder.Entity<District>(entity =>
            {
                entity.ToTable("Tbl_District");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).HasMaxLength(20);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.RegionID);
                entity.HasIndex(e => e.Name);

                entity.HasMany(d => d.Facilities)
                    .WithOne(f => f.District)
                    .HasForeignKey(f => f.DistrictID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure MonitoringUser
            modelBuilder.Entity<MonitoringUser>(entity =>
            {
                entity.ToTable("Tbl_MonitoringUser");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.AccessLevel).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Role).HasMaxLength(50);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.AccessLevel);
                entity.HasIndex(e => e.RegionID);
                entity.HasIndex(e => e.DistrictID);

                entity.HasOne(u => u.Region)
                    .WithMany()
                    .HasForeignKey(u => u.RegionID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.District)
                    .WithMany()
                    .HasForeignKey(u => u.DistrictID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Facility with District relationship
            modelBuilder.Entity<Facility>(entity =>
            {
                entity.ToTable("Tbl_Facility");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Code).HasMaxLength(50);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.DistrictID);
                entity.HasIndex(e => e.Region);
            });

            // Configure existing tables
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("Tbl_Patient");
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<Partograph>(entity =>
            {
                entity.ToTable("Tbl_Partograph");
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<Staff>(entity =>
            {
                entity.ToTable("Tbl_Staff");
                entity.HasKey(e => e.ID);
            });

            // Configure analytics tables
            ConfigureAnalyticsTables(modelBuilder);

            // Configure soft delete filters
            ConfigureSoftDeleteFilters(modelBuilder);
        }

        private void ConfigureAnalyticsTables(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DailyFacilityStats>(entity =>
            {
                entity.ToTable("Tbl_DailyFacilityStats");
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<MonthlyFacilityStats>(entity =>
            {
                entity.ToTable("Tbl_MonthlyFacilityStats");
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<DeliveryOutcomeSummary>(entity =>
            {
                entity.ToTable("Tbl_DeliveryOutcomeSummary");
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<MaternalMortalityRecord>(entity =>
            {
                entity.ToTable("Tbl_MaternalMortalityRecord");
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<NeonatalOutcomeRecord>(entity =>
            {
                entity.ToTable("Tbl_NeonatalOutcomeRecord");
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<ComplicationAnalytics>(entity =>
            {
                entity.ToTable("Tbl_ComplicationAnalytics");
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<ReferralAnalytics>(entity =>
            {
                entity.ToTable("Tbl_ReferralAnalytics");
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<FacilityPerformanceSnapshot>(entity =>
            {
                entity.ToTable("Tbl_FacilityPerformanceSnapshot");
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<AlertSummary>(entity =>
            {
                entity.ToTable("Tbl_AlertSummary");
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<BirthOutcome>(entity =>
            {
                entity.ToTable("Tbl_BirthOutcome");
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<BabyDetails>(entity =>
            {
                entity.ToTable("Tbl_BabyDetails");
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<Referral>(entity =>
            {
                entity.ToTable("Tbl_Referral");
                entity.HasKey(e => e.ID);
            });
        }

        private void ConfigureSoftDeleteFilters(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Region>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<District>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Facility>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<MonitoringUser>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Patient>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Partograph>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Staff>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<BirthOutcome>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<BabyDetails>().HasQueryFilter(e => e.Deleted == 0);
            modelBuilder.Entity<Referral>().HasQueryFilter(e => e.Deleted == 0);
        }
    }
}
