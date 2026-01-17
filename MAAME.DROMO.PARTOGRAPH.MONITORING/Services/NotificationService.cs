using System.Net.Http.Json;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;

        public NotificationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<NotificationSubscription?> GetSubscriptionAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<NotificationSubscription>($"api/monitoring/notifications/subscription/{userId}");
                return response ?? GetDefaultSubscription(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting notification subscription: {ex.Message}");
                return GetDefaultSubscription(userId);
            }
        }

        public async Task<bool> SaveSubscriptionAsync(NotificationSubscription subscription)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/monitoring/notifications/subscription", subscription);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving notification subscription: {ex.Message}");
                return false;
            }
        }

        public async Task<List<NotificationMessage>> GetUnreadNotificationsAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<NotificationMessage>>($"api/monitoring/notifications/unread/{userId}");
                return response ?? GenerateMockNotifications(userId, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting unread notifications: {ex.Message}");
                return GenerateMockNotifications(userId, true);
            }
        }

        public async Task<List<NotificationMessage>> GetNotificationHistoryAsync(string userId, int count = 50)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<NotificationMessage>>($"api/monitoring/notifications/history/{userId}?count={count}");
                return response ?? GenerateMockNotifications(userId, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting notification history: {ex.Message}");
                return GenerateMockNotifications(userId, false);
            }
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/monitoring/notifications/{notificationId}/read", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking notification as read: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/monitoring/notifications/mark-all-read/{userId}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking all notifications as read: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<int>($"api/monitoring/notifications/unread-count/{userId}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting unread count: {ex.Message}");
                return new Random().Next(0, 8);
            }
        }

        private NotificationSubscription GetDefaultSubscription(string userId)
        {
            return new NotificationSubscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                UserName = "User",
                Email = $"{userId}@maamedromo.gh",
                EmailEnabled = true,
                SmsEnabled = false,
                PushEnabled = true,
                InAppEnabled = true,
                CriticalAlerts = true,
                WarningAlerts = true,
                InfoAlerts = false,
                FetalAlerts = true,
                MaternalAlerts = true,
                LaborAlerts = true,
                SystemAlerts = false,
                QuietHoursEnabled = false,
                QuietHoursStart = new TimeOnly(22, 0),
                QuietHoursEnd = new TimeOnly(6, 0),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private List<NotificationMessage> GenerateMockNotifications(string userId, bool unreadOnly)
        {
            var random = new Random();
            var notifications = new List<NotificationMessage>();

            var templates = new (string Title, string Body, string Severity, string Category)[]
            {
                ("Critical Alert: Low FHR", "Patient Abena Mensah - FHR dropped to 95 bpm at Korle Bu", "Critical", "Fetal"),
                ("High Blood Pressure Alert", "Patient Akosua Owusu - BP 165/105 mmHg requires attention", "Warning", "Maternal"),
                ("Prolonged Labor Warning", "Patient Ama Asante - First stage exceeding 14 hours", "Warning", "Labor"),
                ("Measurement Due", "FHR measurement due for 3 patients in your facility", "Info", "System"),
                ("New Admission", "New patient admitted for labor monitoring at Ridge Hospital", "Info", "System"),
                ("Escalated Alert", "Unacknowledged critical alert escalated to district supervisor", "Warning", "System"),
                ("Temperature Alert", "Patient Adwoa Boateng - Temperature 38.5°C detected", "Warning", "Maternal"),
                ("Second Stage Alert", "Patient Yaa Darko - Second stage duration exceeding 2 hours", "Warning", "Labor")
            };

            var count = random.Next(5, 12);
            for (int i = 0; i < count; i++)
            {
                var template = templates[random.Next(templates.Length)];
                var isRead = !unreadOnly && random.Next(3) != 0;
                var createdAt = DateTime.UtcNow.AddMinutes(-random.Next(5, 240));

                notifications.Add(new NotificationMessage
                {
                    Id = Guid.NewGuid(),
                    Title = template.Title,
                    Body = template.Body,
                    Severity = template.Severity,
                    Category = template.Category,
                    AlertId = Guid.NewGuid(),
                    PartographId = template.Category != "System" ? Guid.NewGuid() : null,
                    FacilityId = Guid.NewGuid(),
                    ActionUrl = template.Category != "System" ? "/live-labor" : "/alerts",
                    ActionLabel = template.Category != "System" ? "View Patient" : "View Alerts",
                    CreatedAt = createdAt,
                    IsSent = true,
                    SentAt = createdAt,
                    IsRead = isRead,
                    ReadAt = isRead ? createdAt.AddMinutes(random.Next(1, 30)) : null
                });
            }

            return notifications
                .Where(n => !unreadOnly || !n.IsRead)
                .OrderByDescending(n => n.Severity == "Critical")
                .ThenByDescending(n => n.CreatedAt)
                .ToList();
        }
    }
}
