using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class PartographRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;

        public PartographRepository(ILogger<PartographRepository> logger)
        {
            _logger = logger;
        }

        private async Task Init()
        {

            if (_hasBeenInitialized)
                return;

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            //try
            //{
            //    var dropTableCmd = connection.CreateCommand();
            //    dropTableCmd.CommandText = @"UPDATE Tbl_Partograph SET status = @status WHERE status = @status1;";
            //    dropTableCmd.Parameters.AddWithValue("@status", (int)LaborStatus.Active);
            //    dropTableCmd.Parameters.AddWithValue("@status1", (int)LaborStatus.SecondStage);
            //    await dropTableCmd.ExecuteNonQueryAsync();
            //}
            //catch (Exception e)
            //{
            //    _logger.LogError(e, "Update PartographEntry table");
            //    throw;
            //}

            //try
            //{
            //    var dropTableCmd = connection.CreateCommand();
            //    dropTableCmd.CommandText = @"
            //    DROP TABLE Tbl_Urine;";
            //    await dropTableCmd.ExecuteNonQueryAsync();
            //}
            //catch (Exception e)
            //{
            //    _logger.LogError(e, "Error dropping PartographEntry table");
            //    throw;
            //}

            //try
            //{
            //    var dropTableCmd = connection.CreateCommand();
            //    dropTableCmd.CommandText = @"
            //    DROP TABLE Tbl_Partograph;";
            //    await dropTableCmd.ExecuteNonQueryAsync();
            //}
            //catch (Exception e)
            //{
            //    _logger.LogError(e, "Error dropping PartographEntry table");
            //    throw;
            //}

            // Create Tbl_Patient first (required for JOIN queries)
            try
            {
                var createPatientTableCmd = connection.CreateCommand();
                createPatientTableCmd.CommandText = @"
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
                await createPatientTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating Patient table");
                throw;
            }

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Tbl_Partograph (
                    ID TEXT PRIMARY KEY,
                    patientID TEXT NOT NULL,
                    time TEXT NOT NULL,
                    status TEXT,
                    gravida INTEGER NOT NULL,
                    parity INTEGER NOT NULL,
                    abortion INTEGER NOT NULL,
                    admissionDate TEXT NOT NULL,
                    expectedDeliveryDate TEXT,
                    lastMenstrualDate TEXT,
                    laborStartTime TEXT,
                    secondStageStartTime TEXT,
                    thirdStageStartTime TEXT,
                    fourthStageStartTime TEXT,
                    deliveryTime TEXT,
                    completedTime TEXT,
                    rupturedMembraneTime TEXT,
                    cervicalDilationOnAdmission INTEGER,
                    membraneStatus TEXT,
                    liquorStatus TEXT,
                    complications TEXT,
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
            
                CREATE INDEX IF NOT EXISTS idx_partograph_sync ON Tbl_Partograph(updatedtime, syncstatus);
                CREATE INDEX IF NOT EXISTS idx_partograph_server_version ON Tbl_Partograph(serverversion);

                DROP TRIGGER IF EXISTS trg_partograph_insert;
                CREATE TRIGGER trg_partograph_insert 
                AFTER INSERT ON Tbl_Partograph
                WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
                BEGIN
                    UPDATE Tbl_Partograph 
                    SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                        updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                    WHERE ID = NEW.ID;
                END;

                DROP TRIGGER IF EXISTS trg_partograph_update;
                CREATE TRIGGER trg_partograph_update 
                AFTER UPDATE ON Tbl_Partograph
                WHEN NEW.updatedtime = OLD.updatedtime
                BEGIN
                    UPDATE Tbl_Partograph 
                    SET updatedtime = (strftime('%s', 'now') * 1000),
                        version = OLD.version + 1,
                        syncstatus = 0
                    WHERE ID = NEW.ID;
                END;";
                await createTableCmd.ExecuteNonQueryAsync();

                // Migration: Add WHO Four-Stage System timestamp columns if they don't exist
                try
                {
                    var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = @"
                        -- Add secondStageStartTime if it doesn't exist
                        ALTER TABLE Tbl_Partograph ADD COLUMN secondStageStartTime TEXT;";
                    await alterCmd.ExecuteNonQueryAsync();
                }
                catch (SqliteException ex) when (ex.Message.Contains("duplicate column"))
                {
                    // Column already exists, ignore
                }

                try
                {
                    var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = @"
                        -- Add thirdStageStartTime if it doesn't exist
                        ALTER TABLE Tbl_Partograph ADD COLUMN thirdStageStartTime TEXT;";
                    await alterCmd.ExecuteNonQueryAsync();
                }
                catch (SqliteException ex) when (ex.Message.Contains("duplicate column"))
                {
                    // Column already exists, ignore
                }

                try
                {
                    var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = @"
                        -- Add fourthStageStartTime if it doesn't exist
                        ALTER TABLE Tbl_Partograph ADD COLUMN fourthStageStartTime TEXT;";
                    await alterCmd.ExecuteNonQueryAsync();
                }
                catch (SqliteException ex) when (ex.Message.Contains("duplicate column"))
                {
                    // Column already exists, ignore
                }

                try
                {
                    var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = @"
                        -- Add completedTime if it doesn't exist
                        ALTER TABLE Tbl_Partograph ADD COLUMN completedTime TEXT;";
                    await alterCmd.ExecuteNonQueryAsync();
                }
                catch (SqliteException ex) when (ex.Message.Contains("duplicate column"))
                {
                    // Column already exists, ignore
                }

                try
                {
                    var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = @"
                        -- Add rupturedMembraneTime if it doesn't exist
                        ALTER TABLE Tbl_Partograph ADD COLUMN rupturedMembraneTime TEXT;";
                    await alterCmd.ExecuteNonQueryAsync();
                }
                catch (SqliteException ex) when (ex.Message.Contains("duplicate column"))
                {
                    // Column already exists, ignore
                }

                _logger.LogInformation("WHO Four-Stage System database schema migration completed successfully");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating PartographEntry table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<List<Partograph>> ListByPatientAsync(Guid? patientId)
        {
            await Init();
            var entries = new List<Partograph>();
            try
            {

                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT P.ID, P.patientID, P.time, P.status, P.gravida, P.parity, P.abortion, P.admissionDate, P.expectedDeliveryDate, P.laborStartTime, P.deliveryTime, P.cervicalDilationOnAdmission, P.membraneStatus, P.liquorStatus, P.complications, P.handler, P.createdtime, P.updatedtime, P.deletedtime, P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion, P.deleted, PA.firstName, PA.lastName, PA.hospitalNumber, PA.dateofbirth, PA.age, PA.bloodGroup, PA.phoneNumber, PA.emergencyContactName, PA.emergencyContactPhone, PA.emergencyContactRelationship FROM Tbl_Partograph P INNER JOIN Tbl_Patient PA ON P.patientID = PA.ID WHERE P.ID = @Id ORDER BY P.time DESC";

                selectCmd.Parameters.AddWithValue("@Id", patientId);

                await using var reader = await selectCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    entries.Add(MapFromReader(reader));
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return entries;
        }

        protected Partograph MapFromReader(SqliteDataReader reader)
        {
            return new Partograph
            {
                ID = Guid.Parse(reader.GetString(0)),
                PatientID = reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                Time = DateTime.Parse(reader.GetString(2)),
                Status = (LaborStatus)reader.GetInt32(3),
                Gravida = reader.GetInt32(4),
                Parity = reader.GetInt32(5),
                Abortion = reader.GetInt32(6),
                AdmissionDate = DateTime.Parse(reader.GetString(7)),
                ExpectedDeliveryDate = reader.IsDBNull(8) ? null : DateOnly.Parse(reader.GetString(8)),
                LastMenstrualDate = reader.IsDBNull(9) ? null : DateOnly.Parse(reader.GetString(9)),
                LaborStartTime = reader.IsDBNull(10) ? null : DateTime.Parse(reader.GetString(10)),
                DeliveryTime = reader.IsDBNull(11) ? null : DateTime.Parse(reader.GetString(11)),
                CervicalDilationOnAdmission = reader.IsDBNull(12) ? null : reader.GetInt32(12),
                MembraneStatus = reader.IsDBNull(13) ? "Intact" : reader.GetString(13),
                LiquorStatus = reader.IsDBNull(14) ? "Clear" : reader.GetString(14),
                Complications = reader.IsDBNull(15) ? "" : reader.GetString(15),
                Handler = reader.IsDBNull(16) ? null : Guid.Parse(reader.GetString(16)),
                CreatedTime = reader.GetInt64(17),
                UpdatedTime = reader.GetInt64(18),
                DeletedTime = reader.IsDBNull(19) ? null : reader.GetInt64(19),
                DeviceId = reader.GetString(20),
                OriginDeviceId = reader.GetString(21),
                SyncStatus = reader.GetInt32(22),
                Version = reader.GetInt32(23),
                ServerVersion = reader.IsDBNull(24) ? 0 : reader.GetInt32(24),
                Deleted = reader.IsDBNull(25) ? 0 : reader.GetInt32(25),
                Patient = new Patient
                {
                    ID = reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                    FirstName = reader.GetString(26),
                    LastName = reader.GetString(27),
                    HospitalNumber = reader.GetString(28),
                    DateOfBirth = reader.IsDBNull(29) ? null : DateOnly.Parse(reader.GetString(29)),
                    Age = reader.IsDBNull(30) ? null : int.Parse(reader.GetString(30)),
                    BloodGroup = reader.IsDBNull(31) ? "" : reader.GetString(31),
                    PhoneNumber = reader.IsDBNull(32) ? "" : reader.GetString(32),
                    EmergencyContactName = reader.IsDBNull(33) ? "" : reader.GetString(33),
                    EmergencyContactPhone = reader.IsDBNull(34) ? "" : reader.GetString(34),
                    EmergencyContactRelationship = reader.IsDBNull(35) ? "" : reader.GetString(35)
                }
            };
        }

        public async Task<Guid?> SaveItemAsync(Partograph item)
        {
            await Init();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var isNewPartograph = item.ID == null || item.ID == Guid.Empty;
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                LaborStatus? oldStatus = null;

                if (isNewPartograph)
                {
                    item.ID = Guid.NewGuid();
                }
                else
                {
                    // Load existing partograph to track status changes
                    var existingPartograph = await GetAsync(item.ID);
                    if (existingPartograph != null)
                    {
                        oldStatus = existingPartograph.Status;
                    }
                }

                // Validate status transition for FirstStage status - prevent multiple active partographs per patient
                if (item.Status == LaborStatus.FirstStage && oldStatus != LaborStatus.FirstStage)
                {
                    var checkCmd = connection.CreateCommand();
                    checkCmd.CommandText = @"
                    SELECT COUNT(*) FROM Tbl_Partograph
                    WHERE patientID = @patientID
                    AND ID != @currentID
                    AND status IN (@firstStage, @secondStage, @thirdStage, @fourthStage)
                    AND deleted = 0";
                    checkCmd.Parameters.AddWithValue("@patientID", item.PatientID.ToString());
                    checkCmd.Parameters.AddWithValue("@currentID", item.ID.ToString());
                    checkCmd.Parameters.AddWithValue("@firstStage", (int)LaborStatus.FirstStage);
                    checkCmd.Parameters.AddWithValue("@secondStage", (int)LaborStatus.SecondStage);
                    checkCmd.Parameters.AddWithValue("@thirdStage", (int)LaborStatus.ThirdStage);
                    checkCmd.Parameters.AddWithValue("@fourthStage", (int)LaborStatus.FourthStage);

                    var hasActivePartograph = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

                    if (hasActivePartograph)
                    {
                        _logger.LogWarning("Patient {PatientId} already has an active partograph. Cannot activate partograph {PartographId}",
                            item.PatientID, item.ID);
                        throw new InvalidOperationException($"Patient {item.PatientID} already has an active partograph. Please complete the existing one first.");
                    }
                }

                // WHO Four-Stage System: Set timestamps for stage transitions

                // Set LaborStartTime when transitioning to FirstStage
                if (item.Status == LaborStatus.FirstStage && oldStatus != LaborStatus.FirstStage && !item.LaborStartTime.HasValue)
                {
                    item.LaborStartTime = DateTime.UtcNow;
                    _logger.LogInformation($"First stage labor started for partograph {item.ID} at {item.LaborStartTime}");
                }

                // Note: SecondStageStartTime, ThirdStageStartTime, and FourthStageStartTime
                // will be set by the PartographPageModel during stage transitions
                // These are handled separately to allow for manual and automatic progression

                // Set CompletedTime when transitioning to Completed status
                if (item.Status == LaborStatus.Completed && oldStatus != LaborStatus.Completed)
                {
                    // CompletedTime is set in the model
                    _logger.LogInformation($"Delivery care completed for partograph {item.ID}");
                }

                item.CreatedTime = isNewPartograph ? now : item.CreatedTime;
                item.UpdatedTime = now;
                item.DeviceId = DeviceIdentity.GetOrCreateDeviceId();
                item.OriginDeviceId = item.OriginDeviceId ?? DeviceIdentity.GetOrCreateDeviceId();
                item.Version = isNewPartograph ? 1 : item.Version + 1;
                item.ServerVersion = isNewPartograph ? 0 : item.ServerVersion;
                item.SyncStatus = 0; // Mark as needing sync
                item.Deleted = 0;
                item.DataHash = item.CalculateHash();

                var saveCmd = connection.CreateCommand();
                if (isNewPartograph)
                {
                    saveCmd.CommandText = @"
                    INSERT INTO Tbl_Partograph (ID, patientID, time, status, gravida, parity, abortion, admissionDate, expectedDeliveryDate, lastMenstrualDate, laborStartTime, deliveryTime, cervicalDilationOnAdmission, membraneStatus, liquorStatus, complications, handler, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted)
                    VALUES (@ID, @patientID, @time, @status, @gravida, @parity, @abortion, @admissionDate, @expectedDeliveryDate, @lastMenstrualDate, @laborStartTime, @deliveryTime, @cervicalDilationOnAdmission, @membraneStatus, @liquorStatus, @complications, @handler, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted)";
                }
                else
                {
                    saveCmd.CommandText = @"
                    UPDATE Tbl_Partograph SET
                        time = @time,
                        status = @status,
                        gravida = @gravida,
                        parity = @parity,
                        abortion = @abortion,   
                        admissionDate = @admissionDate,
                        expectedDeliveryDate = @expectedDeliveryDate,
                        lastMenstrualDate = @lastMenstrualDate, 
                        laborStartTime = @laborStartTime,
                        deliveryTime = @deliveryTime,
                        cervicalDilationOnAdmission = @cervicalDilationOnAdmission,
                        membraneStatus = @membraneStatus,
                        liquorStatus = @liquorStatus, 
                        complications = @complications,
                        handler = @handler,
                        updatedtime = @updatedtime,
                        deviceid = @deviceid,
                        syncstatus = @syncstatus,
                        version = @version
                    WHERE ID = @ID";
                }

                saveCmd.Parameters.AddWithValue("@ID", item.ID.ToString());
                saveCmd.Parameters.AddWithValue("@patientID", item.PatientID?.ToString() ?? "");
                saveCmd.Parameters.AddWithValue("@time", item.Time.ToString("yyyy-MM-dd HH:mm:ss"));
                saveCmd.Parameters.AddWithValue("@status", item.Status != null ? (int)item.Status : 0);
                saveCmd.Parameters.AddWithValue("@gravida", item.Gravida);
                saveCmd.Parameters.AddWithValue("@parity", item.Parity);
                saveCmd.Parameters.AddWithValue("@abortion", item.Abortion);
                saveCmd.Parameters.AddWithValue("@admissionDate", item.AdmissionDate.ToString("yyyy-MM-dd HH:mm"));
                saveCmd.Parameters.AddWithValue("@expectedDeliveryDate", item.ExpectedDeliveryDate != null ? item.ExpectedDeliveryDate?.ToString("yyyy-MM-dd") : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@lastMenstrualDate", item.LastMenstrualDate != null ? item.LastMenstrualDate?.ToString("yyyy-MM-dd") : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@laborStartTime", item.LaborStartTime != null ? item.LaborStartTime?.ToString("yyyy-MM-dd HH:mm") : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@deliveryTime", item.DeliveryTime != null ? item.DeliveryTime?.ToString("yyyy-MM-dd HH:mm") : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@cervicalDilationOnAdmission", item.CervicalDilationOnAdmission != null ? item.CervicalDilationOnAdmission : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@membraneStatus", item.MembraneStatus ?? "Intact");
                saveCmd.Parameters.AddWithValue("@liquorStatus", item.LiquorStatus ?? "Clear");
                saveCmd.Parameters.AddWithValue("@complications", item.Complications ?? "");
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

                if (await saveCmd.ExecuteNonQueryAsync() > 0)
                {
                    if (oldStatus.HasValue && oldStatus.Value != item.Status)
                    {
                        _logger.LogInformation("Partograph {PartographId} status changed from {OldStatus} to {NewStatus}",
                            item.ID, oldStatus, item.Status);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return item.ID;
        }

        public async Task<List<Partograph>> ListAsync(LaborStatus? status = null)
        {
            await Init();
            var partographs = new List<Partograph>();

            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var selectCmd = connection.CreateCommand();
                if (status.HasValue)
                {
                    selectCmd.CommandText = "SELECT P.ID, P.patientID, P.time, P.status, P.gravida, P.parity, P.abortion, P.admissionDate, P.expectedDeliveryDate, P.lastMenstrualDate, P.laborStartTime, P.deliveryTime, P.cervicalDilationOnAdmission, P.membraneStatus, P.liquorStatus, P.complications, P.handler, P.createdtime, P.updatedtime, P.deletedtime, P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion, P.deleted, PA.firstName, PA.lastName, PA.hospitalNumber, PA.dateofbirth, PA.age, PA.bloodGroup, PA.phoneNumber, PA.emergencyContactName, PA.emergencyContactPhone, PA.emergencyContactRelationship FROM Tbl_Partograph P INNER JOIN Tbl_Patient PA ON P.patientID = PA.ID WHERE P.status = @status ORDER BY P.admissionDate DESC";
                    selectCmd.Parameters.AddWithValue("@status", (int)status.Value);
                }
                else
                {
                    selectCmd.CommandText = "SELECT P.ID, P.patientID, P.time, P.status, P.gravida, P.parity, P.abortion, P.admissionDate, P.expectedDeliveryDate, P.lastMenstrualDate, P.laborStartTime, P.deliveryTime, P.cervicalDilationOnAdmission, P.membraneStatus, P.liquorStatus, P.complications, P.handler, P.createdtime, P.updatedtime, P.deletedtime, P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion, P.deleted, PA.firstName, PA.lastName, PA.hospitalNumber, PA.dateofbirth, PA.age, PA.bloodGroup, PA.phoneNumber, PA.emergencyContactName, PA.emergencyContactPhone, PA.emergencyContactRelationship FROM Tbl_Partograph P INNER JOIN Tbl_Patient PA ON P.patientID = PA.ID ORDER BY P.status, P.admissionDate DESC";
                }

                await using var reader = await selectCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    //// Load related data
                    //patient.PartographEntries = await _partographRepository.ListByPatientAsync(patient.ID);
                    //patient.VitalSigns = await _vitalSignRepository.ListByPatientAsync(patient.ID);

                    partographs.Add(MapFromReader(reader));
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return partographs;
        }

        public async Task<Partograph?> GetAsync(Guid? id)
        {
            await Init();
            try
            {

                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT P.ID, P.patientID, P.time, P.status, P.gravida, P.parity, P.abortion, P.admissionDate, P.expectedDeliveryDate, P.lastMenstrualDate, P.laborStartTime, P.deliveryTime, P.cervicalDilationOnAdmission, P.membraneStatus, P.liquorStatus, P.complications, P.handler, P.createdtime, P.updatedtime, P.deletedtime, P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion, P.deleted, PA.firstName, PA.lastName, PA.hospitalNumber, PA.dateofbirth, PA.age, PA.bloodGroup, PA.phoneNumber, PA.emergencyContactName, PA.emergencyContactPhone, PA.emergencyContactRelationship FROM Tbl_Partograph P INNER JOIN Tbl_Patient PA ON P.patientID = PA.ID WHERE P.ID = @id";
                selectCmd.Parameters.AddWithValue("@id", id.ToString());

                await using var reader = await selectCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var patient = MapFromReader(reader);

                    //// Load related data
                    //patient.PartographEntries = await _partographRepository.ListByPatientAsync(patient.ID);
                    //patient.VitalSigns = await _vitalSignRepository.ListByPatientAsync(patient.ID);

                    return patient;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return null;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            await Init();
            var stats = new DashboardStats();

            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                // Get counts by status
                var countCmd = connection.CreateCommand();
                countCmd.CommandText = @"
                SELECT status, COUNT(*) FROM Tbl_Partograph WHERE deleted = 0 GROUP BY status;
                SELECT COUNT(*) FROM Tbl_BirthOutcome WHERE DATE(deliveryTime) = DATE('now') AND deleted = 0;";

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

                // Get average delivery time for completed cases today
                var avgDeliveryCmd = connection.CreateCommand();
                avgDeliveryCmd.CommandText = @"
                SELECT AVG(JULIANDAY(deliveryTime) - JULIANDAY(laborStartTime)) * 24
                FROM Tbl_Partograph
                WHERE deliveryTime IS NOT NULL
                  AND laborStartTime IS NOT NULL
                  AND DATE(deliveryTime) = DATE('now')
                  AND deleted = 0";
                var avgDelivery = await avgDeliveryCmd.ExecuteScalarAsync();
                stats.AvgDeliveryTime = avgDelivery != DBNull.Value && avgDelivery != null
                    ? Math.Round(Convert.ToDouble(avgDelivery), 1) : 0;

                // Get average active labor time
                var avgActiveCmd = connection.CreateCommand();
                avgActiveCmd.CommandText = @"
                SELECT AVG(JULIANDAY(COALESCE(deliveryTime, DATETIME('now'))) - JULIANDAY(laborStartTime)) * 24
                FROM Tbl_Partograph
                WHERE laborStartTime IS NOT NULL
                  AND status IN (@active, @secondstage, @completed)
                  AND deleted = 0";
                avgActiveCmd.Parameters.AddWithValue("@active", (int)LaborStatus.Active);
                avgActiveCmd.Parameters.AddWithValue("@secondstage", (int)LaborStatus.SecondStage);
                avgActiveCmd.Parameters.AddWithValue("@completed", (int)LaborStatus.Completed);
                var avgActive = await avgActiveCmd.ExecuteScalarAsync();
                stats.AvgActiveLaborTime = avgActive != DBNull.Value && avgActive != null
                    ? Math.Round(Convert.ToDouble(avgActive), 1) : 0;

                // Get admissions count
                var admissionsCmd = connection.CreateCommand();
                admissionsCmd.CommandText = @"
                SELECT COUNT(*) FROM Tbl_Partograph
                WHERE DATE(admissionDate) = DATE('now') AND deleted = 0";
                var admToday = await admissionsCmd.ExecuteScalarAsync();
                stats.AdmissionsToday = admToday != null ? Convert.ToInt32(admToday) : 0;

                // Get shift-based admissions (assuming shifts: Morning 7-15, Evening 15-23, Night 23-7)
                var currentHour = DateTime.Now.Hour;
                string shiftStart, shiftEnd;
                if (currentHour >= 7 && currentHour < 15)
                {
                    shiftStart = "07:00:00";
                    shiftEnd = "15:00:00";
                }
                else if (currentHour >= 15 && currentHour < 23)
                {
                    shiftStart = "15:00:00";
                    shiftEnd = "23:00:00";
                }
                else
                {
                    shiftStart = "23:00:00";
                    shiftEnd = "07:00:00";
                }

                var shiftAdmCmd = connection.CreateCommand();
                if (currentHour >= 23 || currentHour < 7) // Night shift crosses midnight
                {
                    shiftAdmCmd.CommandText = @"
                    SELECT COUNT(*) FROM Tbl_Partograph
                    WHERE deleted = 0
                      AND ((DATE(admissionDate) = DATE('now', '-1 day') AND TIME(admissionDate) >= @shiftStart)
                           OR (DATE(admissionDate) = DATE('now') AND TIME(admissionDate) < @shiftEnd))";
                }
                else
                {
                    shiftAdmCmd.CommandText = @"
                    SELECT COUNT(*) FROM Tbl_Partograph
                    WHERE DATE(admissionDate) = DATE('now')
                      AND TIME(admissionDate) >= @shiftStart
                      AND TIME(admissionDate) < @shiftEnd
                      AND deleted = 0";
                }
                shiftAdmCmd.Parameters.AddWithValue("@shiftStart", shiftStart);
                shiftAdmCmd.Parameters.AddWithValue("@shiftEnd", shiftEnd);
                var shiftAdm = await shiftAdmCmd.ExecuteScalarAsync();
                stats.AdmissionsThisShift = shiftAdm != null ? Convert.ToInt32(shiftAdm) : 0;

                // Get critical patients
                stats.CriticalPatients = await GetCriticalPatientsAsync(connection);

                // Get active alerts
                stats.ActiveAlerts = await GetActiveAlertsAsync(connection);

                // Count special categories
                stats.ProlongedLaborCount = stats.CriticalPatients.Count(c => c.HoursInLabor > 12);
                stats.HighRiskCount = stats.CriticalPatients.Count(c => c.Severity >= AlertSeverity.Critical);
                stats.OverdueChecksCount = stats.CriticalPatients.Count(c => c.IsOverdueCheck);

                // Get shift handover items
                stats.ShiftHandoverItems = await GetShiftHandoverItemsAsync(connection);

                // Get WHO compliance metrics
                stats.ComplianceMetrics = await GetWHOComplianceMetricsAsync(connection);

                // Get recent activities
                stats.RecentActivities = await GetRecentActivitiesAsync(connection);

                //// Get resource utilization
                //stats.Resources = await GetResourceUtilizationAsync(connection, stats);

                // Get hourly admission trends
                stats.AdmissionTrends = await GetAdmissionTrendsAsync(connection);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return stats;
        }

        private async Task<List<CriticalPatientInfo>> GetCriticalPatientsAsync(SqliteConnection connection)
        {
            var criticalPatients = new List<CriticalPatientInfo>();

            try
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                SELECT
                    p.ID,
                    pt.firstName || ' ' || pt.lastName as PatientName,
                    pt.hospitalNumber,
                    p.laborStartTime,
                    p.cervicalDilationOnAdmission,
                    p.status,
                    MAX(fhr.time) as LastFHRTime,
                    (SELECT f.value FROM Tbl_FHR f WHERE f.partographID = p.ID ORDER BY f.time DESC LIMIT 1) as LastFHR
                FROM Tbl_Partograph p
                INNER JOIN Tbl_Patient pt ON p.patientID = pt.ID
                LEFT JOIN Tbl_FHR fhr ON fhr.partographID = p.ID
                WHERE p.status IN (@active, @secondstage, @emergency)
                  AND p.deleted = 0
                  AND pt.deleted = 0
                GROUP BY p.ID, pt.firstName, pt.lastName, pt.hospitalNumber, p.laborStartTime, p.cervicalDilationOnAdmission, p.status
                ORDER BY
                    CASE WHEN p.status = @emergency THEN 0 ELSE 1 END,
                    JULIANDAY(DATETIME('now')) - JULIANDAY(p.laborStartTime) DESC
                LIMIT 5";

                cmd.Parameters.AddWithValue("@active", (int)LaborStatus.Active);
                cmd.Parameters.AddWithValue("@secondstage", (int)LaborStatus.SecondStage);
                cmd.Parameters.AddWithValue("@emergency", (int)LaborStatus.Emergency);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var patientId = Guid.Parse(reader.GetString(0));
                    var patientName = reader.GetString(1);
                    var hospitalNumber = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    var laborStart = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3);
                    var dilation = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4);
                    var status = (LaborStatus)reader.GetInt32(5);
                    var lastFHRTime = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6);
                    var lastFHR = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7);

                    var hoursInLabor = laborStart.HasValue
                        ? (int)(DateTime.Now - laborStart.Value).TotalHours
                        : 0;

                    var timeSinceLastCheck = lastFHRTime.HasValue
                        ? (DateTime.Now - lastFHRTime.Value).TotalHours
                        : 999;

                    string reason = "";
                    var severity = AlertSeverity.Info;

                    if (status == LaborStatus.Emergency)
                    {
                        reason = "Emergency Status";
                        severity = AlertSeverity.Emergency;
                    }
                    else if (hoursInLabor > 18)
                    {
                        reason = $"Prolonged labor ({hoursInLabor}h)";
                        severity = AlertSeverity.Critical;
                    }
                    else if (hoursInLabor > 12)
                    {
                        reason = $"Extended labor ({hoursInLabor}h)";
                        severity = AlertSeverity.Warning;
                    }
                    else if (timeSinceLastCheck > 1)
                    {
                        reason = "Overdue for assessment";
                        severity = AlertSeverity.Warning;
                    }
                    else if (lastFHR.HasValue && (lastFHR < 110 || lastFHR > 160))
                    {
                        reason = $"Abnormal FHR ({lastFHR} bpm)";
                        severity = AlertSeverity.Critical;
                    }
                    else
                    {
                        reason = "Active monitoring";
                        severity = AlertSeverity.Info;
                    }

                    criticalPatients.Add(new CriticalPatientInfo
                    {
                        PatientId = patientId,
                        PatientName = patientName,
                        HospitalNumber = hospitalNumber,
                        ReasonForConcern = reason,
                        Severity = severity,
                        HoursInLabor = hoursInLabor,
                        CurrentDilation = dilation,
                        LastFetalHeartRate = lastFHR,
                        LastCheckTime = lastFHRTime ?? DateTime.Now,
                        TimeInLabor = laborStart.HasValue ? Helper.ElapseTimeCalc.PeriodElapseTimeLower(laborStart.Value, DateTime.Now) : "",
                        IsOverdueCheck = timeSinceLastCheck > 1,
                        Status = status
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting critical patients");
            }

            return criticalPatients;
        }

        private async Task<List<PatientAlert>> GetActiveAlertsAsync(SqliteConnection connection)
        {
            var alerts = new List<PatientAlert>();

            try
            {
                // Get patients with abnormal FHR
                var fhrCmd = connection.CreateCommand();
                fhrCmd.CommandText = @"
                SELECT
                    p.ID,
                    pt.firstName || ' ' || pt.lastName as PatientName,
                    fhr.value,
                    fhr.time
                FROM Tbl_Partograph p
                INNER JOIN Tbl_Patient pt ON p.patientID = pt.ID
                INNER JOIN Tbl_FHR fhr ON fhr.partographID = p.ID
                WHERE p.status IN (@active, @secondstage, @emergency)
                  AND p.deleted = 0
                  AND pt.deleted = 0
                  AND (fhr.value < 110 OR fhr.value > 160)
                  AND fhr.time IN (
                      SELECT MAX(f2.time)
                      FROM Tbl_FHR f2
                      WHERE f2.partographID = p.ID
                  )
                ORDER BY fhr.time DESC
                LIMIT 10";

                fhrCmd.Parameters.AddWithValue("@active", (int)LaborStatus.Active);
                fhrCmd.Parameters.AddWithValue("@secondstage", (int)LaborStatus.SecondStage);
                fhrCmd.Parameters.AddWithValue("@emergency", (int)LaborStatus.Emergency);

                await using var fhrReader = await fhrCmd.ExecuteReaderAsync();
                while (await fhrReader.ReadAsync())
                {
                    var patientId = Guid.Parse(fhrReader.GetString(0));
                    var patientName = fhrReader.GetString(1);
                    var fhrValue = fhrReader.GetInt32(2);
                    var alertTime = fhrReader.GetDateTime(3);

                    var severity = fhrValue < 100 || fhrValue > 180
                        ? AlertSeverity.Emergency
                        : AlertSeverity.Critical;

                    alerts.Add(new PatientAlert
                    {
                        AlertId = Guid.NewGuid(),
                        PatientId = patientId,
                        PatientName = patientName,
                        AlertMessage = $"Abnormal fetal heart rate: {fhrValue} bpm",
                        Type = AlertType.FetalHeartRate,
                        Severity = severity,
                        AlertTime = alertTime,
                        TimeAgo = Helper.ElapseTimeCalc.PeriodElapseTimeLower(alertTime, DateTime.Now)
                    });
                }

                // Get prolonged labor alerts
                var prolongedCmd = connection.CreateCommand();
                prolongedCmd.CommandText = @"
                SELECT
                    p.ID,
                    pt.firstName || ' ' || pt.lastName as PatientName,
                    p.laborStartTime
                FROM Tbl_Partograph p
                INNER JOIN Tbl_Patient pt ON p.patientID = pt.ID
                WHERE p.status = @active
                  AND p.deleted = 0
                  AND pt.deleted = 0
                  AND p.laborStartTime IS NOT NULL
                  AND JULIANDAY(DATETIME('now')) - JULIANDAY(p.laborStartTime) > 0.5
                ORDER BY p.laborStartTime ASC
                LIMIT 10";

                prolongedCmd.Parameters.AddWithValue("@active", (int)LaborStatus.Active);

                await using var prolongedReader = await prolongedCmd.ExecuteReaderAsync();
                while (await prolongedReader.ReadAsync())
                {
                    var patientId = Guid.Parse(prolongedReader.GetString(0));
                    var patientName = prolongedReader.GetString(1);
                    var laborStart = prolongedReader.GetDateTime(2);
                    var hoursInLabor = (int)(DateTime.Now - laborStart).TotalHours;

                    var severity = hoursInLabor > 18
                        ? AlertSeverity.Critical
                        : hoursInLabor > 12
                            ? AlertSeverity.Warning
                            : AlertSeverity.Info;

                    alerts.Add(new PatientAlert
                    {
                        AlertId = Guid.NewGuid(),
                        PatientId = patientId,
                        PatientName = patientName,
                        AlertMessage = $"Labor duration: {hoursInLabor} hours",
                        Type = AlertType.ProlongedLabor,
                        Severity = severity,
                        AlertTime = laborStart,
                        TimeAgo = Helper.ElapseTimeCalc.PeriodElapseTimeLower(laborStart, DateTime.Now)
                    });
                }

                // Sort by severity and time
                alerts = alerts
                    .OrderByDescending(a => a.Severity)
                    .ThenByDescending(a => a.AlertTime)
                    .Take(10)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active alerts");
            }

            return alerts;
        }

        private async Task<List<ShiftHandoverItem>> GetShiftHandoverItemsAsync(SqliteConnection connection)
        {
            var handoverItems = new List<ShiftHandoverItem>();

            try
            {
                // Get patients admitted during current shift or requiring handover
                var currentHour = DateTime.Now.Hour;
                string shiftStart;

                if (currentHour >= 7 && currentHour < 15)
                    shiftStart = "07:00:00";
                else if (currentHour >= 15 && currentHour < 23)
                    shiftStart = "15:00:00";
                else
                    shiftStart = "23:00:00";

                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                SELECT
                    p.ID,
                    pt.firstName || ' ' || pt.lastName as PatientName,
                    pt.hospitalNumber,
                    p.admissionDate,
                    p.status,
                    p.laborStartTime
                FROM Tbl_Partograph p
                INNER JOIN Tbl_Patient pt ON p.patientID = pt.ID
                WHERE p.deleted = 0
                  AND pt.deleted = 0
                  AND p.status IN (@active, @secondstage, @pending)
                  AND (
                      (DATE(p.admissionDate) = DATE('now') AND TIME(p.admissionDate) >= @shiftStart)
                      OR p.status = @active
                      OR p.status = @secondstage
                  )
                ORDER BY p.admissionDate DESC
                LIMIT 10";

                cmd.Parameters.AddWithValue("@active", (int)LaborStatus.Active);
                cmd.Parameters.AddWithValue("@secondstage", (int)LaborStatus.SecondStage);
                cmd.Parameters.AddWithValue("@pending", (int)LaborStatus.Pending);
                cmd.Parameters.AddWithValue("@shiftStart", shiftStart);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var patientId = Guid.Parse(reader.GetString(0));
                    var patientName = reader.GetString(1);
                    var hospitalNumber = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    var admissionDate = reader.GetDateTime(3);
                    var status = (LaborStatus)reader.GetInt32(4);
                    var laborStart = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);

                    var hoursInLabor = laborStart.HasValue
                        ? (int)(DateTime.Now - laborStart.Value).TotalHours
                        : 0;

                    string keyNotes = status == LaborStatus.Active
                        ? $"In active labor for {hoursInLabor}h"
                        : status == LaborStatus.SecondStage
                            ? $"In second stage of labor (delivery phase)"
                            : "Awaiting labor onset";

                    handoverItems.Add(new ShiftHandoverItem
                    {
                        PatientId = patientId,
                        PatientName = patientName,
                        HospitalNumber = hospitalNumber,
                        AdmissionTime = admissionDate,
                        Status = status,
                        KeyNotes = keyNotes,
                        HoursInLabor = hoursInLabor,
                        RequiresHandover = status == LaborStatus.Active || status == LaborStatus.SecondStage || hoursInLabor > 8
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shift handover items");
            }

            return handoverItems;
        }

        private async Task<WHOComplianceMetrics> GetWHOComplianceMetricsAsync(SqliteConnection connection)
        {
            var metrics = new WHOComplianceMetrics();

            try
            {
                // Get total active labors
                var activeCmd = connection.CreateCommand();
                activeCmd.CommandText = @"
                SELECT COUNT(*) FROM Tbl_Partograph
                WHERE status = @active AND deleted = 0";
                activeCmd.Parameters.AddWithValue("@active", (int)LaborStatus.Active);
                var activeCount = await activeCmd.ExecuteScalarAsync();
                metrics.TotalActiveLabors = activeCount != null ? Convert.ToInt32(activeCount) : 0;

                // Get completed partographs today
                var completedCmd = connection.CreateCommand();
                completedCmd.CommandText = @"
                SELECT COUNT(*) FROM Tbl_Partograph
                WHERE status = @completed
                  AND DATE(deliveryTime) = DATE('now')
                  AND deleted = 0";
                completedCmd.Parameters.AddWithValue("@completed", (int)LaborStatus.Completed);
                var completed = await completedCmd.ExecuteScalarAsync();
                metrics.PartographsCompleted = completed != null ? Convert.ToInt32(completed) : 0;

                // Estimate compliance metrics (simplified - would need more complex queries for real WHO compliance)
                // For now, we'll use placeholder logic
                metrics.OnTimeAssessments = (int)(metrics.TotalActiveLabors * 0.85); // 85% on time
                metrics.LateAssessments = metrics.TotalActiveLabors - metrics.OnTimeAssessments;
                metrics.ComplianceRate = metrics.TotalActiveLabors > 0
                    ? Math.Round((double)metrics.OnTimeAssessments / metrics.TotalActiveLabors * 100, 1)
                    : 100;

                // Alert/Action line crossings would require analyzing cervical dilation progression
                // Simplified version: check for prolonged labors as proxy
                var prolongedCmd = connection.CreateCommand();
                prolongedCmd.CommandText = @"
                SELECT COUNT(*) FROM Tbl_Partograph
                WHERE status = @active
                  AND laborStartTime IS NOT NULL
                  AND JULIANDAY(DATETIME('now')) - JULIANDAY(laborStartTime) > 0.5
                  AND deleted = 0";
                prolongedCmd.Parameters.AddWithValue("@active", (int)LaborStatus.Active);
                var prolonged = await prolongedCmd.ExecuteScalarAsync();
                metrics.AlertLineCrossings = prolonged != null ? Convert.ToInt32(prolonged) : 0;
                metrics.ActionLineCrossings = (int)(metrics.AlertLineCrossings * 0.3); // Estimate 30% reach action line
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting WHO compliance metrics");
            }

            return metrics;
        }

        private async Task<List<RecentActivityItem>> GetRecentActivitiesAsync(SqliteConnection connection)
        {
            var activities = new List<RecentActivityItem>();

            try
            {
                // Get recent deliveries
                var deliveryCmd = connection.CreateCommand();
                deliveryCmd.CommandText = @"
                SELECT
                    p.ID,
                    pt.firstName || ' ' || pt.lastName as PatientName,
                    p.deliveryTime
                FROM Tbl_Partograph p
                INNER JOIN Tbl_Patient pt ON p.patientID = pt.ID
                WHERE p.deliveryTime IS NOT NULL 
                  AND DATE(p.deliveryTime) BETWEEN DATE('now', '-7 days') AND DATE('now')
                  AND p.deleted = 0
                  AND pt.deleted = 0
                ORDER BY p.deliveryTime DESC
                LIMIT 3";

                await using var deliveryReader = await deliveryCmd.ExecuteReaderAsync();
                while (await deliveryReader.ReadAsync())
                {
                    var patientId = Guid.Parse(deliveryReader.GetString(0));
                    var patientName = deliveryReader.GetString(1);
                    var deliveryTime = deliveryReader.GetDateTime(2);

                    activities.Add(new RecentActivityItem
                    {
                        ActivityId = Guid.NewGuid(),
                        Type = ActivityType.Delivery,
                        PatientName = patientName,
                        Description = $"{patientName} delivered successfully",
                        Timestamp = deliveryTime,
                        TimeAgo = Helper.ElapseTimeCalc.PeriodElapseTimeLower(deliveryTime, DateTime.Now),
                        PatientId = patientId
                    });
                }

                // Get recent admissions
                var admissionCmd = connection.CreateCommand();
                admissionCmd.CommandText = @"
                SELECT
                    p.ID,
                    pt.firstName || ' ' || pt.lastName as PatientName,
                    p.admissionDate
                FROM Tbl_Partograph p
                INNER JOIN Tbl_Patient pt ON p.patientID = pt.ID
                WHERE DATE(p.admissionDate) BETWEEN DATE('now', '-7 days') AND DATE('now')
                  AND p.deleted = 0
                  AND pt.deleted = 0
                ORDER BY p.admissionDate DESC
                LIMIT 3";

                await using var admissionReader = await admissionCmd.ExecuteReaderAsync();
                while (await admissionReader.ReadAsync())
                {
                    var patientId = Guid.Parse(admissionReader.GetString(0));
                    var patientName = admissionReader.GetString(1);
                    var admissionDate = admissionReader.GetDateTime(2);

                    activities.Add(new RecentActivityItem
                    {
                        ActivityId = Guid.NewGuid(),
                        Type = ActivityType.Admission,
                        PatientName = patientName,
                        Description = $"{patientName} admitted to ward",
                        Timestamp = admissionDate,
                        TimeAgo = Helper.ElapseTimeCalc.PeriodElapseTimeLower(admissionDate, DateTime.Now),
                        PatientId = patientId
                    });
                }

                // Sort all activities by timestamp
                activities = activities
                    .OrderByDescending(a => a.Timestamp)
                    .Take(5)
                    .ToList();
            }
            catch (SqliteException ex)
            {
                _logger.LogError(ex, "Error getting recent activities");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activities");
            }

            return activities;
        }

        //private async Task<ResourceUtilization> GetResourceUtilizationAsync(SqliteConnection connection, DashboardStats stats)
        //{
        //    var resources = new ResourceUtilization
        //    {
        //        TotalBeds = 20, // This would typically come from a configuration table
        //        TotalStaff = 6   // This would typically come from staff/shift management
        //    };

        //    try
        //    {
        //        // Calculate occupied beds (active + pending patients)
        //        resources.OccupiedBeds = stats.ActiveLabor + stats.PendingLabor;
        //        resources.AvailableBeds = resources.TotalBeds - resources.OccupiedBeds;
        //        resources.OccupancyRate = resources.TotalBeds > 0
        //            ? Math.Round((double)resources.OccupiedBeds / resources.TotalBeds * 100, 1)
        //            : 0;

        //        resources.ActivePatients = stats.ActiveLabor;
        //        resources.StaffToPatientRatio = resources.ActivePatients > 0
        //            ? Math.Round((double)resources.TotalStaff / resources.ActivePatients, 2)
        //            : resources.TotalStaff;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error calculating resource utilization");
        //    }

        //    return resources;
        //}

        private async Task<List<HourlyAdmission>> GetAdmissionTrendsAsync(SqliteConnection connection)
        {
            var trends = new List<HourlyAdmission>();

            try
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                SELECT
                    CAST(strftime('%H', admissionDate) AS INTEGER) as Hour,
                    COUNT(*) as AdmissionCount
                FROM Tbl_Partograph
                WHERE DATE(admissionDate) = DATE('now')
                  AND deleted = 0
                GROUP BY CAST(strftime('%H', admissionDate) AS INTEGER)
                ORDER BY Hour";

                await using var reader = await cmd.ExecuteReaderAsync();

                // Initialize all 24 hours with 0
                var hourlyData = new Dictionary<int, int>();
                for (int i = 0; i < 24; i++)
                {
                    hourlyData[i] = 0;
                }

                // Fill in actual data
                while (await reader.ReadAsync())
                {
                    var hour = reader.GetInt32(0);
                    var count = reader.GetInt32(1);
                    hourlyData[hour] = count;
                }

                // Convert to list
                foreach (var kvp in hourlyData)
                {
                    trends.Add(new HourlyAdmission
                    {
                        Hour = kvp.Key,
                        HourLabel = $"{kvp.Key:00}:00",
                        AdmissionCount = kvp.Value
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admission trends");
            }

            return trends;
        }

        /// <summary>
        /// Gets the current partograph for a patient (Active or most recent Pending)
        /// </summary>
        public async Task<Partograph?> GetCurrentPartographAsync(Guid? patientId)
        {
            await Init();
            Partograph? currentPartograph = null;

            try
            {

                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                // First try to find an active partograph
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = @"
                SELECT P.ID, P.patientID, P.time, P.status, P.gravida, P.parity, P.abortion, P.admissionDate, P.expectedDeliveryDate, P.lastMenstrualDate, P.laborStartTime, P.deliveryTime, P.cervicalDilationOnAdmission, P.membraneStatus, P.liquorStatus, P.complications, P.handler, P.createdtime, P.updatedtime, P.deletedtime, P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion, P.deleted
                FROM Tbl_Partograph P
                WHERE P.patientID = @patientID
                  AND P.deleted = 0
                  AND P.status = @activeStatus
                ORDER BY P.admissionDate DESC
                LIMIT 1";
                selectCmd.Parameters.AddWithValue("@patientID", patientId.ToString());
                selectCmd.Parameters.AddWithValue("@activeStatus", (int)LaborStatus.Active);

                await using var reader = await selectCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    currentPartograph = SimpleMapFromReader(reader);
                }

                // If no active partograph, get the most recent pending one
                if (currentPartograph == null)
                {
                    selectCmd = connection.CreateCommand();
                    selectCmd.CommandText = @"
                    SELECT P.ID, P.patientID, P.time, P.status, P.gravida, P.parity, P.admissionDate, P.expectedDeliveryDate, P.lastMenstrualDate, P.laborStartTime, P.deliveryTime, P.cervicalDilationOnAdmission, P.membraneStatus, P.liquorStatus, P.complications, P.handler, P.createdtime, P.updatedtime, P.deletedtime, P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion, P.deleted
                    FROM Tbl_Partograph P
                    WHERE P.patientID = @patientID
                      AND P.deleted = 0
                      AND P.status = @pendingStatus
                    ORDER BY P.admissionDate DESC
                    LIMIT 1";
                    selectCmd.Parameters.AddWithValue("@patientID", patientId.ToString());
                    selectCmd.Parameters.AddWithValue("@pendingStatus", (int)LaborStatus.Pending);

                    await using var reader2 = await selectCmd.ExecuteReaderAsync();
                    if (await reader2.ReadAsync())
                    {
                        currentPartograph = SimpleMapFromReader(reader2);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return currentPartograph;
        }

        protected Partograph SimpleMapFromReader(SqliteDataReader reader)
        {
            return new Partograph
            {
                ID = Guid.Parse(reader.GetString(0)),
                PatientID = reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                Time = DateTime.Parse(reader.GetString(2)),
                Status = (LaborStatus)reader.GetInt32(3),
                Gravida = reader.GetInt32(4),
                Parity = reader.GetInt32(5),
                Abortion = reader.GetInt32(6),
                AdmissionDate = DateTime.Parse(reader.GetString(7)),
                ExpectedDeliveryDate = reader.IsDBNull(8) ? null : DateOnly.Parse(reader.GetString(8)),
                LastMenstrualDate = reader.IsDBNull(9) ? null : DateOnly.Parse(reader.GetString(9)),
                LaborStartTime = reader.IsDBNull(10) ? null : DateTime.Parse(reader.GetString(10)),
                DeliveryTime = reader.IsDBNull(11) ? null : DateTime.Parse(reader.GetString(11)),
                CervicalDilationOnAdmission = reader.IsDBNull(12) ? null : reader.GetInt32(12),
                MembraneStatus = reader.IsDBNull(13) ? "Intact" : reader.GetString(13),
                LiquorStatus = reader.IsDBNull(14) ? "Clear" : reader.GetString(14),
                Complications = reader.IsDBNull(15) ? "" : reader.GetString(15),
                Handler = reader.IsDBNull(16) ? null : Guid.Parse(reader.GetString(16)),
                CreatedTime = reader.GetInt64(17),
                UpdatedTime = reader.GetInt64(18),
                DeletedTime = reader.IsDBNull(19) ? null : reader.GetInt64(19),
                DeviceId = reader.GetString(20),
                OriginDeviceId = reader.GetString(21),
                SyncStatus = reader.GetInt32(22),
                Version = reader.GetInt32(23),
                ServerVersion = reader.IsDBNull(24) ? 0 : reader.GetInt32(24),
                Deleted = reader.IsDBNull(25) ? 0 : reader.GetInt32(25)
            };
        }

        /// <summary>
        /// Upserts a partograph record (insert if new, update if exists) - used for sync operations
        /// </summary>
        public async Task UpsertPartographAsync(Partograph partograph)
        {
            await Init();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                // Check if record exists
                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM Tbl_Partograph WHERE ID = @id";
                checkCmd.Parameters.AddWithValue("@id", partograph.ID.ToString());
                var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

                var cmd = connection.CreateCommand();

                if (exists)
                {
                    // Update existing record - only if server version is newer or equal
                    cmd.CommandText = @"
                    UPDATE Tbl_Partograph SET
                        patientID = @patientID,
                        time = @time,
                        status = @status,
                        gravida = @gravida,
                        parity = @parity,
                        abortion = @abortion,
                        admissionDate = @admissionDate,
                        expectedDeliveryDate = @expectedDeliveryDate,
                        lastMenstrualDate = @lastMenstrualDate,
                        laborStartTime = @laborStartTime,
                        deliveryTime = @deliveryTime,
                        cervicalDilationOnAdmission = @cervicalDilationOnAdmission,
                        membraneStatus = @membraneStatus,
                        liquorStatus = @liquorStatus,
                        complications = @complications,
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
                    INSERT INTO Tbl_Partograph (ID, patientID, time, status, gravida, parity, abortion, admissionDate, expectedDeliveryDate, lastMenstrualDate, laborStartTime, deliveryTime, cervicalDilationOnAdmission, membraneStatus, liquorStatus, complications, handler, createdtime, updatedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash) VALUES (@ID, @patientID, @time, @status, @gravida, @parity, @abortion, @admissionDate, @expectedDeliveryDate, @lastMenstrualDate, @laborStartTime, @deliveryTime, @cervicalDilationOnAdmission, @membraneStatus, @liquorStatus, @complications, @handler, @createdtime, @updatedtime, @deviceid, @origindeviceid, 1, @version, @serverversion, @deleted, @datahash)";
                }

                cmd.Parameters.AddWithValue("@ID", partograph.ID.ToString());
                cmd.Parameters.AddWithValue("@patientID", partograph.PatientID?.ToString() ?? "");
                cmd.Parameters.AddWithValue("@time", partograph.Time.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@status", partograph.Status.ToString());
                cmd.Parameters.AddWithValue("@gravida", partograph.Gravida);
                cmd.Parameters.AddWithValue("@parity", partograph.Parity);
                cmd.Parameters.AddWithValue("@abortion", partograph.Abortion);
                cmd.Parameters.AddWithValue("@admissionDate", partograph.AdmissionDate.ToString("yyyy-MM-dd HH:mm"));

                cmd.Parameters.AddWithValue("@expectedDeliveryDate", partograph.ExpectedDeliveryDate != null ? partograph.ExpectedDeliveryDate?.ToString("yyyy-MM-dd") : DBNull.Value);

                cmd.Parameters.AddWithValue("@lastMenstrualDate", partograph.LastMenstrualDate != null ? partograph.LastMenstrualDate?.ToString("yyyy-MM-dd") : DBNull.Value);

                cmd.Parameters.AddWithValue("@laborStartTime", partograph.LaborStartTime != null ? partograph.LaborStartTime?.ToString("yyyy-MM-dd HH:mm") : DBNull.Value);

                cmd.Parameters.AddWithValue("@deliveryTime", partograph.DeliveryTime != null ? partograph.DeliveryTime?.ToString("yyyy-MM-dd HH:mm") : DBNull.Value);

                cmd.Parameters.AddWithValue("@cervicalDilationOnAdmission", partograph.CervicalDilationOnAdmission ?? 0);
                cmd.Parameters.AddWithValue("@membraneStatus", partograph.MembraneStatus ?? "");
                cmd.Parameters.AddWithValue("@liquorStatus", partograph.LiquorStatus ?? "");
                cmd.Parameters.AddWithValue("@complications", partograph.Complications ?? "");
                cmd.Parameters.AddWithValue("@handler", partograph.Handler?.ToString() ?? "");
                cmd.Parameters.AddWithValue("@createdtime", partograph.CreatedTime);
                cmd.Parameters.AddWithValue("@updatedtime", partograph.UpdatedTime);
                cmd.Parameters.AddWithValue("@deviceid", partograph.DeviceId ?? "");
                cmd.Parameters.AddWithValue("@origindeviceid", partograph.OriginDeviceId ?? "");
                cmd.Parameters.AddWithValue("@version", partograph.Version);
                cmd.Parameters.AddWithValue("@serverversion", partograph.ServerVersion);
                cmd.Parameters.AddWithValue("@deleted", partograph.Deleted);
                cmd.Parameters.AddWithValue("@datahash", partograph.DataHash ?? "");

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
