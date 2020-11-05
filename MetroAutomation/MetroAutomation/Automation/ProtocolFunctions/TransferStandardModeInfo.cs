using MahApps.Metro.Controls;
using MetroAutomation.Calibration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MetroAutomation.Automation
{
    public class TransferStandardSetModeInfo : PairedModeInfo
    {
        private const decimal MaxOutputVoltage = 2;
        // 0. Отключить выход калибратора
        // 1. Найти максимальный диапазон трансферного стандарта для значения напряжения AC
        // 2. Всплывающее сообщение, что нужно установить диапазон на трансферном стандарте (использовать делитель 1/100)
        // 3. Установить калибратор в режим DC
        // 4. Установить вольтметр в режим DC на диапазон, содержащий 2 В (можно, конечно, взять уже заданый, но это будет не очень верно)
        // 5. Включить выход калибратора
        // 6. Измерить опорное напряжение DC
        // 7. Выключить выход калибратора
        // 8. Установить калабратор в режим AC
        // 9. Включить выход калибратора
        // 10. Измерить номинальное напряжение
        // 11. Ввести измереное значение как AC + DC - NOMINAL
        // 12. При поверке вольтметра в режиме АС использовать это значение как реально установленное

        protected override async Task<bool> BaseProcessFunction(MetroWindow window, Function baseSetFunction, Function setFunction, Function baseGetFunction, Function getFunction)
        {
            if (!baseGetFunction.Device.Functions.TryGetValue(Mode.GetDCV, out Function getDcvFuntion))
            {
                return false;
            }

            if (!baseSetFunction.Device.Functions.TryGetValue(Mode.SetDCV, out Function setDcvFunction))
            {
                return false;
            }

            if (!await baseSetFunction.Device.ChangeOutput(false, false))
            {
                return false;
            }

            // TODO: message display!

            var dcMeasureRange = getDcvFuntion.Range.DiscreteValues
                .OrderByDescending(x => x.ActualValue.GetNormal())
                .FirstOrDefault(x => x.ActualValue.GetNormal() < MaxOutputVoltage);

            if (dcMeasureRange == null)
            {
                return false;
            }


            getDcvFuntion.Range.FromValueInfo(dcMeasureRange.Value, true);
            setDcvFunction.Components[0].FromValueInfo(setFunction.Components[0], true);

            if (!await setDcvFunction.Device.ProcessModeAndRange(setDcvFunction, false))
            {
                return false;
            }

            if (!await getDcvFuntion.Device.ProcessModeAndRange(getDcvFuntion, false))
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

            var value = baseSetFunction.MultipliedValue.GetNormal() + (dcReferenceValue - acReferenceValue);


            var temp = new BaseValueInfo(value, baseGetFunction.Range.Unit, UnitModifier.None);
            temp.UpdateModifier(baseGetFunction.Range.Modifier);

            baseGetFunction.Components[0].FromValueInfo(temp, true);

            return true;
        }

        private (BaseValueInfo, bool) GetTransferStandardRange(ValueInfo valueInfo)
        {
            throw new NotImplementedException();
        }
    }

    public class TransferStandardGetModeInfo : TransferStandardSetModeInfo
    {
        protected override async Task<bool> BaseProcessFunction(MetroWindow window, Function baseSetFunction, Function setFunction, Function baseGetFunction, Function getFunction)
        {
            if (!await base.BaseProcessFunction(window, baseSetFunction, setFunction, baseGetFunction, getFunction))
            {
                return false;
            }

            var calibratorValue = baseGetFunction.Components[0].GetNormal();

            return false;
        }
    }
}
