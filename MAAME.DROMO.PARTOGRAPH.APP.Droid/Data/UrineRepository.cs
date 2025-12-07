using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class UrineRepository : BasePartographRepository<Urine>
    {
        protected override string TableName => "Tbl_Urine";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_Urine (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                protien TEXT,
                acetone TEXT,                    
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
            
            CREATE INDEX IF NOT EXISTS idx_urine_sync ON Tbl_Urine(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_urine_server_version ON Tbl_Urine(serverversion);

            DROP TRIGGER IF EXISTS trg_urine_insert;
            CREATE TRIGGER trg_urine_insert 
            AFTER INSERT ON Tbl_Urine
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_Urine 
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_urine_update;
            CREATE TRIGGER trg_urine_update 
            AFTER UPDATE ON Tbl_Urine
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_Urine 
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";

        public UrineRepository(ILogger<UrineRepository> logger) : base(logger) { }

        protected override Urine MapFromReader(SqliteDataReader reader)
        {
            return new Urine
            {
                ID = Guid.Parse(reader.GetString(0)),
                PartographID = reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                Time = reader.GetDateTime(2),
                Handler = reader.IsDBNull(3) ? null : Guid.Parse(reader.GetString(3)),
                Notes = reader.GetString(4),
                Protein = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                Acetone = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                CreatedTime = reader.GetInt64(7),
                UpdatedTime = reader.GetInt64(8),
                DeletedTime = reader.IsDBNull(9) ? null : reader.GetInt64(9),
                DeviceId = reader.GetString(10),
                OriginDeviceId = reader.GetString(11),
                SyncStatus = reader.GetInt32(12),
                Version = reader.GetInt32(13),
                ServerVersion = reader.IsDBNull(14) ? 0 : reader.GetInt32(14),
                Deleted = reader.IsDBNull(15) ? 0 : reader.GetInt32(15),
                ConflictData = reader.GetString(16),
                DataHash = reader.GetString(17)
            };
        }

        protected override string GetInsertSql() => @"
        INSERT INTO Tbl_Urine (ID, partographID, time, handler, notes, protein, acetone, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted)
        VALUES (@id, @partographId, @time, @handler, @notes, @protein, @acetone, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_Urine
        SET partographID = @partographId,
            time = @time, 
            handler = @handler,
            notes = @notes,
            protein = @protein,
            acetone = @acetone,
            updatedtime = @updatedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, Urine item)
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
            cmd.Parameters.AddWithValue("@protein", item.Protein);
            cmd.Parameters.AddWithValue("@acetone", item.Acetone); 
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

        protected override void AddUpdateParameters(SqliteCommand cmd, Urine item)
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
            cmd.Parameters.AddWithValue("@protein", item.Protein);
            cmd.Parameters.AddWithValue("@acetone", item.Acetone);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            //cmd.Parameters.AddWithValue("@datahash", item.Deleted);
        }
    }
}
