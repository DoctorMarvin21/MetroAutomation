using MahApps.Metro.Controls;

namespace MetroAutomation.Editors
{
    /// <summary>
    /// Interaction logic for EditableItemsWindow.xaml
    /// </summary>
    public partial class EditableItemsWindow : MetroWindow
    {
        public EditableItemsWindow(string title, EditableItemsViewModel viewModel)
        {
            Title = title;
            ViewModel = viewModel;

            // TODO: Check dependencies instead
            ViewModel.Items.RemoveDelegate = null;
            InitializeComponent();
        }

        public EditableItemsViewModel ViewModel { get; }
    }
}
