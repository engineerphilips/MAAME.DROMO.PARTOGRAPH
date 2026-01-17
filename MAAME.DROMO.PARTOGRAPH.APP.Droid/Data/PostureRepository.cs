using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using System;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Posture Repository
    public class PostureRepository : BasePartographRepository<PostureEntry>
    {
        protected override string TableName => "Tbl_Posture";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_Posture (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                posture TEXT,
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
                posturecategory TEXT DEFAULT '',
                starttime TEXT,
                endtime TEXT,
                durationminutes INTEGER,
                reason TEXT DEFAULT '',
                effectonlabor TEXT DEFAULT '',
                effectonpain TEXT DEFAULT '',
                effectoncontractions TEXT DEFAULT '',
                patientchoice INTEGER DEFAULT 1,
                medicallyindicated INTEGER DEFAULT 0,
                mobileandactive INTEGER DEFAULT 0,
                restrictedmobility INTEGER DEFAULT 0,
                mobilityrestriction TEXT DEFAULT '',
                supportequipment TEXT DEFAULT '',
                clinicalalert TEXT DEFAULT ''
            );

            CREATE INDEX IF NOT EXISTS idx_posture_partographid ON Tbl_Posture(partographid);
            CREATE INDEX IF NOT EXISTS idx_posture_sync ON Tbl_Posture(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_posture_server_version ON Tbl_Posture(serverversion);

            DROP TRIGGER IF EXISTS trg_posture_insert;
            CREATE TRIGGER trg_posture_insert
            AFTER INSERT ON Tbl_Posture
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_Posture
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_posture_update;
            CREATE TRIGGER trg_posture_update
            AFTER UPDATE ON Tbl_Posture
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_Posture
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";
                
        //-- Add new columns to existing tables(WHO 2020 enhancements)
        //ALTER TABLE Tbl_Posture ADD COLUMN posturecategory TEXT DEFAULT '';
        //ALTER TABLE Tbl_Posture ADD COLUMN starttime TEXT;
        //    ALTER TABLE Tbl_Posture ADD COLUMN endtime TEXT;
        //    ALTER TABLE Tbl_Posture ADD COLUMN durationminutes INTEGER;
        //    ALTER TABLE Tbl_Posture ADD COLUMN reason TEXT DEFAULT '';
        //ALTER TABLE Tbl_Posture ADD COLUMN effectonlabor TEXT DEFAULT '';
        //ALTER TABLE Tbl_Posture ADD COLUMN effectonpain TEXT DEFAULT '';
        //ALTER TABLE Tbl_Posture ADD COLUMN effectoncontractions TEXT DEFAULT '';
        //ALTER TABLE Tbl_Posture ADD COLUMN patientchoice INTEGER DEFAULT 1;
        //ALTER TABLE Tbl_Posture ADD COLUMN medicallyindicated INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Posture ADD COLUMN mobileandactive INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Posture ADD COLUMN restrictedmobility INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Posture ADD COLUMN mobilityrestriction TEXT DEFAULT '';
        //ALTER TABLE Tbl_Posture ADD COLUMN supportequipment TEXT DEFAULT '';
        //ALTER TABLE Tbl_Posture ADD COLUMN clinicalalert TEXT DEFAULT '';

        public PostureRepository(ILogger<PostureRepository> logger) : base(logger) { }

        protected override PostureEntry MapFromReader(SqliteDataReader reader)
        {
            var item = new PostureEntry
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = reader.IsDBNull(reader.GetOrdinal("staffname")) ? string.Empty : reader.GetString(reader.GetOrdinal("staffname")),
                Notes = reader.GetString(reader.GetOrdinal("notes")),
                Posture = reader.IsDBNull(reader.GetOrdinal("posture")) ? string.Empty : reader.GetString(reader.GetOrdinal("posture")),
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
                item.PostureCategory = GetStringOrDefault(reader, "posturecategory");
                item.StartTime = GetNullableDateTime(reader, "starttime");
                item.EndTime = GetNullableDateTime(reader, "endtime");
                item.DurationMinutes = GetNullableInt(reader, "durationminutes");
                item.Reason = GetStringOrDefault(reader, "reason");
                item.EffectOnLabor = GetStringOrDefault(reader, "effectonlabor");
                item.EffectOnPain = GetStringOrDefault(reader, "effectonpain");
                item.EffectOnContractions = GetStringOrDefault(reader, "effectoncontractions");
                item.PatientChoice = GetBoolFromInt(reader, "patientchoice");
                item.MedicallyIndicated = GetBoolFromInt(reader, "medicallyindicated");
                item.MobileAndActive = GetBoolFromInt(reader, "mobileandactive");
                item.RestrictedMobility = GetBoolFromInt(reader, "restrictedmobility");
                item.MobilityRestriction = GetStringOrDefault(reader, "mobilityrestriction");
                item.SupportEquipment = GetStringOrDefault(reader, "supportequipment");
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
        INSERT INTO Tbl_Posture (ID, partographID, time, handler, notes, posture, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash,
            posturecategory, starttime, endtime, durationminutes, reason, effectonlabor, effectonpain, effectoncontractions, patientchoice, medicallyindicated, mobileandactive, restrictedmobility, mobilityrestriction, supportequipment, clinicalalert)
        VALUES (@id, @partographId, @time, @handler, @notes, @posture, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash,
            @posturecategory, @starttime, @endtime, @durationminutes, @reason, @effectonlabor, @effectonpain, @effectoncontractions, @patientchoice, @medicallyindicated, @mobileandactive, @restrictedmobility, @mobilityrestriction, @supportequipment, @clinicalalert);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_Posture
        SET partographID = @partographId,
            time = @time,
            handler = @handler,
            notes = @notes,
            posture = @posture,
            posturecategory = @posturecategory,
            starttime = @starttime,
            endtime = @endtime,
            durationminutes = @durationminutes,
            reason = @reason,
            effectonlabor = @effectonlabor,
            effectonpain = @effectonpain,
            effectoncontractions = @effectoncontractions,
            patientchoice = @patientchoice,
            medicallyindicated = @medicallyindicated,
            mobileandactive = @mobileandactive,
            restrictedmobility = @restrictedmobility,
            mobilityrestriction = @mobilityrestriction,
            supportequipment = @supportequipment,
            clinicalalert = @clinicalalert,
            updatedtime = @updatedtime,
            deletedtime = @deletedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version,
            datahash = @datahash
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, PostureEntry item)
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
            cmd.Parameters.AddWithValue("@posture", item.Posture ?? "");
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
            cmd.Parameters.AddWithValue("@posturecategory", item.PostureCategory ?? "");
            cmd.Parameters.AddWithValue("@starttime", item.StartTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@endtime", item.EndTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@durationminutes", item.DurationMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@reason", item.Reason ?? "");
            cmd.Parameters.AddWithValue("@effectonlabor", item.EffectOnLabor ?? "");
            cmd.Parameters.AddWithValue("@effectonpain", item.EffectOnPain ?? "");
            cmd.Parameters.AddWithValue("@effectoncontractions", item.EffectOnContractions ?? "");
            cmd.Parameters.AddWithValue("@patientchoice", item.PatientChoice ? 1 : 0);
            cmd.Parameters.AddWithValue("@medicallyindicated", item.MedicallyIndicated ? 1 : 0);
            cmd.Parameters.AddWithValue("@mobileandactive", item.MobileAndActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@restrictedmobility", item.RestrictedMobility ? 1 : 0);
            cmd.Parameters.AddWithValue("@mobilityrestriction", item.MobilityRestriction ?? "");
            cmd.Parameters.AddWithValue("@supportequipment", item.SupportEquipment ?? "");
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, PostureEntry item)
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
            cmd.Parameters.AddWithValue("@posture", item.Posture ?? "");
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@posturecategory", item.PostureCategory ?? "");
            cmd.Parameters.AddWithValue("@starttime", item.StartTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@endtime", item.EndTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@durationminutes", item.DurationMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@reason", item.Reason ?? "");
            cmd.Parameters.AddWithValue("@effectonlabor", item.EffectOnLabor ?? "");
            cmd.Parameters.AddWithValue("@effectonpain", item.EffectOnPain ?? "");
            cmd.Parameters.AddWithValue("@effectoncontractions", item.EffectOnContractions ?? "");
            cmd.Parameters.AddWithValue("@patientchoice", item.PatientChoice ? 1 : 0);
            cmd.Parameters.AddWithValue("@medicallyindicated", item.MedicallyIndicated ? 1 : 0);
            cmd.Parameters.AddWithValue("@mobileandactive", item.MobileAndActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@restrictedmobility", item.RestrictedMobility ? 1 : 0);
            cmd.Parameters.AddWithValue("@mobilityrestriction", item.MobilityRestriction ?? "");
            cmd.Parameters.AddWithValue("@supportequipment", item.SupportEquipment ?? "");
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }
    }
}
