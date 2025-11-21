namespace MAAME.DROMO.PARTOGRAPH.SERVICE.Models;

/// <summary>
/// Request for pulling data from server
/// </summary>
public class SyncPullRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public long LastSyncTimestamp { get; set; }
    public string TableName { get; set; } = string.Empty;
}

/// <summary>
/// Response from server with pulled data
/// </summary>
public class SyncPullResponse<T>
{
    public List<T> Records { get; set; } = new();
    public long ServerTimestamp { get; set; }
    public bool HasMore { get; set; }
}

/// <summary>
/// Request for pushing local changes to server
/// </summary>
public class SyncPushRequest<T>
{
    public string DeviceId { get; set; } = string.Empty;
    public List<T> Changes { get; set; } = new();
}

/// <summary>
/// Response from server after pushing data
/// </summary>
public class SyncPushResponse<T>
{
    public List<string> SuccessIds { get; set; } = new();
    public List<ConflictRecord<T>> Conflicts { get; set; } = new();
    public List<SyncError> Errors { get; set; } = new();
}

/// <summary>
/// Represents a conflict between local and server data
/// </summary>
public class ConflictRecord<T>
{
    public string Id { get; set; } = string.Empty;
    public T? LocalRecord { get; set; }
    public T? ServerRecord { get; set; }
    public DateTime ConflictTime { get; set; }
    public string ConflictReason { get; set; } = string.Empty;
}

/// <summary>
/// Represents a sync error
/// </summary>
public class SyncError
{
    public string Id { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
}

/// <summary>
/// Metadata for tracking sync state per table
/// </summary>
public class SyncMetadata
{
    public string TableName { get; set; } = string.Empty;
    public long LastPullTimestamp { get; set; }
    public long LastPushTimestamp { get; set; }
    public long LastSuccessfulSync { get; set; }
    public int PendingPushCount { get; set; }
    public int ConflictCount { get; set; }
    public string DeviceId { get; set; } = string.Empty;
}
