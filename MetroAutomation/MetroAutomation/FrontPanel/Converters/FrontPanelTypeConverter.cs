using System;
using System.Globalization;
using System.Windows.Data;

namespace MetroAutomation.FrontPanel
{
    public class FrontPanelTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as FrontPanelViewModel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
