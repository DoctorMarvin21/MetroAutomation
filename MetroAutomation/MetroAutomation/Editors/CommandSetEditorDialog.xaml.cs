using MetroAutomation.Calibration;

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
    }
}
