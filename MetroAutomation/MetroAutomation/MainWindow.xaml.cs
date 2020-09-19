using MahApps.Metro.Controls;
using System;
using System.Diagnostics;

namespace MetroAutomation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            try
            {
                ViewModel = new MainViewModel(this);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }

            InitializeComponent();
        }

        public MainViewModel ViewModel { get; }
    }
}
