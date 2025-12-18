using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public static class Constants
    {
        public const string DatabaseFilename = "PartographSQLite.db3";

        public static string DatabasePath =>
            $"Data Source={Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename)}";

        public static Staff? Staff;

        // Selected facility for Super/Maame.Dromo.Admin users
        public static Facility? SelectedFacility;

        // Cached dashboard stats for UI-first loading pattern
        // Preloaded during facility selection transition to avoid loading overlay on homepage
        public static DashboardStats? CachedDashboardStats;

        // Helper method to check if user is Super or Admin
        public static bool IsSuperOrAdmin()
        {
            return Staff?.Role == "SUPER-ADMIN" || Staff?.Role == "Maame.Dromo.Admin";
        }

        // Helper method to get facility for filtering
        public static Guid? GetFacilityForFiltering()
        {
            // For Super/Admin, use selected facility
            if (IsSuperOrAdmin())
            {
                return SelectedFacility?.ID;
            }
            // For other users, use their assigned facility
            return Staff?.Facility;
        }
    }
}