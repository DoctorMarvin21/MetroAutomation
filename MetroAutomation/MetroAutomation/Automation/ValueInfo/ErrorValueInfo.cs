using MetroAutomation.Calibration;
using System;
using System.ComponentModel;

namespace MetroAutomation.Automation
{
    public class ErrorValueInfo : ReadOnlyValueInfo
    {
        private readonly BaseValueInfo value1;
        private readonly BaseValueInfo value2;

        public ErrorValueInfo(BaseValueInfo value1, BaseValueInfo value2)
        {
            value1.PropertyChanged += ValuePropertyChanged;
            value2.PropertyChanged += ValuePropertyChanged;

            this.value1 = value1;
            this.value2 = value2;

            UpdateValue();
        }

        private void ValuePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateValue();
        }

        private void UpdateValue()
        {
            decimal? value;
            UnitModifier modifier;
            Unit unit;

            if (value1.Unit == value2.Unit)
            {
                value = value1.GetNormal() - value2.GetNormal();

                if (value.HasValue)
                {
                    value = Math.Abs(value.Value);
                }

                modifier = (UnitModifier)Math.Min((int)value1.Modifier, (int)value2.Modifier);
                unit = value1.Unit;
            }
            else
            {
                modifier = UnitModifier.None;
                unit = Unit.None;
                value = null;
            }

            value = ValueInfoUtils.UpdateModifier(value, UnitModifier.None, modifier);

            FromValueInfo(new BaseValueInfo(value, unit, modifier), true);
        }
    }
}
