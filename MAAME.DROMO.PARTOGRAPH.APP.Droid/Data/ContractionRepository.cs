using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Continue with more repositories...
    // Contractions Repository
    public class ContractionRepository : BasePartographRepository<ContractionEntry>
    {
        protected override string TableName => "ContractionEntry";
        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS ContractionEntry (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                PatientID INTEGER NOT NULL,
                RecordedTime TEXT NOT NULL,
                RecordedBy TEXT,
                Notes TEXT,
                FrequencyPer10Min INTEGER,
                DurationSeconds INTEGER,
                Strength TEXT,
                Regularity TEXT,
                PalpableAtRest INTEGER NOT NULL,
                EffectOnCervix TEXT,
                Coordinated INTEGER NOT NULL
            );";

        public ContractionRepository(ILogger<ContractionRepository> logger) : base(logger) { }

        protected override ContractionEntry MapFromReader(SqliteDataReader reader)
        {
            return new ContractionEntry
            {
                ID = reader.GetInt32(0),
                PatientID = reader.GetInt32(1),
                RecordedTime = DateTime.Parse(reader.GetString(2)),
                RecordedBy = reader.GetString(3),
                Notes = reader.GetString(4),
                FrequencyPer10Min = reader.GetInt32(5),
                DurationSeconds = reader.GetInt32(6),
                Strength = reader.GetString(7),
                Regularity = reader.GetString(8),
                PalpableAtRest = reader.GetBoolean(9),
                EffectOnCervix = reader.GetString(10),
                Coordinated = reader.GetBoolean(11)
            };
        }

        protected override string GetInsertSql() => @"
            INSERT INTO ContractionEntry (PatientID, RecordedTime, RecordedBy, Notes, FrequencyPer10Min, 
                DurationSeconds, Strength, Regularity, PalpableAtRest, EffectOnCervix, Coordinated)
            VALUES (@PatientID, @RecordedTime, @RecordedBy, @Notes, @FrequencyPer10Min, 
                @DurationSeconds, @Strength, @Regularity, @PalpableAtRest, @EffectOnCervix, @Coordinated);
            SELECT last_insert_rowid();";

        protected override string GetUpdateSql() => @"
            UPDATE ContractionEntry SET RecordedTime = @RecordedTime, RecordedBy = @RecordedBy, Notes = @Notes,
                FrequencyPer10Min = @FrequencyPer10Min, DurationSeconds = @DurationSeconds, Strength = @Strength,
                Regularity = @Regularity, PalpableAtRest = @PalpableAtRest, EffectOnCervix = @EffectOnCervix, Coordinated = @Coordinated
            WHERE ID = @ID";

        protected override void AddInsertParameters(SqliteCommand cmd, ContractionEntry item)
        {
            cmd.Parameters.AddWithValue("@PatientID", item.PatientID);
            cmd.Parameters.AddWithValue("@RecordedTime", item.RecordedTime.ToString("O"));
            cmd.Parameters.AddWithValue("@RecordedBy", item.RecordedBy ?? "");
            cmd.Parameters.AddWithValue("@Notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@FrequencyPer10Min", item.FrequencyPer10Min);
            cmd.Parameters.AddWithValue("@DurationSeconds", item.DurationSeconds);
            cmd.Parameters.AddWithValue("@Strength", item.Strength ?? "Moderate");
            cmd.Parameters.AddWithValue("@Regularity", item.Regularity ?? "Regular");
            cmd.Parameters.AddWithValue("@PalpableAtRest", item.PalpableAtRest);
            cmd.Parameters.AddWithValue("@EffectOnCervix", item.EffectOnCervix ?? "");
            cmd.Parameters.AddWithValue("@Coordinated", item.Coordinated);
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, ContractionEntry item)
        {
            AddInsertParameters(cmd, item);
            cmd.Parameters.AddWithValue("@ID", item.ID);
        }
    }
}
