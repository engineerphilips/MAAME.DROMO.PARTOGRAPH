using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // BP Repository - WHO 2020 Enhanced
    public class BPRepository : BasePartographRepository<BP>
    {
        protected override string TableName => "Tbl_BP";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_BP (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                systolic INTEGER DEFAULT 0,
                diastolic INTEGER DEFAULT 0,
                pulse INTEGER DEFAULT 0,
                maternalPosition TEXT DEFAULT 'Sitting',
                cuffSize TEXT DEFAULT 'Standard',
                repeatMeasurement INTEGER DEFAULT 0,
                irregularPulse INTEGER DEFAULT 0,
                clinicalAlert TEXT DEFAULT '',
                bpCategory TEXT DEFAULT '',
                severeHypertension INTEGER DEFAULT 0,
                preeclampsiaRange INTEGER DEFAULT 0,
                firstElevatedBPTime TEXT,
                consecutiveElevatedReadings INTEGER,
                secondSystolic INTEGER,
                secondDiastolic INTEGER,
                secondReadingTime TEXT,
                thirdSystolic INTEGER,
                thirdDiastolic INTEGER,
                thirdReadingTime TEXT,
                pulseRhythm TEXT DEFAULT '',
                pulseVolume TEXT DEFAULT '',
                pulseCharacter TEXT DEFAULT '',
                pulseDeficit INTEGER DEFAULT 0,
                hypotension INTEGER DEFAULT 0,
                hypotensionCause TEXT DEFAULT '',
                posturalHypotension INTEGER DEFAULT 0,
                posturalDrop INTEGER,
                newOnsetHypertension INTEGER DEFAULT 0,
                knownHypertension INTEGER DEFAULT 0,
                onAntihypertensives INTEGER DEFAULT 0,
                antihypertensiveMedication TEXT DEFAULT '',
                lastAntihypertensiveDose TEXT,
                headache INTEGER DEFAULT 0,
                visualDisturbances INTEGER DEFAULT 0,
                epigastricPain INTEGER DEFAULT 0,
                hyperreflexia INTEGER DEFAULT 0,
                edema INTEGER DEFAULT 0,
                emergencyProtocolActivated INTEGER DEFAULT 0,
                antihypertensiveGiven INTEGER DEFAULT 0,
                antihypertensiveGivenTime TEXT,
                magnesiumSulfateGiven INTEGER DEFAULT 0,
                ivFluidsGiven INTEGER DEFAULT 0,
                positionChanged INTEGER DEFAULT 0,
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

            CREATE INDEX IF NOT EXISTS idx_bp_sync ON Tbl_BP(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_bp_server_version ON Tbl_BP(serverversion);

            DROP TRIGGER IF EXISTS trg_bp_insert;
            CREATE TRIGGER trg_bp_insert
            AFTER INSERT ON Tbl_BP
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_BP
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_bp_update;
            CREATE TRIGGER trg_bp_update
            AFTER UPDATE ON Tbl_BP
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_BP
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";
        
        //    -- Add WHO 2020 columns to existing tables
        //    ALTER TABLE Tbl_BP ADD COLUMN maternalPosition TEXT DEFAULT 'Sitting';
        //ALTER TABLE Tbl_BP ADD COLUMN cuffSize TEXT DEFAULT 'Standard';
        //ALTER TABLE Tbl_BP ADD COLUMN repeatMeasurement INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN irregularPulse INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN clinicalAlert TEXT DEFAULT '';
        //ALTER TABLE Tbl_BP ADD COLUMN bpCategory TEXT DEFAULT '';
        //ALTER TABLE Tbl_BP ADD COLUMN severeHypertension INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN preeclampsiaRange INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN firstElevatedBPTime TEXT;
        //    ALTER TABLE Tbl_BP ADD COLUMN consecutiveElevatedReadings INTEGER;
        //    ALTER TABLE Tbl_BP ADD COLUMN secondSystolic INTEGER;
        //    ALTER TABLE Tbl_BP ADD COLUMN secondDiastolic INTEGER;
        //    ALTER TABLE Tbl_BP ADD COLUMN secondReadingTime TEXT;
        //    ALTER TABLE Tbl_BP ADD COLUMN thirdSystolic INTEGER;
        //    ALTER TABLE Tbl_BP ADD COLUMN thirdDiastolic INTEGER;
        //    ALTER TABLE Tbl_BP ADD COLUMN thirdReadingTime TEXT;
        //    ALTER TABLE Tbl_BP ADD COLUMN pulseRhythm TEXT DEFAULT '';
        //ALTER TABLE Tbl_BP ADD COLUMN pulseVolume TEXT DEFAULT '';
        //ALTER TABLE Tbl_BP ADD COLUMN pulseCharacter TEXT DEFAULT '';
        //ALTER TABLE Tbl_BP ADD COLUMN pulseDeficit INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN hypotension INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN hypotensionCause TEXT DEFAULT '';
        //ALTER TABLE Tbl_BP ADD COLUMN posturalHypotension INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN posturalDrop INTEGER;
        //    ALTER TABLE Tbl_BP ADD COLUMN newOnsetHypertension INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN knownHypertension INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN onAntihypertensives INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN antihypertensiveMedication TEXT DEFAULT '';
        //ALTER TABLE Tbl_BP ADD COLUMN lastAntihypertensiveDose TEXT;
        //    ALTER TABLE Tbl_BP ADD COLUMN headache INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN visualDisturbances INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN epigastricPain INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN hyperreflexia INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN edema INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN emergencyProtocolActivated INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN antihypertensiveGiven INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN antihypertensiveGivenTime TEXT;
        //    ALTER TABLE Tbl_BP ADD COLUMN magnesiumSulfateGiven INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN ivFluidsGiven INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_BP ADD COLUMN positionChanged INTEGER DEFAULT 0;

        public BPRepository(ILogger<BPRepository> logger) : base(logger) { }

        protected override BP MapFromReader(SqliteDataReader reader)
        {
            var item = new BP
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = GetStringOrDefault(reader, "staffname", ""),
                Notes = GetStringOrDefault(reader, "notes", ""),
                Systolic = GetIntOrDefault(reader, "systolic", 0),
                Diastolic = GetIntOrDefault(reader, "diastolic", 0),
                Pulse = GetIntOrDefault(reader, "pulse", 0),
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
                item.MaternalPosition = GetStringOrDefault(reader, "maternalPosition", "Sitting");
                item.CuffSize = GetStringOrDefault(reader, "cuffSize", "Standard");
                item.RepeatMeasurement = GetBoolFromInt(reader, "repeatMeasurement");
                item.IrregularPulse = GetBoolFromInt(reader, "irregularPulse");
                item.ClinicalAlert = GetStringOrDefault(reader, "clinicalAlert", "");
                item.BPCategory = GetStringOrDefault(reader, "bpCategory", "");
                item.SevereHypertension = GetBoolFromInt(reader, "severeHypertension");
                item.PreeclampsiaRange = GetBoolFromInt(reader, "preeclampsiaRange");
                item.FirstElevatedBPTime = GetNullableDateTime(reader, "firstElevatedBPTime");
                item.ConsecutiveElevatedReadings = GetNullableInt(reader, "consecutiveElevatedReadings");
                item.SecondSystolic = GetNullableInt(reader, "secondSystolic");
                item.SecondDiastolic = GetNullableInt(reader, "secondDiastolic");
                item.SecondReadingTime = GetNullableDateTime(reader, "secondReadingTime");
                item.ThirdSystolic = GetNullableInt(reader, "thirdSystolic");
                item.ThirdDiastolic = GetNullableInt(reader, "thirdDiastolic");
                item.ThirdReadingTime = GetNullableDateTime(reader, "thirdReadingTime");
                item.PulseRhythm = GetStringOrDefault(reader, "pulseRhythm", "");
                item.PulseVolume = GetStringOrDefault(reader, "pulseVolume", "");
                item.PulseCharacter = GetStringOrDefault(reader, "pulseCharacter", "");
                item.PulseDeficit = GetBoolFromInt(reader, "pulseDeficit");
                item.Hypotension = GetBoolFromInt(reader, "hypotension");
                item.HypotensionCause = GetStringOrDefault(reader, "hypotensionCause", "");
                item.PosturalHypotension = GetBoolFromInt(reader, "posturalHypotension");
                item.PosturalDrop = GetNullableInt(reader, "posturalDrop");
                item.NewOnsetHypertension = GetBoolFromInt(reader, "newOnsetHypertension");
                item.KnownHypertension = GetBoolFromInt(reader, "knownHypertension");
                item.OnAntihypertensives = GetBoolFromInt(reader, "onAntihypertensives");
                item.AntihypertensiveMedication = GetStringOrDefault(reader, "antihypertensiveMedication", "");
                item.LastAntihypertensiveDose = GetNullableDateTime(reader, "lastAntihypertensiveDose");
                item.Headache = GetBoolFromInt(reader, "headache");
                item.VisualDisturbances = GetBoolFromInt(reader, "visualDisturbances");
                item.EpigastricPain = GetBoolFromInt(reader, "epigastricPain");
                item.Hyperreflexia = GetBoolFromInt(reader, "hyperreflexia");
                item.Edema = GetBoolFromInt(reader, "edema");
                item.EmergencyProtocolActivated = GetBoolFromInt(reader, "emergencyProtocolActivated");
                item.AntihypertensiveGiven = GetBoolFromInt(reader, "antihypertensiveGiven");
                item.AntihypertensiveGivenTime = GetNullableDateTime(reader, "antihypertensiveGivenTime");
                item.MagnesiumSulfateGiven = GetBoolFromInt(reader, "magnesiumSulfateGiven");
                item.IVFluidsGiven = GetBoolFromInt(reader, "ivFluidsGiven");
                item.PositionChanged = GetBoolFromInt(reader, "positionChanged");
            }
            catch { /* Columns don't exist yet in old databases */ }

            return item;
        }

        protected override string GetInsertSql() => @"
INSERT INTO Tbl_BP (ID, partographID, time, handler, notes, systolic, diastolic, pulse, maternalPosition, cuffSize, repeatMeasurement, irregularPulse, clinicalAlert,
    bpCategory, severeHypertension, preeclampsiaRange, firstElevatedBPTime, consecutiveElevatedReadings, secondSystolic, secondDiastolic, secondReadingTime, thirdSystolic, thirdDiastolic, thirdReadingTime,
    pulseRhythm, pulseVolume, pulseCharacter, pulseDeficit, hypotension, hypotensionCause, posturalHypotension, posturalDrop,
    newOnsetHypertension, knownHypertension, onAntihypertensives, antihypertensiveMedication, lastAntihypertensiveDose, headache, visualDisturbances, epigastricPain, hyperreflexia, edema,
    emergencyProtocolActivated, antihypertensiveGiven, antihypertensiveGivenTime, magnesiumSulfateGiven, ivFluidsGiven, positionChanged,
    createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash)
VALUES (@id, @partographId, @time, @handler, @notes, @systolic, @diastolic, @pulse, @maternalPosition, @cuffSize, @repeatMeasurement, @irregularPulse, @clinicalAlert,
    @bpCategory, @severeHypertension, @preeclampsiaRange, @firstElevatedBPTime, @consecutiveElevatedReadings, @secondSystolic, @secondDiastolic, @secondReadingTime, @thirdSystolic, @thirdDiastolic, @thirdReadingTime,
    @pulseRhythm, @pulseVolume, @pulseCharacter, @pulseDeficit, @hypotension, @hypotensionCause, @posturalHypotension, @posturalDrop,
    @newOnsetHypertension, @knownHypertension, @onAntihypertensives, @antihypertensiveMedication, @lastAntihypertensiveDose, @headache, @visualDisturbances, @epigastricPain, @hyperreflexia, @edema,
    @emergencyProtocolActivated, @antihypertensiveGiven, @antihypertensiveGivenTime, @magnesiumSulfateGiven, @ivFluidsGiven, @positionChanged,
    @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash);";

        protected override string GetUpdateSql() => @"
UPDATE Tbl_BP
SET partographID = @partographId,
    time = @time,
    handler = @handler,
    notes = @notes,
    systolic = @systolic,
    diastolic = @diastolic,
    pulse = @pulse,
    maternalPosition = @maternalPosition,
    cuffSize = @cuffSize,
    repeatMeasurement = @repeatMeasurement,
    irregularPulse = @irregularPulse,
    clinicalAlert = @clinicalAlert,
    bpCategory = @bpCategory,
    severeHypertension = @severeHypertension,
    preeclampsiaRange = @preeclampsiaRange,
    firstElevatedBPTime = @firstElevatedBPTime,
    consecutiveElevatedReadings = @consecutiveElevatedReadings,
    secondSystolic = @secondSystolic,
    secondDiastolic = @secondDiastolic,
    secondReadingTime = @secondReadingTime,
    thirdSystolic = @thirdSystolic,
    thirdDiastolic = @thirdDiastolic,
    thirdReadingTime = @thirdReadingTime,
    pulseRhythm = @pulseRhythm,
    pulseVolume = @pulseVolume,
    pulseCharacter = @pulseCharacter,
    pulseDeficit = @pulseDeficit,
    hypotension = @hypotension,
    hypotensionCause = @hypotensionCause,
    posturalHypotension = @posturalHypotension,
    posturalDrop = @posturalDrop,
    newOnsetHypertension = @newOnsetHypertension,
    knownHypertension = @knownHypertension,
    onAntihypertensives = @onAntihypertensives,
    antihypertensiveMedication = @antihypertensiveMedication,
    lastAntihypertensiveDose = @lastAntihypertensiveDose,
    headache = @headache,
    visualDisturbances = @visualDisturbances,
    epigastricPain = @epigastricPain,
    hyperreflexia = @hyperreflexia,
    edema = @edema,
    emergencyProtocolActivated = @emergencyProtocolActivated,
    antihypertensiveGiven = @antihypertensiveGiven,
    antihypertensiveGivenTime = @antihypertensiveGivenTime,
    magnesiumSulfateGiven = @magnesiumSulfateGiven,
    ivFluidsGiven = @ivFluidsGiven,
    positionChanged = @positionChanged,
    updatedtime = @updatedtime,
    deletedtime = @deletedtime,
    deviceid = @deviceid,
    syncstatus = @syncstatus,
    version = @version,
    datahash = @datahash
WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, BP item)
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
            cmd.Parameters.AddWithValue("@systolic", item.Systolic);
            cmd.Parameters.AddWithValue("@diastolic", item.Diastolic);
            cmd.Parameters.AddWithValue("@pulse", item.Pulse);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@maternalPosition", item.MaternalPosition ?? "Sitting");
            cmd.Parameters.AddWithValue("@cuffSize", item.CuffSize ?? "Standard");
            cmd.Parameters.AddWithValue("@repeatMeasurement", item.RepeatMeasurement ? 1 : 0);
            cmd.Parameters.AddWithValue("@irregularPulse", item.IrregularPulse ? 1 : 0);
            cmd.Parameters.AddWithValue("@clinicalAlert", item.ClinicalAlert ?? "");
            cmd.Parameters.AddWithValue("@bpCategory", item.BPCategory ?? "");
            cmd.Parameters.AddWithValue("@severeHypertension", item.SevereHypertension ? 1 : 0);
            cmd.Parameters.AddWithValue("@preeclampsiaRange", item.PreeclampsiaRange ? 1 : 0);
            cmd.Parameters.AddWithValue("@firstElevatedBPTime", item.FirstElevatedBPTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@consecutiveElevatedReadings", item.ConsecutiveElevatedReadings ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@secondSystolic", item.SecondSystolic ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@secondDiastolic", item.SecondDiastolic ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@secondReadingTime", item.SecondReadingTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@thirdSystolic", item.ThirdSystolic ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@thirdDiastolic", item.ThirdDiastolic ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@thirdReadingTime", item.ThirdReadingTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@pulseRhythm", item.PulseRhythm ?? "");
            cmd.Parameters.AddWithValue("@pulseVolume", item.PulseVolume ?? "");
            cmd.Parameters.AddWithValue("@pulseCharacter", item.PulseCharacter ?? "");
            cmd.Parameters.AddWithValue("@pulseDeficit", item.PulseDeficit ? 1 : 0);
            cmd.Parameters.AddWithValue("@hypotension", item.Hypotension ? 1 : 0);
            cmd.Parameters.AddWithValue("@hypotensionCause", item.HypotensionCause ?? "");
            cmd.Parameters.AddWithValue("@posturalHypotension", item.PosturalHypotension ? 1 : 0);
            cmd.Parameters.AddWithValue("@posturalDrop", item.PosturalDrop ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@newOnsetHypertension", item.NewOnsetHypertension ? 1 : 0);
            cmd.Parameters.AddWithValue("@knownHypertension", item.KnownHypertension ? 1 : 0);
            cmd.Parameters.AddWithValue("@onAntihypertensives", item.OnAntihypertensives ? 1 : 0);
            cmd.Parameters.AddWithValue("@antihypertensiveMedication", item.AntihypertensiveMedication ?? "");
            cmd.Parameters.AddWithValue("@lastAntihypertensiveDose", item.LastAntihypertensiveDose?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@headache", item.Headache ? 1 : 0);
            cmd.Parameters.AddWithValue("@visualDisturbances", item.VisualDisturbances ? 1 : 0);
            cmd.Parameters.AddWithValue("@epigastricPain", item.EpigastricPain ? 1 : 0);
            cmd.Parameters.AddWithValue("@hyperreflexia", item.Hyperreflexia ? 1 : 0);
            cmd.Parameters.AddWithValue("@edema", item.Edema ? 1 : 0);
            cmd.Parameters.AddWithValue("@emergencyProtocolActivated", item.EmergencyProtocolActivated ? 1 : 0);
            cmd.Parameters.AddWithValue("@antihypertensiveGiven", item.AntihypertensiveGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@antihypertensiveGivenTime", item.AntihypertensiveGivenTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@magnesiumSulfateGiven", item.MagnesiumSulfateGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@ivFluidsGiven", item.IVFluidsGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@positionChanged", item.PositionChanged ? 1 : 0);

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

        protected override void AddUpdateParameters(SqliteCommand cmd, BP item)
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
            cmd.Parameters.AddWithValue("@systolic", item.Systolic);
            cmd.Parameters.AddWithValue("@diastolic", item.Diastolic);
            cmd.Parameters.AddWithValue("@pulse", item.Pulse);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@maternalPosition", item.MaternalPosition ?? "Sitting");
            cmd.Parameters.AddWithValue("@cuffSize", item.CuffSize ?? "Standard");
            cmd.Parameters.AddWithValue("@repeatMeasurement", item.RepeatMeasurement ? 1 : 0);
            cmd.Parameters.AddWithValue("@irregularPulse", item.IrregularPulse ? 1 : 0);
            cmd.Parameters.AddWithValue("@clinicalAlert", item.ClinicalAlert ?? "");
            cmd.Parameters.AddWithValue("@bpCategory", item.BPCategory ?? "");
            cmd.Parameters.AddWithValue("@severeHypertension", item.SevereHypertension ? 1 : 0);
            cmd.Parameters.AddWithValue("@preeclampsiaRange", item.PreeclampsiaRange ? 1 : 0);
            cmd.Parameters.AddWithValue("@firstElevatedBPTime", item.FirstElevatedBPTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@consecutiveElevatedReadings", item.ConsecutiveElevatedReadings ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@secondSystolic", item.SecondSystolic ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@secondDiastolic", item.SecondDiastolic ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@secondReadingTime", item.SecondReadingTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@thirdSystolic", item.ThirdSystolic ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@thirdDiastolic", item.ThirdDiastolic ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@thirdReadingTime", item.ThirdReadingTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@pulseRhythm", item.PulseRhythm ?? "");
            cmd.Parameters.AddWithValue("@pulseVolume", item.PulseVolume ?? "");
            cmd.Parameters.AddWithValue("@pulseCharacter", item.PulseCharacter ?? "");
            cmd.Parameters.AddWithValue("@pulseDeficit", item.PulseDeficit ? 1 : 0);
            cmd.Parameters.AddWithValue("@hypotension", item.Hypotension ? 1 : 0);
            cmd.Parameters.AddWithValue("@hypotensionCause", item.HypotensionCause ?? "");
            cmd.Parameters.AddWithValue("@posturalHypotension", item.PosturalHypotension ? 1 : 0);
            cmd.Parameters.AddWithValue("@posturalDrop", item.PosturalDrop ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@newOnsetHypertension", item.NewOnsetHypertension ? 1 : 0);
            cmd.Parameters.AddWithValue("@knownHypertension", item.KnownHypertension ? 1 : 0);
            cmd.Parameters.AddWithValue("@onAntihypertensives", item.OnAntihypertensives ? 1 : 0);
            cmd.Parameters.AddWithValue("@antihypertensiveMedication", item.AntihypertensiveMedication ?? "");
            cmd.Parameters.AddWithValue("@lastAntihypertensiveDose", item.LastAntihypertensiveDose?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@headache", item.Headache ? 1 : 0);
            cmd.Parameters.AddWithValue("@visualDisturbances", item.VisualDisturbances ? 1 : 0);
            cmd.Parameters.AddWithValue("@epigastricPain", item.EpigastricPain ? 1 : 0);
            cmd.Parameters.AddWithValue("@hyperreflexia", item.Hyperreflexia ? 1 : 0);
            cmd.Parameters.AddWithValue("@edema", item.Edema ? 1 : 0);
            cmd.Parameters.AddWithValue("@emergencyProtocolActivated", item.EmergencyProtocolActivated ? 1 : 0);
            cmd.Parameters.AddWithValue("@antihypertensiveGiven", item.AntihypertensiveGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@antihypertensiveGivenTime", item.AntihypertensiveGivenTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@magnesiumSulfateGiven", item.MagnesiumSulfateGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@ivFluidsGiven", item.IVFluidsGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@positionChanged", item.PositionChanged ? 1 : 0);

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
