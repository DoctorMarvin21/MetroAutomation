using MahApps.Metro.Controls;
using MetroAutomation.Editors;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System.Windows.Input;

namespace MetroAutomation.FrontPanel
{
    /// <summary>
    /// Interaction logic for ValueSetsDialog.xaml
    /// </summary>
    public partial class ValueSetsDialog : MetroWindow
    {
        public ValueSetsDialog()
        {
            ViewModel = new EditableItemsViewModel(LiteDBAdaptor.GetNames<FrontPanelValueSet>());
            ViewModel.Items.GetInstanceDelegate = null;
            ViewModel.Items.GetCopyDelegate = null;

            ViewModel.Items.RemoveDelegate = (item) =>
            {
                LiteDBAdaptor.RemoveData<FrontPanelValueSet>(item.ID);
                return true;
            };

            OkCommand = new CommandHandler(() => DialogResult = true);
            CancelCommand = new CommandHandler(() => DialogResult = false);

            InitializeComponent();
        }

        public EditableItemsViewModel ViewModel { get; }

        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }
    }
}
