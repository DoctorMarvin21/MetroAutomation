using MahApps.Metro.Controls;
using MetroAutomation.Calibration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetroAutomation.Automation
{
    public enum RowLoadMode
    {
        FromProtocol,
        FromCliche,
        FromCopy
    }

    public class PairedModeInfo
    {
        public string Name { get; set; }

        public AutomationMode AutomationMode { get; set; }

        public Mode SourceMode { get; set; }

        public StandardInfo[] Standards { get; set; }

        public virtual DeviceColumnHeader[] GetBlockHeaders(DeviceProtocolBlock block)
        {
            Function setFunction;
            Function getFunction;

            var deviceFunction = GetFunction(block.Owner.Device.Device, SourceMode);
            var standardFunction = GetFunction(block.Standards[0].Device.Device, Standards[0].Mode);

            if (deviceFunction.Direction == Direction.Get)
            {
                getFunction = deviceFunction;
                setFunction = standardFunction;
            }
            else
            {
                getFunction = standardFunction;
                setFunction = deviceFunction;
            }

            List<DeviceColumnHeader> result = new List<DeviceColumnHeader>();

            if (!getFunction.AutoRange)
            {
                result.Add(new DeviceColumnHeader(0, "Диапазон (изм.)"));
            }

            int setComponentsIndex = getFunction.Components.Length + 4;
            foreach (var setComponent in setFunction.Components)
            {
                result.Add(new DeviceColumnHeader(setComponentsIndex, $"{FunctionDescription.GetDescription(setComponent).FullName} (уст.)"));
                setComponentsIndex++;
            }

            if (setFunction.AvailableMultipliers != null)
            {
                result.Add(new DeviceColumnHeader(getFunction.Components.Length + setFunction.Components.Length + 4, "Коэфф. (уст.)"));
            }

            if (setFunction.AvailableMultipliers != null || !FunctionDescription.IsSingleComponent(setFunction) || setFunction.Components.Any(x => x.IsDiscrete))
            {
                result.Add(new DeviceColumnHeader(getFunction.Components.Length + setFunction.Components.Length + 5, $"{FunctionDescription.GetDescription(setFunction.Value).FullName} (уст.)"));
            }

            int getComponentsIndex = 1;

            foreach (var getComponent in getFunction.Components)
            {
                result.Add(new DeviceColumnHeader(getComponentsIndex, $"{FunctionDescription.GetDescription(getComponent).FullName} (изм.)"));
                getComponentsIndex++;
            }

            if (getFunction.AvailableMultipliers != null)
            {
                result.Add(new DeviceColumnHeader(getFunction.Components.Length + 1, "Коэфф. (изм.)"));
            }

            if (getFunction.AvailableMultipliers != null || !FunctionDescription.IsSingleComponent(getFunction))
            {
                result.Add(new DeviceColumnHeader(getFunction.Components.Length + 2, $"{FunctionDescription.GetDescription(getFunction.Value).FullName} (изм.)"));
            }

            result.Add(new DeviceColumnHeader(getFunction.Components.Length + setFunction.Components.Length + 6, "Погрешность"));
            result.Add(new DeviceColumnHeader(getFunction.Components.Length + setFunction.Components.Length + 7, "Допуск"));
            result.Add(new DeviceColumnHeader(getFunction.Components.Length + setFunction.Components.Length + 8, "Статус"));

            return result.ToArray();
        }

        public virtual DeviceProtocolItem GetProtocolRow(DeviceProtocolBlock block)
        {
            DeviceProtocolItem protocolItem = new DeviceProtocolItem();

            List<BaseValueInfo> values = new List<BaseValueInfo>();

            Function baseGetFunction;
            Function baseSetFunction;

            var deviceFunction = GetFunction(block.Owner.Device.Device, SourceMode);
            var standardFunction = GetFunction(block.Standards[0].Device.Device, Standards[0].Mode);

            if (deviceFunction.Direction == Direction.Get)
            {
                baseGetFunction = deviceFunction;
                baseSetFunction = standardFunction;
            }
            else
            {
                baseGetFunction = standardFunction;
                baseSetFunction = deviceFunction;
            }

            Function setFunction = Function.GetFunction(baseSetFunction.Device, baseSetFunction.Mode);
            Function getFunction = Function.GetFunction(baseGetFunction.Device, baseGetFunction.Mode);

            FillBlock(getFunction, values);
            FillBlock(setFunction, values);

            var error = new ErrorValueInfo(getFunction.MultipliedValue, setFunction.MultipliedValue);
            var allowedError = new BaseValueInfo(0, error.Unit, error.Modifier);

            var result = new ResultValueInfo(error, allowedError);

            values.Add(error);
            values.Add(allowedError);
            values.Add(result);

            return new DeviceProtocolItem
            {
                ProcessFunction = (window) => BaseProcessFunction(window, block, protocolItem, baseSetFunction, setFunction, baseGetFunction, getFunction),
                Values = values.ToArray()
            };
        }

        protected virtual async Task<bool> BaseProcessFunction(MetroWindow window, DeviceProtocolBlock protocolBlock, DeviceProtocolItem protocolItem, Function baseSetFunction, Function setFunction, Function baseGetFunction, Function getFunction)
        {
            baseSetFunction.FromFunction(setFunction);
            baseGetFunction.FromFunction(getFunction);

            if (baseSetFunction.Device.LastRange != baseSetFunction.RangeInfo
                || baseGetFunction.Device.LastRange != baseGetFunction.Device.LastRange)
            {
                if (baseSetFunction.Device.IsOutputOn)
                {
                    await baseSetFunction.Device.ChangeOutput(false, true);
                }

                if (baseGetFunction.Device.LastRange != baseGetFunction.RangeInfo)
                {
                    if (!await baseGetFunction.Device.ProcessModeAndRange(baseGetFunction, false))
                    {
                        return false;
                    }
                }

                if (baseSetFunction.Device.LastRange != baseSetFunction.RangeInfo)
                {
                    if (!await baseSetFunction.Device.ProcessModeAndRange(baseSetFunction, false))
                    {
                        return false;
                    }
                }
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

            // Standard pause before measurement
            await Task.Delay(100);

            if (!await baseGetFunction.Process())
            {
                return false;
            }

            getFunction.FromFunction(baseGetFunction);

            return true;
        }

        protected Function GetFunction(Device device, Mode mode)
        {
            if (device.Functions.TryGetValue(mode, out Function function))
            {
                return function;
            }
            else
            {
                // Setting default function to avoid exceptions
                return Function.GetFunction(device, mode);
            }
        }

        private void FillBlock(Function function, List<BaseValueInfo> infos)
        {
            infos.Add(function.Range);

            foreach (ValueInfo component in function.Components)
            {
                infos.Add(component);
            }

            infos.Add(new MultiplierValueInfo(function));

            // Just to display, will not be stored
            infos.Add(function.MultipliedValue);
        }

        public virtual DeviceProtocolItem GetProtocolRowCopy(DeviceProtocolBlock block, DeviceProtocolItem source, RowLoadMode loadMode)
        {
            var newItem = GetProtocolRow(block);

            int sourceIndex = 0;

            for (int i = 0; i < newItem.Values.Length; i++)
            {
                if (newItem.Values[i] is IReadOnlyValueInfo valueInfo)
                {
                    if (valueInfo.IsReadOnly)
                    {
                        if (loadMode == RowLoadMode.FromCopy)
                        {
                            sourceIndex++;
                        }
                    }
                    else if (valueInfo is ValueInfo functionValueInfo
                        && functionValueInfo.Type == ValueInfoType.Component
                        && functionValueInfo.Function.Direction == Direction.Get)
                    {
                        if (loadMode != RowLoadMode.FromCliche)
                        {
                            newItem.Values[i].FromValueInfo(source.Values[sourceIndex], true);
                            sourceIndex++;
                        }
                    }
                    else
                    {
                        newItem.Values[i].FromValueInfo(source.Values[sourceIndex], true);
                        sourceIndex++;
                    }
                }
                else
                {
                    newItem.Values[i].FromValueInfo(source.Values[sourceIndex], true);
                    sourceIndex++;
                }
            }

            newItem.IsSelected = source.IsSelected;

            return newItem;
        }
    }
}
