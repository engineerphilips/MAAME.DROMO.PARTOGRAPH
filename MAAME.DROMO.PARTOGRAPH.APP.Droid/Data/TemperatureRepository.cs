using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Temperature Repository - WHO 2020 Enhanced
    public class TemperatureRepository : BasePartographRepository<Temperature>
    {
        protected override string TableName => "Tbl_Temperature";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_Temperature (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                temperatureCelsius REAL DEFAULT 0.0,
                measurementSite TEXT DEFAULT 'Oral',
                feverDurationHours INTEGER,
                chillsPresent INTEGER DEFAULT 0,
                associatedSymptoms TEXT DEFAULT '',
                repeatedMeasurement INTEGER DEFAULT 0,
                clinicalAlert TEXT DEFAULT '',
                feverCategory TEXT DEFAULT '',
                intrapartumFever INTEGER DEFAULT 0,
                feverOnsetTime TEXT,
                peakTemperature REAL,
                peakTemperatureTime TEXT,
                secondTemperature REAL,
                secondReadingTime TEXT,
                thirdTemperature REAL,
                thirdReadingTime TEXT,
                choriamnionitisRisk INTEGER DEFAULT 0,
                prolongedRupture INTEGER DEFAULT 0,
                hoursSinceRupture INTEGER,
                maternalTachycardia INTEGER DEFAULT 0,
                fetalTachycardia INTEGER DEFAULT 0,
                uterineTenderness INTEGER DEFAULT 0,
                offensiveLiquor INTEGER DEFAULT 0,
                rigorPresent INTEGER DEFAULT 0,
                sweating INTEGER DEFAULT 0,
                headache INTEGER DEFAULT 0,
                myalgiaArthralgia INTEGER DEFAULT 0,
                sepsisScreeningDone INTEGER DEFAULT 0,
                sepsisScreeningTime TEXT,
                sepsisRiskLevel TEXT DEFAULT '',
                qsofaPositive INTEGER DEFAULT 0,
                qsofaScore INTEGER,
                antipyreticsGiven INTEGER DEFAULT 0,
                antipyreticType TEXT DEFAULT '',
                antipyreticGivenTime TEXT,
                culturesObtained INTEGER DEFAULT 0,
                antibioticsStarted INTEGER DEFAULT 0,
                antibioticsStartTime TEXT,
                ivFluidsGiven INTEGER DEFAULT 0,
                coolingMeasures INTEGER DEFAULT 0,
                increasedMonitoring INTEGER DEFAULT 0,
                monitoringIntervalMinutes INTEGER,
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

            CREATE INDEX IF NOT EXISTS idx_temperature_sync ON Tbl_Temperature(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_temperature_server_version ON Tbl_Temperature(serverversion);

            -- Add WHO 2020 columns to existing tables
            ALTER TABLE Tbl_Temperature ADD COLUMN temperatureCelsius REAL DEFAULT 0.0;
            ALTER TABLE Tbl_Temperature ADD COLUMN measurementSite TEXT DEFAULT 'Oral';
            ALTER TABLE Tbl_Temperature ADD COLUMN feverDurationHours INTEGER;
            ALTER TABLE Tbl_Temperature ADD COLUMN chillsPresent INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN associatedSymptoms TEXT DEFAULT '';
            ALTER TABLE Tbl_Temperature ADD COLUMN repeatedMeasurement INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN clinicalAlert TEXT DEFAULT '';
            ALTER TABLE Tbl_Temperature ADD COLUMN feverCategory TEXT DEFAULT '';
            ALTER TABLE Tbl_Temperature ADD COLUMN intrapartumFever INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN feverOnsetTime TEXT;
            ALTER TABLE Tbl_Temperature ADD COLUMN peakTemperature REAL;
            ALTER TABLE Tbl_Temperature ADD COLUMN peakTemperatureTime TEXT;
            ALTER TABLE Tbl_Temperature ADD COLUMN secondTemperature REAL;
            ALTER TABLE Tbl_Temperature ADD COLUMN secondReadingTime TEXT;
            ALTER TABLE Tbl_Temperature ADD COLUMN thirdTemperature REAL;
            ALTER TABLE Tbl_Temperature ADD COLUMN thirdReadingTime TEXT;
            ALTER TABLE Tbl_Temperature ADD COLUMN choriamnionitisRisk INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN prolongedRupture INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN hoursSinceRupture INTEGER;
            ALTER TABLE Tbl_Temperature ADD COLUMN maternalTachycardia INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN fetalTachycardia INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN uterineTenderness INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN offensiveLiquor INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN rigorPresent INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN sweating INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN headache INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN myalgiaArthralgia INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN sepsisScreeningDone INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN sepsisScreeningTime TEXT;
            ALTER TABLE Tbl_Temperature ADD COLUMN sepsisRiskLevel TEXT DEFAULT '';
            ALTER TABLE Tbl_Temperature ADD COLUMN qsofaPositive INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN qsofaScore INTEGER;
            ALTER TABLE Tbl_Temperature ADD COLUMN antipyreticsGiven INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN antipyreticType TEXT DEFAULT '';
            ALTER TABLE Tbl_Temperature ADD COLUMN antipyreticGivenTime TEXT;
            ALTER TABLE Tbl_Temperature ADD COLUMN culturesObtained INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN antibioticsStarted INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN antibioticsStartTime TEXT;
            ALTER TABLE Tbl_Temperature ADD COLUMN ivFluidsGiven INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN coolingMeasures INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN increasedMonitoring INTEGER DEFAULT 0;
            ALTER TABLE Tbl_Temperature ADD COLUMN monitoringIntervalMinutes INTEGER;

            DROP TRIGGER IF EXISTS trg_temperature_insert;
            CREATE TRIGGER trg_temperature_insert
            AFTER INSERT ON Tbl_Temperature
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_Temperature
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_temperature_update;
            CREATE TRIGGER trg_temperature_update
            AFTER UPDATE ON Tbl_Temperature
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_Temperature
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";

        public TemperatureRepository(ILogger<TemperatureRepository> logger) : base(logger) { }

        protected override Temperature MapFromReader(SqliteDataReader reader)
        {
            var item = new Temperature
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = GetStringOrDefault(reader, "handler", ""),
                Notes = GetStringOrDefault(reader, "notes", ""),
                TemperatureCelsius = GetFloatOrDefault(reader, "temperatureCelsius", 0.0f),
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
                item.MeasurementSite = GetStringOrDefault(reader, "measurementSite", "Oral");
                item.FeverDurationHours = GetNullableInt(reader, "feverDurationHours");
                item.ChillsPresent = GetBoolFromInt(reader, "chillsPresent");
                item.AssociatedSymptoms = GetStringOrDefault(reader, "associatedSymptoms", "");
                item.RepeatedMeasurement = GetBoolFromInt(reader, "repeatedMeasurement");
                item.ClinicalAlert = GetStringOrDefault(reader, "clinicalAlert", "");
                item.FeverCategory = GetStringOrDefault(reader, "feverCategory", "");
                item.IntrapartumFever = GetBoolFromInt(reader, "intrapartumFever");
                item.FeverOnsetTime = GetNullableDateTime(reader, "feverOnsetTime");
                item.PeakTemperature = GetNullableFloat(reader, "peakTemperature");
                item.PeakTemperatureTime = GetNullableDateTime(reader, "peakTemperatureTime");
                item.SecondTemperature = GetNullableFloat(reader, "secondTemperature");
                item.SecondReadingTime = GetNullableDateTime(reader, "secondReadingTime");
                item.ThirdTemperature = GetNullableFloat(reader, "thirdTemperature");
                item.ThirdReadingTime = GetNullableDateTime(reader, "thirdReadingTime");
                item.ChoriamnionitisRisk = GetBoolFromInt(reader, "choriamnionitisRisk");
                item.ProlongedRupture = GetBoolFromInt(reader, "prolongedRupture");
                item.HoursSinceRupture = GetNullableInt(reader, "hoursSinceRupture");
                item.MaternalTachycardia = GetBoolFromInt(reader, "maternalTachycardia");
                item.FetalTachycardia = GetBoolFromInt(reader, "fetalTachycardia");
                item.UterineTenderness = GetBoolFromInt(reader, "uterineTenderness");
                item.OffensiveLiquor = GetBoolFromInt(reader, "offensiveLiquor");
                item.RigorPresent = GetBoolFromInt(reader, "rigorPresent");
                item.Sweating = GetBoolFromInt(reader, "sweating");
                item.Headache = GetBoolFromInt(reader, "headache");
                item.MyalgiaArthralgia = GetBoolFromInt(reader, "myalgiaArthralgia");
                item.SepsisScreeningDone = GetBoolFromInt(reader, "sepsisScreeningDone");
                item.SepsisScreeningTime = GetNullableDateTime(reader, "sepsisScreeningTime");
                item.SepsisRiskLevel = GetStringOrDefault(reader, "sepsisRiskLevel", "");
                item.QSOFAPositive = GetBoolFromInt(reader, "qsofaPositive");
                item.QSOFAScore = GetNullableInt(reader, "qsofaScore");
                item.AntipyreticsGiven = GetBoolFromInt(reader, "antipyreticsGiven");
                item.AntipyreticType = GetStringOrDefault(reader, "antipyreticType", "");
                item.AntipyreticGivenTime = GetNullableDateTime(reader, "antipyreticGivenTime");
                item.CulturesObtained = GetBoolFromInt(reader, "culturesObtained");
                item.AntibioticsStarted = GetBoolFromInt(reader, "antibioticsStarted");
                item.AntibioticsStartTime = GetNullableDateTime(reader, "antibioticsStartTime");
                item.IVFluidsGiven = GetBoolFromInt(reader, "ivFluidsGiven");
                item.CoolingMeasures = GetBoolFromInt(reader, "coolingMeasures");
                item.IncreasedMonitoring = GetBoolFromInt(reader, "increasedMonitoring");
                item.MonitoringIntervalMinutes = GetNullableInt(reader, "monitoringIntervalMinutes");
            }
            catch { /* Columns don't exist yet in old databases */ }

            return item;
        }

        protected override string GetInsertSql() => @"
INSERT INTO Tbl_Temperature (ID, partographID, time, handler, notes, temperatureCelsius, measurementSite, feverDurationHours, chillsPresent, associatedSymptoms, repeatedMeasurement, clinicalAlert,
    feverCategory, intrapartumFever, feverOnsetTime, peakTemperature, peakTemperatureTime, secondTemperature, secondReadingTime, thirdTemperature, thirdReadingTime,
    choriamnionitisRisk, prolongedRupture, hoursSinceRupture, maternalTachycardia, fetalTachycardia, uterineTenderness, offensiveLiquor,
    rigorPresent, sweating, headache, myalgiaArthralgia, sepsisScreeningDone, sepsisScreeningTime, sepsisRiskLevel, qsofaPositive, qsofaScore,
    antipyreticsGiven, antipyreticType, antipyreticGivenTime, culturesObtained, antibioticsStarted, antibioticsStartTime, ivFluidsGiven, coolingMeasures, increasedMonitoring, monitoringIntervalMinutes,
    createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash)
VALUES (@id, @partographId, @time, @handler, @notes, @temperatureCelsius, @measurementSite, @feverDurationHours, @chillsPresent, @associatedSymptoms, @repeatedMeasurement, @clinicalAlert,
    @feverCategory, @intrapartumFever, @feverOnsetTime, @peakTemperature, @peakTemperatureTime, @secondTemperature, @secondReadingTime, @thirdTemperature, @thirdReadingTime,
    @choriamnionitisRisk, @prolongedRupture, @hoursSinceRupture, @maternalTachycardia, @fetalTachycardia, @uterineTenderness, @offensiveLiquor,
    @rigorPresent, @sweating, @headache, @myalgiaArthralgia, @sepsisScreeningDone, @sepsisScreeningTime, @sepsisRiskLevel, @qsofaPositive, @qsofaScore,
    @antipyreticsGiven, @antipyreticType, @antipyreticGivenTime, @culturesObtained, @antibioticsStarted, @antibioticsStartTime, @ivFluidsGiven, @coolingMeasures, @increasedMonitoring, @monitoringIntervalMinutes,
    @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash);";

        protected override string GetUpdateSql() => @"
UPDATE Tbl_Temperature
SET partographID = @partographId,
    time = @time,
    handler = @handler,
    notes = @notes,
    temperatureCelsius = @temperatureCelsius,
    measurementSite = @measurementSite,
    feverDurationHours = @feverDurationHours,
    chillsPresent = @chillsPresent,
    associatedSymptoms = @associatedSymptoms,
    repeatedMeasurement = @repeatedMeasurement,
    clinicalAlert = @clinicalAlert,
    feverCategory = @feverCategory,
    intrapartumFever = @intrapartumFever,
    feverOnsetTime = @feverOnsetTime,
    peakTemperature = @peakTemperature,
    peakTemperatureTime = @peakTemperatureTime,
    secondTemperature = @secondTemperature,
    secondReadingTime = @secondReadingTime,
    thirdTemperature = @thirdTemperature,
    thirdReadingTime = @thirdReadingTime,
    choriamnionitisRisk = @choriamnionitisRisk,
    prolongedRupture = @prolongedRupture,
    hoursSinceRupture = @hoursSinceRupture,
    maternalTachycardia = @maternalTachycardia,
    fetalTachycardia = @fetalTachycardia,
    uterineTenderness = @uterineTenderness,
    offensiveLiquor = @offensiveLiquor,
    rigorPresent = @rigorPresent,
    sweating = @sweating,
    headache = @headache,
    myalgiaArthralgia = @myalgiaArthralgia,
    sepsisScreeningDone = @sepsisScreeningDone,
    sepsisScreeningTime = @sepsisScreeningTime,
    sepsisRiskLevel = @sepsisRiskLevel,
    qsofaPositive = @qsofaPositive,
    qsofaScore = @qsofaScore,
    antipyreticsGiven = @antipyreticsGiven,
    antipyreticType = @antipyreticType,
    antipyreticGivenTime = @antipyreticGivenTime,
    culturesObtained = @culturesObtained,
    antibioticsStarted = @antibioticsStarted,
    antibioticsStartTime = @antibioticsStartTime,
    ivFluidsGiven = @ivFluidsGiven,
    coolingMeasures = @coolingMeasures,
    increasedMonitoring = @increasedMonitoring,
    monitoringIntervalMinutes = @monitoringIntervalMinutes,
    updatedtime = @updatedtime,
    deletedtime = @deletedtime,
    deviceid = @deviceid,
    syncstatus = @syncstatus,
    version = @version,
    datahash = @datahash
WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, Temperature item)
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
            cmd.Parameters.AddWithValue("@temperatureCelsius", item.TemperatureCelsius);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@measurementSite", item.MeasurementSite ?? "Oral");
            cmd.Parameters.AddWithValue("@feverDurationHours", item.FeverDurationHours ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@chillsPresent", item.ChillsPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@associatedSymptoms", item.AssociatedSymptoms ?? "");
            cmd.Parameters.AddWithValue("@repeatedMeasurement", item.RepeatedMeasurement ? 1 : 0);
            cmd.Parameters.AddWithValue("@clinicalAlert", item.ClinicalAlert ?? "");
            cmd.Parameters.AddWithValue("@feverCategory", item.FeverCategory ?? "");
            cmd.Parameters.AddWithValue("@intrapartumFever", item.IntrapartumFever ? 1 : 0);
            cmd.Parameters.AddWithValue("@feverOnsetTime", item.FeverOnsetTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@peakTemperature", item.PeakTemperature ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@peakTemperatureTime", item.PeakTemperatureTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@secondTemperature", item.SecondTemperature ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@secondReadingTime", item.SecondReadingTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@thirdTemperature", item.ThirdTemperature ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@thirdReadingTime", item.ThirdReadingTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@choriamnionitisRisk", item.ChoriamnionitisRisk ? 1 : 0);
            cmd.Parameters.AddWithValue("@prolongedRupture", item.ProlongedRupture ? 1 : 0);
            cmd.Parameters.AddWithValue("@hoursSinceRupture", item.HoursSinceRupture ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@maternalTachycardia", item.MaternalTachycardia ? 1 : 0);
            cmd.Parameters.AddWithValue("@fetalTachycardia", item.FetalTachycardia ? 1 : 0);
            cmd.Parameters.AddWithValue("@uterineTenderness", item.UterineTenderness ? 1 : 0);
            cmd.Parameters.AddWithValue("@offensiveLiquor", item.OffensiveLiquor ? 1 : 0);
            cmd.Parameters.AddWithValue("@rigorPresent", item.RigorPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@sweating", item.Sweating ? 1 : 0);
            cmd.Parameters.AddWithValue("@headache", item.Headache ? 1 : 0);
            cmd.Parameters.AddWithValue("@myalgiaArthralgia", item.MyalgiaArthralgia ? 1 : 0);
            cmd.Parameters.AddWithValue("@sepsisScreeningDone", item.SepsisScreeningDone ? 1 : 0);
            cmd.Parameters.AddWithValue("@sepsisScreeningTime", item.SepsisScreeningTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@sepsisRiskLevel", item.SepsisRiskLevel ?? "");
            cmd.Parameters.AddWithValue("@qsofaPositive", item.QSOFAPositive ? 1 : 0);
            cmd.Parameters.AddWithValue("@qsofaScore", item.QSOFAScore ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@antipyreticsGiven", item.AntipyreticsGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@antipyreticType", item.AntipyreticType ?? "");
            cmd.Parameters.AddWithValue("@antipyreticGivenTime", item.AntipyreticGivenTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@culturesObtained", item.CulturesObtained ? 1 : 0);
            cmd.Parameters.AddWithValue("@antibioticsStarted", item.AntibioticsStarted ? 1 : 0);
            cmd.Parameters.AddWithValue("@antibioticsStartTime", item.AntibioticsStartTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ivFluidsGiven", item.IVFluidsGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@coolingMeasures", item.CoolingMeasures ? 1 : 0);
            cmd.Parameters.AddWithValue("@increasedMonitoring", item.IncreasedMonitoring ? 1 : 0);
            cmd.Parameters.AddWithValue("@monitoringIntervalMinutes", item.MonitoringIntervalMinutes ?? (object)DBNull.Value);

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

        protected override void AddUpdateParameters(SqliteCommand cmd, Temperature item)
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
            cmd.Parameters.AddWithValue("@temperatureCelsius", item.TemperatureCelsius);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@measurementSite", item.MeasurementSite ?? "Oral");
            cmd.Parameters.AddWithValue("@feverDurationHours", item.FeverDurationHours ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@chillsPresent", item.ChillsPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@associatedSymptoms", item.AssociatedSymptoms ?? "");
            cmd.Parameters.AddWithValue("@repeatedMeasurement", item.RepeatedMeasurement ? 1 : 0);
            cmd.Parameters.AddWithValue("@clinicalAlert", item.ClinicalAlert ?? "");
            cmd.Parameters.AddWithValue("@feverCategory", item.FeverCategory ?? "");
            cmd.Parameters.AddWithValue("@intrapartumFever", item.IntrapartumFever ? 1 : 0);
            cmd.Parameters.AddWithValue("@feverOnsetTime", item.FeverOnsetTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@peakTemperature", item.PeakTemperature ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@peakTemperatureTime", item.PeakTemperatureTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@secondTemperature", item.SecondTemperature ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@secondReadingTime", item.SecondReadingTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@thirdTemperature", item.ThirdTemperature ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@thirdReadingTime", item.ThirdReadingTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@choriamnionitisRisk", item.ChoriamnionitisRisk ? 1 : 0);
            cmd.Parameters.AddWithValue("@prolongedRupture", item.ProlongedRupture ? 1 : 0);
            cmd.Parameters.AddWithValue("@hoursSinceRupture", item.HoursSinceRupture ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@maternalTachycardia", item.MaternalTachycardia ? 1 : 0);
            cmd.Parameters.AddWithValue("@fetalTachycardia", item.FetalTachycardia ? 1 : 0);
            cmd.Parameters.AddWithValue("@uterineTenderness", item.UterineTenderness ? 1 : 0);
            cmd.Parameters.AddWithValue("@offensiveLiquor", item.OffensiveLiquor ? 1 : 0);
            cmd.Parameters.AddWithValue("@rigorPresent", item.RigorPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@sweating", item.Sweating ? 1 : 0);
            cmd.Parameters.AddWithValue("@headache", item.Headache ? 1 : 0);
            cmd.Parameters.AddWithValue("@myalgiaArthralgia", item.MyalgiaArthralgia ? 1 : 0);
            cmd.Parameters.AddWithValue("@sepsisScreeningDone", item.SepsisScreeningDone ? 1 : 0);
            cmd.Parameters.AddWithValue("@sepsisScreeningTime", item.SepsisScreeningTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@sepsisRiskLevel", item.SepsisRiskLevel ?? "");
            cmd.Parameters.AddWithValue("@qsofaPositive", item.QSOFAPositive ? 1 : 0);
            cmd.Parameters.AddWithValue("@qsofaScore", item.QSOFAScore ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@antipyreticsGiven", item.AntipyreticsGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@antipyreticType", item.AntipyreticType ?? "");
            cmd.Parameters.AddWithValue("@antipyreticGivenTime", item.AntipyreticGivenTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@culturesObtained", item.CulturesObtained ? 1 : 0);
            cmd.Parameters.AddWithValue("@antibioticsStarted", item.AntibioticsStarted ? 1 : 0);
            cmd.Parameters.AddWithValue("@antibioticsStartTime", item.AntibioticsStartTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ivFluidsGiven", item.IVFluidsGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@coolingMeasures", item.CoolingMeasures ? 1 : 0);
            cmd.Parameters.AddWithValue("@increasedMonitoring", item.IncreasedMonitoring ? 1 : 0);
            cmd.Parameters.AddWithValue("@monitoringIntervalMinutes", item.MonitoringIntervalMinutes ?? (object)DBNull.Value);

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

        private float GetFloatOrDefault(SqliteDataReader reader, string columnName, float defaultValue = 0.0f)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : reader.GetFloat(ordinal);
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
    }
}
