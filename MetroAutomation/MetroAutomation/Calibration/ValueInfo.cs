using LiteDB;
using MetroAutomation.ViewModel;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MetroAutomation.Calibration
{
    public enum ValueInfoType
    {
        Range,
        Component,
        Value
    }

    public enum Unit
    {
        [ExtendedDescription("F", "Гц", "Частота")]
        Hz,
        [ExtendedDescription("V", "В", "Напряжение")]
        V,
        [ExtendedDescription("I", "А", "Сила тока")]
        A,
        [ExtendedDescription("R", "Ом", "Сопротивление")]
        Ohm,
        [ExtendedDescription("С", "Ф", "Емкость")]
        F,
        [ExtendedDescription("P", "Вт", "Мощность")]
        W,
        [ExtendedDescription("l", "L", "Индуктивная нагрузка")]
        LP,
        [ExtendedDescription("c", "C", "Емкостная нагрузка")]
        CP,
        [ExtendedDescription("φ", "°", "Угол сдвига")]
        DP,
    }

    public enum UnitModifier
    {
        [ExtendedDescription("n", "н", "Нано")]
        Nano = -9,
        [ExtendedDescription("µ", "мк", "Микро")]
        Micro = -6,
        [ExtendedDescription("m", "м", "Милли")]
        Mili = -3,
        [ExtendedDescription("", "", "-")]
        None = 0,
        [ExtendedDescription("k", "к", "Кило")]
        Kilo = 3,
        [ExtendedDescription("M", "М", "Мега")]
        Mega = 6,
        [ExtendedDescription("G", "Г", "Гига")]
        Giga = 9
    }

    public interface IValueInfo
    {
        public decimal? Value { get; set; }

        public decimal? Multiplier { get; set; }

        public Unit Unit { get; set; }

        public UnitModifier Modifier { get; set; }
    }

    [Serializable]
    public class BaseValueInfo : IValueInfo, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private decimal? setValue;
        private decimal? multiplier;
        private Unit unit;
        private UnitModifier modifier;

        [NonSerialized]
        private string textValue;

        public BaseValueInfo()
        {
        }

        public BaseValueInfo(decimal? value, Unit unit, UnitModifier modifier)
        {
            setValue = value;
            this.unit = unit;
            this.modifier = modifier;
            UpdateText();
        }

        public BaseValueInfo(IValueInfo source)
            : this(source.Value, source.Unit, source.Modifier)
        {
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [field: NonSerialized]
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public decimal? Value
        {
            get
            {
                return setValue;
            }
            set
            {
                setValue = value;
                OnPropertyChanged();
                UpdateText();
            }
        }

        public decimal? Multiplier
        {
            get
            {
                return multiplier;
            }
            set
            {
                multiplier = value;
                OnPropertyChanged();
                UpdateText();
            }
        }

        public Unit Unit
        {
            get
            {
                return unit;
            }
            set
            {
                unit = value;
                OnPropertyChanged();
                UpdateText();
            }
        }

        public UnitModifier Modifier
        {
            get
            {
                return modifier;
            }
            set
            {
                modifier = value;
                OnPropertyChanged();
                UpdateText();
            }
        }

        [BsonIgnore]
        public string TextValue
        {
            get
            {
                return textValue ?? ValueInfoUtils.GetTextValue(this);
            }
            set
            {
                textValue = value;
                var info = ValueInfoUtils.FromTextValue(textValue, this);
                FromValueInfo(info, false);

                TextInvalidFormat = info == null;

                OnTextChanged();
            }
        }

        protected bool TextInvalidFormat { get; set; }

        public virtual bool HasErrors => TextInvalidFormat;

        public void FromValueInfo(IValueInfo valueInfo, bool updateText)
        {
            if (valueInfo == null)
            {
                return;
            }

            setValue = valueInfo.Value;
            unit = valueInfo.Unit;
            modifier = valueInfo.Modifier;
            multiplier = valueInfo.Multiplier;

            OnPropertyChanged(string.Empty);

            if (updateText)
            {
                UpdateText();
            }
        }

        public void UpdateText()
        {
            textValue = ValueInfoUtils.GetTextValue(this);
            TextInvalidFormat = false;
            OnTextChanged();
        }

        private void OnTextChanged()
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(TextValue)));
            OnPropertyChanged(nameof(TextValue));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName == nameof(TextValue))
            {
                if (HasErrors)
                {
                    if (TextInvalidFormat)
                    {
                        return new string[] { "Неверный формат текста" };
                    }
                    else
                    {
                        return new string[] { $"Введенное значение \"{ValueInfoUtils.GetTextValue(this)}\" за пределами доступных диапазонов" };
                    }
                }
                else
                {
                    return new string[0];
                }
            }
            else
            {
                return null;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is IValueInfo valueInfo)
            {
                return ValueInfoUtils.AreValuesEqual(this, valueInfo);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.GetNormal().GetHashCode(), Unit);
        }

        public override string ToString()
        {
            return ValueInfoUtils.GetTextValue(this);
        }
    }

    [Serializable]
    public class ValueInfo : BaseValueInfo
    {
        public ValueInfo(ValueInfoType type, Function function, decimal? value, Unit unit, UnitModifier modifier)
            : base(value, unit, modifier)
        {
            Type = type;
            Function = function;
            DiscreteValues = GetDiscreteValues();
            SetInitial();
        }

        public ValueInfo(ValueInfo source)
            : this(source.Type, source.Function, source.Value, source.Unit, source.Modifier)
        {
        }

        public ValueInfoType Type { get; }

        public bool IsReadOnly => Type == ValueInfoType.Value;

        public bool IsDiscrete => DiscreteValues?.Length > 0;

        public override bool HasErrors => !IsReadOnly && (TextInvalidFormat || Function.RangeInfo == null);

        public Function Function { get; }

        public ActualValueInfo[] DiscreteValues { get; }

        private ActualValueInfo[] GetDiscreteValues()
        {
            if (Type == ValueInfoType.Range)
            {
                return Function.Device.Configuration.ModeInfo?
                    .FirstOrDefault(x => x.Mode == Function.Mode)?
                    .Ranges?
                    .Select(x => x.Range)
                    .Where(x => x != null)
                    .Distinct()
                    .OrderBy(x => x.GetNormal())
                    .Select(x => new ActualValueInfo(x))
                    .ToArray();
            }
            else if (Type == ValueInfoType.Component)
            {
                var allowedUnits = FunctionDescription.Components[Function.Mode]
                    .Where(x => x.AllowedUnits.Contains(Unit))
                    .SelectMany(x => x.AllowedUnits)
                    .Distinct()
                    .ToArray();

                return Function.Device.Configuration.ModeInfo?
                    .FirstOrDefault(x => x.Mode == Function.Mode)?
                    .ActualValues?
                    .Where(x => x != null)
                    .OrderBy(x => x.Value.GetNormal())
                    .ToArray();
            }
            else
            {
                return null;
            }
        }

        private void SetInitial()
        {
            if (Type == ValueInfoType.Range && Function.Direction == Direction.Get && DiscreteValues?.Length > 0)
            {
                FromValueInfo(DiscreteValues[0].Value, true);
            }
        }

        public override string ToString()
        {
            return TextValue;
        }
    }
}
