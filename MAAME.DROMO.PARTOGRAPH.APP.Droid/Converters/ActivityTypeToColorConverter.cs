using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Globalization;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Converters
{
    public class ActivityTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ActivityType type)
            {
                return type switch
                {
                    ActivityType.Admission => Color.FromArgb("#3B82F6"),        // Blue
                    ActivityType.Delivery => Color.FromArgb("#10B981"),         // Green
                    ActivityType.EmergencyEscalation => Color.FromArgb("#EF4444"), // Red
                    ActivityType.StatusChange => Color.FromArgb("#F59E0B"),     // Amber
                    ActivityType.Assessment => Color.FromArgb("#8B5CF6"),       // Purple
                    ActivityType.Intervention => Color.FromArgb("#EC4899"),     // Pink
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
