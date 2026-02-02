using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    /// <summary>
    /// Repository for managing Region data in local SQLite database
    /// Regions are reference data synced from the server (pull only)
    /// </summary>
    public class RegionRepository
    {
        private bool _hasBeenInitialized = false;
        private readonly ILogger _logger;

        public RegionRepository(ILogger<RegionRepository> logger)
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
                createTableCmd.CommandText = @"CREATE TABLE IF NOT EXISTS Tbl_Region (
                    ID TEXT PRIMARY KEY,
                    name TEXT NOT NULL,
                    code TEXT NOT NULL UNIQUE,
                    country TEXT NOT NULL DEFAULT 'Ghana',
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

                CREATE INDEX IF NOT EXISTS idx_region_code ON Tbl_Region(code);
                CREATE INDEX IF NOT EXISTS idx_region_updatedtime ON Tbl_Region(updatedtime);";

                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, "Error creating Region table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<List<Region>> GetAllAsync()
        {
            await Init();

            var regions = new List<Region>();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"SELECT * FROM Tbl_Region WHERE deleted = 0 AND active = 1 ORDER BY name;";

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                regions.Add(MapFromReader(reader));
            }

            return regions;
        }

        public async Task<Region?> GetByIdAsync(Guid id)
        {
            await Init();

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"SELECT * FROM Tbl_Region WHERE ID = @id AND deleted = 0;";
            selectCmd.Parameters.AddWithValue("@id", id.ToString());

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }

            return null;
        }

        public async Task<Region?> GetByCodeAsync(string code)
        {
            await Init();

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"SELECT * FROM Tbl_Region WHERE code = @code AND deleted = 0;";
            selectCmd.Parameters.AddWithValue("@code", code);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }

            return null;
        }

        public async Task AddOrUpdateAsync(Region region)
        {
            await Init();

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            // Check if exists
            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = @"SELECT COUNT(*) FROM Tbl_Region WHERE ID = @id;";
            checkCmd.Parameters.AddWithValue("@id", region.ID.ToString());
            var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

            if (exists)
            {
                await UpdateAsync(region, connection);
            }
            else
            {
                await InsertAsync(region, connection);
            }
        }

        private async Task InsertAsync(Region region, SqliteConnection connection)
        {
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"INSERT INTO Tbl_Region
                (ID, name, code, country, capital, population, expectedannualdeliveries,
                 directorname, phone, email, latitude, longitude, active,
                 createdtime, updatedtime, deletedtime, deleted)
                VALUES (@id, @name, @code, @country, @capital, @population, @expectedannualdeliveries,
                        @directorname, @phone, @email, @latitude, @longitude, @active,
                        @createdtime, @updatedtime, @deletedtime, @deleted);";

            AddParameters(insertCmd, region);
            await insertCmd.ExecuteNonQueryAsync();
        }

        private async Task UpdateAsync(Region region, SqliteConnection connection)
        {
            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = @"UPDATE Tbl_Region SET
                name = @name, code = @code, country = @country, capital = @capital,
                population = @population, expectedannualdeliveries = @expectedannualdeliveries,
                directorname = @directorname, phone = @phone, email = @email,
                latitude = @latitude, longitude = @longitude, active = @active,
                updatedtime = @updatedtime, deletedtime = @deletedtime, deleted = @deleted
                WHERE ID = @id;";

            AddParameters(updateCmd, region);
            await updateCmd.ExecuteNonQueryAsync();
        }

        private void AddParameters(SqliteCommand cmd, Region region)
        {
            cmd.Parameters.AddWithValue("@id", region.ID.ToString());
            cmd.Parameters.AddWithValue("@name", region.Name);
            cmd.Parameters.AddWithValue("@code", region.Code);
            cmd.Parameters.AddWithValue("@country", region.Country);
            cmd.Parameters.AddWithValue("@capital", region.Capital ?? string.Empty);
            cmd.Parameters.AddWithValue("@population", region.Population);
            cmd.Parameters.AddWithValue("@expectedannualdeliveries", region.ExpectedAnnualDeliveries);
            cmd.Parameters.AddWithValue("@directorname", region.DirectorName ?? string.Empty);
            cmd.Parameters.AddWithValue("@phone", region.Phone ?? string.Empty);
            cmd.Parameters.AddWithValue("@email", region.Email ?? string.Empty);
            cmd.Parameters.AddWithValue("@latitude", region.Latitude.HasValue ? (object)region.Latitude.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@longitude", region.Longitude.HasValue ? (object)region.Longitude.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@active", region.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@createdtime", region.CreatedTime);
            cmd.Parameters.AddWithValue("@updatedtime", region.UpdatedTime);
            cmd.Parameters.AddWithValue("@deletedtime", region.DeletedTime.HasValue ? (object)region.DeletedTime.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@deleted", region.Deleted);
        }

        private Region MapFromReader(SqliteDataReader reader)
        {
            return new Region
            {
                ID = Guid.Parse(reader["ID"].ToString()!),
                Name = reader["name"].ToString()!,
                Code = reader["code"].ToString()!,
                Country = reader["country"]?.ToString() ?? "Ghana",
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
