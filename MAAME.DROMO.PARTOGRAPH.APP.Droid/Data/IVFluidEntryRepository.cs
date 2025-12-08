using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class IVFluidEntryRepository : BasePartographRepository<IVFluidEntry>
    {
        protected override string TableName => "Tbl_IVFluid";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_IVFluid (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                fluidtype TEXT NOT NULL,
                volumeinfused INTEGER NOT NULL,
                rate TEXT NOT NULL,
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

            CREATE INDEX IF NOT EXISTS idx_ivfluid_sync ON Tbl_IVFluid(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_ivfluid_server_version ON Tbl_IVFluid(serverversion);

            DROP TRIGGER IF EXISTS trg_ivfluid_insert;
            CREATE TRIGGER trg_ivfluid_insert
            AFTER INSERT ON Tbl_IVFluid
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_IVFluid
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_ivfluid_update;
            CREATE TRIGGER trg_ivfluid_update
            AFTER UPDATE ON Tbl_IVFluid
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_IVFluid
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
                //Handler = reader.IsDBNull(3) ? null : Guid.Parse(reader.GetString(3)),
                HandlerName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Notes = reader.GetString(4),
                FluidType = reader.GetString(5),
                VolumeInfused = reader.GetInt32(6),
                Rate = reader.GetString(7), 
                CreatedTime = reader.GetInt64(8),
                UpdatedTime = reader.GetInt64(9),
                DeletedTime = reader.IsDBNull(10) ? null : reader.GetInt64(10),
                DeviceId = reader.GetString(11),
                OriginDeviceId = reader.GetString(12),
                SyncStatus = reader.GetInt32(13),
                Version = reader.GetInt32(14),
                ServerVersion = reader.IsDBNull(15) ? 0 : reader.GetInt32(15),
                Deleted = reader.IsDBNull(16) ? 0 : reader.GetInt32(16),
                ConflictData = reader.IsDBNull(17) ? string.Empty : reader.GetString(17),
                DataHash = reader.IsDBNull(18) ? string.Empty : reader.GetString(18)
            };
        }

        protected override string GetInsertSql() => @"
        INSERT INTO Tbl_IVFluid (ID, partographID, time, handler, notes, fluidtype, volumeinfused, rate, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash)
        VALUES (@id, @partographId, @time, @handler, @notes, @fluidtype, @volumeinfused, @rate,  @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_IVFluid
        SET partographID = @partographId,
            time = @time,
            handler = @handler,
            notes = @notes,
            fluidtype = @fluidtype,
            volumeinfused = @volumeinfused,
            rate = @rate, 
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
            cmd.Parameters.AddWithValue("@time", item.Time.ToString());
            cmd.Parameters.AddWithValue("@handler", item.Handler.ToString());
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@fluidtype", item.FluidType ?? "");
            cmd.Parameters.AddWithValue("@volumeinfused", item.VolumeInfused);
            cmd.Parameters.AddWithValue("@rate", item.Rate ?? "");
            //cmd.Parameters.AddWithValue("@starttime", item.StartTime?.ToString("O") ?? (object)DBNull.Value);
            //cmd.Parameters.AddWithValue("@additives", item.Additives ?? "");
            //cmd.Parameters.AddWithValue("@ivsite", item.IVSite ?? "");
            //cmd.Parameters.AddWithValue("@sitehealthy", item.SiteHealthy ? 1 : 0);
            //cmd.Parameters.AddWithValue("@sitecondition", item.SiteCondition ?? "");
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
            cmd.Parameters.AddWithValue("@time", item.Time.ToString());
            cmd.Parameters.AddWithValue("@handler", item.Handler.ToString());
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@fluidtype", item.FluidType ?? "");
            cmd.Parameters.AddWithValue("@volumeinfused", item.VolumeInfused);
            cmd.Parameters.AddWithValue("@rate", item.Rate ?? "");
            //cmd.Parameters.AddWithValue("@starttime", item.StartTime?.ToString("O") ?? (object)DBNull.Value);
            //cmd.Parameters.AddWithValue("@additives", item.Additives ?? "");
            //cmd.Parameters.AddWithValue("@ivsite", item.IVSite ?? "");
            //cmd.Parameters.AddWithValue("@sitehealthy", item.SiteHealthy ? 1 : 0);
            //cmd.Parameters.AddWithValue("@sitecondition", item.SiteCondition ?? "");
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
