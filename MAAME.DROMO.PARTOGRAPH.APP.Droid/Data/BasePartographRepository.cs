using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    // Generic Repository for Partograph Measurements
    public abstract class BasePartographRepository<T> where T : BasePartographMeasurement, new()
    {
        private bool _hasBeenInitialized = false;
        protected readonly ILogger _logger;
        protected abstract string TableName { get; }
        protected abstract string CreateTableSql { get; }

        protected BasePartographRepository(ILogger logger)
        {
            _logger = logger;
        }

        protected virtual async Task Init()
        {
            if (_hasBeenInitialized)
                return;

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = CreateTableSql;
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (SqliteException e)
            {
                throw new Exception(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error creating {TableName} table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        public virtual async Task<List<T>> ListByPatientAsync(Guid? id)
        {
            await Init();
            var entries = new List<T>();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = $@"SELECT m.*, s.name as staffname
                    FROM {TableName} m
                    LEFT JOIN Tbl_Staff s ON m.handler = s.ID
                    WHERE m.partographid = @partographid AND m.deleted = 0
                    ORDER BY m.time DESC";
                selectCmd.Parameters.AddWithValue("@partographid", id.ToString());

                await using var reader = await selectCmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    entries.Add(MapFromReader(reader));
                }
            }
            catch (SqliteException e)
            {
                throw new Exception(e.Message);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return entries;
        }

        public virtual async Task<T?> GetLatestByPatientAsync(Guid? id)
        {
            await Init();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = $@"SELECT m.*, s.name as staffname
                    FROM {TableName} m
                    LEFT JOIN Tbl_Staff s ON m.handler = s.ID
                    WHERE m.partographid = @partographid AND m.deleted = 0
                    ORDER BY m.time DESC LIMIT 1";
                selectCmd.Parameters.AddWithValue("@partographid", id.ToString());

                await using var reader = await selectCmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return MapFromReader(reader);
                }
            }
            catch (SqliteException e)
            {
                throw new Exception(e.Message);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return null;
        }

        /// <summary>
        /// Batch fetch all records for multiple partograph IDs.
        /// Processes in chunks to handle large datasets and avoid SQLite parameter limits.
        /// </summary>
        /// <param name="partographIds">List of partograph IDs to fetch</param>
        /// <param name="chunkSize">Max IDs per query (default 100, max 500 due to SQLite limits)</param>
        public virtual async Task<Dictionary<Guid, List<T>>> ListByPartographIdsAsync(List<Guid> partographIds, int chunkSize = 100)
        {
            await Init();
            var result = partographIds.ToDictionary(id => id, _ => new List<T>());

            if (!partographIds.Any())
                return result;

            // Ensure chunk size doesn't exceed SQLite parameter limit (999)
            chunkSize = Math.Min(chunkSize, 500);

            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                // Process in chunks to avoid parameter limits and reduce memory pressure
                var chunks = partographIds
                    .Select((id, index) => new { id, index })
                    .GroupBy(x => x.index / chunkSize)
                    .Select(g => g.Select(x => x.id).ToList())
                    .ToList();

                foreach (var chunk in chunks)
                {
                    var parameters = chunk.Select((id, i) => $"@id{i}").ToList();
                    var selectCmd = connection.CreateCommand();
                    selectCmd.CommandText = $@"SELECT m.*, s.name as staffname
                        FROM {TableName} m
                        LEFT JOIN Tbl_Staff s ON m.handler = s.ID
                        WHERE m.partographid IN ({string.Join(",", parameters)}) AND m.deleted = 0
                        ORDER BY m.partographid, m.time DESC";

                    for (int i = 0; i < chunk.Count; i++)
                    {
                        selectCmd.Parameters.AddWithValue($"@id{i}", chunk[i].ToString());
                    }

                    await using var reader = await selectCmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        var item = MapFromReader(reader);
                        if (item.PartographID.HasValue && result.ContainsKey(item.PartographID.Value))
                        {
                            result[item.PartographID.Value].Add(item);
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                _logger.LogError(e, $"Error batch fetching from {TableName}");
                throw new Exception(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error batch fetching from {TableName}");
                throw new Exception(e.Message);
            }

            return result;
        }

        public virtual async Task<Guid?> SaveItemAsync(T item)
        {
            await Init();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var saveCmd = connection.CreateCommand();
                if (item.ID == null)
                {
                    saveCmd.CommandText = GetInsertSql();
                    AddInsertParameters(saveCmd, item);
                    if (await saveCmd.ExecuteNonQueryAsync() > 0)
                    {
                        var x = item.ID;
                    }
                    else
                        item.ID = null;
                    //item.ID = Guid.Parse(Convert.ToString(result));
                }
                else
                {
                    saveCmd.CommandText = GetUpdateSql();
                    AddUpdateParameters(saveCmd, item);
                    await saveCmd.ExecuteNonQueryAsync();
                }
            }
            catch (SqliteException e)
            {
                throw new Exception(e.Message);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return item.ID;
        }

        protected abstract T MapFromReader(SqliteDataReader reader);
        protected abstract string GetInsertSql();
        protected abstract string GetUpdateSql();
        protected abstract void AddInsertParameters(SqliteCommand cmd, T item);
        protected abstract void AddUpdateParameters(SqliteCommand cmd, T item);
    }
}