using System;
using System.Globalization;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Converters
{
    /// <summary>
    /// Converts a string value to a boolean.
    /// Returns true if the string is not null or empty, false otherwise.
    /// </summary>
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return !string.IsNullOrWhiteSpace(stringValue);
            }
            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
