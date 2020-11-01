using System;
using System.Globalization;
using System.Windows.Data;

namespace MetroAutomation.Automation
{
    public class TabHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string name)
            {
                return $"Протокол ({name})";
            }
            else
            {
                return "Протокол";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
