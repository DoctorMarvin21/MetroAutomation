using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MetroAutomation.Automation;
using MetroAutomation.Calibration;
using MetroAutomation.Connection;
using MetroAutomation.Editors;
using MetroAutomation.FrontPanel;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MetroAutomation
{
    public class MainViewModel
    {
        public MainViewModel(MetroWindow owningWindow)
        {
            Owner = owningWindow;
            ConnectionManager = new ConnectionManager(this);
            FrontPanelManager = new FrontPanelManager(this);
            ProtocolManager = new DeviceProtocolManager(this);

            OpenCommandSetsCommand = new CommandHandler(OpenCommandSets);
            OpenDeviceConfigurationsCommand = new CommandHandler(OpenDeviceConfigurations);
            OpenFrontPanelsEditorCommand = new AsyncCommandHandler(OpenFrontPanelsEditor);
            OpenDeviceLogsWindowCommand = new CommandHandler(OpenDeviceLogs);
            OpenConnectionManagerCommand = new CommandHandler(OpenConnectionManager);
        }

        public MetroWindow Owner { get; }

        public FrontPanelManager FrontPanelManager { get; }

        public DeviceProtocolManager ProtocolManager { get; }

        public ConnectionManager ConnectionManager { get; }

        public ICommand OpenCommandSetsCommand { get; }

        public ICommand OpenDeviceConfigurationsCommand { get; }

        public IAsyncCommand OpenFrontPanelsEditorCommand { get; }

        public ICommand OpenDeviceLogsWindowCommand { get; }

        public ICommand OpenConnectionManagerCommand { get; }

        private void OpenCommandSets()
        {
            BindableCollection<CommandSet> commandSets = new BindableCollection<CommandSet>(LiteDBAdaptor.LoadAll<CommandSet>());

            EditableItemsViewModel<CommandSet> itemsViewModel = new EditableItemsViewModel<CommandSet>((item) => new CommandSetEditorDialog(item), null);
            EditableItemsWindow itemsWindow = new EditableItemsWindow("Наборы команд", itemsViewModel);
            itemsWindow.ShowDialog();
        }

        private void OpenDeviceConfigurations()
        {
            BindableCollection<DeviceConfiguration> deviceConfigurations = new BindableCollection<DeviceConfiguration>(LiteDBAdaptor.LoadAll<DeviceConfiguration>());

            EditableItemsViewModel<DeviceConfiguration> itemsViewModel = new EditableItemsViewModel<DeviceConfiguration>((item) => new DeviceConfigurationEditorDialog(item), null);
            EditableItemsWindow itemsWindow = new EditableItemsWindow("Конфигурации приборов", itemsViewModel);
            itemsWindow.ShowDialog();
        }

        private async Task OpenFrontPanelsEditor()
        {
            FrontPanelsEditor frontPanelsEditor = new FrontPanelsEditor(FrontPanelManager.FrontPanels);

            if (frontPanelsEditor.ShowDialog() == true)
            {
                FrontPanelManager.Save();
                await RefreshConnections();
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

        private void OpenConnectionManager()
        {
            ConnectionDialog connectionDialog = new ConnectionDialog(ConnectionManager, null);
            connectionDialog.ShowDialog();
        }

        public async Task RefreshConnections()
        {
            var controller = await Owner.ShowProgressAsync("Подготовка", "Подождите, идёт подключение оборудования...");

            await ConnectionManager.DisconnectAndUnloadAllDevices();

            await FrontPanelManager.RefreshFrontPanels();
            await (ProtocolManager.DeviceProtocol?.RefreshDevices() ?? Task.CompletedTask);
            await controller.CloseAsync();
        }
    }
}
