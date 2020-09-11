using LiteDB;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MetroAutomation.Calibration
{
    public enum Direction
    {
        Get,
        Set
    }

    public class Device
    {
        private readonly object queryLocker = new object();
        private Stream commandStream;
        private StreamReader commandStreamReader;
        private StreamWriter commandStreamWriter;
        private DeviceConfiguration configuration;

        private Mode lastMode;
        private RangeInfo lastRange;

        public Device()
        {
        }

        public Device(DeviceConfiguration configuration)
        {
            Configuration = configuration;
        }

        public bool IsConnected { get; private set; }

        public Dictionary<Mode, Function> Functions { get; } = new Dictionary<Mode, Function>();

        public ConnectionSettings ConnectionSettings { get; set; }

        public int ConfigurationID { get; set; }

        [BsonIgnore]
        public DeviceConfiguration Configuration
        {
            get
            {
                return configuration;
            }
            set
            {
                configuration = value;

                if (Configuration != null)
                {
                    ConfigurationID = configuration.ID;

                    if (ConnectionSettings == null)
                    {
                        ConnectionSettings = configuration.DefaultConnectionSettings ?? new ConnectionSettings();
                    }

                    Functions.Clear();

                    if (Configuration.ModeInfo != null)
                    {
                        foreach (var mode in Configuration.ModeInfo)
                        {
                            if (mode.IsAvailable)
                            {
                                Functions.Add(mode.Mode, Function.GetFunction(this, mode.Mode));
                            }
                        }
                    }
                }
            }
        }

        public async Task Connect()
        {
            if (!IsConnected)
            {
                commandStream = await Task.Run(() => VisaComWrapper.GetStream(ConnectionSettings));
                commandStreamWriter = new StreamWriter(commandStream, leaveOpen: true)
                {
                    NewLine = ConnectionSettings.GetNewLineString(),
                    AutoFlush = true
                };

                commandStreamReader = new StreamReader(commandStream, leaveOpen: true);

                string connectCommand = Configuration.CommandSet.ConnectCommand;
                if (connectCommand != null)
                {
                    await QueryAsync(connectCommand);
                }

                IsConnected = true;
            }
        }

        public async Task Disconnect()
        {
            if (IsConnected)
            {
                string disconnectCommand = Configuration.CommandSet?.DisconnectCommand;
                if (disconnectCommand != null)
                {
                    await QueryAsync(disconnectCommand);
                }

                commandStreamReader.Dispose();
                commandStreamWriter.Dispose();
                await commandStream.DisposeAsync();

                IsConnected = false;
            }
        }

        public async Task<bool> ProcessFunction(Function function)
        {
            if (lastMode != function.Mode)
            {
                if (await QueryAction(function, FunctionCommandType.Function))
                {
                    lastRange = null;
                    lastMode = function.Mode;
                }
                else
                {
                    return false;
                }
            }

            if (lastRange != function.RangeInfo)
            {
                if (await QueryAction(function, FunctionCommandType.Range))
                {
                    lastRange = function.RangeInfo;
                }
                else
                {
                    return false;
                }
            }

            if (function.Direction == Direction.Set)
            {
                return await QueryAction(function, FunctionCommandType.Function);
            }
            else
            {
                var result = await QueryResult(function, FunctionCommandType.Value);
                function.ProcessResult(result, UnitModifier.None);

                return result.HasValue;
            }
        }

        public async Task<bool> QueryAction(Function function, FunctionCommandType commandType)
        {
            if (Configuration.CommandSet.TryGetCommand(function.Mode, commandType, out string command))
            {
                try
                {
                    string filled = Utils.FillCommand(command, function, Configuration);
                    string response = await QueryAsync(filled);

                    return Configuration.CommandSet.CheckResponse(response);
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public async Task<decimal?> QueryResult(Function function, FunctionCommandType commandType)
        {
            if (Configuration.CommandSet.TryGetCommand(function.Mode, commandType, out string command))
            {
                try
                {
                    string filled = Utils.FillCommand(command, function, Configuration);
                    string response = await QueryAsync(filled);

                    if (decimal.TryParse(response, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                    {
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public Task<string> QueryAsync(string command)
        {
            return Task.Run(() => Query(command));
        }

        private string Query(string command)
        {
            lock (queryLocker)
            {
                try
                {
                    if (!IsConnected)
                    {
                        return null;
                    }

                    commandStreamWriter.Write(command);

                    Thread.Sleep(ConnectionSettings.PauseAfterWrite);

                    if (Configuration.CommandSet.WaitForActionResponse)
                    {
                        string result = commandStreamReader.ReadLine();

                        Thread.Sleep(ConnectionSettings.PauseAfterRead);

                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
