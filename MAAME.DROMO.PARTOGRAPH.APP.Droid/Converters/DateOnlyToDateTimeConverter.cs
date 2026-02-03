using System;
using System.Globalization;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Converters
{
    /// <summary>
    /// Converts between DateOnly and DateTime for use with DatePicker controls.
    /// DatePicker requires DateTime but ViewModels may use DateOnly for semantic clarity.
    /// </summary>
    public class DateOnlyToDateTimeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateOnly dateOnly)
            {
                return dateOnly.ToDateTime(TimeOnly.MinValue);
            }
            return DateTime.Today;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                return DateOnly.FromDateTime(dateTime);
            }
            return DateOnly.FromDateTime(DateTime.Today);
        }
    }
}
