using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class OralFluidRepository : BasePartographRepository<OralFluidEntry>
    {
        protected override string TableName => "OralFluidEntry";
        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS OralFluidEntry (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                PatientID INTEGER NOT NULL,
                RecordedTime TEXT NOT NULL,
                RecordedBy TEXT,
                Notes TEXT,
                FluidType TEXT,
                AmountMl INTEGER,
                Tolerated INTEGER NOT NULL,
                Vomiting INTEGER NOT NULL,
                Restrictions TEXT
            );";

        public OralFluidRepository(ILogger<OralFluidRepository> logger) : base(logger) { }

        protected override OralFluidEntry MapFromReader(SqliteDataReader reader)
        {
            return new OralFluidEntry
            {
                ID = reader.GetInt32(0),
                PatientID = reader.GetInt32(1),
                RecordedTime = DateTime.Parse(reader.GetString(2)),
                RecordedBy = reader.GetString(3),
                Notes = reader.GetString(4),
                FluidType = reader.GetString(5),
                AmountMl = reader.GetInt32(6),
                Tolerated = reader.GetBoolean(7),
                Vomiting = reader.GetBoolean(8),
                Restrictions = reader.GetString(9)
            };
        }

        protected override string GetInsertSql() => @"
            INSERT INTO OralFluidEntry (PatientID, RecordedTime, RecordedBy, Notes, FluidType, 
                AmountMl, Tolerated, Vomiting, Restrictions)
            VALUES (@PatientID, @RecordedTime, @RecordedBy, @Notes, @FluidType, 
                @AmountMl, @Tolerated, @Vomiting, @Restrictions);
            SELECT last_insert_rowid();";

        protected override string GetUpdateSql() => @"
            UPDATE OralFluidEntry SET RecordedTime = @RecordedTime, RecordedBy = @RecordedBy, Notes = @Notes,
                FluidType = @FluidType, AmountMl = @AmountMl, Tolerated = @Tolerated,
                Vomiting = @Vomiting, Restrictions = @Restrictions
            WHERE ID = @ID";

        protected override void AddInsertParameters(SqliteCommand cmd, OralFluidEntry item)
        {
            cmd.Parameters.AddWithValue("@PatientID", item.PatientID);
            cmd.Parameters.AddWithValue("@RecordedTime", item.RecordedTime.ToString("O"));
            cmd.Parameters.AddWithValue("@RecordedBy", item.RecordedBy ?? "");
            cmd.Parameters.AddWithValue("@Notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@FluidType", item.FluidType ?? "");
            cmd.Parameters.AddWithValue("@AmountMl", item.AmountMl);
            cmd.Parameters.AddWithValue("@Tolerated", item.Tolerated);
            cmd.Parameters.AddWithValue("@Vomiting", item.Vomiting);
            cmd.Parameters.AddWithValue("@Restrictions", item.Restrictions ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, OralFluidEntry item)
        {
            AddInsertParameters(cmd, item);
            cmd.Parameters.AddWithValue("@ID", item.ID);
        }
    }
}
