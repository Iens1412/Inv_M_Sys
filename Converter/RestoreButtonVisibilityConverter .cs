using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Inv_M_Sys.Converter
{
    public class RestoreButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string filter && filter == "Deleted Orders"
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}