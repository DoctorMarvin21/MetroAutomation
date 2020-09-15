using System;

namespace MetroAutomation.Calibration
{
    [Serializable]
    public class ValueText<T>
    {
        public ValueText()
        {
        }

        public ValueText(T value, string text)
        {
            Value = value;
            Text = text;
        }

        public T Value { get; set; }

        public string Text { get; set; }
    }
}
