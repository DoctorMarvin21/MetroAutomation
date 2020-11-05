using MetroAutomation.Calibration;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public abstract class Fluke8508DcBaseConfiguration : AttachedCommand
    {
        private bool filter;
        private Fluke8508Resolution resolution = Fluke8508Resolution.RESL7;
        private bool fast = true;

        public Fluke8508DcBaseConfiguration(Function function)
            : base(function)
        {
        }

        public bool Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
                OnPropertyChanged();
            }
        }

        public Fluke8508Resolution Resolution
        {
            get
            {
                return resolution;
            }
            set
            {
                resolution = value;
                OnPropertyChanged();
            }
        }

        public Fluke8508Resolution[] AvailableResolutions { get; }
            = new[] { Fluke8508Resolution.RESL5, Fluke8508Resolution.RESL6, Fluke8508Resolution.RESL7, Fluke8508Resolution.RESL8 };

        public bool Fast
        {
            get
            {
                return fast;
            }
            set
            {
                fast = value;
                OnPropertyChanged();
            }
        }

        public override AutoExecuteType AutoExecute => AutoExecuteType.AfterRange;
    }

    public class Fluke8508DcvConfiguration : Fluke8508DcBaseConfiguration
    {
        private Fluke8508Wire wire = Fluke8508Wire.TWO_WR;

        public Fluke8508DcvConfiguration(Function function)
            : base(function)
        {
        }

        public Fluke8508Wire Wire
        {
            get
            {
                return wire;
            }
            set
            {
                wire = value;
                OnPropertyChanged();
            }
        }

        public Fluke8508Wire[] AvailableWires { get; }
            = new[] { Fluke8508Wire.TWO_WR, Fluke8508Wire.FOUR_WR };

        public override AutoExecuteType AutoExecute => AutoExecuteType.AfterRange;

        public override decimal? GetErrorArgumentValue(string argument)
        {
            return null;
        }

        public override async Task Process(bool background)
        {
            string command = $"DCV {(Filter ? "FILT_ON" : "FILT_OFF")}, {Resolution}, " +
                $"{(Fast ? "FAST_ON" : "FAST_OFF")}, {Wire};*OPC?";
            await Function.Device.QueryAsync(command, background);
        }

        public override void Reset()
        {
        }
    }

    public class Fluke8508DciConfiguration : Fluke8508DcBaseConfiguration
    {
        public Fluke8508DciConfiguration(Function function)
            : base(function)
        {
        }

        public override AutoExecuteType AutoExecute => AutoExecuteType.AfterRange;

        public override decimal? GetErrorArgumentValue(string argument)
        {
            return null;
        }

        public override async Task Process(bool background)
        {
            string command = $"DCI {(Filter ? "FILT_ON" : "FILT_OFF")}, {Resolution}, " +
                $"{(Fast ? "FAST_ON" : "FAST_OFF")};*OPC?";
            await Function.Device.QueryAsync(command, background);
        }

        public override void Reset()
        {
        }
    }
}
