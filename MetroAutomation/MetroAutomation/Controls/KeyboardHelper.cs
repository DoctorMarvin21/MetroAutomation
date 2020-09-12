using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MetroAutomation.Controls
{
    public static class KeyboardHelper
    {
        public static Dictionary<Key, string> InputText { get; } = new Dictionary<Key, string>
        {
            { Key.A, "Ф" },
            { Key.B, "И" },
            { Key.C, "С" },
            { Key.D, "В" },
            { Key.E, "У" },
            { Key.F, "А" },
            { Key.G, "П" },
            { Key.H, "Р" },
            { Key.I, "Ш" },
            { Key.J, "О" },
            { Key.K, "Л" },
            { Key.L, "Д" },
            { Key.M, "Ь" },
            { Key.N, "Т" },
            { Key.O, "Щ" },
            { Key.P, "З" },
            { Key.Q, "Й" },
            { Key.R, "К" },
            { Key.S, "Ы" },
            { Key.T, "Е" },
            { Key.U, "Г" },
            { Key.V, "М" },
            { Key.W, "Ц" },
            { Key.X, "Ч" },
            { Key.Y, "Н" },
            { Key.Z, "Я" }
        };
    }
}
