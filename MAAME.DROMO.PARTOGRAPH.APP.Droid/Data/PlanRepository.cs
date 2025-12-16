using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Assessment Plan Repository
    public class PlanRepository : BasePartographRepository<Plan>
    {
        protected override string TableName => "Tbl_Plan";
        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_Plan (
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
            
            CREATE INDEX IF NOT EXISTS idx_plan_sync ON Tbl_Plan(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_plan_server_version ON Tbl_Plan(serverversion);

            DROP TRIGGER IF EXISTS trg_plan_insert;
            CREATE TRIGGER trg_plan_insert
            AFTER INSERT ON Tbl_Plan
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_Plan
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_plan_update;
            CREATE TRIGGER trg_plan_update 
            AFTER UPDATE ON Tbl_Plan
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_Plan
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;";

        public PlanRepository(ILogger<PlanRepository> logger) : base(logger) { }

        protected override Plan MapFromReader(SqliteDataReader reader)
        {
            return new Plan
            {
                ID = Guid.Parse((string)reader["ID"]),
                PartographID = reader["partographid"] is DBNull ? null : Guid.Parse((string)reader["partographid"]),
                Time = DateTime.Parse((string)reader["time"]),
                //Handler = reader["handler"] is DBNull ? null : Guid.Parse((string)reader["handler"]),
                HandlerName = reader["handler"] is DBNull ? string.Empty : (string)reader["handler"],
                Notes = (string)reader["notes"],
                CreatedTime = Convert.ToInt64(reader["createdtime"]),
                UpdatedTime = Convert.ToInt64(reader["updatedtime"]),
                DeletedTime = reader["deletedtime"] is DBNull ? null : Convert.ToInt64(reader["deletedtime"]),
                DeviceId = (string)reader["deviceid"],
                OriginDeviceId = (string)reader["origindeviceid"],
                SyncStatus = Convert.ToInt32(reader["syncstatus"]),
                Version = Convert.ToInt32(reader["version"]),
                ServerVersion = reader["serverversion"] is DBNull ? 0 : Convert.ToInt32(reader["serverversion"]),
                Deleted = reader["deleted"] is DBNull ? 0 : Convert.ToInt32(reader["deleted"]),
                ConflictData = reader["conflictdata"] is DBNull ? string.Empty : (string)reader["conflictdata"],
                DataHash = reader["datahash"] is DBNull ? string.Empty : (string)reader["datahash"]
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

        protected override void AddInsertParameters(SqliteCommand cmd, Plan item)
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

        protected override void AddUpdateParameters(SqliteCommand cmd, Plan item)
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