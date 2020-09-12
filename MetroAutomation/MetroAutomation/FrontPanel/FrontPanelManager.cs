using MetroAutomation.Calibration;
using MetroAutomation.Connection;
using MetroAutomation.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MetroAutomation.FrontPanel
{
    public class FrontPanelManager
    {
        public FrontPanelManager(ConnectionManager connectionManager)
        {
            ConnectionManager = connectionManager;
            RefreshFrontPanels();
        }

        public ConnectionManager ConnectionManager { get; }

        public FrontPanels FrontPanels { get; private set; }

        public ObservableCollection<FrontPanelViewModel> FrontPanelViewModels { get; }
            = new ObservableCollection<FrontPanelViewModel>();

        public async void RefreshDevice(DeviceConfiguration configuration)
        {
            var panels = FrontPanelViewModels.Where(x => x?.Device.ConfigurationID == configuration.ID).ToArray();

            foreach (var panel in panels)
            {
                await panel.Device.Disconnect();
                ConnectionManager.UnloadDevice(panel.Device);
                int panelIndex = FrontPanelViewModels.IndexOf(panel);

                FrontPanelViewModels[panelIndex] = null;

                Device device = new Device(configuration);
                var connection = ConnectionManager.LoadDevice(device);
                await connection.Connect();

                var panelsConfiguration = FrontPanels.ConfigurationFrontPanels[panelIndex];
                FrontPanelViewModels[panelIndex] = FrontPanelViewModel.GetViewModel(panelsConfiguration.FrontPanelType, connection.Device);
            }
        }

        public async void RefreshFrontPanels()
        {
            FrontPanels = Load();

            var panels = FrontPanelViewModels.ToArray();
            FrontPanelViewModels.Clear();

            foreach (var frontPanel in panels)
            {
                if (frontPanel != null)
                {
                    await frontPanel.Device.Disconnect();
                    ConnectionManager.UnloadDevice(frontPanel.Device);
                }
            }

            if (FrontPanels.ConfigurationFrontPanels != null)
            {
                foreach (var configuration in FrontPanels.ConfigurationFrontPanels)
                {
                    FrontPanelViewModel panel;

                    if (configuration.ConfigurationID != 0 && configuration.FrontPanelType != FrontPanelType.None)
                    {
                        var connection = ConnectionManager.LoadDevice(configuration.ConfigurationID);

                        if (connection != null)
                        {
                            await connection.Connect();

                            panel = FrontPanelViewModel.GetViewModel(configuration.FrontPanelType, connection.Device);
                        }
                        else
                        {
                            panel = null;
                        }
                    }
                    else
                    {
                        panel = null;
                    }

                    FrontPanelViewModels.Add(panel);
                }
            }
        }

        public void Save()
        {
            LiteDBAdaptor.SaveData(FrontPanels);
            RefreshFrontPanels();
        }

        public static FrontPanels Load()
        {
            try
            {
                return LiteDBAdaptor.LoadData<FrontPanels>(1) ?? new FrontPanels();
            }
            catch
            {
                LiteDBAdaptor.ClearAll<FrontPanels>();
                return new FrontPanels();
            }
        }
    }
}
