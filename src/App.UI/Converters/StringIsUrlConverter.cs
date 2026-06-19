using System;
using System.Globalization;
using System.Windows.Data;

namespace AppUI.Converters
{
    public class StringIsUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                return str.StartsWith("http://") || str.StartsWith("https://");
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
