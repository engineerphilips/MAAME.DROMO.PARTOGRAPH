using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Baseline FHR Repository - WHO 2020 Enhanced
    public class FHRRepository : BasePartographRepository<FHR>
    {
        protected override string TableName => "Tbl_FHR";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_FHR (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                rate INTEGER,
                deceleration TEXT,
                decelerationdurationseconds INTEGER DEFAULT 0,
                variability TEXT DEFAULT '',
                accelerations INTEGER DEFAULT 0,
                pattern TEXT DEFAULT '',
                monitoringmethod TEXT DEFAULT '',
                baselinerate INTEGER,
                clinicalalert TEXT DEFAULT '',
                variabilitybpm INTEGER,
                variabilitytrend TEXT DEFAULT '',
                sinusoidalpattern INTEGER DEFAULT 0,
                saltatorpattern INTEGER DEFAULT 0,
                accelerationcount INTEGER,
                accelerationpeakbpm INTEGER,
                accelerationdurationseconds INTEGER,
                decelerationnadirbpm INTEGER,
                decelerationrecovery TEXT DEFAULT '',
                decelerationamplitudebpm INTEGER,
                decelerationtiming TEXT DEFAULT '',
                prolongedbradycardia INTEGER DEFAULT 0,
                bradycardiastarttime TEXT,
                bradycardiadurationminutes INTEGER,
                tachycardia INTEGER DEFAULT 0,
                tachycardiastarttime TEXT,
                tachycardiadurationminutes INTEGER,
                ctgclassification TEXT DEFAULT '',
                reactivenst INTEGER DEFAULT 0,
                lastreactivetime TEXT,
                maternalposition TEXT DEFAULT '',
                duringcontraction INTEGER DEFAULT 0,
                betweencontractions INTEGER DEFAULT 0,
                interventionrequired INTEGER DEFAULT 0,
                interventiontaken TEXT DEFAULT '',
                interventiontime TEXT,
                changeinposition INTEGER DEFAULT 0,
                oxygenadministered INTEGER DEFAULT 0,
                ivfluidsincreased INTEGER DEFAULT 0,
                emergencyconsultrequired INTEGER DEFAULT 0,
                consultreason TEXT DEFAULT '',
                consulttime TEXT,
                prepareforemergencydelivery INTEGER DEFAULT 0,
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

            CREATE INDEX IF NOT EXISTS idx_fhr_sync ON Tbl_FHR(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_fhr_server_version ON Tbl_FHR(serverversion);

            -- ALTER TABLE Tbl_FHR ADD COLUMN deceleration TEXT DEFAULT '';

            DROP TRIGGER IF EXISTS trg_fhr_insert;
            CREATE TRIGGER trg_fhr_insert
            AFTER INSERT ON Tbl_FHR
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_FHR
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_fhr_update;
            CREATE TRIGGER trg_fhr_update
            AFTER UPDATE ON Tbl_FHR
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_FHR
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";
        
        //    -- Add WHO 2020 columns to existing tables
        //    ALTER TABLE Tbl_FHR ADD COLUMN decelerationdurationseconds INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_FHR ADD COLUMN variability TEXT DEFAULT '';
        //ALTER TABLE Tbl_FHR ADD COLUMN accelerations INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_FHR ADD COLUMN pattern TEXT DEFAULT '';
        //ALTER TABLE Tbl_FHR ADD COLUMN monitoringmethod TEXT DEFAULT '';
        //ALTER TABLE Tbl_FHR ADD COLUMN baselinerate INTEGER;
        //    ALTER TABLE Tbl_FHR ADD COLUMN clinicalalert TEXT DEFAULT '';
        //ALTER TABLE Tbl_FHR ADD COLUMN variabilitybpm INTEGER;
        //    ALTER TABLE Tbl_FHR ADD COLUMN variabilitytrend TEXT DEFAULT '';
        //ALTER TABLE Tbl_FHR ADD COLUMN sinusoidalpattern INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_FHR ADD COLUMN saltatorpattern INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_FHR ADD COLUMN accelerationcount INTEGER;
        //    ALTER TABLE Tbl_FHR ADD COLUMN accelerationpeakbpm INTEGER;
        //    ALTER TABLE Tbl_FHR ADD COLUMN accelerationdurationseconds INTEGER;
        //    ALTER TABLE Tbl_FHR ADD COLUMN decelerationnadirbpm INTEGER;
        //    ALTER TABLE Tbl_FHR ADD COLUMN decelerationrecovery TEXT DEFAULT '';
        //ALTER TABLE Tbl_FHR ADD COLUMN decelerationamplitudebpm INTEGER;
        //    ALTER TABLE Tbl_FHR ADD COLUMN decelerationtiming TEXT DEFAULT '';
        //ALTER TABLE Tbl_FHR ADD COLUMN prolongedbradycardia INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_FHR ADD COLUMN bradycardiastarttime TEXT;
        //    ALTER TABLE Tbl_FHR ADD COLUMN bradycardiadurationminutes INTEGER;
        //    ALTER TABLE Tbl_FHR ADD COLUMN tachycardia INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_FHR ADD COLUMN tachycardiastarttime TEXT;
        //    ALTER TABLE Tbl_FHR ADD COLUMN tachycardiadurationminutes INTEGER;
        //    ALTER TABLE Tbl_FHR ADD COLUMN ctgclassification TEXT DEFAULT '';
        //ALTER TABLE Tbl_FHR ADD COLUMN reactivenst INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_FHR ADD COLUMN lastreactivetime TEXT;
        //    ALTER TABLE Tbl_FHR ADD COLUMN maternalposition TEXT DEFAULT '';
        //ALTER TABLE Tbl_FHR ADD COLUMN duringcontraction INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_FHR ADD COLUMN betweencontractions INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_FHR ADD COLUMN interventionrequired INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_FHR ADD COLUMN interventiontaken TEXT DEFAULT '';
        //ALTER TABLE Tbl_FHR ADD COLUMN interventiontime TEXT;
        //    ALTER TABLE Tbl_FHR ADD COLUMN changeinposition INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_FHR ADD COLUMN oxygenadministered INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_FHR ADD COLUMN ivfluidsincreased INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_FHR ADD COLUMN emergencyconsultrequired INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_FHR ADD COLUMN consultreason TEXT DEFAULT '';
        //ALTER TABLE Tbl_FHR ADD COLUMN consulttime TEXT;
        //    ALTER TABLE Tbl_FHR ADD COLUMN prepareforemergencydelivery INTEGER DEFAULT 0;

        public FHRRepository(ILogger<FHRRepository> logger) : base(logger) { }

        protected override FHR MapFromReader(SqliteDataReader reader)
        {
            var item = new FHR
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = GetStringOrDefault(reader, "staffname", ""),
                Notes = GetStringOrDefault(reader, "notes", ""),
                Rate = GetNullableInt(reader, "rate"),
                Deceleration = GetStringOrDefault(reader, "deceleration", "None"),
                CreatedTime = reader.GetInt64(reader.GetOrdinal("createdtime")),
                UpdatedTime = reader.GetInt64(reader.GetOrdinal("updatedtime")),
                DeletedTime = GetNullableLong(reader, "deletedtime"),
                DeviceId = reader.GetString(reader.GetOrdinal("deviceid")),
                OriginDeviceId = reader.GetString(reader.GetOrdinal("origindeviceid")),
                SyncStatus = reader.GetInt32(reader.GetOrdinal("syncstatus")),
                Version = reader.GetInt32(reader.GetOrdinal("version")),
                ServerVersion = GetIntOrDefault(reader, "serverversion", 0),
                Deleted = GetIntOrDefault(reader, "deleted", 0),
                ConflictData = GetStringOrDefault(reader, "conflictdata", "")
            };

            // WHO 2020 enhancements - safely read new columns
            try
            {
                item.DecelerationDurationSeconds = GetIntOrDefault(reader, "decelerationdurationseconds", 0);
                item.Variability = GetStringOrDefault(reader, "variability", "");
                item.Accelerations = GetBoolFromInt(reader, "accelerations");
                item.Pattern = GetStringOrDefault(reader, "pattern", "");
                item.MonitoringMethod = GetStringOrDefault(reader, "monitoringmethod", "");
                item.BaselineRate = GetNullableInt(reader, "baselinerate");
                item.ClinicalAlert = GetStringOrDefault(reader, "clinicalalert", "");
                item.VariabilityBpm = GetNullableInt(reader, "variabilitybpm");
                item.VariabilityTrend = GetStringOrDefault(reader, "variabilitytrend", "");
                item.SinusoidalPattern = GetBoolFromInt(reader, "sinusoidalpattern");
                item.SaltatorPattern = GetBoolFromInt(reader, "saltatorpattern");
                item.AccelerationCount = GetNullableInt(reader, "accelerationcount");
                item.AccelerationPeakBpm = GetNullableInt(reader, "accelerationpeakbpm");
                item.AccelerationDurationSeconds = GetNullableInt(reader, "accelerationdurationseconds");
                item.DecelerationNadirBpm = GetNullableInt(reader, "decelerationnadirbpm");
                item.DecelerationRecovery = GetStringOrDefault(reader, "decelerationrecovery", "");
                item.DecelerationAmplitudeBpm = GetNullableInt(reader, "decelerationamplitudebpm");
                item.DecelerationTiming = GetStringOrDefault(reader, "decelerationtiming", "");
                item.ProlongedBradycardia = GetBoolFromInt(reader, "prolongedbradycardia");
                item.BradycardiaStartTime = GetNullableDateTime(reader, "bradycardiastarttime");
                item.BradycardiaDurationMinutes = GetNullableInt(reader, "bradycardiadurationminutes");
                item.Tachycardia = GetBoolFromInt(reader, "tachycardia");
                item.TachycardiaStartTime = GetNullableDateTime(reader, "tachycardiastarttime");
                item.TachycardiaDurationMinutes = GetNullableInt(reader, "tachycardiadurationminutes");
                item.CTGClassification = GetStringOrDefault(reader, "ctgclassification", "");
                item.ReactiveNST = GetBoolFromInt(reader, "reactivenst");
                item.LastReactiveTime = GetNullableDateTime(reader, "lastreactivetime");
                item.MaternalPosition = GetStringOrDefault(reader, "maternalposition", "");
                item.DuringContraction = GetBoolFromInt(reader, "duringcontraction");
                item.BetweenContractions = GetBoolFromInt(reader, "betweencontractions");
                item.InterventionRequired = GetBoolFromInt(reader, "interventionrequired");
                item.InterventionTaken = GetStringOrDefault(reader, "interventiontaken", "");
                item.InterventionTime = GetNullableDateTime(reader, "interventiontime");
                item.ChangeInPosition = GetBoolFromInt(reader, "changeinposition");
                item.OxygenAdministered = GetBoolFromInt(reader, "oxygenadministered");
                item.IVFluidsIncreased = GetBoolFromInt(reader, "ivfluidsincreased");
                item.EmergencyConsultRequired = GetBoolFromInt(reader, "emergencyconsultrequired");
                item.ConsultReason = GetStringOrDefault(reader, "consultreason", "");
                item.ConsultTime = GetNullableDateTime(reader, "consulttime");
                item.PrepareForEmergencyDelivery = GetBoolFromInt(reader, "prepareforemergencydelivery");
            }
            catch { /* Columns don't exist yet in old databases */ }

            return item;
        }

        protected override string GetInsertSql() => @"
        INSERT INTO Tbl_FHR (ID, partographID, time, handler, notes, rate, deceleration, decelerationdurationseconds, variability, accelerations, pattern, monitoringmethod, baselinerate, clinicalalert,
            variabilitybpm, variabilitytrend, sinusoidalpattern, saltatorpattern, accelerationcount, accelerationpeakbpm, accelerationdurationseconds,
            decelerationnadirbpm, decelerationrecovery, decelerationamplitudebpm, decelerationtiming, prolongedbradycardia, bradycardiastarttime, bradycardiadurationminutes,
            tachycardia, tachycardiastarttime, tachycardiadurationminutes, ctgclassification, reactivenst, lastreactivetime, maternalposition, duringcontraction, betweencontractions,
            interventionrequired, interventiontaken, interventiontime, changeinposition, oxygenadministered, ivfluidsincreased, emergencyconsultrequired, consultreason, consulttime, prepareforemergencydelivery,
            createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash)
        VALUES (@id, @partographId, @time, @handler, @notes, @rate, @deceleration, @decelerationdurationseconds, @variability, @accelerations, @pattern, @monitoringmethod, @baselinerate, @clinicalalert,
            @variabilitybpm, @variabilitytrend, @sinusoidalpattern, @saltatorpattern, @accelerationcount, @accelerationpeakbpm, @accelerationdurationseconds,
            @decelerationnadirbpm, @decelerationrecovery, @decelerationamplitudebpm, @decelerationtiming, @prolongedbradycardia, @bradycardiastarttime, @bradycardiadurationminutes,
            @tachycardia, @tachycardiastarttime, @tachycardiadurationminutes, @ctgclassification, @reactivenst, @lastreactivetime, @maternalposition, @duringcontraction, @betweencontractions,
            @interventionrequired, @interventiontaken, @interventiontime, @changeinposition, @oxygenadministered, @ivfluidsincreased, @emergencyconsultrequired, @consultreason, @consulttime, @prepareforemergencydelivery,
            @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_FHR
        SET partographID = @partographId, time = @time, handler = @handler, notes = @notes, rate = @rate, deceleration = @deceleration,
            decelerationdurationseconds = @decelerationdurationseconds, variability = @variability, accelerations = @accelerations, pattern = @pattern, monitoringmethod = @monitoringmethod, baselinerate = @baselinerate, clinicalalert = @clinicalalert,
            variabilitybpm = @variabilitybpm, variabilitytrend = @variabilitytrend, sinusoidalpattern = @sinusoidalpattern, saltatorpattern = @saltatorpattern, accelerationcount = @accelerationcount, accelerationpeakbpm = @accelerationpeakbpm, accelerationdurationseconds = @accelerationdurationseconds,
            decelerationnadirbpm = @decelerationnadirbpm, decelerationrecovery = @decelerationrecovery, decelerationamplitudebpm = @decelerationamplitudebpm, decelerationtiming = @decelerationtiming, prolongedbradycardia = @prolongedbradycardia, bradycardiastarttime = @bradycardiastarttime, bradycardiadurationminutes = @bradycardiadurationminutes,
            tachycardia = @tachycardia, tachycardiastarttime = @tachycardiastarttime, tachycardiadurationminutes = @tachycardiadurationminutes, ctgclassification = @ctgclassification, reactivenst = @reactivenst, lastreactivetime = @lastreactivetime, maternalposition = @maternalposition, duringcontraction = @duringcontraction, betweencontractions = @betweencontractions,
            interventionrequired = @interventionrequired, interventiontaken = @interventiontaken, interventiontime = @interventiontime, changeinposition = @changeinposition, oxygenadministered = @oxygenadministered, ivfluidsincreased = @ivfluidsincreased, emergencyconsultrequired = @emergencyconsultrequired, consultreason = @consultreason, consulttime = @consulttime, prepareforemergencydelivery = @prepareforemergencydelivery,
            updatedtime = @updatedtime, deletedtime = @deletedtime, deviceid = @deviceid, syncstatus = @syncstatus, version = @version, datahash = @datahash
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, FHR item)
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
            cmd.Parameters.AddWithValue("@rate", item.Rate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@deceleration", item.Deceleration ?? "None");
            cmd.Parameters.AddWithValue("@createdtime", item.CreatedTime);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@origindeviceid", item.OriginDeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@serverversion", item.ServerVersion);
            cmd.Parameters.AddWithValue("@deleted", item.Deleted);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@decelerationdurationseconds", item.DecelerationDurationSeconds);
            cmd.Parameters.AddWithValue("@variability", item.Variability ?? "");
            cmd.Parameters.AddWithValue("@accelerations", item.Accelerations ? 1 : 0);
            cmd.Parameters.AddWithValue("@pattern", item.Pattern ?? "");
            cmd.Parameters.AddWithValue("@monitoringmethod", item.MonitoringMethod ?? "");
            cmd.Parameters.AddWithValue("@baselinerate", item.BaselineRate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
            cmd.Parameters.AddWithValue("@variabilitybpm", item.VariabilityBpm ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@variabilitytrend", item.VariabilityTrend ?? "");
            cmd.Parameters.AddWithValue("@sinusoidalpattern", item.SinusoidalPattern ? 1 : 0);
            cmd.Parameters.AddWithValue("@saltatorpattern", item.SaltatorPattern ? 1 : 0);
            cmd.Parameters.AddWithValue("@accelerationcount", item.AccelerationCount ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@accelerationpeakbpm", item.AccelerationPeakBpm ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@accelerationdurationseconds", item.AccelerationDurationSeconds ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@decelerationnadirbpm", item.DecelerationNadirBpm ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@decelerationrecovery", item.DecelerationRecovery ?? "");
            cmd.Parameters.AddWithValue("@decelerationamplitudebpm", item.DecelerationAmplitudeBpm ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@decelerationtiming", item.DecelerationTiming ?? "");
            cmd.Parameters.AddWithValue("@prolongedbradycardia", item.ProlongedBradycardia ? 1 : 0);
            cmd.Parameters.AddWithValue("@bradycardiastarttime", item.BradycardiaStartTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@bradycardiadurationminutes", item.BradycardiaDurationMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@tachycardia", item.Tachycardia ? 1 : 0);
            cmd.Parameters.AddWithValue("@tachycardiastarttime", item.TachycardiaStartTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@tachycardiadurationminutes", item.TachycardiaDurationMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ctgclassification", item.CTGClassification ?? "");
            cmd.Parameters.AddWithValue("@reactivenst", item.ReactiveNST ? 1 : 0);
            cmd.Parameters.AddWithValue("@lastreactivetime", item.LastReactiveTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@maternalposition", item.MaternalPosition ?? "");
            cmd.Parameters.AddWithValue("@duringcontraction", item.DuringContraction ? 1 : 0);
            cmd.Parameters.AddWithValue("@betweencontractions", item.BetweenContractions ? 1 : 0);
            cmd.Parameters.AddWithValue("@interventionrequired", item.InterventionRequired ? 1 : 0);
            cmd.Parameters.AddWithValue("@interventiontaken", item.InterventionTaken ?? "");
            cmd.Parameters.AddWithValue("@interventiontime", item.InterventionTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@changeinposition", item.ChangeInPosition ? 1 : 0);
            cmd.Parameters.AddWithValue("@oxygenadministered", item.OxygenAdministered ? 1 : 0);
            cmd.Parameters.AddWithValue("@ivfluidsincreased", item.IVFluidsIncreased ? 1 : 0);
            cmd.Parameters.AddWithValue("@emergencyconsultrequired", item.EmergencyConsultRequired ? 1 : 0);
            cmd.Parameters.AddWithValue("@consultreason", item.ConsultReason ?? "");
            cmd.Parameters.AddWithValue("@consulttime", item.ConsultTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@prepareforemergencydelivery", item.PrepareForEmergencyDelivery ? 1 : 0);
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, FHR item)
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
            cmd.Parameters.AddWithValue("@rate", item.Rate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@deceleration", item.Deceleration ?? "None");
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@decelerationdurationseconds", item.DecelerationDurationSeconds);
            cmd.Parameters.AddWithValue("@variability", item.Variability ?? "");
            cmd.Parameters.AddWithValue("@accelerations", item.Accelerations ? 1 : 0);
            cmd.Parameters.AddWithValue("@pattern", item.Pattern ?? "");
            cmd.Parameters.AddWithValue("@monitoringmethod", item.MonitoringMethod ?? "");
            cmd.Parameters.AddWithValue("@baselinerate", item.BaselineRate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
            cmd.Parameters.AddWithValue("@variabilitybpm", item.VariabilityBpm ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@variabilitytrend", item.VariabilityTrend ?? "");
            cmd.Parameters.AddWithValue("@sinusoidalpattern", item.SinusoidalPattern ? 1 : 0);
            cmd.Parameters.AddWithValue("@saltatorpattern", item.SaltatorPattern ? 1 : 0);
            cmd.Parameters.AddWithValue("@accelerationcount", item.AccelerationCount ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@accelerationpeakbpm", item.AccelerationPeakBpm ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@accelerationdurationseconds", item.AccelerationDurationSeconds ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@decelerationnadirbpm", item.DecelerationNadirBpm ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@decelerationrecovery", item.DecelerationRecovery ?? "");
            cmd.Parameters.AddWithValue("@decelerationamplitudebpm", item.DecelerationAmplitudeBpm ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@decelerationtiming", item.DecelerationTiming ?? "");
            cmd.Parameters.AddWithValue("@prolongedbradycardia", item.ProlongedBradycardia ? 1 : 0);
            cmd.Parameters.AddWithValue("@bradycardiastarttime", item.BradycardiaStartTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@bradycardiadurationminutes", item.BradycardiaDurationMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@tachycardia", item.Tachycardia ? 1 : 0);
            cmd.Parameters.AddWithValue("@tachycardiastarttime", item.TachycardiaStartTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@tachycardiadurationminutes", item.TachycardiaDurationMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ctgclassification", item.CTGClassification ?? "");
            cmd.Parameters.AddWithValue("@reactivenst", item.ReactiveNST ? 1 : 0);
            cmd.Parameters.AddWithValue("@lastreactivetime", item.LastReactiveTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@maternalposition", item.MaternalPosition ?? "");
            cmd.Parameters.AddWithValue("@duringcontraction", item.DuringContraction ? 1 : 0);
            cmd.Parameters.AddWithValue("@betweencontractions", item.BetweenContractions ? 1 : 0);
            cmd.Parameters.AddWithValue("@interventionrequired", item.InterventionRequired ? 1 : 0);
            cmd.Parameters.AddWithValue("@interventiontaken", item.InterventionTaken ?? "");
            cmd.Parameters.AddWithValue("@interventiontime", item.InterventionTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@changeinposition", item.ChangeInPosition ? 1 : 0);
            cmd.Parameters.AddWithValue("@oxygenadministered", item.OxygenAdministered ? 1 : 0);
            cmd.Parameters.AddWithValue("@ivfluidsincreased", item.IVFluidsIncreased ? 1 : 0);
            cmd.Parameters.AddWithValue("@emergencyconsultrequired", item.EmergencyConsultRequired ? 1 : 0);
            cmd.Parameters.AddWithValue("@consultreason", item.ConsultReason ?? "");
            cmd.Parameters.AddWithValue("@consulttime", item.ConsultTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@prepareforemergencydelivery", item.PrepareForEmergencyDelivery ? 1 : 0);
        }

        // Helper methods
        private bool GetBoolFromInt(SqliteDataReader reader, string columnName)
        {
            try { return !reader.IsDBNull(reader.GetOrdinal(columnName)) && reader.GetInt32(reader.GetOrdinal(columnName)) == 1; }
            catch { return false; }
        }

        private string GetStringOrDefault(SqliteDataReader reader, string columnName, string defaultValue = "")
        {
            try { int ordinal = reader.GetOrdinal(columnName); return reader.IsDBNull(ordinal) ? defaultValue : reader.GetString(ordinal); }
            catch { return defaultValue; }
        }

        private int GetIntOrDefault(SqliteDataReader reader, string columnName, int defaultValue = 0)
        {
            try { int ordinal = reader.GetOrdinal(columnName); return reader.IsDBNull(ordinal) ? defaultValue : reader.GetInt32(ordinal); }
            catch { return defaultValue; }
        }

        private int? GetNullableInt(SqliteDataReader reader, string columnName)
        {
            try { int ordinal = reader.GetOrdinal(columnName); return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal); }
            catch { return null; }
        }

        private long? GetNullableLong(SqliteDataReader reader, string columnName)
        {
            try { int ordinal = reader.GetOrdinal(columnName); return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal); }
            catch { return null; }
        }

        private DateTime? GetNullableDateTime(SqliteDataReader reader, string columnName)
        {
            try { int ordinal = reader.GetOrdinal(columnName); return reader.IsDBNull(ordinal) ? null : DateTime.Parse(reader.GetString(ordinal)); }
            catch { return null; }
        }
    }
}
