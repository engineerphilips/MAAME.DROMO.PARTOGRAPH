using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using System;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class OralFluidRepository : BasePartographRepository<OralFluidEntry>
    {
        protected override string TableName => "Tbl_OralFluid";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_OralFluid (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                oralfluid TEXT,
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
                fluidtype TEXT DEFAULT '',
                amountml INTEGER DEFAULT 0,
                runningtotaloralintake INTEGER DEFAULT 0,
                tolerated INTEGER DEFAULT 1,
                vomiting INTEGER DEFAULT 0,
                nausea INTEGER DEFAULT 0,
                vomitingepisodes INTEGER,
                vomitcontent TEXT DEFAULT '',
                foodoffered INTEGER DEFAULT 0,
                foodconsumed INTEGER DEFAULT 0,
                foodtype TEXT DEFAULT '',
                nbm INTEGER DEFAULT 0,
                nbmreason TEXT DEFAULT '',
                restrictions TEXT DEFAULT '',
                restrictionreason TEXT DEFAULT '',
                patientrequestedfluids INTEGER DEFAULT 0,
                patientdeclinedfluids INTEGER DEFAULT 0,
                aspirationriskassessed INTEGER DEFAULT 0,
                aspirationrisklevel TEXT DEFAULT '',
                clinicalalert TEXT DEFAULT ''
            );

            -- Add new columns to existing tables (WHO 2020 enhancements)
            ALTER TABLE Tbl_OralFluid ADD COLUMN fluidtype TEXT DEFAULT '';
            ALTER TABLE Tbl_OralFluid ADD COLUMN amountml INTEGER DEFAULT 0;
            ALTER TABLE Tbl_OralFluid ADD COLUMN runningtotaloralintake INTEGER DEFAULT 0;
            ALTER TABLE Tbl_OralFluid ADD COLUMN tolerated INTEGER DEFAULT 1;
            ALTER TABLE Tbl_OralFluid ADD COLUMN vomiting INTEGER DEFAULT 0;
            ALTER TABLE Tbl_OralFluid ADD COLUMN nausea INTEGER DEFAULT 0;
            ALTER TABLE Tbl_OralFluid ADD COLUMN vomitingepisodes INTEGER;
            ALTER TABLE Tbl_OralFluid ADD COLUMN vomitcontent TEXT DEFAULT '';
            ALTER TABLE Tbl_OralFluid ADD COLUMN foodoffered INTEGER DEFAULT 0;
            ALTER TABLE Tbl_OralFluid ADD COLUMN foodconsumed INTEGER DEFAULT 0;
            ALTER TABLE Tbl_OralFluid ADD COLUMN foodtype TEXT DEFAULT '';
            ALTER TABLE Tbl_OralFluid ADD COLUMN nbm INTEGER DEFAULT 0;
            ALTER TABLE Tbl_OralFluid ADD COLUMN nbmreason TEXT DEFAULT '';
            ALTER TABLE Tbl_OralFluid ADD COLUMN restrictions TEXT DEFAULT '';
            ALTER TABLE Tbl_OralFluid ADD COLUMN restrictionreason TEXT DEFAULT '';
            ALTER TABLE Tbl_OralFluid ADD COLUMN patientrequestedfluids INTEGER DEFAULT 0;
            ALTER TABLE Tbl_OralFluid ADD COLUMN patientdeclinedfluids INTEGER DEFAULT 0;
            ALTER TABLE Tbl_OralFluid ADD COLUMN aspirationriskassessed INTEGER DEFAULT 0;
            ALTER TABLE Tbl_OralFluid ADD COLUMN aspirationrisklevel TEXT DEFAULT '';
            ALTER TABLE Tbl_OralFluid ADD COLUMN clinicalalert TEXT DEFAULT '';

            CREATE INDEX IF NOT EXISTS idx_oralfluid_sync ON Tbl_OralFluid(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_oralfluid_server_version ON Tbl_OralFluid(serverversion);

            DROP TRIGGER IF EXISTS trg_oralfluid_insert;
            CREATE TRIGGER trg_oralfluid_insert
            AFTER INSERT ON Tbl_OralFluid
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_OralFluid
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_oralfluid_update;
            CREATE TRIGGER trg_oralfluid_update
            AFTER UPDATE ON Tbl_OralFluid
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_OralFluid
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";

        public OralFluidRepository(ILogger<OralFluidRepository> logger) : base(logger) { }

        protected override OralFluidEntry MapFromReader(SqliteDataReader reader)
        {
            var item = new OralFluidEntry
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = reader.IsDBNull(reader.GetOrdinal("handler")) ? string.Empty : reader.GetString(reader.GetOrdinal("handler")),
                Notes = reader.GetString(reader.GetOrdinal("notes")),
                OralFluid = reader.IsDBNull(reader.GetOrdinal("oralfluid")) ? null : reader.GetString(reader.GetOrdinal("oralfluid")),
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
                item.FluidType = GetStringOrDefault(reader, "fluidtype");
                item.AmountMl = GetIntOrDefault(reader, "amountml");
                item.RunningTotalOralIntake = GetIntOrDefault(reader, "runningtotaloralintake");
                item.Tolerated = GetBoolFromInt(reader, "tolerated");
                item.Vomiting = GetBoolFromInt(reader, "vomiting");
                item.Nausea = GetBoolFromInt(reader, "nausea");
                item.VomitingEpisodes = GetNullableInt(reader, "vomitingepisodes");
                item.VomitContent = GetStringOrDefault(reader, "vomitcontent");
                item.FoodOffered = GetBoolFromInt(reader, "foodoffered");
                item.FoodConsumed = GetBoolFromInt(reader, "foodconsumed");
                item.FoodType = GetStringOrDefault(reader, "foodtype");
                item.NBM = GetBoolFromInt(reader, "nbm");
                item.NBMReason = GetStringOrDefault(reader, "nbmreason");
                item.Restrictions = GetStringOrDefault(reader, "restrictions");
                item.RestrictionReason = GetStringOrDefault(reader, "restrictionreason");
                item.PatientRequestedFluids = GetBoolFromInt(reader, "patientrequestedfluids");
                item.PatientDeclinedFluids = GetBoolFromInt(reader, "patientdeclinedfluids");
                item.AspirationRiskAssessed = GetBoolFromInt(reader, "aspirationriskassessed");
                item.AspirationRiskLevel = GetStringOrDefault(reader, "aspirationrisklevel");
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

        protected override string GetInsertSql() => @"
        INSERT INTO Tbl_OralFluid (ID, partographID, time, handler, notes, oralfluid, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash,
            fluidtype, amountml, runningtotaloralintake, tolerated, vomiting, nausea, vomitingepisodes, vomitcontent, foodoffered, foodconsumed, foodtype, nbm, nbmreason, restrictions, restrictionreason, patientrequestedfluids, patientdeclinedfluids, aspirationriskassessed, aspirationrisklevel, clinicalalert)
        VALUES (@id, @partographId, @time, @handler, @notes, @oralfluid, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash,
            @fluidtype, @amountml, @runningtotaloralintake, @tolerated, @vomiting, @nausea, @vomitingepisodes, @vomitcontent, @foodoffered, @foodconsumed, @foodtype, @nbm, @nbmreason, @restrictions, @restrictionreason, @patientrequestedfluids, @patientdeclinedfluids, @aspirationriskassessed, @aspirationrisklevel, @clinicalalert);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_OralFluid
        SET partographID = @partographId,
            time = @time,
            handler = @handler,
            notes = @notes,
            oralfluid = @oralfluid,
            fluidtype = @fluidtype,
            amountml = @amountml,
            runningtotaloralintake = @runningtotaloralintake,
            tolerated = @tolerated,
            vomiting = @vomiting,
            nausea = @nausea,
            vomitingepisodes = @vomitingepisodes,
            vomitcontent = @vomitcontent,
            foodoffered = @foodoffered,
            foodconsumed = @foodconsumed,
            foodtype = @foodtype,
            nbm = @nbm,
            nbmreason = @nbmreason,
            restrictions = @restrictions,
            restrictionreason = @restrictionreason,
            patientrequestedfluids = @patientrequestedfluids,
            patientdeclinedfluids = @patientdeclinedfluids,
            aspirationriskassessed = @aspirationriskassessed,
            aspirationrisklevel = @aspirationrisklevel,
            clinicalalert = @clinicalalert,
            updatedtime = @updatedtime,
            deletedtime = @deletedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version,
            datahash = @datahash
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, OralFluidEntry item)
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
            cmd.Parameters.AddWithValue("@oralfluid", item.OralFluid ?? "");
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
            cmd.Parameters.AddWithValue("@fluidtype", item.FluidType ?? "");
            cmd.Parameters.AddWithValue("@amountml", item.AmountMl);
            cmd.Parameters.AddWithValue("@runningtotaloralintake", item.RunningTotalOralIntake);
            cmd.Parameters.AddWithValue("@tolerated", item.Tolerated ? 1 : 0);
            cmd.Parameters.AddWithValue("@vomiting", item.Vomiting ? 1 : 0);
            cmd.Parameters.AddWithValue("@nausea", item.Nausea ? 1 : 0);
            cmd.Parameters.AddWithValue("@vomitingepisodes", item.VomitingEpisodes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@vomitcontent", item.VomitContent ?? "");
            cmd.Parameters.AddWithValue("@foodoffered", item.FoodOffered ? 1 : 0);
            cmd.Parameters.AddWithValue("@foodconsumed", item.FoodConsumed ? 1 : 0);
            cmd.Parameters.AddWithValue("@foodtype", item.FoodType ?? "");
            cmd.Parameters.AddWithValue("@nbm", item.NBM ? 1 : 0);
            cmd.Parameters.AddWithValue("@nbmreason", item.NBMReason ?? "");
            cmd.Parameters.AddWithValue("@restrictions", item.Restrictions ?? "");
            cmd.Parameters.AddWithValue("@restrictionreason", item.RestrictionReason ?? "");
            cmd.Parameters.AddWithValue("@patientrequestedfluids", item.PatientRequestedFluids ? 1 : 0);
            cmd.Parameters.AddWithValue("@patientdeclinedfluids", item.PatientDeclinedFluids ? 1 : 0);
            cmd.Parameters.AddWithValue("@aspirationriskassessed", item.AspirationRiskAssessed ? 1 : 0);
            cmd.Parameters.AddWithValue("@aspirationrisklevel", item.AspirationRiskLevel ?? "");
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, OralFluidEntry item)
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
            cmd.Parameters.AddWithValue("@oralfluid", item.OralFluid ?? "");
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@fluidtype", item.FluidType ?? "");
            cmd.Parameters.AddWithValue("@amountml", item.AmountMl);
            cmd.Parameters.AddWithValue("@runningtotaloralintake", item.RunningTotalOralIntake);
            cmd.Parameters.AddWithValue("@tolerated", item.Tolerated ? 1 : 0);
            cmd.Parameters.AddWithValue("@vomiting", item.Vomiting ? 1 : 0);
            cmd.Parameters.AddWithValue("@nausea", item.Nausea ? 1 : 0);
            cmd.Parameters.AddWithValue("@vomitingepisodes", item.VomitingEpisodes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@vomitcontent", item.VomitContent ?? "");
            cmd.Parameters.AddWithValue("@foodoffered", item.FoodOffered ? 1 : 0);
            cmd.Parameters.AddWithValue("@foodconsumed", item.FoodConsumed ? 1 : 0);
            cmd.Parameters.AddWithValue("@foodtype", item.FoodType ?? "");
            cmd.Parameters.AddWithValue("@nbm", item.NBM ? 1 : 0);
            cmd.Parameters.AddWithValue("@nbmreason", item.NBMReason ?? "");
            cmd.Parameters.AddWithValue("@restrictions", item.Restrictions ?? "");
            cmd.Parameters.AddWithValue("@restrictionreason", item.RestrictionReason ?? "");
            cmd.Parameters.AddWithValue("@patientrequestedfluids", item.PatientRequestedFluids ? 1 : 0);
            cmd.Parameters.AddWithValue("@patientdeclinedfluids", item.PatientDeclinedFluids ? 1 : 0);
            cmd.Parameters.AddWithValue("@aspirationriskassessed", item.AspirationRiskAssessed ? 1 : 0);
            cmd.Parameters.AddWithValue("@aspirationrisklevel", item.AspirationRiskLevel ?? "");
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }
    }
}
