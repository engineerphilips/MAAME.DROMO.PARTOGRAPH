namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;

/// <summary>
/// Interface for monitoring network connectivity status
/// </summary>
public interface IConnectivityService
{
    /// <summary>
    /// Gets whether the device is currently connected to the network
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Event raised when connectivity status changes
    /// </summary>
    event EventHandler<bool> ConnectivityChanged;

    /// <summary>
    /// Checks if a specific host is reachable
    /// </summary>
    Task<bool> IsHostReachableAsync(string host, int timeoutMs = 5000);
}
