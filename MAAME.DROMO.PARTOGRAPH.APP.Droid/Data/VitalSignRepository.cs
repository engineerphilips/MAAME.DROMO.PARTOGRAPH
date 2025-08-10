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

        public async Task<List<VitalSign>> ListByPatientAsync(int patientId)
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
                    ID = reader.GetInt32(0),
                    PatientID = reader.GetInt32(1),
                    RecordedTime = DateTime.Parse(reader.GetString(2)),
                    SystolicBP = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    DiastolicBP = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    Temperature = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                    PulseRate = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                    RespiratoryRate = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                    UrineOutput = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    UrineProtein = reader.IsDBNull(9) ? "Nil" : reader.GetString(9),
                    UrineAcetone = reader.IsDBNull(10) ? "Nil" : reader.GetString(10),
                    RecordedBy = reader.IsDBNull(11) ? "" : reader.GetString(11)
                });
            }

            return vitalSigns;
        }

        public async Task<int> SaveItemAsync(VitalSign item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();
            if (item.ID == 0)
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
            if (item.ID == 0)
            {
                item.ID = Convert.ToInt32(result);
            }

            return item.ID;
        }
    }
}
