using MetroAutomation.Calibration;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MetroAutomation.FrontPanel
{
    public class RangeDisplayConverter : IValueConverter
    {
        public Mode? VisibleMode { get; set; }

        public Direction? VisibleDirection { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IValueInfo valueInfo)
            {
                if (valueInfo.GetNormal() == 0)
                {
                    return "АВТО";
                }
                else
                {
                    return valueInfo.ToString();
                }
            }
            else
            {
                return "-";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
