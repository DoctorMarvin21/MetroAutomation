using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System;
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

                foreach (var harmonic in Harmonics)
                {
                    command += $",{harmonic.Number},{harmonic.Amplitude.Value}pct,{harmonic.Phase.Value}";
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
