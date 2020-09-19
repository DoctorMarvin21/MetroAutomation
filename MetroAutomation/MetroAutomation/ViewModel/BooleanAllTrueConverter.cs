using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace MetroAutomation.ViewModel
{
    public class BooleanAllTrueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.All(x => x is bool boolean && boolean);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
