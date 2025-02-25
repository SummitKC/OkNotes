using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace TwoOkNotes.Util
{
    public class ColorToBrushConverter : IValueConverter
    {
        // Converts Color to SolidColorBrush
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }
            return DependencyProperty.UnsetValue;
        }

        // Converts SolidColorBrush back to Color (if needed)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                return brush.Color;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}