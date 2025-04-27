using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ProjectManager.Converters
{
    public class BooleanToEyeIconConverter : MarkupExtension, IValueConverter
    {
        private static BooleanToEyeIconConverter _instance;

        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolValue)
                return DependencyProperty.UnsetValue;

            bool shouldInvert = parameter != null && bool.TryParse(parameter.ToString(), out var paramInvert)
                ? paramInvert
                : Invert;

            bool visibilityFlag = shouldInvert ? !boolValue : boolValue;

            return visibilityFlag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Visibility visibility)
                return DependencyProperty.UnsetValue;

            bool result = visibility == Visibility.Visible;
            bool shouldInvert = parameter != null && bool.TryParse(parameter.ToString(), out var paramInvert)
                ? paramInvert
                : Invert;

            return shouldInvert ? !result : result;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ??= new BooleanToEyeIconConverter();
        }
    }
}