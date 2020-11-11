using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    [Serializable]
    public class HarmonicTone
    {
        public HarmonicTone()
        {
            Number = 2;
            Amplitude = new BaseValueInfo(1, Unit.Per, UnitModifier.None);
            Phase = new BaseValueInfo(0, Unit.DA, UnitModifier.None);
        }

        public uint Number { get; set; }

        public BaseValueInfo Amplitude { get; set; } 

        public BaseValueInfo Phase { get; set; }
    }

    public class Fluke5520Harmonics : AttachedCommand
    {
        public Fluke5520Harmonics(Function function)
            : base(function)
        {
        }

        public bool HarmonicsMode { get; set; }

        public BindableCollection<HarmonicTone> Harmonics { get; }
            = new BindableCollection<HarmonicTone>();

        public override AutoExecuteType AutoExecute => AutoExecuteType.AfterRange;

        public override decimal? GetErrorArgumentValue(string argument)
        {
            return null;
        }

        public override async Task Process(bool background)
        {
            if (Harmonics.Count > 0)
            {
                HarmonicsMode = true;

                await Function.Device.QueryAction("PQ CH; *OPC?", background);

                string command = "CHTONES PRI";

                var temp = Harmonics.OrderBy(x => x.Number).ToArray();

                uint min = Harmonics.Min(x => x.Number);
                uint max = Harmonics.Max(x => x.Number);

                if (min == max)
                {
                    command += $",{Harmonics[0].Number},{Harmonics[0].Amplitude.Value}pct,{Harmonics[0].Phase.Value}";
                }
                else
                {
                    for (uint i = min; i <= max; i++)
                    {
                        var found = Harmonics.FirstOrDefault(x => x.Number == i);

                        if (found != null)
                        {
                            command += $",{found.Number},{found.Amplitude.Value}pct,{found.Phase.Value}";
                        }
                        else
                        {
                            command += $",{i},0pct,0";
                        }
                    }
                }

                command += ";*OPC?";

                await Function.Device.QueryAction(command, background);
            }
            else if (HarmonicsMode)
            {
                await Function.Device.QueryAction("PQ OFF; *OPC?", background);
                HarmonicsMode = false;
            }
        }

        public override void Reset()
        {
            HarmonicsMode = false;
        }
    }
}
