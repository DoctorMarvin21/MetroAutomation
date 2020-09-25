using MetroAutomation.ViewModel;

namespace MetroAutomation.Calibration
{
    public enum Unit
    {
        [ExtendedDescription("", "", "Безразмерная")]
        None,
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
        [ExtendedDescription("L", "Гн", "Индуктивность")]
        H,
        [ExtendedDescription("P", "Вт", "Мощность")]
        W,
        [ExtendedDescription("l", "L", "Индуктивная нагрузка")]
        LL,
        [ExtendedDescription("c", "C", "Емкостная нагрузка")]
        CL,
        [ExtendedDescription("φ", "°", "Фазовый угол, °")]
        DA,
        [ExtendedDescription("φ", "рад", "Фазовый угол, рад")]
        RA,
        [ExtendedDescription("S", "См", "Электрическая проводимость")]
        S,
        [ExtendedDescription("T", "°C", "Температура, °C")]
        C,
        [ExtendedDescription("%", "%", "Процент")]
        Per
    }

    public enum UnitModifier
    {
        [ExtendedDescription("p", "п", "Пико")]
        Pico = -12,
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

        public Unit Unit { get; set; }

        public UnitModifier Modifier { get; set; }
    }
}
