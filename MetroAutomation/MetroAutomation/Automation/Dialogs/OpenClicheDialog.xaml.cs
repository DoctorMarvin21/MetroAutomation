﻿using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MetroAutomation.Automation
{
    /// <summary>
    /// Interaction logic for OpenClicheDialog.xaml
    /// </summary>
    public partial class OpenClicheDialog : MetroWindow
    {
        private string filter;

        public OpenClicheDialog(DeviceProtocolCliche toSave = null)
        {
            ProtocolCliche.GetInstanceDelegate = null;
            ProtocolCliche.GetCopyDelegate = null;
            ProtocolCliche.EditDelegate = Edit;
            ProtocolCliche.RemoveDelegate = Remove;

            InitializeComponent();

            UpdateCollection();

            if (toSave != null)
            {
                SaveEditied(toSave);
            }
        }

        private void SaveEditied(DeviceProtocolCliche cliche)
        {
            ClicheEditorDialog clicheEditor = new ClicheEditorDialog(cliche);

            if (clicheEditor.ShowDialog() == true)
            {
                LiteDBAdaptor.SaveData(clicheEditor.Cliche);
                ProtocolCliche.Add(clicheEditor.Cliche);
                ProtocolCliche.SelectedItem = clicheEditor.Cliche;
            }
        }

        private IDeviceProtocolClicheDisplayed Edit(IDeviceProtocolClicheDisplayed cliche)
        {
            var originalCliche = LiteDBAdaptor.LoadData<DeviceProtocolCliche>(cliche.ID);
            ClicheEditorDialog clicheEditor = new ClicheEditorDialog(originalCliche);

            if (clicheEditor.ShowDialog() == true)
            {
                LiteDBAdaptor.SaveData(clicheEditor.Cliche);
                return clicheEditor.Cliche;
            }
            else
            {
                return cliche;
            }
        }

        private async Task<bool> Remove(IDeviceProtocolClicheDisplayed item)
        {
            var result = await this.ShowMessageAsync("Удалить",
            $"Вы действительно хотите удалить шаблон протокола на \"{item.Name} {item.Type}\"? Данное действие невозможно будет отменить.",
            MessageDialogStyle.AffirmativeAndNegative,
            new MetroDialogSettings
            {
                DefaultButtonFocus = MessageDialogResult.Negative,
                AffirmativeButtonText = "Да",
                NegativeButtonText = "Нет"
            });

            if (result == MessageDialogResult.Affirmative)
            {
                LiteDBAdaptor.RemoveData<DeviceProtocolCliche>(item.ID);
            }

            return result == MessageDialogResult.Affirmative;
        }

        public string Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
                UpdateCollection();
            }
        }

        public BindableCollection<IDeviceProtocolClicheDisplayed> ProtocolCliche { get; }
            = new BindableCollection<IDeviceProtocolClicheDisplayed>();

        private void UpdateCollection()
        {
            ProtocolCliche.Clear();

            var collection = LiteDBAdaptor.SearchProtocolCliche(100, filter);

            foreach (var item in collection)
            {
                ProtocolCliche.Add(item);
            }
        }

        public ICommand ApplyCommand => new CommandHandler(() => { if (ProtocolCliche.SelectedItem != null) DialogResult = true; });

        public ICommand CancelCommand => new CommandHandler(() => DialogResult = false);
    }
}