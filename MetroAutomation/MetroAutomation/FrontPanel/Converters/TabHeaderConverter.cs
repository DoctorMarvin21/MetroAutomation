using System;
using System.Globalization;
using System.Windows.Data;

namespace MetroAutomation.FrontPanel
{
    public class TabHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string name)
            {
                return $"Виртуальная передняя панель ({name})";
            }
            else
            {
                return "Виртуальная передняя панель";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
