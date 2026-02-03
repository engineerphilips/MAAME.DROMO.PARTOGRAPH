using System;
using System.Globalization;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Converters
{
    /// <summary>
    /// Converts a nullable value to a boolean.
    /// Returns true if the value is not null, false otherwise.
    /// </summary>
    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Guid guidValue)
            {
                return guidValue != Guid.Empty;
            }
            return value != null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
