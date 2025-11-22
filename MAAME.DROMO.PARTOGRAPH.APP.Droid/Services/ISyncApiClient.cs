using MAAME.DROMO.PARTOGRAPH.MODEL;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;

/// <summary>
/// Interface for API client that handles sync operations with the server
/// </summary>
public interface ISyncApiClient
{
    /// <summary>
    /// Pulls data from server for a specific table
    /// </summary>
    Task<SyncPullResponse<T>> PullAsync<T>(SyncPullRequest request) where T : BasePartographMeasurement;

    /// <summary>
    /// Pushes local changes to server
    /// </summary>
    Task<SyncPushResponse<T>> PushAsync<T>(SyncPushRequest<T> request) where T : BasePartographMeasurement;

    /// <summary>
    /// Pulls patient data from server
    /// </summary>
    Task<SyncPullResponse<Patient>> PullPatientsAsync(SyncPullRequest request);

    /// <summary>
    /// Pushes patient changes to server
    /// </summary>
    Task<SyncPushResponse<Patient>> PushPatientsAsync(SyncPushRequest<Patient> request);

    /// <summary>
    /// Pulls partograph data from server
    /// </summary>
    Task<SyncPullResponse<Partograph>> PullPartographsAsync(SyncPullRequest request);

    /// <summary>
    /// Pushes partograph changes to server
    /// </summary>
    Task<SyncPushResponse<Partograph>> PushPartographsAsync(SyncPushRequest<Partograph> request);

    /// <summary>
    /// Pulls staff data from server
    /// </summary>
    Task<SyncPullResponse<Staff>> PullStaffAsync(SyncPullRequest request);

    /// <summary>
    /// Gets the configured API base URL
    /// </summary>
    string BaseUrl { get; }

    /// <summary>
    /// Tests connectivity to the API server
    /// </summary>
    Task<bool> TestConnectionAsync();
}
