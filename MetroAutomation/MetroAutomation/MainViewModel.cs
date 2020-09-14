using MetroAutomation.Calibration;
using MetroAutomation.Connection;
using MetroAutomation.Editors;
using MetroAutomation.FrontPanel;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MetroAutomation
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            ConnectionManager = new ConnectionManager();
            FrontPanelManager = new FrontPanelManager(ConnectionManager);

            OpenCommandSetsCommand = new CommandHandler(OpenCommandSets);
            OpenDeviceConfigurationsCommand = new CommandHandler(OpenDeviceConfigurations);
            OpenFrontPanelsEditorCommand = new CommandHandler(OpenFrontPanelsEditor);
            OpenDeviceLogsWindowCommand = new CommandHandler(OpenDeviceLogs);
        }

        public FrontPanelManager FrontPanelManager { get; }

        public ConnectionManager ConnectionManager { get; }

        public ICommand OpenCommandSetsCommand { get; }

        public ICommand OpenDeviceConfigurationsCommand { get; }

        public ICommand OpenFrontPanelsEditorCommand { get; }

        public ICommand OpenDeviceLogsWindowCommand { get; }

        private void OpenCommandSets()
        {
            BindableCollection<CommandSet> commandSets = new BindableCollection<CommandSet>(LiteDBAdaptor.LoadAll<CommandSet>());

            EditableItemsViewModel<CommandSet> itemsViewModel = new EditableItemsViewModel<CommandSet>((item) => new CommandSetEditorDialog(item));
            EditableItemsWindow itemsWindow = new EditableItemsWindow("Наборы команд", itemsViewModel);
            itemsWindow.ShowDialog();
        }

        private void OpenDeviceConfigurations()
        {
            BindableCollection<DeviceConfiguration> deviceConfigurations = new BindableCollection<DeviceConfiguration>(LiteDBAdaptor.LoadAll<DeviceConfiguration>());

            EditableItemsViewModel<DeviceConfiguration> itemsViewModel = new EditableItemsViewModel<DeviceConfiguration>((item) => new DeviceConfigurationEditorDialog(item));
            EditableItemsWindow itemsWindow = new EditableItemsWindow("Конфигурации приборов", itemsViewModel);
            itemsWindow.ShowDialog();
        }

        private void OpenFrontPanelsEditor()
        {
            FrontPanelManager.FrontPanels.OnBeginEdit();

            FrontPanelsEditor frontPanelsEditor = new FrontPanelsEditor(FrontPanelManager.FrontPanels);

            if (frontPanelsEditor.ShowDialog() == true)
            {
                FrontPanelManager.Save();
            }

            FrontPanelManager.FrontPanels.OnEndEdit();
        }

        private void OpenDeviceLogs()
        {
            DeviceLogWindow logWindow = Application.Current.Windows
                .OfType<DeviceLogWindow>()
                .FirstOrDefault()
                ?? new DeviceLogWindow(ConnectionManager);

            logWindow.Show();
            logWindow.Activate();
        }
    }
}
