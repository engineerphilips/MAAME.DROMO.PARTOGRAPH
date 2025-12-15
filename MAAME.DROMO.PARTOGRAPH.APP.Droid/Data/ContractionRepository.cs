using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Contractions Repository - WHO 2020 Enhanced
    public class ContractionRepository : BasePartographRepository<Contraction>
    {
        protected override string TableName => "Tbl_Contraction";
        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_Contraction (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                frequencyPer10Min INTEGER DEFAULT 0,
                durationSeconds INTEGER DEFAULT 0,
                strength TEXT DEFAULT 'Moderate',
                regularity TEXT DEFAULT 'Regular',
                palpableAtRest INTEGER DEFAULT 0,
                coordinated INTEGER DEFAULT 1,
                effectOnCervix TEXT DEFAULT '',
                intensityMmHg INTEGER,
                clinicalAlert TEXT DEFAULT '',
                contractionPattern TEXT DEFAULT '',
                tachysystole INTEGER DEFAULT 0,
                hyperstimulation INTEGER DEFAULT 0,
                tachysystoleStartTime TEXT,
                tachysystoleDurationMinutes INTEGER,
                intensityAssessment TEXT DEFAULT '',
                indentableDuringContraction INTEGER DEFAULT 0,
                uterusRelaxesBetweenContractions INTEGER DEFAULT 1,
                relaxationTimeSeconds INTEGER,
                restingToneMmHg INTEGER,
                peakPressureMmHg INTEGER,
                montevideUnits INTEGER,
                shortestDurationSeconds INTEGER,
                longestDurationSeconds INTEGER,
                averageDurationSeconds INTEGER,
                prolongedContractions INTEGER DEFAULT 0,
                prolongedContractionCount INTEGER,
                frequencyTrend TEXT DEFAULT '',
                irregularFrequency INTEGER DEFAULT 0,
                averageIntervalMinutes REAL,
                maternalCopingLevel TEXT DEFAULT '',
                maternalExhaustion INTEGER DEFAULT 0,
                painLocation TEXT DEFAULT '',
                onOxytocin INTEGER DEFAULT 0,
                oxytocinEffect TEXT DEFAULT '',
                oxytocinAdjustmentNeeded INTEGER DEFAULT 0,
                suggestedOxytocinAction TEXT DEFAULT '',
                interventionRequired INTEGER DEFAULT 0,
                interventionTaken TEXT DEFAULT '',
                interventionTime TEXT,
                oxytocinStopped INTEGER DEFAULT 0,
                oxytocinReduced INTEGER DEFAULT 0,
                tocolyticsGiven INTEGER DEFAULT 0,
                hypertonicUterus INTEGER DEFAULT 0,
                uterineRuptureRisk INTEGER DEFAULT 0,
                fhrCompromise INTEGER DEFAULT 0,
                emergencyConsultRequired INTEGER DEFAULT 0,
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

            CREATE INDEX IF NOT EXISTS idx_contraction_sync ON Tbl_Contraction(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_contraction_server_version ON Tbl_Contraction(serverversion);

            -- Add WHO 2020 columns to existing tables
            ALTER TABLE Tbl_Contraction ADD COLUMN strength TEXT DEFAULT 'Moderate';
            ALTER TABLE Tbl_Contraction ADD COLUMN regularity TEXT DEFAULT 'Regular';
            ALTER TABLE Tbl_Contraction ADD COLUMN palpableAtRest INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Contraction ADD COLUMN coordinated INTEGER DEFAULT 1;
            ALTER TABLE Tbl_Contraction ADD COLUMN effectOnCervix TEXT DEFAULT '';
            ALTER TABLE Tbl_Contraction ADD COLUMN intensityMmHg INTEGER;
            ALTER TABLE Tbl_Contraction ADD COLUMN clinicalAlert TEXT DEFAULT '';
            ALTER TABLE Tbl_Contraction ADD COLUMN contractionPattern TEXT DEFAULT '';
            ALTER TABLE Tbl_Contraction ADD COLUMN tachysystole INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Contraction ADD COLUMN hyperstimulation INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Contraction ADD COLUMN tachysystoleStartTime TEXT;
            ALTER TABLE Tbl_Contraction ADD COLUMN tachysystoleDurationMinutes INTEGER;
            ALTER TABLE Tbl_Contraction ADD COLUMN intensityAssessment TEXT DEFAULT '';
            ALTER TABLE Tbl_Contraction ADD COLUMN indentableDuringContraction INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Contraction ADD COLUMN uterusRelaxesBetweenContractions INTEGER DEFAULT 1;
            ALTER TABLE Tbl_Contraction ADD COLUMN relaxationTimeSeconds INTEGER;
            ALTER TABLE Tbl_Contraction ADD COLUMN restingToneMmHg INTEGER;
            ALTER TABLE Tbl_Contraction ADD COLUMN peakPressureMmHg INTEGER;
            ALTER TABLE Tbl_Contraction ADD COLUMN montevideUnits INTEGER;
            ALTER TABLE Tbl_Contraction ADD COLUMN shortestDurationSeconds INTEGER;
            ALTER TABLE Tbl_Contraction ADD COLUMN longestDurationSeconds INTEGER;
            ALTER TABLE Tbl_Contraction ADD COLUMN averageDurationSeconds INTEGER;
            ALTER TABLE Tbl_Contraction ADD COLUMN prolongedContractions INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Contraction ADD COLUMN prolongedContractionCount INTEGER;
            ALTER TABLE Tbl_Contraction ADD COLUMN frequencyTrend TEXT DEFAULT '';
            ALTER TABLE Tbl_Contraction ADD COLUMN irregularFrequency INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Contraction ADD COLUMN averageIntervalMinutes REAL;
            ALTER TABLE Tbl_Contraction ADD COLUMN maternalCopingLevel TEXT DEFAULT '';
            ALTER TABLE Tbl_Contraction ADD COLUMN maternalExhaustion INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Contraction ADD COLUMN painLocation TEXT DEFAULT '';
            ALTER TABLE Tbl_Contraction ADD COLUMN onOxytocin INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Contraction ADD COLUMN oxytocinEffect TEXT DEFAULT '';
            ALTER TABLE Tbl_Contraction ADD COLUMN oxytocinAdjustmentNeeded INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Contraction ADD COLUMN suggestedOxytocinAction TEXT DEFAULT '';
            ALTER TABLE Tbl_Contraction ADD COLUMN interventionRequired INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Contraction ADD COLUMN interventionTaken TEXT DEFAULT '';
            ALTER TABLE Tbl_Contraction ADD COLUMN interventionTime TEXT;
            ALTER TABLE Tbl_Contraction ADD COLUMN oxytocinStopped INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Contraction ADD COLUMN oxytocinReduced INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Contraction ADD COLUMN tocolyticsGiven INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Contraction ADD COLUMN hypertonicUterus INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Contraction ADD COLUMN uterineRuptureRisk INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Contraction ADD COLUMN fhrCompromise INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Contraction ADD COLUMN emergencyConsultRequired INTEGER DEFAULT 0;

            DROP TRIGGER IF EXISTS trg_contraction_insert;
            CREATE TRIGGER trg_contraction_insert
            AFTER INSERT ON Tbl_Contraction
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_Contraction
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_contraction_update;
            CREATE TRIGGER trg_contraction_update
            AFTER UPDATE ON Tbl_Contraction
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_Contraction
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;";

        public ContractionRepository(ILogger<ContractionRepository> logger) : base(logger) { }

        protected override Contraction MapFromReader(SqliteDataReader reader)
        {
            var item = new Contraction
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = GetStringOrDefault(reader, "handler", ""),
                Notes = GetStringOrDefault(reader, "notes", ""),
                FrequencyPer10Min = GetIntOrDefault(reader, "frequencyPer10Min", 0),
                DurationSeconds = GetIntOrDefault(reader, "durationSeconds", 0),
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
                item.Strength = GetStringOrDefault(reader, "strength", "Moderate");
                item.Regularity = GetStringOrDefault(reader, "regularity", "Regular");
                item.PalpableAtRest = GetBoolFromInt(reader, "palpableAtRest");
                item.Coordinated = GetBoolFromInt(reader, "coordinated", true);
                item.EffectOnCervix = GetStringOrDefault(reader, "effectOnCervix", "");
                item.IntensityMmHg = GetNullableInt(reader, "intensityMmHg");
                item.ClinicalAlert = GetStringOrDefault(reader, "clinicalAlert", "");
                item.ContractionPattern = GetStringOrDefault(reader, "contractionPattern", "");
                item.Tachysystole = GetBoolFromInt(reader, "tachysystole");
                item.Hyperstimulation = GetBoolFromInt(reader, "hyperstimulation");
                item.TachysystoleStartTime = GetNullableDateTime(reader, "tachysystoleStartTime");
                item.TachysystoleDurationMinutes = GetNullableInt(reader, "tachysystoleDurationMinutes");
                item.IntensityAssessment = GetStringOrDefault(reader, "intensityAssessment", "");
                item.IndentableDuringContraction = GetBoolFromInt(reader, "indentableDuringContraction");
                item.UterusRelaxesBetweenContractions = GetBoolFromInt(reader, "uterusRelaxesBetweenContractions", true);
                item.RelaxationTimeSeconds = GetNullableInt(reader, "relaxationTimeSeconds");
                item.RestingToneMmHg = GetNullableInt(reader, "restingToneMmHg");
                item.PeakPressureMmHg = GetNullableInt(reader, "peakPressureMmHg");
                item.MontevideUnits = GetNullableInt(reader, "montevideUnits");
                item.ShortestDurationSeconds = GetNullableInt(reader, "shortestDurationSeconds");
                item.LongestDurationSeconds = GetNullableInt(reader, "longestDurationSeconds");
                item.AverageDurationSeconds = GetNullableInt(reader, "averageDurationSeconds");
                item.ProlongedContractions = GetBoolFromInt(reader, "prolongedContractions");
                item.ProlongedContractionCount = GetNullableInt(reader, "prolongedContractionCount");
                item.FrequencyTrend = GetStringOrDefault(reader, "frequencyTrend", "");
                item.IrregularFrequency = GetBoolFromInt(reader, "irregularFrequency");
                item.AverageIntervalMinutes = GetNullableDecimal(reader, "averageIntervalMinutes");
                item.MaternalCopingLevel = GetStringOrDefault(reader, "maternalCopingLevel", "");
                item.MaternalExhaustion = GetBoolFromInt(reader, "maternalExhaustion");
                item.PainLocation = GetStringOrDefault(reader, "painLocation", "");
                item.OnOxytocin = GetBoolFromInt(reader, "onOxytocin");
                item.OxytocinEffect = GetStringOrDefault(reader, "oxytocinEffect", "");
                item.OxytocinAdjustmentNeeded = GetBoolFromInt(reader, "oxytocinAdjustmentNeeded");
                item.SuggestedOxytocinAction = GetStringOrDefault(reader, "suggestedOxytocinAction", "");
                item.InterventionRequired = GetBoolFromInt(reader, "interventionRequired");
                item.InterventionTaken = GetStringOrDefault(reader, "interventionTaken", "");
                item.InterventionTime = GetNullableDateTime(reader, "interventionTime");
                item.OxytocinStopped = GetBoolFromInt(reader, "oxytocinStopped");
                item.OxytocinReduced = GetBoolFromInt(reader, "oxytocinReduced");
                item.TocolyticsGiven = GetBoolFromInt(reader, "tocolyticsGiven");
                item.HypertonicUterus = GetBoolFromInt(reader, "hypertonicUterus");
                item.UterineRuptureRisk = GetBoolFromInt(reader, "uterineRuptureRisk");
                item.FHRCompromise = GetBoolFromInt(reader, "fhrCompromise");
                item.EmergencyConsultRequired = GetBoolFromInt(reader, "emergencyConsultRequired");
            }
            catch { /* Columns don't exist yet in old databases */ }

            return item;
        }

        protected override string GetInsertSql() => @"
INSERT INTO Tbl_Contraction (ID, partographID, time, handler, notes, frequencyPer10Min, durationSeconds, strength, regularity, palpableAtRest, coordinated, effectOnCervix, intensityMmHg, clinicalAlert,
    contractionPattern, tachysystole, hyperstimulation, tachysystoleStartTime, tachysystoleDurationMinutes, intensityAssessment, indentableDuringContraction, uterusRelaxesBetweenContractions, relaxationTimeSeconds,
    restingToneMmHg, peakPressureMmHg, montevideUnits, shortestDurationSeconds, longestDurationSeconds, averageDurationSeconds, prolongedContractions, prolongedContractionCount,
    frequencyTrend, irregularFrequency, averageIntervalMinutes, maternalCopingLevel, maternalExhaustion, painLocation,
    onOxytocin, oxytocinEffect, oxytocinAdjustmentNeeded, suggestedOxytocinAction, interventionRequired, interventionTaken, interventionTime, oxytocinStopped, oxytocinReduced, tocolyticsGiven,
    hypertonicUterus, uterineRuptureRisk, fhrCompromise, emergencyConsultRequired,
    createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash)
VALUES (@id, @partographId, @time, @handler, @notes, @frequencyPer10Min, @durationSeconds, @strength, @regularity, @palpableAtRest, @coordinated, @effectOnCervix, @intensityMmHg, @clinicalAlert,
    @contractionPattern, @tachysystole, @hyperstimulation, @tachysystoleStartTime, @tachysystoleDurationMinutes, @intensityAssessment, @indentableDuringContraction, @uterusRelaxesBetweenContractions, @relaxationTimeSeconds,
    @restingToneMmHg, @peakPressureMmHg, @montevideUnits, @shortestDurationSeconds, @longestDurationSeconds, @averageDurationSeconds, @prolongedContractions, @prolongedContractionCount,
    @frequencyTrend, @irregularFrequency, @averageIntervalMinutes, @maternalCopingLevel, @maternalExhaustion, @painLocation,
    @onOxytocin, @oxytocinEffect, @oxytocinAdjustmentNeeded, @suggestedOxytocinAction, @interventionRequired, @interventionTaken, @interventionTime, @oxytocinStopped, @oxytocinReduced, @tocolyticsGiven,
    @hypertonicUterus, @uterineRuptureRisk, @fhrCompromise, @emergencyConsultRequired,
    @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash);";

        protected override string GetUpdateSql() => @"
UPDATE Tbl_Contraction
SET partographID = @partographId,
    time = @time,
    handler = @handler,
    notes = @notes,
    frequencyPer10Min = @frequencyPer10Min,
    durationSeconds = @durationSeconds,
    strength = @strength,
    regularity = @regularity,
    palpableAtRest = @palpableAtRest,
    coordinated = @coordinated,
    effectOnCervix = @effectOnCervix,
    intensityMmHg = @intensityMmHg,
    clinicalAlert = @clinicalAlert,
    contractionPattern = @contractionPattern,
    tachysystole = @tachysystole,
    hyperstimulation = @hyperstimulation,
    tachysystoleStartTime = @tachysystoleStartTime,
    tachysystoleDurationMinutes = @tachysystoleDurationMinutes,
    intensityAssessment = @intensityAssessment,
    indentableDuringContraction = @indentableDuringContraction,
    uterusRelaxesBetweenContractions = @uterusRelaxesBetweenContractions,
    relaxationTimeSeconds = @relaxationTimeSeconds,
    restingToneMmHg = @restingToneMmHg,
    peakPressureMmHg = @peakPressureMmHg,
    montevideUnits = @montevideUnits,
    shortestDurationSeconds = @shortestDurationSeconds,
    longestDurationSeconds = @longestDurationSeconds,
    averageDurationSeconds = @averageDurationSeconds,
    prolongedContractions = @prolongedContractions,
    prolongedContractionCount = @prolongedContractionCount,
    frequencyTrend = @frequencyTrend,
    irregularFrequency = @irregularFrequency,
    averageIntervalMinutes = @averageIntervalMinutes,
    maternalCopingLevel = @maternalCopingLevel,
    maternalExhaustion = @maternalExhaustion,
    painLocation = @painLocation,
    onOxytocin = @onOxytocin,
    oxytocinEffect = @oxytocinEffect,
    oxytocinAdjustmentNeeded = @oxytocinAdjustmentNeeded,
    suggestedOxytocinAction = @suggestedOxytocinAction,
    interventionRequired = @interventionRequired,
    interventionTaken = @interventionTaken,
    interventionTime = @interventionTime,
    oxytocinStopped = @oxytocinStopped,
    oxytocinReduced = @oxytocinReduced,
    tocolyticsGiven = @tocolyticsGiven,
    hypertonicUterus = @hypertonicUterus,
    uterineRuptureRisk = @uterineRuptureRisk,
    fhrCompromise = @fhrCompromise,
    emergencyConsultRequired = @emergencyConsultRequired,
    updatedtime = @updatedtime,
    deletedtime = @deletedtime,
    deviceid = @deviceid,
    syncstatus = @syncstatus,
    version = @version,
    datahash = @datahash
WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, Contraction item)
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
            cmd.Parameters.AddWithValue("@frequencyPer10Min", item.FrequencyPer10Min);
            cmd.Parameters.AddWithValue("@durationSeconds", item.DurationSeconds);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@strength", item.Strength ?? "Moderate");
            cmd.Parameters.AddWithValue("@regularity", item.Regularity ?? "Regular");
            cmd.Parameters.AddWithValue("@palpableAtRest", item.PalpableAtRest ? 1 : 0);
            cmd.Parameters.AddWithValue("@coordinated", item.Coordinated ? 1 : 0);
            cmd.Parameters.AddWithValue("@effectOnCervix", item.EffectOnCervix ?? "");
            cmd.Parameters.AddWithValue("@intensityMmHg", item.IntensityMmHg ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clinicalAlert", item.ClinicalAlert ?? "");
            cmd.Parameters.AddWithValue("@contractionPattern", item.ContractionPattern ?? "");
            cmd.Parameters.AddWithValue("@tachysystole", item.Tachysystole ? 1 : 0);
            cmd.Parameters.AddWithValue("@hyperstimulation", item.Hyperstimulation ? 1 : 0);
            cmd.Parameters.AddWithValue("@tachysystoleStartTime", item.TachysystoleStartTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@tachysystoleDurationMinutes", item.TachysystoleDurationMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@intensityAssessment", item.IntensityAssessment ?? "");
            cmd.Parameters.AddWithValue("@indentableDuringContraction", item.IndentableDuringContraction ? 1 : 0);
            cmd.Parameters.AddWithValue("@uterusRelaxesBetweenContractions", item.UterusRelaxesBetweenContractions ? 1 : 0);
            cmd.Parameters.AddWithValue("@relaxationTimeSeconds", item.RelaxationTimeSeconds ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@restingToneMmHg", item.RestingToneMmHg ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@peakPressureMmHg", item.PeakPressureMmHg ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@montevideUnits", item.MontevideUnits ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@shortestDurationSeconds", item.ShortestDurationSeconds ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@longestDurationSeconds", item.LongestDurationSeconds ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@averageDurationSeconds", item.AverageDurationSeconds ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@prolongedContractions", item.ProlongedContractions ? 1 : 0);
            cmd.Parameters.AddWithValue("@prolongedContractionCount", item.ProlongedContractionCount ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@frequencyTrend", item.FrequencyTrend ?? "");
            cmd.Parameters.AddWithValue("@irregularFrequency", item.IrregularFrequency ? 1 : 0);
            cmd.Parameters.AddWithValue("@averageIntervalMinutes", item.AverageIntervalMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@maternalCopingLevel", item.MaternalCopingLevel ?? "");
            cmd.Parameters.AddWithValue("@maternalExhaustion", item.MaternalExhaustion ? 1 : 0);
            cmd.Parameters.AddWithValue("@painLocation", item.PainLocation ?? "");
            cmd.Parameters.AddWithValue("@onOxytocin", item.OnOxytocin ? 1 : 0);
            cmd.Parameters.AddWithValue("@oxytocinEffect", item.OxytocinEffect ?? "");
            cmd.Parameters.AddWithValue("@oxytocinAdjustmentNeeded", item.OxytocinAdjustmentNeeded ? 1 : 0);
            cmd.Parameters.AddWithValue("@suggestedOxytocinAction", item.SuggestedOxytocinAction ?? "");
            cmd.Parameters.AddWithValue("@interventionRequired", item.InterventionRequired ? 1 : 0);
            cmd.Parameters.AddWithValue("@interventionTaken", item.InterventionTaken ?? "");
            cmd.Parameters.AddWithValue("@interventionTime", item.InterventionTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@oxytocinStopped", item.OxytocinStopped ? 1 : 0);
            cmd.Parameters.AddWithValue("@oxytocinReduced", item.OxytocinReduced ? 1 : 0);
            cmd.Parameters.AddWithValue("@tocolyticsGiven", item.TocolyticsGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@hypertonicUterus", item.HypertonicUterus ? 1 : 0);
            cmd.Parameters.AddWithValue("@uterineRuptureRisk", item.UterineRuptureRisk ? 1 : 0);
            cmd.Parameters.AddWithValue("@fhrCompromise", item.FHRCompromise ? 1 : 0);
            cmd.Parameters.AddWithValue("@emergencyConsultRequired", item.EmergencyConsultRequired ? 1 : 0);

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

        protected override void AddUpdateParameters(SqliteCommand cmd, Contraction item)
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
            cmd.Parameters.AddWithValue("@frequencyPer10Min", item.FrequencyPer10Min);
            cmd.Parameters.AddWithValue("@durationSeconds", item.DurationSeconds);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@strength", item.Strength ?? "Moderate");
            cmd.Parameters.AddWithValue("@regularity", item.Regularity ?? "Regular");
            cmd.Parameters.AddWithValue("@palpableAtRest", item.PalpableAtRest ? 1 : 0);
            cmd.Parameters.AddWithValue("@coordinated", item.Coordinated ? 1 : 0);
            cmd.Parameters.AddWithValue("@effectOnCervix", item.EffectOnCervix ?? "");
            cmd.Parameters.AddWithValue("@intensityMmHg", item.IntensityMmHg ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clinicalAlert", item.ClinicalAlert ?? "");
            cmd.Parameters.AddWithValue("@contractionPattern", item.ContractionPattern ?? "");
            cmd.Parameters.AddWithValue("@tachysystole", item.Tachysystole ? 1 : 0);
            cmd.Parameters.AddWithValue("@hyperstimulation", item.Hyperstimulation ? 1 : 0);
            cmd.Parameters.AddWithValue("@tachysystoleStartTime", item.TachysystoleStartTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@tachysystoleDurationMinutes", item.TachysystoleDurationMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@intensityAssessment", item.IntensityAssessment ?? "");
            cmd.Parameters.AddWithValue("@indentableDuringContraction", item.IndentableDuringContraction ? 1 : 0);
            cmd.Parameters.AddWithValue("@uterusRelaxesBetweenContractions", item.UterusRelaxesBetweenContractions ? 1 : 0);
            cmd.Parameters.AddWithValue("@relaxationTimeSeconds", item.RelaxationTimeSeconds ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@restingToneMmHg", item.RestingToneMmHg ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@peakPressureMmHg", item.PeakPressureMmHg ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@montevideUnits", item.MontevideUnits ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@shortestDurationSeconds", item.ShortestDurationSeconds ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@longestDurationSeconds", item.LongestDurationSeconds ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@averageDurationSeconds", item.AverageDurationSeconds ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@prolongedContractions", item.ProlongedContractions ? 1 : 0);
            cmd.Parameters.AddWithValue("@prolongedContractionCount", item.ProlongedContractionCount ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@frequencyTrend", item.FrequencyTrend ?? "");
            cmd.Parameters.AddWithValue("@irregularFrequency", item.IrregularFrequency ? 1 : 0);
            cmd.Parameters.AddWithValue("@averageIntervalMinutes", item.AverageIntervalMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@maternalCopingLevel", item.MaternalCopingLevel ?? "");
            cmd.Parameters.AddWithValue("@maternalExhaustion", item.MaternalExhaustion ? 1 : 0);
            cmd.Parameters.AddWithValue("@painLocation", item.PainLocation ?? "");
            cmd.Parameters.AddWithValue("@onOxytocin", item.OnOxytocin ? 1 : 0);
            cmd.Parameters.AddWithValue("@oxytocinEffect", item.OxytocinEffect ?? "");
            cmd.Parameters.AddWithValue("@oxytocinAdjustmentNeeded", item.OxytocinAdjustmentNeeded ? 1 : 0);
            cmd.Parameters.AddWithValue("@suggestedOxytocinAction", item.SuggestedOxytocinAction ?? "");
            cmd.Parameters.AddWithValue("@interventionRequired", item.InterventionRequired ? 1 : 0);
            cmd.Parameters.AddWithValue("@interventionTaken", item.InterventionTaken ?? "");
            cmd.Parameters.AddWithValue("@interventionTime", item.InterventionTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@oxytocinStopped", item.OxytocinStopped ? 1 : 0);
            cmd.Parameters.AddWithValue("@oxytocinReduced", item.OxytocinReduced ? 1 : 0);
            cmd.Parameters.AddWithValue("@tocolyticsGiven", item.TocolyticsGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@hypertonicUterus", item.HypertonicUterus ? 1 : 0);
            cmd.Parameters.AddWithValue("@uterineRuptureRisk", item.UterineRuptureRisk ? 1 : 0);
            cmd.Parameters.AddWithValue("@fhrCompromise", item.FHRCompromise ? 1 : 0);
            cmd.Parameters.AddWithValue("@emergencyConsultRequired", item.EmergencyConsultRequired ? 1 : 0);

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
