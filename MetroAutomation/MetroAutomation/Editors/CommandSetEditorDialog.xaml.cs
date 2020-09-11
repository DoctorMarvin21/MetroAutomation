using MetroAutomation.Calibration;
using System.ComponentModel;
using System.Windows;

namespace MetroAutomation.Editors
{
    /// <summary>
    /// Interaction logic for CommandSetEditorDialog.xaml
    /// </summary>
    public partial class CommandSetEditorDialog : BaseEditorDialog, IItemEditor<CommandSet>
    {
        public CommandSetEditorDialog(CommandSet commandSet)
            : base(commandSet)
        {
            Item = commandSet;

            InitializeComponent();
        }

        public CommandSet Item { get; }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult == true)
            {
                ((MainWindow)Application.Current.MainWindow).ViewModel.ConnectionManager.UpdateCommandSets(Item);
            }

            base.OnClosing(e);
        }
    }
}
