using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Globalization;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Converters
{
    public class SeverityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AlertSeverity severity)
            {
                return severity switch
                {
                    AlertSeverity.Info => Color.FromArgb("#3B82F6"),      // Blue
                    AlertSeverity.Warning => Color.FromArgb("#F59E0B"),   // Amber
                    AlertSeverity.Critical => Color.FromArgb("#EF4444"),  // Red
                    AlertSeverity.Emergency => Color.FromArgb("#DC2626"), // Dark Red
                    _ => Colors.Gray
                };
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
