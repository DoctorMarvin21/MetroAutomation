using MetroAutomation.Calibration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace MetroAutomation.FrontPanel
{
    public class MultipliedValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length == 4 && values[1] is Unit unit && values[2] is UnitModifier modifier)
            {
                decimal? value = values[0] as decimal?;
                ValueMultiplier multiplier = values[3] as ValueMultiplier;

                return ValueInfoUtils.GetTextValue(value * (multiplier?.Multiplier ?? 1), unit, modifier);
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
