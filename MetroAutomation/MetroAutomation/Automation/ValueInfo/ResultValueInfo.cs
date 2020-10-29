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
                return GetStatus();
            }
            set { }
        }

        private void ErrorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnTextChanged();
        }

        private string GetStatus()
        {
            string status;

            var normalError = error.GetNormal();
            var allowedNormalError = allowedError.GetNormal();

            if (normalError.HasValue && allowedNormalError.HasValue)
            {
                if (normalError <= allowedNormalError)
                {
                    status = "Удовл.";
                    Status = LedState.Success;
                }
                else
                {
                    status = "Не удовл.";
                    Status = LedState.Fail;
                }
            }
            else
            {
                status = "-";
                Status = LedState.Idle;
            }

            return status;
        }
    }
}
