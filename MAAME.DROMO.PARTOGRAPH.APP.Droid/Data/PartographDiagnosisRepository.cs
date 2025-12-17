using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Partograph Diagnosis Repository
    public class PartographDiagnosisRepository : BasePartographRepository<PartographDiagnosis>
    {
        protected override string TableName => "Tbl_PartographDiagnosis";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_PartographDiagnosis (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                name TEXT NOT NULL,
                notes TEXT NOT NULL,
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
                category TEXT DEFAULT '',
                diagnosistype TEXT DEFAULT '',
                icdcode TEXT DEFAULT '',
                icddescription TEXT DEFAULT '',
                severity TEXT DEFAULT '',
                onsettime TEXT,
                durationhours INTEGER,
                onsettype TEXT DEFAULT '',
                clinicalevidence TEXT DEFAULT '',
                supportingfindings TEXT DEFAULT '',
                linkedmeasurableids TEXT DEFAULT '',
                linkedmeasurabletypes TEXT DEFAULT '',
                status TEXT DEFAULT '',
                diagnosedby TEXT DEFAULT '',
                diagnosedbyrole TEXT DEFAULT '',
                confidencelevel TEXT DEFAULT '',
                managementplan TEXT DEFAULT '',
                managementaction TEXT DEFAULT '',
                requiresescalation INTEGER DEFAULT 0,
                escalatedto TEXT DEFAULT '',
                escalationtime TEXT,
                requiresreview INTEGER DEFAULT 0,
                reviewtime TEXT,
                reviewoutcome TEXT DEFAULT '',
                resolvedtime TEXT,
                discussedwithpatient INTEGER DEFAULT 0,
                discussedwithcompanion INTEGER DEFAULT 0,
                patientunderstanding TEXT DEFAULT '',
                clinicalalert TEXT DEFAULT ''
            );

            -- Add new columns to existing tables (WHO 2020 enhancements)
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN notes TEXT NOT NULL DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN category TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN diagnosistype TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN icdcode TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN icddescription TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN severity TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN onsettime TEXT;
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN durationhours INTEGER;
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN onsettype TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN clinicalevidence TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN supportingfindings TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN linkedmeasurableids TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN linkedmeasurabletypes TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN status TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN diagnosedby TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN diagnosedbyrole TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN confidencelevel TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN managementplan TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN managementaction TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN requiresescalation INTEGER DEFAULT 0;
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN escalatedto TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN escalationtime TEXT;
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN requiresreview INTEGER DEFAULT 0;
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN reviewtime TEXT;
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN reviewoutcome TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN resolvedtime TEXT;
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN discussedwithpatient INTEGER DEFAULT 0;
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN discussedwithcompanion INTEGER DEFAULT 0;
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN patientunderstanding TEXT DEFAULT '';
            ALTER TABLE Tbl_PartographDiagnosis ADD COLUMN clinicalalert TEXT DEFAULT '';

            CREATE INDEX IF NOT EXISTS idx_partographdiagnosis_sync ON Tbl_PartographDiagnosis(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_partographdiagnosis_server_version ON Tbl_PartographDiagnosis(serverversion);

            DROP TRIGGER IF EXISTS trg_partographdiagnosis_insert;
            CREATE TRIGGER trg_partographdiagnosis_insert
            AFTER INSERT ON Tbl_PartographDiagnosis
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_PartographDiagnosis
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_partographdiagnosis_update;
            CREATE TRIGGER trg_partographdiagnosis_update
            AFTER UPDATE ON Tbl_PartographDiagnosis
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_PartographDiagnosis
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";

        public PartographDiagnosisRepository(ILogger<PartographDiagnosisRepository> logger) : base(logger) { }

        protected override PartographDiagnosis MapFromReader(SqliteDataReader reader)
        {
            var item = new PartographDiagnosis
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = reader.IsDBNull(reader.GetOrdinal("staffname")) ? string.Empty : reader.GetString(reader.GetOrdinal("staffname")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Notes = GetStringOrDefault(reader, "notes"),
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
                item.Category = GetStringOrDefault(reader, "category");
                item.DiagnosisType = GetStringOrDefault(reader, "diagnosistype");
                item.ICDCode = GetStringOrDefault(reader, "icdcode");
                item.ICDDescription = GetStringOrDefault(reader, "icddescription");
                item.Severity = GetStringOrDefault(reader, "severity");
                item.OnsetTime = GetNullableDateTime(reader, "onsettime");
                item.DurationHours = GetNullableInt(reader, "durationhours");
                item.OnsetType = GetStringOrDefault(reader, "onsettype");
                item.ClinicalEvidence = GetStringOrDefault(reader, "clinicalevidence");
                item.SupportingFindings = GetStringOrDefault(reader, "supportingfindings");
                item.LinkedMeasurableIDs = DeserializeGuidList(GetStringOrDefault(reader, "linkedmeasurableids"));
                item.LinkedMeasurableTypes = GetStringOrDefault(reader, "linkedmeasurabletypes");
                item.Status = GetStringOrDefault(reader, "status");
                item.DiagnosedBy = GetStringOrDefault(reader, "diagnosedby");
                item.DiagnosedByRole = GetStringOrDefault(reader, "diagnosedbyrole");
                item.ConfidenceLevel = GetStringOrDefault(reader, "confidencelevel");
                item.ManagementPlan = GetStringOrDefault(reader, "managementplan");
                item.ManagementAction = GetStringOrDefault(reader, "managementaction");
                item.RequiresEscalation = GetBoolFromInt(reader, "requiresescalation");
                item.EscalatedTo = GetStringOrDefault(reader, "escalatedto");
                item.EscalationTime = GetNullableDateTime(reader, "escalationtime");
                item.RequiresReview = GetBoolFromInt(reader, "requiresreview");
                item.ReviewTime = GetNullableDateTime(reader, "reviewtime");
                item.ReviewOutcome = GetStringOrDefault(reader, "reviewoutcome");
                item.ResolvedTime = GetNullableDateTime(reader, "resolvedtime");
                item.DiscussedWithPatient = GetBoolFromInt(reader, "discussedwithpatient");
                item.DiscussedWithCompanion = GetBoolFromInt(reader, "discussedwithcompanion");
                item.PatientUnderstanding = GetStringOrDefault(reader, "patientunderstanding");
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

        // Serialize List<Guid> as comma-separated string
        private string SerializeGuidList(List<Guid> guids)
        {
            if (guids == null || guids.Count == 0) return string.Empty;
            return string.Join(",", guids.Select(g => g.ToString()));
        }

        // Deserialize comma-separated string to List<Guid>
        private List<Guid> DeserializeGuidList(string guidsString)
        {
            if (string.IsNullOrWhiteSpace(guidsString)) return new List<Guid>();
            try
            {
                return guidsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => Guid.Parse(s.Trim()))
                    .ToList();
            }
            catch { return new List<Guid>(); }
        }

        protected override string GetInsertSql() => @"
        INSERT INTO Tbl_PartographDiagnosis (ID, partographID, time, handler, name, notes, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash,
            category, diagnosistype, icdcode, icddescription, severity, onsettime, durationhours, onsettype, clinicalevidence, supportingfindings, linkedmeasurableids, linkedmeasurabletypes, status, diagnosedby, diagnosedbyrole, confidencelevel, managementplan, managementaction, requiresescalation, escalatedto, escalationtime, requiresreview, reviewtime, reviewoutcome, resolvedtime, discussedwithpatient, discussedwithcompanion, patientunderstanding, clinicalalert)
        VALUES (@id, @partographId, @time, @handler, @name, @notes, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash,
            @category, @diagnosistype, @icdcode, @icddescription, @severity, @onsettime, @durationhours, @onsettype, @clinicalevidence, @supportingfindings, @linkedmeasurableids, @linkedmeasurabletypes, @status, @diagnosedby, @diagnosedbyrole, @confidencelevel, @managementplan, @managementaction, @requiresescalation, @escalatedto, @escalationtime, @requiresreview, @reviewtime, @reviewoutcome, @resolvedtime, @discussedwithpatient, @discussedwithcompanion, @patientunderstanding, @clinicalalert);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_PartographDiagnosis
        SET partographID = @partographId,
            time = @time,
            handler = @handler,
            name = @name,
            notes = @notes,
            category = @category,
            diagnosistype = @diagnosistype,
            icdcode = @icdcode,
            icddescription = @icddescription,
            severity = @severity,
            onsettime = @onsettime,
            durationhours = @durationhours,
            onsettype = @onsettype,
            clinicalevidence = @clinicalevidence,
            supportingfindings = @supportingfindings,
            linkedmeasurableids = @linkedmeasurableids,
            linkedmeasurabletypes = @linkedmeasurabletypes,
            status = @status,
            diagnosedby = @diagnosedby,
            diagnosedbyrole = @diagnosedbyrole,
            confidencelevel = @confidencelevel,
            managementplan = @managementplan,
            managementaction = @managementaction,
            requiresescalation = @requiresescalation,
            escalatedto = @escalatedto,
            escalationtime = @escalationtime,
            requiresreview = @requiresreview,
            reviewtime = @reviewtime,
            reviewoutcome = @reviewoutcome,
            resolvedtime = @resolvedtime,
            discussedwithpatient = @discussedwithpatient,
            discussedwithcompanion = @discussedwithcompanion,
            patientunderstanding = @patientunderstanding,
            clinicalalert = @clinicalalert,
            updatedtime = @updatedtime,
            deletedtime = @deletedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version,
            datahash = @datahash
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, PartographDiagnosis item)
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
            cmd.Parameters.AddWithValue("@name", item.Name ?? "");
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? "");
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
            cmd.Parameters.AddWithValue("@category", item.Category ?? "");
            cmd.Parameters.AddWithValue("@diagnosistype", item.DiagnosisType ?? "");
            cmd.Parameters.AddWithValue("@icdcode", item.ICDCode ?? "");
            cmd.Parameters.AddWithValue("@icddescription", item.ICDDescription ?? "");
            cmd.Parameters.AddWithValue("@severity", item.Severity ?? "");
            cmd.Parameters.AddWithValue("@onsettime", item.OnsetTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@durationhours", item.DurationHours ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@onsettype", item.OnsetType ?? "");
            cmd.Parameters.AddWithValue("@clinicalevidence", item.ClinicalEvidence ?? "");
            cmd.Parameters.AddWithValue("@supportingfindings", item.SupportingFindings ?? "");
            cmd.Parameters.AddWithValue("@linkedmeasurableids", SerializeGuidList(item.LinkedMeasurableIDs));
            cmd.Parameters.AddWithValue("@linkedmeasurabletypes", item.LinkedMeasurableTypes ?? "");
            cmd.Parameters.AddWithValue("@status", item.Status ?? "");
            cmd.Parameters.AddWithValue("@diagnosedby", item.DiagnosedBy ?? "");
            cmd.Parameters.AddWithValue("@diagnosedbyrole", item.DiagnosedByRole ?? "");
            cmd.Parameters.AddWithValue("@confidencelevel", item.ConfidenceLevel ?? "");
            cmd.Parameters.AddWithValue("@managementplan", item.ManagementPlan ?? "");
            cmd.Parameters.AddWithValue("@managementaction", item.ManagementAction ?? "");
            cmd.Parameters.AddWithValue("@requiresescalation", item.RequiresEscalation ? 1 : 0);
            cmd.Parameters.AddWithValue("@escalatedto", item.EscalatedTo ?? "");
            cmd.Parameters.AddWithValue("@escalationtime", item.EscalationTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@requiresreview", item.RequiresReview ? 1 : 0);
            cmd.Parameters.AddWithValue("@reviewtime", item.ReviewTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@reviewoutcome", item.ReviewOutcome ?? "");
            cmd.Parameters.AddWithValue("@resolvedtime", item.ResolvedTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@discussedwithpatient", item.DiscussedWithPatient ? 1 : 0);
            cmd.Parameters.AddWithValue("@discussedwithcompanion", item.DiscussedWithCompanion ? 1 : 0);
            cmd.Parameters.AddWithValue("@patientunderstanding", item.PatientUnderstanding ?? "");
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, PartographDiagnosis item)
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
            cmd.Parameters.AddWithValue("@name", item.Name ?? "");
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@category", item.Category ?? "");
            cmd.Parameters.AddWithValue("@diagnosistype", item.DiagnosisType ?? "");
            cmd.Parameters.AddWithValue("@icdcode", item.ICDCode ?? "");
            cmd.Parameters.AddWithValue("@icddescription", item.ICDDescription ?? "");
            cmd.Parameters.AddWithValue("@severity", item.Severity ?? "");
            cmd.Parameters.AddWithValue("@onsettime", item.OnsetTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@durationhours", item.DurationHours ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@onsettype", item.OnsetType ?? "");
            cmd.Parameters.AddWithValue("@clinicalevidence", item.ClinicalEvidence ?? "");
            cmd.Parameters.AddWithValue("@supportingfindings", item.SupportingFindings ?? "");
            cmd.Parameters.AddWithValue("@linkedmeasurableids", SerializeGuidList(item.LinkedMeasurableIDs));
            cmd.Parameters.AddWithValue("@linkedmeasurabletypes", item.LinkedMeasurableTypes ?? "");
            cmd.Parameters.AddWithValue("@status", item.Status ?? "");
            cmd.Parameters.AddWithValue("@diagnosedby", item.DiagnosedBy ?? "");
            cmd.Parameters.AddWithValue("@diagnosedbyrole", item.DiagnosedByRole ?? "");
            cmd.Parameters.AddWithValue("@confidencelevel", item.ConfidenceLevel ?? "");
            cmd.Parameters.AddWithValue("@managementplan", item.ManagementPlan ?? "");
            cmd.Parameters.AddWithValue("@managementaction", item.ManagementAction ?? "");
            cmd.Parameters.AddWithValue("@requiresescalation", item.RequiresEscalation ? 1 : 0);
            cmd.Parameters.AddWithValue("@escalatedto", item.EscalatedTo ?? "");
            cmd.Parameters.AddWithValue("@escalationtime", item.EscalationTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@requiresreview", item.RequiresReview ? 1 : 0);
            cmd.Parameters.AddWithValue("@reviewtime", item.ReviewTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@reviewoutcome", item.ReviewOutcome ?? "");
            cmd.Parameters.AddWithValue("@resolvedtime", item.ResolvedTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@discussedwithpatient", item.DiscussedWithPatient ? 1 : 0);
            cmd.Parameters.AddWithValue("@discussedwithcompanion", item.DiscussedWithCompanion ? 1 : 0);
            cmd.Parameters.AddWithValue("@patientunderstanding", item.PatientUnderstanding ?? "");
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }
    }
}
