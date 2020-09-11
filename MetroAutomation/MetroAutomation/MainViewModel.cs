using MetroAutomation.Calibration;
using MetroAutomation.Connection;
using MetroAutomation.Editors;
using MetroAutomation.FrontPanel;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MetroAutomation
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            ConnectionManager = new ConnectionManager();

            OpenCommandSetsCommand = new CommandHandler(OpenCommandSets);
            OpenDeviceConfigurationsCommand = new CommandHandler(OpenDeviceConfigurations);
            OpenFrontPanelsEditorCommand = new CommandHandler(OpenFrontPanelsEditor);

            FrontPanels = FrontPanels.Load();

            FrontPanelViewModels = new ObservableCollection<FrontPanelViewModel>();

            FrontPanelViewModels.Add(new Fluke8508FrontPanelViewModel(TestClass.GetTestMeasureDevice()));
            FrontPanelViewModels.Add(new CalibratorFrontPanelViewModel(TestClass.GetTestDevice()));

            StartTest();
        }

        private void StartTest()
        {
            var testConfig = LiteDBAdaptor.LoadData<DeviceConfiguration>(2);
            testConfig.CommandSet = LiteDBAdaptor.LoadData<CommandSet>(testConfig.CommandSetID);
            var testDevice = new Device(testConfig);

            try
            {
                ConnectionManager.LoadDevice(testDevice);
                FrontPanelViewModels.Add(new CalibratorFrontPanelViewModel(testDevice));
            }
            catch
            {

            }
        }

        public FrontPanels FrontPanels { get; }

        public ObservableCollection<FrontPanelViewModel> FrontPanelViewModels { get; set; }

        public ConnectionManager ConnectionManager { get; }

        public ICommand OpenCommandSetsCommand { get; }

        public ICommand OpenDeviceConfigurationsCommand { get; }

        public ICommand OpenFrontPanelsEditorCommand { get; }

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
            itemsWindow.Show();
            itemsWindow.Activate();
        }

        private void OpenFrontPanelsEditor()
        {
            FrontPanels.OnBeginEdit();

            FrontPanelsEditor frontPanelsEditor = new FrontPanelsEditor(FrontPanels);

            if (frontPanelsEditor.ShowDialog() == true)
            {
                // TODO:
                FrontPanels.Save();
            }

            FrontPanels.OnEndEdit();

        }
    }
}
