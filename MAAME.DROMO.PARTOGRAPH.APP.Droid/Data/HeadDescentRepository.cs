using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Baseline HeadDescent Repository
    public class HeadDescentRepository : BasePartographRepository<HeadDescent>
    {
        protected override string TableName => "Tbl_HeadDescent";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_HeadDescent (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                station INTEGER,
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
                datahash TEXT,
                palpableabdominally TEXT DEFAULT '',
                engaged INTEGER DEFAULT 0,
                synclitism TEXT DEFAULT 'Normal',
                flexion TEXT DEFAULT 'Flexed',
                visibleatintroitus INTEGER DEFAULT 0,
                crowning INTEGER DEFAULT 0,
                rotation TEXT DEFAULT '',
                descentrate TEXT DEFAULT '',
                descentregression INTEGER DEFAULT 0,
                clinicalalert TEXT DEFAULT ''
            );

            -- Add new columns to existing tables (WHO 2020 enhancements)
            ALTER TABLE Tbl_HeadDescent ADD COLUMN palpableabdominally TEXT DEFAULT '';
            ALTER TABLE Tbl_HeadDescent ADD COLUMN engaged INTEGER DEFAULT 0;
            ALTER TABLE Tbl_HeadDescent ADD COLUMN synclitism TEXT DEFAULT 'Normal';
            ALTER TABLE Tbl_HeadDescent ADD COLUMN flexion TEXT DEFAULT 'Flexed';
            ALTER TABLE Tbl_HeadDescent ADD COLUMN visibleatintroitus INTEGER DEFAULT 0;
            ALTER TABLE Tbl_HeadDescent ADD COLUMN crowning INTEGER DEFAULT 0;
            ALTER TABLE Tbl_HeadDescent ADD COLUMN rotation TEXT DEFAULT '';
            ALTER TABLE Tbl_HeadDescent ADD COLUMN descentrate TEXT DEFAULT '';
            ALTER TABLE Tbl_HeadDescent ADD COLUMN descentregression INTEGER DEFAULT 0;
            ALTER TABLE Tbl_HeadDescent ADD COLUMN clinicalalert TEXT DEFAULT '';

            CREATE INDEX IF NOT EXISTS idx_headdescent_sync ON Tbl_HeadDescent(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_headdescent_server_version ON Tbl_HeadDescent(serverversion);

            DROP TRIGGER IF EXISTS trg_headdescent_insert;
            CREATE TRIGGER trg_headdescent_insert 
            AFTER INSERT ON Tbl_HeadDescent
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_HeadDescent 
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_headdescent_update;
            CREATE TRIGGER trg_headdescent_update 
            AFTER UPDATE ON Tbl_HeadDescent
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_HeadDescent 
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";

        public HeadDescentRepository(ILogger<HeadDescentRepository> logger) : base(logger) { }

        protected override HeadDescent MapFromReader(SqliteDataReader reader)
        {
            var item = new HeadDescent
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = reader.IsDBNull(reader.GetOrdinal("handler")) ? string.Empty : reader.GetString(reader.GetOrdinal("handler")),
                Notes = reader.GetString(reader.GetOrdinal("notes")),
                Station = reader.IsDBNull(reader.GetOrdinal("station")) ? 5 : reader.GetInt32(reader.GetOrdinal("station")),
                CreatedTime = reader.GetInt64(reader.GetOrdinal("createdtime")),
                UpdatedTime = reader.GetInt64(reader.GetOrdinal("updatedtime")),
                DeletedTime = reader.IsDBNull(reader.GetOrdinal("deletedtime")) ? null : reader.GetInt64(reader.GetOrdinal("deletedtime")),
                DeviceId = reader.GetString(reader.GetOrdinal("deviceid")),
                OriginDeviceId = reader.GetString(reader.GetOrdinal("origindeviceid")),
                SyncStatus = reader.GetInt32(reader.GetOrdinal("syncstatus")),
                Version = reader.GetInt32(reader.GetOrdinal("version")),
                ServerVersion = reader.IsDBNull(reader.GetOrdinal("serverversion")) ? 0 : reader.GetInt32(reader.GetOrdinal("serverversion")),
                Deleted = reader.IsDBNull(reader.GetOrdinal("deleted")) ? 0 : reader.GetInt32(reader.GetOrdinal("deleted")),
                ConflictData = reader.IsDBNull(reader.GetOrdinal("conflictdata")) ? string.Empty : reader.GetString(reader.GetOrdinal("conflictdata")),
                DataHash = reader.IsDBNull(reader.GetOrdinal("datahash")) ? string.Empty : reader.GetString(reader.GetOrdinal("datahash"))
            };

            // WHO 2020 enhancements - safely read new columns
            try
            {
                item.PalpableAbdominally = GetStringOrDefault(reader, "palpableabdominally");
                item.Engaged = GetBoolFromInt(reader, "engaged");
                item.Synclitism = GetStringOrDefault(reader, "synclitism", "Normal");
                item.Flexion = GetStringOrDefault(reader, "flexion", "Flexed");
                item.VisibleAtIntroitus = GetBoolFromInt(reader, "visibleatintroitus");
                item.Crowning = GetBoolFromInt(reader, "crowning");
                item.Rotation = GetStringOrDefault(reader, "rotation");
                item.DescentRate = GetStringOrDefault(reader, "descentrate");
                item.DescentRegression = GetBoolFromInt(reader, "descentregression");
                item.ClinicalAlert = GetStringOrDefault(reader, "clinicalalert");
            }
            catch { /* Columns don't exist yet in old databases */ }

            return item;
        }

        private bool GetBoolFromInt(SqliteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return !reader.IsDBNull(ordinal) && reader.GetInt32(ordinal) == 1;
            }
            catch { return false; }
        }

        private string GetStringOrDefault(SqliteDataReader reader, string columnName, string defaultValue = "")
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : reader.GetString(ordinal);
            }
            catch { return defaultValue; }
        }

        protected override string GetInsertSql() => @"
        INSERT INTO Tbl_HeadDescent (ID, partographID, time, handler, notes, station, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash,
            palpableabdominally, engaged, synclitism, flexion, visibleatintroitus, crowning, rotation, descentrate, descentregression, clinicalalert)
        VALUES (@id, @partographId, @time, @handler, @notes, @station, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash,
            @palpableabdominally, @engaged, @synclitism, @flexion, @visibleatintroitus, @crowning, @rotation, @descentrate, @descentregression, @clinicalalert);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_HeadDescent
        SET partographID = @partographId,
            time = @time,
            handler = @handler,
            notes = @notes,
            station = @station,
            palpableabdominally = @palpableabdominally,
            engaged = @engaged,
            synclitism = @synclitism,
            flexion = @flexion,
            visibleatintroitus = @visibleatintroitus,
            crowning = @crowning,
            rotation = @rotation,
            descentrate = @descentrate,
            descentregression = @descentregression,
            clinicalalert = @clinicalalert,
            updatedtime = @updatedtime,
            deletedtime = @deletedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version,
            datahash = @datahash
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, HeadDescent item)
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
            cmd.Parameters.AddWithValue("@station", item.Station);
            cmd.Parameters.AddWithValue("@createdtime", item.CreatedTime);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@origindeviceid", item.OriginDeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@serverversion", item.ServerVersion);
            cmd.Parameters.AddWithValue("@deleted", item.Deleted);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@palpableabdominally", item.PalpableAbdominally ?? "");
            cmd.Parameters.AddWithValue("@engaged", item.Engaged ? 1 : 0);
            cmd.Parameters.AddWithValue("@synclitism", item.Synclitism ?? "Normal");
            cmd.Parameters.AddWithValue("@flexion", item.Flexion ?? "Flexed");
            cmd.Parameters.AddWithValue("@visibleatintroitus", item.VisibleAtIntroitus ? 1 : 0);
            cmd.Parameters.AddWithValue("@crowning", item.Crowning ? 1 : 0);
            cmd.Parameters.AddWithValue("@rotation", item.Rotation ?? "");
            cmd.Parameters.AddWithValue("@descentrate", item.DescentRate ?? "");
            cmd.Parameters.AddWithValue("@descentregression", item.DescentRegression ? 1 : 0);
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, HeadDescent item)
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
            cmd.Parameters.AddWithValue("@station", item.Station);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@palpableabdominally", item.PalpableAbdominally ?? "");
            cmd.Parameters.AddWithValue("@engaged", item.Engaged ? 1 : 0);
            cmd.Parameters.AddWithValue("@synclitism", item.Synclitism ?? "Normal");
            cmd.Parameters.AddWithValue("@flexion", item.Flexion ?? "Flexed");
            cmd.Parameters.AddWithValue("@visibleatintroitus", item.VisibleAtIntroitus ? 1 : 0);
            cmd.Parameters.AddWithValue("@crowning", item.Crowning ? 1 : 0);
            cmd.Parameters.AddWithValue("@rotation", item.Rotation ?? "");
            cmd.Parameters.AddWithValue("@descentrate", item.DescentRate ?? "");
            cmd.Parameters.AddWithValue("@descentregression", item.DescentRegression ? 1 : 0);
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }
    }
}
