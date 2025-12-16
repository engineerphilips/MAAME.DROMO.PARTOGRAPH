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
    public class VitalSignRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;

        public VitalSignRepository(ILogger<VitalSignRepository> logger)
        {
            _logger = logger;
        }

        private async Task Init()
        {
            if (_hasBeenInitialized)
                return;

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS VitalSign (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PatientID INTEGER NOT NULL,
                    RecordedTime TEXT NOT NULL,
                    SystolicBP INTEGER,
                    DiastolicBP INTEGER,
                    Temperature REAL,
                    PulseRate INTEGER,
                    RespiratoryRate INTEGER,
                    UrineOutput TEXT,
                    UrineProtein TEXT,
                    UrineAcetone TEXT,
                    RecordedBy TEXT
                );";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating VitalSign table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<List<VitalSign>> ListByPatientAsync(Guid? patientId)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM VitalSign WHERE PatientID = @patientId ORDER BY RecordedTime DESC";
            selectCmd.Parameters.AddWithValue("@patientId", patientId);

            var vitalSigns = new List<VitalSign>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                vitalSigns.Add(new VitalSign
                {
                    ID = Guid.Parse((string)reader["ID"]),
                    PatientID = reader["PatientID"] is DBNull ? null : Guid.Parse((string)reader["PatientID"]),
                    RecordedTime = DateTime.Parse((string)reader["RecordedTime"]),
                    SystolicBP = reader["SystolicBP"] is DBNull ? 0 : Convert.ToInt32(reader["SystolicBP"]),
                    DiastolicBP = reader["DiastolicBP"] is DBNull ? 0 : Convert.ToInt32(reader["DiastolicBP"]),
                    Temperature = reader["Temperature"] is DBNull ? 0 : Convert.ToDecimal(reader["Temperature"]),
                    PulseRate = reader["PulseRate"] is DBNull ? 0 : Convert.ToInt32(reader["PulseRate"]),
                    RespiratoryRate = reader["RespiratoryRate"] is DBNull ? 0 : Convert.ToInt32(reader["RespiratoryRate"]),
                    UrineOutput = reader["UrineOutput"] is DBNull ? "" : (string)reader["UrineOutput"],
                    UrineProtein = reader["UrineProtein"] is DBNull ? "Nil" : (string)reader["UrineProtein"],
                    UrineAcetone = reader["UrineAcetone"] is DBNull ? "Nil" : (string)reader["UrineAcetone"],
                    RecordedBy = reader["RecordedBy"] is DBNull ? "" : (string)reader["RecordedBy"]
                });
            }

            return vitalSigns;
        }

        public async Task<Guid?> SaveItemAsync(VitalSign item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();
            if (item.ID == null)
            {
                saveCmd.CommandText = @"
                INSERT INTO VitalSign (PatientID, RecordedTime, SystolicBP, DiastolicBP, Temperature,
                    PulseRate, RespiratoryRate, UrineOutput, UrineProtein, UrineAcetone, RecordedBy)
                VALUES (@PatientID, @RecordedTime, @SystolicBP, @DiastolicBP, @Temperature,
                    @PulseRate, @RespiratoryRate, @UrineOutput, @UrineProtein, @UrineAcetone, @RecordedBy);
                SELECT last_insert_rowid();";
            }
            else
            {
                saveCmd.CommandText = @"
                UPDATE VitalSign SET 
                    RecordedTime = @RecordedTime, SystolicBP = @SystolicBP, DiastolicBP = @DiastolicBP,
                    Temperature = @Temperature, PulseRate = @PulseRate, RespiratoryRate = @RespiratoryRate,
                    UrineOutput = @UrineOutput, UrineProtein = @UrineProtein, UrineAcetone = @UrineAcetone,
                    RecordedBy = @RecordedBy
                WHERE ID = @ID";
                saveCmd.Parameters.AddWithValue("@ID", item.ID);
            }

            saveCmd.Parameters.AddWithValue("@PatientID", item.PatientID);
            saveCmd.Parameters.AddWithValue("@RecordedTime", item.RecordedTime.ToString("O"));
            saveCmd.Parameters.AddWithValue("@SystolicBP", item.SystolicBP);
            saveCmd.Parameters.AddWithValue("@DiastolicBP", item.DiastolicBP);
            saveCmd.Parameters.AddWithValue("@Temperature", item.Temperature);
            saveCmd.Parameters.AddWithValue("@PulseRate", item.PulseRate);
            saveCmd.Parameters.AddWithValue("@RespiratoryRate", item.RespiratoryRate);
            saveCmd.Parameters.AddWithValue("@UrineOutput", item.UrineOutput ?? "");
            saveCmd.Parameters.AddWithValue("@UrineProtein", item.UrineProtein);
            saveCmd.Parameters.AddWithValue("@UrineAcetone", item.UrineAcetone);
            saveCmd.Parameters.AddWithValue("@RecordedBy", item.RecordedBy ?? "");

            var result = await saveCmd.ExecuteScalarAsync();
            if (item.ID == null)
            {
                item.ID = Guid.Parse(Convert.ToString(result));
            }

            return item.ID;
        }
    }
}
