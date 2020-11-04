using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MetroAutomation.Model;
using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroAutomation.ViewModel
{
    public class DataObjectCollectionImportExport<TCollection, TSource>
        where TCollection : IDataObject
        where TSource : IDataObject, new()
    {
        private const string Filter = "Файлы EZCal (*.ezc)|*.ezc";
        private readonly MetroWindow window;
        private readonly BindableCollection<TCollection> collection;
        private readonly Func<TSource, TCollection> converter;
        private readonly Action<TSource> onItemUpdated;

        public DataObjectCollectionImportExport(MetroWindow window, BindableCollection<TCollection> collection, Func<TSource, TCollection> converter, Action<TSource> onItemUpdated)
        {
            this.window = window;
            this.collection = collection;
            this.converter = converter;
            this.onItemUpdated = onItemUpdated;

            collection.ImportDelegate = Import;
            collection.ExportDelegate = Export;
        }

        private async Task<TCollection> Import()
        {
            OpenFileDialog openFile = new OpenFileDialog
            {
                Filter = Filter
            };

            if (openFile.ShowDialog(window) == true)
            {
                try
                {
                    var data = File.ReadAllBytes(openFile.FileName);
                    var json = Decompress(data);
                    var guid = LiteDBAdaptor.GetGuid(json);

                    bool updated;

                    if (LiteDBAdaptor.Contains<TSource>(guid))
                    {
                        var original = LiteDBAdaptor.LoadData<TSource>(guid);
                        var dialogResult = await window.ShowMessageAsync("Запись существует",
                            $"Запись \"{original}\" уже существует, перезаписать?",
                            MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                            new MetroDialogSettings { AffirmativeButtonText = "Да", NegativeButtonText = "Нет", FirstAuxiliaryButtonText = "Отмена"
                            });

                        if (dialogResult == MessageDialogResult.Affirmative)
                        {
                            updated = true;
                        }
                        else if (dialogResult == MessageDialogResult.Negative)
                        {
                            updated = false;
                            json = LiteDBAdaptor.UpdateGuild(json);
                        }
                        else
                        {
                            return default;
                        }
                    }
                    else
                    {
                        updated = false;
                    }

                    var importedGuid = LiteDBAdaptor.ImportFromJson<TSource>(json);

                    if (importedGuid.HasValue)
                    {
                        var loaded = LiteDBAdaptor.LoadData<TSource>(importedGuid.Value);

                        if (updated)
                        {
                            onItemUpdated?.Invoke(loaded);
                        }

                        var converted = converter(loaded);
                        var existing = collection.FirstOrDefault(x => x.ID == converted.ID);

                        if (existing != null)
                        {
                            int index = collection.IndexOf(existing);
                            collection[index] = converted;

                            return default;
                        }
                        else
                        {
                            return converted;
                        }
                    }
                    else
                    {
                        await window.ShowMessageAsync("Ошибка", $"Файл \"{openFile.FileName}\" повреждён или имеет неверный формат");
                        return default;
                    }
                }
                catch
                {
                    await window.ShowMessageAsync("Ошибка", $"Не удалось прочитать файл \"{openFile.FileName}\"");
                    return default;
                }
            }
            else
            {
                return default;
            }
        }

        private void Export(TCollection item)
        {
            SaveFileDialog saveFile = new SaveFileDialog
            {
                Filter = Filter,
                FileName = item.ToString()
            };

            if (saveFile.ShowDialog(window) == true)
            {
                try
                {
                    var data = LiteDBAdaptor.ExportToJson<TSource>(item.ID);
                    File.WriteAllBytes(saveFile.FileName, Compress(data));
                }
                catch
                {
                }
            }
        }

        private byte[] Compress(string data)
        {
            using var source = new MemoryStream(Encoding.UTF8.GetBytes(data ?? ""));
            using var target = new MemoryStream();
            using DeflateStream deflateStream = new DeflateStream(target, CompressionMode.Compress, true);
            source.CopyTo(deflateStream);
            deflateStream.Flush();

            return target.ToArray();
        }

        private string Decompress(byte[] data)
        {
            using var source = new MemoryStream(data);
            using var target = new MemoryStream();
            using DeflateStream deflateStream = new DeflateStream(source, CompressionMode.Decompress, true);
            deflateStream.CopyTo(target);
            deflateStream.Flush();

            return Encoding.UTF8.GetString(target.ToArray());
        }
    }
}
