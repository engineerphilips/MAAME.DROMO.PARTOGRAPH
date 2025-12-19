using Microsoft.Data.SqlClient;

namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Data
{
    using static DbHelper;

    internal static class ConnectionManager
    {
        internal static SqlConnectionStringBuilder ConnectionConnectionString
        {
            get
            {
                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = DataSource,
                    InitialCatalog = Database,
                    IntegratedSecurity = false,
                    UserID = UserName,
                    Password = Password,
                    TrustServerCertificate = true
                };

                return builder;
            }
        }
    }
}
