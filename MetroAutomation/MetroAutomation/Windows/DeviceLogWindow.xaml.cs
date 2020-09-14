using MahApps.Metro.Controls;
using MetroAutomation.Connection;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Media;

namespace MetroAutomation
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class DeviceLogWindow : MetroWindow
    {
        public DeviceLogWindow(ConnectionManager connectionManager)
        {
            ConnectionManager = connectionManager;

            InitializeComponent();

            ((INotifyCollectionChanged)LogsGrid.Items).CollectionChanged += NotificationsGridCollectionChanged;
        }

        public ConnectionManager ConnectionManager { get; }

        private void NotificationsGridCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (LogsGrid.Items.Count > 0)
            {
                var border = VisualTreeHelper.GetChild(LogsGrid, 0) as Decorator;
                (border?.Child as ScrollViewer)?.ScrollToEnd();
            }
        }
    }
}
