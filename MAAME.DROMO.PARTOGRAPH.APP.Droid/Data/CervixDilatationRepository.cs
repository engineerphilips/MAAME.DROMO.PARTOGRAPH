using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Baseline HeadDescent Repository
    public class CervixDilatationRepository : BasePartographRepository<CervixDilatation>
    {
        protected override string TableName => "Tbl_FHR";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_CervixDilatation (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                dilatation INTEGER,                
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
            
            CREATE INDEX idx_cervix_dilatation_sync ON Tbl_CervixDilatation(updatedtime, syncstatus);
            CREATE INDEX idx_cervix_dilatation_server_version ON Tbl_CervixDilatation(serverversion);

            CREATE TRIGGER trg_cervix_dilatation_insert 
            AFTER INSERT ON Tbl_CervixDilatation
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_CervixDilatation 
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            CREATE TRIGGER trg_cervix_dilatation_update 
            AFTER UPDATE ON Tbl_CervixDilatation
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_CervixDilatation 
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";

        public CervixDilatationRepository(ILogger<CervixDilatationRepository> logger) : base(logger) { }

        protected override CervixDilatation MapFromReader(SqliteDataReader reader)
        {
            return new CervixDilatation
            {
                ID = Guid.Parse(reader.GetString(0)),
                PartographID = reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                Time = reader.GetDateTime(2),
                Handler = reader.IsDBNull(3) ? null : Guid.Parse(reader.GetString(3)),
                Notes = reader.GetString(4),
                DilatationCm = reader.IsDBNull(5) ? 5 : reader.GetInt32(5),
                CreatedTime = reader.GetInt64(6),
                UpdatedTime = reader.GetInt64(7),
                DeletedTime = reader.IsDBNull(8) ? null : reader.GetInt64(8),
                DeviceId = reader.GetString(9),
                OriginDeviceId = reader.GetString(10),
                SyncStatus = reader.GetInt32(11),
                Version = reader.GetInt32(12),
                ServerVersion = reader.IsDBNull(13) ? 0 : reader.GetInt32(13),
                Deleted = reader.IsDBNull(14) ? 0 : reader.GetInt32(14),
                ConflictData = reader.GetString(15),
                DataHash = reader.GetString(16)
            };
        }

        protected override string GetInsertSql() => @"
        INSERT INTO Tbl_CervixDilatation (ID, partographID, time, handler, notes, dilatation, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted)
        VALUES (@id, @partographId, @time, @handler, @notes, @dilatation, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_CervixDilatation
        SET partographID = @partographId,
            time = @time, 
            handler = @handler,
            notes = @notes,
            dilatation = @dilatation,
            updatedtime = @updatedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, CervixDilatation item)
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
            cmd.Parameters.AddWithValue("@dilatation", item.DilatationCm);
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

        protected override void AddUpdateParameters(SqliteCommand cmd, CervixDilatation item)
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
            cmd.Parameters.AddWithValue("@dilatation", item.DilatationCm);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            //cmd.Parameters.AddWithValue("@datahash", item.Deleted);
        }
    }
}
