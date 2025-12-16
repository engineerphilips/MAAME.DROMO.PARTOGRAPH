using System;
using System.Globalization;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Converters
{
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string textParameter)
            {
                var texts = textParameter.Split('|');
                if (texts.Length == 2)
                {
                    return boolValue ? texts[0].Trim() : texts[1].Trim();
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
