using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MetroAutomation.Editors;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System.Threading.Tasks;
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

            ViewModel.Items.RemoveDelegate = async (item) =>
            {
                var result = await this.ShowMessageAsync("Удалить",
                    $"Вы действительно хотите удалить шаблон на \"{item.Name}\"? Данное действие невозможно будет отменить.",
                    MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings
                    {
                        DefaultButtonFocus = MessageDialogResult.Negative,
                        AffirmativeButtonText = "Да",
                        NegativeButtonText = "Нет"
                    });

                if (result == MessageDialogResult.Affirmative)
                {
                    LiteDBAdaptor.RemoveData<FrontPanelValueSet>(item.ID);
                }

                return result == MessageDialogResult.Affirmative;
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
