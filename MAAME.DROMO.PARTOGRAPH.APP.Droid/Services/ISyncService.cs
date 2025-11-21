namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;

/// <summary>
/// Sync status enumeration
/// </summary>
public enum SyncStatus
{
    Idle,
    Syncing,
    Success,
    Error,
    Conflict
}

/// <summary>
/// Sync progress information
/// </summary>
public class SyncProgress
{
    public string TableName { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int ProcessedRecords { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public int ConflictCount { get; set; }
    public string? CurrentOperation { get; set; }
    public double ProgressPercentage => TotalRecords > 0 ? (ProcessedRecords * 100.0 / TotalRecords) : 0;
}

/// <summary>
/// Sync result information
/// </summary>
public class SyncResult
{
    public bool Success { get; set; }
    public DateTime SyncTime { get; set; }
    public int TotalPushed { get; set; }
    public int TotalPulled { get; set; }
    public int TotalConflicts { get; set; }
    public int TotalErrors { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Interface for sync service that manages offline sync operations
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// Gets the current sync status
    /// </summary>
    SyncStatus CurrentStatus { get; }

    /// <summary>
    /// Gets the last sync time
    /// </summary>
    DateTime? LastSyncTime { get; }

    /// <summary>
    /// Gets whether sync is currently in progress
    /// </summary>
    bool IsSyncing { get; }

    /// <summary>
    /// Event raised when sync status changes
    /// </summary>
    event EventHandler<SyncStatus>? StatusChanged;

    /// <summary>
    /// Event raised when sync progress updates
    /// </summary>
    event EventHandler<SyncProgress>? ProgressChanged;

    /// <summary>
    /// Performs a full synchronization (push local changes, then pull server changes)
    /// </summary>
    Task<SyncResult> SyncAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Pushes local changes to the server
    /// </summary>
    Task<SyncResult> PushAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Pulls server changes to the local database
    /// </summary>
    Task<SyncResult> PullAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of pending changes to sync
    /// </summary>
    Task<int> GetPendingChangesCountAsync();

    /// <summary>
    /// Gets the count of unresolved conflicts
    /// </summary>
    Task<int> GetConflictsCountAsync();

    /// <summary>
    /// Resolves a conflict by choosing local or server version
    /// </summary>
    Task ResolveConflictAsync(string recordId, bool useLocalVersion);

    /// <summary>
    /// Cancels the current sync operation
    /// </summary>
    Task CancelSyncAsync();
}
