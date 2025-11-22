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
                    admissionDate TEXT NOT NULL,
                    expectedDeliveryDate TEXT,
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
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT P.ID, P.patientID, P.time, P.status, P.gravida, P.parity, P.admissionDate, P.expectedDeliveryDate, P.laborStartTime, P.deliveryTime, P.cervicalDilationOnAdmission, P.membraneStatus, P.liquorStatus, P.riskFactors, P.complications, P.handler, P.createdtime, P.updatedtime, P.deletedtime, P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion, P.deleted, PA.firstName, PA.lastName, PA.hospitalNumber, PA.dateofbirth, PA.age, PA.bloodGroup, PA.phoneNumber, PA.emergencyContact FROM Tbl_Partograph P INNER JOIN Tbl_Patient PA ON P.patientID = PA.ID WHERE P.ID = @Id ORDER BY P.Time DESC";

            selectCmd.Parameters.AddWithValue("@Id", patientId);

            var entries = new List<Partograph>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                entries.Add(new Partograph
                {
                    ID = Guid.Parse(reader.GetString(0)),
                    PatientID = reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                    Time = DateTime.Parse(reader.GetString(2)),
                    Status = (LaborStatus)reader.GetInt32(3),
                    Gravida = reader.GetInt32(5),
                    Parity = reader.GetInt32(6),
                    AdmissionDate = DateTime.Parse(reader.GetString(7)),
                    ExpectedDeliveryDate = reader.IsDBNull(8) ? null : DateTime.Parse(reader.GetString(8)),
                    LaborStartTime = reader.IsDBNull(9) ? null : DateTime.Parse(reader.GetString(9)),
                    DeliveryTime = reader.IsDBNull(10) ? null : DateTime.Parse(reader.GetString(10)),
                    CervicalDilationOnAdmission = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                    MembraneStatus = reader.IsDBNull(12) ? "Intact" : reader.GetString(12),
                    LiquorStatus = reader.IsDBNull(13) ? "Clear" : reader.GetString(13),
                    RiskFactors = reader.IsDBNull(14) ? "" : reader.GetString(14),
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
                        DateOfBirth = reader.IsDBNull(29) ? null : DateTime.Parse(reader.GetString(29)),
                        Age = reader.IsDBNull(30) ? null : int.Parse(reader.GetString(30)),
                        BloodGroup = reader.IsDBNull(31) ? "" : reader.GetString(31),
                        PhoneNumber = reader.IsDBNull(32) ? "" : reader.GetString(32),
                        EmergencyContact = reader.IsDBNull(33) ? "" : reader.GetString(33)
                    },
                    //CervicalDilation = reader.GetInt32(3),
                    //DescentOfHead = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    //ContractionsPerTenMinutes = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                    //ContractionDuration = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                    //ContractionStrength = reader.IsDBNull(7) ? "Moderate" : reader.GetString(7),
                    //FetalHeartRate = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                    //LiquorStatus = reader.IsDBNull(9) ? "Clear" : reader.GetString(9),
                    //Moulding = reader.IsDBNull(10) ? "None" : reader.GetString(10),
                    //Caput = reader.IsDBNull(11) ? "None" : reader.GetString(11),
                    //MedicationsGiven = reader.IsDBNull(12) ? "" : reader.GetString(12),
                    //OxytocinUnits = reader.IsDBNull(13) ? "" : reader.GetString(13),
                    //IVFluids = reader.IsDBNull(14) ? "" : reader.GetString(14),
                    //RecordedBy = reader.IsDBNull(15) ? "" : reader.GetString(15),
                    //Notes = reader.IsDBNull(16) ? "" : reader.GetString(16)
                });
            }

            return entries;
        }

        public async Task<Guid?> SaveItemAsync(Partograph item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();
            if (item.ID == null)
            {
                saveCmd.CommandText = @"
                INSERT INTO Tbl_Partograph (ID, patientID, status, time, gravida, parity, admissionDate, expectedDeliveryDate, laborStartTime, deliveryTime, cervicalDilationOnAdmission, membraneStatus, liquorStatus, riskFactors, complications)
                VALUES (@ID, @patientID, @status, @time, @gravida, @parity, @admissionDate, @expectedDeliveryDate, @laborStartTime, @deliveryTime, @cervicalDilationOnAdmission, @membraneStatus, @liquorStatus, @riskFactors, @complications;";
            }
            else
            {
                saveCmd.CommandText = @"
                UPDATE Tbl_Partograph SET 
                    time = @time, status = @status, gravida = @gravida, parity = @parity, admissionDate = @admissionDate, expectedDeliveryDate = @expectedDeliveryDate, laborStartTime = @laborStartTime, deliveryTime = @deliveryTime, cervicalDilationOnAdmission = @cervicalDilationOnAdmission, membraneStatus = @membraneStatus, liquorStatus = @liquorStatus, riskFactors = @riskFactors, complications = @complications
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

            saveCmd.Parameters.AddWithValue("@ID", item.ID);
            saveCmd.Parameters.AddWithValue("@handler", item.Handler.ToString());
            saveCmd.Parameters.AddWithValue("@patientID", item.PatientID);
            saveCmd.Parameters.AddWithValue("@status", (int)item.Status);
            saveCmd.Parameters.AddWithValue("@gravida", item.Gravida);
            saveCmd.Parameters.AddWithValue("@parity", item.Parity);
            saveCmd.Parameters.AddWithValue("@admissionDate", item.AdmissionDate.ToString("O"));
            saveCmd.Parameters.AddWithValue("@expectedDeliveryDate", item.ExpectedDeliveryDate?.ToString("O") ?? (object)DBNull.Value);
            saveCmd.Parameters.AddWithValue("@membraneStatus", item.MembraneStatus);
            saveCmd.Parameters.AddWithValue("@liquorStatus", item.LiquorStatus);
            saveCmd.Parameters.AddWithValue("@riskFactors", item.RiskFactors ?? "");
            saveCmd.Parameters.AddWithValue("@complications", item.Complications ?? "");
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
                item.ID = Guid.Parse(result.ToString());
            }

            return item.ID;
        }

        public async Task<List<Partograph>> ListAsync(LaborStatus? status = null)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            if (status.HasValue)
            {
                selectCmd.CommandText = "SELECT P.ID, P.patientID, P.time, P.status, P.gravida, P.parity, P.admissionDate, P.expectedDeliveryDate, P.laborStartTime, P.deliveryTime, P.cervicalDilationOnAdmission, P.membraneStatus, P.liquorStatus, P.riskFactors, P.complications, P.handler, P.createdtime, P.updatedtime, P.deletedtime, P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion, P.deleted, PA.firstName, PA.lastName, PA.hospitalNumber, PA.dateofbirth, PA.age, PA.bloodGroup, PA.phoneNumber, PA.emergencyContact FROM Tbl_Partograph P INNER JOIN Tbl_Patient PA ON P.patientID = PA.ID WHERE P.status = @status ORDER BY P.admissionDate DESC";
                selectCmd.Parameters.AddWithValue("@status", (int)status.Value);
            }
            else
            {
                selectCmd.CommandText = "SELECT P.ID, P.patientID, P.time, P.status, P.gravida, P.parity, P.admissionDate, P.expectedDeliveryDate, P.laborStartTime, P.deliveryTime, P.cervicalDilationOnAdmission, P.membraneStatus, P.liquorStatus, P.riskFactors, P.complications, P.handler, P.createdtime, P.updatedtime, P.deletedtime, P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion, P.deleted, PA.firstName, PA.lastName, PA.hospitalNumber, PA.dateofbirth, PA.age, PA.bloodGroup, PA.phoneNumber, PA.emergencyContact FROM Tbl_Partograph P INNER JOIN Tbl_Patient PA ON P.patientID = PA.ID ORDER BY P.status, P.admissionDate DESC";
            }

            var partographs = new List<Partograph>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var partograph = new Partograph()
                {
                    ID = Guid.Parse(reader.GetString(0)),
                    PatientID = reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                    Time = DateTime.Parse(reader.GetString(2)),
                    Status = (LaborStatus)reader.GetInt32(3),
                    Gravida = reader.GetInt32(5),
                    Parity = reader.GetInt32(6),
                    AdmissionDate = DateTime.Parse(reader.GetString(7)),
                    ExpectedDeliveryDate = reader.IsDBNull(8) ? null : DateTime.Parse(reader.GetString(8)),
                    LaborStartTime = reader.IsDBNull(9) ? null : DateTime.Parse(reader.GetString(9)),
                    DeliveryTime = reader.IsDBNull(10) ? null : DateTime.Parse(reader.GetString(10)),
                    CervicalDilationOnAdmission = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                    MembraneStatus = reader.IsDBNull(12) ? "Intact" : reader.GetString(12),
                    LiquorStatus = reader.IsDBNull(13) ? "Clear" : reader.GetString(13),
                    RiskFactors = reader.IsDBNull(14) ? "" : reader.GetString(14),
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
                        DateOfBirth = reader.IsDBNull(29) ? null : DateTime.Parse(reader.GetString(29)),
                        Age = reader.IsDBNull(30) ? null : int.Parse(reader.GetString(30)),
                        BloodGroup = reader.IsDBNull(31) ? "" : reader.GetString(31),
                        PhoneNumber = reader.IsDBNull(32) ? "" : reader.GetString(32),
                        EmergencyContact = reader.IsDBNull(33) ? "" : reader.GetString(33)
                    },
                };

                //// Load related data
                //patient.PartographEntries = await _partographRepository.ListByPatientAsync(patient.ID);
                //patient.VitalSigns = await _vitalSignRepository.ListByPatientAsync(patient.ID);

                partographs.Add(partograph);
            }

            return partographs;
        }

        public async Task<Partograph?> GetAsync(Guid? id)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT P.ID, P.patientID, P.time, P.status, P.gravida, P.parity, P.admissionDate, P.expectedDeliveryDate, P.laborStartTime, P.deliveryTime, P.cervicalDilationOnAdmission, P.membraneStatus, P.liquorStatus, P.riskFactors, P.complications, P.handler, P.createdtime, P.updatedtime, P.deletedtime, P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion, P.deleted, PA.firstName, PA.lastName, PA.hospitalNumber, PA.dateofbirth, PA.age, PA.bloodGroup, PA.phoneNumber, PA.emergencyContact FROM Tbl_Partograph P INNER JOIN Tbl_Patient PA ON P.patientID = PA.ID WHERE P.ID = @id";
            selectCmd.Parameters.AddWithValue("@id", id);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var patient = new Partograph()
                {
                    ID = Guid.Parse(reader.GetString(0)),
                    PatientID = reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                    Time = DateTime.Parse(reader.GetString(2)),
                    Status = (LaborStatus)reader.GetInt32(3),
                    Gravida = reader.GetInt32(5),
                    Parity = reader.GetInt32(6),
                    AdmissionDate = DateTime.Parse(reader.GetString(7)),
                    ExpectedDeliveryDate = reader.IsDBNull(8) ? null : DateTime.Parse(reader.GetString(8)),
                    LaborStartTime = reader.IsDBNull(9) ? null : DateTime.Parse(reader.GetString(9)),
                    DeliveryTime = reader.IsDBNull(10) ? null : DateTime.Parse(reader.GetString(10)),
                    CervicalDilationOnAdmission = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                    MembraneStatus = reader.IsDBNull(12) ? "Intact" : reader.GetString(12),
                    LiquorStatus = reader.IsDBNull(13) ? "Clear" : reader.GetString(13),
                    RiskFactors = reader.IsDBNull(14) ? "" : reader.GetString(14),
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
                        DateOfBirth = reader.IsDBNull(29) ? null : DateTime.Parse(reader.GetString(29)),
                        Age = reader.IsDBNull(30) ? null : int.Parse(reader.GetString(30)),
                        BloodGroup = reader.IsDBNull(31) ? "" : reader.GetString(31),
                        PhoneNumber = reader.IsDBNull(32) ? "" : reader.GetString(32),
                        EmergencyContact = reader.IsDBNull(33) ? "" : reader.GetString(33)
                    },
                };

                //// Load related data
                //patient.PartographEntries = await _partographRepository.ListByPatientAsync(patient.ID);
                //patient.VitalSigns = await _vitalSignRepository.ListByPatientAsync(patient.ID);

                return patient;
            }

            return null;
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
                SELECT Status, COUNT(*) FROM Tbl_Partograph GROUP BY status;
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

            return stats;
        }

        /// <summary>
        /// Upserts a partograph record (insert if new, update if exists) - used for sync operations
        /// </summary>
        public async Task UpsertPartographAsync(Partograph partograph)
        {
            await Init();
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
                        admissionDate = @admissionDate,
                        expectedDeliveryDate = @expectedDeliveryDate,
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
                    INSERT INTO Tbl_Partograph (
                        ID, patientID, time, status, gravida, parity,
                        admissionDate, expectedDeliveryDate, laborStartTime, deliveryTime,
                        cervicalDilationOnAdmission, membraneStatus, liquorStatus,
                        riskFactors, complications, handler,
                        createdtime, updatedtime, deviceid, origindeviceid,
                        syncstatus, version, serverversion, deleted, datahash
                    ) VALUES (
                        @ID, @patientID, @time, @status, @gravida, @parity,
                        @admissionDate, @expectedDeliveryDate, @laborStartTime, @deliveryTime,
                        @cervicalDilationOnAdmission, @membraneStatus, @liquorStatus,
                        @riskFactors, @complications, @handler,
                        @createdtime, @updatedtime, @deviceid, @origindeviceid,
                        1, @version, @serverversion, @deleted, @datahash
                    )";
            }

            cmd.Parameters.AddWithValue("@ID", partograph.ID.ToString());
            cmd.Parameters.AddWithValue("@patientID", partograph.PatientID?.ToString() ?? "");
            cmd.Parameters.AddWithValue("@time", partograph.Time.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@status", partograph.Status.ToString());
            cmd.Parameters.AddWithValue("@gravida", partograph.Gravida);
            cmd.Parameters.AddWithValue("@parity", partograph.Parity);
            cmd.Parameters.AddWithValue("@admissionDate", partograph.AdmissionDate.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@expectedDeliveryDate", partograph.ExpectedDeliveryDate?.ToString("yyyy-MM-dd") ?? "");
            cmd.Parameters.AddWithValue("@laborStartTime", partograph.LaborStartTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
            cmd.Parameters.AddWithValue("@deliveryTime", partograph.DeliveryTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
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
    }
}
