using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Posture Repository
    public class PostureRepository : BasePartographRepository<PostureEntry>
    {
        protected override string TableName => "PostureEntry";
        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS PostureEntry (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                PatientID INTEGER NOT NULL,
                RecordedTime TEXT NOT NULL,
                RecordedBy TEXT,
                Notes TEXT,
                Position TEXT,
                Mobilizing INTEGER NOT NULL,
                MobilityLevel TEXT,
                UsingBirthBall INTEGER NOT NULL,
                UsingBirthPool INTEGER NOT NULL,
                ComfortMeasures TEXT
            );";

        public PostureRepository(ILogger<PostureRepository> logger) : base(logger) { }

        protected override PostureEntry MapFromReader(SqliteDataReader reader)
        {
            return new PostureEntry
            {
                ID = reader.GetInt32(0),
                PatientID = reader.GetInt32(1),
                RecordedTime = DateTime.Parse(reader.GetString(2)),
                RecordedBy = reader.GetString(3),
                Notes = reader.GetString(4),
                Position = reader.GetString(5),
                Mobilizing = reader.GetBoolean(6),
                MobilityLevel = reader.GetString(7),
                UsingBirthBall = reader.GetBoolean(8),
                UsingBirthPool = reader.GetBoolean(9),
                ComfortMeasures = reader.GetString(10)
            };
        }

        protected override string GetInsertSql() => @"
            INSERT INTO PostureEntry (PatientID, RecordedTime, RecordedBy, Notes, Position, 
                Mobilizing, MobilityLevel, UsingBirthBall, UsingBirthPool, ComfortMeasures)
            VALUES (@PatientID, @RecordedTime, @RecordedBy, @Notes, @Position, 
                @Mobilizing, @MobilityLevel, @UsingBirthBall, @UsingBirthPool, @ComfortMeasures);
            SELECT last_insert_rowid();";

        protected override string GetUpdateSql() => @"
            UPDATE PostureEntry SET RecordedTime = @RecordedTime, RecordedBy = @RecordedBy, Notes = @Notes,
                Position = @Position, Mobilizing = @Mobilizing, MobilityLevel = @MobilityLevel,
                UsingBirthBall = @UsingBirthBall, UsingBirthPool = @UsingBirthPool, ComfortMeasures = @ComfortMeasures
            WHERE ID = @ID";

        protected override void AddInsertParameters(SqliteCommand cmd, PostureEntry item)
        {
            cmd.Parameters.AddWithValue("@PatientID", item.PatientID);
            cmd.Parameters.AddWithValue("@RecordedTime", item.RecordedTime.ToString("O"));
            cmd.Parameters.AddWithValue("@RecordedBy", item.RecordedBy ?? "");
            cmd.Parameters.AddWithValue("@Notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@Position", item.Position ?? "");
            cmd.Parameters.AddWithValue("@Mobilizing", item.Mobilizing);
            cmd.Parameters.AddWithValue("@MobilityLevel", item.MobilityLevel ?? "");
            cmd.Parameters.AddWithValue("@UsingBirthBall", item.UsingBirthBall);
            cmd.Parameters.AddWithValue("@UsingBirthPool", item.UsingBirthPool);
            cmd.Parameters.AddWithValue("@ComfortMeasures", item.ComfortMeasures ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, PostureEntry item)
        {
            AddInsertParameters(cmd, item);
            cmd.Parameters.AddWithValue("@ID", item.ID);
        }
    }
}
