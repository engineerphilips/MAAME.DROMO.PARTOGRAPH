using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using System;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Pain Relief Entry Repository
    public class PainReliefEntryRepository : BasePartographRepository<PainReliefEntry>
    {
        protected override string TableName => "Tbl_PainReliefEntry";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_PainReliefEntry (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                painrelief TEXT,
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
                painscorebefore INTEGER,
                painscoreafter INTEGER,
                painassessmenttool TEXT DEFAULT '',
                painreliefmethod TEXT DEFAULT '',
                nonpharmacologicalmethods TEXT DEFAULT '',
                administeredtime TEXT,
                administeredby TEXT DEFAULT '',
                dose TEXT DEFAULT '',
                route TEXT DEFAULT '',
                effectiveness TEXT DEFAULT '',
                timetoeffectminutes INTEGER,
                durationofeffecthours INTEGER,
                sideeffects INTEGER DEFAULT 0,
                sideeffectsdescription TEXT DEFAULT '',
                continuousmonitoringrequired INTEGER DEFAULT 0,
                bladdercarerequired INTEGER DEFAULT 0,
                lasttopuptime TEXT,
                topupcount INTEGER DEFAULT 0,
                contraindicationschecked INTEGER DEFAULT 0,
                contraindicationspresent INTEGER DEFAULT 0,
                contraindicationdetails TEXT DEFAULT '',
                informedconsentobtained INTEGER DEFAULT 0,
                patientpreference TEXT DEFAULT '',
                clinicalalert TEXT DEFAULT ''
            );

            -- Add new columns to existing tables (WHO 2020 enhancements)
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN painscorebefore INTEGER;
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN painscoreafter INTEGER;
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN painassessmenttool TEXT DEFAULT '';
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN painreliefmethod TEXT DEFAULT '';
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN nonpharmacologicalmethods TEXT DEFAULT '';
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN administeredtime TEXT;
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN administeredby TEXT DEFAULT '';
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN dose TEXT DEFAULT '';
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN route TEXT DEFAULT '';
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN effectiveness TEXT DEFAULT '';
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN timetoeffectminutes INTEGER;
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN durationofeffecthours INTEGER;
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN sideeffects INTEGER DEFAULT 0;
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN sideeffectsdescription TEXT DEFAULT '';
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN continuousmonitoringrequired INTEGER DEFAULT 0;
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN bladdercarerequired INTEGER DEFAULT 0;
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN lasttopuptime TEXT;
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN topupcount INTEGER DEFAULT 0;
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN contraindicationschecked INTEGER DEFAULT 0;
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN contraindicationspresent INTEGER DEFAULT 0;
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN contraindicationdetails TEXT DEFAULT '';
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN informedconsentobtained INTEGER DEFAULT 0;
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN patientpreference TEXT DEFAULT '';
            ALTER TABLE Tbl_PainReliefEntry ADD COLUMN clinicalalert TEXT DEFAULT '';

            CREATE INDEX IF NOT EXISTS idx_painrelief_sync ON Tbl_PainReliefEntry(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_painrelief_server_version ON Tbl_PainReliefEntry(serverversion);

            DROP TRIGGER IF EXISTS trg_painrelief_insert;
            CREATE TRIGGER trg_painrelief_insert
            AFTER INSERT ON Tbl_PainReliefEntry
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_PainReliefEntry
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_painrelief_update;
            CREATE TRIGGER trg_painrelief_update
            AFTER UPDATE ON Tbl_PainReliefEntry
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_PainReliefEntry
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";

        public PainReliefEntryRepository(ILogger<PainReliefEntryRepository> logger) : base(logger) { }

        protected override PainReliefEntry MapFromReader(SqliteDataReader reader)
        {
            var item = new PainReliefEntry
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = reader.IsDBNull(reader.GetOrdinal("staffname")) ? string.Empty : reader.GetString(reader.GetOrdinal("staffname")),
                Notes = reader.GetString(reader.GetOrdinal("notes")),
                PainRelief = reader.IsDBNull(reader.GetOrdinal("painrelief")) ? null : reader.GetString(reader.GetOrdinal("painrelief")),
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
                item.PainScoreBefore = GetNullableInt(reader, "painscorebefore");
                item.PainScoreAfter = GetNullableInt(reader, "painscoreafter");
                item.PainAssessmentTool = GetStringOrDefault(reader, "painassessmenttool");
                item.PainReliefMethod = GetStringOrDefault(reader, "painreliefmethod");
                item.NonPharmacologicalMethods = GetStringOrDefault(reader, "nonpharmacologicalmethods");
                item.AdministeredTime = GetNullableDateTime(reader, "administeredtime");
                item.AdministeredBy = GetStringOrDefault(reader, "administeredby");
                item.Dose = GetStringOrDefault(reader, "dose");
                item.Route = GetStringOrDefault(reader, "route");
                item.Effectiveness = GetStringOrDefault(reader, "effectiveness");
                item.TimeToEffectMinutes = GetNullableInt(reader, "timetoeffectminutes");
                item.DurationOfEffectHours = GetNullableInt(reader, "durationofeffecthours");
                item.SideEffects = GetBoolFromInt(reader, "sideeffects");
                item.SideEffectsDescription = GetStringOrDefault(reader, "sideeffectsdescription");
                item.ContinuousMonitoringRequired = GetBoolFromInt(reader, "continuousmonitoringrequired");
                item.BladderCareRequired = GetBoolFromInt(reader, "bladdercarerequired");
                item.LastTopUpTime = GetNullableDateTime(reader, "lasttopuptime");
                item.TopUpCount = GetIntOrDefault(reader, "topupcount", 0);
                item.ContraindicationsChecked = GetBoolFromInt(reader, "contraindicationschecked");
                item.ContraindicationsPresent = GetBoolFromInt(reader, "contraindicationspresent");
                item.ContraindicationDetails = GetStringOrDefault(reader, "contraindicationdetails");
                item.InformedConsentObtained = GetBoolFromInt(reader, "informedconsentobtained");
                item.PatientPreference = GetStringOrDefault(reader, "patientpreference");
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

        private int GetIntOrDefault(SqliteDataReader reader, string columnName, int defaultValue = 0)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : reader.GetInt32(ordinal);
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
        INSERT INTO Tbl_PainReliefEntry (ID, partographID, time, handler, notes, painrelief, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash,
            painscorebefore, painscoreafter, painassessmenttool, painreliefmethod, nonpharmacologicalmethods, administeredtime, administeredby, dose, route, effectiveness, timetoeffectminutes, durationofeffecthours, sideeffects, sideeffectsdescription, continuousmonitoringrequired, bladdercarerequired, lasttopuptime, topupcount, contraindicationschecked, contraindicationspresent, contraindicationdetails, informedconsentobtained, patientpreference, clinicalalert)
        VALUES (@id, @partographId, @time, @handler, @notes, @painrelief, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash,
            @painscorebefore, @painscoreafter, @painassessmenttool, @painreliefmethod, @nonpharmacologicalmethods, @administeredtime, @administeredby, @dose, @route, @effectiveness, @timetoeffectminutes, @durationofeffecthours, @sideeffects, @sideeffectsdescription, @continuousmonitoringrequired, @bladdercarerequired, @lasttopuptime, @topupcount, @contraindicationschecked, @contraindicationspresent, @contraindicationdetails, @informedconsentobtained, @patientpreference, @clinicalalert);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_PainReliefEntry
        SET partographID = @partographId,
            time = @time,
            handler = @handler,
            notes = @notes,
            painrelief = @painrelief,
            painscorebefore = @painscorebefore,
            painscoreafter = @painscoreafter,
            painassessmenttool = @painassessmenttool,
            painreliefmethod = @painreliefmethod,
            nonpharmacologicalmethods = @nonpharmacologicalmethods,
            administeredtime = @administeredtime,
            administeredby = @administeredby,
            dose = @dose,
            route = @route,
            effectiveness = @effectiveness,
            timetoeffectminutes = @timetoeffectminutes,
            durationofeffecthours = @durationofeffecthours,
            sideeffects = @sideeffects,
            sideeffectsdescription = @sideeffectsdescription,
            continuousmonitoringrequired = @continuousmonitoringrequired,
            bladdercarerequired = @bladdercarerequired,
            lasttopuptime = @lasttopuptime,
            topupcount = @topupcount,
            contraindicationschecked = @contraindicationschecked,
            contraindicationspresent = @contraindicationspresent,
            contraindicationdetails = @contraindicationdetails,
            informedconsentobtained = @informedconsentobtained,
            patientpreference = @patientpreference,
            clinicalalert = @clinicalalert,
            updatedtime = @updatedtime,
            deletedtime = @deletedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version,
            datahash = @datahash
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, PainReliefEntry item)
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
            cmd.Parameters.AddWithValue("@painrelief", item.PainRelief ?? (object)DBNull.Value);
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
            cmd.Parameters.AddWithValue("@painscorebefore", item.PainScoreBefore ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@painscoreafter", item.PainScoreAfter ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@painassessmenttool", item.PainAssessmentTool ?? "");
            cmd.Parameters.AddWithValue("@painreliefmethod", item.PainReliefMethod ?? "");
            cmd.Parameters.AddWithValue("@nonpharmacologicalmethods", item.NonPharmacologicalMethods ?? "");
            cmd.Parameters.AddWithValue("@administeredtime", item.AdministeredTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@administeredby", item.AdministeredBy ?? "");
            cmd.Parameters.AddWithValue("@dose", item.Dose ?? "");
            cmd.Parameters.AddWithValue("@route", item.Route ?? "");
            cmd.Parameters.AddWithValue("@effectiveness", item.Effectiveness ?? "");
            cmd.Parameters.AddWithValue("@timetoeffectminutes", item.TimeToEffectMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@durationofeffecthours", item.DurationOfEffectHours ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@sideeffects", item.SideEffects ? 1 : 0);
            cmd.Parameters.AddWithValue("@sideeffectsdescription", item.SideEffectsDescription ?? "");
            cmd.Parameters.AddWithValue("@continuousmonitoringrequired", item.ContinuousMonitoringRequired ? 1 : 0);
            cmd.Parameters.AddWithValue("@bladdercarerequired", item.BladderCareRequired ? 1 : 0);
            cmd.Parameters.AddWithValue("@lasttopuptime", item.LastTopUpTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@topupcount", item.TopUpCount);
            cmd.Parameters.AddWithValue("@contraindicationschecked", item.ContraindicationsChecked ? 1 : 0);
            cmd.Parameters.AddWithValue("@contraindicationspresent", item.ContraindicationsPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@contraindicationdetails", item.ContraindicationDetails ?? "");
            cmd.Parameters.AddWithValue("@informedconsentobtained", item.InformedConsentObtained ? 1 : 0);
            cmd.Parameters.AddWithValue("@patientpreference", item.PatientPreference ?? "");
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, PainReliefEntry item)
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
            cmd.Parameters.AddWithValue("@painrelief", item.PainRelief ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@painscorebefore", item.PainScoreBefore ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@painscoreafter", item.PainScoreAfter ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@painassessmenttool", item.PainAssessmentTool ?? "");
            cmd.Parameters.AddWithValue("@painreliefmethod", item.PainReliefMethod ?? "");
            cmd.Parameters.AddWithValue("@nonpharmacologicalmethods", item.NonPharmacologicalMethods ?? "");
            cmd.Parameters.AddWithValue("@administeredtime", item.AdministeredTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@administeredby", item.AdministeredBy ?? "");
            cmd.Parameters.AddWithValue("@dose", item.Dose ?? "");
            cmd.Parameters.AddWithValue("@route", item.Route ?? "");
            cmd.Parameters.AddWithValue("@effectiveness", item.Effectiveness ?? "");
            cmd.Parameters.AddWithValue("@timetoeffectminutes", item.TimeToEffectMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@durationofeffecthours", item.DurationOfEffectHours ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@sideeffects", item.SideEffects ? 1 : 0);
            cmd.Parameters.AddWithValue("@sideeffectsdescription", item.SideEffectsDescription ?? "");
            cmd.Parameters.AddWithValue("@continuousmonitoringrequired", item.ContinuousMonitoringRequired ? 1 : 0);
            cmd.Parameters.AddWithValue("@bladdercarerequired", item.BladderCareRequired ? 1 : 0);
            cmd.Parameters.AddWithValue("@lasttopuptime", item.LastTopUpTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@topupcount", item.TopUpCount);
            cmd.Parameters.AddWithValue("@contraindicationschecked", item.ContraindicationsChecked ? 1 : 0);
            cmd.Parameters.AddWithValue("@contraindicationspresent", item.ContraindicationsPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@contraindicationdetails", item.ContraindicationDetails ?? "");
            cmd.Parameters.AddWithValue("@informedconsentobtained", item.InformedConsentObtained ? 1 : 0);
            cmd.Parameters.AddWithValue("@patientpreference", item.PatientPreference ?? "");
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }
    }
}
