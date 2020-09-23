using MetroAutomation.Calibration;
using System;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public class Fluke5520Harmonics : AttachedCommand
    {
        public Fluke5520Harmonics(Function function)
            : base(function)
        {
            PrimaryHarmonic = new BaseValueInfo(0, Unit.None, UnitModifier.None);
            SecondaryHarmonic = new BaseValueInfo(0, Unit.None, UnitModifier.None);
        }

        public bool IsOn { get; set; }

        public BaseValueInfo PrimaryHarmonic { get; }

        public BaseValueInfo SecondaryHarmonic { get; }

        public override AutoExecuteType AutoExecute => AutoExecuteType.AfterValue;

        public override async Task Process(bool background)
        {
            if (PrimaryHarmonic.Value > 0)
            {
                if (PrimaryHarmonic.Value != (int)PrimaryHarmonic.Value)
                {
                    PrimaryHarmonic.Value = Math.Round(PrimaryHarmonic.Value.Value, 0);
                }

                await Function.Device.QueryAction($"HARMONIC {PrimaryHarmonic.Value}, PRI; *OPC?", background);
            }

            if (SecondaryHarmonic.Value > 0)
            {
                if (SecondaryHarmonic.Value != (int)SecondaryHarmonic.Value)
                {
                    SecondaryHarmonic.Value = Math.Round(SecondaryHarmonic.Value.Value, 0);
                }

                await Function.Device.QueryAction($"HARMONIC {SecondaryHarmonic.Value}, SEC; *OPC?", background);
            }
        }


    }
}
