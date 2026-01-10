using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;
using Microsoft.Extensions.Logging;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels;

public partial class SyncSettingsPageModel : ObservableObject
{
    private readonly ISyncService _syncService;
    private readonly IConnectivityService _connectivityService;
    private readonly BackgroundSyncService _backgroundSyncService;
    private readonly ILogger<SyncSettingsPageModel> _logger;

    public SyncSettingsPageModel(
        ISyncService syncService,
        IConnectivityService connectivityService,
        BackgroundSyncService backgroundSyncService,
        ILogger<SyncSettingsPageModel> logger)
    {
        _syncService = syncService;
        _connectivityService = connectivityService;
        _backgroundSyncService = backgroundSyncService;
        _logger = logger;

        // Subscribe to sync status changes
        _syncService.StatusChanged += OnSyncStatusChanged;
        _syncService.ProgressChanged += OnSyncProgressChanged;

        // Load initial values
        LoadSettings();
    }

    [ObservableProperty]
    private string _apiUrl = string.Empty;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isSyncing;

    [ObservableProperty]
    private string _syncStatus = "Idle";

    [ObservableProperty]
    private DateTime? _lastSyncTime;

    [ObservableProperty]
    private int _pendingChanges;

    [ObservableProperty]
    private int _conflicts;

    [ObservableProperty]
    private bool _backgroundSyncEnabled;

    [ObservableProperty]
    private int _syncIntervalMinutes;

    [ObservableProperty]
    private string _syncProgress = string.Empty;

    [ObservableProperty]
    private double _syncProgressPercentage;

    [RelayCommand]
    private async Task Appearing()
    {
        await RefreshStatus();
    }

    [RelayCommand]
    private async Task ManualSync()
    {
        try
        {
            if (IsSyncing)
            {
                await Application.Current!.MainPage!.DisplayAlert("Sync In Progress", "A sync operation is already running.", "OK");
                return;
            }

            if (!IsConnected)
            {
                await Application.Current!.MainPage!.DisplayAlert("No Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            var result = await _syncService.SyncAsync();

            if (result.Success)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Sync Successful",
                    $"Synced: {result.TotalPushed} sent, {result.TotalPulled} received\nDuration: {result.Duration.TotalSeconds:F1}s",
                    "OK"
                );
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Sync Failed",
                    $"Errors: {result.TotalErrors}\n{string.Join("\n", result.ErrorMessages.Take(3))}",
                    "OK"
                );
            }

            await RefreshStatus();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during manual sync");
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Sync failed: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
        try
        {
            // Save API URL
            Preferences.Set("SyncApiUrl", ApiUrl);

            // Save background sync settings
            _backgroundSyncService.IsEnabled = BackgroundSyncEnabled;
            _backgroundSyncService.SyncIntervalMinutes = SyncIntervalMinutes;

            await Application.Current!.MainPage!.DisplayAlert("Settings Saved", "Your sync settings have been saved successfully.", "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings");
            await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to save settings: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task TestConnection()
    {
        try
        {
            var isReachable = await _connectivityService.IsHostReachableAsync(new Uri(ApiUrl).Host);

            if (isReachable)
            {
                await Application.Current!.MainPage!.DisplayAlert("Connection Test", "Successfully connected to the server!", "OK");
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert("Connection Test", "Could not reach the server. Please check the URL and your internet connection.", "OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection");
            await Application.Current!.MainPage!.DisplayAlert("Connection Test", $"Error: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task ViewConflicts()
    {
        if (Conflicts > 0)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Data Conflicts",
                $"You have {Conflicts} unresolved conflicts. Please resolve them to continue syncing.",
                "OK"
            );
        }
        else
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "No Conflicts",
                "There are no data conflicts at this time.",
                "OK"
            );
        }
    }

    private void LoadSettings()
    {
        // Using HTTP for now - will revisit HTTPS later once SSL/TLS issues are resolved
        ApiUrl = Preferences.Get("SyncApiUrl", "http://192.168.8.4:5218");
        BackgroundSyncEnabled = _backgroundSyncService.IsEnabled;
        SyncIntervalMinutes = _backgroundSyncService.SyncIntervalMinutes;
        IsConnected = _connectivityService.IsConnected;
        LastSyncTime = _syncService.LastSyncTime;
        IsSyncing = _syncService.IsSyncing;
        SyncStatus = _syncService.CurrentStatus.ToString();
    }

    private async Task RefreshStatus()
    {
        IsConnected = _connectivityService.IsConnected;
        LastSyncTime = _syncService.LastSyncTime;
        IsSyncing = _syncService.IsSyncing;
        SyncStatus = _syncService.CurrentStatus.ToString();
        PendingChanges = await _syncService.GetPendingChangesCountAsync();
        Conflicts = await _syncService.GetConflictsCountAsync();
    }

    private void OnSyncStatusChanged(object? sender, SyncStatus status)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SyncStatus = status.ToString();
            IsSyncing = status == Services.SyncStatus.Syncing;
        });
    }

    private void OnSyncProgressChanged(object? sender, SyncProgress progress)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SyncProgress = $"{progress.CurrentOperation} - {progress.ProcessedRecords}/{progress.TotalRecords}";
            SyncProgressPercentage = progress.ProgressPercentage;
        });
    }
}
