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
            ViewModel.Owner = this;

            InitializeComponent();
        }

        public EditableItemsViewModel ViewModel { get; }
    }
}
