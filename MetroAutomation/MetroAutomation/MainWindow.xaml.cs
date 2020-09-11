using MahApps.Metro.Controls;
using System;

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
                ViewModel = new MainViewModel();
            }
            catch (Exception ex)
            {

            }
            //TestClass.DatabaseTests();
            InitializeComponent();
        }

        public MainViewModel ViewModel { get; }
    }
}
