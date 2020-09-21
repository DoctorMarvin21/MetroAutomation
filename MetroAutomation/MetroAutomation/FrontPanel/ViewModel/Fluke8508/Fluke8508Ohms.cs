using MetroAutomation.Calibration;
using System.Linq;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public class Fluke8508OhmsConfiguration : AttachedCommand
    {
        private RangeInfo lastRange;
        private Fluke8508OhmsMode mode;
        private Fluke8508OhmsMode[] availableModes;
        private bool filter;
        private Fluke8508Resolution resolution = Fluke8508Resolution.RESL7;
        private bool fast = true;
        private bool isFilterEnabled;

        public Fluke8508OhmsConfiguration(Function function)
            : base(function)
        {
            if (function.Mode == Calibration.Mode.GetRES2W)
            {
                Wire = Fluke8508Wire.TWO_WR;
            }
            else
            {
                Wire = Fluke8508Wire.FOUR_WR;
            }

            AvailableModes = GetAvailableModes(function.RangeInfo?.Range?.GetNormal(), Wire);
            Mode = AvailableModes[0];
        }

        public Fluke8508Wire Wire { get; }

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

        public bool IsFilterEnabled
        {
            get
            {
                return isFilterEnabled;
            }
            private set
            {
                isFilterEnabled = value;
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

        public Fluke8508OhmsMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
                OnPropertyChanged();
                UpdateFilterEnabled();
            }
        }

        public Fluke8508OhmsMode[] AvailableModes
        {
            get
            {
                return availableModes;
            }
            private set
            {
                availableModes = value;
                OnPropertyChanged();
            }
        }

        public override AutoExecuteType AutoExecute => AutoExecuteType.AfterRange;

        public override async Task Process(bool background)
        {
            if (Function.RangeInfo != lastRange)
            {
                lastRange = Function.RangeInfo;

                AvailableModes = GetAvailableModes(lastRange?.Range?.GetNormal(), Wire);

                if (!AvailableModes.Contains(Mode))
                {
                    mode = AvailableModes[0];
                    OnPropertyChanged(nameof(Mode));
                    UpdateFilterEnabled();
                }
            }

            string command;

            switch (mode)
            {
                case Fluke8508OhmsMode.Normal:
                    {
                        command = $"OHMS {lastRange.Alias}, LOI_OFF, ";
                        break;
                    }
                case Fluke8508OhmsMode.NormalLoI:
                    {
                        command = $"OHMS {lastRange.Alias}, LOI_ON, ";
                        break;
                    }
                case Fluke8508OhmsMode.True:
                    {
                        command = $"TRUE_OHMS {lastRange.Alias}, LOI_OFF, ";
                        break;
                    }
                case Fluke8508OhmsMode.TrueLoI:
                    {
                        command = $"TRUE_OHMS {lastRange.Alias}, LOI_ON, ";
                        break;
                    }
                default:
                    {
                        command = $"HIV_OHMS {lastRange.Alias}, ";
                        break;
                    }
            }

            if (mode != Fluke8508OhmsMode.True && mode != Fluke8508OhmsMode.TrueLoI)
            {
                command += $"{(Filter ? "FILT_ON" : "FILT_OFF")}, {Wire}, ";
            }

            command += $"{Resolution}, {(Fast ? "FAST_ON" : "FAST_OFF")};*OPC?";

            await Function.Device.QueryAsync(command, background);
        }

        public override void Reset()
        {
            lastRange = null;
        }

        private void UpdateFilterEnabled()
        {
            IsFilterEnabled = Mode != Fluke8508OhmsMode.True && Mode != Fluke8508OhmsMode.TrueLoI;
        }

        private static Fluke8508OhmsMode[] GetAvailableModes(decimal? range, Fluke8508Wire wire)
        {
            if (wire == Fluke8508Wire.FOUR_WR)
            {
                switch (range)
                {
                    case 2:
                    case 20:
                    case 200:
                    case 2000:
                    case 20000:
                    case 200000:
                    case 2000000:
                        {
                            return new Fluke8508OhmsMode[] { Fluke8508OhmsMode.Normal, Fluke8508OhmsMode.NormalLoI };
                        }
                    case 20000000:
                    case 200000000:
                    case 2000000000:
                        {
                            return new Fluke8508OhmsMode[] { Fluke8508OhmsMode.Normal, Fluke8508OhmsMode.NormalLoI, Fluke8508OhmsMode.High };
                        }
                    case 20000000000:
                        {
                            return new Fluke8508OhmsMode[] { Fluke8508OhmsMode.High };
                        }
                    default:
                        {
                            return new Fluke8508OhmsMode[] { Fluke8508OhmsMode.Normal, Fluke8508OhmsMode.NormalLoI, Fluke8508OhmsMode.True, Fluke8508OhmsMode.TrueLoI, Fluke8508OhmsMode.High };
                        }
                }
            }
            else
            {
                switch (range)
                {
                    case 2:
                    case 20:
                    case 200:
                    case 2000:
                    case 20000:
                        {
                            return new Fluke8508OhmsMode[] { Fluke8508OhmsMode.Normal, Fluke8508OhmsMode.NormalLoI, Fluke8508OhmsMode.True, Fluke8508OhmsMode.TrueLoI };
                        }
                    case 200000:
                    case 2000000:
                        {
                            return new Fluke8508OhmsMode[] { Fluke8508OhmsMode.Normal, Fluke8508OhmsMode.NormalLoI };
                        }
                    case 20000000:
                    case 200000000:
                    case 2000000000:
                        {
                            return new Fluke8508OhmsMode[] { Fluke8508OhmsMode.Normal, Fluke8508OhmsMode.NormalLoI, Fluke8508OhmsMode.High };
                        }
                    case 20000000000:
                        {
                            return new Fluke8508OhmsMode[] { Fluke8508OhmsMode.High };
                        }
                    default:
                        {
                            return new Fluke8508OhmsMode[] { Fluke8508OhmsMode.Normal, Fluke8508OhmsMode.NormalLoI, Fluke8508OhmsMode.True, Fluke8508OhmsMode.TrueLoI, Fluke8508OhmsMode.High };
                        }
                }
            }
        }
    }
}
