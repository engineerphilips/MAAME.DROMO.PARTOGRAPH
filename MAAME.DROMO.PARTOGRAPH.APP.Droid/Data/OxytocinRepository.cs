using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class OxytocinRepository : BasePartographRepository<Oxytocin>
    {
        protected override string TableName => "Tbl_Oxytocin";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_Oxytocin (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                dosemunitspermin REAL NOT NULL,
                totalvolumeinfused REAL NOT NULL,
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

            CREATE INDEX IF NOT EXISTS idx_oxytocin_sync ON Tbl_Oxytocin(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_oxytocin_server_version ON Tbl_Oxytocin(serverversion);

            DROP TRIGGER IF EXISTS trg_oxytocin_insert;
            CREATE TRIGGER trg_oxytocin_insert
            AFTER INSERT ON Tbl_Oxytocin
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_Oxytocin
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_oxytocin_update;
            CREATE TRIGGER trg_oxytocin_update
            AFTER UPDATE ON Tbl_Oxytocin
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_Oxytocin
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";

        public OxytocinRepository(ILogger<OxytocinRepository> logger) : base(logger) { }

        protected override Oxytocin MapFromReader(SqliteDataReader reader)
        {
            return new Oxytocin
            {
                ID = Guid.Parse(reader.GetString(0)),
                PartographID = reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                Time = reader.GetDateTime(2),
                Handler = reader.IsDBNull(3) ? null : Guid.Parse(reader.GetString(3)),
                Notes = reader.GetString(4),
                DoseMUnitsPerMin = (decimal)reader.GetDouble(5),
                TotalVolumeInfused = (decimal)reader.GetDouble(6),
                CreatedTime = reader.GetInt64(7),
                UpdatedTime = reader.GetInt64(8),
                DeletedTime = reader.IsDBNull(9) ? null : reader.GetInt64(9),
                DeviceId = reader.GetString(10),
                OriginDeviceId = reader.GetString(11),
                SyncStatus = reader.GetInt32(12),
                Version = reader.GetInt32(13),
                ServerVersion = reader.IsDBNull(14) ? 0 : reader.GetInt32(14),
                Deleted = reader.IsDBNull(15) ? 0 : reader.GetInt32(15),
                ConflictData = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
                DataHash = reader.IsDBNull(17) ? string.Empty : reader.GetString(17)
            };
        }

        protected override string GetInsertSql() => @"
        INSERT INTO Tbl_Oxytocin (ID, partographID, time, handler, notes, dosemunitspermin, totalvolumeinfused, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash)
        VALUES (@id, @partographId, @time, @handler, @notes, @dosemunitspermin, @totalvolumeinfused, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_Oxytocin
        SET partographID = @partographId,
            time = @time,
            handler = @handler,
            notes = @notes,
            dosemunitspermin = @dosemunitspermin,
            totalvolumeinfused = @totalvolumeinfused,
            updatedtime = @updatedtime,
            deletedtime = @deletedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version,
            datahash = @datahash
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, Oxytocin item)
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
            cmd.Parameters.AddWithValue("@dosemunitspermin", (double)item.DoseMUnitsPerMin);
            cmd.Parameters.AddWithValue("@totalvolumeinfused", (double)item.TotalVolumeInfused);
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

        protected override void AddUpdateParameters(SqliteCommand cmd, Oxytocin item)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            item.UpdatedTime = now;
            item.DeviceId = DeviceIdentity.GetOrCreateDeviceId();
            item.Version++;
            item.SyncStatus = 0;
            item.DataHash = item.CalculateHash();

            cmd.Parameters.AddWithValue("@id", item.ID.ToString());
            cmd.Parameters.AddWithValue("@partographId", item.PartographID.ToString());
            cmd.Parameters.AddWithValue("@time", item.Time.ToString("O"));
            cmd.Parameters.AddWithValue("@handler", item.Handler.ToString());
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@dosemunitspermin", (double)item.DoseMUnitsPerMin);
            cmd.Parameters.AddWithValue("@totalvolumeinfused", (double)item.TotalVolumeInfused);
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
