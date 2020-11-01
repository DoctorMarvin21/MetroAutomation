using LiteDB;
using MahApps.Metro.Controls.Dialogs;
using MetroAutomation.Connection;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MetroAutomation.FrontPanel
{
    public class FrontPanelManager : INotifyPropertyChanged
    {
        private bool isAnyFrontPanelLoaded;
        private FrontPanelValueSet openedValueSet;

        public FrontPanelManager(MainViewModel owner)
        {
            Owner = owner;

            OpenValueSetCommand = new AsyncCommandHandler(OpenValueSet);
            SaveOpenedValueSetCommand = new CommandHandler(SaveOpenedValueSet);
            SaveAsNewValueSetCommand = new AsyncCommandHandler(SaveAsNewValueSet);
            RenameOpenedValueSetCommand = new AsyncCommandHandler(RenameOpenedValueSet);
            CloseValuseSetCommand = new AsyncCommandHandler(CloseValueSet);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel Owner { get; }

        public FrontPanels FrontPanels { get; private set; }

        public ObservableCollection<FrontPanelViewModel> FrontPanelViewModelsLeft { get; }
            = new ObservableCollection<FrontPanelViewModel>();

        public ObservableCollection<FrontPanelViewModel> FrontPanelViewModelsRight { get; }
            = new ObservableCollection<FrontPanelViewModel>();

        public bool IsAnyFrontPanelLoaded
        {
            get
            {
                return isAnyFrontPanelLoaded;
            }
            private set
            {
                isAnyFrontPanelLoaded = value;
                OnPropertyChanged();
            }
        }

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

        public IAsyncCommand OpenValueSetCommand { get; }

        public ICommand SaveOpenedValueSetCommand { get; }

        public IAsyncCommand RenameOpenedValueSetCommand { get; }

        public IAsyncCommand SaveAsNewValueSetCommand { get; }

        public IAsyncCommand CloseValuseSetCommand { get; }

        public DeviceConnection[] GetUsedConnections()
        {
            return FrontPanelViewModelsLeft
                .Select(x => Owner.ConnectionManager.ConnectionByConfigurationID(x.Device.ConfigurationID))
                .Union(FrontPanelViewModelsRight.Select(x => Owner.ConnectionManager.ConnectionByConfigurationID(x.Device.ConfigurationID)))
                .Where(x => x != null)
                .Distinct()
                .ToArray();
        }

        public async Task RefreshFrontPanels()
        {
            var tempValueSet = ToValueSet();
            FrontPanels = Load();
            IsAnyFrontPanelLoaded = FrontPanels.ConfigurationFrontPanels?.Length > 0;

            await RefreshFrontPanels(FrontPanelViewModelsLeft, FrontPanelPosition.Left);
            await RefreshFrontPanels(FrontPanelViewModelsRight, FrontPanelPosition.Right);

            FromValueSet(tempValueSet);
        }

        private async Task RefreshFrontPanels(ObservableCollection<FrontPanelViewModel> frontPanels, FrontPanelPosition position)
        {
            var panels = frontPanels.ToArray();
            frontPanels.Clear();

            foreach (var frontPanel in panels)
            {
                frontPanel.Dispose();
            }

            if (FrontPanels.ConfigurationFrontPanels != null)
            {
                var positionedPanels = FrontPanels.ConfigurationFrontPanels.Where(x => x.Position == position).ToArray();

                foreach (var configuration in positionedPanels)
                {
                    if (configuration.ConfigurationID != 0)
                    {
                        var connection = Owner.ConnectionManager.LoadDevice(configuration.ConfigurationID);
                        await connection.Connect();
                        frontPanels.Add(FrontPanelViewModel.GetViewModel(configuration.FrontPanelType, connection.Device));
                    }
                }
            }
        }

        public void Lock()
        {
            foreach (var panel in FrontPanelViewModelsLeft)
            {
                panel.BlockRequests = true;
            }

            foreach (var panel in FrontPanelViewModelsRight)
            {
                panel.BlockRequests = true;
            }
        }

        public void Unlock()
        {
            foreach (var panel in FrontPanelViewModelsLeft)
            {
                panel.BlockRequests = false;
                panel.Refresh();
            }

            foreach (var panel in FrontPanelViewModelsRight)
            {
                panel.BlockRequests = false;
                panel.Refresh();
            }
        }

        public void ClearValueSet()
        {
            ClearProtocols();
            OpenedValueSet = null;
        }

        private void ClearProtocols()
        {
            foreach (var left in FrontPanelViewModelsLeft)
            {
                left.ClearProtocols();
            }

            foreach (var right in FrontPanelViewModelsRight)
            {
                right.ClearProtocols();
            }
        }

        private void FromValueSet(FrontPanelValueSet valueSet)
        {
            if (valueSet.Values != null)
            {
                ClearProtocols();

                foreach (var set in valueSet.Values)
                {
                    foreach (var left in FrontPanelViewModelsLeft)
                    {
                        if (left.Device.ConfigurationID == set.ConfigurationID)
                        {
                            left.FromValueSet(set);
                        }
                    }

                    foreach (var right in FrontPanelViewModelsRight)
                    {
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

            if (OpenedValueSet?.Values != null)
            {
                var notPresent = OpenedValueSet
                    .Values
                    .Where(x => values.Count(y => y.ConfigurationID == x.ConfigurationID) == 0)
                    .ToArray();

                if (notPresent.Length > 0)
                {
                    values = values.Concat(notPresent).ToArray();
                }
            }

            return new FrontPanelValueSet
            {
                Values = values
            };
        }

        private void SaveOpenedValueSet()
        {
            if (IsValueSetOpen)
            {
                SaveValueSet(OpenedValueSet.ID, OpenedValueSet.Name);
            }
        }

        private async Task OpenValueSet()
        {
            ValueSetsDialog valueSetsDialog = new ValueSetsDialog();
            if (valueSetsDialog.ShowDialog() == true && valueSetsDialog.ViewModel.Items.IsAnySelected)
            {
                if (await SaveCurrentValueSet())
                {
                    LoadValueSet(valueSetsDialog.ViewModel.Items.SelectedItem.ID);
                }
            }
        }

        private async Task<bool> RenameOpenedValueSet()
        {
            if (IsValueSetOpen)
            {
                return await RenameValueSet(OpenedValueSet.ID);
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
            var name = await Owner.Owner.ShowInputAsync(
            "Сохранить шаблон значений",
            "Введите название шаблона",
            new MetroDialogSettings
            {
                AffirmativeButtonText = "Сохранить",
                NegativeButtonText = "Отмена"
            });

            if (name != null)
            {
                SaveValueSet(id, name);
            }

            return name != null;
        }

        public async Task<bool> SaveCurrentValueSet()
        {
            if (ShouldBeSaved())
            {
                var result = await Owner.Owner.ShowMessageAsync("Сохранить",
                "Сохранить текущий шаблон значений?",
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
                    if (IsValueSetOpen)
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
                            return false;
                        }
                    }
                }
                else if (result == MessageDialogResult.Negative)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private async Task CloseValueSet()
        {
            if (await SaveCurrentValueSet())
            {
                ClearValueSet();
            }
        }


        private void SaveValueSet(int id, string name)
        {
            var valueSet = ToValueSet();
            valueSet.ID = id;
            valueSet.Name = name;

            LiteDBAdaptor.SaveData(valueSet);

            OpenedValueSet = valueSet;
        }

        private void LoadValueSet(int id)
        {
            OpenedValueSet = LiteDBAdaptor.LoadData<FrontPanelValueSet>(id);
            FromValueSet(OpenedValueSet);
        }

        public void Save()
        {
            LiteDBAdaptor.ClearAll<FrontPanels>();
            LiteDBAdaptor.SaveData(FrontPanels);
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

        public bool ShouldBeSaved()
        {
            var valueSet = ToValueSet();

            if (IsValueSetOpen)
            {
                valueSet.ID = OpenedValueSet.ID;
                valueSet.Name = OpenedValueSet.Name;

                return !valueSet.DeepBinaryEquals(OpenedValueSet);
            }
            else
            {
                return valueSet.Values?.Length > 0;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
