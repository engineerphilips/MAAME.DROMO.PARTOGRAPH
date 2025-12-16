using System;
using System.Globalization;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string colorParameter)
            {
                var colors = colorParameter.Split('|');
                if (colors.Length == 2)
                {
                    var colorName = boolValue ? colors[0] : colors[1];
                    return GetColorFromName(colorName.Trim());
                }
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private Color GetColorFromName(string colorName)
        {
            return colorName.ToLower() switch
            {
                "green" => Colors.Green,
                "red" => Colors.Red,
                "blue" => Colors.Blue,
                "yellow" => Colors.Yellow,
                "orange" => Colors.Orange,
                "purple" => Colors.Purple,
                "black" => Colors.Black,
                "white" => Colors.White,
                "gray" => Colors.Gray,
                "transparent" => Colors.Transparent,
                _ => Color.FromArgb(colorName) // Try to parse as hex color
            };
        }
    }
}
