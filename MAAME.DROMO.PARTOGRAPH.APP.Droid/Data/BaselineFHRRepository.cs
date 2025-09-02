using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Baseline FHR Repository
    public class BaselineFHRRepository : BasePartographRepository<BaselineFHREntry>
    {
        protected override string TableName => "BaselineFHREntry";
        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS BaselineFHREntry (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                PatientID INTEGER NOT NULL,
                RecordedTime TEXT NOT NULL,
                RecordedBy TEXT,
                Notes TEXT,
                BaselineRate INTEGER,
                Variability TEXT,
                Accelerations INTEGER NOT NULL,
                Pattern TEXT,
                MonitoringMethod TEXT
            );";

        public BaselineFHRRepository(ILogger<BaselineFHRRepository> logger) : base(logger) { }

        protected override BaselineFHREntry MapFromReader(SqliteDataReader reader)
        {
            return new BaselineFHREntry
            {
                ID = reader.GetInt32(0),
                PatientID = reader.GetInt32(1),
                RecordedTime = DateTime.Parse(reader.GetString(2)),
                RecordedBy = reader.GetString(3),
                Notes = reader.GetString(4),
                BaselineRate = reader.GetInt32(5),
                Variability = reader.GetString(6),
                Accelerations = reader.GetBoolean(7),
                Pattern = reader.GetString(8),
                MonitoringMethod = reader.GetString(9)
            };
        }

        protected override string GetInsertSql() => @"
            INSERT INTO BaselineFHREntry (PatientID, RecordedTime, RecordedBy, Notes, BaselineRate, 
                Variability, Accelerations, Pattern, MonitoringMethod)
            VALUES (@PatientID, @RecordedTime, @RecordedBy, @Notes, @BaselineRate, 
                @Variability, @Accelerations, @Pattern, @MonitoringMethod);
            SELECT last_insert_rowid();";

        protected override string GetUpdateSql() => @"
            UPDATE BaselineFHREntry SET RecordedTime = @RecordedTime, RecordedBy = @RecordedBy, Notes = @Notes,
                BaselineRate = @BaselineRate, Variability = @Variability, Accelerations = @Accelerations,
                Pattern = @Pattern, MonitoringMethod = @MonitoringMethod
            WHERE ID = @ID";

        protected override void AddInsertParameters(SqliteCommand cmd, BaselineFHREntry item)
        {
            cmd.Parameters.AddWithValue("@PatientID", item.PatientID);
            cmd.Parameters.AddWithValue("@RecordedTime", item.RecordedTime.ToString("O"));
            cmd.Parameters.AddWithValue("@RecordedBy", item.RecordedBy ?? "");
            cmd.Parameters.AddWithValue("@Notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@BaselineRate", item.BaselineRate);
            cmd.Parameters.AddWithValue("@Variability", item.Variability ?? "");
            cmd.Parameters.AddWithValue("@Accelerations", item.Accelerations);
            cmd.Parameters.AddWithValue("@Pattern", item.Pattern ?? "");
            cmd.Parameters.AddWithValue("@MonitoringMethod", item.MonitoringMethod ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, BaselineFHREntry item)
        {
            AddInsertParameters(cmd, item);
            cmd.Parameters.AddWithValue("@ID", item.ID);
        }
    }
}
