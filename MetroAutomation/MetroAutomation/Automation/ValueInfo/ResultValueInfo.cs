using MetroAutomation.Calibration;
using MetroAutomation.Controls;
using System.ComponentModel;

namespace MetroAutomation.Automation
{
    public class ResultValueInfo : ReadOnlyValueInfo
    {
        private readonly BaseValueInfo error;
        private readonly BaseValueInfo allowedError;
        private LedState status;
        private string statusText;

        public ResultValueInfo(BaseValueInfo error, BaseValueInfo allowedError)
        {
            error.PropertyChanged += ErrorPropertyChanged;
            allowedError.PropertyChanged += ErrorPropertyChanged;

            this.error = error;
            this.allowedError = allowedError;
        }

        public LedState Status
        {
            get
            {
                return status;
            }
            private set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        public override string TextValue
        {
            get
            {
                return statusText;
            }
            set
            {
                statusText = value;
                OnTextChanged();
            }
        }

        private void ErrorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            var normalError = error.GetNormal();
            var allowedNormalError = allowedError.GetNormal();

            if (normalError.HasValue && allowedNormalError.HasValue)
            {
                if (normalError <= allowedNormalError)
                {
                    TextValue = "Удовл.";
                    Status = LedState.Success;
                }
                else
                {
                    TextValue = "Не удовл.";
                    Status = LedState.Fail;
                }
            }
            else
            {
                TextValue = "-";
                Status = LedState.Idle;
            }
        }
    }
}
