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

            //// Add migration for existing databases to add GPS columns
            //try
            //{
            //    var alterTableCmd = connection.CreateCommand();
            //    alterTableCmd.CommandText = @"UPDATE Tbl_Facility SET ID = '5f021d67-3ceb-44cd-8f55-5b10ca9039e1' WHERE code='KBTH'";
            //    await alterTableCmd.ExecuteNonQueryAsync();
            //}
            //catch (SqliteException)
            //{
            //    // Columns already exist, ignore error
            //    throw;
            //}

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @"CREATE TABLE IF NOT EXISTS Tbl_Facility (
                    ID TEXT PRIMARY KEY,
                    name TEXT NOT NULL,
                    code TEXT NOT NULL UNIQUE,
                    type TEXT NOT NULL,
                    level TEXT DEFAULT 'Primary',
                    address TEXT,
                    city TEXT,
                    country TEXT,
                    phone TEXT,
                    email TEXT,
                    districtid TEXT,
                    latitude REAL,
                    longitude REAL,
                    ghpostgps TEXT,
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

                // Migration: Add new columns for existing databases
                try
                {
                    var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = @"ALTER TABLE Tbl_Facility ADD COLUMN level TEXT DEFAULT 'Primary';";
                    await alterCmd.ExecuteNonQueryAsync();
                }
                catch (SqliteException) { /* Column already exists, ignore */ }

                try
                {
                    var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = @"ALTER TABLE Tbl_Facility ADD COLUMN regionid TEXT;";
                    await alterCmd.ExecuteNonQueryAsync();
                }
                catch (SqliteException) { /* Column already exists, ignore */ }

                try
                {
                    var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = @"ALTER TABLE Tbl_Facility ADD COLUMN districtid TEXT;";
                    await alterCmd.ExecuteNonQueryAsync();
                }
                catch (SqliteException) { /* Column already exists, ignore */ }
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

            _hasBeenInitialized = true;

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
        }

        private async Task InsertDefaultFacilities()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var deviceId = Guid.NewGuid().ToString();

            var defaultFacilities = new List<Facility>
            {
                // Greater Accra Region Facilities (preserving original IDs for backward compatibility)
                CreateDefaultFacility("5f021d67-3ceb-44cd-8f55-5b10ca9039e1", "Korle Bu Teaching Hospital", "KBTH", "Hospital", "Tertiary", "Accra", 5.5364, -0.2275, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440113", "Ridge Hospital", "RH", "Hospital", "Regional", "Accra", 5.5569, -0.1958, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440114", "37 Military Hospital", "37MH", "Hospital", "Tertiary", "Accra", 5.5833, -0.1833, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440115", "Police Hospital", "PH", "Hospital", "Specialized", "Accra", 5.5600, -0.2000, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440119", "Tema General Hospital", "TEMGH", "Hospital", "Regional", "Tema", 5.6689, -0.0167, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440123", "Ga East Municipal Hospital", "GEMH", "Hospital", "District", "Abokobi", 5.7000, -0.1500, now, deviceId),

                // Ashanti Region Facilities
                CreateDefaultFacility("44444444-4444-4444-4444-444444440033", "Komfo Anokye Teaching Hospital", "KATH", "Hospital", "Tertiary", "Kumasi", 6.6885, -1.6244, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440034", "Kumasi South Hospital", "KSH", "Hospital", "Regional", "Kumasi", 6.6700, -1.6300, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440039", "KNUST Hospital", "KNUSTH", "Hospital", "Tertiary", "KNUST", 6.6730, -1.5660, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440043", "Ejisu Government Hospital", "EGH", "Hospital", "District", "Ejisu", 6.7261, -1.4614, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440047", "Mampong Government Hospital", "MGH", "Hospital", "District", "Mampong", 7.0619, -1.4011, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440052", "Obuasi Government Hospital", "OGH", "Hospital", "District", "Obuasi", 6.2000, -1.6600, now, deviceId),

                // Central Region Facilities
                CreateDefaultFacility("44444444-4444-4444-4444-444444440082", "Cape Coast Teaching Hospital", "CCTH", "Hospital", "Tertiary", "Cape Coast", 5.1315, -1.2795, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440086", "Winneba Municipal Hospital", "WinMH", "Hospital", "District", "Winneba", 5.3500, -0.6333, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440089", "Kasoa Polyclinic", "KasP", "Polyclinic", "District", "Kasoa", 5.5333, -0.4167, now, deviceId),

                // Eastern Region Facilities
                CreateDefaultFacility("44444444-4444-4444-4444-444444440099", "Eastern Regional Hospital", "ERH", "Hospital", "Regional", "Koforidua", 6.0940, -0.2577, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440103", "Nsawam Government Hospital", "NGH", "Hospital", "District", "Nsawam", 5.8167, -0.3500, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440106", "Kibi Government Hospital", "KiGH", "Hospital", "District", "Kibi", 6.1667, -0.5500, now, deviceId),

                // Western Region Facilities
                CreateDefaultFacility("44444444-4444-4444-4444-444444440178", "Effia Nkwanta Regional Hospital", "ENRH", "Hospital", "Regional", "Sekondi", 4.9400, -1.7700, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440182", "Tarkwa Municipal Hospital", "TarMH", "Hospital", "District", "Tarkwa", 5.3000, -1.9833, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440185", "Axim Government Hospital", "AGH", "Hospital", "District", "Axim", 4.8667, -2.2333, now, deviceId),

                // Volta Region Facilities
                CreateDefaultFacility("44444444-4444-4444-4444-444444440168", "Ho Teaching Hospital", "HTH", "Hospital", "Tertiary", "Ho", 6.6000, 0.4700, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440172", "Ketu South Municipal Hospital", "KSMH", "Hospital", "District", "Denu", 6.0833, 1.1333, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440175", "Keta Municipal Hospital", "KetMH", "Hospital", "District", "Keta", 5.9167, 0.9833, now, deviceId),

                // Northern Region Facilities
                CreateDefaultFacility("44444444-4444-4444-4444-444444440136", "Tamale Teaching Hospital", "TTH", "Hospital", "Tertiary", "Tamale", 9.4008, -0.8393, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440143", "Savelugu Municipal Hospital", "SavMH", "Hospital", "District", "Savelugu", 9.6242, -0.8250, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440146", "Yendi Municipal Hospital", "YMH", "Hospital", "District", "Yendi", 9.4425, -0.0097, now, deviceId),

                // Upper East Region Facilities
                CreateDefaultFacility("44444444-4444-4444-4444-444444440149", "Bolgatanga Regional Hospital", "BRH", "Hospital", "Regional", "Bolgatanga", 10.7856, -0.8519, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440152", "Bawku Presbyterian Hospital", "BPH", "Hospital", "District", "Bawku", 11.0606, -0.2417, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440155", "War Memorial Hospital Navrongo", "WMHN", "Hospital", "District", "Navrongo", 10.8944, -1.0917, now, deviceId),

                // Upper West Region Facilities
                CreateDefaultFacility("44444444-4444-4444-4444-444444440158", "Wa Regional Hospital", "WRH", "Hospital", "Regional", "Wa", 10.0601, -2.5099, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440162", "St. Joseph Hospital Jirapa", "SJHJ", "Hospital", "District", "Jirapa", 10.7833, -2.5500, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440165", "Lawra District Hospital", "LDH", "Hospital", "District", "Lawra", 10.6333, -2.9000, now, deviceId),

                // Bono Region Facilities
                CreateDefaultFacility("44444444-4444-4444-4444-444444440058", "Sunyani Regional Hospital", "SRH", "Hospital", "Regional", "Sunyani", 7.3349, -2.3123, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440062", "Holy Family Hospital Berekum", "HFHB", "Hospital", "District", "Berekum", 7.4567, -2.5850, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440065", "Dormaa Ahenkro Presbyterian Hospital", "DAPH", "Hospital", "District", "Dormaa Ahenkro", 7.3500, -2.9667, now, deviceId),

                // Bono East Region Facilities
                CreateDefaultFacility("44444444-4444-4444-4444-444444440071", "Holy Family Hospital Techiman", "HFHT", "Hospital", "Regional", "Techiman", 7.5833, -1.9333, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440075", "Kintampo Municipal Hospital", "KMH", "Hospital", "District", "Kintampo", 8.0556, -1.7306, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440079", "Atebubu Government Hospital", "AtGH", "Hospital", "District", "Atebubu", 7.7500, -0.9833, now, deviceId),

                // Ahafo Region Facilities
                CreateDefaultFacility("44444444-4444-4444-4444-444444440001", "Goaso Government Hospital", "GGH", "Hospital", "District", "Goaso", 6.8039, -2.5172, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440019", "Hwidiem Government Hospital", "HGH", "Hospital", "District", "Hwidiem", 6.7667, -2.3000, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440023", "Duayaw Nkwanta Government Hospital", "DNGH", "Hospital", "District", "Duayaw Nkwanta", 7.1833, -2.1000, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440028", "Bechem Government Hospital", "BGH", "Hospital", "District", "Bechem", 7.0833, -2.0167, now, deviceId),

                // Western North Region Facilities
                CreateDefaultFacility("44444444-4444-4444-4444-444444440188", "Sefwi Wiawso Municipal Hospital", "SWMH", "Hospital", "District", "Sefwi Wiawso", 6.2167, -2.4833, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440191", "Bibiani Government Hospital", "BibGH", "Hospital", "District", "Bibiani", 6.4667, -2.3167, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440194", "Juaboso Government Hospital", "JuaGH", "Hospital", "District", "Juaboso", 6.4167, -2.8333, now, deviceId),

                // Oti Region Facilities
                CreateDefaultFacility("44444444-4444-4444-4444-444444440197", "Dambai District Hospital", "DamDH", "Hospital", "District", "Dambai", 7.9833, 0.1667, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440200", "Nkwanta District Hospital", "NkwDH", "Hospital", "District", "Nkwanta", 8.2500, 0.5000, now, deviceId),

                // Savannah Region Facilities
                CreateDefaultFacility("44444444-4444-4444-4444-444444440203", "Damongo District Hospital", "DamoDH", "Hospital", "District", "Damongo", 9.0833, -1.8167, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440206", "Salaga District Hospital", "SalDH", "Hospital", "District", "Salaga", 8.5500, -0.5167, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440209", "Bole District Hospital", "BolDH", "Hospital", "District", "Bole", 9.0333, -2.4833, now, deviceId),

                // North East Region Facilities
                CreateDefaultFacility("44444444-4444-4444-4444-444444440212", "Walewale District Hospital", "WalDH", "Hospital", "District", "Walewale", 10.3500, -0.8000, now, deviceId),
                CreateDefaultFacility("44444444-4444-4444-4444-444444440215", "Nalerigu Baptist Medical Centre", "NBMC", "Hospital", "District", "Nalerigu", 10.5167, -0.3667, now, deviceId),
            };

            foreach (var facility in defaultFacilities)
            {
                await AddAsync(facility);
            }
        }

        private Facility CreateDefaultFacility(string id, string name, string code, string type, string level, string city, double latitude, double longitude, long timestamp, string deviceId)
        {
            return new Facility
            {
                ID = Guid.Parse(id),
                Name = name,
                Code = code,
                Type = type,
                Level = level,
                Address = $"{city}, Ghana",
                City = city,
                Country = "Ghana",
                Phone = "+233-000-000-000",
                Email = $"{code.ToLower()}@ghs.gov.gh",
                Latitude = latitude,
                Longitude = longitude,
                GHPostGPS = "",
                IsActive = true,
                CreatedTime = timestamp,
                UpdatedTime = timestamp,
                DeviceId = deviceId,
                OriginDeviceId = deviceId,
                SyncStatus = 1,
                Version = 1,
                ServerVersion = 1,
                Deleted = 0
            };
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
                    Level = reader["level"]?.ToString() ?? "Primary",
                    Address = reader["address"]?.ToString() ?? string.Empty,
                    City = reader["city"]?.ToString() ?? string.Empty,
                    Country = reader["country"]?.ToString() ?? string.Empty,
                    Phone = reader["phone"]?.ToString() ?? string.Empty,
                    Email = reader["email"]?.ToString() ?? string.Empty,
                    DistrictID = reader["districtid"] != DBNull.Value && !string.IsNullOrEmpty(reader["districtid"]?.ToString()) ? Guid.Parse(reader["districtid"].ToString()) : null,
                    Latitude = reader["latitude"] != DBNull.Value ? Convert.ToDouble(reader["latitude"]) : null,
                    Longitude = reader["longitude"] != DBNull.Value ? Convert.ToDouble(reader["longitude"]) : null,
                    GHPostGPS = reader["ghpostgps"]?.ToString() ?? string.Empty,
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
                    Level = reader["level"]?.ToString() ?? "Primary",
                    Address = reader["address"]?.ToString() ?? string.Empty,
                    City = reader["city"]?.ToString() ?? string.Empty,
                    Country = reader["country"]?.ToString() ?? string.Empty,
                    Phone = reader["phone"]?.ToString() ?? string.Empty,
                    Email = reader["email"]?.ToString() ?? string.Empty,
                    DistrictID = reader["districtid"] != DBNull.Value && !string.IsNullOrEmpty(reader["districtid"]?.ToString()) ? Guid.Parse(reader["districtid"].ToString()) : null,
                    Latitude = reader["latitude"] != DBNull.Value ? Convert.ToDouble(reader["latitude"]) : null,
                    Longitude = reader["longitude"] != DBNull.Value ? Convert.ToDouble(reader["longitude"]) : null,
                    GHPostGPS = reader["ghpostgps"]?.ToString() ?? string.Empty,
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
                (ID, name, code, type, level, address, city, country, phone, email, districtid,
                 latitude, longitude, ghpostgps, active, createdtime, updatedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted)
                VALUES (@id, @name, @code, @type, @level, @address, @city, @country, @phone, @email, @districtid,
                        @latitude, @longitude, @ghpostgps, @active, @createdtime, @updatedtime, @deviceid, @origindeviceid, @syncstatus, @version, @serverversion, @deleted);";

            insertCmd.Parameters.AddWithValue("@id", facility.ID.ToString());
            insertCmd.Parameters.AddWithValue("@name", facility.Name);
            insertCmd.Parameters.AddWithValue("@code", facility.Code);
            insertCmd.Parameters.AddWithValue("@type", facility.Type);
            insertCmd.Parameters.AddWithValue("@level", facility.Level ?? "Primary");
            insertCmd.Parameters.AddWithValue("@address", facility.Address ?? string.Empty);
            insertCmd.Parameters.AddWithValue("@city", facility.City ?? string.Empty);
            insertCmd.Parameters.AddWithValue("@country", facility.Country ?? string.Empty);
            insertCmd.Parameters.AddWithValue("@phone", facility.Phone ?? string.Empty);
            insertCmd.Parameters.AddWithValue("@email", facility.Email ?? string.Empty);
            insertCmd.Parameters.AddWithValue("@districtid", facility.DistrictID.HasValue ? (object)facility.DistrictID.Value.ToString() : DBNull.Value);
            insertCmd.Parameters.AddWithValue("@latitude", facility.Latitude.HasValue ? (object)facility.Latitude.Value : DBNull.Value);
            insertCmd.Parameters.AddWithValue("@longitude", facility.Longitude.HasValue ? (object)facility.Longitude.Value : DBNull.Value);
            insertCmd.Parameters.AddWithValue("@ghpostgps", facility.GHPostGPS ?? string.Empty);
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
