using LiteDB;
using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MetroAutomation.Calibration
{
    [Serializable]
    public class BaseValueInfo : IValueInfo, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private decimal? setValue;
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
}
