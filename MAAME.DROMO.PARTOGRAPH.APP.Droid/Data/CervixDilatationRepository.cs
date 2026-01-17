using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Cervix Dilatation Repository - WHO 2020 Enhanced
    public class CervixDilatationRepository : BasePartographRepository<CervixDilatation>
    {
        protected override string TableName => "Tbl_CervixDilatation";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_CervixDilatation (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                dilatation INTEGER DEFAULT 0,
                effacementPercent INTEGER DEFAULT 0,
                consistency TEXT DEFAULT 'Medium',
                position TEXT DEFAULT 'Mid',
                applicationToHead INTEGER DEFAULT 0,
                cervicalEdema TEXT DEFAULT 'None',
                membraneStatus TEXT DEFAULT '',
                cervicalLip INTEGER DEFAULT 0,
                dilatationRateCmPerHour REAL,
                progressionRate TEXT DEFAULT '',
                crossedActionLine INTEGER DEFAULT 0,
                crossedAlertLine INTEGER DEFAULT 0,
                actionLineCrossedTime TEXT,
                cervicalLengthCm REAL,
                examinerName TEXT DEFAULT '',
                examDurationMinutes INTEGER,
                difficultExam INTEGER DEFAULT 0,
                examDifficulty TEXT DEFAULT '',
                cervicalThickness TEXT DEFAULT '',
                anteriorCervicalLip INTEGER DEFAULT 0,
                posteriorCervicalLip INTEGER DEFAULT 0,
                cervicalDilatationPattern TEXT DEFAULT '',
                stationRelativeToPelvicSpines INTEGER,
                presentingPartPosition TEXT DEFAULT '',
                presentingPartWellApplied INTEGER DEFAULT 0,
                membranesBulging INTEGER DEFAULT 0,
                forewatersPresent INTEGER DEFAULT 0,
                hindwatersPresent INTEGER DEFAULT 0,
                clinicalAlert TEXT DEFAULT '',
                prolongedLatentPhase INTEGER DEFAULT 0,
                protractedActivePhase INTEGER DEFAULT 0,
                arrestedDilatation INTEGER DEFAULT 0,
                precipitousLabor INTEGER DEFAULT 0,
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

            CREATE INDEX IF NOT EXISTS idx_cervix_dilatation_partographid ON Tbl_CervixDilatation(partographid);
            CREATE INDEX IF NOT EXISTS idx_cervix_dilatation_sync ON Tbl_CervixDilatation(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_cervix_dilatation_server_version ON Tbl_CervixDilatation(serverversion);
           
            DROP TRIGGER IF EXISTS trg_cervix_dilatation_insert;
            CREATE TRIGGER trg_cervix_dilatation_insert
            AFTER INSERT ON Tbl_CervixDilatation
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_CervixDilatation
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_cervix_dilatation_update;
            CREATE TRIGGER trg_cervix_dilatation_update
            AFTER UPDATE ON Tbl_CervixDilatation
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_CervixDilatation
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";

        //-- Add WHO 2020 columns to existing tables
        //    ALTER TABLE Tbl_CervixDilatation ADD COLUMN effacementPercent INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN consistency TEXT DEFAULT 'Medium';
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN position TEXT DEFAULT 'Mid';
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN applicationToHead INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN cervicalEdema TEXT DEFAULT 'None';
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN membraneStatus TEXT DEFAULT '';
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN cervicalLip INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN dilatationRateCmPerHour REAL;
        //    ALTER TABLE Tbl_CervixDilatation ADD COLUMN progressionRate TEXT DEFAULT '';
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN crossedActionLine INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN crossedAlertLine INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN actionLineCrossedTime TEXT;
        //    ALTER TABLE Tbl_CervixDilatation ADD COLUMN cervicalLengthCm REAL;
        //    ALTER TABLE Tbl_CervixDilatation ADD COLUMN examinerName TEXT DEFAULT '';
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN examDurationMinutes INTEGER;
        //    ALTER TABLE Tbl_CervixDilatation ADD COLUMN difficultExam INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN examDifficulty TEXT DEFAULT '';
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN cervicalThickness TEXT DEFAULT '';
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN anteriorCervicalLip INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN posteriorCervicalLip INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN cervicalDilatationPattern TEXT DEFAULT '';
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN stationRelativeToPelvicSpines INTEGER;
        //    ALTER TABLE Tbl_CervixDilatation ADD COLUMN presentingPartPosition TEXT DEFAULT '';
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN presentingPartWellApplied INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN membranesBulging INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN forewatersPresent INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN hindwatersPresent INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN clinicalAlert TEXT DEFAULT '';
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN prolongedLatentPhase INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN protractedActivePhase INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN arrestedDilatation INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_CervixDilatation ADD COLUMN precipitousLabor INTEGER DEFAULT 0;
        public CervixDilatationRepository(ILogger<CervixDilatationRepository> logger) : base(logger) { }

        protected override CervixDilatation MapFromReader(SqliteDataReader reader)
        {
            var item = new CervixDilatation
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = GetStringOrDefault(reader, "staffname", ""),
                Notes = GetStringOrDefault(reader, "notes", ""),
                DilatationCm = GetIntOrDefault(reader, "dilatation", 0),
                CreatedTime = reader.GetInt64(reader.GetOrdinal("createdtime")),
                UpdatedTime = reader.GetInt64(reader.GetOrdinal("updatedtime")),
                DeletedTime = reader.IsDBNull(reader.GetOrdinal("deletedtime")) ? null : reader.GetInt64(reader.GetOrdinal("deletedtime")),
                DeviceId = GetStringOrDefault(reader, "deviceid", ""),
                OriginDeviceId = GetStringOrDefault(reader, "origindeviceid", ""),
                SyncStatus = GetIntOrDefault(reader, "syncstatus", 0),
                Version = GetIntOrDefault(reader, "version", 1),
                ServerVersion = GetIntOrDefault(reader, "serverversion", 0),
                Deleted = GetIntOrDefault(reader, "deleted", 0),
                ConflictData = GetStringOrDefault(reader, "conflictdata", ""),
                DataHash = GetStringOrDefault(reader, "datahash", "")
            };

            // WHO 2020 enhancements - safely read new columns
            try
            {
                item.EffacementPercent = GetIntOrDefault(reader, "effacementPercent", 0);
                item.Consistency = GetStringOrDefault(reader, "consistency", "Medium");
                item.Position = GetStringOrDefault(reader, "position", "Mid");
                item.ApplicationToHead = GetBoolFromInt(reader, "applicationToHead");
                item.CervicalEdema = GetStringOrDefault(reader, "cervicalEdema", "None");
                item.MembraneStatus = GetStringOrDefault(reader, "membraneStatus", "");
                item.CervicalLip = GetBoolFromInt(reader, "cervicalLip");
                item.DilatationRateCmPerHour = GetNullableDecimal(reader, "dilatationRateCmPerHour");
                item.ProgressionRate = GetStringOrDefault(reader, "progressionRate", "");
                item.CrossedActionLine = GetBoolFromInt(reader, "crossedActionLine");
                item.CrossedAlertLine = GetBoolFromInt(reader, "crossedAlertLine");
                item.ActionLineCrossedTime = GetNullableDateTime(reader, "actionLineCrossedTime");
                item.CervicalLengthCm = GetNullableDecimal(reader, "cervicalLengthCm");
                item.ExaminerName = GetStringOrDefault(reader, "examinerName", "");
                item.ExamDurationMinutes = GetNullableInt(reader, "examDurationMinutes");
                item.DifficultExam = GetBoolFromInt(reader, "difficultExam");
                item.ExamDifficulty = GetStringOrDefault(reader, "examDifficulty", "");
                item.CervicalThickness = GetStringOrDefault(reader, "cervicalThickness", "");
                item.AnteriorCervicalLip = GetBoolFromInt(reader, "anteriorCervicalLip");
                item.PosteriorCervicalLip = GetBoolFromInt(reader, "posteriorCervicalLip");
                item.CervicalDilatationPattern = GetStringOrDefault(reader, "cervicalDilatationPattern", "");
                item.StationRelativeToPelvicSpines = GetNullableInt(reader, "stationRelativeToPelvicSpines");
                item.PresentingPartPosition = GetStringOrDefault(reader, "presentingPartPosition", "");
                item.PresentingPartWellApplied = GetBoolFromInt(reader, "presentingPartWellApplied");
                item.MembranesBulging = GetBoolFromInt(reader, "membranesBulging");
                item.ForewatersPresent = GetBoolFromInt(reader, "forewatersPresent");
                item.HindwatersPresent = GetBoolFromInt(reader, "hindwatersPresent");
                item.ClinicalAlert = GetStringOrDefault(reader, "clinicalAlert", "");
                item.ProlongedLatentPhase = GetBoolFromInt(reader, "prolongedLatentPhase");
                item.ProtractedActivePhase = GetBoolFromInt(reader, "protractedActivePhase");
                item.ArrestedDilatation = GetBoolFromInt(reader, "arrestedDilatation");
                item.PrecipitousLabor = GetBoolFromInt(reader, "precipitousLabor");
            }
            catch { /* Columns don't exist yet in old databases */ }

            return item;
        }

        protected override string GetInsertSql() => @"
INSERT INTO Tbl_CervixDilatation (ID, partographID, time, handler, notes, dilatation, effacementPercent, consistency, position, applicationToHead, cervicalEdema, membraneStatus, cervicalLip,
    dilatationRateCmPerHour, progressionRate, crossedActionLine, crossedAlertLine, actionLineCrossedTime, cervicalLengthCm, examinerName, examDurationMinutes, difficultExam, examDifficulty,
    cervicalThickness, anteriorCervicalLip, posteriorCervicalLip, cervicalDilatationPattern, stationRelativeToPelvicSpines, presentingPartPosition, presentingPartWellApplied,
    membranesBulging, forewatersPresent, hindwatersPresent, clinicalAlert, prolongedLatentPhase, protractedActivePhase, arrestedDilatation, precipitousLabor,
    createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash)
VALUES (@id, @partographId, @time, @handler, @notes, @dilatation, @effacementPercent, @consistency, @position, @applicationToHead, @cervicalEdema, @membraneStatus, @cervicalLip,
    @dilatationRateCmPerHour, @progressionRate, @crossedActionLine, @crossedAlertLine, @actionLineCrossedTime, @cervicalLengthCm, @examinerName, @examDurationMinutes, @difficultExam, @examDifficulty,
    @cervicalThickness, @anteriorCervicalLip, @posteriorCervicalLip, @cervicalDilatationPattern, @stationRelativeToPelvicSpines, @presentingPartPosition, @presentingPartWellApplied,
    @membranesBulging, @forewatersPresent, @hindwatersPresent, @clinicalAlert, @prolongedLatentPhase, @protractedActivePhase, @arrestedDilatation, @precipitousLabor,
    @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash);";

        protected override string GetUpdateSql() => @"
UPDATE Tbl_CervixDilatation
SET partographID = @partographId,
    time = @time,
    handler = @handler,
    notes = @notes,
    dilatation = @dilatation,
    effacementPercent = @effacementPercent,
    consistency = @consistency,
    position = @position,
    applicationToHead = @applicationToHead,
    cervicalEdema = @cervicalEdema,
    membraneStatus = @membraneStatus,
    cervicalLip = @cervicalLip,
    dilatationRateCmPerHour = @dilatationRateCmPerHour,
    progressionRate = @progressionRate,
    crossedActionLine = @crossedActionLine,
    crossedAlertLine = @crossedAlertLine,
    actionLineCrossedTime = @actionLineCrossedTime,
    cervicalLengthCm = @cervicalLengthCm,
    examinerName = @examinerName,
    examDurationMinutes = @examDurationMinutes,
    difficultExam = @difficultExam,
    examDifficulty = @examDifficulty,
    cervicalThickness = @cervicalThickness,
    anteriorCervicalLip = @anteriorCervicalLip,
    posteriorCervicalLip = @posteriorCervicalLip,
    cervicalDilatationPattern = @cervicalDilatationPattern,
    stationRelativeToPelvicSpines = @stationRelativeToPelvicSpines,
    presentingPartPosition = @presentingPartPosition,
    presentingPartWellApplied = @presentingPartWellApplied,
    membranesBulging = @membranesBulging,
    forewatersPresent = @forewatersPresent,
    hindwatersPresent = @hindwatersPresent,
    clinicalAlert = @clinicalAlert,
    prolongedLatentPhase = @prolongedLatentPhase,
    protractedActivePhase = @protractedActivePhase,
    arrestedDilatation = @arrestedDilatation,
    precipitousLabor = @precipitousLabor,
    updatedtime = @updatedtime,
    deletedtime = @deletedtime,
    deviceid = @deviceid,
    syncstatus = @syncstatus,
    version = @version,
    datahash = @datahash
WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, CervixDilatation item)
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

            // Base parameters
            cmd.Parameters.AddWithValue("@id", item.ID.ToString());
            cmd.Parameters.AddWithValue("@partographId", item.PartographID.ToString());
            cmd.Parameters.AddWithValue("@time", item.Time.ToString("O"));
            cmd.Parameters.AddWithValue("@handler", item.Handler.ToString());
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@dilatation", item.DilatationCm);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@effacementPercent", item.EffacementPercent);
            cmd.Parameters.AddWithValue("@consistency", item.Consistency ?? "Medium");
            cmd.Parameters.AddWithValue("@position", item.Position ?? "Mid");
            cmd.Parameters.AddWithValue("@applicationToHead", item.ApplicationToHead ? 1 : 0);
            cmd.Parameters.AddWithValue("@cervicalEdema", item.CervicalEdema ?? "None");
            cmd.Parameters.AddWithValue("@membraneStatus", item.MembraneStatus ?? "");
            cmd.Parameters.AddWithValue("@cervicalLip", item.CervicalLip ? 1 : 0);
            cmd.Parameters.AddWithValue("@dilatationRateCmPerHour", item.DilatationRateCmPerHour ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@progressionRate", item.ProgressionRate ?? "");
            cmd.Parameters.AddWithValue("@crossedActionLine", item.CrossedActionLine ? 1 : 0);
            cmd.Parameters.AddWithValue("@crossedAlertLine", item.CrossedAlertLine ? 1 : 0);
            cmd.Parameters.AddWithValue("@actionLineCrossedTime", item.ActionLineCrossedTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@cervicalLengthCm", item.CervicalLengthCm ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@examinerName", item.ExaminerName ?? "");
            cmd.Parameters.AddWithValue("@examDurationMinutes", item.ExamDurationMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@difficultExam", item.DifficultExam ? 1 : 0);
            cmd.Parameters.AddWithValue("@examDifficulty", item.ExamDifficulty ?? "");
            cmd.Parameters.AddWithValue("@cervicalThickness", item.CervicalThickness ?? "");
            cmd.Parameters.AddWithValue("@anteriorCervicalLip", item.AnteriorCervicalLip ? 1 : 0);
            cmd.Parameters.AddWithValue("@posteriorCervicalLip", item.PosteriorCervicalLip ? 1 : 0);
            cmd.Parameters.AddWithValue("@cervicalDilatationPattern", item.CervicalDilatationPattern ?? "");
            cmd.Parameters.AddWithValue("@stationRelativeToPelvicSpines", item.StationRelativeToPelvicSpines ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@presentingPartPosition", item.PresentingPartPosition ?? "");
            cmd.Parameters.AddWithValue("@presentingPartWellApplied", item.PresentingPartWellApplied ? 1 : 0);
            cmd.Parameters.AddWithValue("@membranesBulging", item.MembranesBulging ? 1 : 0);
            cmd.Parameters.AddWithValue("@forewatersPresent", item.ForewatersPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@hindwatersPresent", item.HindwatersPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@clinicalAlert", item.ClinicalAlert ?? "");
            cmd.Parameters.AddWithValue("@prolongedLatentPhase", item.ProlongedLatentPhase ? 1 : 0);
            cmd.Parameters.AddWithValue("@protractedActivePhase", item.ProtractedActivePhase ? 1 : 0);
            cmd.Parameters.AddWithValue("@arrestedDilatation", item.ArrestedDilatation ? 1 : 0);
            cmd.Parameters.AddWithValue("@precipitousLabor", item.PrecipitousLabor ? 1 : 0);

            // Sync parameters
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
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, CervixDilatation item)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            item.UpdatedTime = now;
            item.DeviceId = DeviceIdentity.GetOrCreateDeviceId();
            item.Version++;
            item.SyncStatus = 0;
            item.DataHash = item.CalculateHash();

            // Base parameters
            cmd.Parameters.AddWithValue("@id", item.ID.ToString());
            cmd.Parameters.AddWithValue("@partographId", item.PartographID.ToString());
            cmd.Parameters.AddWithValue("@time", item.Time.ToString("O"));
            cmd.Parameters.AddWithValue("@handler", item.Handler.ToString());
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@dilatation", item.DilatationCm);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@effacementPercent", item.EffacementPercent);
            cmd.Parameters.AddWithValue("@consistency", item.Consistency ?? "Medium");
            cmd.Parameters.AddWithValue("@position", item.Position ?? "Mid");
            cmd.Parameters.AddWithValue("@applicationToHead", item.ApplicationToHead ? 1 : 0);
            cmd.Parameters.AddWithValue("@cervicalEdema", item.CervicalEdema ?? "None");
            cmd.Parameters.AddWithValue("@membraneStatus", item.MembraneStatus ?? "");
            cmd.Parameters.AddWithValue("@cervicalLip", item.CervicalLip ? 1 : 0);
            cmd.Parameters.AddWithValue("@dilatationRateCmPerHour", item.DilatationRateCmPerHour ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@progressionRate", item.ProgressionRate ?? "");
            cmd.Parameters.AddWithValue("@crossedActionLine", item.CrossedActionLine ? 1 : 0);
            cmd.Parameters.AddWithValue("@crossedAlertLine", item.CrossedAlertLine ? 1 : 0);
            cmd.Parameters.AddWithValue("@actionLineCrossedTime", item.ActionLineCrossedTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@cervicalLengthCm", item.CervicalLengthCm ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@examinerName", item.ExaminerName ?? "");
            cmd.Parameters.AddWithValue("@examDurationMinutes", item.ExamDurationMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@difficultExam", item.DifficultExam ? 1 : 0);
            cmd.Parameters.AddWithValue("@examDifficulty", item.ExamDifficulty ?? "");
            cmd.Parameters.AddWithValue("@cervicalThickness", item.CervicalThickness ?? "");
            cmd.Parameters.AddWithValue("@anteriorCervicalLip", item.AnteriorCervicalLip ? 1 : 0);
            cmd.Parameters.AddWithValue("@posteriorCervicalLip", item.PosteriorCervicalLip ? 1 : 0);
            cmd.Parameters.AddWithValue("@cervicalDilatationPattern", item.CervicalDilatationPattern ?? "");
            cmd.Parameters.AddWithValue("@stationRelativeToPelvicSpines", item.StationRelativeToPelvicSpines ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@presentingPartPosition", item.PresentingPartPosition ?? "");
            cmd.Parameters.AddWithValue("@presentingPartWellApplied", item.PresentingPartWellApplied ? 1 : 0);
            cmd.Parameters.AddWithValue("@membranesBulging", item.MembranesBulging ? 1 : 0);
            cmd.Parameters.AddWithValue("@forewatersPresent", item.ForewatersPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@hindwatersPresent", item.HindwatersPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@clinicalAlert", item.ClinicalAlert ?? "");
            cmd.Parameters.AddWithValue("@prolongedLatentPhase", item.ProlongedLatentPhase ? 1 : 0);
            cmd.Parameters.AddWithValue("@protractedActivePhase", item.ProtractedActivePhase ? 1 : 0);
            cmd.Parameters.AddWithValue("@arrestedDilatation", item.ArrestedDilatation ? 1 : 0);
            cmd.Parameters.AddWithValue("@precipitousLabor", item.PrecipitousLabor ? 1 : 0);

            // Sync parameters
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);
        }

        // Helper methods for safe column reading
        private bool GetBoolFromInt(SqliteDataReader reader, string columnName, bool defaultValue = false)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal)) return defaultValue;
                return reader.GetInt32(ordinal) == 1;
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

        private int? GetNullableInt(SqliteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
            }
            catch { return null; }
        }

        private long? GetNullableLong(SqliteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
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

        private decimal? GetNullableDecimal(SqliteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal)) return null;
                return (decimal)reader.GetDouble(ordinal);
            }
            catch { return null; }
        }
    }
}
