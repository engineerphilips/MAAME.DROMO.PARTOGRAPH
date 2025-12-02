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
                selectCmd.CommandText = $"SELECT * FROM {TableName} WHERE partographid = @partographid ORDER BY time DESC";
                selectCmd.Parameters.AddWithValue("@partographid", id.ToString());

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

        public virtual async Task<T?> GetLatestByPatientAsync(Guid? id)
        {
            await Init();
            try
            {
                await using var connection = new SqliteConnection(Constants.DatabasePath);
                await connection.OpenAsync();

                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = $"SELECT * FROM {TableName} WHERE partographid = @partographid ORDER BY time DESC LIMIT 1";
                selectCmd.Parameters.AddWithValue("@partographid", id.ToString());

                await using var reader = await selectCmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return MapFromReader(reader);
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return null;
        }

        public virtual async Task<Guid?> SaveItemAsync(T item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();
            if (item.ID == null)
            {
                saveCmd.CommandText = GetInsertSql();
                AddInsertParameters(saveCmd, item);
                await saveCmd.ExecuteScalarAsync();
                //item.ID = Guid.Parse(Convert.ToString(result));
            }
            else
            {
                saveCmd.CommandText = GetUpdateSql();
                AddUpdateParameters(saveCmd, item);
                await saveCmd.ExecuteNonQueryAsync();
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