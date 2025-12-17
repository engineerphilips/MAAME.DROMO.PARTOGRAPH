using CommunityToolkit.Mvvm.ComponentModel;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.PageModels.Modals
{
    /// <summary>
    /// Represents a single measurement history entry for display in modals.
    /// Formats the date/time appropriately (time only if today, full date/time otherwise).
    /// </summary>
    public partial class MeasurementHistoryItem : ObservableObject
    {
        [ObservableProperty]
        private string _value = string.Empty;

        [ObservableProperty]
        private string _dateTimeDisplay = string.Empty;

        [ObservableProperty]
        private string _recordedBy = string.Empty;

        [ObservableProperty]
        private DateTime _recordedAt;

        /// <summary>
        /// Creates a measurement history item with properly formatted date/time.
        /// Shows time only if today, otherwise shows full date and time.
        /// </summary>
        /// <param name="value">The measurement value to display</param>
        /// <param name="recordedAt">When the measurement was taken</param>
        /// <param name="recordedBy">Who recorded the measurement</param>
        public MeasurementHistoryItem(string value, DateTime recordedAt, string recordedBy)
        {
            Value = value;
            RecordedAt = recordedAt;
            RecordedBy = string.IsNullOrWhiteSpace(recordedBy) ? "Unknown" : recordedBy;
            DateTimeDisplay = FormatDateTime(recordedAt);
        }

        /// <summary>
        /// Formats the date/time for display.
        /// If the date is today, shows only the time.
        /// Otherwise, shows the full date and time.
        /// </summary>
        private static string FormatDateTime(DateTime dateTime)
        {
            if (dateTime.Date == DateTime.Today)
            {
                return $"Today at {dateTime:HH:mm}";
            }
            else if (dateTime.Date == DateTime.Today.AddDays(-1))
            {
                return $"Yesterday at {dateTime:HH:mm}";
            }
            else
            {
                return dateTime.ToString("dd MMM yyyy, HH:mm");
            }
        }

        /// <summary>
        /// Creates a formatted display string for the entire history entry.
        /// </summary>
        public string DisplayText => $"{Value} - {DateTimeDisplay} ({RecordedBy})";
    }
}
