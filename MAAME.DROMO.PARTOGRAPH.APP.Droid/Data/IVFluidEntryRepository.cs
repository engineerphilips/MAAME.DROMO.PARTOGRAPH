using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using System;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class IVFluidEntryRepository : BasePartographRepository<IVFluidEntry>
    {
        protected override string TableName => "Tbl_IVFluid";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_IVFluid (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                time TEXT NOT NULL,
                handler TEXT,
                notes TEXT NOT NULL,
                fluidtype TEXT NOT NULL,
                volumeinfused INTEGER NOT NULL,
                rate TEXT NOT NULL,
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
                ratemlperhour REAL DEFAULT 0.0,
                starttime TEXT,
                endtime TEXT,
                durationminutes INTEGER,
                additives TEXT DEFAULT '',
                additiveconcentration TEXT DEFAULT '',
                additivedose TEXT DEFAULT '',
                ivsite TEXT DEFAULT '',
                sitehealthy INTEGER DEFAULT 1,
                sitecondition TEXT DEFAULT '',
                phlebitisscore INTEGER DEFAULT 0,
                lastsiteassessment TEXT,
                lastdressingchange TEXT,
                cannelainsertiondate TEXT,
                indication TEXT DEFAULT '',
                batchnumber TEXT DEFAULT '',
                runningtotalinput INTEGER DEFAULT 0,
                clinicalalert TEXT DEFAULT ''
            );

            CREATE INDEX IF NOT EXISTS idx_ivfluid_partographid ON Tbl_IVFluid(partographid);
            CREATE INDEX IF NOT EXISTS idx_ivfluid_sync ON Tbl_IVFluid(updatedtime, syncstatus);
            CREATE INDEX IF NOT EXISTS idx_ivfluid_server_version ON Tbl_IVFluid(serverversion);

            DROP TRIGGER IF EXISTS trg_ivfluid_insert;
            CREATE TRIGGER trg_ivfluid_insert
            AFTER INSERT ON Tbl_IVFluid
            WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
            BEGIN
                UPDATE Tbl_IVFluid
                SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                    updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                WHERE ID = NEW.ID;
            END;

            DROP TRIGGER IF EXISTS trg_ivfluid_update;
            CREATE TRIGGER trg_ivfluid_update
            AFTER UPDATE ON Tbl_IVFluid
            WHEN NEW.updatedtime = OLD.updatedtime
            BEGIN
                UPDATE Tbl_IVFluid
                SET updatedtime = (strftime('%s', 'now') * 1000),
                    version = OLD.version + 1,
                    syncstatus = 0
                WHERE ID = NEW.ID;
            END;
            ";
        
        //    -- Add new columns to existing tables(WHO 2020 enhancements)
        //    ALTER TABLE Tbl_IVFluid ADD COLUMN ratemlperhour REAL DEFAULT 0.0;
        //ALTER TABLE Tbl_IVFluid ADD COLUMN starttime TEXT;
        //    ALTER TABLE Tbl_IVFluid ADD COLUMN endtime TEXT;
        //    ALTER TABLE Tbl_IVFluid ADD COLUMN durationminutes INTEGER;
        //    ALTER TABLE Tbl_IVFluid ADD COLUMN additives TEXT DEFAULT '';
        //ALTER TABLE Tbl_IVFluid ADD COLUMN additiveconcentration TEXT DEFAULT '';
        //ALTER TABLE Tbl_IVFluid ADD COLUMN additivedose TEXT DEFAULT '';
        //ALTER TABLE Tbl_IVFluid ADD COLUMN ivsite TEXT DEFAULT '';
        //ALTER TABLE Tbl_IVFluid ADD COLUMN sitehealthy INTEGER DEFAULT 1;
        //ALTER TABLE Tbl_IVFluid ADD COLUMN sitecondition TEXT DEFAULT '';
        //ALTER TABLE Tbl_IVFluid ADD COLUMN phlebitisscore INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_IVFluid ADD COLUMN lastsiteassessment TEXT;
        //    ALTER TABLE Tbl_IVFluid ADD COLUMN lastdressingchange TEXT;
        //    ALTER TABLE Tbl_IVFluid ADD COLUMN cannelainsertiondate TEXT;
        //    ALTER TABLE Tbl_IVFluid ADD COLUMN indication TEXT DEFAULT '';
        //ALTER TABLE Tbl_IVFluid ADD COLUMN batchnumber TEXT DEFAULT '';
        //ALTER TABLE Tbl_IVFluid ADD COLUMN runningtotalinput INTEGER DEFAULT 0;
        //ALTER TABLE Tbl_IVFluid ADD COLUMN clinicalalert TEXT DEFAULT '';

        public IVFluidEntryRepository(ILogger<IVFluidEntryRepository> logger) : base(logger) { }

        protected override IVFluidEntry MapFromReader(SqliteDataReader reader)
        {
            var item = new IVFluidEntry
            {
                ID = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographID = reader.IsDBNull(reader.GetOrdinal("partographid")) ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                HandlerName = reader.IsDBNull(reader.GetOrdinal("staffname")) ? string.Empty : reader.GetString(reader.GetOrdinal("staffname")),
                Notes = reader.GetString(reader.GetOrdinal("notes")),
                FluidType = reader.GetString(reader.GetOrdinal("fluidtype")),
                VolumeInfused = reader.GetInt32(reader.GetOrdinal("volumeinfused")),
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
                item.RateMlPerHour = GetDecimalOrDefault(reader, "ratemlperhour");
                item.StartTime = GetNullableDateTime(reader, "starttime");
                item.EndTime = GetNullableDateTime(reader, "endtime");
                item.DurationMinutes = GetNullableInt(reader, "durationminutes");
                item.Additives = GetStringOrDefault(reader, "additives");
                item.AdditiveConcentration = GetStringOrDefault(reader, "additiveconcentration");
                item.AdditiveDose = GetStringOrDefault(reader, "additivedose");
                item.IVSite = GetStringOrDefault(reader, "ivsite");
                item.SiteHealthy = GetBoolFromInt(reader, "sitehealthy");
                item.SiteCondition = GetStringOrDefault(reader, "sitecondition");
                item.PhlebitisScore = GetIntOrDefault(reader, "phlebitisscore");
                item.LastSiteAssessment = GetNullableDateTime(reader, "lastsiteassessment");
                item.LastDressingChange = GetNullableDateTime(reader, "lastdressingchange");
                item.CannelaInsertionDate = GetNullableDateTime(reader, "cannelainsertiondate");
                item.Indication = GetStringOrDefault(reader, "indication");
                item.BatchNumber = GetStringOrDefault(reader, "batchnumber");
                item.RunningTotalInput = GetIntOrDefault(reader, "runningtotalinput");
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

        private decimal GetDecimalOrDefault(SqliteDataReader reader, string columnName, decimal defaultValue = 0)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : (decimal)reader.GetDouble(ordinal);
            }
            catch { return defaultValue; }
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

        protected override string GetInsertSql() => @"
        INSERT INTO Tbl_IVFluid (ID, partographID, time, handler, notes, fluidtype, volumeinfused, rate, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash,
            ratemlperhour, starttime, endtime, durationminutes, additives, additiveconcentration, additivedose, ivsite, sitehealthy, sitecondition, phlebitisscore, lastsiteassessment, lastdressingchange, cannelainsertiondate, indication, batchnumber, runningtotalinput, clinicalalert)
        VALUES (@id, @partographId, @time, @handler, @notes, @fluidtype, @volumeinfused, @rate, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash,
            @ratemlperhour, @starttime, @endtime, @durationminutes, @additives, @additiveconcentration, @additivedose, @ivsite, @sitehealthy, @sitecondition, @phlebitisscore, @lastsiteassessment, @lastdressingchange, @cannelainsertiondate, @indication, @batchnumber, @runningtotalinput, @clinicalalert);";

        protected override string GetUpdateSql() => @"
        UPDATE Tbl_IVFluid
        SET partographID = @partographId,
            time = @time,
            handler = @handler,
            notes = @notes,
            fluidtype = @fluidtype,
            volumeinfused = @volumeinfused,
            rate = @rate,
            ratemlperhour = @ratemlperhour,
            starttime = @starttime,
            endtime = @endtime,
            durationminutes = @durationminutes,
            additives = @additives,
            additiveconcentration = @additiveconcentration,
            additivedose = @additivedose,
            ivsite = @ivsite,
            sitehealthy = @sitehealthy,
            sitecondition = @sitecondition,
            phlebitisscore = @phlebitisscore,
            lastsiteassessment = @lastsiteassessment,
            lastdressingchange = @lastdressingchange,
            cannelainsertiondate = @cannelainsertiondate,
            indication = @indication,
            batchnumber = @batchnumber,
            runningtotalinput = @runningtotalinput,
            clinicalalert = @clinicalalert,
            updatedtime = @updatedtime,
            deletedtime = @deletedtime,
            deviceid = @deviceid,
            syncstatus = @syncstatus,
            version = @version,
            datahash = @datahash
        WHERE ID = @id";

        protected override void AddInsertParameters(SqliteCommand cmd, IVFluidEntry item)
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
            cmd.Parameters.AddWithValue("@fluidtype", item.FluidType ?? "");
            cmd.Parameters.AddWithValue("@volumeinfused", item.VolumeInfused);
            cmd.Parameters.AddWithValue("@rate", ""); // Legacy field, keep for backward compatibility
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
            cmd.Parameters.AddWithValue("@ratemlperhour", (double)item.RateMlPerHour);
            cmd.Parameters.AddWithValue("@starttime", item.StartTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@endtime", item.EndTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@durationminutes", item.DurationMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@additives", item.Additives ?? "");
            cmd.Parameters.AddWithValue("@additiveconcentration", item.AdditiveConcentration ?? "");
            cmd.Parameters.AddWithValue("@additivedose", item.AdditiveDose ?? "");
            cmd.Parameters.AddWithValue("@ivsite", item.IVSite ?? "");
            cmd.Parameters.AddWithValue("@sitehealthy", item.SiteHealthy ? 1 : 0);
            cmd.Parameters.AddWithValue("@sitecondition", item.SiteCondition ?? "");
            cmd.Parameters.AddWithValue("@phlebitisscore", item.PhlebitisScore);
            cmd.Parameters.AddWithValue("@lastsiteassessment", item.LastSiteAssessment?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@lastdressingchange", item.LastDressingChange?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@cannelainsertiondate", item.CannelaInsertionDate?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@indication", item.Indication ?? "");
            cmd.Parameters.AddWithValue("@batchnumber", item.BatchNumber ?? "");
            cmd.Parameters.AddWithValue("@runningtotalinput", item.RunningTotalInput);
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, IVFluidEntry item)
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
            cmd.Parameters.AddWithValue("@fluidtype", item.FluidType ?? "");
            cmd.Parameters.AddWithValue("@volumeinfused", item.VolumeInfused);
            cmd.Parameters.AddWithValue("@rate", ""); // Legacy field, keep for backward compatibility
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash);

            // WHO 2020 enhancements
            cmd.Parameters.AddWithValue("@ratemlperhour", (double)item.RateMlPerHour);
            cmd.Parameters.AddWithValue("@starttime", item.StartTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@endtime", item.EndTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@durationminutes", item.DurationMinutes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@additives", item.Additives ?? "");
            cmd.Parameters.AddWithValue("@additiveconcentration", item.AdditiveConcentration ?? "");
            cmd.Parameters.AddWithValue("@additivedose", item.AdditiveDose ?? "");
            cmd.Parameters.AddWithValue("@ivsite", item.IVSite ?? "");
            cmd.Parameters.AddWithValue("@sitehealthy", item.SiteHealthy ? 1 : 0);
            cmd.Parameters.AddWithValue("@sitecondition", item.SiteCondition ?? "");
            cmd.Parameters.AddWithValue("@phlebitisscore", item.PhlebitisScore);
            cmd.Parameters.AddWithValue("@lastsiteassessment", item.LastSiteAssessment?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@lastdressingchange", item.LastDressingChange?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@cannelainsertiondate", item.CannelaInsertionDate?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@indication", item.Indication ?? "");
            cmd.Parameters.AddWithValue("@batchnumber", item.BatchNumber ?? "");
            cmd.Parameters.AddWithValue("@runningtotalinput", item.RunningTotalInput);
            cmd.Parameters.AddWithValue("@clinicalalert", item.ClinicalAlert ?? "");
        }
    }
}
