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
            ConnectionManager = new ConnectionManager();
            FrontPanelManager = new FrontPanelManager(ConnectionManager);

            OpenCommandSetsCommand = new CommandHandler(OpenCommandSets);
            OpenDeviceConfigurationsCommand = new CommandHandler(OpenDeviceConfigurations);
            OpenFrontPanelsEditorCommand = new AsyncCommandHandler(OpenFrontPanelsEditor);
            OpenDeviceLogsWindowCommand = new CommandHandler(OpenDeviceLogs);
            RenameOpenedValueSetCommand = new AsyncCommandHandler(RenameOpenedValueSet);
            OpenConnectionManagerCommand = new CommandHandler(OpenConnectionManager);

            OpenValueSetCommand = new AsyncCommandHandler(OpenValueSet);
            SaveOpenedValueSetCommand = new CommandHandler(SaveOpenedValueSet);
            SaveAsNewValueSetCommand = new AsyncCommandHandler(SaveAsNewValueSet);
            CloseValuseSetCommand = new AsyncCommandHandler(CloseValueSet);

            SetTestDevice();
        }


        private void SetTestDevice()
        {
            DeviceProtocol = new DeviceProtocol
            {
                ConfigurationID = 2
            };

            var testBlock = new DeviceProtocolBlock { Name = "Test Block", AutomationMode = AutomationMode.GetDCV, StandardConfigurationIDs = new[] { 1 } };
            DeviceProtocol.Blocks = new[] { testBlock };

            DeviceProtocol.Initialize(this);
        }

        public MetroWindow Owner { get; }

        public FrontPanelManager FrontPanelManager { get; }

        public ConnectionManager ConnectionManager { get; }

        public DeviceProtocol DeviceProtocol { get; set; }

        public ICommand OpenCommandSetsCommand { get; }

        public ICommand OpenDeviceConfigurationsCommand { get; }

        public IAsyncCommand OpenFrontPanelsEditorCommand { get; }

        public ICommand OpenDeviceLogsWindowCommand { get; }

        public ICommand OpenConnectionManagerCommand { get; }

        public IAsyncCommand OpenValueSetCommand { get; }

        public ICommand SaveOpenedValueSetCommand { get; }

        public IAsyncCommand RenameOpenedValueSetCommand { get; }

        public IAsyncCommand SaveAsNewValueSetCommand { get; }

        public IAsyncCommand CloseValuseSetCommand { get; }

        public void UnloadUnusedDevices()
        {
            var usedConnections = (DeviceProtocol?.GetUsedConnections() ?? new DeviceConnection[0])
                .Union(FrontPanelManager.GetUsedConnections())
                .Distinct()
                .ToArray();

            var fixedConnections = ConnectionManager.Connections.ToArray();

            foreach (var connection in fixedConnections)
            {
                if (!usedConnections.Contains(connection) && !connection.IsConnected)
                {
                    ConnectionManager.UnloadDevice(connection.Device);
                }
            }
        }

        public async Task DisconnectUnusedDevices()
        {
            var usedConnections = (DeviceProtocol?.GetUsedConnections() ?? new DeviceConnection[0])
                .Union(FrontPanelManager.GetUsedConnections())
                .Distinct()
                .ToArray();

            var fixedConnections = ConnectionManager.Connections.ToArray();

            foreach (var connection in fixedConnections)
            {
                if (!usedConnections.Contains(connection))
                {
                    await connection.Device.Disconnect();
                    ConnectionManager.UnloadDevice(connection.Device);
                }
            }
        }

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

        private void SaveOpenedValueSet()
        {
            if (FrontPanelManager.IsValueSetOpen)
            {
                FrontPanelManager.SaveValueSet(FrontPanelManager.OpenedValueSet.ID, FrontPanelManager.OpenedValueSet.Name);
            }
        }

        private async Task OpenValueSet()
        {
            ValueSetsDialog valueSetsDialog = new ValueSetsDialog();
            if (valueSetsDialog.ShowDialog() == true && valueSetsDialog.ViewModel.Items.IsAnySelected)
            {
                if (await SaveCurrentValueSet() != null)
                {
                    FrontPanelManager.LoadValueSet(valueSetsDialog.ViewModel.Items.SelectedItem.ID);
                }
            }
        }

        private async Task<bool> RenameOpenedValueSet()
        {
            if (FrontPanelManager.IsValueSetOpen)
            {
                return await RenameValueSet(FrontPanelManager.OpenedValueSet.ID);
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> SaveAsNewValueSet()
        {
            return await RenameValueSet(0);
        }

        private async Task<bool> RenameValueSet(int id)
        {
            var name = await Owner.ShowInputAsync(
            "Сохранить шаблон",
            "Введите название шаблона",
            new MetroDialogSettings
            {
                AffirmativeButtonText = "Сохранить",
                NegativeButtonText = "Отмена"
            });

            if (name != null)
            {
                FrontPanelManager.SaveValueSet(id, name);
            }

            return name != null;
        }

        public async Task<bool?> SaveCurrentValueSet()
        {
            if (FrontPanelManager.ShouldBeSaved())
            {
                var result = await Owner.ShowMessageAsync("Сохранить",
                "Сохранить текущий шаблон?",
                MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                new MetroDialogSettings
                {
                    DefaultButtonFocus = MessageDialogResult.Affirmative,
                    AffirmativeButtonText = "Да",
                    NegativeButtonText = "Нет",
                    FirstAuxiliaryButtonText = "Отмена"
                });

                if (result == MessageDialogResult.Affirmative)
                {
                    if (FrontPanelManager.IsValueSetOpen)
                    {
                        SaveOpenedValueSet();
                        return true;
                    }
                    else
                    {
                        if (await SaveAsNewValueSet())
                        {
                            return true;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else if (result == MessageDialogResult.Negative)
                {
                    return false;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return true;
            }
        }

        private async Task CloseValueSet()
        {
            if (await SaveCurrentValueSet() != null)
            {
                FrontPanelManager.ClearValueSet();
            }
        }

        public async Task RefreshConnections()
        {
            var controller = await Owner.ShowProgressAsync("Подготовка", "Подождите, идёт подключение оборудования...");
            await FrontPanelManager.RefreshFrontPanels();
            DeviceProtocol?.UpdateDevice();
            await controller.CloseAsync();
        }
    }
}
