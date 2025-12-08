using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Posture Repository
    public class PostureRepository : BasePartographRepository<PostureEntry>
    {
        protected override string TableName => "Tbl_Posture";
        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_Posture (
                    ID TEXT PRIMARY KEY,
                    partographid TEXT,
                    time TEXT NOT NULL,
                    handler TEXT,
                    notes TEXT NOT NULL,
                    posture TEXT,              
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
            
                CREATE INDEX IF NOT EXISTS idx_posture_sync ON Tbl_Posture(updatedtime, syncstatus);
                CREATE INDEX IF NOT EXISTS idx_posture_server_version ON Tbl_Posture(serverversion);

                DROP TRIGGER IF EXISTS trg_posture_insert;
                CREATE TRIGGER trg_posture_insert 
                AFTER INSERT ON Tbl_Posture
                WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
                BEGIN
                    UPDATE Tbl_Posture 
                    SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                        updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                    WHERE ID = NEW.ID;
                END;

                DROP TRIGGER IF EXISTS trg_posture_update;
                CREATE TRIGGER trg_posture_update 
                AFTER UPDATE ON Tbl_Posture
                WHEN NEW.updatedtime = OLD.updatedtime
                BEGIN
                    UPDATE Tbl_Posture 
                    SET updatedtime = (strftime('%s', 'now') * 1000),
                        version = OLD.version + 1,
                        syncstatus = 0
                    WHERE ID = NEW.ID;
                END;";

        public PostureRepository(ILogger<PostureRepository> logger) : base(logger) { }

        protected override PostureEntry MapFromReader(SqliteDataReader reader)
        {
            return new PostureEntry
            {
                ID = Guid.Parse(reader.GetString(0)),
                PartographID = reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                Time = DateTime.Parse(reader.GetString(2)),
                //Handler = reader.IsDBNull(3) ? null : Guid.Parse(reader.GetString(3)),
                HandlerName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Notes = reader.GetString(4),
                Posture = reader.GetString(5),
                CreatedTime = reader.GetInt64(6),
                UpdatedTime = reader.GetInt64(7),
                DeletedTime = reader.IsDBNull(8) ? null : reader.GetInt64(8),
                DeviceId = reader.GetString(9),
                OriginDeviceId = reader.GetString(10),
                SyncStatus = reader.GetInt32(11),
                Version = reader.GetInt32(12),
                ServerVersion = reader.IsDBNull(13) ? 0 : reader.GetInt32(13),
                Deleted = reader.IsDBNull(14) ? 0 : reader.GetInt32(14)
            };
        }

        protected override string GetInsertSql() => @"
            INSERT INTO Tbl_Posture (ID, partographID, time, handler, notes, posture, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted)
            VALUES (@id, @partographId, @time, @handler, @notes, @posture, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted);";

        protected override string GetUpdateSql() => @"
            UPDATE Tbl_Posture
        SET partographID = @partographId,
            time = @time, 
            handler = @handler,
            notes = @notes,
            posture = @posture,
            updatedtime = @updatedtime,
            deletedtime = @deletedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version,
            datahash = @datahash
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, PostureEntry item)
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
            cmd.Parameters.AddWithValue("@posture", item.Posture);
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

        protected override void AddUpdateParameters(SqliteCommand cmd, PostureEntry item)
        {
            //AddInsertParameters(cmd, item);
            cmd.Parameters.AddWithValue("@id", item.ID.ToString());
            cmd.Parameters.AddWithValue("@partographId", item.PartographID.ToString());
            cmd.Parameters.AddWithValue("@time", item.Time.ToString("O"));
            cmd.Parameters.AddWithValue("@handler", item.Handler.ToString());
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@posture", item.Posture);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            //cmd.Parameters.AddWithValue("@conflictdata", item.ConflictData);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);
        }
    }
}
