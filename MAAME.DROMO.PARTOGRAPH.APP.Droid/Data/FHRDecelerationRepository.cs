using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // FHR Deceleration Repository
    public class FHRDecelerationRepository : BasePartographRepository<FHRDecelerationEntry>
    {
        protected override string TableName => "FHRDecelerationEntry";
        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS FHRDecelerationEntry (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                PatientID INTEGER NOT NULL,
                RecordedTime TEXT NOT NULL,
                RecordedBy TEXT,
                Notes TEXT,
                DecelerationsPresent INTEGER NOT NULL,
                DecelerationType TEXT,
                Severity TEXT,
                Duration INTEGER,
                Recovery TEXT,
                RequiresAction INTEGER NOT NULL,
                ActionTaken TEXT
            );";

        public FHRDecelerationRepository(ILogger<FHRDecelerationRepository> logger) : base(logger) { }

        protected override FHRDecelerationEntry MapFromReader(SqliteDataReader reader)
        {
            return new FHRDecelerationEntry
            {
                ID = reader.GetInt32(0),
                PatientID = reader.GetInt32(1),
                RecordedTime = DateTime.Parse(reader.GetString(2)),
                RecordedBy = reader.GetString(3),
                Notes = reader.GetString(4),
                DecelerationsPresent = reader.GetBoolean(5),
                DecelerationType = reader.GetString(6),
                Severity = reader.GetString(7),
                Duration = reader.GetInt32(8),
                Recovery = reader.GetString(9),
                RequiresAction = reader.GetBoolean(10),
                ActionTaken = reader.GetString(11)
            };
        }

        protected override string GetInsertSql() => @"
            INSERT INTO FHRDecelerationEntry (PatientID, RecordedTime, RecordedBy, Notes, DecelerationsPresent, 
                DecelerationType, Severity, Duration, Recovery, RequiresAction, ActionTaken)
            VALUES (@PatientID, @RecordedTime, @RecordedBy, @Notes, @DecelerationsPresent, 
                @DecelerationType, @Severity, @Duration, @Recovery, @RequiresAction, @ActionTaken);
            SELECT last_insert_rowid();";

        protected override string GetUpdateSql() => @"
            UPDATE FHRDecelerationEntry SET RecordedTime = @RecordedTime, RecordedBy = @RecordedBy, Notes = @Notes,
                DecelerationsPresent = @DecelerationsPresent, DecelerationType = @DecelerationType, Severity = @Severity,
                Duration = @Duration, Recovery = @Recovery, RequiresAction = @RequiresAction, ActionTaken = @ActionTaken
            WHERE ID = @ID";

        protected override void AddInsertParameters(SqliteCommand cmd, FHRDecelerationEntry item)
        {
            cmd.Parameters.AddWithValue("@PatientID", item.PatientID);
            cmd.Parameters.AddWithValue("@RecordedTime", item.RecordedTime.ToString("O"));
            cmd.Parameters.AddWithValue("@RecordedBy", item.RecordedBy ?? "");
            cmd.Parameters.AddWithValue("@Notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@DecelerationsPresent", item.DecelerationsPresent);
            cmd.Parameters.AddWithValue("@DecelerationType", item.DecelerationType ?? "");
            cmd.Parameters.AddWithValue("@Severity", item.Severity ?? "");
            cmd.Parameters.AddWithValue("@Duration", item.Duration);
            cmd.Parameters.AddWithValue("@Recovery", item.Recovery ?? "");
            cmd.Parameters.AddWithValue("@RequiresAction", item.RequiresAction);
            cmd.Parameters.AddWithValue("@ActionTaken", item.ActionTaken ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, FHRDecelerationEntry item)
        {
            AddInsertParameters(cmd, item);
            cmd.Parameters.AddWithValue("@ID", item.ID);
        }
    }
}
