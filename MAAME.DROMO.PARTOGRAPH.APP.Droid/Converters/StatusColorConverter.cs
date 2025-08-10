using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LaborStatus status)
            {
                return status switch
                {
                    LaborStatus.Pending => Color.FromArgb("#FFA726"),
                    LaborStatus.Active => Color.FromArgb("#66BB6A"),
                    LaborStatus.Completed => Color.FromArgb("#42A5F5"),
                    LaborStatus.Emergency => Color.FromArgb("#EF5350"),
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
