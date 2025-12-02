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
            
            //    try
            //{
            //    var dropTableCmd = connection.CreateCommand();
            //    dropTableCmd.CommandText = @"
            //    DELETE FROM Tbl_Companion;";
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
                    deliveryTime TEXT,
                    cervicalDilationOnAdmission INTEGER,
                    membraneStatus TEXT,
                    liquorStatus TEXT, 
                    riskFactors TEXT,
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
                selectCmd.CommandText = "SELECT P.ID, P.patientID, P.time, P.status, P.gravida, P.parity, P.abortion, P.admissionDate, P.expectedDeliveryDate, P.laborStartTime, P.deliveryTime, P.cervicalDilationOnAdmission, P.membraneStatus, P.liquorStatus, P.riskFactors, P.complications, P.handler, P.createdtime, P.updatedtime, P.deletedtime, P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion, P.deleted, PA.firstName, PA.lastName, PA.hospitalNumber, PA.dateofbirth, PA.age, PA.bloodGroup, PA.phoneNumber, PA.emergencyContactName, PA.emergencyContactPhone, PA.emergencyContactRelationship FROM Tbl_Partograph P INNER JOIN Tbl_Patient PA ON P.patientID = PA.ID WHERE P.ID = @Id ORDER BY P.time DESC";

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
                RiskFactors = reader.IsDBNull(15) ? "" : reader.GetString(15),
                Complications = reader.IsDBNull(16) ? "" : reader.GetString(16),
                Handler = reader.IsDBNull(17) ? null : Guid.Parse(reader.GetString(17)),
                CreatedTime = reader.GetInt64(18),
                UpdatedTime = reader.GetInt64(19),
                DeletedTime = reader.IsDBNull(20) ? null : reader.GetInt64(20),
                DeviceId = reader.GetString(21),
                OriginDeviceId = reader.GetString(22),
                SyncStatus = reader.GetInt32(23),
                Version = reader.GetInt32(24),
                ServerVersion = reader.IsDBNull(25) ? 0 : reader.GetInt32(25),
                Deleted = reader.IsDBNull(26) ? 0 : reader.GetInt32(26),
                Patient = new Patient
                {
                    ID = reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                    FirstName = reader.GetString(27),
                    LastName = reader.GetString(28),
                    HospitalNumber = reader.GetString(29),
                    DateOfBirth = reader.IsDBNull(30) ? null : DateOnly.Parse(reader.GetString(30)),
                    Age = reader.IsDBNull(31) ? null : int.Parse(reader.GetString(31)),
                    BloodGroup = reader.IsDBNull(32) ? "" : reader.GetString(32),
                    PhoneNumber = reader.IsDBNull(33) ? "" : reader.GetString(33),
                    EmergencyContactName = reader.IsDBNull(34) ? "" : reader.GetString(34),
                    EmergencyContactPhone = reader.IsDBNull(35) ? "" : reader.GetString(35),
                    EmergencyContactRelationship = reader.IsDBNull(36) ? "" : reader.GetString(36)
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

                // Validate status transition for Active status - prevent multiple active partographs per patient
                if (item.Status == LaborStatus.Active && oldStatus != LaborStatus.Active)
                {
                    var checkCmd = connection.CreateCommand();
                    checkCmd.CommandText = @"
                    SELECT COUNT(*) FROM Tbl_Partograph
                    WHERE patientID = @patientID
                    AND ID != @currentID
                    AND status = @status
                    AND deleted = 0";
                    checkCmd.Parameters.AddWithValue("@patientID", item.PatientID.ToString());
                    checkCmd.Parameters.AddWithValue("@currentID", item.ID.ToString());
                    checkCmd.Parameters.AddWithValue("@status", (int)LaborStatus.Active);

                    var hasActivePartograph = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

                    if (hasActivePartograph)
                    {
                        _logger.LogWarning("Patient {PatientId} already has an active partograph. Cannot activate partograph {PartographId}",
                            item.PatientID, item.ID);
                        throw new InvalidOperationException($"Patient {item.PatientID} already has an active partograph. Please complete the existing one first.");
                    }
                }

                // Set LaborStartTime when transitioning to Active status
                if (item.Status == LaborStatus.Active && oldStatus != LaborStatus.Active && !item.LaborStartTime.HasValue)
                {
                    item.LaborStartTime = DateTime.UtcNow;
                    _logger.LogInformation($"Labor started for partograph {item.ID} at {item.LaborStartTime}");
                }

                // Set DeliveryTime when transitioning to Completed status
                if (item.Status == LaborStatus.Completed && oldStatus != LaborStatus.Completed && !item.DeliveryTime.HasValue)
                {
                    item.DeliveryTime = DateTime.UtcNow;
                    _logger.LogInformation($"Delivery completed for partograph {item.ID} at {item.DeliveryTime}");
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
                    INSERT INTO Tbl_Partograph (ID, patientID, time, status, gravida, parity, abortion, admissionDate, expectedDeliveryDate, lastMenstrualDate, laborStartTime, deliveryTime, cervicalDilationOnAdmission, membraneStatus, liquorStatus, riskFactors, complications, handler, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted)
                    VALUES (@ID, @patientID, @time, @status, @gravida, @parity, @abortion, @admissionDate, @expectedDeliveryDate, @lastMenstrualDate, @laborStartTime, @deliveryTime, @cervicalDilationOnAdmission, @membraneStatus, @liquorStatus, @riskFactors, @complications, @handler, @createdtime, @updatedtime, @deletedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted)";
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
                        riskFactors = @riskFactors,
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
                saveCmd.Parameters.AddWithValue("@riskFactors", item.RiskFactors ?? "");
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
                    selectCmd.CommandText = "SELECT P.ID, P.patientID, P.time, P.status, P.gravida, P.parity, P.abortion, P.admissionDate, P.expectedDeliveryDate, P.lastMenstrualDate, P.laborStartTime, P.deliveryTime, P.cervicalDilationOnAdmission, P.membraneStatus, P.liquorStatus, P.riskFactors, P.complications, P.handler, P.createdtime, P.updatedtime, P.deletedtime, P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion, P.deleted, PA.firstName, PA.lastName, PA.hospitalNumber, PA.dateofbirth, PA.age, PA.bloodGroup, PA.phoneNumber, PA.emergencyContactName, PA.emergencyContactPhone, PA.emergencyContactRelationship FROM Tbl_Partograph P INNER JOIN Tbl_Patient PA ON P.patientID = PA.ID WHERE P.status = @status ORDER BY P.admissionDate DESC";
                    selectCmd.Parameters.AddWithValue("@status", (int)status.Value);
                }
                else
                {
                    selectCmd.CommandText = "SELECT P.ID, P.patientID, P.time, P.status, P.gravida, P.parity, P.abortion, P.admissionDate, P.expectedDeliveryDate, P.lastMenstrualDate, P.laborStartTime, P.deliveryTime, P.cervicalDilationOnAdmission, P.membraneStatus, P.liquorStatus, P.riskFactors, P.complications, P.handler, P.createdtime, P.updatedtime, P.deletedtime, P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion, P.deleted, PA.firstName, PA.lastName, PA.hospitalNumber, PA.dateofbirth, PA.age, PA.bloodGroup, PA.phoneNumber, PA.emergencyContactName, PA.emergencyContactPhone, PA.emergencyContactRelationship FROM Tbl_Partograph P INNER JOIN Tbl_Patient PA ON P.patientID = PA.ID ORDER BY P.status, P.admissionDate DESC";
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
                selectCmd.CommandText = "SELECT P.ID, P.patientID, P.time, P.status, P.gravida, P.parity, P.abortion, P.admissionDate, P.expectedDeliveryDate, P.lastMenstrualDate, P.laborStartTime, P.deliveryTime, P.cervicalDilationOnAdmission, P.membraneStatus, P.liquorStatus, P.riskFactors, P.complications, P.handler, P.createdtime, P.updatedtime, P.deletedtime, P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion, P.deleted, PA.firstName, PA.lastName, PA.hospitalNumber, PA.dateofbirth, PA.age, PA.bloodGroup, PA.phoneNumber, PA.emergencyContactName, PA.emergencyContactPhone, PA.emergencyContactRelationship FROM Tbl_Partograph P INNER JOIN Tbl_Patient PA ON P.patientID = PA.ID WHERE P.ID = @id";
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
                SELECT status, COUNT(*) FROM Tbl_Partograph GROUP BY status;
                SELECT COUNT(*) FROM Tbl_Partograph WHERE DATE(deliveryTime) = DATE('now');";

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
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return stats;
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
                SELECT P.ID, P.patientID, P.time, P.status, P.gravida, P.parity, P.abortion, P.admissionDate, P.expectedDeliveryDate, P.lastMenstrualDate, P.laborStartTime, P.deliveryTime, P.cervicalDilationOnAdmission, P.membraneStatus, P.liquorStatus, P.riskFactors, P.complications, P.handler, P.createdtime, P.updatedtime, P.deletedtime, P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion, P.deleted
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
                    SELECT P.ID, P.patientID, P.time, P.status, P.gravida, P.parity, P.admissionDate, P.expectedDeliveryDate, P.lastMenstrualDate, P.laborStartTime, P.deliveryTime, P.cervicalDilationOnAdmission, P.membraneStatus, P.liquorStatus, P.riskFactors, P.complications, P.handler, P.createdtime, P.updatedtime, P.deletedtime, P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion, P.deleted
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
                RiskFactors = reader.IsDBNull(15) ? "" : reader.GetString(15),
                Complications = reader.IsDBNull(16) ? "" : reader.GetString(16),
                Handler = reader.IsDBNull(17) ? null : Guid.Parse(reader.GetString(17)),
                CreatedTime = reader.GetInt64(18),
                UpdatedTime = reader.GetInt64(19),
                DeletedTime = reader.IsDBNull(20) ? null : reader.GetInt64(20),
                DeviceId = reader.GetString(21),
                OriginDeviceId = reader.GetString(22),
                SyncStatus = reader.GetInt32(23),
                Version = reader.GetInt32(24),
                ServerVersion = reader.IsDBNull(25) ? 0 : reader.GetInt32(25),
                Deleted = reader.IsDBNull(26) ? 0 : reader.GetInt32(26)
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
                        riskFactors = @riskFactors,
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
                    INSERT INTO Tbl_Partograph (ID, patientID, time, status, gravida, parity, abortion, admissionDate, expectedDeliveryDate, lastMenstrualDate, laborStartTime, deliveryTime, cervicalDilationOnAdmission, membraneStatus, liquorStatus, riskFactors, complications, handler, createdtime, updatedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, datahash) VALUES (@ID, @patientID, @time, @status, @gravida, @parity, @abortion, @admissionDate, @expectedDeliveryDate, @lastMenstrualDate, @laborStartTime, @deliveryTime, @cervicalDilationOnAdmission, @membraneStatus, @liquorStatus, @riskFactors, @complications, @handler, @createdtime, @updatedtime, @deviceid, @origindeviceid, 1, @version, @serverversion, @deleted, @datahash)";
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
                cmd.Parameters.AddWithValue("@riskFactors", partograph.RiskFactors ?? "");
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
