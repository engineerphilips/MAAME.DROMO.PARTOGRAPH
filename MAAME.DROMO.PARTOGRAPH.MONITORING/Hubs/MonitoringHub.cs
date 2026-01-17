using Microsoft.AspNetCore.SignalR;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time partograph monitoring updates
    /// </summary>
    public class MonitoringHub : Hub
    {
        private readonly ILogger<MonitoringHub> _logger;

        public MonitoringHub(ILogger<MonitoringHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Subscribe to facility-specific updates
        /// </summary>
        public async Task JoinFacilityGroup(string facilityId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"facility_{facilityId}");
            _logger.LogInformation($"Client {Context.ConnectionId} joined facility group: {facilityId}");
        }

        /// <summary>
        /// Subscribe to district-specific updates
        /// </summary>
        public async Task JoinDistrictGroup(string districtId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"district_{districtId}");
            _logger.LogInformation($"Client {Context.ConnectionId} joined district group: {districtId}");
        }

        /// <summary>
        /// Subscribe to region-specific updates
        /// </summary>
        public async Task JoinRegionGroup(string regionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"region_{regionId}");
            _logger.LogInformation($"Client {Context.ConnectionId} joined region group: {regionId}");
        }

        /// <summary>
        /// Subscribe to national-level updates
        /// </summary>
        public async Task JoinNationalGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "national");
            _logger.LogInformation($"Client {Context.ConnectionId} joined national group");
        }

        /// <summary>
        /// Leave a facility group
        /// </summary>
        public async Task LeaveFacilityGroup(string facilityId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"facility_{facilityId}");
        }

        /// <summary>
        /// Leave a district group
        /// </summary>
        public async Task LeaveDistrictGroup(string districtId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"district_{districtId}");
        }

        /// <summary>
        /// Leave a region group
        /// </summary>
        public async Task LeaveRegionGroup(string regionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"region_{regionId}");
        }

        /// <summary>
        /// Leave the national group
        /// </summary>
        public async Task LeaveNationalGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "national");
        }

        /// <summary>
        /// Acknowledge an alert (from client to server)
        /// </summary>
        public async Task AcknowledgeAlert(Guid alertId, string acknowledgedBy, string? notes)
        {
            _logger.LogInformation($"Alert {alertId} acknowledged by {acknowledgedBy}");

            // Broadcast acknowledgment to all connected clients
            await Clients.All.SendAsync("AlertAcknowledged", new
            {
                AlertId = alertId,
                AcknowledgedBy = acknowledgedBy,
                AcknowledgedAt = DateTime.UtcNow,
                Notes = notes
            });
        }

        /// <summary>
        /// Request immediate refresh of live labor data
        /// </summary>
        public async Task RequestLiveLaborRefresh()
        {
            await Clients.Caller.SendAsync("RefreshLiveLabor");
        }
    }

    /// <summary>
    /// Service for sending notifications through SignalR hub
    /// </summary>
    public interface IMonitoringNotificationService
    {
        Task SendAlertAsync(EnhancedAlert alert);
        Task SendLaborUpdateAsync(LiveLaborCase laborCase);
        Task SendSummaryUpdateAsync(LiveLaborSummary summary);
        Task SendAlertAcknowledgmentAsync(Guid alertId, string acknowledgedBy, DateTime acknowledgedAt);
        Task SendAlertResolutionAsync(Guid alertId, string resolvedBy, DateTime resolvedAt);
        Task SendMeasurementDueNotificationAsync(Guid partographId, string patientName, string measurementType);
    }

    public class MonitoringNotificationService : IMonitoringNotificationService
    {
        private readonly IHubContext<MonitoringHub> _hubContext;
        private readonly ILogger<MonitoringNotificationService> _logger;

        public MonitoringNotificationService(
            IHubContext<MonitoringHub> hubContext,
            ILogger<MonitoringNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendAlertAsync(EnhancedAlert alert)
        {
            try
            {
                // Send to all clients
                await _hubContext.Clients.All.SendAsync("NewAlert", alert);

                // Send to specific facility group
                if (alert.FacilityId != Guid.Empty)
                {
                    await _hubContext.Clients.Group($"facility_{alert.FacilityId}")
                        .SendAsync("FacilityAlert", alert);
                }

                _logger.LogInformation($"Alert sent: {alert.Title} ({alert.Severity})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending alert notification");
            }
        }

        public async Task SendLaborUpdateAsync(LiveLaborCase laborCase)
        {
            try
            {
                // Send to all clients
                await _hubContext.Clients.All.SendAsync("LaborUpdate", laborCase);

                // Send to specific facility group
                if (laborCase.FacilityId != Guid.Empty)
                {
                    await _hubContext.Clients.Group($"facility_{laborCase.FacilityId}")
                        .SendAsync("FacilityLaborUpdate", laborCase);
                }

                _logger.LogInformation($"Labor update sent for patient: {laborCase.PatientName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending labor update notification");
            }
        }

        public async Task SendSummaryUpdateAsync(LiveLaborSummary summary)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("SummaryUpdate", summary);
                _logger.LogInformation($"Summary update sent: {summary.TotalActiveCases} active cases");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending summary update notification");
            }
        }

        public async Task SendAlertAcknowledgmentAsync(Guid alertId, string acknowledgedBy, DateTime acknowledgedAt)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("AlertAcknowledged", new
                {
                    AlertId = alertId,
                    AcknowledgedBy = acknowledgedBy,
                    AcknowledgedAt = acknowledgedAt
                });

                _logger.LogInformation($"Alert acknowledgment sent: {alertId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending alert acknowledgment notification");
            }
        }

        public async Task SendAlertResolutionAsync(Guid alertId, string resolvedBy, DateTime resolvedAt)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("AlertResolved", new
                {
                    AlertId = alertId,
                    ResolvedBy = resolvedBy,
                    ResolvedAt = resolvedAt
                });

                _logger.LogInformation($"Alert resolution sent: {alertId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending alert resolution notification");
            }
        }

        public async Task SendMeasurementDueNotificationAsync(Guid partographId, string patientName, string measurementType)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("MeasurementDue", new
                {
                    PartographId = partographId,
                    PatientName = patientName,
                    MeasurementType = measurementType,
                    DueAt = DateTime.UtcNow
                });

                _logger.LogInformation($"Measurement due notification sent: {patientName} - {measurementType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending measurement due notification");
            }
        }
    }
}
