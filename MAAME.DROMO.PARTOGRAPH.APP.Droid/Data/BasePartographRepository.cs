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
    // Generic Repository for Partograph Measurements
    public abstract class BasePartographRepository<T> where T : BasePartographMeasurement, new()
    {
        private bool _hasBeenInitialized = false;
        protected readonly ILogger _logger;
        protected abstract string TableName { get; }
        protected abstract string CreateTableSql { get; }

        protected BasePartographRepository(ILogger logger)
        {
            _logger = logger;
        }

        protected virtual async Task Init()
        {
            if (_hasBeenInitialized)
                return;

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = CreateTableSql;
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error creating {TableName} table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public virtual async Task<List<T>> ListByPatientAsync(int patientId)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = $"SELECT * FROM {TableName} WHERE PatientID = @patientId ORDER BY RecordedTime DESC";
            selectCmd.Parameters.AddWithValue("@patientId", patientId);

            var entries = new List<T>();
            await using var reader = await selectCmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                entries.Add(MapFromReader(reader));
            }

            return entries;
        }

        public virtual async Task<T?> GetLatestByPatientAsync(int patientId)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = $"SELECT * FROM {TableName} WHERE PatientID = @patientId ORDER BY RecordedTime DESC LIMIT 1";
            selectCmd.Parameters.AddWithValue("@patientId", patientId);

            await using var reader = await selectCmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }

            return null;
        }

        public virtual async Task<int> SaveItemAsync(T item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();
            if (item.ID == 0)
            {
                saveCmd.CommandText = GetInsertSql();
                AddInsertParameters(saveCmd, item);
                var result = await saveCmd.ExecuteScalarAsync();
                item.ID = Convert.ToInt32(result);
            }
            else
            {
                saveCmd.CommandText = GetUpdateSql();
                AddUpdateParameters(saveCmd, item);
                await saveCmd.ExecuteNonQueryAsync();
            }

            return item.ID;
        }

        protected abstract T MapFromReader(SqliteDataReader reader);
        protected abstract string GetInsertSql();
        protected abstract string GetUpdateSql();
        protected abstract void AddInsertParameters(SqliteCommand cmd, T item);
        protected abstract void AddUpdateParameters(SqliteCommand cmd, T item);
    }

    // Companion Repository
    public class CompanionRepository : BasePartographRepository<CompanionEntry>
    {
        protected override string TableName => "CompanionEntry";
        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS CompanionEntry (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                PatientID INTEGER NOT NULL,
                RecordedTime TEXT NOT NULL,
                RecordedBy TEXT,
                Notes TEXT,
                HasCompanion INTEGER NOT NULL,
                CompanionName TEXT,
                CompanionRelationship TEXT,
                CompanionPresent INTEGER NOT NULL,
                CompanionSupport TEXT
            );";

        public CompanionRepository(ILogger<CompanionRepository> logger) : base(logger) { }

        protected override CompanionEntry MapFromReader(SqliteDataReader reader)
        {
            return new CompanionEntry
            {
                ID = reader.GetInt32(0),
                PatientID = reader.GetInt32(1),
                RecordedTime = DateTime.Parse(reader.GetString(2)),
                RecordedBy = reader.GetString(3),
                Notes = reader.GetString(4),
                HasCompanion = reader.GetBoolean(5),
                CompanionName = reader.GetString(6),
                CompanionRelationship = reader.GetString(7),
                CompanionPresent = reader.GetBoolean(8),
                CompanionSupport = reader.GetString(9)
            };
        }

        protected override string GetInsertSql() => @"
            INSERT INTO CompanionEntry (PatientID, RecordedTime, RecordedBy, Notes, HasCompanion, 
                CompanionName, CompanionRelationship, CompanionPresent, CompanionSupport)
            VALUES (@PatientID, @RecordedTime, @RecordedBy, @Notes, @HasCompanion, 
                @CompanionName, @CompanionRelationship, @CompanionPresent, @CompanionSupport);
            SELECT last_insert_rowid();";

        protected override string GetUpdateSql() => @"
            UPDATE CompanionEntry SET RecordedTime = @RecordedTime, RecordedBy = @RecordedBy, Notes = @Notes,
                HasCompanion = @HasCompanion, CompanionName = @CompanionName, CompanionRelationship = @CompanionRelationship,
                CompanionPresent = @CompanionPresent, CompanionSupport = @CompanionSupport
            WHERE ID = @ID";

        protected override void AddInsertParameters(SqliteCommand cmd, CompanionEntry item)
        {
            cmd.Parameters.AddWithValue("@PatientID", item.PatientID);
            cmd.Parameters.AddWithValue("@RecordedTime", item.RecordedTime.ToString("O"));
            cmd.Parameters.AddWithValue("@RecordedBy", item.RecordedBy ?? "");
            cmd.Parameters.AddWithValue("@Notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@HasCompanion", item.HasCompanion);
            cmd.Parameters.AddWithValue("@CompanionName", item.CompanionName ?? "");
            cmd.Parameters.AddWithValue("@CompanionRelationship", item.CompanionRelationship ?? "");
            cmd.Parameters.AddWithValue("@CompanionPresent", item.CompanionPresent);
            cmd.Parameters.AddWithValue("@CompanionSupport", item.CompanionSupport ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, CompanionEntry item)
        {
            AddInsertParameters(cmd, item);
            cmd.Parameters.AddWithValue("@ID", item.ID);
        }
    }

    // Pain Relief Repository
    public class PainReliefRepository : BasePartographRepository<PainReliefEntry>
    {
        protected override string TableName => "PainReliefEntry";
        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS PainReliefEntry (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                PatientID INTEGER NOT NULL,
                RecordedTime TEXT NOT NULL,
                RecordedBy TEXT,
                Notes TEXT,
                PainLevel TEXT,
                PainReliefMethod TEXT,
                AdministeredTime TEXT,
                Dose TEXT,
                Effectiveness TEXT,
                SideEffects INTEGER NOT NULL,
                SideEffectsDescription TEXT
            );";

        public PainReliefRepository(ILogger<PainReliefRepository> logger) : base(logger) { }

        protected override PainReliefEntry MapFromReader(SqliteDataReader reader)
        {
            return new PainReliefEntry
            {
                ID = reader.GetInt32(0),
                PatientID = reader.GetInt32(1),
                RecordedTime = DateTime.Parse(reader.GetString(2)),
                RecordedBy = reader.GetString(3),
                Notes = reader.GetString(4),
                PainLevel = reader.GetString(5),
                PainReliefMethod = reader.GetString(6),
                AdministeredTime = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7)),
                Dose = reader.GetString(8),
                Effectiveness = reader.GetString(9),
                SideEffects = reader.GetBoolean(10),
                SideEffectsDescription = reader.GetString(11)
            };
        }

        protected override string GetInsertSql() => @"
            INSERT INTO PainReliefEntry (PatientID, RecordedTime, RecordedBy, Notes, PainLevel, 
                PainReliefMethod, AdministeredTime, Dose, Effectiveness, SideEffects, SideEffectsDescription)
            VALUES (@PatientID, @RecordedTime, @RecordedBy, @Notes, @PainLevel, 
                @PainReliefMethod, @AdministeredTime, @Dose, @Effectiveness, @SideEffects, @SideEffectsDescription);
            SELECT last_insert_rowid();";

        protected override string GetUpdateSql() => @"
            UPDATE PainReliefEntry SET RecordedTime = @RecordedTime, RecordedBy = @RecordedBy, Notes = @Notes,
                PainLevel = @PainLevel, PainReliefMethod = @PainReliefMethod, AdministeredTime = @AdministeredTime,
                Dose = @Dose, Effectiveness = @Effectiveness, SideEffects = @SideEffects, SideEffectsDescription = @SideEffectsDescription
            WHERE ID = @ID";

        protected override void AddInsertParameters(SqliteCommand cmd, PainReliefEntry item)
        {
            cmd.Parameters.AddWithValue("@PatientID", item.PatientID);
            cmd.Parameters.AddWithValue("@RecordedTime", item.RecordedTime.ToString("O"));
            cmd.Parameters.AddWithValue("@RecordedBy", item.RecordedBy ?? "");
            cmd.Parameters.AddWithValue("@Notes", item.Notes ?? "");
            cmd.Parameters.AddWithValue("@PainLevel", item.PainLevel ?? "0");
            cmd.Parameters.AddWithValue("@PainReliefMethod", item.PainReliefMethod ?? "");
            cmd.Parameters.AddWithValue("@AdministeredTime", item.AdministeredTime?.ToString("O") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Dose", item.Dose ?? "");
            cmd.Parameters.AddWithValue("@Effectiveness", item.Effectiveness ?? "");
            cmd.Parameters.AddWithValue("@SideEffects", item.SideEffects);
            cmd.Parameters.AddWithValue("@SideEffectsDescription", item.SideEffectsDescription ?? "");
        }

        protected override void AddUpdateParameters(SqliteCommand cmd, PainReliefEntry item)
        {
            AddInsertParameters(cmd, item);
            cmd.Parameters.AddWithValue("@ID", item.ID);
        }
    }

    // Assessment Plan Repository
    public class AssessmentPlanRepository : BasePartographRepository<AssessmentPlanEntry>
    {
        protected override string TableName => "AssessmentPlanEntry";
        protected override string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS AssessmentPlanEntry (
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
                ID = reader.GetInt32(0),
                PatientID = reader.GetInt32(1),
                RecordedTime = DateTime.Parse(reader.GetString(2)),
                RecordedBy = reader.GetString(3),
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
            cmd.Parameters.AddWithValue("@PatientID", item.PatientID);
            cmd.Parameters.AddWithValue("@RecordedTime", item.RecordedTime.ToString("O"));
            cmd.Parameters.AddWithValue("@RecordedBy", item.RecordedBy ?? "");
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