using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class IVFluidEntryRepository : BasePartographRepository<IVFluidEntry>
    {
        protected override string TableName => "Tbl_IVFluidEntry";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_IVFluidEntry (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                fluidtype TEXT NOT NULL,
                volumeinfused INTEGER NOT NULL,
                rate TEXT NOT NULL,
                starttime TEXT,
                additives TEXT NOT NULL,
                ivsite TEXT NOT NULL,
                sitehealthy INTEGER NOT NULL,
                sitecondition TEXT NOT NULL,
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

            CREATE INDEX idx_ivfluid_sync ON Tbl_IVFluidEntry(updatedtime, syncstatus);
            CREATE INDEX idx_ivfluid_server_version ON Tbl_IVFluidEntry(serverversion);

            CREATE TRIGGER trg_ivfluid_insert
            AFTER INSERT ON Tbl_IVFluidEntry
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_IVFluidEntry
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            CREATE TRIGGER trg_ivfluid_update
            AFTER UPDATE ON Tbl_IVFluidEntry
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_IVFluidEntry
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";

        public IVFluidEntryRepository(ILogger<IVFluidEntryRepository> logger) : base(logger) { }

        protected override IVFluidEntry MapFromReader(SqliteDataReader reader)
        {
            return new IVFluidEntry
            {
                ID = Guid.Parse(reader.GetString(0)),
                PartographID = reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                Time = reader.GetDateTime(2),
                Handler = reader.IsDBNull(3) ? null : Guid.Parse(reader.GetString(3)),
                Notes = reader.GetString(4),
                FluidType = reader.GetString(5),
                VolumeInfused = reader.GetInt32(6),
                Rate = reader.GetString(7),
                StartTime = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                Additives = reader.GetString(9),
                IVSite = reader.GetString(10),
                SiteHealthy = reader.GetBoolean(11),
                SiteCondition = reader.GetString(12),
                CreatedTime = reader.GetInt64(13),
                UpdatedTime = reader.GetInt64(14),
                DeletedTime = reader.IsDBNull(15) ? null : reader.GetInt64(15),
                DeviceId = reader.GetString(16),
                OriginDeviceId = reader.GetString(17),
                SyncStatus = reader.GetInt32(18),
                Version = reader.GetInt32(19),
                ServerVersion = reader.IsDBNull(20) ? 0 : reader.GetInt32(20),
                Deleted = reader.IsDBNull(21) ? 0 : reader.GetInt32(21),
                ConflictData = reader.GetString(22),
                DataHash = reader.GetString(23)
            };
        }

        protected override string GetInsertSql() => @"
        INSERT INTO Tbl_IVFluidEntry (ID, partographID, time, handler, notes, fluidtype, volumeinfused, rate, starttime, additives, ivsite, sitehealthy, sitecondition, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted)
        VALUES (@id, @partographId, @time, @handler, @notes, @fluidtype, @volumeinfused, @rate, @starttime, @additives, @ivsite, @sitehealthy, @sitecondition, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_IVFluidEntry
        SET partographID = @partographId,
            time = @time,
            handler = @handler,
            notes = @notes,
            fluidtype = @fluidtype,
            volumeinfused = @volumeinfused,
            rate = @rate,
            starttime = @starttime,
            additives = @additives,
            ivsite = @ivsite,
            sitehealthy = @sitehealthy,
            sitecondition = @sitecondition,
            updatedtime = @updatedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, IVFluidEntry item)
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
            cmd.Parameters.AddWithValue("@fluidtype", item.FluidType ?? "");
            cmd.Parameters.AddWithValue("@volumeinfused", item.VolumeInfused);
            cmd.Parameters.AddWithValue("@rate", item.Rate ?? "");
            cmd.Parameters.AddWithValue("@starttime", item.StartTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@additives", item.Additives ?? "");
            cmd.Parameters.AddWithValue("@ivsite", item.IVSite ?? "");
            cmd.Parameters.AddWithValue("@sitehealthy", item.SiteHealthy ? 1 : 0);
            cmd.Parameters.AddWithValue("@sitecondition", item.SiteCondition ?? "");
            cmd.Parameters.AddWithValue("@createdtime", item.CreatedTime);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@origindeviceid", item.OriginDeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@serverversion", item.ServerVersion);
            cmd.Parameters.AddWithValue("@deleted", item.Deleted);
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, IVFluidEntry item)
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
            cmd.Parameters.AddWithValue("@fluidtype", item.FluidType ?? "");
            cmd.Parameters.AddWithValue("@volumeinfused", item.VolumeInfused);
            cmd.Parameters.AddWithValue("@rate", item.Rate ?? "");
            cmd.Parameters.AddWithValue("@starttime", item.StartTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@additives", item.Additives ?? "");
            cmd.Parameters.AddWithValue("@ivsite", item.IVSite ?? "");
            cmd.Parameters.AddWithValue("@sitehealthy", item.SiteHealthy ? 1 : 0);
            cmd.Parameters.AddWithValue("@sitecondition", item.SiteCondition ?? "");
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
        }
    }
}
