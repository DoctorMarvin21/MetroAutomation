using MetroAutomation.Calibration;
using System.ComponentModel;

namespace MetroAutomation.Automation
{
    public class ResultValueInfo : ReadOnlyValueInfo
    {
        private readonly BaseValueInfo error;
        private readonly BaseValueInfo allowedError;
        private bool isProcessing;

        public ResultValueInfo(BaseValueInfo error, BaseValueInfo allowedError)
        {
            error.PropertyChanged += ErrorPropertyChanged;
            allowedError.PropertyChanged += ErrorPropertyChanged;

            this.error = error;
            this.allowedError = allowedError;
        }

        public bool IsProcessing
        {
            get
            {
                return isProcessing;
            }
            set
            {
                isProcessing = value;
                OnPropertyChanged();
                UpdateText();
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

            if (IsProcessing)
            {
                status = "Измерение...";
            }
            else
            {
                var normalError = error.GetNormal();
                var allowedNormalError = allowedError.GetNormal();

                if (normalError.HasValue && allowedNormalError.HasValue)
                {
                    if (normalError <= allowedNormalError)
                    {
                        status = "Удовл.";
                    }
                    else
                    {
                        status = "Не удовл.";
                    }
                }
                else
                {
                    status = "-";
                }
            }

            return status;
        }
    }
}
