using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Pain Relief Repository
    public class PainReliefRepository : BasePartographRepository<PainReliefEntry>
    {
        protected override string TableName => "Tbl_PainRelief";
        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_PainRelief (
                    ID TEXT PRIMARY KEY,
                    partographid TEXT,
                    time TEXT NOT NULL,
                    handler TEXT,
                    notes TEXT NOT NULL,
                    painrelief TEXT,              
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
            
                CREATE INDEX IF NOT EXISTS idx_painrelief_sync ON Tbl_PainRelief(updatedtime, syncstatus);
                CREATE INDEX IF NOT EXISTS idx_painrelief_server_version ON Tbl_PainRelief(serverversion);

                DROP TRIGGER IF EXISTS trg_painrelief_insert;
                CREATE TRIGGER trg_painrelief_insert 
                AFTER INSERT ON Tbl_PainRelief
                WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
                BEGIN
                    UPDATE Tbl_PainRelief 
                    SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                        updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                    WHERE ID = NEW.ID;
                END;

                DROP TRIGGER IF EXISTS trg_painrelief_update;
                CREATE TRIGGER trg_painrelief_update 
                AFTER UPDATE ON Tbl_PainRelief
                WHEN NEW.updatedtime = OLD.updatedtime
                BEGIN
                    UPDATE Tbl_PainRelief 
                    SET updatedtime = (strftime('%s', 'now') * 1000),
                        version = OLD.version + 1,
                        syncstatus = 0
                    WHERE ID = NEW.ID;
                END;";

        public PainReliefRepository(ILogger<PainReliefRepository> logger) : base(logger) { }

        protected override PainReliefEntry MapFromReader(SqliteDataReader reader)
        {
            var idOrdinal = reader.GetOrdinal("ID");
            var partographidOrdinal = reader.GetOrdinal("partographid");
            var timeOrdinal = reader.GetOrdinal("time");
            var handlerOrdinal = reader.GetOrdinal("handler");
            var notesOrdinal = reader.GetOrdinal("notes");
            var painreliefOrdinal = reader.GetOrdinal("painrelief");
            var createdtimeOrdinal = reader.GetOrdinal("createdtime");
            var updatedtimeOrdinal = reader.GetOrdinal("updatedtime");
            var deletedtimeOrdinal = reader.GetOrdinal("deletedtime");
            var deviceidOrdinal = reader.GetOrdinal("deviceid");
            var origindeviceidOrdinal = reader.GetOrdinal("origindeviceid");
            var syncstatusOrdinal = reader.GetOrdinal("syncstatus");
            var versionOrdinal = reader.GetOrdinal("version");
            var serverversionOrdinal = reader.GetOrdinal("serverversion");
            var deletedOrdinal = reader.GetOrdinal("deleted");
            var conflictdataOrdinal = reader.GetOrdinal("conflictdata");
            var datahashOrdinal = reader.GetOrdinal("datahash");

            return new PainReliefEntry
            {
                ID = Guid.Parse(reader.GetString(idOrdinal)),
                PartographID = reader.IsDBNull(partographidOrdinal) ? null : Guid.Parse(reader.GetString(partographidOrdinal)),
                Time = DateTime.Parse(reader.GetString(timeOrdinal)),
                //Handler = reader.IsDBNull(handlerOrdinal) ? null : Guid.Parse(reader.GetString(handlerOrdinal)),
                HandlerName = reader.IsDBNull(handlerOrdinal) ? string.Empty : reader.GetString(handlerOrdinal),
                Notes = reader.GetString(notesOrdinal),
                PainRelief = reader.GetString(painreliefOrdinal),
                CreatedTime = reader.GetInt64(createdtimeOrdinal),
                UpdatedTime = reader.GetInt64(updatedtimeOrdinal),
                DeletedTime = reader.IsDBNull(deletedtimeOrdinal) ? null : reader.GetInt64(deletedtimeOrdinal),
                DeviceId = reader.GetString(deviceidOrdinal),
                OriginDeviceId = reader.GetString(origindeviceidOrdinal),
                SyncStatus = reader.GetInt32(syncstatusOrdinal),
                Version = reader.GetInt32(versionOrdinal),
                ServerVersion = reader.IsDBNull(serverversionOrdinal) ? 0 : reader.GetInt32(serverversionOrdinal),
                Deleted = reader.IsDBNull(deletedOrdinal) ? 0 : reader.GetInt32(deletedOrdinal),
                ConflictData = reader.IsDBNull(conflictdataOrdinal) ? string.Empty : reader.GetString(conflictdataOrdinal),
                DataHash = reader.IsDBNull(datahashOrdinal) ? string.Empty : reader.GetString(datahashOrdinal)
            };
        }

        protected override string GetInsertSql() => @"
            INSERT INTO Tbl_PainRelief (ID, partographID, time, handler, notes, painrelief, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash)
            VALUES (@id, @partographId, @time, @handler, @notes, @painrelief, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_PainRelief
        SET partographID = @partographId,
            time = @time, 
            handler = @handler,
            notes = @notes,
            painrelief = @painrelief,
            updatedtime = @updatedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, PainReliefEntry item)
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
            cmd.Parameters.AddWithValue("@handler", !string.IsNullOrWhiteSpace(item.Handler.ToString()) ? item.Handler.ToString() : new Guid("2bef74f2-a5cd-4f66-99fe-e36a08b29613").ToString());
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@painrelief", item.PainRelief);
            cmd.Parameters.AddWithValue("@createdtime", item.CreatedTime);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@origindeviceid", item.OriginDeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@serverversion", item.ServerVersion);
            cmd.Parameters.AddWithValue("@deleted", item.Deleted);
            //cmd.Parameters.AddWithValue("@conflictdata", item.Deleted);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, PainReliefEntry item)
        {
            //AddInsertParameters(cmd, item);
            cmd.Parameters.AddWithValue("@id", item.ID.ToString());
            cmd.Parameters.AddWithValue("@partographId", item.PartographID.ToString());
            cmd.Parameters.AddWithValue("@time", item.Time.ToString("O"));
            cmd.Parameters.AddWithValue("@handler", item.Handler.ToString());
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@painrelief", item.PainRelief);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            //cmd.Parameters.AddWithValue("@datahash", item.Deleted);
        }
    }
}