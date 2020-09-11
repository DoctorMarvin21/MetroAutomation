using System;
using System.Globalization;
using System.Windows.Data;

namespace MetroAutomation.ViewModel
{
    public class NullPlaceholderConverter : IValueConverter
    {
        public string NullPlaceholder { get; set; } = "Значение не задано";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string textValue && string.IsNullOrWhiteSpace(textValue))
            {
                return NullPlaceholder;
            }
            if (value == null)
            {
                return NullPlaceholder;
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
