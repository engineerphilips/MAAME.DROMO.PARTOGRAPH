using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using System;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class FetalPositionRepository : BasePartographRepository<FetalPosition>
    {
        protected override string TableName => "Tbl_FetalPosition";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_FetalPosition (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                position TEXT,
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
                lie TEXT DEFAULT 'Longitudinal',
                presentation TEXT DEFAULT 'Vertex',
                presentingpart TEXT DEFAULT '',
                variety TEXT DEFAULT '',
                flexion TEXT DEFAULT 'Flexed',
                engaged INTEGER DEFAULT 0,
                level TEXT DEFAULT '',
                assessmentmethod TEXT DEFAULT '',
                rotationprogress TEXT DEFAULT '',
                clinicalalert TEXT DEFAULT ''
            );

            CREATE INDEX IF NOT EXISTS idx_fetalposition_partographid ON Tbl_FetalPosition(partographid);
            CREATE INDEX IF NOT EXISTS idx_fetalposition_sync ON Tbl_FetalPosition(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_fetalposition_server_version ON Tbl_FetalPosition(serverversion);

            DROP TRIGGER IF EXISTS trg_companion_insert;
            CREATE TRIGGER trg_companion_insert
            AFTER INSERT ON Tbl_FetalPosition
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_FetalPosition
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_companion_update;
            CREATE TRIGGER trg_companion_update
            AFTER UPDATE ON Tbl_FetalPosition
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_FetalPosition
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";
                
        //    -- Add new columns to existing tables(WHO 2020 enhancements)
        //    ALTER TABLE Tbl_FetalPosition ADD COLUMN lie TEXT DEFAULT 'Longitudinal';
        //ALTER TABLE Tbl_FetalPosition ADD COLUMN presentation TEXT DEFAULT 'Vertex';
        //ALTER TABLE Tbl_FetalPosition ADD COLUMN presentingpart TEXT DEFAULT '';
        //ALTER TABLE Tbl_FetalPosition ADD COLUMN variety TEXT DEFAULT '';
        //ALTER TABLE Tbl_FetalPosition ADD COLUMN flexion TEXT DEFAULT 'Flexed';
        //ALTER TABLE Tbl_FetalPosition ADD COLUMN engaged INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_FetalPosition ADD COLUMN level TEXT DEFAULT '';
        //ALTER TABLE Tbl_FetalPosition ADD COLUMN assessmentmethod TEXT DEFAULT '';
        //ALTER TABLE Tbl_FetalPosition ADD COLUMN rotationprogress TEXT DEFAULT '';
        //ALTER TABLE Tbl_FetalPosition ADD COLUMN clinicalalert TEXT DEFAULT '';

        public FetalPositionRepository(ILogger<FetalPositionRepository> logger) : base(logger) { }

        protected override FetalPosition MapFromReader(SqliteDataReader reader)
        {
            var item = new FetalPosition
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = reader.IsDBNull(reader.GetOrdinal("staffname")) ? string.Empty : reader.GetString(reader.GetOrdinal("staffname")),
                Notes = reader.GetString(reader.GetOrdinal("notes")),
                Position = reader.IsDBNull(reader.GetOrdinal("position")) ? string.Empty : reader.GetString(reader.GetOrdinal("position")),
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
                item.Lie = GetStringOrDefault(reader, "lie", "Longitudinal");
                item.Presentation = GetStringOrDefault(reader, "presentation", "Vertex");
                item.PresentingPart = GetStringOrDefault(reader, "presentingpart");
                item.Variety = GetStringOrDefault(reader, "variety");
                item.Flexion = GetStringOrDefault(reader, "flexion", "Flexed");
                item.Engaged = GetBoolFromInt(reader, "engaged");
                item.Level = GetStringOrDefault(reader, "level");
                item.AssessmentMethod = GetStringOrDefault(reader, "assessmentmethod");
                item.RotationProgress = GetStringOrDefault(reader, "rotationprogress");
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
        INSERT INTO Tbl_FetalPosition (ID, partographID, time, handler, notes, position, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash,
            lie, presentation, presentingpart, variety, flexion, engaged, level, assessmentmethod, rotationprogress, clinicalalert)
        VALUES (@id, @partographId, @time, @handler, @notes, @position, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash,
            @lie, @presentation, @presentingpart, @variety, @flexion, @engaged, @level, @assessmentmethod, @rotationprogress, @clinicalalert);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_FetalPosition
        SET partographID = @partographId,
            time = @time,
            handler = @handler,
            notes = @notes,
            position = @position,
            lie = @lie,
            presentation = @presentation,
            presentingpart = @presentingpart,
            variety = @variety,
            flexion = @flexion,
            engaged = @engaged,
            level = @level,
            assessmentmethod = @assessmentmethod,
            rotationprogress = @rotationprogress,
            clinicalalert = @clinicalalert,
            updatedtime = @updatedtime,
            deletedtime = @deletedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version,
            datahash = @datahash
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, FetalPosition item)
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
            cmd.Parameters.AddWithValue("@position", item.Position);
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
            cmd.Parameters.AddWithValue("@lie", item.Lie ?? "Longitudinal");
            cmd.Parameters.AddWithValue("@presentation", item.Presentation ?? "Vertex");
            cmd.Parameters.AddWithValue("@presentingpart", item.PresentingPart ?? "");
            cmd.Parameters.AddWithValue("@variety", item.Variety ?? "");
            cmd.Parameters.AddWithValue("@flexion", item.Flexion ?? "Flexed");
            cmd.Parameters.AddWithValue("@engaged", item.Engaged ? 1 : 0);
            cmd.Parameters.AddWithValue("@level", item.Level ?? "");
            cmd.Parameters.AddWithValue("@assessmentmethod", item.AssessmentMethod ?? "");
            cmd.Parameters.AddWithValue("@rotationprogress", item.RotationProgress ?? "");
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, FetalPosition item)
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
            cmd.Parameters.AddWithValue("@position", item.Position);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@lie", item.Lie ?? "Longitudinal");
            cmd.Parameters.AddWithValue("@presentation", item.Presentation ?? "Vertex");
            cmd.Parameters.AddWithValue("@presentingpart", item.PresentingPart ?? "");
            cmd.Parameters.AddWithValue("@variety", item.Variety ?? "");
            cmd.Parameters.AddWithValue("@flexion", item.Flexion ?? "Flexed");
            cmd.Parameters.AddWithValue("@engaged", item.Engaged ? 1 : 0);
            cmd.Parameters.AddWithValue("@level", item.Level ?? "");
            cmd.Parameters.AddWithValue("@assessmentmethod", item.AssessmentMethod ?? "");
            cmd.Parameters.AddWithValue("@rotationprogress", item.RotationProgress ?? "");
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }
    }
}
