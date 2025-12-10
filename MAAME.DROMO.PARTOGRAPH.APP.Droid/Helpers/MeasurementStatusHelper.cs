using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Helpers
{
    public enum MeasurementDueStatus
    {
        NotDue,      // Still within normal interval
        DueSoon,     // Within 15 minutes of due time
        Overdue      // Past the due time
    }

    public class MeasurementStatus
    {
        public string LatestValue { get; set; } = string.Empty;
        public DateTime? LastRecordedTime { get; set; }
        public MeasurementDueStatus DueStatus { get; set; }
        public DateTime? NextDueTime { get; set; }
        public string TimeSinceLastRecord { get; set; } = string.Empty;
        public string DueStatusText { get; set; } = string.Empty;
        public string ButtonColor { get; set; } = "LightGray"; // Default color
    }

    public static class MeasurementStatusHelper
    {
        private const int DueSoonThresholdMinutes = 15;

        /// <summary>
        /// Calculates the status of a measurement including due status and latest value
        /// </summary>
        public static MeasurementStatus CalculateStatus<T>(
            DateTime? lastRecordedTime,
            string latestValue) where T : BasePartographMeasurement
        {
            var status = new MeasurementStatus
            {
                LatestValue = latestValue,
                LastRecordedTime = lastRecordedTime
            };

            if (lastRecordedTime.HasValue)
            {
                // Calculate time since last record
                var timeSince = DateTime.Now - lastRecordedTime.Value;
                status.TimeSinceLastRecord = FormatTimeSince(timeSince);

                // Check if this measurement type has a schedule
                var measurementType = typeof(T);
                if (MeasurementSchedule.ScheduleIntervals.ContainsKey(measurementType))
                {
                    var interval = MeasurementSchedule.ScheduleIntervals[measurementType];
                    status.NextDueTime = lastRecordedTime.Value.Add(interval);

                    var timeUntilDue = status.NextDueTime.Value - DateTime.Now;

                    if (timeUntilDue.TotalMinutes < 0)
                    {
                        // Overdue
                        status.DueStatus = MeasurementDueStatus.Overdue;
                        status.DueStatusText = $"Overdue by {FormatTimeSince(-timeUntilDue)}";
                        status.ButtonColor = "#FFCDD2"; // Light red
                    }
                    else if (timeUntilDue.TotalMinutes <= DueSoonThresholdMinutes)
                    {
                        // Due soon
                        status.DueStatus = MeasurementDueStatus.DueSoon;
                        status.DueStatusText = $"Due in {FormatTimeSince(timeUntilDue)}";
                        status.ButtonColor = "#FFF9C4"; // Light yellow
                    }
                    else
                    {
                        // Not due yet
                        status.DueStatus = MeasurementDueStatus.NotDue;
                        status.DueStatusText = $"Next: {status.NextDueTime.Value:HH:mm}";
                        status.ButtonColor = "#C8E6C9"; // Light green
                    }
                }
                else
                {
                    // No schedule for this measurement type
                    status.DueStatusText = $"Last: {lastRecordedTime.Value:HH:mm}, {MeasurementStatusHelper.FormatTimeSince(lastRecordedTime.Value - DateTime.Now)}";
                    status.ButtonColor = "LightGray";
                }
            }
            else
            {
                // No record yet
                status.DueStatusText = "";
                status.TimeSinceLastRecord = "Never";
                status.ButtonColor = "#E0E0E0"; // Gray
            }

            return status;
        }

        /// <summary>
        /// Formats a time span into a human-readable string
        /// </summary>
        public static string FormatTimeSince(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes < 1)
                return "just now";
            else if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            else if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m ago";
            else
                return $"{(int)timeSpan.TotalDays}d ago";
        }

        /// <summary>
        /// Gets a color based on the due status
        /// </summary>
        public static string GetStatusColor(MeasurementDueStatus status)
        {
            return status switch
            {
                MeasurementDueStatus.Overdue => "#FFCDD2",     // Light red
                MeasurementDueStatus.DueSoon => "#FFF9C4",     // Light yellow
                MeasurementDueStatus.NotDue => "#C8E6C9",      // Light green
                _ => "LightGray"
            };
        }

        /// <summary>
        /// Checks if a measurement is scheduled for periodic tracking
        /// </summary>
        public static bool IsScheduledMeasurement<T>() where T : BasePartographMeasurement
        {
            return MeasurementSchedule.ScheduleIntervals.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Gets the interval for a measurement type
        /// </summary>
        public static TimeSpan? GetMeasurementInterval<T>() where T : BasePartographMeasurement
        {
            var type = typeof(T);
            if (MeasurementSchedule.ScheduleIntervals.TryGetValue(type, out var interval))
                return interval;
            return null;
        }
    }
}
