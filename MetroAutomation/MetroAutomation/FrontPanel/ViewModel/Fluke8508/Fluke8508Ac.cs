using MetroAutomation.Calibration;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public abstract class Fluke8508AcBaseConfiguration : AttachedCommand
    {
        private Fluke8508Filter filter = Fluke8508Filter.FILT40HZ;
        private Fluke8508Resolution resolution = Fluke8508Resolution.RESL6;
        private Fluke8508Coupling coupling = Fluke8508Coupling.ACCP;

        public Fluke8508AcBaseConfiguration(Function function)
            : base(function)
        {
        }

        public Fluke8508Filter Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
            }
        }

        public Fluke8508Filter[] AvailableFilters { get; }
            = new[] { Fluke8508Filter.FILT100HZ, Fluke8508Filter.FILT40HZ, Fluke8508Filter.FILT10HZ, Fluke8508Filter.FILT1HZ };

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
            = new[] { Fluke8508Resolution.RESL5, Fluke8508Resolution.RESL6 };

        public Fluke8508Coupling Coupling
        {
            get
            {
                return coupling;
            }
            set
            {
                coupling = value;
                OnPropertyChanged();
            }
        }

        public Fluke8508Coupling[] AvailableCouplings { get; }
            = new[] { Fluke8508Coupling.DCCP, Fluke8508Coupling.ACCP };

        public override AutoExecuteType AutoExecute => AutoExecuteType.AfterRange;
    }

    public class Fluke8508AcvConfiguration : Fluke8508AcBaseConfiguration
    {
        private Fluke8508Wire wire = Fluke8508Wire.TWO_WR;

        public Fluke8508AcvConfiguration(Function function)
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
            string command = $"ACV {Filter}, {Resolution}, {Coupling}, {Wire};*OPC?";
            await Function.Device.QueryAsync(command, background);
        }

        public override void Reset()
        {
        }
    }

    public class Fluke8508AciConfiguration : Fluke8508AcBaseConfiguration
    {
        public Fluke8508AciConfiguration(Function function)
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
            string command = $"ACI {Filter}, {Resolution}, {Coupling};*OPC?";
            await Function.Device.QueryAsync(command, background);
        }

        public override void Reset()
        {
        }
    }
}
