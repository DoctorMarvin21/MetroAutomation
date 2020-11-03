using MetroAutomation.Calibration;
using System;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public enum Fluke52120AOutput
    {
        HIGH,
        LOW
    }

    public class Fluke52120AAmplificationCommand : AttachedCommand
    {
        public Fluke52120AAmplificationCommand(Fluke52120AFrontPanelViewModel owner, Function function)
            : base(function)
        {
            Owner = owner;
        }

        public Fluke52120AFrontPanelViewModel Owner { get; }

        public Fluke52120AOutput Output { get; set; }

        public Fluke52120AOutput[] Outputs { get; } = new[] { Fluke52120AOutput.LOW, Fluke52120AOutput.HIGH };

        public override AutoExecuteType AutoExecute => AutoExecuteType.AfterValue;

        public override async Task Process(bool background)
        {
            var calibratorDevice = Owner.CalibratorDevice;

            if (!calibratorDevice.IsConnected)
            {
                await calibratorDevice.Connect();

                if (!calibratorDevice.IsConnected)
                {
                    return;
                }
            }

            if (Owner.Device.IsOutputOn)
            {
                await Owner.Device.ChangeOutput(false, true);
            }

            if (calibratorDevice.IsOutputOn)
            {
                await calibratorDevice.ChangeOutput(false, true);
            }

            if (!await UpdateRange())
            {
                return;
            }

            switch (Function.Mode)
            {
                case Mode.SetDCI:
                    {
                        if (calibratorDevice.Functions.TryGetValue(Mode.SetDCV, out Function function))
                        {
                            await UpdateValue(function, function.Components[0]);
                            break;
                        }

                        break;
                    }
                case Mode.SetACI:
                    {
                        if (calibratorDevice.Functions.TryGetValue(Mode.SetACV, out Function function))
                        {
                            function.Components[1].FromValueInfo(Function.Components[1], true);
                            await UpdateValue(function, function.Components[0]);
                            break;
                        }

                        break;
                    }
                case Mode.SetDCP:
                    {
                        if (calibratorDevice.Functions.TryGetValue(Mode.SetDCV_DCV, out Function function))
                        {
                            function.Components[0].FromValueInfo(Function.Components[0], true);
                            await UpdateValue(function, function.Components[1]);
                            break;
                        }

                        break;
                    }
                case Mode.SetACP:
                    {
                        if (calibratorDevice.Functions.TryGetValue(Mode.SetACV_ACV, out Function function))
                        {
                            function.Components[0].FromValueInfo(Function.Components[0], true);
                            function.Components[2].FromValueInfo(Function.Components[2], true);
                            function.Components[3].FromValueInfo(Function.Components[3], true);
                            await UpdateValue(function, function.Components[1]);
                            break;
                        }

                        break;
                    }
            }
        }

        public async Task<bool> UpdateOutput()
        {
            return await Function.Device.QueryAction($"OUTP:TERM {Output}", false);
        }

        public async Task<bool> UpdateRange()
        {
            int? range = GetRange();

            if (range.HasValue)
            {
                return await Function.Device.QueryAction($"CURR:RANG {range}", false);
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> UpdateValue(Function function, ValueInfo valueInfo)
        {
            decimal? normal = GetCurrentValueInfo().GetNormal();
            int? range = GetRange();

            if (normal.HasValue && range.HasValue)
            {
                if (range == 120)
                {
                    normal /= 100;
                }
                else if (range == 20)
                {
                    normal /= 10;
                }

                BaseValueInfo temp = new BaseValueInfo(normal, Unit.V, UnitModifier.None);
                temp.AutoModifier();

                valueInfo.FromValueInfo(temp, true);

                return await function.Process();
            }
            else
            {
                return false;
            }
        }

        public ValueInfo GetCurrentValueInfo()
        {
            switch (Function.Mode)
            {
                case Mode.SetDCI:
                case Mode.SetACI:
                    {
                        return Function.Components[0];
                    }
                default:
                    {
                        return Function.Components[1];
                    }
            }
        }

        private int? GetRange()
        {
            var normal = GetCurrentValueInfo().GetNormal();

            if (normal.HasValue)
            {
                if (normal > 20 && Output == Fluke52120AOutput.LOW)
                {
                    return null;
                }

                if (normal > 120)
                {
                    return null;
                }

                int range;

                if (normal > 20)
                {
                    range = 120;
                }
                else if (normal > 2)
                {
                    range = 20;
                }
                else
                {
                    range = 2;
                }

                return range;
            }
            else
            {
                return null;
            }
        }
    }
}
