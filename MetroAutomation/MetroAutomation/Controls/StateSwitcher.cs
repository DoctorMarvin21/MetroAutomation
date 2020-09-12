using MahApps.Metro.Controls;
using System.Windows.Input;

namespace MetroAutomation.Controls
{
    public class StateSwitcher : ToggleSwitch
    {
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
            RiseCommands();

            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                RiseCommands();
            }

            base.OnKeyDown(e);
        }

        private void RiseCommands()
        {
            if (IsOn)
            {
                OffCommand?.Execute(CommandParameter);
            }
            else
            {
                OnCommand?.Execute(CommandParameter);
            }

            Command?.Execute(CommandParameter);
        }
    }
}
