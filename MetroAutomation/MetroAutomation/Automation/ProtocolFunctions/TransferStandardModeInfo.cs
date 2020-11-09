using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MetroAutomation.Calibration;
using System.Linq;
using System.Threading.Tasks;

namespace MetroAutomation.Automation
{
    public class TransferStandardSetModeInfo : PairedModeInfo
    {
        private const decimal MaxOutputVoltage = 2;
        private const decimal DividerValue = 100;

        private readonly BaseValueInfo[] standardRanges = new BaseValueInfo[]
        {
            new BaseValueInfo(22, Unit.V, UnitModifier.Mili),
            new BaseValueInfo(220, Unit.V, UnitModifier.Mili),
            new BaseValueInfo(700, Unit.V, UnitModifier.Mili),
            new BaseValueInfo(2.2m, Unit.V, UnitModifier.None),
            new BaseValueInfo(7, Unit.V, UnitModifier.None),
            new BaseValueInfo(22, Unit.V, UnitModifier.None),
            new BaseValueInfo(70, Unit.V, UnitModifier.None),
            new BaseValueInfo(220, Unit.V, UnitModifier.None),
        };

        protected override async Task<bool> BaseProcessFunction(MetroWindow window, DeviceProtocolBlock protocolBlock, DeviceProtocolItem protocolItem, Function baseSetFunction, Function setFunction, Function baseGetFunction, Function getFunction)
        {
            if (!baseGetFunction.Device.Functions.TryGetValue(Mode.GetDCV, out Function getDcvFuntion))
            {
                return false;
            }

            if (!baseSetFunction.Device.Functions.TryGetValue(Mode.SetDCV, out Function setDcvFunction))
            {
                return false;
            }

            if (baseSetFunction.Device.IsOutputOn)
            {
                if (!await baseSetFunction.Device.ChangeOutput(false, false))
                {
                    return false;
                }
            }

            if (!await HintTransferStandardRange(window, setFunction.MultipliedValue))
            {
                return false;
            }

            var dcMeasureRange = getDcvFuntion.Range.DiscreteValues
                .OrderBy(x => x.ActualValue.GetNormal())
                .FirstOrDefault(x => MaxOutputVoltage <= x.ActualValue.GetNormal());

            if (dcMeasureRange == null)
            {
                return false;
            }

            getDcvFuntion.Range.FromValueInfo(dcMeasureRange.Value, true);

            // Start DC block

            setDcvFunction.Components[0].FromValueInfo(setFunction.MultipliedValue, true);

            if (!await getDcvFuntion.Device.ProcessModeAndRange(getDcvFuntion, false))
            {
                return false;
            }

            if (!await setDcvFunction.Device.ProcessModeAndRange(setDcvFunction, false))
            {
                return false;
            }

            if (!await setDcvFunction.Process())
            {
                return false;
            }

            if (!await setDcvFunction.Device.ChangeOutput(true, false))
            {
                return false;
            }

            if (!await getDcvFuntion.Process())
            {
                return false;
            }

            var dcReferenceValue = getDcvFuntion.MultipliedValue.GetNormal();

            // end DC block

            if (!await baseSetFunction.Device.ChangeOutput(false, false))
            {
                return false;
            }

            baseSetFunction.FromFunction(setFunction);

            if (!await baseSetFunction.Process())
            {
                return false;
            }

            if (!await baseSetFunction.Device.ChangeOutput(true, true))
            {
                return false;
            }

            if (!await getDcvFuntion.Process())
            {
                return false;
            }

            var acReferenceValue = getDcvFuntion.MultipliedValue.GetNormal();

            var value = setFunction.Value.GetNormal() + (dcReferenceValue - acReferenceValue);

            var temp = new BaseValueInfo(value, getFunction.Range.Unit, UnitModifier.None);
            temp.UpdateModifier(getFunction.Range.Modifier);

            getFunction.Components[0].FromValueInfo(temp, true);

            return true;
        }

        private async Task<bool> HintTransferStandardRange(MetroWindow window, BaseValueInfo valueInfo)
        {
            var normal = valueInfo.GetNormal();

            bool dividerUsed;

            if (normal > standardRanges[^1].GetNormal())
            {
                normal /= DividerValue;
                dividerUsed = true;
            }
            else
            {
                dividerUsed = false;
            }


            BaseValueInfo sutableRange = standardRanges.FirstOrDefault(x => normal <= x.GetNormal());

            string message = $"Установите на трансферном стандарте диапазон {sutableRange}";

            if (dividerUsed)
            {
                message += $" с делителем напряжения 1/{DividerValue}";
            }

            var dialogResult = await window.ShowMessageAsync(
            $"Сообщение от трансферного стандарта", message,
            MessageDialogStyle.AffirmativeAndNegative,
            new MetroDialogSettings
            {
                AffirmativeButtonText = "ОК",
                NegativeButtonText = "Отмена",
                DefaultButtonFocus = MessageDialogResult.Affirmative
            });

            return dialogResult == MessageDialogResult.Affirmative;
        }
    }

    public class TransferStandardGetModeInfo : TransferStandardSetModeInfo
    {
        protected override async Task<bool> BaseProcessFunction(MetroWindow window, DeviceProtocolBlock protocolBlock, DeviceProtocolItem protocolItem, Function baseSetFunction, Function setFunction, Function baseGetFunction, Function getFunction)
        {
            baseSetFunction.Device.ResetRangeAndMode();
            baseGetFunction.Device.ResetRangeAndMode();

            if (!await base.BaseProcessFunction(window, protocolBlock, protocolItem, baseSetFunction, setFunction, baseGetFunction, getFunction))
            {
                return false;
            }

            var calibratedValue = new BaseValueInfo(getFunction.Components[0]);

            baseSetFunction.FromFunction(setFunction);
            baseGetFunction.FromFunction(getFunction);

            if (baseSetFunction.Device.IsOutputOn)
            {
                await baseSetFunction.Device.ChangeOutput(false, true);
            }

            if (!await HintTransferStandardDisconnect(window))
            {
                return false;
            }

            baseSetFunction.Device.ResetRangeAndMode();
            baseGetFunction.Device.ResetRangeAndMode();

            if (!await baseGetFunction.Device.ProcessModeAndRange(baseGetFunction, false))
            {
                return false;
            }

            if (!await baseSetFunction.Device.ProcessModeAndRange(baseSetFunction, false))
            {
                return false;
            }

            if (!await baseSetFunction.Process())
            {
                return false;
            }

            if (!baseSetFunction.Device.IsOutputOn)
            {
                if (!await baseSetFunction.Device.ChangeOutput(true, true))
                {
                    return false;
                }
            }

            if (!await baseGetFunction.Process())
            {
                return false;
            }

            setFunction.Components[0].FromValueInfo(calibratedValue, true);

            //reset multiplier
            //var multiplier = protocolItem.Values.OfType<MultiplierValueInfo>().FirstOrDefault();
            //multiplier?.FromValueInfo(new BaseValueInfo(1, Unit.None, UnitModifier.None), true);

            getFunction.FromFunction(baseGetFunction);

            return false;
        }

        private async Task<bool> HintTransferStandardDisconnect(MetroWindow window)
        {
            string message = $"Отключите трансферный стандарт";

            var dialogResult = await window.ShowMessageAsync(
            $"Сообщение от трансферного стандарта", message,
            MessageDialogStyle.AffirmativeAndNegative,
            new MetroDialogSettings
            {
                AffirmativeButtonText = "ОК",
                NegativeButtonText = "Отмена",
                DefaultButtonFocus = MessageDialogResult.Affirmative
            });

            return dialogResult == MessageDialogResult.Affirmative;
        }
    }


}
