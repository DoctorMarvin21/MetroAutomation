using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System.Collections.Generic;
using System.Linq;

namespace MetroAutomation.Automation
{
    public static class ProtocolFunctions
    {
        public static Dictionary<AutomationMode, PairedModeInfo> PairedFunctions { get; }

        static ProtocolFunctions()
        {
            PairedFunctions = new Dictionary<AutomationMode, PairedModeInfo>
            {
                {
                    AutomationMode.GetDCV,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetDCV,
                        SourceMode = Mode.GetDCV,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetDCV, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Калибратор напряжения", Mode.SetDCV) }
                    }
                },
                {
                    AutomationMode.SetDCV,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.SetDCV,
                        SourceMode = Mode.SetDCV,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.SetDCV, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Измеритель напряжения", Mode.GetDCV) }
                    }
                },
                {
                    AutomationMode.GetACV,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetACV,
                        SourceMode = Mode.GetACV,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetACV, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Калибратор напряжения", Mode.SetACV) }
                    }
                },
                {
                    AutomationMode.GetACV792A,
                    new TransferStandardGetModeInfo
                    {
                        AutomationMode = AutomationMode.GetACV792A,
                        SourceMode = Mode.GetACV,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetACV, DescriptionType.Full) + " c использованием Fluke 792A",
                        Standards = new[] { new StandardInfo("Калибратор напряжения", Mode.SetACV) }
                    }
                },
                {
                    AutomationMode.SetACV,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.SetACV,
                        SourceMode = Mode.SetACV,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.SetACV, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Измеритель напряжения", Mode.GetACV) }
                    }
                },
                {
                    AutomationMode.SetACV792A,
                    new TransferStandardSetModeInfo
                    {
                        AutomationMode = AutomationMode.SetACV792A,
                        SourceMode = Mode.SetACV,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.SetACV, DescriptionType.Full) + " c использованием Fluke 792A",
                        Standards = new[] { new StandardInfo("Измеритель напряжения", Mode.GetACV) }
                    }
                },
                {
                    AutomationMode.GetDCI,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetDCI,
                        SourceMode = Mode.GetDCI,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetDCI, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Калибратор тока", Mode.SetDCI) }
                    }
                },
                {
                    AutomationMode.SetDCI,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.SetDCI,
                        SourceMode = Mode.SetDCI,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.SetDCI, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Измеритель тока", Mode.GetDCI) }
                    }
                },
                {
                    AutomationMode.GetACI,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetACI,
                        SourceMode = Mode.GetACI,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetACI, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Калибратор тока", Mode.SetACI) }
                    }
                },
                {
                    AutomationMode.GetACI792A,
                    new TransferStandardGetModeInfo
                    {
                        AutomationMode = AutomationMode.GetACI792A,
                        SourceMode = Mode.GetACI,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetACI, DescriptionType.Full) + " c использованием Fluke 792A",
                        Standards = new[] { new StandardInfo("Калибратор тока", Mode.SetACI) }
                    }
                },
                {
                    AutomationMode.SetACI,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.SetACI,
                        SourceMode = Mode.SetACI,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.SetACI, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Измеритель тока", Mode.GetACI) }
                    }
                },
                {
                    AutomationMode.SetACI792A,
                    new TransferStandardSetModeInfo
                    {
                        AutomationMode = AutomationMode.SetACI792A,
                        SourceMode = Mode.SetACI,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.SetACI, DescriptionType.Full) + " c использованием Fluke 792A",
                        Standards = new[] { new StandardInfo("Измеритель напряжения", Mode.GetACI) }
                    }
                },
                {
                    AutomationMode.GetRES2W,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetRES2W,
                        SourceMode = Mode.GetRES2W,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetRES2W, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Калибратор сопротивления", Mode.SetRES2W) }
                    }
                },
                {
                    AutomationMode.SetRES2WZERO,
                    new ResistanceModeInfo
                    {
                        AutomationMode = AutomationMode.SetRES2WZERO,
                        SourceMode = Mode.GetRES2W,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetRES2W, DescriptionType.Full) + " с калибровкой нуля",
                        Standards = new[] { new StandardInfo("Калибратор сопротивления", Mode.SetRES2W) }
                    }
                },
                {
                    AutomationMode.SetRES2W,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.SetRES2W,
                        SourceMode = Mode.SetRES2W,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.SetRES2W, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Измеритель сопротивления", Mode.GetRES2W) }
                    }
                },
                {
                    AutomationMode.GetRES4W,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetRES4W,
                        SourceMode = Mode.GetRES4W,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetRES4W, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Калибратор сопротивления", Mode.SetRES4W) }
                    }
                },
                {
                    AutomationMode.GetRES4W2W,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetRES4W2W,
                        SourceMode = Mode.GetRES4W,
                        Name = "Измерение сопротивления по высокоомной четырехпроводной схеме",
                        Standards = new[] { new StandardInfo("Калибратор сопротивления", Mode.SetRES2W) }
                    }
                },
                {
                    AutomationMode.SetRES4W,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.SetRES4W,
                        SourceMode = Mode.SetRES4W,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.SetRES4W, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Измеритель сопротивления", Mode.GetRES4W) }
                    }
                },
                {
                    AutomationMode.GetCAP2W,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetCAP2W,
                        SourceMode = Mode.GetCAP2W,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetCAP2W, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Калибратор емкости", Mode.SetCAP2W) }
                    }
                },
                {
                    AutomationMode.SetCAP2W,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.SetCAP2W,
                        SourceMode = Mode.SetCAP2W,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.SetCAP2W, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Измеритель емкости", Mode.GetCAP2W) }
                    }
                },
                {
                    AutomationMode.GetCAP4W,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetCAP4W,
                        SourceMode = Mode.GetCAP4W,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetCAP4W, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Калибратор емкости", Mode.SetCAP4W) }
                    }
                },
                {
                    AutomationMode.SetCAP4W,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.SetCAP4W,
                        SourceMode = Mode.SetCAP4W,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.SetCAP4W, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Измеритель емкости", Mode.GetCAP4W) }
                    }
                },
                {
                    AutomationMode.GetIND2W,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetIND2W,
                        SourceMode = Mode.GetIND2W,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetIND2W, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Калибратор индуктивности", Mode.SetIND2W) }
                    }
                },
                {
                    AutomationMode.SetIND2W,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.SetIND2W,
                        SourceMode = Mode.SetIND2W,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.SetIND2W, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Измеритель индуктивности", Mode.GetIND2W) }
                    }
                },
                {
                    AutomationMode.GetIND4W,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetIND4W,
                        SourceMode = Mode.GetIND4W,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetIND4W, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Калибратор индуктивности", Mode.SetIND4W) }
                    }
                },
                {
                    AutomationMode.SetIND4W,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.SetIND4W,
                        SourceMode = Mode.SetIND4W,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.SetIND4W, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Измеритель индуктивности", Mode.GetIND4W) }
                    }
                },
                {
                    AutomationMode.GetDCP,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetDCP,
                        SourceMode = Mode.GetDCP,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetDCP, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Калибратор мощности", Mode.SetDCP) }
                    }
                },
                {
                    AutomationMode.SetDCP,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.SetDCP,
                        SourceMode = Mode.SetDCP,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.SetDCP, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Измеритель мощности", Mode.GetDCP) }
                    }
                },
                {
                    AutomationMode.GetACP,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetACP,
                        SourceMode = Mode.GetACP,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetACP, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Калибратор мощности", Mode.SetACP) }
                    }
                },
                {
                    AutomationMode.SetACP,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.SetACP,
                        SourceMode = Mode.SetACP,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.SetACP, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Измеритель мощности", Mode.GetACP) }
                    }
                },
                {
                    AutomationMode.GetTEMP,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetTEMP,
                        SourceMode = Mode.GetTEMP,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetTEMP, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Калибратор температуры", Mode.SetTEMP) }
                    }
                },
                {
                    AutomationMode.SetTEMP,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.SetTEMP,
                        SourceMode = Mode.SetTEMP,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.SetTEMP, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Измеритель температуры", Mode.GetTEMP) }
                    }
                },
                {
                    AutomationMode.GetFREQ,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetFREQ,
                        SourceMode = Mode.GetFREQ,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetFREQ, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Калибратор частоты", Mode.SetFREQ) }
                    }
                },
                {
                    AutomationMode.SetFREQ,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.SetFREQ,
                        SourceMode = Mode.SetFREQ,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.SetFREQ, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Измеритель частоты", Mode.GetFREQ) }
                    }
                },
            };
        }

        public static PairedModeInfo GetPairedModeInfo(DeviceProtocolBlock deviceProtocolBlock)
        {
            if (PairedFunctions.TryGetValue(deviceProtocolBlock.AutomationMode, out var mode))
            {
                return mode;
            }
            else
            {
                // Not good a good practice
                return new PairedModeInfo();
            }
        }

        public static StandardInfo GetStandardInfo(DeviceProtocolBlock deviceProtocolBlock, int index)
        {
            var modeInfo = GetPairedModeInfo(deviceProtocolBlock);

            if (modeInfo.Standards?.Length > index)
            {
                return modeInfo.Standards[index];
            }
            else
            {
                return null;
            }
        }

        public static PairedModeInfo[] GetModeInfo(Device device)
        {
            return PairedFunctions.Values.Where(x => device.Functions.Values.Any(y => y.Mode == x.SourceMode)).ToArray();
        }
    }
}
