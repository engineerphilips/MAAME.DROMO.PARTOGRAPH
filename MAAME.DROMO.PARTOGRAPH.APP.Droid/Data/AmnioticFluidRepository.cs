using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Amniotic Fluid Repository - WHO 2020 Enhanced
    public class AmnioticFluidRepository : BasePartographRepository<AmnioticFluid>
    {
        protected override string TableName => "Tbl_AmnioticFluid";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_AmnioticFluid (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                color TEXT DEFAULT 'Clear',
                meconiumStaining INTEGER DEFAULT 0,
                meconiumGrade TEXT DEFAULT '',
                consistency TEXT DEFAULT 'Normal',
                amount TEXT DEFAULT 'Normal',
                odor TEXT DEFAULT 'None',
                ruptureStatus TEXT DEFAULT 'Intact',
                ruptureTime TEXT,
                clinicalAlert TEXT DEFAULT '',
                ruptureMethod TEXT DEFAULT '',
                ruptureLocation TEXT DEFAULT '',
                confirmedRupture INTEGER DEFAULT 0,
                confirmationMethod TEXT DEFAULT '',
                fluidVolume TEXT DEFAULT '',
                estimatedVolumeMl INTEGER,
                poolingInVagina INTEGER DEFAULT 0,
                meconiumFirstNotedTime TEXT,
                meconiumThickParticulate INTEGER DEFAULT 0,
                neonatalTeamAlerted INTEGER DEFAULT 0,
                neonatalTeamAlertTime TEXT,
                prolongedRupture INTEGER DEFAULT 0,
                hoursSinceRupture INTEGER,
                maternalFever INTEGER DEFAULT 0,
                maternalTachycardia INTEGER DEFAULT 0,
                fetalTachycardia INTEGER DEFAULT 0,
                uterineTenderness INTEGER DEFAULT 0,
                bloodSource TEXT DEFAULT '',
                activeBleeding INTEGER DEFAULT 0,
                bleedingAmount TEXT DEFAULT '',
                cordProlapse INTEGER DEFAULT 0,
                cordPresentation INTEGER DEFAULT 0,
                cordComplicationTime TEXT,
                antibioticsIndicated INTEGER DEFAULT 0,
                amnioinfusionConsidered INTEGER DEFAULT 0,
                expeditedDeliveryNeeded INTEGER DEFAULT 0,
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

            CREATE INDEX IF NOT EXISTS idx_amnioticfluid_sync ON Tbl_AmnioticFluid(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_amnioticfluid_server_version ON Tbl_AmnioticFluid(serverversion);

            DROP TRIGGER IF EXISTS trg_amnioticfluid_insert;
            CREATE TRIGGER trg_amnioticfluid_insert
            AFTER INSERT ON Tbl_AmnioticFluid
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_AmnioticFluid
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_amnioticfluid_update;
            CREATE TRIGGER trg_amnioticfluid_update
            AFTER UPDATE ON Tbl_AmnioticFluid
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_AmnioticFluid
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";
        
        //    -- Add WHO 2020 columns to existing tables
        //    ALTER TABLE Tbl_AmnioticFluid ADD COLUMN meconiumStaining INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN meconiumGrade TEXT DEFAULT '';
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN consistency TEXT DEFAULT 'Normal';
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN amount TEXT DEFAULT 'Normal';
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN odor TEXT DEFAULT 'None';
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN ruptureStatus TEXT DEFAULT 'Intact';
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN ruptureTime TEXT;
        //    ALTER TABLE Tbl_AmnioticFluid ADD COLUMN clinicalAlert TEXT DEFAULT '';
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN ruptureMethod TEXT DEFAULT '';
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN ruptureLocation TEXT DEFAULT '';
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN confirmedRupture INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN confirmationMethod TEXT DEFAULT '';
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN fluidVolume TEXT DEFAULT '';
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN estimatedVolumeMl INTEGER;
        //    ALTER TABLE Tbl_AmnioticFluid ADD COLUMN poolingInVagina INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN meconiumFirstNotedTime TEXT;
        //    ALTER TABLE Tbl_AmnioticFluid ADD COLUMN meconiumThickParticulate INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN neonatalTeamAlerted INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN neonatalTeamAlertTime TEXT;
        //    ALTER TABLE Tbl_AmnioticFluid ADD COLUMN prolongedRupture INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN hoursSinceRupture INTEGER;
        //    ALTER TABLE Tbl_AmnioticFluid ADD COLUMN maternalFever INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN maternalTachycardia INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN fetalTachycardia INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN uterineTenderness INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN bloodSource TEXT DEFAULT '';
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN activeBleeding INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN bleedingAmount TEXT DEFAULT '';
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN cordProlapse INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN cordPresentation INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN cordComplicationTime TEXT;
        //    ALTER TABLE Tbl_AmnioticFluid ADD COLUMN antibioticsIndicated INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN amnioinfusionConsidered INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_AmnioticFluid ADD COLUMN expeditedDeliveryNeeded INTEGER DEFAULT 0;

        public AmnioticFluidRepository(ILogger<AmnioticFluidRepository> logger) : base(logger) { }

        protected override AmnioticFluid MapFromReader(SqliteDataReader reader)
        {
            var item = new AmnioticFluid
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = GetStringOrDefault(reader, "handler", ""),
                Notes = GetStringOrDefault(reader, "notes", ""),
                Color = GetStringOrDefault(reader, "color", "Clear"),
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
                item.MeconiumStaining = GetBoolFromInt(reader, "meconiumStaining");
                item.MeconiumGrade = GetStringOrDefault(reader, "meconiumGrade", "");
                item.Consistency = GetStringOrDefault(reader, "consistency", "Normal");
                item.Amount = GetStringOrDefault(reader, "amount", "Normal");
                item.Odor = GetStringOrDefault(reader, "odor", "None");
                item.RuptureStatus = GetStringOrDefault(reader, "ruptureStatus", "Intact");
                item.RuptureTime = GetNullableDateTime(reader, "ruptureTime");
                item.ClinicalAlert = GetStringOrDefault(reader, "clinicalAlert", "");
                item.RuptureMethod = GetStringOrDefault(reader, "ruptureMethod", "");
                item.RuptureLocation = GetStringOrDefault(reader, "ruptureLocation", "");
                item.ConfirmedRupture = GetBoolFromInt(reader, "confirmedRupture");
                item.ConfirmationMethod = GetStringOrDefault(reader, "confirmationMethod", "");
                item.FluidVolume = GetStringOrDefault(reader, "fluidVolume", "");
                item.EstimatedVolumeMl = GetNullableInt(reader, "estimatedVolumeMl");
                item.PoolingInVagina = GetBoolFromInt(reader, "poolingInVagina");
                item.MeconiumFirstNotedTime = GetNullableDateTime(reader, "meconiumFirstNotedTime");
                item.MeconiumThickParticulate = GetBoolFromInt(reader, "meconiumThickParticulate");
                item.NeonatalTeamAlerted = GetBoolFromInt(reader, "neonatalTeamAlerted");
                item.NeonatalTeamAlertTime = GetNullableDateTime(reader, "neonatalTeamAlertTime");
                item.ProlongedRupture = GetBoolFromInt(reader, "prolongedRupture");
                item.HoursSinceRupture = GetNullableInt(reader, "hoursSinceRupture");
                item.MaternalFever = GetBoolFromInt(reader, "maternalFever");
                item.MaternalTachycardia = GetBoolFromInt(reader, "maternalTachycardia");
                item.FetalTachycardia = GetBoolFromInt(reader, "fetalTachycardia");
                item.UterineTenderness = GetBoolFromInt(reader, "uterineTenderness");
                item.BloodSource = GetStringOrDefault(reader, "bloodSource", "");
                item.ActiveBleeding = GetBoolFromInt(reader, "activeBleeding");
                item.BleedingAmount = GetStringOrDefault(reader, "bleedingAmount", "");
                item.CordProlapse = GetBoolFromInt(reader, "cordProlapse");
                item.CordPresentation = GetBoolFromInt(reader, "cordPresentation");
                item.CordComplicationTime = GetNullableDateTime(reader, "cordComplicationTime");
                item.AntibioticsIndicated = GetBoolFromInt(reader, "antibioticsIndicated");
                item.AmnioinfusionConsidered = GetBoolFromInt(reader, "amnioinfusionConsidered");
                item.ExpeditedDeliveryNeeded = GetBoolFromInt(reader, "expeditedDeliveryNeeded");
            }
            catch { /* Columns don't exist yet in old databases */ }

            return item;
        }

        protected override string GetInsertSql() => @"
INSERT INTO Tbl_AmnioticFluid (ID, partographID, time, handler, notes, color, meconiumStaining, meconiumGrade, consistency, amount, odor, ruptureStatus, ruptureTime, clinicalAlert,
    ruptureMethod, ruptureLocation, confirmedRupture, confirmationMethod, fluidVolume, estimatedVolumeMl, poolingInVagina,
    meconiumFirstNotedTime, meconiumThickParticulate, neonatalTeamAlerted, neonatalTeamAlertTime, prolongedRupture, hoursSinceRupture,
    maternalFever, maternalTachycardia, fetalTachycardia, uterineTenderness, bloodSource, activeBleeding, bleedingAmount,
    cordProlapse, cordPresentation, cordComplicationTime, antibioticsIndicated, amnioinfusionConsidered, expeditedDeliveryNeeded,
    createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash)
VALUES (@id, @partographId, @time, @handler, @notes, @color, @meconiumStaining, @meconiumGrade, @consistency, @amount, @odor, @ruptureStatus, @ruptureTime, @clinicalAlert,
    @ruptureMethod, @ruptureLocation, @confirmedRupture, @confirmationMethod, @fluidVolume, @estimatedVolumeMl, @poolingInVagina,
    @meconiumFirstNotedTime, @meconiumThickParticulate, @neonatalTeamAlerted, @neonatalTeamAlertTime, @prolongedRupture, @hoursSinceRupture,
    @maternalFever, @maternalTachycardia, @fetalTachycardia, @uterineTenderness, @bloodSource, @activeBleeding, @bleedingAmount,
    @cordProlapse, @cordPresentation, @cordComplicationTime, @antibioticsIndicated, @amnioinfusionConsidered, @expeditedDeliveryNeeded,
    @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash);";

        protected override string GetUpdateSql() => @"
UPDATE Tbl_AmnioticFluid
SET partographID = @partographId,
    time = @time,
    handler = @handler,
    notes = @notes,
    color = @color,
    meconiumStaining = @meconiumStaining,
    meconiumGrade = @meconiumGrade,
    consistency = @consistency,
    amount = @amount,
    odor = @odor,
    ruptureStatus = @ruptureStatus,
    ruptureTime = @ruptureTime,
    clinicalAlert = @clinicalAlert,
    ruptureMethod = @ruptureMethod,
    ruptureLocation = @ruptureLocation,
    confirmedRupture = @confirmedRupture,
    confirmationMethod = @confirmationMethod,
    fluidVolume = @fluidVolume,
    estimatedVolumeMl = @estimatedVolumeMl,
    poolingInVagina = @poolingInVagina,
    meconiumFirstNotedTime = @meconiumFirstNotedTime,
    meconiumThickParticulate = @meconiumThickParticulate,
    neonatalTeamAlerted = @neonatalTeamAlerted,
    neonatalTeamAlertTime = @neonatalTeamAlertTime,
    prolongedRupture = @prolongedRupture,
    hoursSinceRupture = @hoursSinceRupture,
    maternalFever = @maternalFever,
    maternalTachycardia = @maternalTachycardia,
    fetalTachycardia = @fetalTachycardia,
    uterineTenderness = @uterineTenderness,
    bloodSource = @bloodSource,
    activeBleeding = @activeBleeding,
    bleedingAmount = @bleedingAmount,
    cordProlapse = @cordProlapse,
    cordPresentation = @cordPresentation,
    cordComplicationTime = @cordComplicationTime,
    antibioticsIndicated = @antibioticsIndicated,
    amnioinfusionConsidered = @amnioinfusionConsidered,
    expeditedDeliveryNeeded = @expeditedDeliveryNeeded,
    updatedtime = @updatedtime,
    deletedtime = @deletedtime,
    deviceid = @deviceid,
    syncstatus = @syncstatus,
    version = @version,
    datahash = @datahash
WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, AmnioticFluid item)
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
            cmd.Parameters.AddWithValue("@color", item.Color ?? "Clear");

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@meconiumStaining", item.MeconiumStaining ? 1 : 0);
            cmd.Parameters.AddWithValue("@meconiumGrade", item.MeconiumGrade ?? "");
            cmd.Parameters.AddWithValue("@consistency", item.Consistency ?? "Normal");
            cmd.Parameters.AddWithValue("@amount", item.Amount ?? "Normal");
            cmd.Parameters.AddWithValue("@odor", item.Odor ?? "None");
            cmd.Parameters.AddWithValue("@ruptureStatus", item.RuptureStatus ?? "Intact");
            cmd.Parameters.AddWithValue("@ruptureTime", item.RuptureTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clinicalAlert", item.ClinicalAlert ?? "");
            cmd.Parameters.AddWithValue("@ruptureMethod", item.RuptureMethod ?? "");
            cmd.Parameters.AddWithValue("@ruptureLocation", item.RuptureLocation ?? "");
            cmd.Parameters.AddWithValue("@confirmedRupture", item.ConfirmedRupture ? 1 : 0);
            cmd.Parameters.AddWithValue("@confirmationMethod", item.ConfirmationMethod ?? "");
            cmd.Parameters.AddWithValue("@fluidVolume", item.FluidVolume ?? "");
            cmd.Parameters.AddWithValue("@estimatedVolumeMl", item.EstimatedVolumeMl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@poolingInVagina", item.PoolingInVagina ? 1 : 0);
            cmd.Parameters.AddWithValue("@meconiumFirstNotedTime", item.MeconiumFirstNotedTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@meconiumThickParticulate", item.MeconiumThickParticulate ? 1 : 0);
            cmd.Parameters.AddWithValue("@neonatalTeamAlerted", item.NeonatalTeamAlerted ? 1 : 0);
            cmd.Parameters.AddWithValue("@neonatalTeamAlertTime", item.NeonatalTeamAlertTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@prolongedRupture", item.ProlongedRupture ? 1 : 0);
            cmd.Parameters.AddWithValue("@hoursSinceRupture", item.HoursSinceRupture ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@maternalFever", item.MaternalFever ? 1 : 0);
            cmd.Parameters.AddWithValue("@maternalTachycardia", item.MaternalTachycardia ? 1 : 0);
            cmd.Parameters.AddWithValue("@fetalTachycardia", item.FetalTachycardia ? 1 : 0);
            cmd.Parameters.AddWithValue("@uterineTenderness", item.UterineTenderness ? 1 : 0);
            cmd.Parameters.AddWithValue("@bloodSource", item.BloodSource ?? "");
            cmd.Parameters.AddWithValue("@activeBleeding", item.ActiveBleeding ? 1 : 0);
            cmd.Parameters.AddWithValue("@bleedingAmount", item.BleedingAmount ?? "");
            cmd.Parameters.AddWithValue("@cordProlapse", item.CordProlapse ? 1 : 0);
            cmd.Parameters.AddWithValue("@cordPresentation", item.CordPresentation ? 1 : 0);
            cmd.Parameters.AddWithValue("@cordComplicationTime", item.CordComplicationTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@antibioticsIndicated", item.AntibioticsIndicated ? 1 : 0);
            cmd.Parameters.AddWithValue("@amnioinfusionConsidered", item.AmnioinfusionConsidered ? 1 : 0);
            cmd.Parameters.AddWithValue("@expeditedDeliveryNeeded", item.ExpeditedDeliveryNeeded ? 1 : 0);

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

        protected override void AddUpdateParameters(SqliteCommand cmd, AmnioticFluid item)
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
            cmd.Parameters.AddWithValue("@color", item.Color ?? "Clear");

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@meconiumStaining", item.MeconiumStaining ? 1 : 0);
            cmd.Parameters.AddWithValue("@meconiumGrade", item.MeconiumGrade ?? "");
            cmd.Parameters.AddWithValue("@consistency", item.Consistency ?? "Normal");
            cmd.Parameters.AddWithValue("@amount", item.Amount ?? "Normal");
            cmd.Parameters.AddWithValue("@odor", item.Odor ?? "None");
            cmd.Parameters.AddWithValue("@ruptureStatus", item.RuptureStatus ?? "Intact");
            cmd.Parameters.AddWithValue("@ruptureTime", item.RuptureTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clinicalAlert", item.ClinicalAlert ?? "");
            cmd.Parameters.AddWithValue("@ruptureMethod", item.RuptureMethod ?? "");
            cmd.Parameters.AddWithValue("@ruptureLocation", item.RuptureLocation ?? "");
            cmd.Parameters.AddWithValue("@confirmedRupture", item.ConfirmedRupture ? 1 : 0);
            cmd.Parameters.AddWithValue("@confirmationMethod", item.ConfirmationMethod ?? "");
            cmd.Parameters.AddWithValue("@fluidVolume", item.FluidVolume ?? "");
            cmd.Parameters.AddWithValue("@estimatedVolumeMl", item.EstimatedVolumeMl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@poolingInVagina", item.PoolingInVagina ? 1 : 0);
            cmd.Parameters.AddWithValue("@meconiumFirstNotedTime", item.MeconiumFirstNotedTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@meconiumThickParticulate", item.MeconiumThickParticulate ? 1 : 0);
            cmd.Parameters.AddWithValue("@neonatalTeamAlerted", item.NeonatalTeamAlerted ? 1 : 0);
            cmd.Parameters.AddWithValue("@neonatalTeamAlertTime", item.NeonatalTeamAlertTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@prolongedRupture", item.ProlongedRupture ? 1 : 0);
            cmd.Parameters.AddWithValue("@hoursSinceRupture", item.HoursSinceRupture ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@maternalFever", item.MaternalFever ? 1 : 0);
            cmd.Parameters.AddWithValue("@maternalTachycardia", item.MaternalTachycardia ? 1 : 0);
            cmd.Parameters.AddWithValue("@fetalTachycardia", item.FetalTachycardia ? 1 : 0);
            cmd.Parameters.AddWithValue("@uterineTenderness", item.UterineTenderness ? 1 : 0);
            cmd.Parameters.AddWithValue("@bloodSource", item.BloodSource ?? "");
            cmd.Parameters.AddWithValue("@activeBleeding", item.ActiveBleeding ? 1 : 0);
            cmd.Parameters.AddWithValue("@bleedingAmount", item.BleedingAmount ?? "");
            cmd.Parameters.AddWithValue("@cordProlapse", item.CordProlapse ? 1 : 0);
            cmd.Parameters.AddWithValue("@cordPresentation", item.CordPresentation ? 1 : 0);
            cmd.Parameters.AddWithValue("@cordComplicationTime", item.CordComplicationTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@antibioticsIndicated", item.AntibioticsIndicated ? 1 : 0);
            cmd.Parameters.AddWithValue("@amnioinfusionConsidered", item.AmnioinfusionConsidered ? 1 : 0);
            cmd.Parameters.AddWithValue("@expeditedDeliveryNeeded", item.ExpeditedDeliveryNeeded ? 1 : 0);

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
