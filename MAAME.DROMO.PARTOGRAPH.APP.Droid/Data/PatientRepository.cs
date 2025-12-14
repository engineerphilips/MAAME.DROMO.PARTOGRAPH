using MAAME.DROMO.PARTOGRAPH.MODEL;
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
        //private readonly VitalSignRepository _vitalSignRepository;

        //_vitalSignRepository = vitalSignRepository;
        public PatientRepository(PartographRepository partographRepository,
            ILogger<PatientRepository> logger)
        {
            _partographRepository = partographRepository;
            _logger = logger;
        }
        //VitalSignRepository vitalSignRepository,
        private async Task Init()
        {
            if (_hasBeenInitialized)
                return;

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            //try
            //{
            //    var dropTableCmd = connection.CreateCommand();
            //    dropTableCmd.CommandText = @"
            //    DROP TABLE Tbl_Patient;";
            //    await dropTableCmd.ExecuteNonQueryAsync();
            //}
            //catch (Exception e)
            //{
            //    _logger.LogError(e, "Error dropping PartographEntry table");
            //    throw;
            //}

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
                    emergencyContactName TEXT, 
                    emergencyContactRelationship TEXT, 
                    emergencyContactPhone TEXT, 
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
            
                CREATE INDEX IF NOT EXISTS idx_patient_sync ON Tbl_Patient(updatedtime, syncstatus);
                CREATE INDEX IF NOT EXISTS idx_patient_server_version ON Tbl_Patient(serverversion);

                DROP TRIGGER IF EXISTS trg_patient_insert;
                CREATE TRIGGER trg_patient_insert 
                AFTER INSERT ON Tbl_Patient
                WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
                BEGIN
                    UPDATE Tbl_Patient 
                    SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                        updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                    WHERE ID = NEW.ID;
                END;

                DROP TRIGGER IF EXISTS trg_patient_update;
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
            var patients = new List<Patient>();

            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT ID, time, firstName, lastName, hospitalNumber, dateofbirth, age, bloodGroup, phoneNumber, emergencyContactName, emergencyContactRelationship, emergencyContactPhone, handler, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted FROM Tbl_Patient ORDER BY time DESC";

                await using var reader = await selectCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var patient = new Patient
                    {
                        ID = Guid.Parse(reader.GetString(0)),
                        FirstName = reader.GetString(2),
                        LastName = reader.GetString(3),
                        HospitalNumber = reader.GetString(4),
                        DateOfBirth = reader.IsDBNull(5) ? null : DateOnly.Parse(reader.GetString(5)),
                        Age = reader.IsDBNull(6) ? null : int.Parse(reader.GetString(6)),
                        BloodGroup = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        PhoneNumber = reader.IsDBNull(8) ? "" : reader.GetString(8),
                        EmergencyContactName = reader.IsDBNull(9) ? "" : reader.GetString(9),
                        EmergencyContactRelationship = reader.IsDBNull(10) ? "" : reader.GetString(10),
                        EmergencyContactPhone = reader.IsDBNull(11) ? "" : reader.GetString(11),
                        Handler = reader.IsDBNull(12) ? null : Guid.Parse(reader.GetString(12)),
                        CreatedTime = reader.GetInt64(13),
                        UpdatedTime = reader.GetInt64(14),
                        DeletedTime = reader.IsDBNull(15) ? null : reader.GetInt64(15),
                        DeviceId = reader.GetString(16),
                        OriginDeviceId = reader.GetString(17),
                        SyncStatus = reader.GetInt32(18),
                        Version = reader.GetInt32(19),
                        ServerVersion = reader.IsDBNull(20) ? 0 : reader.GetInt32(20),
                        Deleted = reader.IsDBNull(21) ? 0 : reader.GetInt32(21)
                    };

                    // Load related data
                    //patient.PartographEntries = await _partographRepository.ListByPatientAsync(patient.ID);
                    //patient.VitalSigns = await _vitalSignRepository.ListByPatientAsync(patient.ID);

                    patients.Add(patient);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving Patient table");
                throw;
            }

            return patients;
        }

        public async Task<Patient?> GetAsync(Guid? id)
        {
            await Init();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT ID, time, firstName, lastName, hospitalNumber, dateofbirth, age, bloodGroup, phoneNumber, emergencyContactName, emergencyContactRelationship, emergencyContactPhone, handler, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted FROM Tbl_Patient WHERE ID = @id";
                selectCmd.Parameters.AddWithValue("@id", id.ToString());

                await using var reader = await selectCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var patient = new Patient
                    {
                        ID = Guid.Parse(reader.GetString(0)),
                        FirstName = reader.GetString(2),
                        LastName = reader.GetString(3),
                        HospitalNumber = reader.GetString(4),
                        DateOfBirth = reader.IsDBNull(5) ? null : DateOnly.Parse(reader.GetString(5)),
                        Age = reader.IsDBNull(6) ? null : int.Parse(reader.GetString(6)),
                        BloodGroup = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        PhoneNumber = reader.IsDBNull(8) ? "" : reader.GetString(8),
                        EmergencyContactName = reader.IsDBNull(9) ? "" : reader.GetString(9),
                        EmergencyContactRelationship = reader.IsDBNull(10) ? "" : reader.GetString(10),
                        EmergencyContactPhone = reader.IsDBNull(11) ? "" : reader.GetString(11),
                        Handler = reader.IsDBNull(12) ? null : Guid.Parse(reader.GetString(12)),
                        CreatedTime = reader.GetInt64(13),
                        UpdatedTime = reader.GetInt64(14),
                        DeletedTime = reader.IsDBNull(15) ? null : reader.GetInt64(15),
                        DeviceId = reader.GetString(16),
                        OriginDeviceId = reader.GetString(17),
                        SyncStatus = reader.GetInt32(18),
                        Version = reader.GetInt32(19),
                        ServerVersion = reader.IsDBNull(20) ? 0 : reader.GetInt32(20),
                        Deleted = reader.IsDBNull(21) ? 0 : reader.GetInt32(21)

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
                    //patient.PartographEntries = await _partographRepository.ListByPatientAsync(patient.ID);
                    //patient.VitalSigns = await _vitalSignRepository.ListByPatientAsync(patient.ID);

                    return patient;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving Patient table");
                throw;
            }

            return null;
        }

        public async Task<Guid?> SaveItemAsync(Patient item)
        {
            await Init();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var isNewPatient = item.ID == null || item.ID == Guid.Empty;
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (isNewPatient)
                {
                    item.ID = Guid.NewGuid();
                }

                item.CreatedTime = isNewPatient ? now : item.CreatedTime;
                item.UpdatedTime = now;
                item.DeviceId = DeviceIdentity.GetOrCreateDeviceId();
                item.OriginDeviceId = item.OriginDeviceId ?? DeviceIdentity.GetOrCreateDeviceId();
                item.Version = isNewPatient ? 1 : item.Version + 1;
                item.ServerVersion = isNewPatient ? 0 : item.ServerVersion;
                item.SyncStatus = 0; // Mark as needing sync
                item.Deleted = 0;
                item.DataHash = item.CalculateHash();

                var saveCmd = connection.CreateCommand();
                if (isNewPatient)
                {
                    saveCmd.CommandText = @"
                INSERT INTO Tbl_Patient (ID, time, firstName, lastName, hospitalNumber, dateofbirth, age, bloodGroup, phoneNumber, emergencyContactName, emergencyContactRelationship, emergencyContactPhone, handler, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted)
                VALUES (@ID, @time, @firstName, @lastName, @hospitalNumber, @dateofbirth, @age, @bloodGroup, @phoneNumber, @emergencyContactName, @emergencyContactRelationship, @emergencyContactPhone, @handler, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted)";
                }
                else
                {
                    saveCmd.CommandText = @"
                UPDATE Tbl_Patient SET
                    firstName = @firstName,
                    lastName = @lastName,
                    hospitalNumber = @hospitalNumber,
                    dateofbirth = @dateofbirth,
                    age = @age,
                    bloodGroup = @bloodGroup,
                    phoneNumber = @phoneNumber,
                    emergencyContactName = @emergencyContactName,
                    emergencyContactRelationship = @emergencyContactRelationship,
                    emergencyContactPhone = @emergencyContactPhone,
                    handler = @handler,
                    updatedtime = @updatedtime,
                    deviceid = @deviceid,
                    syncstatus = @syncstatus,
                    version = @version
                WHERE ID = @ID";
                }

                saveCmd.Parameters.AddWithValue("@ID", item.ID.ToString());
                saveCmd.Parameters.AddWithValue("@time", now.ToString());
                saveCmd.Parameters.AddWithValue("@firstName", item.FirstName ?? "");
                saveCmd.Parameters.AddWithValue("@lastName", item.LastName ?? "");
                saveCmd.Parameters.AddWithValue("@hospitalNumber", item.HospitalNumber ?? "");

                saveCmd.Parameters.AddWithValue("@dateofbirth", item.DateOfBirth != null ? item.DateOfBirth?.ToString("yyyy-MM-dd") : DBNull.Value);

                saveCmd.Parameters.AddWithValue("@age", item.Age != null ? item.Age : DBNull.Value);

                saveCmd.Parameters.AddWithValue("@bloodGroup", item.BloodGroup ?? "");
                saveCmd.Parameters.AddWithValue("@phoneNumber", item.PhoneNumber ?? "");
                saveCmd.Parameters.AddWithValue("@emergencyContactName", item.EmergencyContactName ?? "");
                saveCmd.Parameters.AddWithValue("@emergencyContactRelationship", item.EmergencyContactRelationship ?? "");
                saveCmd.Parameters.AddWithValue("@emergencyContactPhone", item.EmergencyContactPhone ?? "");
                saveCmd.Parameters.AddWithValue("@handler", item.Handler != null ? item.Handler?.ToString() : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@createdtime", item.CreatedTime);
                saveCmd.Parameters.AddWithValue("@updatedtime", item.UpdatedTime);
                saveCmd.Parameters.AddWithValue("@deletedtime", item.DeletedTime != null ? item.DeletedTime : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@deviceid", item.DeviceId);
                saveCmd.Parameters.AddWithValue("@origindeviceid", item.OriginDeviceId);
                saveCmd.Parameters.AddWithValue("@syncstatus", item.SyncStatus);
                saveCmd.Parameters.AddWithValue("@version", item.Version);
                saveCmd.Parameters.AddWithValue("@serverversion", item.ServerVersion);
                saveCmd.Parameters.AddWithValue("@deleted", item.Deleted);

                await saveCmd.ExecuteNonQueryAsync();

                // Automatically create a Partograph with Pending status for new patients
                if (isNewPatient)
                {
                    //var partograph = new Partograph
                    //{
                    //    ID = Guid.NewGuid(),
                    //    PatientID = item.ID,
                    //    Status = LaborStatus.Pending,
                    //    Time = DateTime.UtcNow,
                    //    AdmissionDate = DateTime.UtcNow,
                    //    Gravida = 0,
                    //    Parity = 0,
                    //    MembraneStatus = "Intact",
                    //    LiquorStatus = "Clear",
                    //    HandlerName = item.HandlerName,
                    //    Handler = item.Handler,
                    //    DeviceId = item.DeviceId,
                    //    OriginDeviceId = item.OriginDeviceId,
                    //    CreatedTime = now,
                    //    UpdatedTime = now,
                    //    Version = 1,
                    //    ServerVersion = 0,
                    //    SyncStatus = 0,
                    //    Deleted = 0
                    //};

                    //partograph.DataHash = partograph.CalculateHash();

                    //await _partographRepository.SaveItemAsync(partograph);

                    //_logger.LogInformation("Created patient {PatientId} with automatic partograph {PartographId} in Pending status",
                    //    item.ID, partograph.ID);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error saving patient record");
                throw;
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

        /// <summary>
        /// Upserts a patient record (insert if new, update if exists) - used for sync operations
        /// </summary>
        public async Task UpsertPatientAsync(Patient patient)
        {
            await Init();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                // Check if record exists
                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM Tbl_Patient WHERE ID = @id";
                checkCmd.Parameters.AddWithValue("@id", patient.ID.ToString());
                var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

                var cmd = connection.CreateCommand();

                if (exists)
                {
                    // Update existing record - only if server version is newer or equal
                    cmd.CommandText = @"
                    UPDATE Tbl_Patient SET
                        firstName = @firstName,
                        lastName = @lastName,
                        hospitalNumber = @hospitalNumber,
                        dateofbirth = @dateofbirth,
                        age = @age,
                        bloodGroup = @bloodGroup,
                        phoneNumber = @phoneNumber,
                        emergencyContactName = @emergencyContactName,
                        emergencyContactRelationship = @emergencyContactRelationship,
                        emergencyContactPhone = @emergencyContactPhone,
                        handler = @handler,
                        updatedtime = @updatedtime,
                        serverversion = @serverversion,
                        syncstatus = 1,
                        datahash = @datahash
                    WHERE ID = @ID AND serverversion <= @serverversion";
                }
                else
                {
                    // Insert new record
                    cmd.CommandText = @"
                    INSERT INTO Tbl_Patient (ID, firstName, lastName, hospitalNumber, dateofbirth, age, bloodGroup, phoneNumber, emergencyContactName, emergencyContactRelationship, emergencyContactPhone, createdtime, updatedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash) VALUES (@ID, @firstName, @lastName, @hospitalNumber, @dateofbirth, @age, @bloodGroup, @phoneNumber, @emergencyContactName, @emergencyContactRelationship, @emergencyContactPhone, @createdtime, @updatedtime, @deviceid, @origindeviceid, 1, @version, @serverversion, @deleted, @datahash)";
                }

                cmd.Parameters.AddWithValue("@ID", patient.ID.ToString());
                cmd.Parameters.AddWithValue("@firstName", patient.FirstName ?? "");
                cmd.Parameters.AddWithValue("@lastName", patient.LastName ?? "");
                cmd.Parameters.AddWithValue("@hospitalNumber", patient.HospitalNumber ?? "");
                cmd.Parameters.AddWithValue("@dateofbirth", patient.DateOfBirth != null ? patient.DateOfBirth.Value.ToString("yyyy-MM-dd") : null);
                cmd.Parameters.AddWithValue("@age", patient.Age);
                cmd.Parameters.AddWithValue("@bloodGroup", patient.BloodGroup ?? "");
                cmd.Parameters.AddWithValue("@phoneNumber", patient.PhoneNumber ?? "");
                cmd.Parameters.AddWithValue("@emergencyContactName", patient.EmergencyContactName ?? "");
                cmd.Parameters.AddWithValue("@emergencyContactRelationship", patient.EmergencyContactRelationship ?? "");
                cmd.Parameters.AddWithValue("@emergencyContactPhone", patient.EmergencyContactPhone ?? "");
                cmd.Parameters.AddWithValue("@handler", patient.Handler?.ToString() ?? "");
                cmd.Parameters.AddWithValue("@createdtime", patient.CreatedTime);
                cmd.Parameters.AddWithValue("@updatedtime", patient.UpdatedTime);
                cmd.Parameters.AddWithValue("@deviceid", patient.DeviceId ?? "");
                cmd.Parameters.AddWithValue("@origindeviceid", patient.OriginDeviceId ?? "");
                cmd.Parameters.AddWithValue("@version", patient.Version);
                cmd.Parameters.AddWithValue("@serverversion", patient.ServerVersion);
                cmd.Parameters.AddWithValue("@deleted", patient.Deleted);
                cmd.Parameters.AddWithValue("@datahash", patient.DataHash ?? "");

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating patient record");
                throw;
            }
        }
    }
}
