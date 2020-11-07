using MetroAutomation.ViewModel;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Input;
using MetroAutomation.Calibration;

namespace MetroAutomation
{
    public enum MeasureInputResult
    {
        Ok,
        Cancel
    }

    /// <summary>
    /// Interaction logic for MeasureInputDialog.xaml
    /// </summary>
    public partial class MeasureInputDialog : CustomDialog
    {
        private decimal? oldValue;

        public MeasureInputDialog(MetroWindow owner, string header, string message, IValueInfo value)
            : base(owner)
        {
            Message = message;
            Value = new BaseValueInfo(value);
            oldValue = Value.GetNormal();

            OkCommand = new CommandHandler(() => SetResult(MeasureInputResult.Ok));
            AcceptValueCommand = new CommandHandler(AcceptValue);
            CancelCommand = new CommandHandler(() => SetResult(MeasureInputResult.Cancel));

            InitializeComponent();

            Title = header;

            ValueInput.ValueTextBox.Loaded += (s, e) => ValueInput.ValueTextBox.Focus();
        }

        public string Message { get; }

        public BaseValueInfo Value { get; set; }

        public MeasureInputResult Result { get; private set; }

        public ICommand AcceptValueCommand { get; }

        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }

        private void AcceptValue()
        {
            var normal = Value.GetNormal();
            if (oldValue == normal)
            {
                SetResult(MeasureInputResult.Ok);
            }
            else
            {
                oldValue = normal;
            }
        }

        private async void SetResult(MeasureInputResult result)
        {
            if (!Value.HasErrors || result == MeasureInputResult.Cancel)
            {
                Result = result;
                await OwningWindow.HideMetroDialogAsync(this);
            }
        }
    }
}
