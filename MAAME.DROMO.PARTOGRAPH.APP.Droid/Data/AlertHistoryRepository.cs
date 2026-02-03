using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using MAAME.DROMO.PARTOGRAPH.APP.Droid.Services;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    /// <summary>
    /// Model for persisted alert history records
    /// </summary>
    public class AlertHistoryRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? PartographId { get; set; }
        public Guid? PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty; // "MeasurementDue" or "ClinicalAlert"
        public string MeasurementType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // Critical, Warning, Info
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? AcknowledgedAt { get; set; }
        public string AcknowledgedBy { get; set; } = string.Empty;
        public DateTime? ResolvedAt { get; set; }
        public string ResolvedBy { get; set; } = string.Empty;
        public int EscalationLevel { get; set; } = 0;
        public DateTime? EscalatedAt { get; set; }
        public int ResponseTimeMinutes { get; set; }
        public bool IsMissed { get; set; }
        public string ShiftId { get; set; } = string.Empty;
        public Guid? FacilityId { get; set; }

        // Calculated properties
        public bool IsAcknowledged => AcknowledgedAt.HasValue;
        public bool IsResolved => ResolvedAt.HasValue;
        public bool IsEscalated => EscalationLevel > 0;

        public string SeverityColor => Severity switch
        {
            "Critical" => "#EF5350",
            "Warning" => "#FF9800",
            "Info" => "#2196F3",
            _ => "#9E9E9E"
        };

        public string SeverityIcon => Severity switch
        {
            "Critical" => "🚨",
            "Warning" => "⚠️",
            "Info" => "ℹ️",
            _ => "🔔"
        };

        // UI Display properties
        public bool HasStatus => IsAcknowledged || IsMissed || IsEscalated;

        public string StatusDisplay
        {
            get
            {
                if (IsMissed) return "MISSED";
                if (IsAcknowledged) return "✓ ACK";
                if (IsEscalated) return $"L{EscalationLevel}";
                return "";
            }
        }

        public string StatusColor
        {
            get
            {
                if (IsMissed) return "#C62828";
                if (IsAcknowledged) return "#4CAF50";
                if (IsEscalated) return "#E65100";
                return "#9E9E9E";
            }
        }
    }

    /// <summary>
    /// Analytics model for alert compliance tracking
    /// </summary>
    public class AlertAnalytics
    {
        public int TotalAlerts { get; set; }
        public int AcknowledgedAlerts { get; set; }
        public int MissedAlerts { get; set; }
        public int EscalatedAlerts { get; set; }
        public double AverageResponseTimeMinutes { get; set; }
        public double CompliancePercentage { get; set; }
        public Dictionary<string, int> AlertsByType { get; set; } = new();
        public Dictionary<string, int> AlertsBySeverity { get; set; } = new();
        public Dictionary<string, double> ComplianceByMeasurementType { get; set; } = new();
        public List<HourlyAlertCount> AlertsByHour { get; set; } = new();
        public List<AlertHistoryRecord> RecentAlerts { get; set; } = new();

        // Alias properties for ViewModel compatibility
        public int AcknowledgedCount => AcknowledgedAlerts;
        public int MissedCount => MissedAlerts;
        public Dictionary<string, int> ByType => AlertsByType;
        public Dictionary<string, int> BySeverity => AlertsBySeverity;
        public Dictionary<int, int> ByHour => AlertsByHour.ToDictionary(h => h.Hour, h => h.Count);
    }

    public class HourlyAlertCount
    {
        public int Hour { get; set; }
        public int Count { get; set; }
        public int Acknowledged { get; set; }
        public int Missed { get; set; }
    }

    /// <summary>
    /// Shift handover report model
    /// </summary>
    public class ShiftHandoverReport
    {
        public string ShiftId { get; set; } = string.Empty;
        public DateTime ShiftStart { get; set; }
        public DateTime ShiftEnd { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public Guid? FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;

        // Summary stats
        public int TotalActivePatients { get; set; }
        public int TotalAlertsGenerated { get; set; }
        public int AlertsAcknowledged { get; set; }
        public int AlertsMissed { get; set; }
        public int MeasurementsCompleted { get; set; }
        public double CompliancePercentage { get; set; }

        // Pending items for next shift
        public List<AlertHistoryRecord> PendingAlerts { get; set; } = new();
        public List<PatientAttentionItem> PatientsRequiringAttention { get; set; } = new();
        public List<OverdueMeasurement> OverdueMeasurements { get; set; } = new();

        // Completed items this shift
        public List<AlertHistoryRecord> ResolvedAlerts { get; set; } = new();
    }

    public class PatientAttentionItem
    {
        public Guid PatientId { get; set; }
        public Guid PartographId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public int OverdueMinutes { get; set; }
    }

    public class OverdueMeasurement
    {
        public Guid PartographId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MeasurementType { get; set; } = string.Empty;
        public DateTime LastMeasurementTime { get; set; }
        public int MinutesOverdue { get; set; }
    }

    /// <summary>
    /// Repository for persisting alert history to local SQLite database.
    /// Supports analytics, shift handover reports, and compliance tracking.
    /// </summary>
    public class AlertHistoryRepository
    {
        private readonly ILogger<AlertHistoryRepository>? _logger;
        private SqliteConnection? _connection;
        private bool _initialized;
        private readonly SemaphoreSlim _initLock = new(1, 1);

        private const string DatabaseName = "alerthistory.db";

        private string CreateTableSql => @"
            CREATE TABLE IF NOT EXISTS Tbl_AlertHistory (
                ID TEXT PRIMARY KEY,
                partographid TEXT,
                patientid TEXT,
                patientname TEXT,
                alerttype TEXT NOT NULL,
                measurementtype TEXT,
                severity TEXT NOT NULL,
                title TEXT NOT NULL,
                message TEXT,
                createdat INTEGER NOT NULL,
                acknowledgedat INTEGER,
                acknowledgedby TEXT,
                resolvedat INTEGER,
                resolvedby TEXT,
                escalationlevel INTEGER DEFAULT 0,
                escalatedat INTEGER,
                responsetimeminutes INTEGER DEFAULT 0,
                ismissed INTEGER DEFAULT 0,
                shiftid TEXT,
                facilityid TEXT
            );

            CREATE INDEX IF NOT EXISTS idx_alert_partograph ON Tbl_AlertHistory(partographid);
            CREATE INDEX IF NOT EXISTS idx_alert_patient ON Tbl_AlertHistory(patientid);
            CREATE INDEX IF NOT EXISTS idx_alert_createdat ON Tbl_AlertHistory(createdat);
            CREATE INDEX IF NOT EXISTS idx_alert_severity ON Tbl_AlertHistory(severity);
            CREATE INDEX IF NOT EXISTS idx_alert_shiftid ON Tbl_AlertHistory(shiftid);
            CREATE INDEX IF NOT EXISTS idx_alert_facility ON Tbl_AlertHistory(facilityid);
            CREATE INDEX IF NOT EXISTS idx_alert_acknowledged ON Tbl_AlertHistory(acknowledgedat);
        ";

        public AlertHistoryRepository(ILogger<AlertHistoryRepository>? logger = null)
        {
            _logger = logger;
        }

        private async Task InitAsync()
        {
            if (_initialized) return;

            await _initLock.WaitAsync();
            try
            {
                if (_initialized) return;

                var dbPath = Path.Combine(FileSystem.AppDataDirectory, DatabaseName);
                _connection = new SqliteConnection($"Data Source={dbPath}");
                await _connection.OpenAsync();

                using var cmd = _connection.CreateCommand();
                cmd.CommandText = CreateTableSql;
                await cmd.ExecuteNonQueryAsync();

                _initialized = true;
                _logger?.LogInformation("AlertHistoryRepository initialized at {Path}", dbPath);
            }
            finally
            {
                _initLock.Release();
            }
        }

        /// <summary>
        /// Saves an alert to the history database
        /// </summary>
        public async Task<Guid> SaveAlertAsync(AlertHistoryRecord alert)
        {
            await InitAsync();

            using var cmd = _connection!.CreateCommand();
            cmd.CommandText = @"
                INSERT OR REPLACE INTO Tbl_AlertHistory
                (ID, partographid, patientid, patientname, alerttype, measurementtype, severity,
                 title, message, createdat, acknowledgedat, acknowledgedby, resolvedat, resolvedby,
                 escalationlevel, escalatedat, responsetimeminutes, ismissed, shiftid, facilityid)
                VALUES
                (@id, @partographid, @patientid, @patientname, @alerttype, @measurementtype, @severity,
                 @title, @message, @createdat, @acknowledgedat, @acknowledgedby, @resolvedat, @resolvedby,
                 @escalationlevel, @escalatedat, @responsetimeminutes, @ismissed, @shiftid, @facilityid)";

            cmd.Parameters.AddWithValue("@id", alert.Id.ToString());
            cmd.Parameters.AddWithValue("@partographid", alert.PartographId?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@patientid", alert.PatientId?.ToString() ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@patientname", alert.PatientName);
            cmd.Parameters.AddWithValue("@alerttype", alert.AlertType);
            cmd.Parameters.AddWithValue("@measurementtype", alert.MeasurementType);
            cmd.Parameters.AddWithValue("@severity", alert.Severity);
            cmd.Parameters.AddWithValue("@title", alert.Title);
            cmd.Parameters.AddWithValue("@message", alert.Message);
            cmd.Parameters.AddWithValue("@createdat", new DateTimeOffset(alert.CreatedAt).ToUnixTimeMilliseconds());
            cmd.Parameters.AddWithValue("@acknowledgedat", alert.AcknowledgedAt.HasValue
                ? new DateTimeOffset(alert.AcknowledgedAt.Value).ToUnixTimeMilliseconds()
                : DBNull.Value);
            cmd.Parameters.AddWithValue("@acknowledgedby", alert.AcknowledgedBy ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@resolvedat", alert.ResolvedAt.HasValue
                ? new DateTimeOffset(alert.ResolvedAt.Value).ToUnixTimeMilliseconds()
                : DBNull.Value);
            cmd.Parameters.AddWithValue("@resolvedby", alert.ResolvedBy ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@escalationlevel", alert.EscalationLevel);
            cmd.Parameters.AddWithValue("@escalatedat", alert.EscalatedAt.HasValue
                ? new DateTimeOffset(alert.EscalatedAt.Value).ToUnixTimeMilliseconds()
                : DBNull.Value);
            cmd.Parameters.AddWithValue("@responsetimeminutes", alert.ResponseTimeMinutes);
            cmd.Parameters.AddWithValue("@ismissed", alert.IsMissed ? 1 : 0);
            cmd.Parameters.AddWithValue("@shiftid", alert.ShiftId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@facilityid", alert.FacilityId?.ToString() ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
            return alert.Id;
        }

        /// <summary>
        /// Gets an alert by ID
        /// </summary>
        public async Task<AlertHistoryRecord?> GetAlertAsync(Guid id)
        {
            await InitAsync();

            using var cmd = _connection!.CreateCommand();
            cmd.CommandText = "SELECT * FROM Tbl_AlertHistory WHERE ID = @id";
            cmd.Parameters.AddWithValue("@id", id.ToString());

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }
            return null;
        }

        /// <summary>
        /// Gets all unacknowledged alerts
        /// </summary>
        public async Task<List<AlertHistoryRecord>> GetUnacknowledgedAlertsAsync(Guid? facilityId = null)
        {
            await InitAsync();

            using var cmd = _connection!.CreateCommand();
            var sql = "SELECT * FROM Tbl_AlertHistory WHERE acknowledgedat IS NULL";
            if (facilityId.HasValue)
            {
                sql += " AND facilityid = @facilityid";
                cmd.Parameters.AddWithValue("@facilityid", facilityId.Value.ToString());
            }
            sql += " ORDER BY createdat DESC";
            cmd.CommandText = sql;

            var alerts = new List<AlertHistoryRecord>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                alerts.Add(MapFromReader(reader));
            }
            return alerts;
        }

        /// <summary>
        /// Gets alerts for a specific shift
        /// </summary>
        public async Task<List<AlertHistoryRecord>> GetAlertsByShiftAsync(string shiftId)
        {
            await InitAsync();

            using var cmd = _connection!.CreateCommand();
            cmd.CommandText = "SELECT * FROM Tbl_AlertHistory WHERE shiftid = @shiftid ORDER BY createdat DESC";
            cmd.Parameters.AddWithValue("@shiftid", shiftId);

            var alerts = new List<AlertHistoryRecord>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                alerts.Add(MapFromReader(reader));
            }
            return alerts;
        }

        /// <summary>
        /// Gets alerts for a specific patient
        /// </summary>
        public async Task<List<AlertHistoryRecord>> GetAlertsByPatientAsync(Guid patientId)
        {
            await InitAsync();

            using var cmd = _connection!.CreateCommand();
            cmd.CommandText = "SELECT * FROM Tbl_AlertHistory WHERE patientid = @patientid ORDER BY createdat DESC";
            cmd.Parameters.AddWithValue("@patientid", patientId.ToString());

            var alerts = new List<AlertHistoryRecord>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                alerts.Add(MapFromReader(reader));
            }
            return alerts;
        }

        /// <summary>
        /// Gets alerts for a specific partograph
        /// </summary>
        public async Task<List<AlertHistoryRecord>> GetAlertsByPartographAsync(Guid partographId)
        {
            await InitAsync();

            using var cmd = _connection!.CreateCommand();
            cmd.CommandText = "SELECT * FROM Tbl_AlertHistory WHERE partographid = @partographid ORDER BY createdat DESC";
            cmd.Parameters.AddWithValue("@partographid", partographId.ToString());

            var alerts = new List<AlertHistoryRecord>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                alerts.Add(MapFromReader(reader));
            }
            return alerts;
        }

        /// <summary>
        /// Acknowledges an alert
        /// </summary>
        public async Task AcknowledgeAlertAsync(Guid alertId, string acknowledgedBy)
        {
            await InitAsync();

            var alert = await GetAlertAsync(alertId);
            if (alert == null || alert.IsAcknowledged) return;

            var responseTime = (int)(DateTime.Now - alert.CreatedAt).TotalMinutes;

            using var cmd = _connection!.CreateCommand();
            cmd.CommandText = @"
                UPDATE Tbl_AlertHistory
                SET acknowledgedat = @acknowledgedat,
                    acknowledgedby = @acknowledgedby,
                    responsetimeminutes = @responsetime
                WHERE ID = @id";

            cmd.Parameters.AddWithValue("@id", alertId.ToString());
            cmd.Parameters.AddWithValue("@acknowledgedat", DateTimeOffset.Now.ToUnixTimeMilliseconds());
            cmd.Parameters.AddWithValue("@acknowledgedby", acknowledgedBy);
            cmd.Parameters.AddWithValue("@responsetime", responseTime);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Resolves an alert
        /// </summary>
        public async Task ResolveAlertAsync(Guid alertId, string resolvedBy)
        {
            await InitAsync();

            using var cmd = _connection!.CreateCommand();
            cmd.CommandText = @"
                UPDATE Tbl_AlertHistory
                SET resolvedat = @resolvedat, resolvedby = @resolvedby
                WHERE ID = @id";

            cmd.Parameters.AddWithValue("@id", alertId.ToString());
            cmd.Parameters.AddWithValue("@resolvedat", DateTimeOffset.Now.ToUnixTimeMilliseconds());
            cmd.Parameters.AddWithValue("@resolvedby", resolvedBy);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Escalates an alert
        /// </summary>
        public async Task EscalateAlertAsync(Guid alertId, int newLevel)
        {
            await InitAsync();

            using var cmd = _connection!.CreateCommand();
            cmd.CommandText = @"
                UPDATE Tbl_AlertHistory
                SET escalationlevel = @level, escalatedat = @escalatedat
                WHERE ID = @id";

            cmd.Parameters.AddWithValue("@id", alertId.ToString());
            cmd.Parameters.AddWithValue("@level", newLevel);
            cmd.Parameters.AddWithValue("@escalatedat", DateTimeOffset.Now.ToUnixTimeMilliseconds());

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Marks an alert as missed
        /// </summary>
        public async Task MarkAlertAsMissedAsync(Guid alertId)
        {
            await InitAsync();

            using var cmd = _connection!.CreateCommand();
            cmd.CommandText = "UPDATE Tbl_AlertHistory SET ismissed = 1 WHERE ID = @id";
            cmd.Parameters.AddWithValue("@id", alertId.ToString());

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Gets analytics for a date range
        /// </summary>
        public async Task<AlertAnalytics> GetAnalyticsAsync(DateTime startDate, DateTime endDate, Guid? facilityId = null)
        {
            await InitAsync();

            var analytics = new AlertAnalytics();
            var startMs = new DateTimeOffset(startDate).ToUnixTimeMilliseconds();
            var endMs = new DateTimeOffset(endDate).ToUnixTimeMilliseconds();

            // Total alerts
            using (var cmd = _connection!.CreateCommand())
            {
                var sql = "SELECT COUNT(*) FROM Tbl_AlertHistory WHERE createdat BETWEEN @start AND @end";
                if (facilityId.HasValue)
                {
                    sql += " AND facilityid = @facilityid";
                    cmd.Parameters.AddWithValue("@facilityid", facilityId.Value.ToString());
                }
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@start", startMs);
                cmd.Parameters.AddWithValue("@end", endMs);
                analytics.TotalAlerts = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            // Acknowledged alerts
            using (var cmd = _connection!.CreateCommand())
            {
                var sql = "SELECT COUNT(*) FROM Tbl_AlertHistory WHERE createdat BETWEEN @start AND @end AND acknowledgedat IS NOT NULL";
                if (facilityId.HasValue)
                {
                    sql += " AND facilityid = @facilityid";
                    cmd.Parameters.AddWithValue("@facilityid", facilityId.Value.ToString());
                }
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@start", startMs);
                cmd.Parameters.AddWithValue("@end", endMs);
                analytics.AcknowledgedAlerts = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            // Missed alerts
            using (var cmd = _connection!.CreateCommand())
            {
                var sql = "SELECT COUNT(*) FROM Tbl_AlertHistory WHERE createdat BETWEEN @start AND @end AND ismissed = 1";
                if (facilityId.HasValue)
                {
                    sql += " AND facilityid = @facilityid";
                    cmd.Parameters.AddWithValue("@facilityid", facilityId.Value.ToString());
                }
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@start", startMs);
                cmd.Parameters.AddWithValue("@end", endMs);
                analytics.MissedAlerts = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            // Escalated alerts
            using (var cmd = _connection!.CreateCommand())
            {
                var sql = "SELECT COUNT(*) FROM Tbl_AlertHistory WHERE createdat BETWEEN @start AND @end AND escalationlevel > 0";
                if (facilityId.HasValue)
                {
                    sql += " AND facilityid = @facilityid";
                    cmd.Parameters.AddWithValue("@facilityid", facilityId.Value.ToString());
                }
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@start", startMs);
                cmd.Parameters.AddWithValue("@end", endMs);
                analytics.EscalatedAlerts = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            // Average response time
            using (var cmd = _connection!.CreateCommand())
            {
                var sql = "SELECT AVG(responsetimeminutes) FROM Tbl_AlertHistory WHERE createdat BETWEEN @start AND @end AND acknowledgedat IS NOT NULL";
                if (facilityId.HasValue)
                {
                    sql += " AND facilityid = @facilityid";
                    cmd.Parameters.AddWithValue("@facilityid", facilityId.Value.ToString());
                }
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@start", startMs);
                cmd.Parameters.AddWithValue("@end", endMs);
                var result = await cmd.ExecuteScalarAsync();
                analytics.AverageResponseTimeMinutes = result != DBNull.Value ? Convert.ToDouble(result) : 0;
            }

            // Compliance percentage
            analytics.CompliancePercentage = analytics.TotalAlerts > 0
                ? (double)analytics.AcknowledgedAlerts / analytics.TotalAlerts * 100
                : 100;

            // Alerts by severity
            using (var cmd = _connection!.CreateCommand())
            {
                var sql = "SELECT severity, COUNT(*) as cnt FROM Tbl_AlertHistory WHERE createdat BETWEEN @start AND @end";
                if (facilityId.HasValue)
                {
                    sql += " AND facilityid = @facilityid";
                    cmd.Parameters.AddWithValue("@facilityid", facilityId.Value.ToString());
                }
                sql += " GROUP BY severity";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@start", startMs);
                cmd.Parameters.AddWithValue("@end", endMs);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    analytics.AlertsBySeverity[reader.GetString(0)] = reader.GetInt32(1);
                }
            }

            // Alerts by measurement type
            using (var cmd = _connection!.CreateCommand())
            {
                var sql = "SELECT measurementtype, COUNT(*) as cnt FROM Tbl_AlertHistory WHERE createdat BETWEEN @start AND @end AND measurementtype != ''";
                if (facilityId.HasValue)
                {
                    sql += " AND facilityid = @facilityid";
                    cmd.Parameters.AddWithValue("@facilityid", facilityId.Value.ToString());
                }
                sql += " GROUP BY measurementtype";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@start", startMs);
                cmd.Parameters.AddWithValue("@end", endMs);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    analytics.AlertsByType[reader.GetString(0)] = reader.GetInt32(1);
                }
            }

            return analytics;
        }

        /// <summary>
        /// Checks if an alert already exists for a specific partograph and measurement type within a time window
        /// </summary>
        public async Task<bool> AlertExistsAsync(Guid partographId, string measurementType, int withinMinutes = 30)
        {
            await InitAsync();

            var cutoffTime = DateTimeOffset.Now.AddMinutes(-withinMinutes).ToUnixTimeMilliseconds();

            using var cmd = _connection!.CreateCommand();
            cmd.CommandText = @"
                SELECT COUNT(*) FROM Tbl_AlertHistory
                WHERE partographid = @partographid
                AND measurementtype = @measurementtype
                AND createdat > @cutoff
                AND resolvedat IS NULL";
            cmd.Parameters.AddWithValue("@partographid", partographId.ToString());
            cmd.Parameters.AddWithValue("@measurementtype", measurementType);
            cmd.Parameters.AddWithValue("@cutoff", cutoffTime);

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }

        /// <summary>
        /// Gets alert history records within a time range
        /// </summary>
        public async Task<List<AlertHistoryRecord>> GetAlertsInRangeAsync(DateTime startDate, DateTime endDate, Guid? facilityId = null)
        {
            await InitAsync();

            var startMs = new DateTimeOffset(startDate).ToUnixTimeMilliseconds();
            var endMs = new DateTimeOffset(endDate).ToUnixTimeMilliseconds();

            using var cmd = _connection!.CreateCommand();
            var sql = "SELECT * FROM Tbl_AlertHistory WHERE createdat BETWEEN @start AND @end";
            if (facilityId.HasValue)
            {
                sql += " AND facilityid = @facilityid";
                cmd.Parameters.AddWithValue("@facilityid", facilityId.Value.ToString());
            }
            sql += " ORDER BY createdat DESC";
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@start", startMs);
            cmd.Parameters.AddWithValue("@end", endMs);

            var alerts = new List<AlertHistoryRecord>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                alerts.Add(MapFromReader(reader));
            }
            return alerts;
        }

        private AlertHistoryRecord MapFromReader(SqliteDataReader reader)
        {
            return new AlertHistoryRecord
            {
                Id = Guid.Parse(reader.GetString(reader.GetOrdinal("ID"))),
                PartographId = reader.IsDBNull(reader.GetOrdinal("partographid"))
                    ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("partographid"))),
                PatientId = reader.IsDBNull(reader.GetOrdinal("patientid"))
                    ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("patientid"))),
                PatientName = reader.IsDBNull(reader.GetOrdinal("patientname"))
                    ? string.Empty : reader.GetString(reader.GetOrdinal("patientname")),
                AlertType = reader.GetString(reader.GetOrdinal("alerttype")),
                MeasurementType = reader.IsDBNull(reader.GetOrdinal("measurementtype"))
                    ? string.Empty : reader.GetString(reader.GetOrdinal("measurementtype")),
                Severity = reader.GetString(reader.GetOrdinal("severity")),
                Title = reader.GetString(reader.GetOrdinal("title")),
                Message = reader.IsDBNull(reader.GetOrdinal("message"))
                    ? string.Empty : reader.GetString(reader.GetOrdinal("message")),
                CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(reader.GetOrdinal("createdat"))).LocalDateTime,
                AcknowledgedAt = reader.IsDBNull(reader.GetOrdinal("acknowledgedat"))
                    ? null : DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(reader.GetOrdinal("acknowledgedat"))).LocalDateTime,
                AcknowledgedBy = reader.IsDBNull(reader.GetOrdinal("acknowledgedby"))
                    ? string.Empty : reader.GetString(reader.GetOrdinal("acknowledgedby")),
                ResolvedAt = reader.IsDBNull(reader.GetOrdinal("resolvedat"))
                    ? null : DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(reader.GetOrdinal("resolvedat"))).LocalDateTime,
                ResolvedBy = reader.IsDBNull(reader.GetOrdinal("resolvedby"))
                    ? string.Empty : reader.GetString(reader.GetOrdinal("resolvedby")),
                EscalationLevel = reader.GetInt32(reader.GetOrdinal("escalationlevel")),
                EscalatedAt = reader.IsDBNull(reader.GetOrdinal("escalatedat"))
                    ? null : DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(reader.GetOrdinal("escalatedat"))).LocalDateTime,
                ResponseTimeMinutes = reader.GetInt32(reader.GetOrdinal("responsetimeminutes")),
                IsMissed = reader.GetInt32(reader.GetOrdinal("ismissed")) == 1,
                ShiftId = reader.IsDBNull(reader.GetOrdinal("shiftid"))
                    ? string.Empty : reader.GetString(reader.GetOrdinal("shiftid")),
                FacilityId = reader.IsDBNull(reader.GetOrdinal("facilityid"))
                    ? null : Guid.Parse(reader.GetString(reader.GetOrdinal("facilityid")))
            };
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
