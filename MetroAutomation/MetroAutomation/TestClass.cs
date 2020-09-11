using MetroAutomation.Calibration;
using MetroAutomation.Model;
using System.Collections.Generic;

namespace MetroAutomation
{
    class TestClass
    {
        public static Device GetTestDevice()
        {
            CommandSet set = new CommandSet();

            set.ActionFail = "0";
            set.ActionSuccess = "1";

            DeviceConfiguration configuration = new DeviceConfiguration();
            configuration.CommandSet = set;
            configuration.Name = "Fluke 5720A";

            configuration.ModeInfo = new ModeInfo[]
            {
                new ModeInfo
                {
                    Mode = Mode.SetDCV,
                    IsAvailable = true,
                    Ranges = new[]
                    {
                        new RangeInfo
                        {
                            Output = "Normal",
                            Range = new BaseValueInfo(1000, Unit.V, UnitModifier.None),
                            ComponentsRanges = new[]
                            {
                                new ValueRange(new BaseValueInfo(-1000, Unit.V, UnitModifier.None), new BaseValueInfo(1000, Unit.V, UnitModifier.None))
                            }
                        }
                    }
                },
                new ModeInfo
                {
                    Mode = Mode.SetACV,
                    IsAvailable = true,
                    Ranges = new[]
                    {
                        new RangeInfo
                        {
                            Output = "Normal",
                            Range = new BaseValueInfo(1000, Unit.V, UnitModifier.None),
                            ComponentsRanges = new[]
                            {
                                new ValueRange(new BaseValueInfo(0, Unit.V, UnitModifier.None), new BaseValueInfo(1000, Unit.V, UnitModifier.None)),
                                new ValueRange(new BaseValueInfo(0, Unit.Hz, UnitModifier.Kilo), new BaseValueInfo(1000, Unit.Hz, UnitModifier.Kilo))
                            }
                        }
                    }
                },
                new ModeInfo
                {
                    Mode = Mode.SetDCI,
                    IsAvailable = true,
                },
                new ModeInfo
                {
                    Mode = Mode.SetACI,
                    IsAvailable = true,
                },
                new ModeInfo
                {
                    Mode = Mode.SetRES2W,
                    IsAvailable = true,
                    Ranges = new[]
                    {
                        new RangeInfo
                        {
                            Output = "Normal",
                            Range = new BaseValueInfo(1000, Unit.V, UnitModifier.None),
                            ComponentsRanges = new[]
                            {
                                new ValueRange(new BaseValueInfo(0, Unit.Ohm, UnitModifier.None), new BaseValueInfo(100, Unit.Ohm, UnitModifier.Mega)),
                            }
                        }
                    },
                    ActualValues = new[]
                    {
                        new ActualValueInfo { Value = new BaseValueInfo(1, Unit.Ohm, UnitModifier.None), ActualValue = new BaseValueInfo(1.001m,  Unit.Ohm, UnitModifier.None) },
                        new ActualValueInfo { Value = new BaseValueInfo(10, Unit.Ohm, UnitModifier.None), ActualValue = new BaseValueInfo(10.0021m,  Unit.Ohm, UnitModifier.None) },
                        new ActualValueInfo { Value = new BaseValueInfo(100, Unit.Ohm, UnitModifier.None), ActualValue = new BaseValueInfo(100.0121m,  Unit.Ohm, UnitModifier.None) },
                        new ActualValueInfo { Value = new BaseValueInfo(1000, Unit.Ohm, UnitModifier.None), ActualValue = new BaseValueInfo(1000.021m,  Unit.Ohm, UnitModifier.None) }
                    }
                },
                new ModeInfo
                {
                    Mode = Mode.GetRES4W,
                    IsAvailable = true,
                },
            };

            Device device = new Device();

            device.Configuration = configuration;
            device.ConnectionSettings.Type = ConnectionType.Gpib;

            return device;
        }

        public static Device GetTestMeasureDevice()
        {
            CommandSet set = new CommandSet();

            set.ActionFail = "0";
            set.ActionSuccess = "1";

            DeviceConfiguration configuration = new DeviceConfiguration();
            configuration.CommandSet = set;
            configuration.Name = "Fluke 8508A";

            configuration.ModeInfo = new ModeInfo[]
            {
                new ModeInfo
                {
                    Mode = Mode.GetDCV,
                    IsAvailable = true,
                    Ranges = new RangeInfo[]
                    {
                        new RangeInfo
                        {
                            Output = "Normal",
                            Range = new BaseValueInfo(200, Unit.V, UnitModifier.Mili),
                            ComponentsRanges = new[]
                            {
                                new ValueRange(new BaseValueInfo(-200, Unit.V, UnitModifier.Mili), new BaseValueInfo(200, Unit.V, UnitModifier.Mili))
                            }
                        },
                        new RangeInfo
                        {
                            Output = "Normal",
                            Range = new BaseValueInfo(2, Unit.V, UnitModifier.None),
                            ComponentsRanges = new[]
                            {
                                new ValueRange(new BaseValueInfo(-2, Unit.V, UnitModifier.None), new BaseValueInfo(2, Unit.V, UnitModifier.None))
                            }
                        },
                        new RangeInfo
                        {
                            Output = "Normal",
                            Range = new BaseValueInfo(20, Unit.V, UnitModifier.None),
                            ComponentsRanges = new[]
                            {
                                new ValueRange(new BaseValueInfo(-20, Unit.V, UnitModifier.None), new BaseValueInfo(20, Unit.V, UnitModifier.None))
                            }
                        },
                        new RangeInfo
                        {
                            Output = "Normal",
                            Range = new BaseValueInfo(200, Unit.V, UnitModifier.None),
                            ComponentsRanges = new[]
                            {
                                new ValueRange(new BaseValueInfo(-200, Unit.V, UnitModifier.None), new BaseValueInfo(200, Unit.V, UnitModifier.None))
                            }
                        },
                        new RangeInfo
                        {
                            Output = "Normal",
                            Range = new BaseValueInfo(1000, Unit.V, UnitModifier.None),
                            ComponentsRanges = new[]
                            {
                                new ValueRange(new BaseValueInfo(-1000, Unit.V, UnitModifier.None), new BaseValueInfo(1000, Unit.V, UnitModifier.None))
                            }
                        }
                    }
                },
                new ModeInfo
                {
                    Mode = Mode.GetACV,
                    IsAvailable = true,
                    Ranges = new RangeInfo[]
                    {
                        new RangeInfo
                        {
                            Output = "Normal",
                            Range = new BaseValueInfo(1000, Unit.V, UnitModifier.None),
                            ComponentsRanges = new[]
                            {
                                new ValueRange(new BaseValueInfo(0, Unit.V, UnitModifier.None), new BaseValueInfo(1000, Unit.V, UnitModifier.None))
                            }
                        }
                    }
                },
                new ModeInfo
                {
                    Mode = Mode.GetDCI,
                    IsAvailable = true,
                },
                new ModeInfo
                {
                    Mode = Mode.GetACI,
                    IsAvailable = true,
                },
                new ModeInfo
                {
                    Mode = Mode.GetRES2W,
                    IsAvailable = true,

                }
            };

            Device device = new Device();

            device.Configuration = configuration;
            device.ConnectionSettings.Type = ConnectionType.Gpib;

            return device;
        }

        public static void DatabaseTests()
        {
            //LiteDBAdaptor.ClearAll<CommandSet>();
            //LiteDBAdaptor.ClearAll<DeviceConfiguration>();

            //CommandSet set = new CommandSet();
            //set.UnitNames[Unit.V].Value = "V";
            //set.UnitNames[Unit.Hz].Value = "HZ";
            //set.UnitNames[Unit.Ohm].Value = "OHM";

            //set.ActionFail = "0";
            //set.ActionSuccess = "1";
            ////set.FunctionCommands[CommandType.SetACV] = "OUT <V;1;U>, <V;2;U>; *OPC?";

            //LiteDBAdaptor.SaveData(set);

            //DeviceConfiguration configuration = new DeviceConfiguration();
            //configuration.CommandSet = set;
            //configuration.Name = "Fluke 5720A";
            //configuration.AvailableModes[Mode.SetACV] = true;
            //configuration.AvailableModes[Mode.SetDCV] = true;

            //LiteDBAdaptor.SaveData(configuration);

            //var a = LiteDBAdaptor.LoadData<DeviceConfiguration>(1);
            //a.CommandSet = LiteDBAdaptor.LoadData<CommandSet>(a.CommandSetID);
        }

        public async void Test()
        {
            var device = GetTestDevice();

            var setAcv = device.Functions[Mode.SetACV];
            setAcv.Components[0].Value = 1m;
            setAcv.Components[0].Modifier = UnitModifier.None;
            setAcv.Components[1].Value = 100m;
            setAcv.Components[1].Modifier = UnitModifier.None;

            var result = await setAcv.Process();

            setAcv.Components[0].Value = 2m;
            setAcv.Components[0].Modifier = UnitModifier.None;
            setAcv.Components[1].Value = 200m;
            setAcv.Components[1].Modifier = UnitModifier.None;

            result = await setAcv.Process();
        }
    }
}
