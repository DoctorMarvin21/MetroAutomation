using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.ComponentModel;

namespace MetroAutomation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private bool checkIfSaved = true;
        private bool checkIfConnected = true;

        public MainWindow()
        {
            ViewModel = new MainViewModel(this);
            InitializeComponent();

            Loaded += MainWindowLoaded;
        }

        private async void MainWindowLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await ViewModel.RefreshConnections();
        }

        public MainViewModel ViewModel { get; }

        protected override async void OnClosing(CancelEventArgs e)
        {
            if (checkIfSaved && ViewModel.FrontPanelManager.ShouldBeSaved())
            {
                e.Cancel = true;

                var result = await ViewModel.SaveCurrentValueSet();

                if (result.HasValue)
                {
                    checkIfSaved = false;
                    Close();
                }
            }
            else if (checkIfConnected)
            {
                e.Cancel = true;

                var controller = await this.ShowProgressAsync("Завершение", "Подождите, идёт отключение оборудования...", false);
                await ViewModel.ConnectionManager.DisconnectAndUnloadAllDevices();
                checkIfConnected = false;

                await controller.CloseAsync();

                Close();
            }
            base.OnClosing(e);
        }
    }
}
