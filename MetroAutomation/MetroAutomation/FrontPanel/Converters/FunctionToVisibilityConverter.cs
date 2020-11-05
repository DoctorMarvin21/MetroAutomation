using MetroAutomation.Calibration;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MetroAutomation.FrontPanel
{
    public class FunctionToVisibilityConverter : IValueConverter
    {
        public Mode? VisibleMode { get; set; }

        public Direction? VisibleDirection { get; set; }

        public bool? VisibleAutoRange { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Function function)
            {
                if ((!VisibleDirection.HasValue || function.Direction == VisibleDirection)
                    && (!VisibleMode.HasValue || function.Mode == VisibleMode)
                    && (!VisibleAutoRange.HasValue) || function.AutoRange == VisibleAutoRange)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
