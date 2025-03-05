using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TwoOkNotes.Util
{
    public class FileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string fileName)
            {
                // Handle empty or null values
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return string.Empty;
                }

                // Specific handling for .isf extension
                if (fileName.EndsWith(".isf", StringComparison.OrdinalIgnoreCase))
                {
                    return fileName.Substring(0, fileName.Length - 4);
                }

                // General handling for other extensions
                return Path.GetFileNameWithoutExtension(fileName);
            }
            return value ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string displayName && parameter is string extension)
            {
                // If we're converting back and an extension was provided
                return $"{displayName}{extension}";
            }

            // Default case - just return the display name
            return value?.ToString() ?? string.Empty;
        }
    }
}
