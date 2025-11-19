using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public static class Constants
    {
        public const string DatabaseFilename = "PartographSQLite.db3";

        public static string DatabasePath =>
            $"Data Source={Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename)}";

        public static Staff? Staff;
    }
}