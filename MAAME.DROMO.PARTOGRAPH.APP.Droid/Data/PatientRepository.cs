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
        private readonly PartographRepository _partographRepository;
        private readonly VitalSignRepository _vitalSignRepository;

        public PatientRepository(PartographRepository partographRepository,
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
                CREATE TABLE IF NOT EXISTS Tbl_Patient (
                    ID TEXT PRIMARY KEY,
                    time TEXT NOT NULL,
                    firstName TEXT NOT NULL,
                    lastName TEXT NOT NULL,
                    hospitalNumber TEXT NOT NULL,
                    dateofbirth TEXT NULL,
                    age INTEGER NULL,
                    bloodGroup TEXT,
                    phoneNumber TEXT,
                    emergencyContact TEXT, 
                    handler TEXT,                
                    createdtime INTEGER NOT NULL,
                    updatedtime INTEGER NOT NULL,
                    deletedtime INTEGER, 
                    deviceid TEXT NOT NULL,
                    origindeviceid TEXT NOT NULL,
                    syncstatus INTEGER DEFAULT 0,
                    version INTEGER DEFAULT 1,
                    serverversion INTEGER DEFAULT 0,
                    deleted INTEGER DEFAULT 0,
                    conflictdata TEXT,
                    datahash TEXT
                );
            
                CREATE INDEX idx_patient_sync ON Tbl_Patient(updatedtime, syncstatus);
                CREATE INDEX idx_patient_server_version ON Tbl_Patient(serverversion);

                CREATE TRIGGER trg_patient_insert 
                AFTER INSERT ON Tbl_Patient
                WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
                BEGIN
                    UPDATE Tbl_Patient 
                    SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                        updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                    WHERE ID = NEW.ID;
                END;

                CREATE TRIGGER trg_patient_update 
                AFTER UPDATE ON Tbl_Patient
                WHEN NEW.updatedtime = OLD.updatedtime
                BEGIN
                    UPDATE Tbl_Patient 
                    SET updatedtime = (strftime('%s', 'now') * 1000),
                        version = OLD.version + 1,
                        syncstatus = 0
                    WHERE ID = NEW.ID;
                END;";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating Patient table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<List<Patient>> ListAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT ID, time, firstName, lastName, hospitalNumber, dateofbirth, age, bloodGroup, phoneNumber, emergencyContact, handler, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted FROM Tbl_Patient ORDER BY admissionDate DESC";

            var patients = new List<Patient>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var patient = new Patient
                {
                    ID = Guid.Parse(reader.GetString(0)),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    HospitalNumber = reader.GetString(3),
                    DateOfBirth = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
                    Age = reader.IsDBNull(5) ? null : int.Parse(reader.GetString(5)),
                    BloodGroup = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    PhoneNumber = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    EmergencyContact = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    Handler = reader.IsDBNull(9) ? null : Guid.Parse(reader.GetString(9)),
                    CreatedTime = reader.GetInt64(10),
                    UpdatedTime = reader.GetInt64(11),
                    DeletedTime = reader.IsDBNull(12) ? null : reader.GetInt64(12),
                    DeviceId = reader.GetString(13),
                    OriginDeviceId = reader.GetString(14),
                    SyncStatus = reader.GetInt32(15),
                    Version = reader.GetInt32(16),
                    ServerVersion = reader.IsDBNull(17) ? 0 : reader.GetInt32(17),
                    Deleted = reader.IsDBNull(18) ? 0 : reader.GetInt32(18)
                };

                // Load related data
                patient.PartographEntries = await _partographRepository.ListByPatientAsync(patient.ID);
                patient.VitalSigns = await _vitalSignRepository.ListByPatientAsync(patient.ID);

                patients.Add(patient);
            }

            return patients;
        }

        public async Task<Patient?> GetAsync(Guid? id)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT ID, time, firstName, lastName, hospitalNumber, dateofbirth, age, bloodGroup, phoneNumber, emergencyContact, handler, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted FROM Tbl_Patient WHERE ID = @id";
            selectCmd.Parameters.AddWithValue("@id", id);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var patient = new Patient
                {
                    ID = Guid.Parse(reader.GetString(0)),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    HospitalNumber = reader.GetString(3),
                    DateOfBirth = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
                    Age = reader.IsDBNull(5) ? null : int.Parse(reader.GetString(5)),
                    BloodGroup = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    PhoneNumber = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    EmergencyContact = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    Handler = reader.IsDBNull(9) ? null : Guid.Parse(reader.GetString(9)),
                    CreatedTime = reader.GetInt64(10),
                    UpdatedTime = reader.GetInt64(11),
                    DeletedTime = reader.IsDBNull(12) ? null : reader.GetInt64(12),
                    DeviceId = reader.GetString(13),
                    OriginDeviceId = reader.GetString(14),
                    SyncStatus = reader.GetInt32(15),
                    Version = reader.GetInt32(16),
                    ServerVersion = reader.IsDBNull(17) ? 0 : reader.GetInt32(17),
                    Deleted = reader.IsDBNull(18) ? 0 : reader.GetInt32(18)

                    //ID = Guid.Parse(reader.GetString(0)),
                    //FirstName = reader.GetString(1),
                    //LastName = reader.GetString(2),
                    //HospitalNumber = reader.GetString(3),
                    //DateOfBirth = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
                    //Age = reader.IsDBNull(5) ? null : int.Parse(reader.GetString(5)),
                    //Gravida = reader.GetInt32(6),
                    //Parity = reader.GetInt32(7),
                    //AdmissionDate = DateTime.Parse(reader.GetString(8)),
                    //ExpectedDeliveryDate = reader.IsDBNull(9) ? null : DateTime.Parse(reader.GetString(9)),
                    //BloodGroup = reader.IsDBNull(10) ? "" : reader.GetString(10),
                    //PhoneNumber = reader.IsDBNull(11) ? "" : reader.GetString(11),
                    //EmergencyContact = reader.IsDBNull(12) ? "" : reader.GetString(12),
                    //Status = (LaborStatus)reader.GetInt32(13),
                    //LaborStartTime = reader.IsDBNull(14) ? null : DateTime.Parse(reader.GetString(14)),
                    //DeliveryTime = reader.IsDBNull(15) ? null : DateTime.Parse(reader.GetString(15)),
                    //CervicalDilationOnAdmission = reader.IsDBNull(16) ? null : reader.GetInt32(16),
                    //MembraneStatus = reader.IsDBNull(17) ? "Intact" : reader.GetString(17),
                    //LiquorStatus = reader.IsDBNull(18) ? "Clear" : reader.GetString(18),
                    //RiskFactors = reader.IsDBNull(19) ? "" : reader.GetString(19),
                    //Complications = reader.IsDBNull(20) ? "" : reader.GetString(20),
                    //Handler = reader.IsDBNull(21) ? null : Guid.Parse(reader.GetString(21))
                };

                // Load related data
                patient.PartographEntries = await _partographRepository.ListByPatientAsync(patient.ID);
                patient.VitalSigns = await _vitalSignRepository.ListByPatientAsync(patient.ID);

                return patient;
            }

            return null;
        }

        public async Task<Guid?> SaveItemAsync(Patient item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();
            if (item.ID == null)
            {
                item.ID = Guid.NewGuid();
                saveCmd.CommandText = @"
                INSERT INTO Tbl_Patient (ID, firstName, lastName, hospitalNumber, dateofbirth, age, bloodGroup, phoneNumber, emergencyContact, handler, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted)
                VALUES (@firstName, @lastName, @hospitalNumber, @dateofbirth, @age, @bloodGroup, @phoneNumber, @emergencyContact, @handler, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted);";

                //SELECT last_insert_rowid();

                saveCmd.Parameters.AddWithValue("@ID", item.ID);
            }
            else
            {
                saveCmd.CommandText = @"
                UPDATE Tbl_Patient SET 
                    firstName = @firstName, lastName = @lastName, hospitalNumber = @hospitalNumber, dateofbirth = @dateofbirth, age = @age, gravida = @gravida, parity = @parity, admissionDate = @admissionDate, expectedDeliveryDate = @expectedDeliveryDate, bloodGroup = @bloodGroup, phoneNumber = @phoneNumber, emergencyContact = @emergencyContact, laborStartTime = @laborStartTime, deliveryTime = @deliveryTime, cervicalDilationOnAdmission = @cervicalDilationOnAdmission, membraneStatus = @membraneStatus, liquorStatus = @liquorStatus, riskFactors = @riskFactors, complications = @complications, updatedtime = @updatedtime, deviceid = @deviceid, syncstatus = @syncstatus, version = @version
                WHERE ID = @ID";
                saveCmd.Parameters.AddWithValue("@ID", item.ID);
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            item.ID = item.ID ?? Guid.NewGuid();
            item.CreatedTime = now;
            item.UpdatedTime = now;
            item.DeviceId = DeviceIdentity.GetOrCreateDeviceId();
            item.OriginDeviceId = DeviceIdentity.GetOrCreateDeviceId();
            item.Version = 1;
            item.ServerVersion = 0;
            item.SyncStatus = 0;
            item.Deleted = 0;
            item.DataHash = item.CalculateHash();

            saveCmd.Parameters.AddWithValue("@firstname", item.FirstName);
            saveCmd.Parameters.AddWithValue("@lastname", item.LastName);
            saveCmd.Parameters.AddWithValue("@hospitalNumber", item.HospitalNumber);
            saveCmd.Parameters.AddWithValue("@dateofbirth", item.DateOfBirth);
            saveCmd.Parameters.AddWithValue("@age", item.Age);
            saveCmd.Parameters.AddWithValue("@bloodGroup", item.BloodGroup ?? "");
            saveCmd.Parameters.AddWithValue("@phoneNumber", item.PhoneNumber ?? "");
            saveCmd.Parameters.AddWithValue("@emergencyContact", item.EmergencyContact ?? "");
            saveCmd.Parameters.AddWithValue("@handler", item.Handler.ToString());
            saveCmd.Parameters.AddWithValue("@createdtime", item.CreatedTime);
            saveCmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
            saveCmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
            saveCmd.Parameters.AddWithValue("@origindeviceid", item.OriginDeviceId);
            saveCmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
            saveCmd.Parameters.AddWithValue("@version", item.Version);
            saveCmd.Parameters.AddWithValue("@deleted", item.Deleted);

            var result = await saveCmd.ExecuteScalarAsync();
            if (item.ID == null)
            {
                item.ID = Guid.Parse(Convert.ToString(result));
            }

            return item.ID;
        }

        //public async Task<DashboardStats> GetDashboardStatsAsync()
        //{
        //    await Init();
        //    await using var connection = new SqliteConnection(Constants.DatabasePath);
        //    await connection.OpenAsync();

        //    var stats = new DashboardStats();

        //    // Get counts by status
        //    var countCmd = connection.CreateCommand();
        //    countCmd.CommandText = @"
        //        SELECT Status, COUNT(*) FROM Tbl_Patient GROUP BY status;
        //        SELECT COUNT(*) FROM Tbl_Patient WHERE DATE(deliveryTime) = DATE('now');";

        //    await using var reader = await countCmd.ExecuteReaderAsync();
        //    while (await reader.ReadAsync())
        //    {
        //        var status = (LaborStatus)reader.GetInt32(0);
        //        var count = reader.GetInt32(1);

        //        switch (status)
        //        {
        //            case LaborStatus.Pending:
        //                stats.PendingLabor = count;
        //                break;
        //            case LaborStatus.Active:
        //                stats.ActiveLabor = count;
        //                break;
        //            case LaborStatus.Emergency:
        //                stats.EmergencyCases = count;
        //                break;
        //        }
        //        stats.TotalPatients += count;
        //    }

        //    if (await reader.NextResultAsync() && await reader.ReadAsync())
        //    {
        //        stats.CompletedToday = reader.GetInt32(0);
        //    }

        //    return stats;
        //}
    }
}
