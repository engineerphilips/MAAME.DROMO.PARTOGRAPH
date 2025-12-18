using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public string TrueValue { get; set; } = "Yes";
        public string FalseValue { get; set; } = "No";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueValue : FalseValue;
            }
            return FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue.Equals(TrueValue, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
