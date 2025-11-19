using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Baseline FHR Repository
    public class BPRepository : BasePartographRepository<BP>
    {
        protected override string TableName => "Tbl_BP";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_BP (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                systolic INTEGER,
                diastolic INTEGER,
                pulse INTEGER,                     
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
            
            CREATE INDEX idx_temperature_sync ON Tbl_BP(updatedtime, syncstatus);
            CREATE INDEX idx_temperature_server_version ON Tbl_BP(serverversion);

            CREATE TRIGGER trg_temperature_insert 
            AFTER INSERT ON Tbl_BP
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_BP 
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            CREATE TRIGGER trg_temperature_update 
            AFTER UPDATE ON Tbl_BP
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_BP 
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";

        public BPRepository(ILogger<BPRepository> logger) : base(logger) { }

        protected override BP MapFromReader(SqliteDataReader reader)
        {
            return new BP
            {
                ID = Guid.Parse(reader.GetString(0)),
                PartographID = reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                Time = reader.GetDateTime(2),
                Handler = reader.IsDBNull(3) ? null : Guid.Parse(reader.GetString(3)),
                Notes = reader.GetString(4),
                Systolic = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                Diastolic = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                Pulse = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                CreatedTime = reader.GetInt64(8),
                UpdatedTime = reader.GetInt64(9),
                DeletedTime = reader.IsDBNull(10) ? null : reader.GetInt64(10),
                DeviceId = reader.GetString(11),
                OriginDeviceId = reader.GetString(12),
                SyncStatus = reader.GetInt32(13),
                Version = reader.GetInt32(14),
                ServerVersion = reader.IsDBNull(15) ? 0 : reader.GetInt32(15),
                Deleted = reader.IsDBNull(16) ? 0 : reader.GetInt32(14),
                ConflictData = reader.GetString(17),
                DataHash = reader.GetString(18)
            };
        }

        protected override string GetInsertSql() => @"
        INSERT INTO Tbl_BP (ID, partographID, time, handler, notes, systolic, diastolic, pulse, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted)
        VALUES (@id, @partographId, @time, @handler, @notes, @systolic, @diastolic, @pulse, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_BP
        SET partographID = @partographId,
            time = @time, 
            handler = @handler,
            notes = @notes,
            systolic = @systolic,
            diastolic = @diastolic, 
            pulse = @pulse,
            updatedtime = @updatedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, BP item)
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
            cmd.Parameters.AddWithValue("@time", item.Time.ToString("O"));
            cmd.Parameters.AddWithValue("@handler", item.Handler.ToString());
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@systolic", item.Systolic);
            cmd.Parameters.AddWithValue("@diastolic", item.Diastolic);
            cmd.Parameters.AddWithValue("@pulse", item.Pulse);
            cmd.Parameters.AddWithValue("@createdtime", item.CreatedTime);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@origindeviceid", item.OriginDeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@serverversion", item.ServerVersion);
            cmd.Parameters.AddWithValue("@deleted", item.Deleted);
            //cmd.Parameters.AddWithValue("@conflictdata", item.Deleted);
            //cmd.Parameters.AddWithValue("@datahash", item.Deleted);
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, BP item)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            item.UpdatedTime = now;
            item.DeviceId = DeviceIdentity.GetOrCreateDeviceId();
            item.Version++;
            item.SyncStatus = 0; // Mark as needing sync
            item.DataHash = item.CalculateHash();

            cmd.Parameters.AddWithValue("@id", item.ID.ToString());
            cmd.Parameters.AddWithValue("@partographId", item.PartographID.ToString());
            cmd.Parameters.AddWithValue("@time", item.Time.ToString("O"));
            cmd.Parameters.AddWithValue("@handler", item.Handler.ToString());
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@systolic", item.Systolic);
            cmd.Parameters.AddWithValue("@diastolic", item.Diastolic);
            cmd.Parameters.AddWithValue("@pulse", item.Pulse);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            //cmd.Parameters.AddWithValue("@datahash", item.Deleted);
        }
    }
}
