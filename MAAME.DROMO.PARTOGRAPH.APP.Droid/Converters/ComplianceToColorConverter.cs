using System;
using System.Globalization;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Converters
{
    public class ComplianceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double compliance)
            {
                if (compliance >= 90)
                    return Color.FromArgb("#10B981"); // Green
                if (compliance >= 75)
                    return Color.FromArgb("#F59E0B"); // Amber
                return Color.FromArgb("#EF4444");      // Red
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
