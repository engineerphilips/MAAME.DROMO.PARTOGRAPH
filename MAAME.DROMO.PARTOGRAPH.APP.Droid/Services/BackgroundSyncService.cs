using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;

/// <summary>
/// Service that manages background synchronization
/// </summary>
public class BackgroundSyncService : IDisposable
{
    private readonly ISyncService _syncService;
    private readonly IConnectivityService _connectivityService;
    private readonly ILogger<BackgroundSyncService> _logger;
    private Timer? _syncTimer;
    private bool _isEnabled;
    private int _syncIntervalMinutes = 15; // Default 15 minutes
    private bool _disposed;

    public BackgroundSyncService(
        ISyncService syncService,
        IConnectivityService connectivityService,
        ILogger<BackgroundSyncService> logger)
    {
        _syncService = syncService;
        _connectivityService = connectivityService;
        _logger = logger;

        // Load settings
        _isEnabled = Preferences.Get("BackgroundSyncEnabled", true);
        _syncIntervalMinutes = Preferences.Get("SyncIntervalMinutes", 15);

        // Subscribe to connectivity changes
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
    }

    /// <summary>
    /// Gets or sets whether background sync is enabled
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                Preferences.Set("BackgroundSyncEnabled", value);

                if (value)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the sync interval in minutes
    /// </summary>
    public int SyncIntervalMinutes
    {
        get => _syncIntervalMinutes;
        set
        {
            if (_syncIntervalMinutes != value && value > 0)
            {
                _syncIntervalMinutes = value;
                Preferences.Set("SyncIntervalMinutes", value);

                // Restart timer with new interval
                if (_isEnabled)
                {
                    Stop();
                    Start();
                }
            }
        }
    }

    /// <summary>
    /// Gets whether the sync service is currently running
    /// </summary>
    public bool IsRunning => _syncTimer != null;

    /// <summary>
    /// Gets the last sync result
    /// </summary>
    public SyncResult? LastSyncResult { get; private set; }

    /// <summary>
    /// Starts the background sync service
    /// </summary>
    public void Start()
    {
        if (!_isEnabled)
        {
            _logger.LogInformation("Background sync is disabled");
            return;
        }

        Stop(); // Stop any existing timer

        _logger.LogInformation("Starting background sync with interval of {Interval} minutes", _syncIntervalMinutes);

        var interval = TimeSpan.FromMinutes(_syncIntervalMinutes);
        _syncTimer = new Timer(OnSyncTimerElapsed, null, interval, interval);
    }

    /// <summary>
    /// Starts the background sync service and triggers an immediate sync
    /// </summary>
    public async Task StartWithImmediateSyncAsync()
    {
        Start();

        if (_connectivityService.IsConnected)
        {
            _logger.LogInformation("Starting immediate sync on app startup");
            await PerformFullSyncAsync();
        }
    }

    /// <summary>
    /// Stops the background sync service
    /// </summary>
    public void Stop()
    {
        if (_syncTimer != null)
        {
            _logger.LogInformation("Stopping background sync");
            _syncTimer.Dispose();
            _syncTimer = null;
        }
    }

    /// <summary>
    /// Manually triggers a sync operation
    /// </summary>
    public async Task TriggerSyncAsync()
    {
        await PerformSyncAsync();
    }

    private async void OnSyncTimerElapsed(object? state)
    {
        await PerformSyncAsync();
    }

    private async void OnConnectivityChanged(object? sender, bool isConnected)
    {
        if (isConnected && _isEnabled)
        {
            _logger.LogInformation("Connectivity restored, triggering sync");
            await PerformSyncAsync();
        }
    }

    private async Task PerformSyncAsync()
    {
        try
        {
            // Check if already syncing
            if (_syncService.IsSyncing)
            {
                _logger.LogInformation("Sync already in progress, skipping");
                return;
            }

            // Check connectivity
            if (!_connectivityService.IsConnected)
            {
                _logger.LogInformation("No connectivity, skipping background sync");
                return;
            }

            // Check if there are pending changes
            var pendingCount = await _syncService.GetPendingChangesCountAsync();
            if (pendingCount == 0)
            {
                _logger.LogInformation("No pending changes to sync");
                return;
            }

            _logger.LogInformation("Starting background sync with {Count} pending changes", pendingCount);

            var result = await _syncService.SyncAsync();
            LastSyncResult = result;

            if (result.Success)
            {
                _logger.LogInformation(
                    "Background sync completed successfully: Pushed={Pushed}, Pulled={Pulled}",
                    result.TotalPushed,
                    result.TotalPulled
                );
            }
            else
            {
                _logger.LogWarning(
                    "Background sync completed with errors: {Errors}",
                    string.Join(", ", result.ErrorMessages)
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during background sync");
        }
    }

    /// <summary>
    /// Performs a full sync operation (push and pull) regardless of pending changes.
    /// This is used for initial sync on app startup to ensure data is up-to-date.
    /// </summary>
    private async Task PerformFullSyncAsync()
    {
        try
        {
            // Check if already syncing
            if (_syncService.IsSyncing)
            {
                _logger.LogInformation("Sync already in progress, skipping full sync");
                return;
            }

            // Check connectivity
            if (!_connectivityService.IsConnected)
            {
                _logger.LogInformation("No connectivity, skipping full sync");
                return;
            }

            _logger.LogInformation("Starting full sync on app startup");

            var result = await _syncService.SyncAsync();
            LastSyncResult = result;

            if (result.Success)
            {
                _logger.LogInformation(
                    "Full sync completed successfully: Pushed={Pushed}, Pulled={Pulled}, Duration={Duration}ms",
                    result.TotalPushed,
                    result.TotalPulled,
                    result.Duration.TotalMilliseconds
                );
            }
            else
            {
                _logger.LogWarning(
                    "Full sync completed with errors: {Errors}",
                    string.Join(", ", result.ErrorMessages)
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during full sync");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            Stop();
            _connectivityService.ConnectivityChanged -= OnConnectivityChanged;
        }
    }
}
