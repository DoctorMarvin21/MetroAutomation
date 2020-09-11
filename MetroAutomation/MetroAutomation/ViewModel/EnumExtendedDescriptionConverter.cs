using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace MetroAutomation.ViewModel
{
    public class EnumExtendedDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                DescriptionType descriptionType;
                if (parameter is string typeString)
                {
                    descriptionType = Enum.Parse<DescriptionType>(typeString);
                }
                else
                {
                    descriptionType = DescriptionType.Normal;
                }

                return ExtendedDescriptionAttribute.GetDescription(enumValue, descriptionType);
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
