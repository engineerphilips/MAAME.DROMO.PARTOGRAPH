using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using System;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Caput Succedaneum Repository
    public class CaputRepository : BasePartographRepository<Caput>
    {
        protected override string TableName => "Tbl_Caput";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_Caput (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                degree TEXT,
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
                location TEXT DEFAULT '',
                size TEXT DEFAULT '',
                consistency TEXT DEFAULT '',
                increasing INTEGER DEFAULT 0,
                decreasing INTEGER DEFAULT 0,
                stable INTEGER DEFAULT 0,
                progressionrate TEXT DEFAULT '',
                firstdetectedtime TEXT,
                durationhours INTEGER,
                mouldingpresent INTEGER DEFAULT 0,
                mouldingdegree TEXT DEFAULT '',
                suggestsobstruction INTEGER DEFAULT 0,
                suggestionprolongedlabor INTEGER DEFAULT 0,
                changefromprevious TEXT DEFAULT '',
                clinicalalert TEXT DEFAULT ''
            );

            CREATE INDEX IF NOT EXISTS idx_caput_partographid ON Tbl_Caput(partographid);
            CREATE INDEX IF NOT EXISTS idx_caput_sync ON Tbl_Caput(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_caput_server_version ON Tbl_Caput(serverversion);

            DROP TRIGGER IF EXISTS trg_caput_insert;
            CREATE TRIGGER trg_caput_insert
            AFTER INSERT ON Tbl_Caput
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_Caput
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_caput_update;
            CREATE TRIGGER trg_caput_update
            AFTER UPDATE ON Tbl_Caput
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_Caput
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";
        
        //    -- Add new columns to existing tables(WHO 2020 enhancements)
        //    ALTER TABLE Tbl_Caput ADD COLUMN location TEXT DEFAULT '';
        //ALTER TABLE Tbl_Caput ADD COLUMN size TEXT DEFAULT '';
        //ALTER TABLE Tbl_Caput ADD COLUMN consistency TEXT DEFAULT '';
        //ALTER TABLE Tbl_Caput ADD COLUMN increasing INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Caput ADD COLUMN decreasing INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Caput ADD COLUMN stable INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Caput ADD COLUMN progressionrate TEXT DEFAULT '';
        //ALTER TABLE Tbl_Caput ADD COLUMN firstdetectedtime TEXT;
        //    ALTER TABLE Tbl_Caput ADD COLUMN durationhours INTEGER;
        //    ALTER TABLE Tbl_Caput ADD COLUMN mouldingpresent INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Caput ADD COLUMN mouldingdegree TEXT DEFAULT '';
        //ALTER TABLE Tbl_Caput ADD COLUMN suggestsobstruction INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Caput ADD COLUMN suggestionprolongedlabor INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Caput ADD COLUMN changefromprevious TEXT DEFAULT '';
        //ALTER TABLE Tbl_Caput ADD COLUMN clinicalalert TEXT DEFAULT '';

        public CaputRepository(ILogger<CaputRepository> logger) : base(logger) { }

        protected override Caput MapFromReader(SqliteDataReader reader)
        {
            var item = new Caput
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = reader.IsDBNull(reader.GetOrdinal("staffname")) ? string.Empty : reader.GetString(reader.GetOrdinal("staffname")),
                Notes = reader.GetString(reader.GetOrdinal("notes")),
                Degree = reader.IsDBNull(reader.GetOrdinal("degree")) ? null : reader.GetString(reader.GetOrdinal("degree")),
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
                item.Location = GetStringOrDefault(reader, "location");
                item.Size = GetStringOrDefault(reader, "size");
                item.Consistency = GetStringOrDefault(reader, "consistency");
                item.Increasing = GetBoolFromInt(reader, "increasing");
                item.Decreasing = GetBoolFromInt(reader, "decreasing");
                item.Stable = GetBoolFromInt(reader, "stable");
                item.ProgressionRate = GetStringOrDefault(reader, "progressionrate");
                item.FirstDetectedTime = GetNullableDateTime(reader, "firstdetectedtime");
                item.DurationHours = GetNullableInt(reader, "durationhours");
                item.MouldingPresent = GetBoolFromInt(reader, "mouldingpresent");
                item.MouldingDegree = GetStringOrDefault(reader, "mouldingdegree");
                item.SuggestsObstruction = GetBoolFromInt(reader, "suggestsobstruction");
                item.SuggestionProlongedLabor = GetBoolFromInt(reader, "suggestionprolongedlabor");
                item.ChangeFromPrevious = GetStringOrDefault(reader, "changefromprevious");
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

        private int? GetNullableInt(SqliteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
            }
            catch { return null; }
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

        protected override string GetInsertSql() => @"
        INSERT INTO Tbl_Caput (ID, partographID, time, handler, notes, degree, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash,
            location, size, consistency, increasing, decreasing, stable, progressionrate, firstdetectedtime, durationhours, mouldingpresent, mouldingdegree, suggestsobstruction, suggestionprolongedlabor, changefromprevious, clinicalalert)
        VALUES (@id, @partographId, @time, @handler, @notes, @degree, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash,
            @location, @size, @consistency, @increasing, @decreasing, @stable, @progressionrate, @firstdetectedtime, @durationhours, @mouldingpresent, @mouldingdegree, @suggestsobstruction, @suggestionprolongedlabor, @changefromprevious, @clinicalalert);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_Caput
        SET partographID = @partographId,
            time = @time,
            handler = @handler,
            notes = @notes,
            degree = @degree,
            location = @location,
            size = @size,
            consistency = @consistency,
            increasing = @increasing,
            decreasing = @decreasing,
            stable = @stable,
            progressionrate = @progressionrate,
            firstdetectedtime = @firstdetectedtime,
            durationhours = @durationhours,
            mouldingpresent = @mouldingpresent,
            mouldingdegree = @mouldingdegree,
            suggestsobstruction = @suggestsobstruction,
            suggestionprolongedlabor = @suggestionprolongedlabor,
            changefromprevious = @changefromprevious,
            clinicalalert = @clinicalalert,
            updatedtime = @updatedtime,
            deletedtime = @deletedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version,
            datahash = @datahash
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, Caput item)
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
            cmd.Parameters.AddWithValue("@degree", item.Degree ?? "");
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
            cmd.Parameters.AddWithValue("@location", item.Location ?? "");
            cmd.Parameters.AddWithValue("@size", item.Size ?? "");
            cmd.Parameters.AddWithValue("@consistency", item.Consistency ?? "");
            cmd.Parameters.AddWithValue("@increasing", item.Increasing ? 1 : 0);
            cmd.Parameters.AddWithValue("@decreasing", item.Decreasing ? 1 : 0);
            cmd.Parameters.AddWithValue("@stable", item.Stable ? 1 : 0);
            cmd.Parameters.AddWithValue("@progressionrate", item.ProgressionRate ?? "");
            cmd.Parameters.AddWithValue("@firstdetectedtime", item.FirstDetectedTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@durationhours", item.DurationHours ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@mouldingpresent", item.MouldingPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@mouldingdegree", item.MouldingDegree ?? "");
            cmd.Parameters.AddWithValue("@suggestsobstruction", item.SuggestsObstruction ? 1 : 0);
            cmd.Parameters.AddWithValue("@suggestionprolongedlabor", item.SuggestionProlongedLabor ? 1 : 0);
            cmd.Parameters.AddWithValue("@changefromprevious", item.ChangeFromPrevious ?? "");
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, Caput item)
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
            cmd.Parameters.AddWithValue("@degree", item.Degree ?? "");
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@location", item.Location ?? "");
            cmd.Parameters.AddWithValue("@size", item.Size ?? "");
            cmd.Parameters.AddWithValue("@consistency", item.Consistency ?? "");
            cmd.Parameters.AddWithValue("@increasing", item.Increasing ? 1 : 0);
            cmd.Parameters.AddWithValue("@decreasing", item.Decreasing ? 1 : 0);
            cmd.Parameters.AddWithValue("@stable", item.Stable ? 1 : 0);
            cmd.Parameters.AddWithValue("@progressionrate", item.ProgressionRate ?? "");
            cmd.Parameters.AddWithValue("@firstdetectedtime", item.FirstDetectedTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@durationhours", item.DurationHours ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@mouldingpresent", item.MouldingPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@mouldingdegree", item.MouldingDegree ?? "");
            cmd.Parameters.AddWithValue("@suggestsobstruction", item.SuggestsObstruction ? 1 : 0);
            cmd.Parameters.AddWithValue("@suggestionprolongedlabor", item.SuggestionProlongedLabor ? 1 : 0);
            cmd.Parameters.AddWithValue("@changefromprevious", item.ChangeFromPrevious ?? "");
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }
    }
}
