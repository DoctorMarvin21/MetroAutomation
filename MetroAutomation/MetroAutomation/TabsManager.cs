using MetroAutomation.Automation;
using MetroAutomation.FrontPanel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MetroAutomation
{
    public class TabsManager : INotifyPropertyChanged
    {
        private int panelIndex;
        private Visibility frontPanelVisiblity = Visibility.Collapsed;
        private Visibility protocolPanelVisibility = Visibility.Collapsed;

        public TabsManager(MainViewModel mainViewModel)
        {
            Owner = mainViewModel;

            Owner.FrontPanelManager.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FrontPanelManager.IsValueSetOpen))
                {
                    if (Owner.FrontPanelManager.IsValueSetOpen && Owner.FrontPanelManager.IsAnyFrontPanelLoaded)
                    {
                        PanelIndex = 0;
                    }
                }
                else if (e.PropertyName == nameof(FrontPanelManager.IsAnyFrontPanelLoaded))
                {
                    if (Owner.FrontPanelManager.IsAnyFrontPanelLoaded)
                    {
                        FrontPanelVisiblity = Visibility.Visible;
                        PanelIndex = 0;
                    }
                    else
                    {
                        if (Owner.ProtocolManager.IsProtocolLoaded)
                        {
                            PanelIndex = 1;
                        }

                        FrontPanelVisiblity = Visibility.Collapsed;
                    }
                }
            };

            Owner.ProtocolManager.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DeviceProtocolManager.IsProtocolLoaded))
                {
                    if (Owner.ProtocolManager.IsProtocolLoaded)
                    {
                        PanelIndex = 1;
                        ProtocolPanelVisibility = Visibility.Visible;
                    }
                    else
                    {
                        if (Owner.FrontPanelManager.IsAnyFrontPanelLoaded)
                        {
                            PanelIndex = 0;
                        }

                        ProtocolPanelVisibility = Visibility.Collapsed;
                    }
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel Owner { get; }

        public Visibility FrontPanelVisiblity
        {
            get
            {
                return frontPanelVisiblity;
            }
            private set
            {
                frontPanelVisiblity = value;
                OnPropertyChanged();
            }
        }

        public Visibility ProtocolPanelVisibility
        {
            get
            {
                return protocolPanelVisibility;
            }
            private set
            {
                protocolPanelVisibility = value;
                OnPropertyChanged();
            }
        }

        public int PanelIndex
        {
            get
            {
                return panelIndex;
            }
            set
            {
                panelIndex = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
