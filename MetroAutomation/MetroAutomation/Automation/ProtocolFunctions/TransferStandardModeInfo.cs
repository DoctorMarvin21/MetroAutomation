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
            new BaseValueInfo(1100, Unit.V, UnitModifier.None),
        };

        private decimal? lastSetDcValue;
        private decimal? dcReferenceValue;
        BaseValueInfo lastTransferStandardRange;

        protected override async Task<bool> BaseProcessFunction(MetroWindow window, DeviceProtocolBlock protocolBlock, DeviceProtocolItem protocolItem, Function baseSetFunction, Function setFunction, Function baseGetFunction, Function getFunction)
        {
            if (setFunction.Mode == Mode.SetACI && setFunction.MultipliedValue.Unit != Unit.V)
            {
                await HintSetAcvValue(window);
                return false;
            }

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

            if (lastSetDcValue != setFunction.MultipliedValue.GetNormal())
            {
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

                if (!await baseSetFunction.Device.ChangeOutput(false, false))
                {
                    return false;
                }

                lastSetDcValue = setFunction.MultipliedValue.GetNormal();
                dcReferenceValue = getDcvFuntion.MultipliedValue.GetNormal();
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

            var value = (setFunction.MultipliedValue.GetNormal() + (dcReferenceValue - acReferenceValue)) / (setFunction.ValueMultiplier?.Value.GetNormal() ?? 1);

            var temp = new BaseValueInfo(value, getFunction.Range.Unit, UnitModifier.None);
            temp.UpdateModifier(getFunction.Range.Modifier);

            getFunction.Components[0].FromValueInfo(temp, true);

            return true;
        }

        private async Task HintSetAcvValue(MetroWindow window)
        {
            await window.ShowMessageAsync(
           "Ошибка", "Установленное значение на калибраторе должно быть в вольтах",
           MessageDialogStyle.Affirmative,
           new MetroDialogSettings
           {
               AffirmativeButtonText = "ОК"
           });
        }

        private async Task<bool> HintTransferStandardRange(MetroWindow window, BaseValueInfo valueInfo)
        {
            var normal = valueInfo.GetNormal();

            BaseValueInfo sutableRange = standardRanges.FirstOrDefault(x => normal <= x.GetNormal());           

            if (lastTransferStandardRange != sutableRange)
            {
                string message;

                if (sutableRange == standardRanges[^1])
                {
                    message = $"Установите на трансферном стандарте диапазон 2,2 В с резистором на диапазон 1000 В";
                }
                else
                {
                    message = $"Установите на трансферном стандарте диапазон {sutableRange}";
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

                if (dialogResult == MessageDialogResult.Affirmative)
                {
                    lastTransferStandardRange = sutableRange;
                }

                return dialogResult == MessageDialogResult.Affirmative;
            }
            else
            {
                return true;
            }
        }

        protected void ResetStandardRange()
        {
            lastTransferStandardRange = null;
        }

        public override void Reset()
        {
            ResetStandardRange();
            lastSetDcValue = null;
            dcReferenceValue = null;
        }
    }

    public class TransferStandardGetModeInfo : TransferStandardSetModeInfo
    {
        protected override async Task<bool> BaseProcessFunction(MetroWindow window, DeviceProtocolBlock protocolBlock, DeviceProtocolItem protocolItem, Function baseSetFunction, Function setFunction, Function baseGetFunction, Function getFunction)
        {
            ResetStandardRange();
            baseGetFunction.Device.ResetRangeAndMode();
            baseSetFunction.Device.ResetRangeAndMode();

            if (!await base.BaseProcessFunction(window, protocolBlock, protocolItem, baseSetFunction, setFunction, baseGetFunction, getFunction))
            {
                return false;
            }

            var calibratedValue = new BaseValueInfo(getFunction.Components[0]);

            baseGetFunction.Device.ResetRangeAndMode();
            baseSetFunction.Device.ResetRangeAndMode();

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

            if (!await ProcessOriginalFunction(baseSetFunction, baseGetFunction))
            {
                return false;
            }

            setFunction.Components[0].FromValueInfo(calibratedValue, true);
            getFunction.FromFunction(baseGetFunction);

            return true;
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
