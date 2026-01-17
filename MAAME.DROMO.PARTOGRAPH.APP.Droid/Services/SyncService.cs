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
    private readonly FHRRepository _fHRRepository;
    private readonly BPRepository _bPRepository;
    private readonly PlanRepository _planRepository;
    private readonly CaputRepository _caputRepository;
    private readonly UrineRepository _urineRepository;
    private readonly IVFluidEntryRepository _ivFluidEntryRepository;
    private readonly PostureRepository _postureRepository;
    private readonly MouldingRepository _mouldingRepository;
    private readonly OxytocinRepository _oxytocinRepository;
    private readonly CompanionRepository _companionRepository;
    private readonly OralFluidRepository _oralFluidRepository;
    private readonly AssessmentRepository _assessmentRepository;
    private readonly CervixDilatationRepository _cervixDilatationRepository;
    private readonly MedicationEntryRepository _medicationEntryRepository;
    private readonly PainReliefEntryRepository _painReliefEntryRepository;
    private readonly BishopScoreRepository _bishopScoreRepository;
    private readonly ContractionRepository _contractionRepository;
    private readonly HeadDescentRepository _headDescentRepository;
    private readonly TemperatureRepository _temperatureRepository;
    private readonly AmnioticFluidRepository _amnioticFluidRepository;
    private readonly FetalPositionRepository _fetalPositionRepository;
    private readonly ILogger<SyncService> _logger;

    private SyncStatus _currentStatus = SyncStatus.Idle;
    private DateTime? _lastSyncTime;
    private CancellationTokenSource? _cancellationTokenSource;

    public SyncService(
        ISyncApiClient apiClient,
        IConnectivityService connectivityService,
        PatientRepository patientRepository,
        PartographRepository partographRepository,
        FHRRepository fHRRepository,
        BPRepository bPRepository,
        PlanRepository planRepository,
        CaputRepository caputRepository,
        UrineRepository urineRepository,
        IVFluidEntryRepository ivFluidEntryRepository,
        PostureRepository postureRepository,
        MouldingRepository mouldingRepository,
        OxytocinRepository oxytocinRepository,
        CompanionRepository companionRepository,
        OralFluidRepository oralFluidRepository,
        AssessmentRepository assessmentRepository,
        CervixDilatationRepository cervixDilatationRepository,
        MedicationEntryRepository medicationEntryRepository,
        PainReliefEntryRepository painReliefEntryRepository,
        BishopScoreRepository bishopScoreRepository,
        ContractionRepository contractionRepository,
        HeadDescentRepository headDescentRepository,
        TemperatureRepository temperatureRepository,
        AmnioticFluidRepository amnioticFluidRepository,
        FetalPositionRepository fetalPositionRepository,
        ILogger<SyncService> logger)
    {
        _apiClient = apiClient;
        _connectivityService = connectivityService;
        _patientRepository = patientRepository;
        _partographRepository = partographRepository;
        _fHRRepository = fHRRepository;
        _bPRepository = bPRepository;
        _planRepository = planRepository;
        _caputRepository = caputRepository;
        _urineRepository = urineRepository;
        _ivFluidEntryRepository = ivFluidEntryRepository;
        _postureRepository = postureRepository;
        _mouldingRepository = mouldingRepository;
        _oxytocinRepository = oxytocinRepository;
        _companionRepository = companionRepository;
        _oralFluidRepository = oralFluidRepository;
        _assessmentRepository = assessmentRepository;
        _cervixDilatationRepository = cervixDilatationRepository;
        _medicationEntryRepository = medicationEntryRepository;
        _painReliefEntryRepository = painReliefEntryRepository;
        _bishopScoreRepository = bishopScoreRepository;
        _contractionRepository = contractionRepository;
        _headDescentRepository = headDescentRepository;
        _temperatureRepository = temperatureRepository;
        _amnioticFluidRepository = amnioticFluidRepository;
        _fetalPositionRepository = fetalPositionRepository;
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
            var patientsProgress = new SyncProgress { TableName = "Tbl_Patient", CurrentOperation = "Pushing patients" };
            ProgressChanged?.Invoke(this, patientsProgress);

            var pendingPatients = await GetPendingPatientsAsync();
            patientsProgress.TotalRecords = pendingPatients.Count;

            foreach (var item in pendingPatients)
            {
                if (string.IsNullOrWhiteSpace(item.DeviceId))
                    item.DeviceId = deviceId;

                if (string.IsNullOrWhiteSpace(item.OriginDeviceId))
                    item.OriginDeviceId = deviceId;

                if (string.IsNullOrWhiteSpace(item.DataHash))
                    item.DataHash = item.CalculateHash();

                if (string.IsNullOrWhiteSpace(item.ConflictData))
                    item.ConflictData = "{}";
            }

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
            var partographsProgress = new SyncProgress { TableName = "Tbl_Partograph", CurrentOperation = "Pushing partographs" };
            ProgressChanged?.Invoke(this, partographsProgress);

            var pendingPartographs = await GetPendingPartographsAsync();
            partographsProgress.TotalRecords = pendingPartographs.Count;

            // Validate partograph fields
            foreach (var item in pendingPartographs)
            {
                if (string.IsNullOrWhiteSpace(item.DeviceId))
                    item.DeviceId = deviceId;

                if (string.IsNullOrWhiteSpace(item.OriginDeviceId))
                    item.OriginDeviceId = deviceId;

                if (string.IsNullOrWhiteSpace(item.DataHash))
                    item.DataHash = item.CalculateHash();

                if (string.IsNullOrWhiteSpace(item.ConflictData))
                    item.ConflictData = "{}";
            }

            // Batch fetch all measurables in parallel (21 queries instead of 21 * N)
            if (pendingPartographs.Any())
            {
                var partographIds = pendingPartographs.Where(p => p.ID.HasValue).Select(p => p.ID!.Value).ToList();

                // Execute all batch queries in parallel
                var bpsTask = _bPRepository.ListByPartographIdsAsync(partographIds);
                var fhrsTask = _fHRRepository.ListByPartographIdsAsync(partographIds);
                var plansTask = _planRepository.ListByPartographIdsAsync(partographIds);
                var caputsTask = _caputRepository.ListByPartographIdsAsync(partographIds);
                var urinesTask = _urineRepository.ListByPartographIdsAsync(partographIds);
                var ivFluidsTask = _ivFluidEntryRepository.ListByPartographIdsAsync(partographIds);
                var posturesTask = _postureRepository.ListByPartographIdsAsync(partographIds);
                var mouldingsTask = _mouldingRepository.ListByPartographIdsAsync(partographIds);
                var oxytocinsTask = _oxytocinRepository.ListByPartographIdsAsync(partographIds);
                var companionsTask = _companionRepository.ListByPartographIdsAsync(partographIds);
                var oralFluidsTask = _oralFluidRepository.ListByPartographIdsAsync(partographIds);
                var assessmentsTask = _assessmentRepository.ListByPartographIdsAsync(partographIds);
                var dilatationsTask = _cervixDilatationRepository.ListByPartographIdsAsync(partographIds);
                var medicationsTask = _medicationEntryRepository.ListByPartographIdsAsync(partographIds);
                var painReliefsTask = _painReliefEntryRepository.ListByPartographIdsAsync(partographIds);
                var bishopScoresTask = _bishopScoreRepository.ListByPartographIdsAsync(partographIds);
                var contractionsTask = _contractionRepository.ListByPartographIdsAsync(partographIds);
                var headDescentsTask = _headDescentRepository.ListByPartographIdsAsync(partographIds);
                var temperaturesTask = _temperatureRepository.ListByPartographIdsAsync(partographIds);
                var amnioticFluidsTask = _amnioticFluidRepository.ListByPartographIdsAsync(partographIds);
                var fetalPositionsTask = _fetalPositionRepository.ListByPartographIdsAsync(partographIds);

                await Task.WhenAll(
                    bpsTask, fhrsTask, plansTask, caputsTask, urinesTask, ivFluidsTask,
                    posturesTask, mouldingsTask, oxytocinsTask, companionsTask, oralFluidsTask,
                    assessmentsTask, dilatationsTask, medicationsTask, painReliefsTask, bishopScoresTask,
                    contractionsTask, headDescentsTask, temperaturesTask, amnioticFluidsTask, fetalPositionsTask
                );

                // Get results
                var allBPs = await bpsTask;
                var allFhrs = await fhrsTask;
                var allPlans = await plansTask;
                var allCaputs = await caputsTask;
                var allUrines = await urinesTask;
                var allIVFluids = await ivFluidsTask;
                var allPostures = await posturesTask;
                var allMouldings = await mouldingsTask;
                var allOxytocins = await oxytocinsTask;
                var allCompanions = await companionsTask;
                var allOralFluids = await oralFluidsTask;
                var allAssessments = await assessmentsTask;
                var allDilatations = await dilatationsTask;
                var allMedications = await medicationsTask;
                var allPainReliefs = await painReliefsTask;
                var allBishopScores = await bishopScoresTask;
                var allContractions = await contractionsTask;
                var allHeadDescents = await headDescentsTask;
                var allTemperatures = await temperaturesTask;
                var allAmnioticFluids = await amnioticFluidsTask;
                var allFetalPositions = await fetalPositionsTask;

                // Assign to each partograph
                foreach (var item in pendingPartographs)
                {
                    if (!item.ID.HasValue) continue;
                    var id = item.ID.Value;

                    item.BPs = allBPs.TryGetValue(id, out var bps) ? bps : new List<BP>();
                    item.Fhrs = allFhrs.TryGetValue(id, out var fhrs) ? fhrs : new List<FHR>();
                    item.Plans = allPlans.TryGetValue(id, out var plans) ? plans : new List<Plan>();
                    item.Caputs = allCaputs.TryGetValue(id, out var caputs) ? caputs : new List<Caput>();
                    item.Urines = allUrines.TryGetValue(id, out var urines) ? urines : new List<Urine>();
                    item.IVFluids = allIVFluids.TryGetValue(id, out var ivFluids) ? ivFluids : new List<IVFluidEntry>();
                    item.Postures = allPostures.TryGetValue(id, out var postures) ? postures : new List<PostureEntry>();
                    item.Mouldings = allMouldings.TryGetValue(id, out var mouldings) ? mouldings : new List<Moulding>();
                    item.Oxytocins = allOxytocins.TryGetValue(id, out var oxytocins) ? oxytocins : new List<Oxytocin>();
                    item.Companions = allCompanions.TryGetValue(id, out var companions) ? companions : new List<CompanionEntry>();
                    item.OralFluids = allOralFluids.TryGetValue(id, out var oralFluids) ? oralFluids : new List<OralFluidEntry>();
                    item.Assessments = allAssessments.TryGetValue(id, out var assessments) ? assessments : new List<Assessment>();
                    item.Dilatations = allDilatations.TryGetValue(id, out var dilatations) ? dilatations : new List<CervixDilatation>();
                    item.Medications = allMedications.TryGetValue(id, out var medications) ? medications : new List<MedicationEntry>();
                    item.PainReliefs = allPainReliefs.TryGetValue(id, out var painReliefs) ? painReliefs : new List<PainReliefEntry>();
                    item.BishopScores = allBishopScores.TryGetValue(id, out var bishopScores) ? bishopScores : new List<BishopScore>();
                    item.Contractions = allContractions.TryGetValue(id, out var contractions) ? contractions : new List<Contraction>();
                    item.HeadDescents = allHeadDescents.TryGetValue(id, out var headDescents) ? headDescents : new List<HeadDescent>();
                    item.Temperatures = allTemperatures.TryGetValue(id, out var temperatures) ? temperatures : new List<MODEL.Temperature>();
                    item.AmnioticFluids = allAmnioticFluids.TryGetValue(id, out var amnioticFluids) ? amnioticFluids : new List<AmnioticFluid>();
                    item.FetalPositions = allFetalPositions.TryGetValue(id, out var fetalPositions) ? fetalPositions : new List<FetalPosition>();
                }
            }

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
            long latestServerTimestamp = lastPullTimestamp;

            // Pull patients with pagination
            var patientsProgress = new SyncProgress { TableName = "Tbl_Patient", CurrentOperation = "Pulling patients" };
            ProgressChanged?.Invoke(this, patientsProgress);

            var patientsPulled = await PullWithPaginationAsync<Patient>(
                deviceId,
                lastPullTimestamp,
                "Tbl_Patient",
                async (request) => await _apiClient.PullPatientsAsync(request),
                async (records) => await MergePatients(records),
                patientsProgress,
                cancellationToken);

            result.TotalPulled += patientsPulled.recordCount;
            if (patientsPulled.serverTimestamp > latestServerTimestamp)
            {
                latestServerTimestamp = patientsPulled.serverTimestamp;
            }

            // Pull partographs with pagination
            var partographsProgress = new SyncProgress { TableName = "Tbl_Partograph", CurrentOperation = "Pulling partographs" };
            ProgressChanged?.Invoke(this, partographsProgress);

            var partographsPulled = await PullWithPaginationAsync<Partograph>(
                deviceId,
                lastPullTimestamp,
                "Tbl_Partograph",
                async (request) => await _apiClient.PullPartographsAsync(request),
                async (records) => await MergePartographs(records),
                partographsProgress,
                cancellationToken);

            result.TotalPulled += partographsPulled.recordCount;
            if (partographsPulled.serverTimestamp > latestServerTimestamp)
            {
                latestServerTimestamp = partographsPulled.serverTimestamp;
            }

            // Update last pull timestamp using SERVER timestamp (not device time)
            // This prevents clock skew issues between device and server
            if (latestServerTimestamp > lastPullTimestamp)
            {
                Preferences.Set("LastPullTimestamp", latestServerTimestamp);
                _logger.LogInformation("Updated LastPullTimestamp to server time: {Timestamp}", latestServerTimestamp);
            }

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

    /// <summary>
    /// Pulls data from server with pagination support
    /// </summary>
    private async Task<(int recordCount, long serverTimestamp)> PullWithPaginationAsync<T>(
        string deviceId,
        long lastSyncTimestamp,
        string tableName,
        Func<SyncPullRequest, Task<SyncPullResponse<T>>> pullFunc,
        Func<List<T>, Task> mergeFunc,
        SyncProgress progress,
        CancellationToken cancellationToken)
    {
        int totalPulled = 0;
        long latestServerTimestamp = lastSyncTimestamp;
        bool hasMore = true;
        var currentTimestamp = lastSyncTimestamp;

        while (hasMore && !cancellationToken.IsCancellationRequested)
        {
            var pullRequest = new SyncPullRequest
            {
                DeviceId = deviceId,
                LastSyncTimestamp = currentTimestamp,
                TableName = tableName
            };

            var pullResponse = await pullFunc(pullRequest);

            if (pullResponse.Records.Any())
            {
                await mergeFunc(pullResponse.Records);
                totalPulled += pullResponse.Records.Count;

                // Update progress
                progress.TotalRecords += pullResponse.Records.Count;
                progress.ProcessedRecords = totalPulled;
                progress.SuccessCount = totalPulled;
                ProgressChanged?.Invoke(this, progress);
            }

            // Use server timestamp for next page and final storage
            if (pullResponse.ServerTimestamp > latestServerTimestamp)
            {
                latestServerTimestamp = pullResponse.ServerTimestamp;
            }

            // For pagination, use the timestamp from the last record to get next page
            currentTimestamp = pullResponse.ServerTimestamp;
            hasMore = pullResponse.HasMore;

            _logger.LogDebug("Pulled {Count} {Table} records, hasMore={HasMore}",
                pullResponse.Records.Count, tableName, hasMore);
        }

        return (totalPulled, latestServerTimestamp);
    }

    /// <inheritdoc/>
    public async Task<int> GetPendingChangesCountAsync()
    {
        try
        {
            var count = 0;

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            //using var connection = new SqliteConnection($"Data Source={Constants.DatabasePath}");
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

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            //using var connection = new SqliteConnection($"Data Source={Constants.DatabasePath}");
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

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        //using var connection = new SqliteConnection($"Data Source={Constants.DatabasePath}");
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
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        //using var connection = new SqliteConnection($"Data Source={Constants.DatabasePath}");
        await connection.OpenAsync();

        //try
        //{
        //    var dropTableCmd = connection.CreateCommand();
        //    dropTableCmd.CommandText = @"
        //    UPDATE Tbl_Patient SET SyncStatus = 1;";
        //    await dropTableCmd.ExecuteNonQueryAsync();
        //}
        //catch (Exception e)
        //{
        //    _logger.LogError(e, "Error dropping SyncStatus table");
        //    throw;
        //}

        var patients = new List<Patient>();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Tbl_Patient WHERE SyncStatus = 0";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                patients.Add(MapPatientFromReader(reader));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving Patient table");
            throw;
        }

        return patients;
    }

    private async Task<List<Partograph>> GetPendingPartographsAsync()
    {
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        //using var connection = new SqliteConnection($"Data Source={Constants.DatabasePath}");
        await connection.OpenAsync();

        var partographs = new List<Partograph>();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Tbl_Partograph WHERE SyncStatus = 0";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                partographs.Add(MapPartographFromReader(reader));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving Partograph table");
            throw;
        }

        return partographs;
    }

    /// <summary>
    /// Marks records as synced using batch update for better performance
    /// </summary>
    private async Task MarkRecordsAsSyncedAsync(string tableName, List<string> recordIds)
    {
        if (!recordIds.Any()) return;

        //using var connection = new SqliteConnection($"Data Source={Constants.DatabasePath}");
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        // Use transaction for batch operations (DATA INTEGRITY)
        using var transaction = connection.BeginTransaction();

        try
        {
            // Batch update using parameterized IN clause
            // SQLite doesn't support array parameters, so we batch in chunks
            const int batchSize = 100;
            var batches = recordIds
                .Select((id, index) => new { id, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.id).ToList());

            foreach (var batch in batches)
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                // Build parameterized query for this batch
                var parameters = batch.Select((id, i) => $"@id{i}").ToList();
                command.CommandText = $"UPDATE {tableName} SET SyncStatus = 1 WHERE ID IN ({string.Join(",", parameters)})";

                for (int i = 0; i < batch.Count; i++)
                {
                    command.Parameters.AddWithValue($"@id{i}", batch[i]);
                }

                await command.ExecuteNonQueryAsync();
            }

            transaction.Commit();
            _logger.LogDebug("Batch marked {Count} records as synced in {Table}", recordIds.Count, tableName);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Failed to batch mark records as synced, rolling back");
            throw;
        }
    }

    /// <summary>
    /// Stores conflicts with transaction scope for data integrity
    /// </summary>
    private async Task StoreConflictsAsync<T>(List<ConflictRecord<T>> conflicts)
    {
        if (!conflicts.Any()) return;

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        //using var connection = new SqliteConnection($"Data Source={Constants.DatabasePath}");
        await connection.OpenAsync();

        // Use transaction for batch operations (DATA INTEGRITY)
        using var transaction = connection.BeginTransaction();

        try
        {
            // Determine table name
            var tableName = typeof(T) == typeof(Patient) ? "Tbl_Patient" : "Tbl_Partograph";

            foreach (var conflict in conflicts)
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = $@"
                    UPDATE {tableName}
                    SET SyncStatus = 2,
                        ConflictData = @conflictData
                    WHERE ID = @id";

                command.Parameters.AddWithValue("@id", conflict.Id);
                command.Parameters.AddWithValue("@conflictData", System.Text.Json.JsonSerializer.Serialize(conflict.ServerRecord));

                await command.ExecuteNonQueryAsync();
            }

            transaction.Commit();
            _logger.LogDebug("Stored {Count} conflicts for {Table}", conflicts.Count, tableName);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Failed to store conflicts, rolling back");
            throw;
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
            DateOfBirth = DateOnly.Parse(reader["DateOfBirth"]?.ToString() ?? DateTime.Now.ToString()),
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
