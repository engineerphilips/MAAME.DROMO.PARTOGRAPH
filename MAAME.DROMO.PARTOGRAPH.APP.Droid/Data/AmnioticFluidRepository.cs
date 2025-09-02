using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Continue with other repositories...
    // Amniotic Fluid Repository
    public class AmnioticFluidRepository : BasePartographRepository<AmnioticFluidEntry>
    {
        protected override string TableName => "AmnioticFluidEntry";
        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS AmnioticFluidEntry (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                PatientID INTEGER NOT NULL,
                RecordedTime TEXT NOT NULL,
                RecordedBy TEXT,
                Notes TEXT,
                Color TEXT,
                Consistency TEXT,
                Amount TEXT,
                Odor TEXT,
                MeconiumStaining INTEGER NOT NULL,
                MeconiumGrade TEXT
            );";

        public AmnioticFluidRepository(ILogger<AmnioticFluidRepository> logger) : base(logger) { }

        protected override AmnioticFluidEntry MapFromReader(SqliteDataReader reader)
        {
            return new AmnioticFluidEntry
            {
                ID = reader.GetInt32(0),
                PatientID = reader.GetInt32(1),
                RecordedTime = DateTime.Parse(reader.GetString(2)),
                RecordedBy = reader.GetString(3),
                Notes = reader.GetString(4),
                Color = reader.GetString(5),
                Consistency = reader.GetString(6),
                Amount = reader.GetString(7),
                Odor = reader.GetString(8),
                MeconiumStaining = reader.GetBoolean(9),
                MeconiumGrade = reader.GetString(10)
            };
        }

        protected override string GetInsertSql() => @"
            INSERT INTO AmnioticFluidEntry (PatientID, RecordedTime, RecordedBy, Notes, Color, 
                Consistency, Amount, Odor, MeconiumStaining, MeconiumGrade)
            VALUES (@PatientID, @RecordedTime, @RecordedBy, @Notes, @Color, 
                @Consistency, @Amount, @Odor, @MeconiumStaining, @MeconiumGrade);
            SELECT last_insert_rowid();";

        protected override string GetUpdateSql() => @"
            UPDATE AmnioticFluidEntry SET RecordedTime = @RecordedTime, RecordedBy = @RecordedBy, Notes = @Notes,
                Color = @Color, Consistency = @Consistency, Amount = @Amount,
                Odor = @Odor, MeconiumStaining = @MeconiumStaining, MeconiumGrade = @MeconiumGrade
            WHERE ID = @ID";

        protected override void AddInsertParameters(SqliteCommand cmd, AmnioticFluidEntry item)
        {
            cmd.Parameters.AddWithValue("@PatientID", item.PatientID);
            cmd.Parameters.AddWithValue("@RecordedTime", item.RecordedTime.ToString("O"));
            cmd.Parameters.AddWithValue("@RecordedBy", item.RecordedBy ?? "");
            cmd.Parameters.AddWithValue("@Notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@Color", item.Color ?? "Clear");
            cmd.Parameters.AddWithValue("@Consistency", item.Consistency ?? "Normal");
            cmd.Parameters.AddWithValue("@Amount", item.Amount ?? "Normal");
            cmd.Parameters.AddWithValue("@Odor", item.Odor ?? "None");
            cmd.Parameters.AddWithValue("@MeconiumStaining", item.MeconiumStaining);
            cmd.Parameters.AddWithValue("@MeconiumGrade", item.MeconiumGrade ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, AmnioticFluidEntry item)
        {
            AddInsertParameters(cmd, item);
            cmd.Parameters.AddWithValue("@ID", item.ID);
        }
    }
}
