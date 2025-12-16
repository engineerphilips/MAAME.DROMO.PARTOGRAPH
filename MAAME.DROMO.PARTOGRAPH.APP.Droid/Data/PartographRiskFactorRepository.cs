using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class PartographRiskFactorRepository : BasePartographRepository<PartographRiskFactor>
    {
        protected override string TableName => "Tbl_PartographRiskFactor";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_PartographRiskFactor (
                    ID TEXT PRIMARY KEY,
                    partographid TEXT,
                    time TEXT NOT NULL,
                    name TEXT NOT NULL,
                    handler TEXT,
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

                CREATE INDEX IF NOT EXISTS idx_partographriskfactor_sync ON Tbl_PartographRiskFactor(updatedtime, syncstatus);
                CREATE INDEX IF NOT EXISTS idx_partographriskfactor_server_version ON Tbl_PartographRiskFactor(serverversion);

                DROP TRIGGER IF EXISTS trg_partographriskfactor_insert;
                CREATE TRIGGER trg_partographriskfactor_insert
                AFTER INSERT ON Tbl_PartographRiskFactor
                WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
                BEGIN
                    UPDATE Tbl_PartographRiskFactor
                    SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                        updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                    WHERE ID = NEW.ID;
                END;

                DROP TRIGGER IF EXISTS trg_partographriskfactor_update;
                CREATE TRIGGER trg_partographriskfactor_update
                AFTER UPDATE ON Tbl_PartographRiskFactor
                WHEN NEW.updatedtime = OLD.updatedtime
                BEGIN
                    UPDATE Tbl_PartographRiskFactor
                    SET updatedtime = (strftime('%s', 'now') * 1000),
                        version = OLD.version + 1,
                        syncstatus = 0
                    WHERE ID = NEW.ID;
                END;
            ";

        public PartographRiskFactorRepository(ILogger<PartographRiskFactorRepository> logger) : base(logger) { }

        protected override PartographRiskFactor MapFromReader(SqliteDataReader reader)
        {
            var idOrdinal = reader.GetOrdinal("ID");
            var partographidOrdinal = reader.GetOrdinal("partographid");
            var timeOrdinal = reader.GetOrdinal("time");
            var nameOrdinal = reader.GetOrdinal("name");
            var handlerOrdinal = reader.GetOrdinal("handler");
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

            return new PartographRiskFactor
            {
                ID = Guid.Parse(reader.GetString(idOrdinal)),
                PartographID = reader.IsDBNull(partographidOrdinal) ? null : Guid.Parse(reader.GetString(partographidOrdinal)),
                Time = reader.GetDateTime(timeOrdinal),
                //Handler = reader.IsDBNull(handlerOrdinal) ? null : Guid.Parse(reader.GetString(handlerOrdinal)),
                HandlerName = reader.IsDBNull(handlerOrdinal) ? string.Empty : reader.GetString(handlerOrdinal),
                Name = reader.GetString(nameOrdinal),
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
        INSERT INTO Tbl_PartographRiskFactor (ID, partographID, time, handler, name, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash)
        VALUES (@id, @partographId, @time, @handler, @name, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_PartographRiskFactor
        SET partographID = @partographId,
            time = @time, 
            handler = @handler,
            name = @name, 
            updatedtime = @updatedtime,
            deletedtime = @deletedtime, 
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version,
            datahash = @datahash
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, PartographRiskFactor item)
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
            cmd.Parameters.AddWithValue("@name", item.Name ?? "");
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

        protected override void AddUpdateParameters(SqliteCommand cmd, PartographRiskFactor item)
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
            cmd.Parameters.AddWithValue("@name", item.Name ?? ""); 
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);
        }
    }
}
