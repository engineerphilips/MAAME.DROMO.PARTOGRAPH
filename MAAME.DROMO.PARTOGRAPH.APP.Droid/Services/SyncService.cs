using MAAME.DROMO.PARTOGRAPH.APP.Droid.Data;
using MAAME.DROMO.PARTOGRAPH.MODEL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;

/// <summary>
/// Service that manages offline sync operations
/// </summary>
public class SyncService : ISyncService
{
    private readonly ISyncApiClient _apiClient;
    private readonly IConnectivityService _connectivityService;
    private readonly PatientRepository _patientRepository;
    private readonly PartographRepository _partographRepository;
    private readonly ILogger<SyncService> _logger;

    private SyncStatus _currentStatus = SyncStatus.Idle;
    private DateTime? _lastSyncTime;
    private CancellationTokenSource? _cancellationTokenSource;

    public SyncService(
        ISyncApiClient apiClient,
        IConnectivityService connectivityService,
        PatientRepository patientRepository,
        PartographRepository partographRepository,
        ILogger<SyncService> logger)
    {
        _apiClient = apiClient;
        _connectivityService = connectivityService;
        _patientRepository = patientRepository;
        _partographRepository = partographRepository;
        _logger = logger;

        // Load last sync time from preferences
        var lastSyncTicks = Preferences.Get("LastSyncTime", 0L);
        if (lastSyncTicks > 0)
        {
            _lastSyncTime = new DateTime(lastSyncTicks);
        }
    }

    /// <inheritdoc/>
    public SyncStatus CurrentStatus
    {
        get => _currentStatus;
        private set
        {
            if (_currentStatus != value)
            {
                _currentStatus = value;
                StatusChanged?.Invoke(this, value);
            }
        }
    }

    /// <inheritdoc/>
    public DateTime? LastSyncTime => _lastSyncTime;

    /// <inheritdoc/>
    public bool IsSyncing => CurrentStatus == SyncStatus.Syncing;

    /// <inheritdoc/>
    public event EventHandler<SyncStatus>? StatusChanged;

    /// <inheritdoc/>
    public event EventHandler<SyncProgress>? ProgressChanged;

    /// <inheritdoc/>
    public async Task<SyncResult> SyncAsync(CancellationToken cancellationToken = default)
    {
        if (IsSyncing)
        {
            throw new InvalidOperationException("Sync is already in progress");
        }

        if (!_connectivityService.IsConnected)
        {
            return new SyncResult
            {
                Success = false,
                SyncTime = DateTime.Now,
                ErrorMessages = new List<string> { "No network connection available" }
            };
        }

        var stopwatch = Stopwatch.StartNew();
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            CurrentStatus = SyncStatus.Syncing;
            _logger.LogInformation("Starting full synchronization");

            var pushResult = await PushAsync(_cancellationTokenSource.Token);
            var pullResult = await PullAsync(_cancellationTokenSource.Token);

            var combinedResult = new SyncResult
            {
                Success = pushResult.Success && pullResult.Success,
                SyncTime = DateTime.Now,
                TotalPushed = pushResult.TotalPushed,
                TotalPulled = pullResult.TotalPulled,
                TotalConflicts = pushResult.TotalConflicts + pullResult.TotalConflicts,
                TotalErrors = pushResult.TotalErrors + pullResult.TotalErrors,
                ErrorMessages = pushResult.ErrorMessages.Concat(pullResult.ErrorMessages).ToList(),
                Duration = stopwatch.Elapsed
            };

            if (combinedResult.Success)
            {
                CurrentStatus = SyncStatus.Success;
                _lastSyncTime = DateTime.Now;
                Preferences.Set("LastSyncTime", _lastSyncTime.Value.Ticks);
            }
            else if (combinedResult.TotalConflicts > 0)
            {
                CurrentStatus = SyncStatus.Conflict;
            }
            else
            {
                CurrentStatus = SyncStatus.Error;
            }

            _logger.LogInformation(
                "Sync completed: Pushed={Pushed}, Pulled={Pulled}, Conflicts={Conflicts}, Errors={Errors}",
                combinedResult.TotalPushed,
                combinedResult.TotalPulled,
                combinedResult.TotalConflicts,
                combinedResult.TotalErrors
            );

            return combinedResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync failed with exception");
            CurrentStatus = SyncStatus.Error;

            return new SyncResult
            {
                Success = false,
                SyncTime = DateTime.Now,
                ErrorMessages = new List<string> { ex.Message },
                Duration = stopwatch.Elapsed
            };
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            if (CurrentStatus == SyncStatus.Syncing)
            {
                CurrentStatus = SyncStatus.Idle;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<SyncResult> PushAsync(CancellationToken cancellationToken = default)
    {
        var result = new SyncResult { SyncTime = DateTime.Now };
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting push operation");

            var deviceId = DeviceIdentity.GetOrCreateDeviceId();

            // Push patients
            var patientsProgress = new SyncProgress { TableName = "Patients", CurrentOperation = "Pushing patients" };
            ProgressChanged?.Invoke(this, patientsProgress);

            var pendingPatients = await GetPendingPatientsAsync();
            patientsProgress.TotalRecords = pendingPatients.Count;

            if (pendingPatients.Any())
            {
                var pushRequest = new SyncPushRequest<Patient>
                {
                    DeviceId = deviceId,
                    Changes = pendingPatients
                };

                var pushResponse = await _apiClient.PushPatientsAsync(pushRequest);

                result.TotalPushed += pushResponse.SuccessIds.Count;
                result.TotalConflicts += pushResponse.Conflicts.Count;
                result.TotalErrors += pushResponse.Errors.Count;

                // Mark successful records as synced
                await MarkRecordsAsSyncedAsync("Tbl_Patient", pushResponse.SuccessIds);

                // Store conflicts
                await StoreConflictsAsync(pushResponse.Conflicts);

                patientsProgress.SuccessCount = pushResponse.SuccessIds.Count;
                patientsProgress.ConflictCount = pushResponse.Conflicts.Count;
                patientsProgress.ErrorCount = pushResponse.Errors.Count;
                patientsProgress.ProcessedRecords = pendingPatients.Count;
                ProgressChanged?.Invoke(this, patientsProgress);
            }

            // Push partographs
            var partographsProgress = new SyncProgress { TableName = "Partographs", CurrentOperation = "Pushing partographs" };
            ProgressChanged?.Invoke(this, partographsProgress);

            var pendingPartographs = await GetPendingPartographsAsync();
            partographsProgress.TotalRecords = pendingPartographs.Count;

            if (pendingPartographs.Any())
            {
                var pushRequest = new SyncPushRequest<Partograph>
                {
                    DeviceId = deviceId,
                    Changes = pendingPartographs
                };

                var pushResponse = await _apiClient.PushPartographsAsync(pushRequest);

                result.TotalPushed += pushResponse.SuccessIds.Count;
                result.TotalConflicts += pushResponse.Conflicts.Count;
                result.TotalErrors += pushResponse.Errors.Count;

                await MarkRecordsAsSyncedAsync("Tbl_Partograph", pushResponse.SuccessIds);
                await StoreConflictsAsync(pushResponse.Conflicts);

                partographsProgress.SuccessCount = pushResponse.SuccessIds.Count;
                partographsProgress.ConflictCount = pushResponse.Conflicts.Count;
                partographsProgress.ErrorCount = pushResponse.Errors.Count;
                partographsProgress.ProcessedRecords = pendingPartographs.Count;
                ProgressChanged?.Invoke(this, partographsProgress);
            }

            result.Success = result.TotalErrors == 0;
            result.Duration = stopwatch.Elapsed;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Push operation failed");
            result.Success = false;
            result.ErrorMessages.Add(ex.Message);
            result.Duration = stopwatch.Elapsed;
            return result;
        }
    }

    /// <inheritdoc/>
    public async Task<SyncResult> PullAsync(CancellationToken cancellationToken = default)
    {
        var result = new SyncResult { SyncTime = DateTime.Now };
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting pull operation");

            var deviceId = DeviceIdentity.GetOrCreateDeviceId();
            var lastPullTimestamp = Preferences.Get("LastPullTimestamp", 0L);

            // Pull patients
            var patientsProgress = new SyncProgress { TableName = "Tbl_Patient", CurrentOperation = "Pulling patients" };
            ProgressChanged?.Invoke(this, patientsProgress);

            var patientPullRequest = new SyncPullRequest
            {
                DeviceId = deviceId,
                LastSyncTimestamp = lastPullTimestamp,
                TableName = "Tbl_Patient"
            };

            var patientPullResponse = await _apiClient.PullPatientsAsync(patientPullRequest);

            if (patientPullResponse.Records.Any())
            {
                await MergePatients(patientPullResponse.Records);
                result.TotalPulled += patientPullResponse.Records.Count;
            }

            patientsProgress.TotalRecords = patientPullResponse.Records.Count;
            patientsProgress.ProcessedRecords = patientPullResponse.Records.Count;
            patientsProgress.SuccessCount = patientPullResponse.Records.Count;
            ProgressChanged?.Invoke(this, patientsProgress);

            // Pull partographs
            var partographsProgress = new SyncProgress { TableName = "Tbl_Partograph", CurrentOperation = "Pulling partographs" };
            ProgressChanged?.Invoke(this, partographsProgress);

            var partographPullRequest = new SyncPullRequest
            {
                DeviceId = deviceId,
                LastSyncTimestamp = lastPullTimestamp,
                TableName = "Tbl_Partograph"
            };

            var partographPullResponse = await _apiClient.PullPartographsAsync(partographPullRequest);

            if (partographPullResponse.Records.Any())
            {
                await MergePartographs(partographPullResponse.Records);
                result.TotalPulled += partographPullResponse.Records.Count;
            }

            partographsProgress.TotalRecords = partographPullResponse.Records.Count;
            partographsProgress.ProcessedRecords = partographPullResponse.Records.Count;
            partographsProgress.SuccessCount = partographPullResponse.Records.Count;
            ProgressChanged?.Invoke(this, partographsProgress);

            // Update last pull timestamp
            Preferences.Set("LastPullTimestamp", DateTimeOffset.Now.ToUnixTimeMilliseconds());

            result.Success = true;
            result.Duration = stopwatch.Elapsed;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pull operation failed");
            result.Success = false;
            result.ErrorMessages.Add(ex.Message);
            result.Duration = stopwatch.Elapsed;
            return result;
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetPendingChangesCountAsync()
    {
        try
        {
            var count = 0;

            using var connection = new SqliteConnection($"Data Source={Constants.DatabasePath}");
            await connection.OpenAsync();

            // Count pending patients
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Tbl_Patient WHERE SyncStatus = 0";
                var result = await command.ExecuteScalarAsync();
                count += Convert.ToInt32(result);
            }

            // Count pending partographs
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Tbl_Partograph WHERE SyncStatus = 0";
                var result = await command.ExecuteScalarAsync();
                count += Convert.ToInt32(result);
            }

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending changes count");
            return 0;
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetConflictsCountAsync()
    {
        try
        {
            var count = 0;

            using var connection = new SqliteConnection($"Data Source={Constants.DatabasePath}");
            await connection.OpenAsync();

            // Count conflicted patients
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Tbl_Patient WHERE SyncStatus = 2";
                var result = await command.ExecuteScalarAsync();
                count += Convert.ToInt32(result);
            }

            // Count conflicted partographs
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Tbl_Partograph WHERE SyncStatus = 2";
                var result = await command.ExecuteScalarAsync();
                count += Convert.ToInt32(result);
            }

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conflicts count");
            return 0;
        }
    }

    /// <inheritdoc/>
    public async Task ResolveConflictAsync(string recordId, bool useLocalVersion)
    {
        _logger.LogInformation("Resolving conflict for record {RecordId}, useLocal={UseLocal}", recordId, useLocalVersion);

        using var connection = new SqliteConnection($"Data Source={Constants.DatabasePath}");
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        if (useLocalVersion)
        {
            // Mark as pending to push again
            command.CommandText = @"
                UPDATE Tbl_Patient
                SET SyncStatus = 0, ConflictData = NULL
                WHERE ID = @id
                UNION ALL
                UPDATE Tbl_Partograph
                SET SyncStatus = 0, ConflictData = NULL
                WHERE ID = @id";
        }
        else
        {
            // Accept server version (implementation depends on how ConflictData is stored)
            command.CommandText = @"
                UPDATE Tbl_Patient
                SET SyncStatus = 1, ConflictData = NULL
                WHERE ID = @id
                UNION ALL
                UPDATE Tbl_Partograph
                SET SyncStatus = 1, ConflictData = NULL
                WHERE ID = @id";
        }

        command.Parameters.AddWithValue("@id", recordId);
        await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public async Task CancelSyncAsync()
    {
        _logger.LogInformation("Cancelling sync operation");
        _cancellationTokenSource?.Cancel();
        CurrentStatus = SyncStatus.Idle;
        await Task.CompletedTask;
    }

    // Private helper methods

    private async Task<List<Patient>> GetPendingPatientsAsync()
    {
        using var connection = new SqliteConnection($"Data Source={Constants.DatabasePath}");
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Tbl_Patient WHERE SyncStatus = 0";

        var patients = new List<Patient>();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            patients.Add(MapPatientFromReader(reader));
        }

        return patients;
    }

    private async Task<List<Partograph>> GetPendingPartographsAsync()
    {
        using var connection = new SqliteConnection($"Data Source={Constants.DatabasePath}");
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Tbl_Partograph WHERE SyncStatus = 0";

        var partographs = new List<Partograph>();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            partographs.Add(MapPartographFromReader(reader));
        }

        return partographs;
    }

    private async Task MarkRecordsAsSyncedAsync(string tableName, List<string> recordIds)
    {
        if (!recordIds.Any()) return;

        using var connection = new SqliteConnection($"Data Source={Constants.DatabasePath}");
        await connection.OpenAsync();

        foreach (var id in recordIds)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"UPDATE {tableName} SET SyncStatus = 1 WHERE ID = @id";
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task StoreConflictsAsync<T>(List<ConflictRecord<T>> conflicts)
    {
        foreach (var conflict in conflicts)
        {
            using var connection = new SqliteConnection($"Data Source={Constants.DatabasePath}");
            await connection.OpenAsync();

            using var command = connection.CreateCommand();

            // Determine table name
            var tableName = typeof(T) == typeof(Patient) ? "Tbl_Patient" : "Tbl_Partograph";

            command.CommandText = $@"
                UPDATE {tableName}
                SET SyncStatus = 2,
                    ConflictData = @conflictData
                WHERE ID = @id";

            command.Parameters.AddWithValue("@id", conflict.Id);
            command.Parameters.AddWithValue("@conflictData", System.Text.Json.JsonSerializer.Serialize(conflict.ServerRecord));

            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task MergePatients(List<Patient> patients)
    {
        foreach (var patient in patients)
        {
            await _patientRepository.UpsertPatientAsync(patient);
        }
    }

    private async Task MergePartographs(List<Partograph> partographs)
    {
        foreach (var partograph in partographs)
        {
            await _partographRepository.UpsertPartographAsync(partograph);
        }
    }

    private Patient MapPatientFromReader(SqliteDataReader reader)
    {
        return new Patient
        {
            ID = Guid.Parse(reader["ID"].ToString()!),
            FirstName = reader["FirstName"]?.ToString() ?? string.Empty,
            LastName = reader["LastName"]?.ToString() ?? string.Empty,
            DateOfBirth = DateTime.Parse(reader["DateOfBirth"]?.ToString() ?? DateTime.Now.ToString()),
            // Add other properties as needed
        };
    }

    private Partograph MapPartographFromReader(SqliteDataReader reader)
    {
        return new Partograph
        {
            ID = Guid.Parse(reader["ID"].ToString()!),
            PatientID = Guid.Parse(reader["PatientID"].ToString()!),
            AdmissionDate = DateTime.Parse(reader["AdmissionDate"]?.ToString() ?? DateTime.Now.ToString()),
            // Add other properties as needed
        };
    }
}
