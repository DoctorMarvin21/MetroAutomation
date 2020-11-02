using MetroAutomation.Calibration;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MetroAutomation.Automation
{
    public static class ReportGenerator
    {
        public static FlowDocument ToDocument(DeviceProtocol protocol, bool includeUnits)
        {
            FlowDocument document = new FlowDocument
            {
                PagePadding = new Thickness(0)
            };

            AddLabel(document, "Счёт", protocol.AccountInfo);
            AddLabel(document, "Номер протокола", protocol.ProtocolNumber);
            AddLabel(document, "Дата работ", protocol.CalibrationDate.ToShortDateString());
            AddLabel(document, "Наименование", protocol.Name);
            AddLabel(document, "Тип", protocol.Type);
            AddLabel(document, "ГРСИ", protocol.Grsi);
            AddLabel(document, "Заводской номер", protocol.SerialNumber);
            AddLabel(document, "Владелец", protocol.DeviceOwner);
            AddLabel(document, "Статус", protocol.WorkStatus.GetDescription());
            for (int i = 0; i < protocol.BindableBlocks.Count; i++)
            {
                DeviceProtocolBlock block = protocol.BindableBlocks[i];

                var emptyParagraph = new Paragraph()
                {
                    Margin = new Thickness()
                };

                document.Blocks.Add(emptyParagraph);

                var nameParagraph = new Paragraph(new Run(block.Name))
                {
                    Margin = new Thickness()
                };

                document.Blocks.Add(nameParagraph);


                Table table = new Table
                {
                    CellSpacing = 0,
                    Margin = new Thickness(0, 0, 8, 8)
                };

                // TODO: rework this section.
                TableRowGroup rowGroup = new TableRowGroup();
                FillHeader(table, rowGroup, block, includeUnits);
                FillRowGroup(rowGroup, block, includeUnits);

                table.RowGroups.Add(rowGroup);
                document.Blocks.Add(table);
            }

            return document;
        }

        private static void AddLabel(FlowDocument document, string label, string content)
        {
            if (content != null)
            {
                var paragraph = new Paragraph
                {
                    Margin = new Thickness(0, 0, 4, 4)
                };

                paragraph.Inlines.Add(new Run($"{label}: "));
                paragraph.Inlines.Add(new Run(content) { FontWeight = FontWeights.Bold });

                document.Blocks.Add(paragraph);
            }
        }

        private static void FillHeader(Table table, TableRowGroup rowGroup, DeviceProtocolBlock block, bool includeUnits)
        {
            var modeInfo = ProtocolFunctions.GetPairedModeInfo(block);
            var headerInfo = modeInfo.GetBlockHeaders(block);
            var emptyRow = modeInfo.GetProtocolRow(block);

            var headerRow = new TableRow();

            for (int i = 0; i < headerInfo.Length; i++)
            {
                DeviceColumnHeader header = headerInfo[i];
                table.Columns.Add(new TableColumn());
                int leftBorder = i == 0 ? 1 : 0;

                string content;

                if (includeUnits)
                {
                    content = header.Name;
                }
                else
                {
                    var unit = GetPreferredUnit(block, header.Index) ?? emptyRow.Values[header.Index].Unit;
                    var modifier = GetPreferredModifier(block, header.Index) ?? emptyRow.Values[header.Index].Modifier;

                    if (unit != Unit.None)
                    {
                        content = $"{header.Name}, {modifier.GetDescription()}{unit.GetDescription()}";
                    }
                    else
                    {
                        content = header.Name;
                    }
                }

                var cell = new TableCell(new Paragraph(new Run(content)) { Margin = new Thickness(2) })
                {
                    BorderThickness = new Thickness(leftBorder, 1, 1, 1),
                    BorderBrush = Brushes.Black
                };

                headerRow.Cells.Add(cell);
            }

            rowGroup.Rows.Add(headerRow);
        }

        private static void FillRowGroup(TableRowGroup rowGroup, DeviceProtocolBlock block, bool includeUnits)
        {
            var modeInfo = ProtocolFunctions.GetPairedModeInfo(block);
            var headerInfo = modeInfo.GetBlockHeaders(block);

            UnitModifier?[] preffered;

            if (includeUnits)
            {
                preffered = null;
            }
            else
            {
                preffered = headerInfo.Select(x => GetPreferredModifier(block, x.Index)).ToArray();
            }

            foreach (var item in block.BindableItems)
            {
                var row = new TableRow();

                for (int i = 0; i < headerInfo.Length; i++)
                {
                    DeviceColumnHeader column = headerInfo[i];

                    var cellContent = GetTextValue(item.Values[column.Index], includeUnits, preffered?[i]);
                    int leftBorder = i == 0 ? 1 : 0;

                    var cell = new TableCell(new Paragraph(new Run(cellContent)) { Margin = new Thickness(2) })
                    {
                        BorderThickness = new Thickness(leftBorder, 0, 1, 1),
                        BorderBrush = Brushes.Black
                    };
                    row.Cells.Add(cell);
                }

                rowGroup.Rows.Add(row);
            }

        }

        private static UnitModifier? GetPreferredModifier(DeviceProtocolBlock block, int index)
        {
            if (block.BindableItems.Count == 0)
            {
                return null;
            }
            else if (block.BindableItems.All(x=>x.Values[index].Unit == Unit.None))
            {
                return null;
            }
            else
            {
                return block.BindableItems
                    .Select(x => x.Values[index].Modifier)
                    .GroupBy(x => x)
                    .OrderByDescending(x => x.Count())
                    .Select(x => x.Key)
                    .First();
            }
        }

        private static Unit? GetPreferredUnit(DeviceProtocolBlock block, int index)
        {
            if (block.BindableItems.Count == 0)
            {
                return null;
            }
            else
            {
                return block.BindableItems
                    .Select(x => x.Values[index].Unit)
                    .GroupBy(x => x)
                    .OrderByDescending(x => x.Count())
                    .Select(x => x.Key)
                    .First();
            }
        }

        private static string GetTextValue(BaseValueInfo valueInfo, bool includeUnit, UnitModifier? prederredModifiler = null)
        {
            if (includeUnit)
            {
                return valueInfo.TextValue;
            }
            else
            {
                if (prederredModifiler.HasValue && valueInfo.Unit != Unit.None)
                {
                    BaseValueInfo temp = new BaseValueInfo(valueInfo);
                    temp.UpdateModifier(prederredModifiler.Value);
                    return temp.Value?.ToString() ?? "-";
                }
                else
                {
                    return valueInfo.TextValue;
                }
            }
        }
    }
}
