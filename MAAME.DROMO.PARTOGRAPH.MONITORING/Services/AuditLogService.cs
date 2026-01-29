using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public interface IAuditLogService
    {
        Task LogActionAsync(AuditLogEntry entry);
        Task<List<AuditLogEntry>> GetAuditTrailAsync(Guid? patientId = null, Guid? userId = null, DateTime? fromDate = null);
        Task<List<AuditLogEntry>> GetRecentLogsAsync(int count = 50);
    }

    public class AuditLogService : IAuditLogService
    {
        // In a real application, this would be a high-performance database table
        // For this implementation, we will use an in-memory list seeded with some mock data
        private static List<AuditLogEntry> _auditLog = new List<AuditLogEntry>();

        public AuditLogService()
        {
            if (!_auditLog.Any())
            {
                SeedMockData();
            }
        }

        public async Task LogActionAsync(AuditLogEntry entry)
        {
            // In reality, this would be an async DB Write
            entry.Timestamp = DateTime.UtcNow;
            _auditLog.Add(entry);
            await Task.CompletedTask;
        }

        public async Task<List<AuditLogEntry>> GetAuditTrailAsync(Guid? patientId = null, Guid? userId = null, DateTime? fromDate = null)
        {
            var query = _auditLog.AsQueryable();

            if (patientId.HasValue)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (userId.HasValue)
                query = query.Where(x => x.UserId == userId.Value);

            if (fromDate.HasValue)
                query = query.Where(x => x.Timestamp >= fromDate.Value);

            return await Task.FromResult(query.OrderByDescending(x => x.Timestamp).ToList());
        }

        public async Task<List<AuditLogEntry>> GetRecentLogsAsync(int count = 50)
        {
            return await Task.FromResult(_auditLog.OrderByDescending(x => x.Timestamp).Take(count).ToList());
        }

        private void SeedMockData()
        {
            var random = new Random();
            var actions = new[] { "UPDATE", "CREATE", "OVERRIDE" };
            var fields = new[] { "FetalHeartRate", "MaternalBP", "OxytocinDose" };
            var users = new[] { "MW Sarah", "Dr. Mensah", "Nurse Joy" };

            for (int i = 0; i < 20; i++)
            {
                _auditLog.Add(new AuditLogEntry
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow.AddHours(-random.Next(1, 48)),
                    UserName = users[random.Next(users.Length)],
                    UserId = Guid.NewGuid(),
                    ActionType = actions[random.Next(actions.Length)],
                    EntityName = "PartographData",
                    FieldName = fields[random.Next(fields.Length)],
                    OldValue = random.Next(100, 120).ToString(),
                    NewValue = random.Next(121, 140).ToString(),
                    Reason = "Correction of entry error",
                    PatientName = "Ama Osei"
                });
            }
        }
    }
}
