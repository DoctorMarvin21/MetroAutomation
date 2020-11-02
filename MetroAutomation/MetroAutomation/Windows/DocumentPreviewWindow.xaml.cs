using MahApps.Metro.Controls;
using MetroAutomation.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

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

        public ICommand SaveCommand => new CommandHandler(Save);

        public ICommand CloseCommand => new CommandHandler(Close);

        public FlowDocument Document { get; }

        public string Text { get; }

        private void CopyExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            try
            {
                DataObject dataObject = new DataObject();
                dataObject.SetData(DataFormats.UnicodeText, GetSelectedText());
                dataObject.SetData(DataFormats.Rtf, GetSelectedRtf());

                Clipboard.SetDataObject(dataObject, true);
            }
            catch
            {
            }
        }

        private string GetSelectedText()
        {
            if (RTF.Selection?.Start?.Paragraph?.Parent is TableCell tableCellStart
                && RTF.Selection?.End?.Paragraph?.Parent is TableCell tableCellEnd
                && tableCellStart != tableCellEnd
                && ((tableCellStart.Parent as TableRow)?.Parent as TableRowGroup)?.Parent is Table tableStart
                && ((tableCellEnd.Parent as TableRow)?.Parent as TableRowGroup)?.Parent is Table tableEnd
                && tableStart == tableEnd)
            {
                List<string[]> selectedRows = new List<string[]>();

                foreach (var groupRow in tableStart.RowGroups)
                {
                    foreach (var row in groupRow.Rows)
                    {
                        List<string> selectedCells = new List<string>();

                        foreach (var cell in row.Cells)
                        {
                            if (IsSelected(cell, RTF.Selection))
                            {
                                TextRange range = new TextRange(cell.ContentStart, cell.ContentEnd);
                                selectedCells.Add(range.Text);
                            }
                        }

                        if (selectedCells.Count > 0)
                        {
                            selectedRows.Add(selectedCells.ToArray());
                        }
                    }
                }

                return string.Join(Environment.NewLine, selectedRows.Select(x => string.Join('\t', x)));
            }
            else if (RTF.Selection?.Start != null && RTF.Selection?.End != null)
            {
                TextRange range = new TextRange(RTF.Selection?.Start, RTF.Selection?.End);
                return range.Text;
            }
            else
            {
                return string.Empty;
            }
        }

        private bool IsSelected(TableCell tableCell, TextSelection selection)
        {
            return selection.Contains(tableCell.ContentEnd);
        }

        private string GetSelectedRtf()
        {
            using MemoryStream ms = new MemoryStream();
            RTF.Selection.Save(ms, DataFormats.Rtf);
            ms.Position = 0;
            string rtf = Encoding.UTF8.GetString(ms.ToArray());
            return rtf;
        }

        private string GetRtf()
        {
            using MemoryStream ms = new MemoryStream();
            TextRange range = new TextRange(Document.ContentStart, Document.ContentEnd);
            range.Save(ms, DataFormats.Rtf);
            ms.Position = 0;
            string rtf = Encoding.UTF8.GetString(ms.ToArray());
            return rtf;
        }

        private void Save()
        {
            if (Text != null)
            {
                SaveFile(Text, "Текст (*.txt)|*.txt|Все файлы (*.*)|*.*");
            }
            else if (Document != null)
            {
                SaveFile(GetRtf(), "RTF (*.rtf)|*.rtf|Все файлы (*.*)|*.*");
            }

            Close();
        }

        private void SaveFile(string contents, string filter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = filter
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, contents);
                }
                catch
                {

                }
            }
        }
    }
}
