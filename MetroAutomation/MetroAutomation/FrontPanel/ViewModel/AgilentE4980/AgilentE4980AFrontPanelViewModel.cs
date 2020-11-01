using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public enum ImpedanceMode
    {
        [ExtendedDescription("Cp-D", "", "Емкость (Cp) и затухание (D) в параллельном контуре")]
        CPD,
        [ExtendedDescription("Cp-Q", "", "Емкость (Cp) и добротность (Q) в параллельном контуре")]
        CPQ,
        [ExtendedDescription("Cp-G", "", "Емкость (Cp) и активная проводимость (G) в параллельном контуре")]
        CPG,
        [ExtendedDescription("Cp-Rp", "", "Емкость (Cp) и сопротивление (Rp) в параллельном контуре")]
        CPRP,
        [ExtendedDescription("Cs-D", "", "Емкость (Cs) и затухание (D) в последовательном контуре")]
        CSD,
        [ExtendedDescription("Cs-Q", "", "Емкость (Cs) и добротность (Q) в последовательном контуре")]
        CSQ,
        [ExtendedDescription("Cs-Rs", "", "Емкость (Cs) и сопротивление (Rs) в последовательном контуре")]
        CSRS,
        [ExtendedDescription("Lp-D", "", "Индуктивность (Lp) и затухание (D) в параллельном контуре")]
        LPD,
        [ExtendedDescription("Lp-Q", "", "Индуктивность (Lp) и добротность (Q) в параллельном контуре")]
        LPQ,
        [ExtendedDescription("Lp-G", "", "Индуктивность (Lp) и активная проводимость (G) в параллельном контуре")]
        LPG,
        [ExtendedDescription("Lp-Rp", "", "Индуктивность (Lp) и сопротивление (Rp) в параллельном контуре")]
        LPRP,
        [ExtendedDescription("Ls-D", "", "Индуктивность (Ls) и затухание (D) в последовательном контуре")]
        LSD,
        [ExtendedDescription("Ls-Q", "", "Индуктивность (Ls) и добротность (Q) в последовательном контуре")]
        LSQ,
        [ExtendedDescription("Ls-Rs", "", "Индуктивность (Ls) и сопротивление (Rs) в последовательном контуре")]
        LSRS,
        [ExtendedDescription("R-X", "", "Активное (R) и реактивное (X) сопротивление")]
        RX,
        [ExtendedDescription("Z-θd", "", "Полное сопротивление (Z) и фазовый угол в градусах (θd)")]
        ZTD,
        [ExtendedDescription("Z-θr", "", "Полное сопротивление (Z) и фазовый угол в радианах (θr)")]
        ZTR,
        [ExtendedDescription("G-B", "", "Активная (G) и реактивная (B) проводимости")]
        GB,
        [ExtendedDescription("Y-θd", "", "Полная проводимость (Y) и фазовый угол в градусах (θd)")]
        YTD,
        [ExtendedDescription("Y-θr", "", "Полная проводимость (Y) и фазовый угол в радианах (θr)")]
        YTR
    }

    public class AgilentE4980AFrontPanelViewModel : FrontPanelViewModel
    {
        public AgilentE4980AFrontPanelViewModel(Device device)
            : base(device)
        {
            ImpedanceMeasurement = new ImpedanceMeasurement(this)
            {
                CurrentFunction = SelectedFunction
            };

            if (device.Functions.TryGetValue(Mode.GetCAP4W, out Function capacitance))
            {
                capacitance.AttachedCommands.Add(ImpedanceMeasurement);
            }

            if (device.Functions.TryGetValue(Mode.GetRES4W, out Function resistance))
            {
                resistance.AttachedCommands.Add(ImpedanceMeasurement);
            }

            if (device.Functions.TryGetValue(Mode.GetIND4W, out Function inductance))
            {
                inductance.AttachedCommands.Add(ImpedanceMeasurement);
            }

            if (device.Functions.TryGetValue(Mode.GetADM4W, out Function admittance))
            {
                admittance.AttachedCommands.Add(ImpedanceMeasurement);
            }
        }

        public override FrontPanelType Type => FrontPanelType.Agilent4980A;

        public ImpedanceMeasurement ImpedanceMeasurement { get; }

        protected override Task OnFunctionChanged(Function oldFunction, Function newFunction)
        {
            if (!BlockRequests && ImpedanceMeasurement != null)
            {
                ImpedanceMeasurement.CurrentFunction = newFunction;
            }

            return base.OnFunctionChanged(oldFunction, newFunction);
        }
    }
}
