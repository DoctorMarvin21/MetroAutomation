//using CalibrationTools.Controls;
//using System;
//using System.Globalization;
//using System.Windows.Data;

//namespace CalibrationTools.ViewModel
//{
//    public class BooleanToLedStateConverter : IValueConverter
//    {
//        public LedState TrueState { get; set; } = LedState.Success;

//        public LedState FalseState { get; set; } = LedState.Idle;

//        public LedState NullState { get; set; } = LedState.Fail;

//        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            if (value is bool boolean)
//            {
//                return boolean ? TrueState : FalseState;
//            }
//            else
//            {
//                return NullState;
//            }
//        }

//        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
