using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Converters
{
    public class HighBPConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int bp)
            {
                // For systolic
                if (parameter?.ToString() == "Systolic")
                    return bp >= 140;
                // For diastolic
                else
                    return bp >= 90;
            }
            if (value is string bpString && int.TryParse(bpString, out int bpValue))
            {
                if (parameter?.ToString() == "Systolic")
                    return bpValue >= 140;
                else
                    return bpValue >= 90;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
