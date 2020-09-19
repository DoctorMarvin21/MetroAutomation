using System;
using System.Globalization;
using System.Windows.Data;

namespace MetroAutomation.ViewModel
{
    public class BooleanToStringConverter : IValueConverter
    {
        public string TrueValue { get; set; }

        public string FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            if (value is bool boolValue)
            {
                result = boolValue ? TrueValue : FalseValue;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
