using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Converters
{
    public class FHRColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int fhr)
            {
                if (fhr >= 110 && fhr <= 160)
                    return Color.FromArgb("#66BB6A"); // Normal - Green
                else if ((fhr >= 100 && fhr < 110) || (fhr > 160 && fhr <= 180))
                    return Color.FromArgb("#FFA726"); // Borderline - Orange
                else
                    return Color.FromArgb("#EF5350"); // Abnormal - Red
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
