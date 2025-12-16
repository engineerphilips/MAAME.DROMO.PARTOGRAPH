using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Assessment Plan Repository
    public class AssessmentRepository : BasePartographRepository<Assessment>
    {
        protected override string TableName => "Tbl_Assessment";
        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_Assessment (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,              
                createdtime INTEGER NOT NULL,
                updatedtime INTEGER NOT NULL,
                deletedtime INTEGER, 
                deviceid TEXT NOT NULL,
                origindeviceid TEXT NOT NULL,
                syncstatus INTEGER DEFAULT 0,
                version INTEGER DEFAULT 1,
                serverversion INTEGER DEFAULT 0,
                deleted INTEGER DEFAULT 0,
                conflictdata TEXT,
                datahash TEXT
            );
            
            CREATE INDEX IF NOT EXISTS idx_assessment_sync ON Tbl_Assessment(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_assessment_server_version ON Tbl_Assessment(serverversion);

            DROP TRIGGER IF EXISTS trg_assessment_insert;
            CREATE TRIGGER trg_assessment_insert
            AFTER INSERT ON Tbl_Assessment
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_Assessment
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_assessment_update;
            CREATE TRIGGER trg_assessment_update 
            AFTER UPDATE ON Tbl_Assessment
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_Assessment
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;";

        public AssessmentRepository(ILogger<AssessmentRepository> logger) : base(logger) { }

        protected override Assessment MapFromReader(SqliteDataReader reader)
        {
            return new Assessment
            {
                ID = Guid.Parse(reader["ID"].ToString()),
                PartographID = reader["partographid"] is DBNull ? null : Guid.Parse(reader["partographid"].ToString()),
                Time = DateTime.Parse(reader["time"].ToString()),
                //Handler = reader["handler"] is DBNull ? null : Guid.Parse(reader["handler"].ToString()),
                HandlerName = reader["handler"] is DBNull ? string.Empty : reader["handler"].ToString(),
                Notes = reader["notes"].ToString(),
                CreatedTime = Convert.ToInt64(reader["createdtime"]),
                UpdatedTime = Convert.ToInt64(reader["updatedtime"]),
                DeletedTime = reader["deletedtime"] is DBNull ? null : Convert.ToInt64(reader["deletedtime"]),
                DeviceId = reader["deviceid"].ToString(),
                OriginDeviceId = reader["origindeviceid"].ToString(),
                SyncStatus = Convert.ToInt32(reader["syncstatus"]),
                Version = Convert.ToInt32(reader["version"]),
                ServerVersion = reader["serverversion"] is DBNull ? 0 : Convert.ToInt32(reader["serverversion"]),
                Deleted = reader["deleted"] is DBNull ? 0 : Convert.ToInt32(reader["deleted"]),
                ConflictData = reader["conflictdata"] is DBNull ? string.Empty : reader["conflictdata"].ToString(),
                DataHash = reader["datahash"] is DBNull ? string.Empty : reader["datahash"].ToString()
            };
        }

        protected override string GetInsertSql() => @"
            INSERT INTO Tbl_BP (ID, partographID, time, handler, notes, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash)
        VALUES (@id, @partographId, @time, @handler, @notes, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash);";

        protected override string GetUpdateSql() => @"
            UPDATE Tbl_BP SET time = @time, handler = @handler, notes = @notes, 
                updatedtime = @updatedtime, deletedtime = @deletedtime, deviceid = @deviceid,
                origindeviceid = @origindeviceid, syncstatus = @syncstatus, version = @version,
                serverversion = @serverversion, deleted = @deleted, datahash = @datahash
            WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, Assessment item)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            item.ID = item.ID ?? Guid.NewGuid();
            item.CreatedTime = now;
            item.UpdatedTime = now;
            item.DeviceId = DeviceIdentity.GetOrCreateDeviceId();
            item.OriginDeviceId = DeviceIdentity.GetOrCreateDeviceId();
            item.Version = 1;
            item.ServerVersion = 0;
            item.SyncStatus = 0;
            item.Deleted = 0;
            item.DataHash = item.CalculateHash();

            cmd.Parameters.AddWithValue("@id", item.ID.ToString());
            cmd.Parameters.AddWithValue("@partographId", item.PartographID.ToString());
            cmd.Parameters.AddWithValue("@time", item.Time.ToString());
            cmd.Parameters.AddWithValue("@handler", item.Handler.ToString());
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@createdtime", item.CreatedTime);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@origindeviceid", item.OriginDeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@serverversion", item.ServerVersion);
            cmd.Parameters.AddWithValue("@deleted", item.Deleted);
            //cmd.Parameters.AddWithValue("@conflictdata", item.ConflictData);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, Assessment item)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            item.UpdatedTime = now;
            item.DeviceId = DeviceIdentity.GetOrCreateDeviceId();
            item.Version++;
            item.SyncStatus = 0; // Mark as needing sync
            item.DataHash = item.CalculateHash();

            cmd.Parameters.AddWithValue("@id", item.ID.ToString());
            cmd.Parameters.AddWithValue("@partographId", item.PartographID.ToString());
            cmd.Parameters.AddWithValue("@time", item.Time.ToString());
            cmd.Parameters.AddWithValue("@handler", item.Handler.ToString());
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);
        }
    }
}