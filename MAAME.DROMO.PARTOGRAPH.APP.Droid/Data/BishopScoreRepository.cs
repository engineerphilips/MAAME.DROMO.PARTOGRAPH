using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class BishopScoreRepository : BasePartographRepository<BishopScore>
    {
        protected override string TableName => "Tbl_BishopScore";

        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_BishopScore (
                    ID TEXT PRIMARY KEY,
                    PartographID TEXT NOT NULL,
                    Time TEXT NOT NULL,
                    Dilation INTEGER NOT NULL,
                    Effacement INTEGER NOT NULL,
                    Consistency INTEGER NOT NULL,
                    Position INTEGER NOT NULL,
                    Station INTEGER NOT NULL,
                    TotalScore INTEGER NOT NULL,
                    DilationCm INTEGER,
                    EffacementPercent INTEGER,
                    CervicalConsistency TEXT,
                    CervicalPosition TEXT,
                    StationValue INTEGER,
                    Interpretation TEXT,
                    FavorableForDelivery INTEGER NOT NULL,
                    Notes TEXT, 
                    HandlerName TEXT,
                    Handler TEXT,
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
                FOREIGN KEY (PartographID) REFERENCES Tbl_Partograph(ID)
            );

            CREATE INDEX IF NOT EXISTS idx_bishopscore_time ON Tbl_BishopScore(Time);

                CREATE INDEX IF NOT EXISTS idx_bishopscore_partographid ON Tbl_BishopScore(PartographID);
                CREATE INDEX IF NOT EXISTS idx_bishopscore_sync ON Tbl_BishopScore(updatedtime, syncstatus);

                DROP TRIGGER IF EXISTS trg_bishopscore_insert;
                CREATE TRIGGER trg_bishopscore_insert
                AFTER INSERT ON Tbl_BishopScore
                WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
                BEGIN
                    UPDATE Tbl_BishopScore
                    SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                        updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                    WHERE ID = NEW.ID;
                END;

                DROP TRIGGER IF EXISTS trg_bishopscore_update;
                CREATE TRIGGER trg_bishopscore_update
                AFTER UPDATE ON Tbl_BishopScore
                WHEN NEW.updatedtime = OLD.updatedtime
                BEGIN
                    UPDATE Tbl_BishopScore
                    SET updatedtime = (strftime('%s', 'now') * 1000),
                        version = OLD.version + 1,
                        syncstatus = 0
                    WHERE ID = NEW.ID;
                END;
        ";

        public BishopScoreRepository(ILogger<BishopScoreRepository> logger) : base(logger)
        {
        }

        protected override BishopScore MapFromReader(SqliteDataReader reader)
        {
            return new BishopScore
            {
                ID = Guid.Parse(reader["ID"].ToString()),
                PartographID = reader["PartographID"] == DBNull.Value ? null : Guid.Parse(reader["PartographID"].ToString()),
                Time = DateTime.Parse(reader["Time"].ToString()),
                Dilation = Convert.ToInt32(reader["Dilation"]),
                Effacement = Convert.ToInt32(reader["Effacement"]),
                Consistency = Convert.ToInt32(reader["Consistency"]),
                Position = Convert.ToInt32(reader["Position"]),
                Station = Convert.ToInt32(reader["Station"]),
                TotalScore = Convert.ToInt32(reader["TotalScore"]),
                DilationCm = reader["DilationCm"] == DBNull.Value ? null : Convert.ToInt32(reader["DilationCm"]),
                EffacementPercent = reader["EffacementPercent"] == DBNull.Value ? null : Convert.ToInt32(reader["EffacementPercent"]),
                CervicalConsistency = reader["CervicalConsistency"]?.ToString() ?? string.Empty,
                CervicalPosition = reader["CervicalPosition"]?.ToString() ?? string.Empty,
                StationValue = reader["StationValue"] == DBNull.Value ? null : Convert.ToInt32(reader["StationValue"]),
                Interpretation = reader["Interpretation"]?.ToString() ?? string.Empty,
                FavorableForDelivery = Convert.ToBoolean(reader["FavorableForDelivery"]),
                Notes = reader["Notes"]?.ToString() ?? string.Empty, 
                HandlerName = reader["staffname"]?.ToString() ?? string.Empty,
                Handler = reader["Handler"] == DBNull.Value ? null : Guid.Parse(reader["Handler"].ToString()),
                CreatedTime = Convert.ToInt64(reader["createdtime"]),
                UpdatedTime = Convert.ToInt64(reader["updatedtime"]),
                DeletedTime = reader["deletedtime"] == DBNull.Value ? null : Convert.ToInt64(reader["deletedtime"]),
                DeviceId = reader["deviceid"]?.ToString() ?? string.Empty,
                OriginDeviceId = reader["origindeviceid"]?.ToString() ?? string.Empty,
                SyncStatus = Convert.ToInt32(reader["syncstatus"]),
                Version = Convert.ToInt32(reader["version"]),
                ServerVersion = Convert.ToInt32(reader["serverversion"]),
                Deleted = Convert.ToInt32(reader["deleted"]),
                ConflictData = Convert.IsDBNull(reader["conflictdata"]) ? string.Empty : reader["conflictdata"]?.ToString(),
                DataHash = Convert.IsDBNull(reader["datahash"]) ? string.Empty : reader["datahash"]?.ToString()
            };
        }

        protected override string GetInsertSql()
        {
            return @"
                INSERT INTO Tbl_BishopScore (
                    ID, PartographID, Time, Dilation, Effacement, Consistency, Position, Station, TotalScore, DilationCm, EffacementPercent, CervicalConsistency, CervicalPosition, StationValue, Interpretation, FavorableForDelivery, Notes, Handler, createdtime, updatedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash) VALUES (@id, @partographid, @time, @dilation, @effacement, @consistency, @position, @station, @totalscore, @dilationcm, @effacementpercent, @cervicalconsistency, @cervicalposition, @stationvalue, @interpretation, @favorablefordelivery, @notes, @handler, @createdtime, @updatedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted, @datahash)";
        }

        protected override string GetUpdateSql()
        {
            return @"
                UPDATE Tbl_BishopScore SET
                    Time = @time, Dilation = @dilation, Effacement = @effacement, Consistency = @consistency, Position = @position, Station = @station, TotalScore = @totalscore, DilationCm = @dilationcm, EffacementPercent = @effacementpercent, CervicalConsistency = @cervicalconsistency, CervicalPosition = @cervicalposition, StationValue = @stationvalue, Interpretation = @interpretation, FavorableForDelivery = @favorablefordelivery, Notes = @notes, Handler = @handler, updatedtime = @updatedtime, syncstatus = 0, version = @version, datahash = @datahash WHERE ID = @id";
        }

        protected override void AddInsertParameters(SqliteCommand cmd, BishopScore item)
        {
            //item.ID = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            item.ID = Guid.NewGuid();
            item.CreatedTime = now;
            item.UpdatedTime = now;
            item.DeviceId = DeviceIdentity.GetOrCreateDeviceId();
            item.OriginDeviceId = DeviceIdentity.GetOrCreateDeviceId();
            item.Version = 1;
            item.ServerVersion = 0;
            item.SyncStatus = 0;
            item.Deleted = 0;
            item.DataHash = item.CalculateHash();

            AddCommonParameters(cmd, item);
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, BishopScore item)
        {
            item.UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            item.Version++;
            item.DataHash = item.CalculateHash();

            AddCommonParameters(cmd, item);
        }

        private void AddCommonParameters(SqliteCommand cmd, BishopScore item)
        {
            cmd.Parameters.AddWithValue("@id", item.ID.ToString());
            cmd.Parameters.AddWithValue("@partographid", item.PartographID?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@time", item.Time.ToString("o"));
            cmd.Parameters.AddWithValue("@dilation", item.Dilation);
            cmd.Parameters.AddWithValue("@effacement", item.Effacement);
            cmd.Parameters.AddWithValue("@consistency", item.Consistency);
            cmd.Parameters.AddWithValue("@position", item.Position);
            cmd.Parameters.AddWithValue("@station", item.Station);
            cmd.Parameters.AddWithValue("@totalscore", item.TotalScore);
            cmd.Parameters.AddWithValue("@dilationcm", item.DilationCm.HasValue ? item.DilationCm.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@effacementpercent", item.EffacementPercent.HasValue ? item.EffacementPercent.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@cervicalconsistency", item.CervicalConsistency ?? string.Empty);
            cmd.Parameters.AddWithValue("@cervicalposition", item.CervicalPosition ?? string.Empty);
            cmd.Parameters.AddWithValue("@stationvalue", item.StationValue.HasValue ? item.StationValue.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@interpretation", item.Interpretation ?? string.Empty);
            cmd.Parameters.AddWithValue("@favorablefordelivery", item.FavorableForDelivery ? 1 : 0);
            cmd.Parameters.AddWithValue("@notes", item.Notes ?? string.Empty);
            cmd.Parameters.AddWithValue("@handlername", item.HandlerName ?? string.Empty);
            cmd.Parameters.AddWithValue("@handler", item.Handler?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@createdtime", item.CreatedTime);
            cmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            cmd.Parameters.AddWithValue("@deviceid", item.DeviceId ?? string.Empty);
            cmd.Parameters.AddWithValue("@origindeviceid", item.OriginDeviceId ?? string.Empty);
            cmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            cmd.Parameters.AddWithValue("@version", item.Version);
            cmd.Parameters.AddWithValue("@serverversion", item.ServerVersion);
            cmd.Parameters.AddWithValue("@deleted", item.Deleted);
            cmd.Parameters.AddWithValue("@datahash", item.DataHash ?? string.Empty);
        }
    }
}
