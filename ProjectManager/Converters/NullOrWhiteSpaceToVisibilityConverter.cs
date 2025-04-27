using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ProjectManager.Converters
{
    public class NullOrWhiteSpaceToVisibilityConverter : MarkupExtension, IValueConverter
    {
        // Singleton instance
        private static NullOrWhiteSpaceToVisibilityConverter _converter;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value?.ToString())
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported for NullOrWhiteSpaceToVisibilityConverter");
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ??= new NullOrWhiteSpaceToVisibilityConverter();
        }
    }
}