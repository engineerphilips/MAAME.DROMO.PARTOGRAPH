using MAAME.DROMO.PARTOGRAPH.MODEL;
using System;
using System.Globalization;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Converters
{
    public class ActivityTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ActivityType type)
            {
                return type switch
                {
                    ActivityType.Admission => "➕",
                    ActivityType.Delivery => "👶",
                    ActivityType.EmergencyEscalation => "🚨",
                    ActivityType.StatusChange => "🔄",
                    ActivityType.Assessment => "📋",
                    ActivityType.Intervention => "💉",
                    _ => "•"
                };
            }
            return "•";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
