using System;
using System.Globalization;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Converters
{
    public class GramsToKilogramsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "0 KG";

            decimal grams = 0;

            if (value is int intValue)
                grams = intValue;
            else if (value is double doubleValue)
                grams = (decimal)doubleValue;
            else if (value is decimal decimalValue)
                grams = decimalValue;
            else if (value is float floatValue)
                grams = (decimal)floatValue;
            else
                return "0 KG";

            // Convert grams to kilograms
            decimal kilograms = grams / 1000m;

            // Get decimal places from parameter, default to 2
            int decimalPlaces = 2;
            if (parameter is string paramStr && int.TryParse(paramStr, out int places))
                decimalPlaces = places;

            // Format with specified decimal places
            return $"{kilograms.ToString($"F{decimalPlaces}", culture)} KG";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
