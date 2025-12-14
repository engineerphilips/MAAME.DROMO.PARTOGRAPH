using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public class FacilityRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;

        public FacilityRepository(ILogger<FacilityRepository> logger)
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
                createTableCmd.CommandText = @"CREATE TABLE IF NOT EXISTS Tbl_Facility (
                    ID TEXT PRIMARY KEY,
                    name TEXT NOT NULL,
                    code TEXT NOT NULL UNIQUE,
                    type TEXT NOT NULL,
                    address TEXT,
                    city TEXT,
                    region TEXT,
                    country TEXT,
                    phone TEXT,
                    email TEXT,
                    active INTEGER NOT NULL DEFAULT 1,
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

                CREATE INDEX IF NOT EXISTS idx_facility_sync ON Tbl_Facility(updatedtime, syncstatus);
                CREATE INDEX IF NOT EXISTS idx_facility_server_version ON Tbl_Facility(serverversion);

                DROP TRIGGER IF EXISTS trg_facility_insert;
                CREATE TRIGGER trg_facility_insert
                AFTER INSERT ON Tbl_Facility
                WHEN NEW.createdtime IS NULL OR NEW.updatedtime IS NULL
                BEGIN
                    UPDATE Tbl_Facility
                    SET createdtime = COALESCE(NEW.createdtime, (strftime('%s', 'now') * 1000)),
                        updatedtime = COALESCE(NEW.updatedtime, (strftime('%s', 'now') * 1000))
                    WHERE ID = NEW.ID;
                END;

                DROP TRIGGER IF EXISTS trg_facility_update;
                CREATE TRIGGER trg_facility_update
                AFTER UPDATE ON Tbl_Facility
                WHEN NEW.updatedtime = OLD.updatedtime
                BEGIN
                    UPDATE Tbl_Facility
                    SET updatedtime = (strftime('%s', 'now') * 1000),
                        version = OLD.version + 1,
                        syncstatus = 0
                    WHERE ID = NEW.ID;
                END;";

                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Error creating Facility table");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating Facility table");
                throw;
            }

            try
            {
                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = @"SELECT COUNT(*) FROM Tbl_Facility;";
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                if (count == 0)
                {
                    // Insert default facilities for demo
                    await InsertDefaultFacilities();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error checking Facility count");
            }

            _hasBeenInitialized = true;
        }

        private async Task InsertDefaultFacilities()
        {
            var defaultFacilities = new List<Facility>
            {
                new Facility
                {
                    ID = Guid.NewGuid(),
                    Name = "Korle Bu Teaching Hospital",
                    Code = "KBTH",
                    Type = "Teaching Hospital",
                    Address = "Korle Bu",
                    City = "Accra",
                    Region = "Greater Accra",
                    Country = "Ghana",
                    Phone = "+233-302-674-191",
                    Email = "info@kbth.gov.gh",
                    IsActive = true,
                    CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    DeviceId = Guid.NewGuid().ToString(),
                    OriginDeviceId = Guid.NewGuid().ToString(),
                    SyncStatus = 1,
                    Version = 1,
                    ServerVersion = 1,
                    Deleted = 0
                },
                new Facility
                {
                    ID = Guid.NewGuid(),
                    Name = "Ridge Hospital",
                    Code = "RH",
                    Type = "Hospital",
                    Address = "Castle Road, Ridge",
                    City = "Accra",
                    Region = "Greater Accra",
                    Country = "Ghana",
                    Phone = "+233-302-776-111",
                    Email = "info@ridgehospital.gov.gh",
                    IsActive = true,
                    CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    DeviceId = Guid.NewGuid().ToString(),
                    OriginDeviceId = Guid.NewGuid().ToString(),
                    SyncStatus = 1,
                    Version = 1,
                    ServerVersion = 1,
                    Deleted = 0
                },
                new Facility
                {
                    ID = Guid.NewGuid(),
                    Name = "37 Military Hospital",
                    Code = "37MH",
                    Type = "Military Hospital",
                    Address = "Liberation Road",
                    City = "Accra",
                    Region = "Greater Accra",
                    Country = "Ghana",
                    Phone = "+233-302-776-111",
                    Email = "info@37mh.gov.gh",
                    IsActive = true,
                    CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    UpdatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    DeviceId = Guid.NewGuid().ToString(),
                    OriginDeviceId = Guid.NewGuid().ToString(),
                    SyncStatus = 1,
                    Version = 1,
                    ServerVersion = 1,
                    Deleted = 0
                }
            };

            foreach (var facility in defaultFacilities)
            {
                await AddAsync(facility);
            }
        }

        public async Task<List<Facility>> GetAllAsync()
        {
            await Init();

            var facilities = new List<Facility>();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"SELECT * FROM Tbl_Facility WHERE deleted = 0 AND active = 1 ORDER BY name;";

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                facilities.Add(new Facility
                {
                    ID = Guid.Parse(reader["ID"].ToString()),
                    Name = reader["name"].ToString(),
                    Code = reader["code"].ToString(),
                    Type = reader["type"].ToString(),
                    Address = reader["address"]?.ToString() ?? string.Empty,
                    City = reader["city"]?.ToString() ?? string.Empty,
                    Region = reader["region"]?.ToString() ?? string.Empty,
                    Country = reader["country"]?.ToString() ?? string.Empty,
                    Phone = reader["phone"]?.ToString() ?? string.Empty,
                    Email = reader["email"]?.ToString() ?? string.Empty,
                    IsActive = Convert.ToBoolean(reader["active"]),
                    CreatedTime = Convert.ToInt64(reader["createdtime"]),
                    UpdatedTime = Convert.ToInt64(reader["updatedtime"]),
                    DeviceId = reader["deviceid"]?.ToString(),
                    OriginDeviceId = reader["origindeviceid"]?.ToString(),
                    SyncStatus = Convert.ToInt32(reader["syncstatus"]),
                    Version = Convert.ToInt32(reader["version"]),
                    ServerVersion = Convert.ToInt32(reader["serverversion"]),
                    Deleted = Convert.ToInt32(reader["deleted"])
                });
            }

            return facilities;
        }

        public async Task<Facility?> GetByIdAsync(Guid id)
        {
            await Init();

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"SELECT * FROM Tbl_Facility WHERE ID = @id AND deleted = 0;";
            selectCmd.Parameters.AddWithValue("@id", id.ToString());

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Facility
                {
                    ID = Guid.Parse(reader["ID"].ToString()),
                    Name = reader["name"].ToString(),
                    Code = reader["code"].ToString(),
                    Type = reader["type"].ToString(),
                    Address = reader["address"]?.ToString() ?? string.Empty,
                    City = reader["city"]?.ToString() ?? string.Empty,
                    Region = reader["region"]?.ToString() ?? string.Empty,
                    Country = reader["country"]?.ToString() ?? string.Empty,
                    Phone = reader["phone"]?.ToString() ?? string.Empty,
                    Email = reader["email"]?.ToString() ?? string.Empty,
                    IsActive = Convert.ToBoolean(reader["active"]),
                    CreatedTime = Convert.ToInt64(reader["createdtime"]),
                    UpdatedTime = Convert.ToInt64(reader["updatedtime"]),
                    DeviceId = reader["deviceid"]?.ToString(),
                    OriginDeviceId = reader["origindeviceid"]?.ToString(),
                    SyncStatus = Convert.ToInt32(reader["syncstatus"]),
                    Version = Convert.ToInt32(reader["version"]),
                    ServerVersion = Convert.ToInt32(reader["serverversion"]),
                    Deleted = Convert.ToInt32(reader["deleted"])
                };
            }

            return null;
        }

        public async Task AddAsync(Facility facility)
        {
            await Init();

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"INSERT INTO Tbl_Facility
                (ID, name, code, type, address, city, region, country, phone, email, active,
                 createdtime, updatedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted)
                VALUES (@id, @name, @code, @type, @address, @city, @region, @country, @phone, @email, @active,
                        @createdtime, @updatedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted);";

            insertCmd.Parameters.AddWithValue("@id", facility.ID.ToString());
            insertCmd.Parameters.AddWithValue("@name", facility.Name);
            insertCmd.Parameters.AddWithValue("@code", facility.Code);
            insertCmd.Parameters.AddWithValue("@type", facility.Type);
            insertCmd.Parameters.AddWithValue("@address", facility.Address ?? string.Empty);
            insertCmd.Parameters.AddWithValue("@city", facility.City ?? string.Empty);
            insertCmd.Parameters.AddWithValue("@region", facility.Region ?? string.Empty);
            insertCmd.Parameters.AddWithValue("@country", facility.Country ?? string.Empty);
            insertCmd.Parameters.AddWithValue("@phone", facility.Phone ?? string.Empty);
            insertCmd.Parameters.AddWithValue("@email", facility.Email ?? string.Empty);
            insertCmd.Parameters.AddWithValue("@active", facility.IsActive ? 1 : 0);
            insertCmd.Parameters.AddWithValue("@createdtime", facility.CreatedTime);
            insertCmd.Parameters.AddWithValue("@updatedtime", facility.UpdatedTime);
            insertCmd.Parameters.AddWithValue("@deviceid", facility.DeviceId ?? Guid.NewGuid().ToString());
            insertCmd.Parameters.AddWithValue("@origindeviceid", facility.OriginDeviceId ?? Guid.NewGuid().ToString());
            insertCmd.Parameters.AddWithValue("@syncstatus", facility.SyncStatus);
            insertCmd.Parameters.AddWithValue("@version", facility.Version);
            insertCmd.Parameters.AddWithValue("@serverversion", facility.ServerVersion);
            insertCmd.Parameters.AddWithValue("@deleted", facility.Deleted);

            await insertCmd.ExecuteNonQueryAsync();
        }
    }
}
