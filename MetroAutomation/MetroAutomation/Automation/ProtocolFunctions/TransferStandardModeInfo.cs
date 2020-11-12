using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MetroAutomation.Calibration;
using System.Linq;
using System.Threading.Tasks;

namespace MetroAutomation.Automation
{
    public class TransferStandardModeInfo : PairedModeInfo
    {
        private const decimal MaxOutputVoltage = 2;

        private readonly (BaseValueInfo, BaseValueInfo)[] standardRanges = new (BaseValueInfo, BaseValueInfo)[]
        {
            (new BaseValueInfo(22, Unit.V, UnitModifier.Mili), new BaseValueInfo(20, Unit.V, UnitModifier.Mili)),
            (new BaseValueInfo(220, Unit.V, UnitModifier.Mili), new BaseValueInfo(200, Unit.V, UnitModifier.Mili)),
            (new BaseValueInfo(700, Unit.V, UnitModifier.Mili), new BaseValueInfo(600, Unit.V, UnitModifier.Mili)),
            (new BaseValueInfo(2.2m, Unit.V, UnitModifier.None), new BaseValueInfo(2, Unit.V, UnitModifier.None)),
            (new BaseValueInfo(7, Unit.V, UnitModifier.None), new BaseValueInfo(6, Unit.V, UnitModifier.None)),
            (new BaseValueInfo(22, Unit.V, UnitModifier.None), new BaseValueInfo(20, Unit.V, UnitModifier.None)),
            (new BaseValueInfo(70, Unit.V, UnitModifier.None), new BaseValueInfo(60, Unit.V, UnitModifier.None)),
            (new BaseValueInfo(220, Unit.V, UnitModifier.None), new BaseValueInfo(200, Unit.V, UnitModifier.None)),
            (new BaseValueInfo(1000, Unit.V, UnitModifier.None), new BaseValueInfo(1000, Unit.V, UnitModifier.None)),
        };

        private decimal? lastSetDcValue;
        private decimal? dcReferenceValue;
        (BaseValueInfo, BaseValueInfo) lastTransferStandardRange = (null, null);

        protected override async Task<bool> BaseProcessFunction(MetroWindow window, DeviceProtocolBlock protocolBlock, DeviceProtocolItem protocolItem, Function baseSetFunction, Function setFunction, Function baseGetFunction, Function getFunction)
        {
            if (setFunction.Mode == Mode.SetACI && setFunction.MultipliedValue.Unit != Unit.V)
            {
                await HintSetAcvValue(window);
                return false;
            }

            Device measurerDevice;

            if (protocolBlock.Standards.Length == 2)
            {
                measurerDevice = protocolBlock.Standards[1].Device.Device;
            }
            else
            {
                measurerDevice = baseGetFunction.Device;
            }

            if (!measurerDevice.Functions.TryGetValue(Mode.GetDCV, out Function getDcvFuntion))
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

                if (!await HintPressOperate(window, true))
                {
                    return false;
                }

                if (!await getDcvFuntion.Process())
                {
                    if (!await HintPressOperate(window, false))
                    {
                        return false;
                    }

                    return false;
                }

                if (!await HintPressOperate(window, false))
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

            if (protocolBlock.Standards.Length == 2)
            {
                baseGetFunction.FromFunction(getFunction);

                if (baseGetFunction.Device.LastRange != baseGetFunction.RangeInfo)
                {
                    if (!await baseGetFunction.Device.ProcessModeAndRange(baseGetFunction, false))
                    {
                        return false;
                    }
                }
            }

            if (!await baseSetFunction.Process())
            {
                return false;
            }

            if (!await baseSetFunction.Device.ChangeOutput(true, true))
            {
                return false;
            }

            if (!await HintPressOperate(window, true))
            {
                return false;
            }

            if (!await getDcvFuntion.Process())
            {
                if (!await HintPressOperate(window, false))
                {
                    return false;
                }

                return false;
            }

            if (protocolBlock.Standards.Length == 2)
            {
                await baseGetFunction.Process();
                getFunction.FromFunction(baseGetFunction);
            }

            if (!await HintPressOperate(window, false))
            {
                return false;
            }

            if (!await baseSetFunction.Device.ChangeOutput(false, false))
            {
                return false;
            }

            var acReferenceValue = getDcvFuntion.MultipliedValue.GetNormal();

            decimal? value;

            if (dcReferenceValue.HasValue && dcReferenceValue != 0)
            {
                var setValue = setFunction.MultipliedValue.GetNormal();

                var error = (dcReferenceValue - acReferenceValue) / dcReferenceValue;
                value = setValue + setValue * error;
            }
            else
            {
                value = null;
            }

            if (protocolBlock.Standards.Length == 2)
            {
                var temp = new BaseValueInfo(value, setFunction.Components[0].Unit, UnitModifier.None);
                temp.UpdateModifier(setFunction.Components[0].Modifier);
                setFunction.Components[0].FromValueInfo(temp, true);
            }
            else
            {
                var temp = new BaseValueInfo(value, getFunction.Range.Unit, UnitModifier.None);
                temp.UpdateModifier(getFunction.Range.Modifier);

                getFunction.Components[0].FromValueInfo(temp, true);
            }

            return true;
        }

        private async Task<bool> HintPressOperate(MetroWindow window, bool on)
        {
            string operation = on ? "Нажмите" : "Отожмите";

            var dialogResult = await window.ShowMessageAsync(
            $"Сообщение от трансферного стандарта", $"{operation} кнопку \"OPERATE\"",
            MessageDialogStyle.AffirmativeAndNegative,
            new MetroDialogSettings
            {
                AffirmativeButtonText = "ОК",
                NegativeButtonText = "Отмена",
                DefaultButtonFocus = MessageDialogResult.Affirmative
            });

            return dialogResult == MessageDialogResult.Affirmative;
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

            var sutableRange = standardRanges.FirstOrDefault(x => normal <= x.Item2.GetNormal());           

            if (lastTransferStandardRange != sutableRange)
            {
                string message;

                if (sutableRange == standardRanges[^1])
                {
                    message = $"Установите на трансферном стандарте диапазон 2,2 В с резистором на диапазон 1000 В";
                }
                else
                {
                    message = $"Установите на трансферном стандарте диапазон {sutableRange.Item1}";
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

        public override void Reset()
        {
            lastTransferStandardRange = (null, null);
            lastSetDcValue = null;
            dcReferenceValue = null;
        }
    }
}
