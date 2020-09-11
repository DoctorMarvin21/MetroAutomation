using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MetroAutomation.Controls
{
    public enum LedState
    {
        Idle,
        Success,
        Fail,
        Warn
    }

    /// <summary>
    /// Interaction logic for Led.xaml
    /// </summary>
    public partial class Led : UserControl
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius),
            typeof(Led));

        public static readonly DependencyProperty LedStateProperty =
            DependencyProperty.Register(
            nameof(LedState), typeof(LedState),
            typeof(Led));

        public static readonly DependencyProperty ShadowColorProperty =
            DependencyProperty.Register(
            nameof(ShadowColor), typeof(Color),
            typeof(Led), new PropertyMetadata(Colors.Transparent));

        public static readonly DependencyProperty ShadowRadiusProperty =
            DependencyProperty.Register(
            nameof(ShadowRadius), typeof(double),
            typeof(Led), new PropertyMetadata(0d));

        public static readonly DependencyProperty CharacterCasingProperty =
            DependencyProperty.Register(
            nameof(CharacterCasing), typeof(CharacterCasing),
            typeof(Led), new PropertyMetadata(CharacterCasing.Normal));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public LedState LedState
        {
            get { return (LedState)GetValue(LedStateProperty); }
            set { SetValue(LedStateProperty, value); }
        }

        public Color ShadowColor
        {
            get { return (Color)GetValue(ShadowColorProperty); }
            set { SetValue(ShadowColorProperty, value); }
        }

        public double ShadowRadius
        {
            get { return (double)GetValue(ShadowRadiusProperty); }
            set { SetValue(ShadowRadiusProperty, value); }
        }

        public CharacterCasing CharacterCasing
        {
            get { return (CharacterCasing)GetValue(CharacterCasingProperty); }
            set { SetValue(CharacterCasingProperty, value); }
        }
    }
}
