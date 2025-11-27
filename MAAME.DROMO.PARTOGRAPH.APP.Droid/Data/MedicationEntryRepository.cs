using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class MedicationEntryRepository : BasePartographRepository<MedicationEntry>
    {
        protected override string TableName => "Tbl_MedicationEntry";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_MedicationEntry (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                medicationname TEXT NOT NULL,
                dose TEXT NOT NULL,
                route TEXT NOT NULL,
                administrationtime TEXT NOT NULL,
                indication TEXT NOT NULL,
                prescribedby TEXT NOT NULL,
                response TEXT NOT NULL,
                adversereaction INTEGER NOT NULL,
                adversereactiondetails TEXT NOT NULL,
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

            CREATE INDEX idx_medication_sync ON Tbl_MedicationEntry(updatedtime, syncstatus);
            CREATE INDEX idx_medication_server_version ON Tbl_MedicationEntry(serverversion);

            CREATE TRIGGER trg_medication_insert
            AFTER INSERT ON Tbl_MedicationEntry
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_MedicationEntry
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            CREATE TRIGGER trg_medication_update
            AFTER UPDATE ON Tbl_MedicationEntry
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_MedicationEntry
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";

        public MedicationEntryRepository(ILogger<MedicationEntryRepository> logger) : base(logger) { }

        protected override MedicationEntry MapFromReader(SqliteDataReader reader)
        {
            return new MedicationEntry
            {
                ID = Guid.Parse(reader.GetString(0)),
                PartographID = reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                Time = reader.GetDateTime(2),
                Handler = reader.IsDBNull(3) ? null : Guid.Parse(reader.GetString(3)),
                Notes = reader.GetString(4),
                MedicationName = reader.GetString(5),
                Dose = reader.GetString(6),
                Route = reader.GetString(7),
                AdministrationTime = reader.GetDateTime(8),
                Indication = reader.GetString(9),
                PrescribedBy = reader.GetString(10),
                Response = reader.GetString(11),
                AdverseReaction = reader.GetBoolean(12),
                AdverseReactionDetails = reader.GetString(13),
                CreatedTime = reader.GetInt64(14),
                UpdatedTime = reader.GetInt64(15),
                DeletedTime = reader.IsDBNull(16) ? null : reader.GetInt64(16),
                DeviceId = reader.GetString(17),
                OriginDeviceId = reader.GetString(18),
                SyncStatus = reader.GetInt32(19),
                Version = reader.GetInt32(20),
                ServerVersion = reader.IsDBNull(21) ? 0 : reader.GetInt32(21),
                Deleted = reader.IsDBNull(22) ? 0 : reader.GetInt32(22),
                ConflictData = reader.GetString(23),
                DataHash = reader.GetString(24)
            };
        }

        protected override string GetInsertSql() => @"
        INSERT INTO Tbl_MedicationEntry (ID, partographID, time, handler, notes, medicationname, dose, route, administrationtime, indication, prescribedby, response, adversereaction, adversereactiondetails, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted)
        VALUES (@id, @partographId, @time, @handler, @notes, @medicationname, @dose, @route, @administrationtime, @indication, @prescribedby, @response, @adversereaction, @adversereactiondetails, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_MedicationEntry
        SET partographID = @partographId,
            time = @time,
            handler = @handler,
            notes = @notes,
            medicationname = @medicationname,
            dose = @dose,
            route = @route,
            administrationtime = @administrationtime,
            indication = @indication,
            prescribedby = @prescribedby,
            response = @response,
            adversereaction = @adversereaction,
            adversereactiondetails = @adversereactiondetails,
            updatedtime = @updatedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, MedicationEntry item)
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
            cmd.Parameters.AddWithValue("@medicationname", item.MedicationName ?? "");
            cmd.Parameters.AddWithValue("@dose", item.Dose ?? "");
            cmd.Parameters.AddWithValue("@route", item.Route ?? "");
            cmd.Parameters.AddWithValue("@administrationtime", item.AdministrationTime.ToString("O"));
            cmd.Parameters.AddWithValue("@indication", item.Indication ?? "");
            cmd.Parameters.AddWithValue("@prescribedby", item.PrescribedBy ?? "");
            cmd.Parameters.AddWithValue("@response", item.Response ?? "");
            cmd.Parameters.AddWithValue("@adversereaction", item.AdverseReaction ? 1 : 0);
            cmd.Parameters.AddWithValue("@adversereactiondetails", item.AdverseReactionDetails ?? "");
            cmd.Parameters.AddWithValue("@createdtime", item.CreatedTime);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@origindeviceid", item.OriginDeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@serverversion", item.ServerVersion);
            cmd.Parameters.AddWithValue("@deleted", item.Deleted);
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, MedicationEntry item)
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
            cmd.Parameters.AddWithValue("@medicationname", item.MedicationName ?? "");
            cmd.Parameters.AddWithValue("@dose", item.Dose ?? "");
            cmd.Parameters.AddWithValue("@route", item.Route ?? "");
            cmd.Parameters.AddWithValue("@administrationtime", item.AdministrationTime.ToString("O"));
            cmd.Parameters.AddWithValue("@indication", item.Indication ?? "");
            cmd.Parameters.AddWithValue("@prescribedby", item.PrescribedBy ?? "");
            cmd.Parameters.AddWithValue("@response", item.Response ?? "");
            cmd.Parameters.AddWithValue("@adversereaction", item.AdverseReaction ? 1 : 0);
            cmd.Parameters.AddWithValue("@adversereactiondetails", item.AdverseReactionDetails ?? "");
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
        }
    }
}
