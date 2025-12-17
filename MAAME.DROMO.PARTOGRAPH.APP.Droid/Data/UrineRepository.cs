using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Urine Repository - WHO 2020 Enhanced
    public class UrineRepository : BasePartographRepository<Urine>
    {
        protected override string TableName => "Tbl_Urine";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_Urine (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                outputMl INTEGER DEFAULT 0,
                color TEXT DEFAULT 'Yellow',
                protein TEXT DEFAULT 'Nil',
                ketones TEXT DEFAULT 'Nil',
                glucose TEXT DEFAULT 'Nil',
                specificGravity TEXT DEFAULT '',
                voidingMethod TEXT DEFAULT 'Spontaneous',
                bladderPalpable INTEGER DEFAULT 0,
                lastVoided TEXT,
                clinicalAlert TEXT DEFAULT '',
                voidingTime TEXT,
                timeSinceLastVoidMinutes INTEGER,
                cumulativeOutputMl INTEGER,
                hourlyOutputRate REAL,
                oliguria INTEGER DEFAULT 0,
                anuria INTEGER DEFAULT 0,
                consecutiveOliguriaHours INTEGER,
                clarity TEXT DEFAULT '',
                hematuria INTEGER DEFAULT 0,
                concentrated INTEGER DEFAULT 0,
                dilute INTEGER DEFAULT 0,
                odor TEXT DEFAULT '',
                bloodDipstick TEXT DEFAULT 'Nil',
                leukocytesDipstick TEXT DEFAULT 'Nil',
                nitritesDipstick TEXT DEFAULT 'Nil',
                phLevel REAL,
                bladderFullness INTEGER DEFAULT 0,
                bladderFullnessLevel TEXT DEFAULT '',
                difficultVoiding INTEGER DEFAULT 0,
                urinaryRetention INTEGER DEFAULT 0,
                catheterizationIndicated INTEGER DEFAULT 0,
                lastCatheterizationTime TEXT,
                catheterType TEXT DEFAULT '',
                proteinuriaNewOnset INTEGER DEFAULT 0,
                proteinuriaWorsening INTEGER DEFAULT 0,
                firstProteinDetectedTime TEXT,
                laboratorySampleSent INTEGER DEFAULT 0,
                proteinCreatinineRatio TEXT DEFAULT '',
                signsOfDehydration INTEGER DEFAULT 0,
                prolongedLabor INTEGER DEFAULT 0,
                increasedKetoneTrend INTEGER DEFAULT 0,
                firstKetoneDetectedTime TEXT,
                totalOralIntakeMl INTEGER,
                totalIVIntakeMl INTEGER,
                fluidBalanceMl INTEGER,
                encourageOralFluids INTEGER DEFAULT 0,
                ivFluidsStarted INTEGER DEFAULT 0,
                catheterizationPerformed INTEGER DEFAULT 0,
                nephrologyConsultRequired INTEGER DEFAULT 0,
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

            CREATE INDEX IF NOT EXISTS idx_urine_sync ON Tbl_Urine(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_urine_server_version ON Tbl_Urine(serverversion);

            DROP TRIGGER IF EXISTS trg_urine_insert;
            CREATE TRIGGER trg_urine_insert
            AFTER INSERT ON Tbl_Urine
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_Urine
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_urine_update;
            CREATE TRIGGER trg_urine_update
            AFTER UPDATE ON Tbl_Urine
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_Urine
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";
        
        //    -- Add WHO 2020 columns to existing tables
        //    ALTER TABLE Tbl_Urine ADD COLUMN outputMl INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN color TEXT DEFAULT 'Yellow';
        //ALTER TABLE Tbl_Urine ADD COLUMN ketones TEXT DEFAULT 'Nil';
        //ALTER TABLE Tbl_Urine ADD COLUMN glucose TEXT DEFAULT 'Nil';
        //ALTER TABLE Tbl_Urine ADD COLUMN specificGravity TEXT DEFAULT '';
        //ALTER TABLE Tbl_Urine ADD COLUMN voidingMethod TEXT DEFAULT 'Spontaneous';
        //ALTER TABLE Tbl_Urine ADD COLUMN bladderPalpable INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN lastVoided TEXT;
        //    ALTER TABLE Tbl_Urine ADD COLUMN clinicalAlert TEXT DEFAULT '';
        //ALTER TABLE Tbl_Urine ADD COLUMN voidingTime TEXT;
        //    ALTER TABLE Tbl_Urine ADD COLUMN timeSinceLastVoidMinutes INTEGER;
        //    ALTER TABLE Tbl_Urine ADD COLUMN cumulativeOutputMl INTEGER;
        //    ALTER TABLE Tbl_Urine ADD COLUMN hourlyOutputRate REAL;
        //    ALTER TABLE Tbl_Urine ADD COLUMN oliguria INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN anuria INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN consecutiveOliguriaHours INTEGER;
        //    ALTER TABLE Tbl_Urine ADD COLUMN clarity TEXT DEFAULT '';
        //ALTER TABLE Tbl_Urine ADD COLUMN hematuria INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN concentrated INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN dilute INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN odor TEXT DEFAULT '';
        //ALTER TABLE Tbl_Urine ADD COLUMN bloodDipstick TEXT DEFAULT 'Nil';
        //ALTER TABLE Tbl_Urine ADD COLUMN leukocytesDipstick TEXT DEFAULT 'Nil';
        //ALTER TABLE Tbl_Urine ADD COLUMN nitritesDipstick TEXT DEFAULT 'Nil';
        //ALTER TABLE Tbl_Urine ADD COLUMN phLevel REAL;
        //    ALTER TABLE Tbl_Urine ADD COLUMN bladderFullness INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN bladderFullnessLevel TEXT DEFAULT '';
        //ALTER TABLE Tbl_Urine ADD COLUMN difficultVoiding INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN urinaryRetention INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN catheterizationIndicated INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN lastCatheterizationTime TEXT;
        //    ALTER TABLE Tbl_Urine ADD COLUMN catheterType TEXT DEFAULT '';
        //ALTER TABLE Tbl_Urine ADD COLUMN proteinuriaNewOnset INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN proteinuriaWorsening INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN firstProteinDetectedTime TEXT;
        //    ALTER TABLE Tbl_Urine ADD COLUMN laboratorySampleSent INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN proteinCreatinineRatio TEXT DEFAULT '';
        //ALTER TABLE Tbl_Urine ADD COLUMN signsOfDehydration INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN prolongedLabor INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN increasedKetoneTrend INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN firstKetoneDetectedTime TEXT;
        //    ALTER TABLE Tbl_Urine ADD COLUMN totalOralIntakeMl INTEGER;
        //    ALTER TABLE Tbl_Urine ADD COLUMN totalIVIntakeMl INTEGER;
        //    ALTER TABLE Tbl_Urine ADD COLUMN fluidBalanceMl INTEGER;
        //    ALTER TABLE Tbl_Urine ADD COLUMN encourageOralFluids INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN ivFluidsStarted INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN catheterizationPerformed INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_Urine ADD COLUMN nephrologyConsultRequired INTEGER DEFAULT 0;

        public UrineRepository(ILogger<UrineRepository> logger) : base(logger) { }

        protected override Urine MapFromReader(SqliteDataReader reader)
        {
            var item = new Urine
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = GetStringOrDefault(reader, "staffname", ""),
                Notes = GetStringOrDefault(reader, "notes", ""),
                Protein = GetStringOrDefault(reader, "protein", "Nil"),
                Ketones = GetStringOrDefault(reader, "ketones", "Nil"),
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
                item.OutputMl = GetIntOrDefault(reader, "outputMl", 0);
                item.Color = GetStringOrDefault(reader, "color", "Yellow");
                item.Glucose = GetStringOrDefault(reader, "glucose", "Nil");
                item.SpecificGravity = GetStringOrDefault(reader, "specificGravity", "");
                item.VoidingMethod = GetStringOrDefault(reader, "voidingMethod", "Spontaneous");
                item.BladderPalpable = GetBoolFromInt(reader, "bladderPalpable");
                item.LastVoided = GetNullableDateTime(reader, "lastVoided");
                item.ClinicalAlert = GetStringOrDefault(reader, "clinicalAlert", "");
                item.VoidingTime = GetNullableDateTime(reader, "voidingTime");
                item.TimeSinceLastVoidMinutes = GetNullableInt(reader, "timeSinceLastVoidMinutes");
                item.CumulativeOutputMl = GetNullableInt(reader, "cumulativeOutputMl");
                item.HourlyOutputRate = GetNullableDecimal(reader, "hourlyOutputRate");
                item.Oliguria = GetBoolFromInt(reader, "oliguria");
                item.Anuria = GetBoolFromInt(reader, "anuria");
                item.ConsecutiveOliguriaHours = GetNullableInt(reader, "consecutiveOliguriaHours");
                item.Clarity = GetStringOrDefault(reader, "clarity", "");
                item.Hematuria = GetBoolFromInt(reader, "hematuria");
                item.Concentrated = GetBoolFromInt(reader, "concentrated");
                item.Dilute = GetBoolFromInt(reader, "dilute");
                item.Odor = GetStringOrDefault(reader, "odor", "");
                item.BloodDipstick = GetStringOrDefault(reader, "bloodDipstick", "Nil");
                item.LeukocytesDipstick = GetStringOrDefault(reader, "leukocytesDipstick", "Nil");
                item.NitritesDipstick = GetStringOrDefault(reader, "nitritesDipstick", "Nil");
                item.PHLevel = GetNullableFloat(reader, "phLevel");
                item.BladderFullness = GetBoolFromInt(reader, "bladderFullness");
                item.BladderFullnessLevel = GetStringOrDefault(reader, "bladderFullnessLevel", "");
                item.DifficultVoiding = GetBoolFromInt(reader, "difficultVoiding");
                item.UrinaryRetention = GetBoolFromInt(reader, "urinaryRetention");
                item.CatheterizationIndicated = GetBoolFromInt(reader, "catheterizationIndicated");
                item.LastCatheterizationTime = GetNullableDateTime(reader, "lastCatheterizationTime");
                item.CatheterType = GetStringOrDefault(reader, "catheterType", "");
                item.ProteinuriaNewOnset = GetBoolFromInt(reader, "proteinuriaNewOnset");
                item.ProteinuriaWorsening = GetBoolFromInt(reader, "proteinuriaWorsening");
                item.FirstProteinDetectedTime = GetNullableDateTime(reader, "firstProteinDetectedTime");
                item.LaboratorySampleSent = GetBoolFromInt(reader, "laboratorySampleSent");
                item.ProteinCreatinineRatio = GetStringOrDefault(reader, "proteinCreatinineRatio", "");
                item.SignsOfDehydration = GetBoolFromInt(reader, "signsOfDehydration");
                item.ProlongedLabor = GetBoolFromInt(reader, "prolongedLabor");
                item.IncreasedKetoneTrend = GetBoolFromInt(reader, "increasedKetoneTrend");
                item.FirstKetoneDetectedTime = GetNullableDateTime(reader, "firstKetoneDetectedTime");
                item.TotalOralIntakeMl = GetNullableInt(reader, "totalOralIntakeMl");
                item.TotalIVIntakeMl = GetNullableInt(reader, "totalIVIntakeMl");
                item.FluidBalanceMl = GetNullableInt(reader, "fluidBalanceMl");
                item.EncourageOralFluids = GetBoolFromInt(reader, "encourageOralFluids");
                item.IVFluidsStarted = GetBoolFromInt(reader, "ivFluidsStarted");
                item.CatheterizationPerformed = GetBoolFromInt(reader, "catheterizationPerformed");
                item.NephrologyConsultRequired = GetBoolFromInt(reader, "nephrologyConsultRequired");
            }
            catch { /* Columns don't exist yet in old databases */ }

            return item;
        }

        protected override string GetInsertSql() => @"
INSERT INTO Tbl_Urine (ID, partographID, time, handler, notes, outputMl, color, protein, ketones, glucose, specificGravity, voidingMethod, bladderPalpable, lastVoided, clinicalAlert,
    voidingTime, timeSinceLastVoidMinutes, cumulativeOutputMl, hourlyOutputRate, oliguria, anuria, consecutiveOliguriaHours,
    clarity, hematuria, concentrated, dilute, odor, bloodDipstick, leukocytesDipstick, nitritesDipstick, phLevel,
    bladderFullness, bladderFullnessLevel, difficultVoiding, urinaryRetention, catheterizationIndicated, lastCatheterizationTime, catheterType,
    proteinuriaNewOnset, proteinuriaWorsening, firstProteinDetectedTime, laboratorySampleSent, proteinCreatinineRatio,
    signsOfDehydration, prolongedLabor, increasedKetoneTrend, firstKetoneDetectedTime, totalOralIntakeMl, totalIVIntakeMl, fluidBalanceMl,
    encourageOralFluids, ivFluidsStarted, catheterizationPerformed, nephrologyConsultRequired,
    createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash)
VALUES (@id, @partographId, @time, @handler, @notes, @outputMl, @color, @protein, @ketones, @glucose, @specificGravity, @voidingMethod, @bladderPalpable, @lastVoided, @clinicalAlert,
    @voidingTime, @timeSinceLastVoidMinutes, @cumulativeOutputMl, @hourlyOutputRate, @oliguria, @anuria, @consecutiveOliguriaHours,
    @clarity, @hematuria, @concentrated, @dilute, @odor, @bloodDipstick, @leukocytesDipstick, @nitritesDipstick, @phLevel,
    @bladderFullness, @bladderFullnessLevel, @difficultVoiding, @urinaryRetention, @catheterizationIndicated, @lastCatheterizationTime, @catheterType,
    @proteinuriaNewOnset, @proteinuriaWorsening, @firstProteinDetectedTime, @laboratorySampleSent, @proteinCreatinineRatio,
    @signsOfDehydration, @prolongedLabor, @increasedKetoneTrend, @firstKetoneDetectedTime, @totalOralIntakeMl, @totalIVIntakeMl, @fluidBalanceMl,
    @encourageOralFluids, @ivFluidsStarted, @catheterizationPerformed, @nephrologyConsultRequired,
    @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash);";

        protected override string GetUpdateSql() => @"
UPDATE Tbl_Urine
SET partographID = @partographId,
    time = @time,
    handler = @handler,
    notes = @notes,
    outputMl = @outputMl,
    color = @color,
    protein = @protein,
    ketones = @ketones,
    glucose = @glucose,
    specificGravity = @specificGravity,
    voidingMethod = @voidingMethod,
    bladderPalpable = @bladderPalpable,
    lastVoided = @lastVoided,
    clinicalAlert = @clinicalAlert,
    voidingTime = @voidingTime,
    timeSinceLastVoidMinutes = @timeSinceLastVoidMinutes,
    cumulativeOutputMl = @cumulativeOutputMl,
    hourlyOutputRate = @hourlyOutputRate,
    oliguria = @oliguria,
    anuria = @anuria,
    consecutiveOliguriaHours = @consecutiveOliguriaHours,
    clarity = @clarity,
    hematuria = @hematuria,
    concentrated = @concentrated,
    dilute = @dilute,
    odor = @odor,
    bloodDipstick = @bloodDipstick,
    leukocytesDipstick = @leukocytesDipstick,
    nitritesDipstick = @nitritesDipstick,
    phLevel = @phLevel,
    bladderFullness = @bladderFullness,
    bladderFullnessLevel = @bladderFullnessLevel,
    difficultVoiding = @difficultVoiding,
    urinaryRetention = @urinaryRetention,
    catheterizationIndicated = @catheterizationIndicated,
    lastCatheterizationTime = @lastCatheterizationTime,
    catheterType = @catheterType,
    proteinuriaNewOnset = @proteinuriaNewOnset,
    proteinuriaWorsening = @proteinuriaWorsening,
    firstProteinDetectedTime = @firstProteinDetectedTime,
    laboratorySampleSent = @laboratorySampleSent,
    proteinCreatinineRatio = @proteinCreatinineRatio,
    signsOfDehydration = @signsOfDehydration,
    prolongedLabor = @prolongedLabor,
    increasedKetoneTrend = @increasedKetoneTrend,
    firstKetoneDetectedTime = @firstKetoneDetectedTime,
    totalOralIntakeMl = @totalOralIntakeMl,
    totalIVIntakeMl = @totalIVIntakeMl,
    fluidBalanceMl = @fluidBalanceMl,
    encourageOralFluids = @encourageOralFluids,
    ivFluidsStarted = @ivFluidsStarted,
    catheterizationPerformed = @catheterizationPerformed,
    nephrologyConsultRequired = @nephrologyConsultRequired,
    updatedtime = @updatedtime,
    deletedtime = @deletedtime,
    deviceid = @deviceid,
    syncstatus = @syncstatus,
    version = @version,
    datahash = @datahash
WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, Urine item)
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
            cmd.Parameters.AddWithValue("@protein", item.Protein ?? "Nil");
            cmd.Parameters.AddWithValue("@ketones", item.Ketones ?? "Nil");

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@outputMl", item.OutputMl);
            cmd.Parameters.AddWithValue("@color", item.Color ?? "Yellow");
            cmd.Parameters.AddWithValue("@glucose", item.Glucose ?? "Nil");
            cmd.Parameters.AddWithValue("@specificGravity", item.SpecificGravity ?? "");
            cmd.Parameters.AddWithValue("@voidingMethod", item.VoidingMethod ?? "Spontaneous");
            cmd.Parameters.AddWithValue("@bladderPalpable", item.BladderPalpable ? 1 : 0);
            cmd.Parameters.AddWithValue("@lastVoided", item.LastVoided?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clinicalAlert", item.ClinicalAlert ?? "");
            cmd.Parameters.AddWithValue("@voidingTime", item.VoidingTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@timeSinceLastVoidMinutes", item.TimeSinceLastVoidMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@cumulativeOutputMl", item.CumulativeOutputMl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@hourlyOutputRate", item.HourlyOutputRate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@oliguria", item.Oliguria ? 1 : 0);
            cmd.Parameters.AddWithValue("@anuria", item.Anuria ? 1 : 0);
            cmd.Parameters.AddWithValue("@consecutiveOliguriaHours", item.ConsecutiveOliguriaHours ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clarity", item.Clarity ?? "");
            cmd.Parameters.AddWithValue("@hematuria", item.Hematuria ? 1 : 0);
            cmd.Parameters.AddWithValue("@concentrated", item.Concentrated ? 1 : 0);
            cmd.Parameters.AddWithValue("@dilute", item.Dilute ? 1 : 0);
            cmd.Parameters.AddWithValue("@odor", item.Odor ?? "");
            cmd.Parameters.AddWithValue("@bloodDipstick", item.BloodDipstick ?? "Nil");
            cmd.Parameters.AddWithValue("@leukocytesDipstick", item.LeukocytesDipstick ?? "Nil");
            cmd.Parameters.AddWithValue("@nitritesDipstick", item.NitritesDipstick ?? "Nil");
            cmd.Parameters.AddWithValue("@phLevel", item.PHLevel ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@bladderFullness", item.BladderFullness ? 1 : 0);
            cmd.Parameters.AddWithValue("@bladderFullnessLevel", item.BladderFullnessLevel ?? "");
            cmd.Parameters.AddWithValue("@difficultVoiding", item.DifficultVoiding ? 1 : 0);
            cmd.Parameters.AddWithValue("@urinaryRetention", item.UrinaryRetention ? 1 : 0);
            cmd.Parameters.AddWithValue("@catheterizationIndicated", item.CatheterizationIndicated ? 1 : 0);
            cmd.Parameters.AddWithValue("@lastCatheterizationTime", item.LastCatheterizationTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@catheterType", item.CatheterType ?? "");
            cmd.Parameters.AddWithValue("@proteinuriaNewOnset", item.ProteinuriaNewOnset ? 1 : 0);
            cmd.Parameters.AddWithValue("@proteinuriaWorsening", item.ProteinuriaWorsening ? 1 : 0);
            cmd.Parameters.AddWithValue("@firstProteinDetectedTime", item.FirstProteinDetectedTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@laboratorySampleSent", item.LaboratorySampleSent ? 1 : 0);
            cmd.Parameters.AddWithValue("@proteinCreatinineRatio", item.ProteinCreatinineRatio ?? "");
            cmd.Parameters.AddWithValue("@signsOfDehydration", item.SignsOfDehydration ? 1 : 0);
            cmd.Parameters.AddWithValue("@prolongedLabor", item.ProlongedLabor ? 1 : 0);
            cmd.Parameters.AddWithValue("@increasedKetoneTrend", item.IncreasedKetoneTrend ? 1 : 0);
            cmd.Parameters.AddWithValue("@firstKetoneDetectedTime", item.FirstKetoneDetectedTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@totalOralIntakeMl", item.TotalOralIntakeMl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@totalIVIntakeMl", item.TotalIVIntakeMl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@fluidBalanceMl", item.FluidBalanceMl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@encourageOralFluids", item.EncourageOralFluids ? 1 : 0);
            cmd.Parameters.AddWithValue("@ivFluidsStarted", item.IVFluidsStarted ? 1 : 0);
            cmd.Parameters.AddWithValue("@catheterizationPerformed", item.CatheterizationPerformed ? 1 : 0);
            cmd.Parameters.AddWithValue("@nephrologyConsultRequired", item.NephrologyConsultRequired ? 1 : 0);

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

        protected override void AddUpdateParameters(SqliteCommand cmd, Urine item)
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
            cmd.Parameters.AddWithValue("@protein", item.Protein ?? "Nil");
            cmd.Parameters.AddWithValue("@ketones", item.Ketones ?? "Nil");

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@outputMl", item.OutputMl);
            cmd.Parameters.AddWithValue("@color", item.Color ?? "Yellow");
            cmd.Parameters.AddWithValue("@glucose", item.Glucose ?? "Nil");
            cmd.Parameters.AddWithValue("@specificGravity", item.SpecificGravity ?? "");
            cmd.Parameters.AddWithValue("@voidingMethod", item.VoidingMethod ?? "Spontaneous");
            cmd.Parameters.AddWithValue("@bladderPalpable", item.BladderPalpable ? 1 : 0);
            cmd.Parameters.AddWithValue("@lastVoided", item.LastVoided?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clinicalAlert", item.ClinicalAlert ?? "");
            cmd.Parameters.AddWithValue("@voidingTime", item.VoidingTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@timeSinceLastVoidMinutes", item.TimeSinceLastVoidMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@cumulativeOutputMl", item.CumulativeOutputMl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@hourlyOutputRate", item.HourlyOutputRate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@oliguria", item.Oliguria ? 1 : 0);
            cmd.Parameters.AddWithValue("@anuria", item.Anuria ? 1 : 0);
            cmd.Parameters.AddWithValue("@consecutiveOliguriaHours", item.ConsecutiveOliguriaHours ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clarity", item.Clarity ?? "");
            cmd.Parameters.AddWithValue("@hematuria", item.Hematuria ? 1 : 0);
            cmd.Parameters.AddWithValue("@concentrated", item.Concentrated ? 1 : 0);
            cmd.Parameters.AddWithValue("@dilute", item.Dilute ? 1 : 0);
            cmd.Parameters.AddWithValue("@odor", item.Odor ?? "");
            cmd.Parameters.AddWithValue("@bloodDipstick", item.BloodDipstick ?? "Nil");
            cmd.Parameters.AddWithValue("@leukocytesDipstick", item.LeukocytesDipstick ?? "Nil");
            cmd.Parameters.AddWithValue("@nitritesDipstick", item.NitritesDipstick ?? "Nil");
            cmd.Parameters.AddWithValue("@phLevel", item.PHLevel ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@bladderFullness", item.BladderFullness ? 1 : 0);
            cmd.Parameters.AddWithValue("@bladderFullnessLevel", item.BladderFullnessLevel ?? "");
            cmd.Parameters.AddWithValue("@difficultVoiding", item.DifficultVoiding ? 1 : 0);
            cmd.Parameters.AddWithValue("@urinaryRetention", item.UrinaryRetention ? 1 : 0);
            cmd.Parameters.AddWithValue("@catheterizationIndicated", item.CatheterizationIndicated ? 1 : 0);
            cmd.Parameters.AddWithValue("@lastCatheterizationTime", item.LastCatheterizationTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@catheterType", item.CatheterType ?? "");
            cmd.Parameters.AddWithValue("@proteinuriaNewOnset", item.ProteinuriaNewOnset ? 1 : 0);
            cmd.Parameters.AddWithValue("@proteinuriaWorsening", item.ProteinuriaWorsening ? 1 : 0);
            cmd.Parameters.AddWithValue("@firstProteinDetectedTime", item.FirstProteinDetectedTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@laboratorySampleSent", item.LaboratorySampleSent ? 1 : 0);
            cmd.Parameters.AddWithValue("@proteinCreatinineRatio", item.ProteinCreatinineRatio ?? "");
            cmd.Parameters.AddWithValue("@signsOfDehydration", item.SignsOfDehydration ? 1 : 0);
            cmd.Parameters.AddWithValue("@prolongedLabor", item.ProlongedLabor ? 1 : 0);
            cmd.Parameters.AddWithValue("@increasedKetoneTrend", item.IncreasedKetoneTrend ? 1 : 0);
            cmd.Parameters.AddWithValue("@firstKetoneDetectedTime", item.FirstKetoneDetectedTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@totalOralIntakeMl", item.TotalOralIntakeMl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@totalIVIntakeMl", item.TotalIVIntakeMl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@fluidBalanceMl", item.FluidBalanceMl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@encourageOralFluids", item.EncourageOralFluids ? 1 : 0);
            cmd.Parameters.AddWithValue("@ivFluidsStarted", item.IVFluidsStarted ? 1 : 0);
            cmd.Parameters.AddWithValue("@catheterizationPerformed", item.CatheterizationPerformed ? 1 : 0);
            cmd.Parameters.AddWithValue("@nephrologyConsultRequired", item.NephrologyConsultRequired ? 1 : 0);

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

        private float? GetNullableFloat(SqliteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetFloat(ordinal);
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
