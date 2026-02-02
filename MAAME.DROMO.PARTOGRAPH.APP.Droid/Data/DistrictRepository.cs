using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    /// <summary>
    /// Repository for managing District data in local SQLite database
    /// Districts are reference data synced from the server (pull only)
    /// </summary>
    public class DistrictRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;

        public DistrictRepository(ILogger<DistrictRepository> logger)
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
                createTableCmd.CommandText = @"CREATE TABLE IF NOT EXISTS Tbl_District (
                    ID TEXT PRIMARY KEY,
                    name TEXT NOT NULL,
                    code TEXT NOT NULL UNIQUE,
                    type TEXT NOT NULL DEFAULT 'District',
                    regionid TEXT NOT NULL,
                    regionname TEXT,
                    capital TEXT,
                    population INTEGER DEFAULT 0,
                    expectedannualdeliveries INTEGER DEFAULT 0,
                    directorname TEXT,
                    phone TEXT,
                    email TEXT,
                    latitude REAL,
                    longitude REAL,
                    active INTEGER NOT NULL DEFAULT 1,
                    createdtime INTEGER NOT NULL,
                    updatedtime INTEGER NOT NULL,
                    deletedtime INTEGER,
                    deleted INTEGER DEFAULT 0
                );

                CREATE INDEX IF NOT EXISTS idx_district_code ON Tbl_District(code);
                CREATE INDEX IF NOT EXISTS idx_district_regionid ON Tbl_District(regionid);
                CREATE INDEX IF NOT EXISTS idx_district_updatedtime ON Tbl_District(updatedtime);";

                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Error creating District table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<List<District>> GetAllAsync()
        {
            await Init();

            var districts = new List<District>();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"SELECT * FROM Tbl_District WHERE deleted = 0 AND active = 1 ORDER BY name;";

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                districts.Add(MapFromReader(reader));
            }

            return districts;
        }

        public async Task<List<District>> GetByRegionAsync(Guid regionId)
        {
            await Init();

            var districts = new List<District>();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"SELECT * FROM Tbl_District WHERE regionid = @regionid AND deleted = 0 AND active = 1 ORDER BY name;";
            selectCmd.Parameters.AddWithValue("@regionid", regionId.ToString());

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                districts.Add(MapFromReader(reader));
            }

            return districts;
        }

        public async Task<District?> GetByIdAsync(Guid id)
        {
            await Init();

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"SELECT * FROM Tbl_District WHERE ID = @id AND deleted = 0;";
            selectCmd.Parameters.AddWithValue("@id", id.ToString());

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }

            return null;
        }

        public async Task<District?> GetByCodeAsync(string code)
        {
            await Init();

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"SELECT * FROM Tbl_District WHERE code = @code AND deleted = 0;";
            selectCmd.Parameters.AddWithValue("@code", code);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }

            return null;
        }

        public async Task AddOrUpdateAsync(District district)
        {
            await Init();

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            // Check if exists
            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = @"SELECT COUNT(*) FROM Tbl_District WHERE ID = @id;";
            checkCmd.Parameters.AddWithValue("@id", district.ID.ToString());
            var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

            if (exists)
            {
                await UpdateAsync(district, connection);
            }
            else
            {
                await InsertAsync(district, connection);
            }
        }

        private async Task InsertAsync(District district, SqliteConnection connection)
        {
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"INSERT INTO Tbl_District
                (ID, name, code, type, regionid, regionname, capital, population, expectedannualdeliveries,
                 directorname, phone, email, latitude, longitude, active,
                 createdtime, updatedtime, deletedtime, deleted)
                VALUES (@id, @name, @code, @type, @regionid, @regionname, @capital, @population, @expectedannualdeliveries,
                        @directorname, @phone, @email, @latitude, @longitude, @active,
                        @createdtime, @updatedtime, @deletedtime, @deleted);";

            AddParameters(insertCmd, district);
            await insertCmd.ExecuteNonQueryAsync();
        }

        private async Task UpdateAsync(District district, SqliteConnection connection)
        {
            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = @"UPDATE Tbl_District SET
                name = @name, code = @code, type = @type, regionid = @regionid, regionname = @regionname,
                capital = @capital, population = @population, expectedannualdeliveries = @expectedannualdeliveries,
                directorname = @directorname, phone = @phone, email = @email,
                latitude = @latitude, longitude = @longitude, active = @active,
                updatedtime = @updatedtime, deletedtime = @deletedtime, deleted = @deleted
                WHERE ID = @id;";

            AddParameters(updateCmd, district);
            await updateCmd.ExecuteNonQueryAsync();
        }

        private void AddParameters(SqliteCommand cmd, District district)
        {
            cmd.Parameters.AddWithValue("@id", district.ID.ToString());
            cmd.Parameters.AddWithValue("@name", district.Name);
            cmd.Parameters.AddWithValue("@code", district.Code);
            cmd.Parameters.AddWithValue("@type", district.Type);
            cmd.Parameters.AddWithValue("@regionid", district.RegionID.ToString());
            cmd.Parameters.AddWithValue("@regionname", district.RegionName ?? string.Empty);
            cmd.Parameters.AddWithValue("@capital", district.Capital ?? string.Empty);
            cmd.Parameters.AddWithValue("@population", district.Population);
            cmd.Parameters.AddWithValue("@expectedannualdeliveries", district.ExpectedAnnualDeliveries);
            cmd.Parameters.AddWithValue("@directorname", district.DirectorName ?? string.Empty);
            cmd.Parameters.AddWithValue("@phone", district.Phone ?? string.Empty);
            cmd.Parameters.AddWithValue("@email", district.Email ?? string.Empty);
            cmd.Parameters.AddWithValue("@latitude", district.Latitude.HasValue ? (object)district.Latitude.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@longitude", district.Longitude.HasValue ? (object)district.Longitude.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@active", district.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@createdtime", district.CreatedTime);
            cmd.Parameters.AddWithValue("@updatedtime", district.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", district.DeletedTime.HasValue ? (object)district.DeletedTime.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@deleted", district.Deleted);
        }

        private District MapFromReader(SqliteDataReader reader)
        {
            return new District
            {
                ID = Guid.Parse(reader["ID"].ToString()!),
                Name = reader["name"].ToString()!,
                Code = reader["code"].ToString()!,
                Type = reader["type"]?.ToString() ?? "District",
                RegionID = Guid.Parse(reader["regionid"].ToString()!),
                RegionName = reader["regionname"]?.ToString() ?? string.Empty,
                Capital = reader["capital"]?.ToString() ?? string.Empty,
                Population = Convert.ToInt64(reader["population"]),
                ExpectedAnnualDeliveries = Convert.ToInt32(reader["expectedannualdeliveries"]),
                DirectorName = reader["directorname"]?.ToString() ?? string.Empty,
                Phone = reader["phone"]?.ToString() ?? string.Empty,
                Email = reader["email"]?.ToString() ?? string.Empty,
                Latitude = reader["latitude"] != DBNull.Value ? Convert.ToDouble(reader["latitude"]) : null,
                Longitude = reader["longitude"] != DBNull.Value ? Convert.ToDouble(reader["longitude"]) : null,
                IsActive = Convert.ToBoolean(reader["active"]),
                CreatedTime = Convert.ToInt64(reader["createdtime"]),
                UpdatedTime = Convert.ToInt64(reader["updatedtime"]),
                DeletedTime = reader["deletedtime"] != DBNull.Value ? Convert.ToInt64(reader["deletedtime"]) : null,
                Deleted = Convert.ToInt32(reader["deleted"])
            };
        }
    }
}
