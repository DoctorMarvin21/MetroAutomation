using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MetroAutomation.ViewModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace MetroAutomation.Editors
{
    public class BaseEditorDialog : MetroWindow
    {
        private readonly object itemCopy;
        private readonly object originaItem;

        public BaseEditorDialog()
        {
            MinHeight = 300;
            MinWidth = 400;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            OkCommand = new CommandHandler(() => DialogResult = true);
            CancelCommand = new CommandHandler(() => DialogResult = false);
        }

        public BaseEditorDialog(object item)
            : this()
        {
            itemCopy = item.BinaryDeepClone();
            originaItem = item;

            OkCommand = new CommandHandler(() => DialogResult = true);
            CancelCommand = new CommandHandler(() => DialogResult = false);
        }

        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }

        protected override async void OnClosing(CancelEventArgs e)
        {
            var focused = FocusManager.GetFocusedElement(this);

            if (focused != this && focused != null)
            {
                Focus();
            }

            if (DialogResult == null)
            {
                if (!originaItem.DeepBinaryEquals(itemCopy))
                {
                    e.Cancel = true;

                    var result = await this.ShowMessageAsync("Сохранить", "Сохранить изменения?",
                       MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                       new MetroDialogSettings { AffirmativeButtonText = "Да", NegativeButtonText = "Нет", FirstAuxiliaryButtonText = "Отмена" });

                    switch (result)
                    {
                        case MessageDialogResult.Affirmative:
                            {
                                DialogResult = true;
                                Close();
                                break;
                            }
                        case MessageDialogResult.Negative:
                            {
                                DialogResult = false;
                                Close();
                                break;
                            }
                    }
                }
                else
                {
                    DialogResult = false;
                }
            }

            base.OnClosing(e);
        }
    }
}
