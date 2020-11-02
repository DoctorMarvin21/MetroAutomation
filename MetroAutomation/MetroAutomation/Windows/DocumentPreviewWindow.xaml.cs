using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Documents;

namespace MetroAutomation
{
    /// <summary>
    /// Interaction logic for DocumentPreviewWindow.xaml
    /// </summary>
    public partial class DocumentPreviewWindow : MetroWindow
    {
        public DocumentPreviewWindow(string text)
        {
            InitializeComponent();

            RTF.Visibility = Visibility.Collapsed;
            Text = text;
            TXT.Text = text;
        }

        public DocumentPreviewWindow(FlowDocument flowDocument)
        {
            InitializeComponent();

            TXT.Visibility = Visibility.Collapsed;
            Document = flowDocument;
            RTF.Document = flowDocument;
        }

        public FlowDocument Document { get; }

        public string Text { get; }
    }
}
