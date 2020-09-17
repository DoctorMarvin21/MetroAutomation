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

        public Unit Unit { get; set; }

        public UnitModifier Modifier { get; set; }
    }
}
