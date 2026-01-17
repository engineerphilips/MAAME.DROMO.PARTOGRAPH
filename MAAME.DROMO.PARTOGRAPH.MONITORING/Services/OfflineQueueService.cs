using System.Net.Http.Json;
using Blazored.LocalStorage;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class OfflineQueueService : IOfflineQueueService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private const string QUEUED_ALERTS_KEY = "offline_queued_alerts";
        private const string QUEUED_ACKNOWLEDGMENTS_KEY = "offline_queued_acknowledgments";
        private const string LAST_SYNC_KEY = "last_sync_time";

        public OfflineQueueService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task<OfflineQueueStatus> GetQueueStatusAsync()
        {
            try
            {
                var pendingAlerts = await GetPendingAlertsAsync();
                var pendingAcknowledgments = await GetPendingAcknowledgmentsAsync();
                var lastSyncTime = await _localStorage.GetItemAsync<DateTime?>(LAST_SYNC_KEY);

                // Check connectivity
                var isOnline = await CheckConnectivityAsync();

                return new OfflineQueueStatus
                {
                    IsOnline = isOnline,
                    PendingAlerts = pendingAlerts.Count,
                    PendingAcknowledgments = pendingAcknowledgments.Count,
                    LastSyncTime = lastSyncTime,
                    NextSyncAttempt = isOnline ? null : DateTime.UtcNow.AddMinutes(5),
                    SyncErrors = new List<string>()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting queue status: {ex.Message}");
                return new OfflineQueueStatus
                {
                    IsOnline = false,
                    PendingAlerts = 0,
                    PendingAcknowledgments = 0,
                    SyncErrors = new List<string> { ex.Message }
                };
            }
        }

        public async Task QueueAlertAsync(EnhancedAlert alert)
        {
            try
            {
                var queuedAlerts = await _localStorage.GetItemAsync<List<QueuedAlert>>(QUEUED_ALERTS_KEY) ?? new List<QueuedAlert>();

                queuedAlerts.Add(new QueuedAlert
                {
                    Id = Guid.NewGuid(),
                    Alert = alert,
                    QueuedAt = DateTime.UtcNow,
                    IsSynced = false,
                    SyncAttempts = 0
                });

                await _localStorage.SetItemAsync(QUEUED_ALERTS_KEY, queuedAlerts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error queuing alert: {ex.Message}");
            }
        }

        public async Task QueueAcknowledgmentAsync(AlertAcknowledgmentRequest request)
        {
            try
            {
                var queuedAcknowledgments = await _localStorage.GetItemAsync<List<QueuedAcknowledgment>>(QUEUED_ACKNOWLEDGMENTS_KEY) ?? new List<QueuedAcknowledgment>();

                queuedAcknowledgments.Add(new QueuedAcknowledgment
                {
                    Id = Guid.NewGuid(),
                    Request = request,
                    QueuedAt = DateTime.UtcNow,
                    IsSynced = false,
                    SyncAttempts = 0
                });

                await _localStorage.SetItemAsync(QUEUED_ACKNOWLEDGMENTS_KEY, queuedAcknowledgments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error queuing acknowledgment: {ex.Message}");
            }
        }

        public async Task<bool> SyncQueueAsync()
        {
            try
            {
                var isOnline = await CheckConnectivityAsync();
                if (!isOnline) return false;

                var success = true;

                // Sync alerts
                var queuedAlerts = await _localStorage.GetItemAsync<List<QueuedAlert>>(QUEUED_ALERTS_KEY) ?? new List<QueuedAlert>();
                foreach (var queuedAlert in queuedAlerts.Where(a => !a.IsSynced))
                {
                    try
                    {
                        var response = await _httpClient.PostAsJsonAsync("api/monitoring/alerts/sync", queuedAlert.Alert);
                        if (response.IsSuccessStatusCode)
                        {
                            queuedAlert.IsSynced = true;
                            queuedAlert.SyncedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            queuedAlert.SyncAttempts++;
                            queuedAlert.LastSyncError = $"HTTP {response.StatusCode}";
                            success = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        queuedAlert.SyncAttempts++;
                        queuedAlert.LastSyncError = ex.Message;
                        success = false;
                    }
                }
                await _localStorage.SetItemAsync(QUEUED_ALERTS_KEY, queuedAlerts);

                // Sync acknowledgments
                var queuedAcknowledgments = await _localStorage.GetItemAsync<List<QueuedAcknowledgment>>(QUEUED_ACKNOWLEDGMENTS_KEY) ?? new List<QueuedAcknowledgment>();
                foreach (var queuedAck in queuedAcknowledgments.Where(a => !a.IsSynced))
                {
                    try
                    {
                        var response = await _httpClient.PostAsJsonAsync("api/monitoring/alerts/acknowledge", queuedAck.Request);
                        if (response.IsSuccessStatusCode)
                        {
                            queuedAck.IsSynced = true;
                            queuedAck.SyncedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            queuedAck.SyncAttempts++;
                            queuedAck.LastSyncError = $"HTTP {response.StatusCode}";
                            success = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        queuedAck.SyncAttempts++;
                        queuedAck.LastSyncError = ex.Message;
                        success = false;
                    }
                }
                await _localStorage.SetItemAsync(QUEUED_ACKNOWLEDGMENTS_KEY, queuedAcknowledgments);

                // Update last sync time
                await _localStorage.SetItemAsync(LAST_SYNC_KEY, DateTime.UtcNow);

                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error syncing queue: {ex.Message}");
                return false;
            }
        }

        public async Task<List<QueuedAlert>> GetPendingAlertsAsync()
        {
            try
            {
                var queuedAlerts = await _localStorage.GetItemAsync<List<QueuedAlert>>(QUEUED_ALERTS_KEY) ?? new List<QueuedAlert>();
                return queuedAlerts.Where(a => !a.IsSynced).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting pending alerts: {ex.Message}");
                return new List<QueuedAlert>();
            }
        }

        public async Task ClearSyncedItemsAsync()
        {
            try
            {
                var queuedAlerts = await _localStorage.GetItemAsync<List<QueuedAlert>>(QUEUED_ALERTS_KEY) ?? new List<QueuedAlert>();
                queuedAlerts = queuedAlerts.Where(a => !a.IsSynced).ToList();
                await _localStorage.SetItemAsync(QUEUED_ALERTS_KEY, queuedAlerts);

                var queuedAcknowledgments = await _localStorage.GetItemAsync<List<QueuedAcknowledgment>>(QUEUED_ACKNOWLEDGMENTS_KEY) ?? new List<QueuedAcknowledgment>();
                queuedAcknowledgments = queuedAcknowledgments.Where(a => !a.IsSynced).ToList();
                await _localStorage.SetItemAsync(QUEUED_ACKNOWLEDGMENTS_KEY, queuedAcknowledgments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing synced items: {ex.Message}");
            }
        }

        private async Task<List<QueuedAcknowledgment>> GetPendingAcknowledgmentsAsync()
        {
            try
            {
                var queuedAcknowledgments = await _localStorage.GetItemAsync<List<QueuedAcknowledgment>>(QUEUED_ACKNOWLEDGMENTS_KEY) ?? new List<QueuedAcknowledgment>();
                return queuedAcknowledgments.Where(a => !a.IsSynced).ToList();
            }
            catch
            {
                return new List<QueuedAcknowledgment>();
            }
        }

        private async Task<bool> CheckConnectivityAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

    // Helper class for queued acknowledgments
    internal class QueuedAcknowledgment
    {
        public Guid Id { get; set; }
        public AlertAcknowledgmentRequest Request { get; set; } = new();
        public DateTime QueuedAt { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? SyncedAt { get; set; }
        public int SyncAttempts { get; set; }
        public string? LastSyncError { get; set; }
    }
}
