using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class StaffRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;

        public StaffRepository(ILogger<StaffRepository> logger)
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
                createTableCmd.CommandText = @"CREATE TABLE IF NOT EXISTS Tbl_Staff (
                    ID TEXT PRIMARY KEY,
                    name TEXT NOT NULL,
                    staffid TEXT NOT NULL UNIQUE,
                    email TEXT NOT NULL UNIQUE,
                    role TEXT NOT NULL,
                    department TEXT NOT NULL,
                    password TEXT NOT NULL,
                    lastlogin TEXT,
                    active INTEGER NOT NULL DEFAULT 1, 
                    facility TEXT NOT NULL,              
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
            
                CREATE INDEX IF NOT EXISTS idx_staff_sync ON Tbl_Staff(updatedtime, syncstatus);
                CREATE INDEX IF NOT EXISTS idx_staff_server_version ON Tbl_Staff(serverversion);

                DROP TRIGGER IF EXISTS trg_staff_insert;
                CREATE TRIGGER trg_staff_insert 
                AFTER INSERT ON Tbl_Staff
                WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
                BEGIN
                    UPDATE Tbl_Staff 
                    SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                        updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                    WHERE ID = NEW.ID;
                END;

                DROP TRIGGER IF EXISTS trg_staff_update;
                CREATE TRIGGER trg_staff_update 
                AFTER UPDATE ON Tbl_Staff
                WHEN NEW.updatedtime = OLD.updatedtime
                BEGIN
                    UPDATE Tbl_Staff 
                    SET updatedtime = (strftime('%s', 'now') * 1000),
                        version = OLD.version + 1,
                        syncstatus = 0
                    WHERE ID = NEW.ID;
                END;";

                // Insert default staff for demo
                await InsertDefaultStaff();
                
            }
            //catch (SqliteException e)
            //{
            //    _logger.LogError(e, "Error creating Staff table");
            //    throw;
            //}
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating Staff table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        private async Task InsertDefaultStaff()
        {
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM Tbl_Staff";
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (count == 0)
                {
                    var insertCmd = connection.CreateCommand();
                    //insertCmd.CommandText = @"
                    //INSERT INTO Staff (Name, StaffID, Email, Role, Department, Password, IsActive) VALUES (@Name, @StaffID, @Email, @Role, @Department, @Password, @IsActive);";


                    var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    //item.ID = item.ID ?? Guid.NewGuid();
                    //item.CreatedTime = now;
                    //item.UpdatedTime = now;
                    //item.DeviceId = DeviceIdentity.GetOrCreateDeviceId();
                    //item.OriginDeviceId = DeviceIdentity.GetOrCreateDeviceId();
                    //item.Version = 1;
                    //item.ServerVersion = 0;
                    //item.SyncStatus = 0;
                    //item.Deleted = 0;
                    //item.DataHash = item.CalculateHash();

                    var facility = new Guid("5f021d67-3ceb-44cd-8f55-5b10ca9039e1").ToString();
                    var super = new Guid("2bef74f2-a5cd-4f66-99fe-e36a08b29613").ToString();
                    var midwife = new Guid("cad70e60-b4cb-4621-b3fc-63ad5be6313a").ToString();
                    var admin = new Guid("8d9f9a54-a874-488c-aaa3-d2de2925ac2c").ToString();
                    var device = DeviceIdentity.GetOrCreateDeviceId();
                    insertCmd.CommandText = @$"
                INSERT INTO Tbl_Staff (ID, name, staffId, email, role, department, password, active, facility, createdtime, updatedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted) VALUES ('{super}','Super Administrator', 'SUPER', 'super@emperorsoftware.co', 'SUPER-ADMIN', 'Labor Ward', 'system.password', 1, '{facility}', '{now}', '{now}', '{device}', '{device}', 0, 1, 0, 0),
                ('{midwife}','Midwife', 'MIDWIFE', 'midwife@emperorsoftware.co', 'MIDWIFE', 'Labor Ward', 'system.password', 1, '{facility}', '{now}', '{now}', '{device}', '{device}', 0, 1, 0, 0),
                ('{admin}','Administrator', 'ADMINISTRATOR', 'administrator@emperorsoftware.c', 'ADMIN', 'Labor Ward', 'system.password', 1, '{facility}', '{now}', '{now}', '{device}', '{device}', 0, 1, 0, 0);";
                    await insertCmd.ExecuteNonQueryAsync();
                }
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Error inserting Staff table");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error inserting Staff table");
                throw;
            }
        }

        //        SELECT name FROM sqlite_master
        //WHERE type = 'table' AND name = 'Tbl_Staff';

        public async Task<List<string>> AuthenticateAsync()
        {
            var lists = new List<string>();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"SELECT name FROM sqlite_master WHERE type = 'table'";

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lists.Add(reader.GetString(0));
            }

            return lists;
        }

        public async Task<Staff?> AuthenticateAsync(string emailOrStaffId, string password)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"
                SELECT ID, name, staffid, email, role, department, password, lastlogin, active, facility, createdtime, updatedtime, deletedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted, conflictdata, datahash FROM Tbl_Staff WHERE (email = @identifier OR staffId = @identifier) AND password = @password AND active = 1 and deleted = 0";
            selectCmd.Parameters.AddWithValue("@identifier", emailOrStaffId);
            selectCmd.Parameters.AddWithValue("@password", password);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var staff = new Staff
                {
                    ID = Guid.Parse(reader.GetString(0)),
                    Name = reader.GetString(1),
                    StaffID = reader.GetString(2),
                    Email = reader.GetString(3),
                    Role = reader.GetString(4),
                    Department = reader.GetString(5),
                    Password = reader.GetString(6),
                    LastLogin = reader.IsDBNull(7) ? DateTime.Now : DateTime.Parse(reader.GetString(7)),
                    IsActive = reader.GetBoolean(8),
                    Facility = reader.IsDBNull(7) ? null : Guid.Parse(reader.GetString(9)),
                    CreatedTime = reader.GetInt64(10),
                    UpdatedTime = reader.GetInt64(11),
                    DeletedTime = reader.IsDBNull(12) ? null : reader.GetInt64(12),
                    DeviceId = reader.GetString(13),
                    OriginDeviceId = reader.GetString(14),
                    SyncStatus = reader.GetInt32(15),
                    Version = reader.GetInt32(16),
                    ServerVersion = reader.IsDBNull(17) ? 0 : reader.GetInt32(17),
                    Deleted = reader.IsDBNull(18) ? 0 : reader.GetInt32(18),
                    //ConflictData = reader.GetString(15),
                    //DataHash = reader.GetString(16)
                };

                // Update last login
                var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = "UPDATE Tbl_Staff SET lastlogin = @lastLogin WHERE ID = @id";
                updateCmd.Parameters.AddWithValue("@lastLogin", DateTime.Now.ToString("O"));
                updateCmd.Parameters.AddWithValue("@id", staff.ID);
                await updateCmd.ExecuteNonQueryAsync();

                return staff;
            }

            return null;
        }
    }
}
