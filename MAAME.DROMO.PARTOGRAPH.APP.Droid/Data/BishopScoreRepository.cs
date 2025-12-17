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
                RecordedBy TEXT,
                HandlerName TEXT,
                Handler TEXT,
                FOREIGN KEY (PartographID) REFERENCES Tbl_Partograph(ID)
            );

            CREATE INDEX IF NOT EXISTS idx_bishopscore_partographid ON Tbl_BishopScore(PartographID);
            CREATE INDEX IF NOT EXISTS idx_bishopscore_time ON Tbl_BishopScore(Time);
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
                RecordedBy = reader["RecordedBy"]?.ToString() ?? string.Empty,
                HandlerName = reader["staffname"]?.ToString() ?? string.Empty,
                Handler = reader["Handler"] == DBNull.Value ? null : Guid.Parse(reader["Handler"].ToString())
            };
        }

        protected override string GetInsertSql()
        {
            return @"
                INSERT INTO Tbl_BishopScore (
                    ID, PartographID, Time, Dilation, Effacement, Consistency, Position, Station,
                    TotalScore, DilationCm, EffacementPercent, CervicalConsistency, CervicalPosition,
                    StationValue, Interpretation, FavorableForDelivery, Notes, RecordedBy, HandlerName, Handler
                ) VALUES (
                    @id, @partographid, @time, @dilation, @effacement, @consistency, @position, @station,
                    @totalscore, @dilationcm, @effacementpercent, @cervicalconsistency, @cervicalposition,
                    @stationvalue, @interpretation, @favorablefordelivery, @notes, @recordedby, @handlername, @handler
                )";
        }

        protected override string GetUpdateSql()
        {
            return @"
                UPDATE Tbl_BishopScore SET
                    Time = @time, Dilation = @dilation, Effacement = @effacement, Consistency = @consistency,
                    Position = @position, Station = @station, TotalScore = @totalscore,
                    DilationCm = @dilationcm, EffacementPercent = @effacementpercent,
                    CervicalConsistency = @cervicalconsistency, CervicalPosition = @cervicalposition,
                    StationValue = @stationvalue, Interpretation = @interpretation,
                    FavorableForDelivery = @favorablefordelivery, Notes = @notes, RecordedBy = @recordedby,
                    HandlerName = @handlername, Handler = @handler
                WHERE ID = @id";
        }

        protected override void AddInsertParameters(SqliteCommand cmd, BishopScore item)
        {
            item.ID = Guid.NewGuid();
            AddCommonParameters(cmd, item);
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, BishopScore item)
        {
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
            cmd.Parameters.AddWithValue("@recordedby", item.RecordedBy ?? string.Empty);
            cmd.Parameters.AddWithValue("@handlername", item.HandlerName ?? string.Empty);
            cmd.Parameters.AddWithValue("@handler", item.Handler?.ToString() ?? (object)DBNull.Value);
        }
    }
}
