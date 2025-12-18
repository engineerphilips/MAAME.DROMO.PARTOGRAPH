using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    /// <summary>
    /// Fourth Stage Vitals Repository - WHO 2020 Postpartum Monitoring
    /// Persists fundal height, bleeding, bladder, and uterine status assessments
    /// </summary>
    public class FourthStageVitalsRepository : BasePartographRepository<FourthStageVitals>
    {
        protected override string TableName => "Tbl_FourthStageVitals";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_FourthStageVitals (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,

                -- Fundal Height
                fundalHeight TEXT DEFAULT 'AtUmbilicus',
                fundalHeightNotes TEXT DEFAULT '',

                -- Bleeding Assessment
                bleedingStatus TEXT DEFAULT 'NormalLochia',
                estimatedBloodLossMl INTEGER,
                clotsPresent INTEGER DEFAULT 0,
                bleedingNotes TEXT DEFAULT '',

                -- Uterine Status
                uterineStatus TEXT DEFAULT 'Firm',
                uterineMassage INTEGER DEFAULT 0,
                uterineNotes TEXT DEFAULT '',

                -- Bladder Status
                bladderStatus TEXT DEFAULT 'Empty',
                catheterizationRequired INTEGER DEFAULT 0,
                bladderNotes TEXT DEFAULT '',

                -- PPH Risk
                pphRisk INTEGER DEFAULT 0,
                pphProtocolActivated INTEGER DEFAULT 0,
                uterotonicGiven INTEGER DEFAULT 0,
                uterotonicType TEXT DEFAULT '',
                uterotonicTime TEXT,

                -- Perineal Status
                perinealPainControlled INTEGER DEFAULT 1,
                perinealSwelling INTEGER DEFAULT 0,
                perinealHematoma INTEGER DEFAULT 0,

                -- General Wellbeing
                maternalComfortable INTEGER DEFAULT 1,
                bondingInitiated INTEGER DEFAULT 0,
                breastfeedingInitiated INTEGER DEFAULT 0,
                skinToSkinContact INTEGER DEFAULT 0,

                -- Vital Signs References
                associatedBPId TEXT,
                associatedTemperatureId TEXT,

                -- Alerts
                requiresAttention INTEGER DEFAULT 0,
                alertMessage TEXT DEFAULT '',

                -- Sync columns
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

            CREATE INDEX IF NOT EXISTS idx_fourthstagevitals_sync ON Tbl_FourthStageVitals(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_fourthstagevitals_partograph ON Tbl_FourthStageVitals(partographid);
            CREATE INDEX IF NOT EXISTS idx_fourthstagevitals_server_version ON Tbl_FourthStageVitals(serverversion);

            DROP TRIGGER IF EXISTS trg_fourthstagevitals_insert;
            CREATE TRIGGER trg_fourthstagevitals_insert
            AFTER INSERT ON Tbl_FourthStageVitals
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_FourthStageVitals
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_fourthstagevitals_update;
            CREATE TRIGGER trg_fourthstagevitals_update
            AFTER UPDATE ON Tbl_FourthStageVitals
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_FourthStageVitals
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";

        public FourthStageVitalsRepository(ILogger<FourthStageVitalsRepository> logger) : base(logger) { }

        protected override FourthStageVitals MapFromReader(SqliteDataReader reader)
        {
            var item = new FourthStageVitals
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = GetStringOrDefault(reader, "staffname", ""),
                Notes = GetStringOrDefault(reader, "notes", ""),
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

            // Fundal Height
            item.FundalHeight = Enum.TryParse<FundalHeightStatus>(GetStringOrDefault(reader, "fundalHeight", "AtUmbilicus"), out var fh) ? fh : FundalHeightStatus.AtUmbilicus;
            item.FundalHeightNotes = GetStringOrDefault(reader, "fundalHeightNotes", "");

            // Bleeding Assessment
            item.BleedingStatus = Enum.TryParse<BleedingStatus>(GetStringOrDefault(reader, "bleedingStatus", "NormalLochia"), out var bs) ? bs : BleedingStatus.NormalLochia;
            item.EstimatedBloodLossMl = GetNullableInt(reader, "estimatedBloodLossMl");
            item.ClotsPresent = GetBoolFromInt(reader, "clotsPresent");
            item.BleedingNotes = GetStringOrDefault(reader, "bleedingNotes", "");

            // Uterine Status
            item.UterineStatus = Enum.TryParse<UterineStatus>(GetStringOrDefault(reader, "uterineStatus", "Firm"), out var us) ? us : UterineStatus.Firm;
            item.UterineMassage = GetBoolFromInt(reader, "uterineMassage");
            item.UterineNotes = GetStringOrDefault(reader, "uterineNotes", "");

            // Bladder Status
            item.BladderStatus = Enum.TryParse<BladderStatus>(GetStringOrDefault(reader, "bladderStatus", "Empty"), out var bls) ? bls : BladderStatus.Empty;
            item.CatheterizationRequired = GetBoolFromInt(reader, "catheterizationRequired");
            item.BladderNotes = GetStringOrDefault(reader, "bladderNotes", "");

            // PPH Risk
            item.PPHRisk = GetBoolFromInt(reader, "pphRisk");
            item.PPHProtocolActivated = GetBoolFromInt(reader, "pphProtocolActivated");
            item.UterotonicGiven = GetBoolFromInt(reader, "uterotonicGiven");
            item.UterotonicType = GetStringOrDefault(reader, "uterotonicType", "");
            item.UterotonicTime = GetNullableDateTime(reader, "uterotonicTime");

            // Perineal Status
            item.PerinealPainControlled = GetBoolFromInt(reader, "perinealPainControlled", true);
            item.PerinealSwelling = GetBoolFromInt(reader, "perinealSwelling");
            item.PerinealHematoma = GetBoolFromInt(reader, "perinealHematoma");

            // General Wellbeing
            item.MaternalComfortable = GetBoolFromInt(reader, "maternalComfortable", true);
            item.BondingInitiated = GetBoolFromInt(reader, "bondingInitiated");
            item.BreastfeedingInitiated = GetBoolFromInt(reader, "breastfeedingInitiated");
            item.SkinToSkinContact = GetBoolFromInt(reader, "skinToSkinContact");

            // Vital Signs References
            item.AssociatedBPId = GetNullableGuid(reader, "associatedBPId");
            item.AssociatedTemperatureId = GetNullableGuid(reader, "associatedTemperatureId");

            // Alerts
            item.RequiresAttention = GetBoolFromInt(reader, "requiresAttention");
            item.AlertMessage = GetStringOrDefault(reader, "alertMessage", "");

            return item;
        }

        protected override string GetInsertSql() => @"
INSERT INTO Tbl_FourthStageVitals (
    ID, partographid, time, handler, notes,
    fundalHeight, fundalHeightNotes,
    bleedingStatus, estimatedBloodLossMl, clotsPresent, bleedingNotes,
    uterineStatus, uterineMassage, uterineNotes,
    bladderStatus, catheterizationRequired, bladderNotes,
    pphRisk, pphProtocolActivated, uterotonicGiven, uterotonicType, uterotonicTime,
    perinealPainControlled, perinealSwelling, perinealHematoma,
    maternalComfortable, bondingInitiated, breastfeedingInitiated, skinToSkinContact,
    associatedBPId, associatedTemperatureId,
    requiresAttention, alertMessage,
    createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash
)
VALUES (
    @id, @partographId, @time, @handler, @notes,
    @fundalHeight, @fundalHeightNotes,
    @bleedingStatus, @estimatedBloodLossMl, @clotsPresent, @bleedingNotes,
    @uterineStatus, @uterineMassage, @uterineNotes,
    @bladderStatus, @catheterizationRequired, @bladderNotes,
    @pphRisk, @pphProtocolActivated, @uterotonicGiven, @uterotonicType, @uterotonicTime,
    @perinealPainControlled, @perinealSwelling, @perinealHematoma,
    @maternalComfortable, @bondingInitiated, @breastfeedingInitiated, @skinToSkinContact,
    @associatedBPId, @associatedTemperatureId,
    @requiresAttention, @alertMessage,
    @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash
);";

        protected override string GetUpdateSql() => @"
UPDATE Tbl_FourthStageVitals
SET partographid = @partographId,
    time = @time,
    handler = @handler,
    notes = @notes,
    fundalHeight = @fundalHeight,
    fundalHeightNotes = @fundalHeightNotes,
    bleedingStatus = @bleedingStatus,
    estimatedBloodLossMl = @estimatedBloodLossMl,
    clotsPresent = @clotsPresent,
    bleedingNotes = @bleedingNotes,
    uterineStatus = @uterineStatus,
    uterineMassage = @uterineMassage,
    uterineNotes = @uterineNotes,
    bladderStatus = @bladderStatus,
    catheterizationRequired = @catheterizationRequired,
    bladderNotes = @bladderNotes,
    pphRisk = @pphRisk,
    pphProtocolActivated = @pphProtocolActivated,
    uterotonicGiven = @uterotonicGiven,
    uterotonicType = @uterotonicType,
    uterotonicTime = @uterotonicTime,
    perinealPainControlled = @perinealPainControlled,
    perinealSwelling = @perinealSwelling,
    perinealHematoma = @perinealHematoma,
    maternalComfortable = @maternalComfortable,
    bondingInitiated = @bondingInitiated,
    breastfeedingInitiated = @breastfeedingInitiated,
    skinToSkinContact = @skinToSkinContact,
    associatedBPId = @associatedBPId,
    associatedTemperatureId = @associatedTemperatureId,
    requiresAttention = @requiresAttention,
    alertMessage = @alertMessage,
    updatedtime = @updatedtime,
    deletedtime = @deletedtime,
    deviceid = @deviceid,
    syncstatus = @syncstatus,
    version = @version,
    datahash = @datahash
WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, FourthStageVitals item)
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
            cmd.Parameters.AddWithValue("@handler", item.Handler?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? "");

            // Fundal Height
            cmd.Parameters.AddWithValue("@fundalHeight", item.FundalHeight.ToString());
            cmd.Parameters.AddWithValue("@fundalHeightNotes", item.FundalHeightNotes ?? "");

            // Bleeding Assessment
            cmd.Parameters.AddWithValue("@bleedingStatus", item.BleedingStatus.ToString());
            cmd.Parameters.AddWithValue("@estimatedBloodLossMl", item.EstimatedBloodLossMl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clotsPresent", item.ClotsPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@bleedingNotes", item.BleedingNotes ?? "");

            // Uterine Status
            cmd.Parameters.AddWithValue("@uterineStatus", item.UterineStatus.ToString());
            cmd.Parameters.AddWithValue("@uterineMassage", item.UterineMassage ? 1 : 0);
            cmd.Parameters.AddWithValue("@uterineNotes", item.UterineNotes ?? "");

            // Bladder Status
            cmd.Parameters.AddWithValue("@bladderStatus", item.BladderStatus.ToString());
            cmd.Parameters.AddWithValue("@catheterizationRequired", item.CatheterizationRequired ? 1 : 0);
            cmd.Parameters.AddWithValue("@bladderNotes", item.BladderNotes ?? "");

            // PPH Risk
            cmd.Parameters.AddWithValue("@pphRisk", item.PPHRisk ? 1 : 0);
            cmd.Parameters.AddWithValue("@pphProtocolActivated", item.PPHProtocolActivated ? 1 : 0);
            cmd.Parameters.AddWithValue("@uterotonicGiven", item.UterotonicGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@uterotonicType", item.UterotonicType ?? "");
            cmd.Parameters.AddWithValue("@uterotonicTime", item.UterotonicTime?.ToString("O") ?? (object)DBNull.Value);

            // Perineal Status
            cmd.Parameters.AddWithValue("@perinealPainControlled", item.PerinealPainControlled ? 1 : 0);
            cmd.Parameters.AddWithValue("@perinealSwelling", item.PerinealSwelling ? 1 : 0);
            cmd.Parameters.AddWithValue("@perinealHematoma", item.PerinealHematoma ? 1 : 0);

            // General Wellbeing
            cmd.Parameters.AddWithValue("@maternalComfortable", item.MaternalComfortable ? 1 : 0);
            cmd.Parameters.AddWithValue("@bondingInitiated", item.BondingInitiated ? 1 : 0);
            cmd.Parameters.AddWithValue("@breastfeedingInitiated", item.BreastfeedingInitiated ? 1 : 0);
            cmd.Parameters.AddWithValue("@skinToSkinContact", item.SkinToSkinContact ? 1 : 0);

            // Vital Signs References
            cmd.Parameters.AddWithValue("@associatedBPId", item.AssociatedBPId?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@associatedTemperatureId", item.AssociatedTemperatureId?.ToString() ?? (object)DBNull.Value);

            // Alerts
            cmd.Parameters.AddWithValue("@requiresAttention", item.RequiresAttention ? 1 : 0);
            cmd.Parameters.AddWithValue("@alertMessage", item.AlertMessage ?? "");

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

        protected override void AddUpdateParameters(SqliteCommand cmd, FourthStageVitals item)
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
            cmd.Parameters.AddWithValue("@handler", item.Handler?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? "");

            // Fundal Height
            cmd.Parameters.AddWithValue("@fundalHeight", item.FundalHeight.ToString());
            cmd.Parameters.AddWithValue("@fundalHeightNotes", item.FundalHeightNotes ?? "");

            // Bleeding Assessment
            cmd.Parameters.AddWithValue("@bleedingStatus", item.BleedingStatus.ToString());
            cmd.Parameters.AddWithValue("@estimatedBloodLossMl", item.EstimatedBloodLossMl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clotsPresent", item.ClotsPresent ? 1 : 0);
            cmd.Parameters.AddWithValue("@bleedingNotes", item.BleedingNotes ?? "");

            // Uterine Status
            cmd.Parameters.AddWithValue("@uterineStatus", item.UterineStatus.ToString());
            cmd.Parameters.AddWithValue("@uterineMassage", item.UterineMassage ? 1 : 0);
            cmd.Parameters.AddWithValue("@uterineNotes", item.UterineNotes ?? "");

            // Bladder Status
            cmd.Parameters.AddWithValue("@bladderStatus", item.BladderStatus.ToString());
            cmd.Parameters.AddWithValue("@catheterizationRequired", item.CatheterizationRequired ? 1 : 0);
            cmd.Parameters.AddWithValue("@bladderNotes", item.BladderNotes ?? "");

            // PPH Risk
            cmd.Parameters.AddWithValue("@pphRisk", item.PPHRisk ? 1 : 0);
            cmd.Parameters.AddWithValue("@pphProtocolActivated", item.PPHProtocolActivated ? 1 : 0);
            cmd.Parameters.AddWithValue("@uterotonicGiven", item.UterotonicGiven ? 1 : 0);
            cmd.Parameters.AddWithValue("@uterotonicType", item.UterotonicType ?? "");
            cmd.Parameters.AddWithValue("@uterotonicTime", item.UterotonicTime?.ToString("O") ?? (object)DBNull.Value);

            // Perineal Status
            cmd.Parameters.AddWithValue("@perinealPainControlled", item.PerinealPainControlled ? 1 : 0);
            cmd.Parameters.AddWithValue("@perinealSwelling", item.PerinealSwelling ? 1 : 0);
            cmd.Parameters.AddWithValue("@perinealHematoma", item.PerinealHematoma ? 1 : 0);

            // General Wellbeing
            cmd.Parameters.AddWithValue("@maternalComfortable", item.MaternalComfortable ? 1 : 0);
            cmd.Parameters.AddWithValue("@bondingInitiated", item.BondingInitiated ? 1 : 0);
            cmd.Parameters.AddWithValue("@breastfeedingInitiated", item.BreastfeedingInitiated ? 1 : 0);
            cmd.Parameters.AddWithValue("@skinToSkinContact", item.SkinToSkinContact ? 1 : 0);

            // Vital Signs References
            cmd.Parameters.AddWithValue("@associatedBPId", item.AssociatedBPId?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@associatedTemperatureId", item.AssociatedTemperatureId?.ToString() ?? (object)DBNull.Value);

            // Alerts
            cmd.Parameters.AddWithValue("@requiresAttention", item.RequiresAttention ? 1 : 0);
            cmd.Parameters.AddWithValue("@alertMessage", item.AlertMessage ?? "");

            // Sync parameters
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);
        }

        #region Helper Methods

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

        private Guid? GetNullableGuid(SqliteDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal)) return null;
                return Guid.Parse(reader.GetString(ordinal));
            }
            catch { return null; }
        }

        #endregion
    }
}
