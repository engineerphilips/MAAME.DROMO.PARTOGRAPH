using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class PatientRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;
        private readonly PartographEntryRepository _partographRepository;
        private readonly VitalSignRepository _vitalSignRepository;

        public PatientRepository(PartographEntryRepository partographRepository,
            VitalSignRepository vitalSignRepository, ILogger<PatientRepository> logger)
        {
            _partographRepository = partographRepository;
            _vitalSignRepository = vitalSignRepository;
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
                CREATE TABLE IF NOT EXISTS Patient (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    HospitalNumber TEXT NOT NULL,
                    Age INTEGER NOT NULL,
                    Gravidity INTEGER NOT NULL,
                    Parity INTEGER NOT NULL,
                    AdmissionDate TEXT NOT NULL,
                    ExpectedDeliveryDate TEXT,
                    BloodGroup TEXT,
                    PhoneNumber TEXT,
                    EmergencyContact TEXT,
                    Status INTEGER NOT NULL,
                    LaborStartTime TEXT,
                    DeliveryTime TEXT,
                    CervicalDilationOnAdmission INTEGER,
                    MembraneStatus TEXT,
                    LiquorStatus TEXT,
                    RiskFactors TEXT,
                    Complications TEXT
                );";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating Patient table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<List<Patient>> ListAsync(LaborStatus? status = null)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            if (status.HasValue)
            {
                selectCmd.CommandText = "SELECT * FROM Patient WHERE Status = @status ORDER BY AdmissionDate DESC";
                selectCmd.Parameters.AddWithValue("@status", (int)status.Value);
            }
            else
            {
                selectCmd.CommandText = "SELECT * FROM Patient ORDER BY Status, AdmissionDate DESC";
            }

            var patients = new List<Patient>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var patient = new Patient
                {
                    ID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    HospitalNumber = reader.GetString(2),
                    Age = reader.GetInt32(3),
                    Gravidity = reader.GetInt32(4),
                    Parity = reader.GetInt32(5),
                    AdmissionDate = DateTime.Parse(reader.GetString(6)),
                    ExpectedDeliveryDate = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7)),
                    BloodGroup = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    PhoneNumber = reader.IsDBNull(9) ? "" : reader.GetString(9),
                    EmergencyContact = reader.IsDBNull(10) ? "" : reader.GetString(10),
                    Status = (LaborStatus)reader.GetInt32(11),
                    LaborStartTime = reader.IsDBNull(12) ? null : DateTime.Parse(reader.GetString(12)),
                    DeliveryTime = reader.IsDBNull(13) ? null : DateTime.Parse(reader.GetString(13)),
                    CervicalDilationOnAdmission = reader.IsDBNull(14) ? null : reader.GetInt32(14),
                    MembraneStatus = reader.IsDBNull(15) ? "Intact" : reader.GetString(15),
                    LiquorStatus = reader.IsDBNull(16) ? "Clear" : reader.GetString(16),
                    RiskFactors = reader.IsDBNull(17) ? "" : reader.GetString(17),
                    Complications = reader.IsDBNull(18) ? "" : reader.GetString(18)
                };

                // Load related data
                patient.PartographEntries = await _partographRepository.ListByPatientAsync(patient.ID);
                patient.VitalSigns = await _vitalSignRepository.ListByPatientAsync(patient.ID);

                patients.Add(patient);
            }

            return patients;
        }

        public async Task<Patient?> GetAsync(int id)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Patient WHERE ID = @id";
            selectCmd.Parameters.AddWithValue("@id", id);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var patient = new Patient
                {
                    ID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    HospitalNumber = reader.GetString(2),
                    Age = reader.GetInt32(3),
                    Gravidity = reader.GetInt32(4),
                    Parity = reader.GetInt32(5),
                    AdmissionDate = DateTime.Parse(reader.GetString(6)),
                    ExpectedDeliveryDate = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7)),
                    BloodGroup = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    PhoneNumber = reader.IsDBNull(9) ? "" : reader.GetString(9),
                    EmergencyContact = reader.IsDBNull(10) ? "" : reader.GetString(10),
                    Status = (LaborStatus)reader.GetInt32(11),
                    LaborStartTime = reader.IsDBNull(12) ? null : DateTime.Parse(reader.GetString(12)),
                    DeliveryTime = reader.IsDBNull(13) ? null : DateTime.Parse(reader.GetString(13)),
                    CervicalDilationOnAdmission = reader.IsDBNull(14) ? null : reader.GetInt32(14),
                    MembraneStatus = reader.IsDBNull(15) ? "Intact" : reader.GetString(15),
                    LiquorStatus = reader.IsDBNull(16) ? "Clear" : reader.GetString(16),
                    RiskFactors = reader.IsDBNull(17) ? "" : reader.GetString(17),
                    Complications = reader.IsDBNull(18) ? "" : reader.GetString(18)
                };

                // Load related data
                patient.PartographEntries = await _partographRepository.ListByPatientAsync(patient.ID);
                patient.VitalSigns = await _vitalSignRepository.ListByPatientAsync(patient.ID);

                return patient;
            }

            return null;
        }

        public async Task<int> SaveItemAsync(Patient item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();
            if (item.ID == 0)
            {
                saveCmd.CommandText = @"
                INSERT INTO Patient (Name, HospitalNumber, Age, Gravidity, Parity, AdmissionDate, 
                    ExpectedDeliveryDate, BloodGroup, PhoneNumber, EmergencyContact, Status, 
                    LaborStartTime, DeliveryTime, CervicalDilationOnAdmission, MembraneStatus, 
                    LiquorStatus, RiskFactors, Complications)
                VALUES (@Name, @HospitalNumber, @Age, @Gravidity, @Parity, @AdmissionDate, 
                    @ExpectedDeliveryDate, @BloodGroup, @PhoneNumber, @EmergencyContact, @Status, 
                    @LaborStartTime, @DeliveryTime, @CervicalDilationOnAdmission, @MembraneStatus, 
                    @LiquorStatus, @RiskFactors, @Complications);
                SELECT last_insert_rowid();";
            }
            else
            {
                saveCmd.CommandText = @"
                UPDATE Patient SET 
                    Name = @Name, HospitalNumber = @HospitalNumber, Age = @Age, 
                    Gravidity = @Gravidity, Parity = @Parity, AdmissionDate = @AdmissionDate,
                    ExpectedDeliveryDate = @ExpectedDeliveryDate, BloodGroup = @BloodGroup,
                    PhoneNumber = @PhoneNumber, EmergencyContact = @EmergencyContact,
                    Status = @Status, LaborStartTime = @LaborStartTime, DeliveryTime = @DeliveryTime,
                    CervicalDilationOnAdmission = @CervicalDilationOnAdmission,
                    MembraneStatus = @MembraneStatus, LiquorStatus = @LiquorStatus,
                    RiskFactors = @RiskFactors, Complications = @Complications
                WHERE ID = @ID";
                saveCmd.Parameters.AddWithValue("@ID", item.ID);
            }

            saveCmd.Parameters.AddWithValue("@Name", item.Name);
            saveCmd.Parameters.AddWithValue("@HospitalNumber", item.HospitalNumber);
            saveCmd.Parameters.AddWithValue("@Age", item.Age);
            saveCmd.Parameters.AddWithValue("@Gravidity", item.Gravidity);
            saveCmd.Parameters.AddWithValue("@Parity", item.Parity);
            saveCmd.Parameters.AddWithValue("@AdmissionDate", item.AdmissionDate.ToString("O"));
            saveCmd.Parameters.AddWithValue("@ExpectedDeliveryDate", item.ExpectedDeliveryDate?.ToString("O") ?? (object)DBNull.Value);
            saveCmd.Parameters.AddWithValue("@BloodGroup", item.BloodGroup ?? "");
            saveCmd.Parameters.AddWithValue("@PhoneNumber", item.PhoneNumber ?? "");
            saveCmd.Parameters.AddWithValue("@EmergencyContact", item.EmergencyContact ?? "");
            saveCmd.Parameters.AddWithValue("@Status", (int)item.Status);
            saveCmd.Parameters.AddWithValue("@LaborStartTime", item.LaborStartTime?.ToString("O") ?? (object)DBNull.Value);
            saveCmd.Parameters.AddWithValue("@DeliveryTime", item.DeliveryTime?.ToString("O") ?? (object)DBNull.Value);
            saveCmd.Parameters.AddWithValue("@CervicalDilationOnAdmission", item.CervicalDilationOnAdmission ?? (object)DBNull.Value);
            saveCmd.Parameters.AddWithValue("@MembraneStatus", item.MembraneStatus);
            saveCmd.Parameters.AddWithValue("@LiquorStatus", item.LiquorStatus);
            saveCmd.Parameters.AddWithValue("@RiskFactors", item.RiskFactors ?? "");
            saveCmd.Parameters.AddWithValue("@Complications", item.Complications ?? "");

            var result = await saveCmd.ExecuteScalarAsync();
            if (item.ID == 0)
            {
                item.ID = Convert.ToInt32(result);
            }

            return item.ID;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var stats = new DashboardStats();

            // Get counts by status
            var countCmd = connection.CreateCommand();
            countCmd.CommandText = @"
                SELECT Status, COUNT(*) FROM Patient GROUP BY Status;
                SELECT COUNT(*) FROM Patient WHERE DATE(DeliveryTime) = DATE('now');";

            await using var reader = await countCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var status = (LaborStatus)reader.GetInt32(0);
                var count = reader.GetInt32(1);

                switch (status)
                {
                    case LaborStatus.Pending:
                        stats.PendingLabor = count;
                        break;
                    case LaborStatus.Active:
                        stats.ActiveLabor = count;
                        break;
                    case LaborStatus.Emergency:
                        stats.EmergencyCases = count;
                        break;
                }
                stats.TotalPatients += count;
            }

            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                stats.CompletedToday = reader.GetInt32(0);
            }

            return stats;
        }
    }
}
