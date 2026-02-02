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
    private readonly PainReliefRepository _painReliefEntryRepository;
    private readonly BishopScoreRepository _bishopScoreRepository;
    private readonly ContractionRepository _contractionRepository;
    private readonly HeadDescentRepository _headDescentRepository;
    private readonly TemperatureRepository _temperatureRepository;
    private readonly AmnioticFluidRepository _amnioticFluidRepository;
    private readonly FetalPositionRepository _fetalPositionRepository;
    private readonly FourthStageVitalsRepository _fourthStageVitalsRepository;
    //private readonly MedicalNoteRepository _medicalNoteRepository;
    private readonly PartographDiagnosisRepository _partographDiagnosisRepository;
    private readonly PartographRiskFactorRepository _partographRiskFactorRepository;
    private readonly StaffRepository _staffRepository;
    private readonly BirthOutcomeRepository _birthOutcomeRepository;
    private readonly BabyDetailsRepository _babyDetailsRepository;
    private readonly ReferralRepository _referralRepository;
    private readonly FacilityRepository _facilityRepository;
    private readonly RegionRepository _regionRepository;
    private readonly DistrictRepository _districtRepository;
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
        PainReliefRepository painReliefEntryRepository,
        BishopScoreRepository bishopScoreRepository,
        ContractionRepository contractionRepository,
        HeadDescentRepository headDescentRepository,
        TemperatureRepository temperatureRepository,
        AmnioticFluidRepository amnioticFluidRepository,
        FetalPositionRepository fetalPositionRepository,
        FourthStageVitalsRepository fourthStageVitalsRepository,
        PartographDiagnosisRepository partographDiagnosisRepository,
        PartographRiskFactorRepository partographRiskFactorRepository,
        StaffRepository staffRepository,
        BirthOutcomeRepository birthOutcomeRepository,
        BabyDetailsRepository babyDetailsRepository,
        ReferralRepository referralRepository,
        FacilityRepository facilityRepository,
        RegionRepository regionRepository,
        DistrictRepository districtRepository,
        ILogger<SyncService> logger)
    {
        //MedicalNoteRepository medicalNoteRepository,
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
        _fourthStageVitalsRepository = fourthStageVitalsRepository;
        //_medicalNoteRepository = medicalNoteRepository;
        _partographDiagnosisRepository = partographDiagnosisRepository;
        _partographRiskFactorRepository = partographRiskFactorRepository;
        _staffRepository = staffRepository;
        _birthOutcomeRepository = birthOutcomeRepository;
        _babyDetailsRepository = babyDetailsRepository;
        _referralRepository = referralRepository;
        _facilityRepository = facilityRepository;
        _regionRepository = regionRepository;
        _districtRepository = districtRepository;
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

                if (string.IsNullOrWhiteSpace(item.Patient?.DeviceId))
                    item.Patient.DeviceId = deviceId;

                if (string.IsNullOrWhiteSpace(item.Patient?.OriginDeviceId))
                    item.Patient.OriginDeviceId = deviceId;

                if (string.IsNullOrWhiteSpace(item.Patient?.DataHash))
                    item.Patient.DataHash = item.Patient.CalculateHash();

                if (string.IsNullOrWhiteSpace(item.Patient?.ConflictData))
                    item.Patient.ConflictData = "{}";
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
                var fourthStageVitalsTask = _fourthStageVitalsRepository.ListByPartographIdsAsync(partographIds);
                //var medicalNotesTask = _medicalNoteRepository.ListByPartographIdsAsync(partographIds);
                var diagnosesTask = _partographDiagnosisRepository.ListByPartographIdsAsync(partographIds);
                var riskFactorsTask = _partographRiskFactorRepository.ListByPartographIdsAsync(partographIds);

                await Task.WhenAll(
                    bpsTask, fhrsTask, plansTask, caputsTask, urinesTask, ivFluidsTask,
                    posturesTask, mouldingsTask, oxytocinsTask, companionsTask, oralFluidsTask,
                    assessmentsTask, dilatationsTask, medicationsTask, painReliefsTask, bishopScoresTask,
                    contractionsTask, headDescentsTask, temperaturesTask, amnioticFluidsTask, fetalPositionsTask,
                    fourthStageVitalsTask, diagnosesTask, riskFactorsTask
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
                var allFourthStageVitals = await fourthStageVitalsTask;
                //var allMedicalNotes = await medicalNotesTask;
                var allDiagnoses = await diagnosesTask;
                var allRiskFactors = await riskFactorsTask;

                // Validate measurable fields for all fetched records
                foreach (var item in allBPs.Values.SelectMany(x => x))
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

                foreach (var item in allFhrs.Values.SelectMany(x => x))
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

                foreach (var item in allPlans.Values.SelectMany(x => x))
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

                foreach (var item in allCaputs.Values.SelectMany(x => x))
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

                foreach (var item in allUrines.Values.SelectMany(x => x))
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

                foreach (var item in allIVFluids.Values.SelectMany(x => x))
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

                foreach (var item in allPostures.Values.SelectMany(x => x))
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

                foreach (var item in allMouldings.Values.SelectMany(x => x))
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

                foreach (var item in allOxytocins.Values.SelectMany(x => x))
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

                foreach (var item in allCompanions.Values.SelectMany(x => x))
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

                foreach (var item in allOralFluids.Values.SelectMany(x => x))
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

                foreach (var item in allAssessments.Values.SelectMany(x => x))
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

                foreach (var item in allDilatations.Values.SelectMany(x => x))
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

                foreach (var item in allMedications.Values.SelectMany(x => x))
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

                foreach (var item in allPainReliefs.Values.SelectMany(x => x))
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

                foreach (var item in allBishopScores.Values.SelectMany(x => x))
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

                foreach (var item in allContractions.Values.SelectMany(x => x))
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

                foreach (var item in allHeadDescents.Values.SelectMany(x => x))
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

                foreach (var item in allTemperatures.Values.SelectMany(x => x))
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

                foreach (var item in allAmnioticFluids.Values.SelectMany(x => x))
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

                foreach (var item in allFetalPositions.Values.SelectMany(x => x))
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

                foreach (var item in allFourthStageVitals.Values.SelectMany(x => x))
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

                //foreach (var item in allMedicalNotes.Values.SelectMany(x => x))
                //{
                //    if (string.IsNullOrWhiteSpace(item.DeviceId))
                //        item.DeviceId = deviceId;
                //    if (string.IsNullOrWhiteSpace(item.OriginDeviceId))
                //        item.OriginDeviceId = deviceId;
                //    if (string.IsNullOrWhiteSpace(item.DataHash))
                //        item.DataHash = item.CalculateHash();
                //    if (string.IsNullOrWhiteSpace(item.ConflictData))
                //        item.ConflictData = "{}";
                //}

                foreach (var item in allDiagnoses.Values.SelectMany(x => x))
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

                foreach (var item in allRiskFactors.Values.SelectMany(x => x))
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
                    item.FourthStageVitals = allFourthStageVitals.TryGetValue(id, out var fourthStageVitals) ? fourthStageVitals : new List<FourthStageVitals>();
                    //item.MedicalNotes = allMedicalNotes.TryGetValue(id, out var medicalNotes) ? medicalNotes : new List<MedicalNote>();
                    item.Diagnoses = allDiagnoses.TryGetValue(id, out var diagnoses) ? diagnoses : new List<PartographDiagnosis>();
                    item.RiskFactors = allRiskFactors.TryGetValue(id, out var riskFactors) ? riskFactors : new List<PartographRiskFactor>();
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

            // Push staff
            var staffProgress = new SyncProgress { TableName = "Tbl_Staff", CurrentOperation = "Pushing staff" };
            ProgressChanged?.Invoke(this, staffProgress);

            var pendingStaff = await GetPendingStaffAsync();
            staffProgress.TotalRecords = pendingStaff.Count;

            foreach (var item in pendingStaff)
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

            if (pendingStaff.Any())
            {
                var pushRequest = new SyncPushRequest<Staff>
                {
                    DeviceId = deviceId,
                    Changes = pendingStaff
                };

                var pushResponse = await _apiClient.PushStaffAsync(pushRequest);

                result.TotalPushed += pushResponse.SuccessIds.Count;
                result.TotalConflicts += pushResponse.Conflicts.Count;
                result.TotalErrors += pushResponse.Errors.Count;

                await MarkRecordsAsSyncedAsync("Tbl_Staff", pushResponse.SuccessIds);
                await StoreStaffConflictsAsync(pushResponse.Conflicts);

                staffProgress.SuccessCount = pushResponse.SuccessIds.Count;
                staffProgress.ConflictCount = pushResponse.Conflicts.Count;
                staffProgress.ErrorCount = pushResponse.Errors.Count;
                staffProgress.ProcessedRecords = pendingStaff.Count;
                ProgressChanged?.Invoke(this, staffProgress);
            }

            // Push birth outcomes
            var birthOutcomeProgress = new SyncProgress { TableName = "Tbl_BirthOutcome", CurrentOperation = "Pushing birth outcomes" };
            ProgressChanged?.Invoke(this, birthOutcomeProgress);

            var pendingBirthOutcomes = await GetPendingBirthOutcomesAsync();
            birthOutcomeProgress.TotalRecords = pendingBirthOutcomes.Count;

            foreach (var item in pendingBirthOutcomes)
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

            if (pendingBirthOutcomes.Any())
            {
                var pushRequest = new SyncPushRequest<BirthOutcome>
                {
                    DeviceId = deviceId,
                    Changes = pendingBirthOutcomes
                };

                var pushResponse = await _apiClient.PushBirthOutcomesAsync(pushRequest);

                result.TotalPushed += pushResponse.SuccessIds.Count;
                result.TotalConflicts += pushResponse.Conflicts.Count;
                result.TotalErrors += pushResponse.Errors.Count;

                await MarkRecordsAsSyncedAsync("Tbl_BirthOutcome", pushResponse.SuccessIds);

                birthOutcomeProgress.SuccessCount = pushResponse.SuccessIds.Count;
                birthOutcomeProgress.ConflictCount = pushResponse.Conflicts.Count;
                birthOutcomeProgress.ErrorCount = pushResponse.Errors.Count;
                birthOutcomeProgress.ProcessedRecords = pendingBirthOutcomes.Count;
                ProgressChanged?.Invoke(this, birthOutcomeProgress);
            }

            // Push baby details
            var babyDetailsProgress = new SyncProgress { TableName = "Tbl_Baby", CurrentOperation = "Pushing baby details" };
            ProgressChanged?.Invoke(this, babyDetailsProgress);

            var pendingBabyDetails = await GetPendingBabyDetailsAsync();
            babyDetailsProgress.TotalRecords = pendingBabyDetails.Count;

            foreach (var item in pendingBabyDetails)
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

            if (pendingBabyDetails.Any())
            {
                var pushRequest = new SyncPushRequest<BabyDetails>
                {
                    DeviceId = deviceId,
                    Changes = pendingBabyDetails
                };

                var pushResponse = await _apiClient.PushBabyDetailsAsync(pushRequest);

                result.TotalPushed += pushResponse.SuccessIds.Count;
                result.TotalConflicts += pushResponse.Conflicts.Count;
                result.TotalErrors += pushResponse.Errors.Count;

                await MarkRecordsAsSyncedAsync("Tbl_Baby", pushResponse.SuccessIds);

                babyDetailsProgress.SuccessCount = pushResponse.SuccessIds.Count;
                babyDetailsProgress.ConflictCount = pushResponse.Conflicts.Count;
                babyDetailsProgress.ErrorCount = pushResponse.Errors.Count;
                babyDetailsProgress.ProcessedRecords = pendingBabyDetails.Count;
                ProgressChanged?.Invoke(this, babyDetailsProgress);
            }

            // Push referrals
            var referralProgress = new SyncProgress { TableName = "Tbl_Referral", CurrentOperation = "Pushing referrals" };
            ProgressChanged?.Invoke(this, referralProgress);

            var pendingReferrals = await GetPendingReferralsAsync();
            referralProgress.TotalRecords = pendingReferrals.Count;

            foreach (var item in pendingReferrals)
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

            if (pendingReferrals.Any())
            {
                var pushRequest = new SyncPushRequest<Referral>
                {
                    DeviceId = deviceId,
                    Changes = pendingReferrals
                };

                var pushResponse = await _apiClient.PushReferralsAsync(pushRequest);

                result.TotalPushed += pushResponse.SuccessIds.Count;
                result.TotalConflicts += pushResponse.Conflicts.Count;
                result.TotalErrors += pushResponse.Errors.Count;

                await MarkRecordsAsSyncedAsync("Tbl_Referral", pushResponse.SuccessIds);

                referralProgress.SuccessCount = pushResponse.SuccessIds.Count;
                referralProgress.ConflictCount = pushResponse.Conflicts.Count;
                referralProgress.ErrorCount = pushResponse.Errors.Count;
                referralProgress.ProcessedRecords = pendingReferrals.Count;
                ProgressChanged?.Invoke(this, referralProgress);
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

            // Pull staff with pagination
            var staffProgress = new SyncProgress { TableName = "Tbl_Staff", CurrentOperation = "Pulling staff" };
            ProgressChanged?.Invoke(this, staffProgress);

            var staffPulled = await PullStaffWithPaginationAsync(
                deviceId,
                lastPullTimestamp,
                "Tbl_Staff",
                staffProgress,
                cancellationToken);

            result.TotalPulled += staffPulled.recordCount;
            if (staffPulled.serverTimestamp > latestServerTimestamp)
            {
                latestServerTimestamp = staffPulled.serverTimestamp;
            }

            // Pull birth outcomes with pagination
            var birthOutcomeProgress = new SyncProgress { TableName = "Tbl_BirthOutcome", CurrentOperation = "Pulling birth outcomes" };
            ProgressChanged?.Invoke(this, birthOutcomeProgress);

            var birthOutcomesPulled = await PullWithPaginationAsync<BirthOutcome>(
                deviceId,
                lastPullTimestamp,
                "Tbl_BirthOutcome",
                async (request) => await _apiClient.PullBirthOutcomesAsync(request),
                async (records) => await MergeBirthOutcomes(records),
                birthOutcomeProgress,
                cancellationToken);

            result.TotalPulled += birthOutcomesPulled.recordCount;
            if (birthOutcomesPulled.serverTimestamp > latestServerTimestamp)
            {
                latestServerTimestamp = birthOutcomesPulled.serverTimestamp;
            }

            // Pull baby details with pagination
            var babyDetailsProgress = new SyncProgress { TableName = "Tbl_Baby", CurrentOperation = "Pulling baby details" };
            ProgressChanged?.Invoke(this, babyDetailsProgress);

            var babyDetailsPulled = await PullWithPaginationAsync<BabyDetails>(
                deviceId,
                lastPullTimestamp,
                "Tbl_Baby",
                async (request) => await _apiClient.PullBabyDetailsAsync(request),
                async (records) => await MergeBabyDetails(records),
                babyDetailsProgress,
                cancellationToken);

            result.TotalPulled += babyDetailsPulled.recordCount;
            if (babyDetailsPulled.serverTimestamp > latestServerTimestamp)
            {
                latestServerTimestamp = babyDetailsPulled.serverTimestamp;
            }

            // Pull referrals with pagination
            var referralProgress = new SyncProgress { TableName = "Tbl_Referral", CurrentOperation = "Pulling referrals" };
            ProgressChanged?.Invoke(this, referralProgress);

            var referralsPulled = await PullWithPaginationAsync<Referral>(
                deviceId,
                lastPullTimestamp,
                "Tbl_Referral",
                async (request) => await _apiClient.PullReferralsAsync(request),
                async (records) => await MergeReferrals(records),
                referralProgress,
                cancellationToken);

            result.TotalPulled += referralsPulled.recordCount;
            if (referralsPulled.serverTimestamp > latestServerTimestamp)
            {
                latestServerTimestamp = referralsPulled.serverTimestamp;
            }

            // Pull facilities with pagination (reference data - pull only)
            var facilityProgress = new SyncProgress { TableName = "Tbl_Facility", CurrentOperation = "Pulling facilities" };
            ProgressChanged?.Invoke(this, facilityProgress);

            var facilitiesPulled = await PullWithPaginationAsync<Facility>(
                deviceId,
                lastPullTimestamp,
                "Tbl_Facility",
                async (request) => await _apiClient.PullFacilitiesAsync(request),
                async (records) => await MergeFacilities(records),
                facilityProgress,
                cancellationToken);

            result.TotalPulled += facilitiesPulled.recordCount;
            if (facilitiesPulled.serverTimestamp > latestServerTimestamp)
            {
                latestServerTimestamp = facilitiesPulled.serverTimestamp;
            }

            // Pull regions with pagination (reference data - pull only)
            var regionProgress = new SyncProgress { TableName = "Tbl_Region", CurrentOperation = "Pulling regions" };
            ProgressChanged?.Invoke(this, regionProgress);

            var regionsPulled = await PullWithPaginationAsync<Region>(
                deviceId,
                lastPullTimestamp,
                "Tbl_Region",
                async (request) => await _apiClient.PullRegionsAsync(request),
                async (records) => await MergeRegions(records),
                regionProgress,
                cancellationToken);

            result.TotalPulled += regionsPulled.recordCount;
            if (regionsPulled.serverTimestamp > latestServerTimestamp)
            {
                latestServerTimestamp = regionsPulled.serverTimestamp;
            }

            // Pull districts with pagination (reference data - pull only)
            var districtProgress = new SyncProgress { TableName = "Tbl_District", CurrentOperation = "Pulling districts" };
            ProgressChanged?.Invoke(this, districtProgress);

            var districtsPulled = await PullWithPaginationAsync<District>(
                deviceId,
                lastPullTimestamp,
                "Tbl_District",
                async (request) => await _apiClient.PullDistrictsAsync(request),
                async (records) => await MergeDistricts(records),
                districtProgress,
                cancellationToken);

            result.TotalPulled += districtsPulled.recordCount;
            if (districtsPulled.serverTimestamp > latestServerTimestamp)
            {
                latestServerTimestamp = districtsPulled.serverTimestamp;
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
                TableName = tableName,
                FacilityID = Constants.Staff?.Facility
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

    /// <summary>
    /// Pulls staff data from server with pagination support
    /// </summary>
    private async Task<(int recordCount, long serverTimestamp)> PullStaffWithPaginationAsync(
        string deviceId,
        long lastSyncTimestamp,
        string tableName,
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
                TableName = tableName,
                FacilityID = Constants.Staff?.Facility
            };

            var pullResponse = await _apiClient.PullStaffAsync(pullRequest);

            if (pullResponse.Records.Any())
            {
                await MergeStaff(pullResponse.Records);
                totalPulled += pullResponse.Records.Count;

                progress.TotalRecords += pullResponse.Records.Count;
                progress.ProcessedRecords = totalPulled;
                progress.SuccessCount = totalPulled;
                ProgressChanged?.Invoke(this, progress);
            }

            if (pullResponse.ServerTimestamp > latestServerTimestamp)
            {
                latestServerTimestamp = pullResponse.ServerTimestamp;
            }

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

            // Count pending staff
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Tbl_Staff WHERE SyncStatus = 0";
                var result = await command.ExecuteScalarAsync();
                count += Convert.ToInt32(result);
            }

            // Count pending birth outcomes
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Tbl_BirthOutcome WHERE SyncStatus = 0";
                var result = await command.ExecuteScalarAsync();
                count += Convert.ToInt32(result);
            }

            // Count pending baby details
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Tbl_Baby WHERE SyncStatus = 0";
                var result = await command.ExecuteScalarAsync();
                count += Convert.ToInt32(result);
            }

            // Count pending referrals
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Tbl_Referral WHERE SyncStatus = 0";
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

            // Count conflicted staff
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Tbl_Staff WHERE SyncStatus = 2";
                var result = await command.ExecuteScalarAsync();
                count += Convert.ToInt32(result);
            }

            // Count conflicted birth outcomes
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Tbl_BirthOutcome WHERE SyncStatus = 2";
                var result = await command.ExecuteScalarAsync();
                count += Convert.ToInt32(result);
            }

            // Count conflicted baby details
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Tbl_Baby WHERE SyncStatus = 2";
                var result = await command.ExecuteScalarAsync();
                count += Convert.ToInt32(result);
            }

            // Count conflicted referrals
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Tbl_Referral WHERE SyncStatus = 2";
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
                WHERE ID = @id;
                UPDATE Tbl_Partograph
                SET SyncStatus = 0, ConflictData = NULL
                WHERE ID = @id;
                UPDATE Tbl_Staff
                SET SyncStatus = 0, ConflictData = NULL
                WHERE ID = @id;
                UPDATE Tbl_BirthOutcome
                SET SyncStatus = 0, ConflictData = NULL
                WHERE ID = @id;
                UPDATE Tbl_Baby
                SET SyncStatus = 0, ConflictData = NULL
                WHERE ID = @id;
                UPDATE Tbl_Referral
                SET SyncStatus = 0, ConflictData = NULL
                WHERE ID = @id;";
        }
        else
        {
            // Accept server version (implementation depends on how ConflictData is stored)
            command.CommandText = @"
                UPDATE Tbl_Patient
                SET SyncStatus = 1, ConflictData = NULL
                WHERE ID = @id;
                UPDATE Tbl_Partograph
                SET SyncStatus = 1, ConflictData = NULL
                WHERE ID = @id;
                UPDATE Tbl_Staff
                SET SyncStatus = 1, ConflictData = NULL
                WHERE ID = @id;
                UPDATE Tbl_BirthOutcome
                SET SyncStatus = 1, ConflictData = NULL
                WHERE ID = @id;
                UPDATE Tbl_Baby
                SET SyncStatus = 1, ConflictData = NULL
                WHERE ID = @id;
                UPDATE Tbl_Referral
                SET SyncStatus = 1, ConflictData = NULL
                WHERE ID = @id;";
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
            command.CommandText = @"
                SELECT
                    P.ID, P.patientid, P.time, P.gravida, P.parity, P.abortion,
                    P.admissiondate, P.expecteddeliverydate, P.lastmenstrualdate,
                    P.status, P.currentphase, P.laborstarttime, P.secondstagestarttime,
                    P.thirdstagestarttime, P.fourthstagestarttime, P.completedtime,
                    P.rupturedmembranetime, P.deliverytime, P.cervicaldilationonadmission,
                    P.membranestatus, P.liquorstatus, P.riskscore, P.risklevel, P.riskcolor,
                    P.complications, P.handlername, P.handler,
                    P.createdtime, P.updatedtime, P.deletedtime,
                    P.deviceid, P.origindeviceid, P.syncstatus, P.version, P.serverversion,
                    P.deleted, P.conflictdata, P.datahash,
                    PA.ID as patient_id, PA.firstname as patient_firstname, PA.lastname as patient_lastname,
                    PA.hospitalnumber as patient_hospitalnumber, PA.dateofbirth as patient_dateofbirth,
                    PA.age as patient_age, PA.bloodgroup as patient_bloodgroup,
                    PA.phonenumber as patient_phonenumber, PA.address as patient_address,
                    PA.emergencycontactname as patient_emergencycontactname,
                    PA.emergencycontactphone as patient_emergencycontactphone,
                    PA.emergencycontactrelationship as patient_emergencycontactrelationship,
                    PA.weight as patient_weight, PA.height as patient_height,
                    PA.haspreviouscsection as patient_haspreviouscsection,
                    PA.numberofpreviouscsections as patient_numberofpreviouscsections,
                    PA.livebirths as patient_livebirths, PA.stillbirths as patient_stillbirths,
                    PA.neonataldeaths as patient_neonataldeaths,
                    PA.handlername as patient_handlername, PA.handler as patient_handler,
                    PA.createdtime as patient_createdtime, PA.updatedtime as patient_updatedtime,
                    PA.deletedtime as patient_deletedtime,
                    PA.deviceid as patient_deviceid, PA.origindeviceid as patient_origindeviceid,
                    PA.syncstatus as patient_syncstatus, PA.version as patient_version,
                    PA.serverversion as patient_serverversion, PA.deleted as patient_deleted,
                    PA.conflictdata as patient_conflictdata, PA.datahash as patient_datahash
                FROM Tbl_Partograph P
                INNER JOIN Tbl_Patient PA ON P.patientID = PA.ID
                WHERE P.SyncStatus = 0";

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

    private async Task StoreStaffConflictsAsync(List<ConflictRecord<Staff>> conflicts)
    {
        if (!conflicts.Any()) return;

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            foreach (var conflict in conflicts)
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = @"
                    UPDATE Tbl_Staff
                    SET SyncStatus = 2,
                        ConflictData = @conflictData
                    WHERE ID = @id";

                command.Parameters.AddWithValue("@id", conflict.Id);
                command.Parameters.AddWithValue("@conflictData", System.Text.Json.JsonSerializer.Serialize(conflict.ServerRecord));

                await command.ExecuteNonQueryAsync();
            }

            transaction.Commit();
            _logger.LogDebug("Stored {Count} staff conflicts", conflicts.Count);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Failed to store staff conflicts, rolling back");
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

    private async Task<List<Staff>> GetPendingStaffAsync()
    {
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var staff = new List<Staff>();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Tbl_Staff WHERE SyncStatus = 0";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                staff.Add(MapStaffFromReader(reader));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving Staff table");
            throw;
        }

        return staff;
    }

    private async Task MergeStaff(List<Staff> staffList)
    {
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        foreach (var staff in staffList)
        {
            try
            {
                using var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM Tbl_Staff WHERE ID = @id";
                checkCmd.Parameters.AddWithValue("@id", staff.ID.ToString());
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (count > 0)
                {
                    using var updateCmd = connection.CreateCommand();
                    updateCmd.CommandText = @"
                        UPDATE Tbl_Staff
                        SET name = @name, staffid = @staffid, email = @email, role = @role,
                            department = @department, active = @active, facility = @facility,
                            updatedtime = @updatedtime, syncstatus = 1, serverversion = @serverversion
                        WHERE ID = @id";
                    updateCmd.Parameters.AddWithValue("@id", staff.ID.ToString());
                    updateCmd.Parameters.AddWithValue("@name", staff.Name ?? string.Empty);
                    updateCmd.Parameters.AddWithValue("@staffid", staff.StaffID ?? string.Empty);
                    updateCmd.Parameters.AddWithValue("@email", staff.Email ?? string.Empty);
                    updateCmd.Parameters.AddWithValue("@role", staff.Role ?? string.Empty);
                    updateCmd.Parameters.AddWithValue("@department", staff.Department ?? string.Empty);
                    updateCmd.Parameters.AddWithValue("@active", staff.IsActive ? 1 : 0);
                    updateCmd.Parameters.AddWithValue("@facility", staff.Facility?.ToString() ?? string.Empty);
                    updateCmd.Parameters.AddWithValue("@updatedtime", staff.UpdatedTime);
                    updateCmd.Parameters.AddWithValue("@serverversion", staff.ServerVersion);
                    await updateCmd.ExecuteNonQueryAsync();
                }
                else
                {
                    using var insertCmd = connection.CreateCommand();
                    insertCmd.CommandText = @"
                        INSERT INTO Tbl_Staff (ID, name, staffid, email, role, department, password, active, facility,
                            createdtime, updatedtime, deviceid, origindeviceid, syncstatus, version, serverversion, deleted)
                        VALUES (@id, @name, @staffid, @email, @role, @department, @password, @active, @facility,
                            @createdtime, @updatedtime, @deviceid, @origindeviceid, 1, @version, @serverversion, @deleted)";
                    insertCmd.Parameters.AddWithValue("@id", staff.ID.ToString());
                    insertCmd.Parameters.AddWithValue("@name", staff.Name ?? string.Empty);
                    insertCmd.Parameters.AddWithValue("@staffid", staff.StaffID ?? string.Empty);
                    insertCmd.Parameters.AddWithValue("@email", staff.Email ?? string.Empty);
                    insertCmd.Parameters.AddWithValue("@role", staff.Role ?? string.Empty);
                    insertCmd.Parameters.AddWithValue("@department", staff.Department ?? string.Empty);
                    insertCmd.Parameters.AddWithValue("@password", staff.Password ?? string.Empty);
                    insertCmd.Parameters.AddWithValue("@active", staff.IsActive ? 1 : 0);
                    insertCmd.Parameters.AddWithValue("@facility", staff.Facility?.ToString() ?? string.Empty);
                    insertCmd.Parameters.AddWithValue("@createdtime", staff.CreatedTime);
                    insertCmd.Parameters.AddWithValue("@updatedtime", staff.UpdatedTime);
                    insertCmd.Parameters.AddWithValue("@deviceid", staff.DeviceId ?? string.Empty);
                    insertCmd.Parameters.AddWithValue("@origindeviceid", staff.OriginDeviceId ?? string.Empty);
                    insertCmd.Parameters.AddWithValue("@version", staff.Version);
                    insertCmd.Parameters.AddWithValue("@serverversion", staff.ServerVersion);
                    insertCmd.Parameters.AddWithValue("@deleted", staff.Deleted);
                    await insertCmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error merging staff {StaffId}", staff.ID);
            }
        }
    }

    private Staff MapStaffFromReader(SqliteDataReader reader)
    {
        return new Staff
        {
            ID = Guid.Parse(reader["ID"].ToString()!),
            Name = reader["name"]?.ToString() ?? string.Empty,
            StaffID = reader["staffid"]?.ToString() ?? string.Empty,
            Email = reader["email"]?.ToString() ?? string.Empty,
            Role = reader["role"]?.ToString() ?? string.Empty,
            Department = reader["department"]?.ToString() ?? string.Empty,
            Password = reader["password"]?.ToString() ?? string.Empty,
            IsActive = Convert.ToInt32(reader["active"]) == 1,
            Facility = reader["facility"] is DBNull ? null : Guid.Parse(reader["facility"].ToString()!),
            CreatedTime = Convert.ToInt64(reader["createdtime"]),
            UpdatedTime = Convert.ToInt64(reader["updatedtime"]),
            DeviceId = reader["deviceid"]?.ToString() ?? string.Empty,
            OriginDeviceId = reader["origindeviceid"]?.ToString() ?? string.Empty,
            SyncStatus = Convert.ToInt32(reader["syncstatus"]),
            Version = Convert.ToInt32(reader["version"]),
            ServerVersion = reader["serverversion"] is DBNull ? 0 : Convert.ToInt32(reader["serverversion"]),
            Deleted = reader["deleted"] is DBNull ? 0 : Convert.ToInt32(reader["deleted"]),
        };
    }

    private Patient MapPatientFromReader(SqliteDataReader reader)
    {
        return new Patient
        {
            ID = Guid.Parse(reader["ID"].ToString()!),
            FirstName = reader["firstname"]?.ToString() ?? string.Empty,
            LastName = reader["lastname"]?.ToString() ?? string.Empty,
            HospitalNumber = reader["hospitalnumber"]?.ToString() ?? string.Empty,
            DateOfBirth = reader["dateofbirth"] is DBNull ? null : DateOnly.Parse(reader["dateofbirth"].ToString()!),
            Age = reader["age"] is DBNull ? null : Convert.ToInt32(reader["age"]),
            BloodGroup = reader["bloodgroup"]?.ToString() ?? string.Empty,
            PhoneNumber = reader["phonenumber"]?.ToString() ?? string.Empty,
            Address = reader["address"]?.ToString() ?? string.Empty,
            EmergencyContactName = reader["emergencycontactname"]?.ToString() ?? string.Empty,
            EmergencyContactPhone = reader["emergencycontactphone"]?.ToString() ?? string.Empty,
            EmergencyContactRelationship = reader["emergencycontactrelationship"]?.ToString() ?? string.Empty,
            Weight = reader["weight"] is DBNull ? null : Convert.ToDouble(reader["weight"]),
            Height = reader["height"] is DBNull ? null : Convert.ToDouble(reader["height"]),
            HasPreviousCSection = reader["haspreviouscsection"] is DBNull ? false : Convert.ToBoolean(reader["haspreviouscsection"]),
            NumberOfPreviousCsections = reader["numberofpreviouscsections"] is DBNull ? null : Convert.ToInt32(reader["numberofpreviouscsections"]),
            LiveBirths = reader["livebirths"] is DBNull ? null : Convert.ToInt32(reader["livebirths"]),
            Stillbirths = reader["stillbirths"] is DBNull ? null : Convert.ToInt32(reader["stillbirths"]),
            NeonatalDeaths = reader["neonataldeaths"] is DBNull ? null : Convert.ToInt32(reader["neonataldeaths"]),
            HandlerName = reader["handlername"]?.ToString() ?? string.Empty,
            Handler = reader["handler"] is DBNull ? null : Guid.Parse(reader["handler"].ToString()!),
            CreatedTime = Convert.ToInt64(reader["createdtime"]),
            UpdatedTime = Convert.ToInt64(reader["updatedtime"]),
            DeletedTime = reader["deletedtime"] is DBNull ? null : Convert.ToInt64(reader["deletedtime"]),
            DeviceId = reader["deviceid"]?.ToString() ?? string.Empty,
            OriginDeviceId = reader["origindeviceid"]?.ToString() ?? string.Empty,
            SyncStatus = Convert.ToInt32(reader["syncstatus"]),
            Version = Convert.ToInt32(reader["version"]),
            ServerVersion = reader["serverversion"] is DBNull ? 0 : Convert.ToInt32(reader["serverversion"]),
            Deleted = reader["deleted"] is DBNull ? 0 : Convert.ToInt32(reader["deleted"]),
            ConflictData = reader["conflictdata"]?.ToString() ?? string.Empty,
            DataHash = reader["datahash"]?.ToString() ?? string.Empty
        };
    }

    private Partograph MapPartographFromReader(SqliteDataReader reader)
    {
        return new Partograph
        {
            ID = Guid.Parse(reader["ID"].ToString()!),
            PatientID = Guid.Parse(reader["patientid"].ToString()!),
            Time = reader["time"] is DBNull ? DateTime.Now : DateTime.Parse(reader["time"].ToString()!),
            Gravida = reader["gravida"] is DBNull ? 0 : Convert.ToInt32(reader["gravida"]),
            Parity = reader["parity"] is DBNull ? 0 : Convert.ToInt32(reader["parity"]),
            Abortion = reader["abortion"] is DBNull ? 0 : Convert.ToInt32(reader["abortion"]),
            AdmissionDate = reader["admissiondate"] is DBNull ? DateTime.Now : DateTime.Parse(reader["admissiondate"].ToString()!),
            ExpectedDeliveryDate = reader["expecteddeliverydate"] is DBNull ? null : DateOnly.Parse(reader["expecteddeliverydate"].ToString()!),
            LastMenstrualDate = reader["lastmenstrualdate"] is DBNull ? null : DateOnly.Parse(reader["lastmenstrualdate"].ToString()!),
            Status = reader["status"] is DBNull ? LaborStatus.Pending : (LaborStatus)Convert.ToInt32(reader["status"]),
            CurrentPhase = reader["currentphase"] is DBNull ? FirstStagePhase.NotDetermined : (FirstStagePhase)Convert.ToInt32(reader["currentphase"]),
            LaborStartTime = reader["laborstarttime"] is DBNull ? null : DateTime.Parse(reader["laborstarttime"].ToString()!),
            SecondStageStartTime = reader["secondstagestarttime"] is DBNull ? null : DateTime.Parse(reader["secondstagestarttime"].ToString()!),
            ThirdStageStartTime = reader["thirdstagestarttime"] is DBNull ? null : DateTime.Parse(reader["thirdstagestarttime"].ToString()!),
            FourthStageStartTime = reader["fourthstagestarttime"] is DBNull ? null : DateTime.Parse(reader["fourthstagestarttime"].ToString()!),
            CompletedTime = reader["completedtime"] is DBNull ? null : DateTime.Parse(reader["completedtime"].ToString()!),
            RupturedMembraneTime = reader["rupturedmembranetime"] is DBNull ? null : DateTime.Parse(reader["rupturedmembranetime"].ToString()!),
            DeliveryTime = reader["deliverytime"] is DBNull ? null : DateTime.Parse(reader["deliverytime"].ToString()!),
            CervicalDilationOnAdmission = reader["cervicaldilationonadmission"] is DBNull ? null : Convert.ToInt32(reader["cervicaldilationonadmission"]),
            MembraneStatus = reader["membranestatus"]?.ToString() ?? "Intact",
            LiquorStatus = reader["liquorstatus"]?.ToString() ?? "Clear",
            RiskScore = reader["riskscore"] is DBNull ? 0 : Convert.ToInt32(reader["riskscore"]),
            RiskLevel = reader["risklevel"]?.ToString() ?? "Low Risk",
            RiskColor = reader["riskcolor"]?.ToString() ?? "#4CAF50",
            Complications = reader["complications"]?.ToString() ?? string.Empty,
            HandlerName = reader["handlername"]?.ToString() ?? string.Empty,
            Handler = reader["handler"] is DBNull ? null : Guid.Parse(reader["handler"].ToString()!),
            CreatedTime = Convert.ToInt64(reader["createdtime"]),
            UpdatedTime = Convert.ToInt64(reader["updatedtime"]),
            DeletedTime = reader["deletedtime"] is DBNull ? null : Convert.ToInt64(reader["deletedtime"]),
            DeviceId = reader["deviceid"]?.ToString() ?? string.Empty,
            OriginDeviceId = reader["origindeviceid"]?.ToString() ?? string.Empty,
            SyncStatus = Convert.ToInt32(reader["syncstatus"]),
            Version = Convert.ToInt32(reader["version"]),
            ServerVersion = reader["serverversion"] is DBNull ? 0 : Convert.ToInt32(reader["serverversion"]),
            Deleted = reader["deleted"] is DBNull ? 0 : Convert.ToInt32(reader["deleted"]),
            ConflictData = reader["conflictdata"]?.ToString() ?? string.Empty,
            DataHash = reader["datahash"]?.ToString() ?? string.Empty,
            Patient = new Patient()
            {
                ID = Guid.Parse(reader["patient_id"].ToString()!),
                FirstName = reader["patient_firstname"]?.ToString() ?? string.Empty,
                LastName = reader["patient_lastname"]?.ToString() ?? string.Empty,
                HospitalNumber = reader["patient_hospitalnumber"]?.ToString() ?? string.Empty,
                DateOfBirth = reader["patient_dateofbirth"] is DBNull ? null : DateOnly.Parse(reader["patient_dateofbirth"].ToString()!),
                Age = reader["patient_age"] is DBNull ? null : Convert.ToInt32(reader["patient_age"]),
                BloodGroup = reader["patient_bloodgroup"]?.ToString() ?? string.Empty,
                PhoneNumber = reader["patient_phonenumber"]?.ToString() ?? string.Empty,
                Address = reader["patient_address"]?.ToString() ?? string.Empty,
                EmergencyContactName = reader["patient_emergencycontactname"]?.ToString() ?? string.Empty,
                EmergencyContactPhone = reader["patient_emergencycontactphone"]?.ToString() ?? string.Empty,
                EmergencyContactRelationship = reader["patient_emergencycontactrelationship"]?.ToString() ?? string.Empty,
                Weight = reader["patient_weight"] is DBNull ? null : Convert.ToDouble(reader["patient_weight"]),
                Height = reader["patient_height"] is DBNull ? null : Convert.ToDouble(reader["patient_height"]),
                HasPreviousCSection = reader["patient_haspreviouscsection"] is DBNull ? false : Convert.ToBoolean(reader["patient_haspreviouscsection"]),
                NumberOfPreviousCsections = reader["patient_numberofpreviouscsections"] is DBNull ? null : Convert.ToInt32(reader["patient_numberofpreviouscsections"]),
                LiveBirths = reader["patient_livebirths"] is DBNull ? null : Convert.ToInt32(reader["patient_livebirths"]),
                Stillbirths = reader["patient_stillbirths"] is DBNull ? null : Convert.ToInt32(reader["patient_stillbirths"]),
                NeonatalDeaths = reader["patient_neonataldeaths"] is DBNull ? null : Convert.ToInt32(reader["patient_neonataldeaths"]),
                HandlerName = reader["patient_handlername"]?.ToString() ?? string.Empty,
                Handler = reader["patient_handler"] is DBNull ? null : Guid.Parse(reader["patient_handler"].ToString()!),
                CreatedTime = Convert.ToInt64(reader["patient_createdtime"]),
                UpdatedTime = Convert.ToInt64(reader["patient_updatedtime"]),
                DeletedTime = reader["patient_deletedtime"] is DBNull ? null : Convert.ToInt64(reader["patient_deletedtime"]),
                DeviceId = reader["patient_deviceid"]?.ToString() ?? string.Empty,
                OriginDeviceId = reader["patient_origindeviceid"]?.ToString() ?? string.Empty,
                SyncStatus = Convert.ToInt32(reader["patient_syncstatus"]),
                Version = Convert.ToInt32(reader["patient_version"]),
                ServerVersion = reader["patient_serverversion"] is DBNull ? 0 : Convert.ToInt32(reader["patient_serverversion"]),
                Deleted = reader["patient_deleted"] is DBNull ? 0 : Convert.ToInt32(reader["patient_deleted"]),
                ConflictData = reader["patient_conflictdata"]?.ToString() ?? string.Empty,
                DataHash = reader["patient_datahash"]?.ToString() ?? string.Empty
            }
        };
    }

    private async Task<List<BirthOutcome>> GetPendingBirthOutcomesAsync()
    {
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var items = new List<BirthOutcome>();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Tbl_BirthOutcome WHERE SyncStatus = 0 AND deleted = 0";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new BirthOutcome
                {
                    ID = Guid.Parse(reader["ID"].ToString()!),
                    PartographID = Guid.Parse(reader["partographid"].ToString()!),
                    RecordedTime = DateTime.Parse(reader["recordedtime"].ToString()!),
                    MaternalStatus = (MaternalOutcomeStatus)Convert.ToInt32(reader["maternalstatus"]),
                    DeliveryMode = (DeliveryMode)Convert.ToInt32(reader["deliverymode"]),
                    NumberOfBabies = Convert.ToInt32(reader["numberofbabies"]),
                    PerinealStatus = (PerinealStatus)Convert.ToInt32(reader["perinealstatus"]),
                    PlacentaComplete = Convert.ToBoolean(reader["placentacomplete"]),
                    EstimatedBloodLoss = Convert.ToInt32(reader["estimatedbloodloss"]),
                    PostpartumHemorrhage = Convert.ToBoolean(reader["postpartumhemorrhage"]),
                    Eclampsia = Convert.ToBoolean(reader["eclampsia"]),
                    SepticShock = Convert.ToBoolean(reader["septicshock"]),
                    ObstructedLabor = Convert.ToBoolean(reader["obstructedlabor"]),
                    RupturedUterus = Convert.ToBoolean(reader["ruptureduterus"]),
                    OxytocinGiven = Convert.ToBoolean(reader["oxytocingiven"]),
                    AntibioticsGiven = Convert.ToBoolean(reader["antibioticsgiven"]),
                    BloodTransfusionGiven = Convert.ToBoolean(reader["bloodtransfusiongiven"]),
                    Notes = reader["notes"]?.ToString() ?? string.Empty,
                    CreatedTime = Convert.ToInt64(reader["createdtime"]),
                    UpdatedTime = Convert.ToInt64(reader["updatedtime"]),
                    DeviceId = reader["deviceid"]?.ToString() ?? string.Empty,
                    OriginDeviceId = reader["origindeviceid"]?.ToString() ?? string.Empty,
                    SyncStatus = Convert.ToInt32(reader["syncstatus"]),
                    Version = Convert.ToInt32(reader["version"]),
                    ServerVersion = Convert.ToInt32(reader["serverversion"]),
                    Deleted = Convert.ToInt32(reader["deleted"]),
                    DataHash = reader["datahash"]?.ToString()
                });
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving BirthOutcome table");
            throw;
        }

        return items;
    }

    private async Task<List<BabyDetails>> GetPendingBabyDetailsAsync()
    {
        var items = await _babyDetailsRepository.ListAsync();
        return items.Where(x => x.SyncStatus == 0).ToList();
    }

    private async Task<List<Referral>> GetPendingReferralsAsync()
    {
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var items = new List<Referral>();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Tbl_Referral WHERE SyncStatus = 0 AND deleted = 0";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new Referral
                {
                    ID = Guid.Parse(reader["ID"].ToString()!),
                    PartographID = Guid.Parse(reader["partographid"].ToString()!),
                    ReferralTime = DateTime.Parse(reader["referraltime"].ToString()!),
                    ReferralType = (ReferralType)Convert.ToInt32(reader["referraltype"]),
                    Urgency = (ReferralUrgency)Convert.ToInt32(reader["urgency"]),
                    Status = (ReferralStatus)Convert.ToInt32(reader["status"]),
                    TransportMode = (TransportMode)Convert.ToInt32(reader["transportmode"]),
                    ReferringFacilityName = reader["referringfacilityname"]?.ToString() ?? string.Empty,
                    DestinationFacilityName = reader["destinationfacilityname"]?.ToString() ?? string.Empty,
                    PrimaryDiagnosis = reader["primarydiagnosis"]?.ToString() ?? string.Empty,
                    ClinicalSummary = reader["clinicalsummary"]?.ToString() ?? string.Empty,
                    Notes = reader["notes"]?.ToString() ?? string.Empty,
                    CreatedTime = Convert.ToInt64(reader["createdtime"]),
                    UpdatedTime = Convert.ToInt64(reader["updatedtime"]),
                    DeviceId = reader["deviceid"]?.ToString() ?? string.Empty,
                    OriginDeviceId = reader["origindeviceid"]?.ToString() ?? string.Empty,
                    SyncStatus = Convert.ToInt32(reader["syncstatus"]),
                    Version = Convert.ToInt32(reader["version"]),
                    ServerVersion = Convert.ToInt32(reader["serverversion"]),
                    Deleted = Convert.ToInt32(reader["deleted"]),
                    DataHash = reader["datahash"]?.ToString()
                });
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving Referral table");
            throw;
        }

        return items;
    }

    private async Task MergeBirthOutcomes(List<BirthOutcome> items)
    {
        foreach (var item in items)
        {
            await _birthOutcomeRepository.SaveItemAsync(item);
        }
    }

    private async Task MergeBabyDetails(List<BabyDetails> items)
    {
        foreach (var item in items)
        {
            await _babyDetailsRepository.SaveItemAsync(item);
        }
    }

    private async Task MergeReferrals(List<Referral> items)
    {
        foreach (var item in items)
        {
            await _referralRepository.SaveItemAsync(item);
        }
    }

    private async Task MergeFacilities(List<Facility> items)
    {
        foreach (var item in items)
        {
            await _facilityRepository.AddAsync(item);
        }
    }

    private async Task MergeRegions(List<Region> items)
    {
        foreach (var item in items)
        {
            await _regionRepository.AddOrUpdateAsync(item);
        }
    }

    private async Task MergeDistricts(List<District> items)
    {
        foreach (var item in items)
        {
            await _districtRepository.AddOrUpdateAsync(item);
        }
    }
}
