using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Assessment Plan Repository
    public class AssessmentPlanRepository : BasePartographRepository<AssessmentPlanEntry>
    {
        protected override string TableName => "AssessmentPlanEntry";
        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS TBL_AssessmentPlan (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                PatientID INTEGER NOT NULL,
                RecordedTime TEXT NOT NULL,
                RecordedBy TEXT,
                Notes TEXT,
                LaborProgress TEXT,
                FetalWellbeing TEXT,
                MaternalCondition TEXT,
                RiskFactors TEXT,
                Complications TEXT,
                Plan TEXT,
                ExpectedDelivery TEXT,
                RequiresIntervention INTEGER NOT NULL,
                InterventionRequired TEXT,
                NextAssessment TEXT,
                AssessedBy TEXT
            );";

        public AssessmentPlanRepository(ILogger<AssessmentPlanRepository> logger) : base(logger) { }

        protected override AssessmentPlanEntry MapFromReader(SqliteDataReader reader)
        {
            return new AssessmentPlanEntry
            {
                ID = Guid.Parse(reader.GetString(0)),
                PartographID = reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                Time = DateTime.Parse(reader.GetString(2)),
                HandlerName = reader.GetString(3),
                Notes = reader.GetString(4),
                LaborProgress = reader.GetString(5),
                FetalWellbeing = reader.GetString(6),
                MaternalCondition = reader.GetString(7),
                RiskFactors = reader.GetString(8),
                Complications = reader.GetString(9),
                Plan = reader.GetString(10),
                ExpectedDelivery = reader.GetString(11),
                RequiresIntervention = reader.GetBoolean(12),
                InterventionRequired = reader.GetString(13),
                NextAssessment = reader.IsDBNull(14) ? null : DateTime.Parse(reader.GetString(14)),
                AssessedBy = reader.GetString(15)
            };
        }

        protected override string GetInsertSql() => @"
            INSERT INTO AssessmentPlanEntry (PatientID, RecordedTime, RecordedBy, Notes, LaborProgress, 
                FetalWellbeing, MaternalCondition, RiskFactors, Complications, Plan, ExpectedDelivery,
                RequiresIntervention, InterventionRequired, NextAssessment, AssessedBy)
            VALUES (@PatientID, @RecordedTime, @RecordedBy, @Notes, @LaborProgress, 
                @FetalWellbeing, @MaternalCondition, @RiskFactors, @Complications, @Plan, @ExpectedDelivery,
                @RequiresIntervention, @InterventionRequired, @NextAssessment, @AssessedBy);
            SELECT last_insert_rowid();";

        protected override string GetUpdateSql() => @"
            UPDATE AssessmentPlanEntry SET RecordedTime = @RecordedTime, RecordedBy = @RecordedBy, Notes = @Notes,
                LaborProgress = @LaborProgress, FetalWellbeing = @FetalWellbeing, MaternalCondition = @MaternalCondition,
                RiskFactors = @RiskFactors, Complications = @Complications, Plan = @Plan, ExpectedDelivery = @ExpectedDelivery,
                RequiresIntervention = @RequiresIntervention, InterventionRequired = @InterventionRequired,
                NextAssessment = @NextAssessment, AssessedBy = @AssessedBy
            WHERE ID = @ID";

        protected override void AddInsertParameters(SqliteCommand cmd, AssessmentPlanEntry item)
        {
            cmd.Parameters.AddWithValue("@PatientID", item.PartographID);
            cmd.Parameters.AddWithValue("@RecordedTime", item.Time.ToString("O"));
            cmd.Parameters.AddWithValue("@RecordedBy", item.HandlerName ?? "");
            cmd.Parameters.AddWithValue("@Notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@LaborProgress", item.LaborProgress ?? "");
            cmd.Parameters.AddWithValue("@FetalWellbeing", item.FetalWellbeing ?? "");
            cmd.Parameters.AddWithValue("@MaternalCondition", item.MaternalCondition ?? "");
            cmd.Parameters.AddWithValue("@RiskFactors", item.RiskFactors ?? "");
            cmd.Parameters.AddWithValue("@Complications", item.Complications ?? "");
            cmd.Parameters.AddWithValue("@Plan", item.Plan ?? "");
            cmd.Parameters.AddWithValue("@ExpectedDelivery", item.ExpectedDelivery ?? "");
            cmd.Parameters.AddWithValue("@RequiresIntervention", item.RequiresIntervention);
            cmd.Parameters.AddWithValue("@InterventionRequired", item.InterventionRequired ?? "");
            cmd.Parameters.AddWithValue("@NextAssessment", item.NextAssessment?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AssessedBy", item.AssessedBy ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, AssessmentPlanEntry item)
        {
            AddInsertParameters(cmd, item);
            cmd.Parameters.AddWithValue("@ID", item.ID);
        }
    }
}