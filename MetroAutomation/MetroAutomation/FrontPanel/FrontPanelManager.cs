﻿using LiteDB;
using MetroAutomation.Calibration;
using MetroAutomation.Connection;
using MetroAutomation.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public class FrontPanelManager : INotifyPropertyChanged
    {
        private FrontPanelValueSet openedValueSet;

        public FrontPanelManager(ConnectionManager connectionManager)
        {
            ConnectionManager = connectionManager;
            RefreshFrontPanels();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ConnectionManager ConnectionManager { get; }

        public FrontPanels FrontPanels { get; private set; }

        public ObservableCollection<FrontPanelViewModel> FrontPanelViewModelsLeft { get; }
            = new ObservableCollection<FrontPanelViewModel>();

        public ObservableCollection<FrontPanelViewModel> FrontPanelViewModelsRight { get; }
            = new ObservableCollection<FrontPanelViewModel>();

        public FrontPanelValueSet OpenedValueSet
        {
            get
            {
                return openedValueSet;
            }
            private set
            {
                openedValueSet = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValueSetOpen));
            }
        }

        public bool IsValueSetOpen => OpenedValueSet != null;

        public async void RefreshDevice(DeviceConfiguration configuration)
        {
            await RefreshDevice(configuration, FrontPanelViewModelsLeft);
            await RefreshDevice(configuration, FrontPanelViewModelsRight);
        }

        private async Task RefreshDevice(DeviceConfiguration configuration, ObservableCollection<FrontPanelViewModel> frontPanels)
        {
            var panels = frontPanels.Where(x => x?.Device.ConfigurationID == configuration.ID).ToArray();

            foreach (var panel in panels)
            {
                await panel.Device.Disconnect();
                ConnectionManager.UnloadDevice(panel.Device);
                int panelIndex = frontPanels.IndexOf(panel);

                frontPanels[panelIndex] = null;
                panel.Dispose();

                Device device = new Device(configuration);
                var connection = ConnectionManager.LoadDevice(device);
                await connection.Connect();

                var panelsConfiguration = FrontPanels.ConfigurationFrontPanels[panelIndex];
                frontPanels[panelIndex] = FrontPanelViewModel.GetViewModel(panelsConfiguration.FrontPanelType, connection.Device);
            }
        }

        public async void RefreshFrontPanels()
        {
            await RefreshFrontPanels(FrontPanelViewModelsLeft, FrontPanelPosition.Left);
            await RefreshFrontPanels(FrontPanelViewModelsRight, FrontPanelPosition.Right);
        }

        private async Task RefreshFrontPanels(ObservableCollection<FrontPanelViewModel> frontPanels, FrontPanelPosition position)
        {
            FrontPanels = Load();

            var panels = frontPanels.ToArray();
            frontPanels.Clear();

            foreach (var frontPanel in panels)
            {
                await frontPanel.Device.Disconnect();
                ConnectionManager.UnloadDevice(frontPanel.Device);

                frontPanel.Dispose();
            }

            if (FrontPanels.ConfigurationFrontPanels != null)
            {
                var positionedPanels = FrontPanels.ConfigurationFrontPanels.Where(x => x.Position == position).ToArray();

                foreach (var configuration in positionedPanels)
                {
                    if (configuration.ConfigurationID != 0)
                    {
                        var connection = ConnectionManager.LoadDevice(configuration.ConfigurationID);

                        if (connection != null)
                        {
                            await connection.Connect();
                            frontPanels.Add(FrontPanelViewModel.GetViewModel(configuration.FrontPanelType, connection.Device));
                        }
                    }
                }
            }
        }


        private void FromValueSet(FrontPanelValueSet valueSet)
        {
            if (valueSet.Values != null)
            {
                foreach (var set in valueSet.Values)
                {
                    foreach (var left in FrontPanelViewModelsLeft)
                    {
                        left.ClearProtocols();

                        if (left.Device.ConfigurationID == set.ConfigurationID)
                        {
                            left.FromValueSet(set);
                        }
                    }

                    foreach (var right in FrontPanelViewModelsRight)
                    {
                        right.ClearProtocols();

                        if (right.Device.ConfigurationID == set.ConfigurationID)
                        {
                            right.FromValueSet(set);
                        }
                    }
                }
            }
        }

        private FrontPanelValueSet ToValueSet()
        {
            var values = FrontPanelViewModelsLeft
                .Select(x => x.ToValueSet())
                .Where(x => x != null)
                .Concat(FrontPanelViewModelsRight
                    .Select(x => x.ToValueSet())
                    .Where(x => x != null))
                .GroupBy(x => x.ConfigurationID)
                .Select(x => x.First()).ToArray();

            return new FrontPanelValueSet
            {
                Values = values
            };
        }

        public void SaveOpenedValueSet()
        {
            if (IsValueSetOpen)
            {
                var valueSet = ToValueSet();
                valueSet.ID = OpenedValueSet.ID;
                valueSet.Name = OpenedValueSet.Name;

                LiteDBAdaptor.SaveData(valueSet);

                OpenedValueSet = valueSet;
            }
        }

        public void SaveNewValueSet(string name)
        {
            var valueSet = ToValueSet();
            valueSet.Name = name;

            LiteDBAdaptor.SaveData(valueSet);

            OpenedValueSet = valueSet;
        }

        public void LoadValueSet(int id)
        {
            OpenedValueSet = LiteDBAdaptor.LoadData<FrontPanelValueSet>(id);
            FromValueSet(OpenedValueSet);
        }

        public void Save()
        {
            LiteDBAdaptor.ClearAll<FrontPanels>();
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
