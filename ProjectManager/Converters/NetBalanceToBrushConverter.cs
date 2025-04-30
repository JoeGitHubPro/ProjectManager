using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ProjectManager.Converters
{
    public class NetBalanceToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Brushes.Gray;

            if (value is decimal decimalValue)
            {
                if (decimalValue > 0)
                    return Brushes.Green;
                if (decimalValue < 0)
                    return Brushes.Red;
                return Brushes.Black;
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
