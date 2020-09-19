using MetroAutomation.Calibration;
using System.Collections.Generic;

namespace MetroAutomation.FrontPanel
{
    public static class ImpedanceHelper
    {
        public static Dictionary<ImpedanceMode, (Unit, string)> SecondUnitInfo { get; } = new Dictionary<ImpedanceMode, (Unit, string)>
        {
            { ImpedanceMode.CPD, (Unit.None, "Затухание") },
            { ImpedanceMode.CPQ, (Unit.None, "Добротность") },
            { ImpedanceMode.CPG, (Unit.S, "Aктивная проводимость") },
            { ImpedanceMode.CPRP, (Unit.Ohm, "Сопротивление") },
            { ImpedanceMode.CSD, (Unit.None, "Затухание") },
            { ImpedanceMode.CSQ, (Unit.None, "Добротность") },
            { ImpedanceMode.CSRS, (Unit.Ohm, "Сопротивление") },
            { ImpedanceMode.LPD, (Unit.None, "Затухание") },
            { ImpedanceMode.LPQ, (Unit.None, "Добротность") },
            { ImpedanceMode.LPG, (Unit.S, "Aктивная проводимость") },
            { ImpedanceMode.LPRP, (Unit.Ohm, "Сопротивление") },
            { ImpedanceMode.LSD, (Unit.None, "Затухание") },
            { ImpedanceMode.LSQ, (Unit.None, "Добротность") },
            { ImpedanceMode.LSRS, (Unit.Ohm, "Сопротивление") },
            { ImpedanceMode.RX, (Unit.Ohm, "Сопротивление") },
            { ImpedanceMode.ZTD, (Unit.DA, "Фазовый угол") },
            { ImpedanceMode.ZTR, (Unit.RA, "Фазовый угол") },
            { ImpedanceMode.GB, (Unit.S, "Реактивная проводимость") },
            { ImpedanceMode.YTD, (Unit.DA, "Фазовый угол") },
            { ImpedanceMode.YTR, (Unit.RA, "Фазовый угол") }
        };
    }
}
