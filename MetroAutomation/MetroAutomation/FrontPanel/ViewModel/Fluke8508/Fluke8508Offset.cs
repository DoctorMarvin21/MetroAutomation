using MetroAutomation.Calibration;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public class Fluke8508OffsetConfiguration : AttachedCommand
    {
        private bool offset;
        private decimal? offsetValue;

        public Fluke8508OffsetConfiguration(Function function)
            : base(function)
        {
        }

        public bool Offset
        {
            get
            {
                return offset;
            }
            set
            {
                offset = value;
                offsetValue = null;
                OnPropertyChanged();
                _ = Function.Process();
            }
        }

        public override AutoExecuteType AutoExecute => AutoExecuteType.AfterValue;

        public override Task Process()
        {
            if (offset)
            {
                if (offsetValue == null)
                {
                    offsetValue = Function.Value.GetNormal();
                }

                var normal = Function.Components[0].GetNormal();
                Function.Components[0].Value = normal - offsetValue;
            }

            return Task.CompletedTask;
        }

        public override void Reset()
        {
            Offset = false;
        }
    }
}
