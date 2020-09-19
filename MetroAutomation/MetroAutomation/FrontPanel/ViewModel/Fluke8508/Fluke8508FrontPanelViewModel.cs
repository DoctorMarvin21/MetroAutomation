using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;

namespace MetroAutomation.FrontPanel
{
    public enum Fluke8508Filter
    {
        [ExtendedDescription("100 Гц", null, "Фильтр 100 Гц")]
        FILT100HZ,
        [ExtendedDescription("40 Гц", null, "Фильтр 40 Гц")]
        FILT40HZ,
        [ExtendedDescription("10 Гц", null, "Фильтр 10 Гц")]
        FILT10HZ,
        [ExtendedDescription("1 Гц", null, "Фильтр 1 Гц")]
        FILT1HZ
    }

    public enum Fluke8508Resolution
    {
        [ExtendedDescription("5.5", null, "Разрядность 5.5")]
        RESL5,
        [ExtendedDescription("6.5", null, "Разрядность 6.5")]
        RESL6,
        [ExtendedDescription("7.5", null, "Разрядность 7.5")]
        RESL7,
        [ExtendedDescription("8.5", null, "Разрядность 8.5")]
        RESL8
    }

    public enum Fluke8508Wire
    {
        [ExtendedDescription("2W", null, "Двухпроводная схема")]
        TWO_WR,
        [ExtendedDescription("4W", null, "Четырехпроводная схема")]
        FOUR_WR
    }

    public enum Fluke8508Coupling
    {
        [ExtendedDescription("DC", null, "Связь по постоянному току")]
        DCCP,
        [ExtendedDescription("AC", null, "Связь по переменному току")]
        ACCP
    }

    public enum Fluke8508OhmsMode
    {
        [ExtendedDescription("NORM", null, "Normal OHMS")]
        Normal,
        [ExtendedDescription("NORM LoI", null, "Normal OHMS, Low Current")]
        NormalLoI,
        [ExtendedDescription("TRUE", null, "True OHMS")]
        True,
        [ExtendedDescription("TRUE LoI", null, "True OHMS, Low Current")]
        TrueLoI,
        [ExtendedDescription("HIGH", null, "High Voltage OHMS")]
        High,
    }

    public class Fluke8508FrontPanelViewModel : FrontPanelViewModel
    {
        public Fluke8508FrontPanelViewModel(Device device)
            : base(device)
        {
            if (device.Functions.TryGetValue(Mode.GetDCV, out var dcv))
            {
                Dcv = new Fluke8508DcvConfiguration(dcv);
                dcv.AttachedCommands.Add(Dcv);
                dcv.AttachedCommands.Add(new Fluke8508OffsetConfiguration(dcv));
            }

            if (device.Functions.TryGetValue(Mode.GetDCI, out var dci))
            {
                Dci = new Fluke8508DciConfiguration(dci);
                dci.AttachedCommands.Add(Dci);
                dci.AttachedCommands.Add(new Fluke8508OffsetConfiguration(dci));
            }

            if (device.Functions.TryGetValue(Mode.GetACV, out var acv))
            {
                Acv = new Fluke8508AcvConfiguration(acv);
                acv.AttachedCommands.Add(Acv);
                acv.AttachedCommands.Add(new Fluke8508OffsetConfiguration(acv));
            }

            if (device.Functions.TryGetValue(Mode.GetACI, out var aci))
            {
                Aci = new Fluke8508AciConfiguration(aci);
                aci.AttachedCommands.Add(Aci);
                aci.AttachedCommands.Add(new Fluke8508OffsetConfiguration(aci));
            }

            if (device.Functions.TryGetValue(Mode.GetRES2W, out var res2w))
            {
                Owhms2w = new Fluke8508OhmsConfiguration(res2w);
                res2w.AttachedCommands.Add(Owhms2w);
            }
        }

        public Fluke8508DcvConfiguration Dcv { get; }

        public Fluke8508DciConfiguration Dci { get; }

        public Fluke8508AcvConfiguration Acv { get; }

        public Fluke8508AciConfiguration Aci { get; }

        public Fluke8508OhmsConfiguration Owhms2w { get; }

        public Fluke8508OhmsConfiguration Owhms4w { get; }

        public override FrontPanelType Type => FrontPanelType.Fluke8508;
    }
}
