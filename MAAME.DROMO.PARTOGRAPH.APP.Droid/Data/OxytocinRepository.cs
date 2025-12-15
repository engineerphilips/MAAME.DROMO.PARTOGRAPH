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
                datahash TEXT,
                inuse INTEGER DEFAULT 0,
                starttime TEXT,
                stoptime TEXT,
                concentrationmunitsperml REAL DEFAULT 10,
                infusionratemlperhour REAL DEFAULT 0,
                indication TEXT DEFAULT '',
                contraindicationschecked INTEGER DEFAULT 0,
                contraindicationspresent INTEGER DEFAULT 0,
                contraindicationdetails TEXT DEFAULT '',
                response TEXT DEFAULT '',
                dosetitration TEXT DEFAULT '',
                timetonextincrease INTEGER DEFAULT 0,
                maxdosereached INTEGER DEFAULT 0,
                stoppedreason TEXT DEFAULT '',
                clinicalalert TEXT DEFAULT ''
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
                
        //    -- Add new columns to existing tables(WHO 2020 enhancements)
        //    ALTER TABLE Tbl_Oxytocin ADD COLUMN inuse INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Oxytocin ADD COLUMN starttime TEXT;
        //    ALTER TABLE Tbl_Oxytocin ADD COLUMN stoptime TEXT;
        //    ALTER TABLE Tbl_Oxytocin ADD COLUMN concentrationmunitsperml REAL DEFAULT 10;
        //ALTER TABLE Tbl_Oxytocin ADD COLUMN infusionratemlperhour REAL DEFAULT 0;
        //ALTER TABLE Tbl_Oxytocin ADD COLUMN indication TEXT DEFAULT '';
        //ALTER TABLE Tbl_Oxytocin ADD COLUMN contraindicationschecked INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Oxytocin ADD COLUMN contraindicationspresent INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Oxytocin ADD COLUMN contraindicationdetails TEXT DEFAULT '';
        //ALTER TABLE Tbl_Oxytocin ADD COLUMN response TEXT DEFAULT '';
        //ALTER TABLE Tbl_Oxytocin ADD COLUMN dosetitration TEXT DEFAULT '';
        //ALTER TABLE Tbl_Oxytocin ADD COLUMN timetonextincrease INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Oxytocin ADD COLUMN maxdosereached INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Oxytocin ADD COLUMN stoppedreason TEXT DEFAULT '';
        //ALTER TABLE Tbl_Oxytocin ADD COLUMN clinicalalert TEXT DEFAULT '';

        public OxytocinRepository(ILogger<OxytocinRepository> logger) : base(logger) { }

        protected override Oxytocin MapFromReader(SqliteDataReader reader)
        {
            var item = new Oxytocin
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = reader.IsDBNull(reader.GetOrdinal("handler")) ? string.Empty : reader.GetString(reader.GetOrdinal("handler")),
                Notes = reader.GetString(reader.GetOrdinal("notes")),
                DoseMUnitsPerMin = (decimal)reader.GetDouble(reader.GetOrdinal("dosemunitspermin")),
                TotalVolumeInfused = (decimal)reader.GetDouble(reader.GetOrdinal("totalvolumeinfused")),
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
                item.InUse = GetBoolFromInt(reader, "inuse");
                item.StartTime = GetNullableDateTime(reader, "starttime");
                item.StopTime = GetNullableDateTime(reader, "stoptime");
                item.ConcentrationMUnitsPerMl = GetDecimalOrDefault(reader, "concentrationmunitsperml", 10);
                item.InfusionRateMlPerHour = GetDecimalOrDefault(reader, "infusionratemlperhour", 0);
                item.Indication = GetStringOrDefault(reader, "indication");
                item.ContraindicationsChecked = GetBoolFromInt(reader, "contraindicationschecked");
                item.ContraindicationsPresent = GetBoolFromInt(reader, "contraindicationspresent");
                item.ContraindicationDetails = GetStringOrDefault(reader, "contraindicationdetails");
                item.Response = GetStringOrDefault(reader, "response");
                item.DoseTitration = GetStringOrDefault(reader, "dosetitration");
                item.TimeToNextIncrease = GetIntOrDefault(reader, "timetonextincrease", 0);
                item.MaxDoseReached = GetBoolFromInt(reader, "maxdosereached");
                item.StoppedReason = GetStringOrDefault(reader, "stoppedreason");
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

        private DateTime? GetNullableDateTime(SqliteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal)) return null;
                return DateTime.Parse(reader.GetString(ordinal));
            }
            catch { return null; }
        }

        private decimal GetDecimalOrDefault(SqliteDataReader reader, string columnName, decimal defaultValue = 0)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : (decimal)reader.GetDouble(ordinal);
            }
            catch { return defaultValue; }
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

        private int GetIntOrDefault(SqliteDataReader reader, string columnName, int defaultValue = 0)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : reader.GetInt32(ordinal);
            }
            catch { return defaultValue; }
        }

        protected override string GetInsertSql() => @"
        INSERT INTO Tbl_Oxytocin (ID, partographID, time, handler, notes, dosemunitspermin, totalvolumeinfused, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash,
            inuse, starttime, stoptime, concentrationmunitsperml, infusionratemlperhour, indication, contraindicationschecked, contraindicationspresent, contraindicationdetails,
            response, dosetitration, timetonextincrease, maxdosereached, stoppedreason, clinicalalert)
        VALUES (@id, @partographId, @time, @handler, @notes, @dosemunitspermin, @totalvolumeinfused, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash,
            @inuse, @starttime, @stoptime, @concentrationmunitsperml, @infusionratemlperhour, @indication, @contraindicationschecked, @contraindicationspresent, @contraindicationdetails,
            @response, @dosetitration, @timetonextincrease, @maxdosereached, @stoppedreason, @clinicalalert);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_Oxytocin
        SET partographID = @partographId,
            time = @time,
            handler = @handler,
            notes = @notes,
            dosemunitspermin = @dosemunitspermin,
            totalvolumeinfused = @totalvolumeinfused,
            inuse = @inuse,
            starttime = @starttime,
            stoptime = @stoptime,
            concentrationmunitsperml = @concentrationmunitsperml,
            infusionratemlperhour = @infusionratemlperhour,
            indication = @indication,
            contraindicationschecked = @contraindicationschecked,
            contraindicationspresent = @contraindicationspresent,
            contraindicationdetails = @contraindicationdetails,
            response = @response,
            dosetitration = @dosetitration,
            timetonextincrease = @timetonextincrease,
            maxdosereached = @maxdosereached,
            stoppedreason = @stoppedreason,
            clinicalalert = @clinicalalert,
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
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@inuse", item.InUse ? 1 : 0);
            cmd.Parameters.AddWithValue("@starttime", item.StartTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@stoptime", item.StopTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@concentrationmunitsperml", (double)item.ConcentrationMUnitsPerMl);
            cmd.Parameters.AddWithValue("@infusionratemlperhour", (double)item.InfusionRateMlPerHour);
            cmd.Parameters.AddWithValue("@indication", item.Indication ?? "");
            cmd.Parameters.AddWithValue("@contraindicationschecked", item.ContraindicationsChecked ? 1 : 0);
            cmd.Parameters.AddWithValue("@contraindicationspresent", item.ContraindicationsPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@contraindicationdetails", item.ContraindicationDetails ?? "");
            cmd.Parameters.AddWithValue("@response", item.Response ?? "");
            cmd.Parameters.AddWithValue("@dosetitration", item.DoseTitration ?? "");
            cmd.Parameters.AddWithValue("@timetonextincrease", item.TimeToNextIncrease);
            cmd.Parameters.AddWithValue("@maxdosereached", item.MaxDoseReached ? 1 : 0);
            cmd.Parameters.AddWithValue("@stoppedreason", item.StoppedReason ?? "");
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
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
