using MahApps.Metro.Controls;
using MetroAutomation.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace MetroAutomation.Automation
{
    /// <summary>
    /// Interaction logic for ClicheEditorDialog.xaml
    /// </summary>
    public partial class ClicheEditorDialog : MetroWindow
    {
        public ClicheEditorDialog(DeviceProtocolCliche cliche)
        {
            Cliche = cliche.BinaryDeepClone();

            InitializeComponent();
        }

        public DeviceProtocolCliche Cliche { get; }

        public ICommand SaveCommand => new CommandHandler(() => DialogResult = true);

        public ICommand CancelCommand => new CommandHandler(() => DialogResult = false);
    }
}
