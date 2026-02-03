using System;
using System.Globalization;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Converters
{
    /// <summary>
    /// Converts a boolean value to an opacity value.
    /// True = full opacity (1.0), False = reduced opacity (0.5 by default).
    /// Parameter can be used to specify custom opacity values: "trueOpacity|falseOpacity"
    /// </summary>
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            double trueOpacity = 0.5;  // Acknowledged items are dimmed
            double falseOpacity = 1.0; // Unacknowledged items are full opacity

            if (parameter is string opacityParameter)
            {
                var opacities = opacityParameter.Split('|');
                if (opacities.Length == 2)
                {
                    if (double.TryParse(opacities[0], out var trueVal))
                        trueOpacity = trueVal;
                    if (double.TryParse(opacities[1], out var falseVal))
                        falseOpacity = falseVal;
                }
            }

            if (value is bool boolValue)
            {
                return boolValue ? trueOpacity : falseOpacity;
            }

            return falseOpacity;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
