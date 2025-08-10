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
    public class PartographEntryRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;

        public PartographEntryRepository(ILogger<PartographEntryRepository> logger)
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
                CREATE TABLE IF NOT EXISTS PartographEntry (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PatientID INTEGER NOT NULL,
                    RecordedTime TEXT NOT NULL,
                    CervicalDilation INTEGER NOT NULL,
                    DescentOfHead TEXT,
                    ContractionsPerTenMinutes INTEGER,
                    ContractionDuration INTEGER,
                    ContractionStrength TEXT,
                    FetalHeartRate INTEGER,
                    LiquorStatus TEXT,
                    Moulding TEXT,
                    Caput TEXT,
                    MedicationsGiven TEXT,
                    OxytocinUnits TEXT,
                    IVFluids TEXT,
                    RecordedBy TEXT,
                    Notes TEXT
                );";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating PartographEntry table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<List<PartographEntry>> ListByPatientAsync(int patientId)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM PartographEntry WHERE PatientID = @patientId ORDER BY RecordedTime DESC";
            selectCmd.Parameters.AddWithValue("@patientId", patientId);

            var entries = new List<PartographEntry>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                entries.Add(new PartographEntry
                {
                    ID = reader.GetInt32(0),
                    PatientID = reader.GetInt32(1),
                    RecordedTime = DateTime.Parse(reader.GetString(2)),
                    CervicalDilation = reader.GetInt32(3),
                    DescentOfHead = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    ContractionsPerTenMinutes = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                    ContractionDuration = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                    ContractionStrength = reader.IsDBNull(7) ? "Moderate" : reader.GetString(7),
                    FetalHeartRate = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                    LiquorStatus = reader.IsDBNull(9) ? "Clear" : reader.GetString(9),
                    Moulding = reader.IsDBNull(10) ? "None" : reader.GetString(10),
                    Caput = reader.IsDBNull(11) ? "None" : reader.GetString(11),
                    MedicationsGiven = reader.IsDBNull(12) ? "" : reader.GetString(12),
                    OxytocinUnits = reader.IsDBNull(13) ? "" : reader.GetString(13),
                    IVFluids = reader.IsDBNull(14) ? "" : reader.GetString(14),
                    RecordedBy = reader.IsDBNull(15) ? "" : reader.GetString(15),
                    Notes = reader.IsDBNull(16) ? "" : reader.GetString(16)
                });
            }

            return entries;
        }

        public async Task<int> SaveItemAsync(PartographEntry item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();
            if (item.ID == 0)
            {
                saveCmd.CommandText = @"
                INSERT INTO PartographEntry (PatientID, RecordedTime, CervicalDilation, DescentOfHead,
                    ContractionsPerTenMinutes, ContractionDuration, ContractionStrength, FetalHeartRate,
                    LiquorStatus, Moulding, Caput, MedicationsGiven, OxytocinUnits, IVFluids, RecordedBy, Notes)
                VALUES (@PatientID, @RecordedTime, @CervicalDilation, @DescentOfHead,
                    @ContractionsPerTenMinutes, @ContractionDuration, @ContractionStrength, @FetalHeartRate,
                    @LiquorStatus, @Moulding, @Caput, @MedicationsGiven, @OxytocinUnits, @IVFluids, @RecordedBy, @Notes);
                SELECT last_insert_rowid();";
            }
            else
            {
                saveCmd.CommandText = @"
                UPDATE PartographEntry SET 
                    RecordedTime = @RecordedTime, CervicalDilation = @CervicalDilation,
                    DescentOfHead = @DescentOfHead, ContractionsPerTenMinutes = @ContractionsPerTenMinutes,
                    ContractionDuration = @ContractionDuration, ContractionStrength = @ContractionStrength,
                    FetalHeartRate = @FetalHeartRate, LiquorStatus = @LiquorStatus,
                    Moulding = @Moulding, Caput = @Caput, MedicationsGiven = @MedicationsGiven,
                    OxytocinUnits = @OxytocinUnits, IVFluids = @IVFluids, RecordedBy = @RecordedBy, Notes = @Notes
                WHERE ID = @ID";
                saveCmd.Parameters.AddWithValue("@ID", item.ID);
            }

            saveCmd.Parameters.AddWithValue("@PatientID", item.PatientID);
            saveCmd.Parameters.AddWithValue("@RecordedTime", item.RecordedTime.ToString("O"));
            saveCmd.Parameters.AddWithValue("@CervicalDilation", item.CervicalDilation);
            saveCmd.Parameters.AddWithValue("@DescentOfHead", item.DescentOfHead ?? "");
            saveCmd.Parameters.AddWithValue("@ContractionsPerTenMinutes", item.ContractionsPerTenMinutes);
            saveCmd.Parameters.AddWithValue("@ContractionDuration", item.ContractionDuration);
            saveCmd.Parameters.AddWithValue("@ContractionStrength", item.ContractionStrength);
            saveCmd.Parameters.AddWithValue("@FetalHeartRate", item.FetalHeartRate);
            saveCmd.Parameters.AddWithValue("@LiquorStatus", item.LiquorStatus);
            saveCmd.Parameters.AddWithValue("@Moulding", item.Moulding);
            saveCmd.Parameters.AddWithValue("@Caput", item.Caput);
            saveCmd.Parameters.AddWithValue("@MedicationsGiven", item.MedicationsGiven ?? "");
            saveCmd.Parameters.AddWithValue("@OxytocinUnits", item.OxytocinUnits ?? "");
            saveCmd.Parameters.AddWithValue("@IVFluids", item.IVFluids ?? "");
            saveCmd.Parameters.AddWithValue("@RecordedBy", item.RecordedBy ?? "");
            saveCmd.Parameters.AddWithValue("@Notes", item.Notes ?? "");

            var result = await saveCmd.ExecuteScalarAsync();
            if (item.ID == 0)
            {
                item.ID = Convert.ToInt32(result);
            }

            return item.ID;
        }
    }
}
