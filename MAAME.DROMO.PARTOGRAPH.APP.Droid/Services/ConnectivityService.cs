using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;

/// <summary>
/// Service for monitoring network connectivity status
/// </summary>
public class ConnectivityService : IConnectivityService
{
    private readonly ILogger<ConnectivityService> _logger;

    public ConnectivityService(ILogger<ConnectivityService> logger)
    {
        _logger = logger;

        // Subscribe to MAUI connectivity changes
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    /// <inheritdoc/>
    public bool IsConnected => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    /// <inheritdoc/>
    public event EventHandler<bool>? ConnectivityChanged;

    /// <inheritdoc/>
    public async Task<bool> IsHostReachableAsync(string host, int timeoutMs = 5000)
    {
        try
        {
            // Try to ping the host
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(host, timeoutMs);
            return reply.Status == IPStatus.Success;
        }
        catch (PingException ex)
        {
            _logger.LogWarning(ex, "Ping failed for host {Host}", host);

            // Fallback: Try TCP connection
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(host, 80);
                var timeoutTask = Task.Delay(timeoutMs);
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == connectTask && !connectTask.IsFaulted)
                {
                    return true;
                }
            }
            catch (Exception tcpEx)
            {
                _logger.LogWarning(tcpEx, "TCP connection failed for host {Host}", host);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking host reachability for {Host}", host);
            return false;
        }
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        var isConnected = e.NetworkAccess == NetworkAccess.Internet;
        _logger.LogInformation("Connectivity changed: {IsConnected}", isConnected);
        ConnectivityChanged?.Invoke(this, isConnected);
    }
}
