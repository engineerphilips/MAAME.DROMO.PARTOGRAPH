using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using System;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class MedicalNoteRepository : BasePartographRepository<MedicalNote>
    {
        protected override string TableName => "Tbl_MedicalNote";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_MedicalNote (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                notetype TEXT DEFAULT 'General',
                content TEXT DEFAULT '',
                createdby TEXT DEFAULT '',
                isimportant INTEGER DEFAULT 0,
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
                role TEXT DEFAULT '',
                urgencylevel TEXT DEFAULT '',
                clinicalcategory TEXT DEFAULT '',
                whosection TEXT DEFAULT '',
                linkedmeasurabletype TEXT DEFAULT '',
                linkedmeasurableid TEXT,
                linkedmeasurabletime TEXT,
                requiresreview INTEGER DEFAULT 0,
                requiresfollowup INTEGER DEFAULT 0,
                reviewedtime TEXT,
                reviewedby TEXT DEFAULT '',
                reviewoutcome TEXT DEFAULT '',
                escalated INTEGER DEFAULT 0,
                escalatedto TEXT DEFAULT '',
                escalationtime TEXT,
                escalationreason TEXT DEFAULT '',
                includeinhandover INTEGER DEFAULT 0,
                communicatedtopatient INTEGER DEFAULT 0,
                communicatedtocompanion INTEGER DEFAULT 0,
                attachmentpath TEXT DEFAULT '',
                referencedocument TEXT DEFAULT '',
                clinicalalert TEXT DEFAULT ''
            );

            -- Add new columns to existing tables (WHO 2020 enhancements)
            ALTER TABLE Tbl_MedicalNote ADD COLUMN role TEXT DEFAULT '';
            ALTER TABLE Tbl_MedicalNote ADD COLUMN urgencylevel TEXT DEFAULT '';
            ALTER TABLE Tbl_MedicalNote ADD COLUMN clinicalcategory TEXT DEFAULT '';
            ALTER TABLE Tbl_MedicalNote ADD COLUMN whosection TEXT DEFAULT '';
            ALTER TABLE Tbl_MedicalNote ADD COLUMN linkedmeasurabletype TEXT DEFAULT '';
            ALTER TABLE Tbl_MedicalNote ADD COLUMN linkedmeasurableid TEXT;
            ALTER TABLE Tbl_MedicalNote ADD COLUMN linkedmeasurabletime TEXT;
            ALTER TABLE Tbl_MedicalNote ADD COLUMN requiresreview INTEGER DEFAULT 0;
            ALTER TABLE Tbl_MedicalNote ADD COLUMN requiresfollowup INTEGER DEFAULT 0;
            ALTER TABLE Tbl_MedicalNote ADD COLUMN reviewedtime TEXT;
            ALTER TABLE Tbl_MedicalNote ADD COLUMN reviewedby TEXT DEFAULT '';
            ALTER TABLE Tbl_MedicalNote ADD COLUMN reviewoutcome TEXT DEFAULT '';
            ALTER TABLE Tbl_MedicalNote ADD COLUMN escalated INTEGER DEFAULT 0;
            ALTER TABLE Tbl_MedicalNote ADD COLUMN escalatedto TEXT DEFAULT '';
            ALTER TABLE Tbl_MedicalNote ADD COLUMN escalationtime TEXT;
            ALTER TABLE Tbl_MedicalNote ADD COLUMN escalationreason TEXT DEFAULT '';
            ALTER TABLE Tbl_MedicalNote ADD COLUMN includeinhandover INTEGER DEFAULT 0;
            ALTER TABLE Tbl_MedicalNote ADD COLUMN communicatedtopatient INTEGER DEFAULT 0;
            ALTER TABLE Tbl_MedicalNote ADD COLUMN communicatedtocompanion INTEGER DEFAULT 0;
            ALTER TABLE Tbl_MedicalNote ADD COLUMN attachmentpath TEXT DEFAULT '';
            ALTER TABLE Tbl_MedicalNote ADD COLUMN referencedocument TEXT DEFAULT '';
            ALTER TABLE Tbl_MedicalNote ADD COLUMN clinicalalert TEXT DEFAULT '';

            CREATE INDEX IF NOT EXISTS idx_medicalnote_sync ON Tbl_MedicalNote(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_medicalnote_server_version ON Tbl_MedicalNote(serverversion);

            DROP TRIGGER IF EXISTS trg_medicalnote_insert;
            CREATE TRIGGER trg_medicalnote_insert
            AFTER INSERT ON Tbl_MedicalNote
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_MedicalNote
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_medicalnote_update;
            CREATE TRIGGER trg_medicalnote_update
            AFTER UPDATE ON Tbl_MedicalNote
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_MedicalNote
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";

        public MedicalNoteRepository(ILogger<MedicalNoteRepository> logger) : base(logger) { }

        protected override MedicalNote MapFromReader(SqliteDataReader reader)
        {
            var item = new MedicalNote
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = reader.IsDBNull(reader.GetOrdinal("staffname")) ? string.Empty : reader.GetString(reader.GetOrdinal("staffname")),
                Notes = reader.GetString(reader.GetOrdinal("notes")),
                NoteType = GetStringOrDefault(reader, "notetype", "General"),
                Content = GetStringOrDefault(reader, "content"),
                CreatedBy = GetStringOrDefault(reader, "createdby"),
                IsImportant = GetBoolFromInt(reader, "isimportant"),
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
                item.Role = GetStringOrDefault(reader, "role");
                item.UrgencyLevel = GetStringOrDefault(reader, "urgencylevel");
                item.ClinicalCategory = GetStringOrDefault(reader, "clinicalcategory");
                item.WHOSection = GetStringOrDefault(reader, "whosection");
                item.LinkedMeasurableType = GetStringOrDefault(reader, "linkedmeasurabletype");
                item.LinkedMeasurableID = GetNullableGuid(reader, "linkedmeasurableid");
                item.LinkedMeasurableTime = GetNullableDateTime(reader, "linkedmeasurabletime");
                item.RequiresReview = GetBoolFromInt(reader, "requiresreview");
                item.RequiresFollowUp = GetBoolFromInt(reader, "requiresfollowup");
                item.ReviewedTime = GetNullableDateTime(reader, "reviewedtime");
                item.ReviewedBy = GetStringOrDefault(reader, "reviewedby");
                item.ReviewOutcome = GetStringOrDefault(reader, "reviewoutcome");
                item.Escalated = GetBoolFromInt(reader, "escalated");
                item.EscalatedTo = GetStringOrDefault(reader, "escalatedto");
                item.EscalationTime = GetNullableDateTime(reader, "escalationtime");
                item.EscalationReason = GetStringOrDefault(reader, "escalationreason");
                item.IncludeInHandover = GetBoolFromInt(reader, "includeinhandover");
                item.CommunicatedToPatient = GetBoolFromInt(reader, "communicatedtopatient");
                item.CommunicatedToCompanion = GetBoolFromInt(reader, "communicatedtocompanion");
                item.AttachmentPath = GetStringOrDefault(reader, "attachmentpath");
                item.ReferenceDocument = GetStringOrDefault(reader, "referencedocument");
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

        private Guid? GetNullableGuid(SqliteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal)) return null;
                return Guid.Parse(reader.GetString(ordinal));
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
        INSERT INTO Tbl_MedicalNote (ID, partographID, time, handler, notes, notetype, content, createdby, isimportant, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash,
            role, urgencylevel, clinicalcategory, whosection, linkedmeasurabletype, linkedmeasurableid, linkedmeasurabletime, requiresreview, requiresfollowup, reviewedtime, reviewedby, reviewoutcome, escalated, escalatedto, escalationtime, escalationreason, includeinhandover, communicatedtopatient, communicatedtocompanion, attachmentpath, referencedocument, clinicalalert)
        VALUES (@id, @partographId, @time, @handler, @notes, @notetype, @content, @createdby, @isimportant, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash,
            @role, @urgencylevel, @clinicalcategory, @whosection, @linkedmeasurabletype, @linkedmeasurableid, @linkedmeasurabletime, @requiresreview, @requiresfollowup, @reviewedtime, @reviewedby, @reviewoutcome, @escalated, @escalatedto, @escalationtime, @escalationreason, @includeinhandover, @communicatedtopatient, @communicatedtocompanion, @attachmentpath, @referencedocument, @clinicalalert);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_MedicalNote
        SET partographID = @partographId,
            time = @time,
            handler = @handler,
            notes = @notes,
            notetype = @notetype,
            content = @content,
            createdby = @createdby,
            isimportant = @isimportant,
            role = @role,
            urgencylevel = @urgencylevel,
            clinicalcategory = @clinicalcategory,
            whosection = @whosection,
            linkedmeasurabletype = @linkedmeasurabletype,
            linkedmeasurableid = @linkedmeasurableid,
            linkedmeasurabletime = @linkedmeasurabletime,
            requiresreview = @requiresreview,
            requiresfollowup = @requiresfollowup,
            reviewedtime = @reviewedtime,
            reviewedby = @reviewedby,
            reviewoutcome = @reviewoutcome,
            escalated = @escalated,
            escalatedto = @escalatedto,
            escalationtime = @escalationtime,
            escalationreason = @escalationreason,
            includeinhandover = @includeinhandover,
            communicatedtopatient = @communicatedtopatient,
            communicatedtocompanion = @communicatedtocompanion,
            attachmentpath = @attachmentpath,
            referencedocument = @referencedocument,
            clinicalalert = @clinicalalert,
            updatedtime = @updatedtime,
            deletedtime = @deletedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version,
            datahash = @datahash
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, MedicalNote item)
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
            cmd.Parameters.AddWithValue("@notetype", item.NoteType ?? "General");
            cmd.Parameters.AddWithValue("@content", item.Content ?? "");
            cmd.Parameters.AddWithValue("@createdby", item.CreatedBy ?? "");
            cmd.Parameters.AddWithValue("@isimportant", item.IsImportant ? 1 : 0);
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
            cmd.Parameters.AddWithValue("@role", item.Role ?? "");
            cmd.Parameters.AddWithValue("@urgencylevel", item.UrgencyLevel ?? "");
            cmd.Parameters.AddWithValue("@clinicalcategory", item.ClinicalCategory ?? "");
            cmd.Parameters.AddWithValue("@whosection", item.WHOSection ?? "");
            cmd.Parameters.AddWithValue("@linkedmeasurabletype", item.LinkedMeasurableType ?? "");
            cmd.Parameters.AddWithValue("@linkedmeasurableid", item.LinkedMeasurableID?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@linkedmeasurabletime", item.LinkedMeasurableTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@requiresreview", item.RequiresReview ? 1 : 0);
            cmd.Parameters.AddWithValue("@requiresfollowup", item.RequiresFollowUp ? 1 : 0);
            cmd.Parameters.AddWithValue("@reviewedtime", item.ReviewedTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@reviewedby", item.ReviewedBy ?? "");
            cmd.Parameters.AddWithValue("@reviewoutcome", item.ReviewOutcome ?? "");
            cmd.Parameters.AddWithValue("@escalated", item.Escalated ? 1 : 0);
            cmd.Parameters.AddWithValue("@escalatedto", item.EscalatedTo ?? "");
            cmd.Parameters.AddWithValue("@escalationtime", item.EscalationTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@escalationreason", item.EscalationReason ?? "");
            cmd.Parameters.AddWithValue("@includeinhandover", item.IncludeInHandover ? 1 : 0);
            cmd.Parameters.AddWithValue("@communicatedtopatient", item.CommunicatedToPatient ? 1 : 0);
            cmd.Parameters.AddWithValue("@communicatedtocompanion", item.CommunicatedToCompanion ? 1 : 0);
            cmd.Parameters.AddWithValue("@attachmentpath", item.AttachmentPath ?? "");
            cmd.Parameters.AddWithValue("@referencedocument", item.ReferenceDocument ?? "");
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, MedicalNote item)
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
            cmd.Parameters.AddWithValue("@notetype", item.NoteType ?? "General");
            cmd.Parameters.AddWithValue("@content", item.Content ?? "");
            cmd.Parameters.AddWithValue("@createdby", item.CreatedBy ?? "");
            cmd.Parameters.AddWithValue("@isimportant", item.IsImportant ? 1 : 0);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@role", item.Role ?? "");
            cmd.Parameters.AddWithValue("@urgencylevel", item.UrgencyLevel ?? "");
            cmd.Parameters.AddWithValue("@clinicalcategory", item.ClinicalCategory ?? "");
            cmd.Parameters.AddWithValue("@whosection", item.WHOSection ?? "");
            cmd.Parameters.AddWithValue("@linkedmeasurabletype", item.LinkedMeasurableType ?? "");
            cmd.Parameters.AddWithValue("@linkedmeasurableid", item.LinkedMeasurableID?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@linkedmeasurabletime", item.LinkedMeasurableTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@requiresreview", item.RequiresReview ? 1 : 0);
            cmd.Parameters.AddWithValue("@requiresfollowup", item.RequiresFollowUp ? 1 : 0);
            cmd.Parameters.AddWithValue("@reviewedtime", item.ReviewedTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@reviewedby", item.ReviewedBy ?? "");
            cmd.Parameters.AddWithValue("@reviewoutcome", item.ReviewOutcome ?? "");
            cmd.Parameters.AddWithValue("@escalated", item.Escalated ? 1 : 0);
            cmd.Parameters.AddWithValue("@escalatedto", item.EscalatedTo ?? "");
            cmd.Parameters.AddWithValue("@escalationtime", item.EscalationTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@escalationreason", item.EscalationReason ?? "");
            cmd.Parameters.AddWithValue("@includeinhandover", item.IncludeInHandover ? 1 : 0);
            cmd.Parameters.AddWithValue("@communicatedtopatient", item.CommunicatedToPatient ? 1 : 0);
            cmd.Parameters.AddWithValue("@communicatedtocompanion", item.CommunicatedToCompanion ? 1 : 0);
            cmd.Parameters.AddWithValue("@attachmentpath", item.AttachmentPath ?? "");
            cmd.Parameters.AddWithValue("@referencedocument", item.ReferenceDocument ?? "");
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }
    }
}
