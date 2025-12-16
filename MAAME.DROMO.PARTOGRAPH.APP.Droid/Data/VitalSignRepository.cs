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
                var idOrdinal = reader.GetOrdinal("ID");
                var patientIdOrdinal = reader.GetOrdinal("PatientID");
                var recordedTimeOrdinal = reader.GetOrdinal("RecordedTime");
                var systolicBPOrdinal = reader.GetOrdinal("SystolicBP");
                var diastolicBPOrdinal = reader.GetOrdinal("DiastolicBP");
                var temperatureOrdinal = reader.GetOrdinal("Temperature");
                var pulseRateOrdinal = reader.GetOrdinal("PulseRate");
                var respiratoryRateOrdinal = reader.GetOrdinal("RespiratoryRate");
                var urineOutputOrdinal = reader.GetOrdinal("UrineOutput");
                var urineProteinOrdinal = reader.GetOrdinal("UrineProtein");
                var urineAcetoneOrdinal = reader.GetOrdinal("UrineAcetone");
                var recordedByOrdinal = reader.GetOrdinal("RecordedBy");

                vitalSigns.Add(new VitalSign
                {
                    ID = Guid.Parse(reader.GetString(idOrdinal)),
                    PatientID = reader.IsDBNull(patientIdOrdinal) ? null : Guid.Parse(reader.GetString(patientIdOrdinal)),
                    RecordedTime = DateTime.Parse(reader.GetString(recordedTimeOrdinal)),
                    SystolicBP = reader.IsDBNull(systolicBPOrdinal) ? 0 : reader.GetInt32(systolicBPOrdinal),
                    DiastolicBP = reader.IsDBNull(diastolicBPOrdinal) ? 0 : reader.GetInt32(diastolicBPOrdinal),
                    Temperature = reader.IsDBNull(temperatureOrdinal) ? 0 : reader.GetDecimal(temperatureOrdinal),
                    PulseRate = reader.IsDBNull(pulseRateOrdinal) ? 0 : reader.GetInt32(pulseRateOrdinal),
                    RespiratoryRate = reader.IsDBNull(respiratoryRateOrdinal) ? 0 : reader.GetInt32(respiratoryRateOrdinal),
                    UrineOutput = reader.IsDBNull(urineOutputOrdinal) ? "" : reader.GetString(urineOutputOrdinal),
                    UrineProtein = reader.IsDBNull(urineProteinOrdinal) ? "Nil" : reader.GetString(urineProteinOrdinal),
                    UrineAcetone = reader.IsDBNull(urineAcetoneOrdinal) ? "Nil" : reader.GetString(urineAcetoneOrdinal),
                    RecordedBy = reader.IsDBNull(recordedByOrdinal) ? "" : reader.GetString(recordedByOrdinal)
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
