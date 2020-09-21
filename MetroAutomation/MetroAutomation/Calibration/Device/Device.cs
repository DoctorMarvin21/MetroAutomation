using MetroAutomation.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MetroAutomation.Calibration
{
    public class Device : IDisposable, INotifyPropertyChanged
    {
        private readonly SemaphoreSlim connectionLocker = new SemaphoreSlim(1, 1);

        private readonly object queryLocker = new object();

        private bool isConnected;
        private bool isProcessing;
        private bool isOutputOn;
        private bool isOutputAutoOff;

        private Stream commandStream;
        private StreamReader commandStreamReader;
        private StreamWriter commandStreamWriter;
        private DeviceConfiguration configuration;

        private Mode lastMode;
        private RangeInfo lastRange;
        private readonly bool testMode;

        public Device(DeviceConfiguration configuration)
        {
            Configuration = configuration;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<DeviceConnectionChangedEventArgs> ConnectionChanged;

        public event EventHandler<DeviceLogEventArgs> Log;

        public bool IsConnected
        {
            get
            {
                return isConnected;
            }
            private set
            {
                isConnected = value;
                OnPropertyChanged();
            }
        }

        public bool IsProcessing
        {
            get
            {
                return isProcessing;
            }
            private set
            {
                isProcessing = value;
                OnPropertyChanged();
            }
        }

        public bool IsOutputOn
        {
            get
            {
                return isOutputOn;
            }
            private set
            {
                isOutputOn = value;
                OnPropertyChanged();
            }
        }

        public bool IsOutputAutoOff
        {
            get
            {
                return isOutputAutoOff;
            }
            private set
            {
                isOutputAutoOff = value;
                OnPropertyChanged();
            }
        }

        public Dictionary<Mode, Function> Functions { get; } = new Dictionary<Mode, Function>();

        public ConnectionSettings ConnectionSettings { get; set; }

        public int ConfigurationID { get; set; }

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
            await connectionLocker.WaitAsync();

            if (!IsConnected)
            {
                OnConnectionChanged(ConnectionStatus.Connecting);

                try
                {
                    if (!testMode)
                    {
                        commandStream = await Task.Run(() => VisaComWrapper.GetStream(ConnectionSettings));
                        commandStreamWriter = new StreamWriter(commandStream, leaveOpen: true)
                        {
                            NewLine = ConnectionSettings.GetNewLineString(),
                            AutoFlush = true
                        };

                        commandStreamReader = new StreamReader(commandStream, leaveOpen: true);
                    }

                    IsConnected = true;

                    string connectCommand = Configuration.CommandSet.ConnectCommand;

                    if (!string.IsNullOrEmpty(connectCommand))
                    {
                        await QueryAsync(connectCommand, false);
                    }

                    await ChangeOutput(false, false);

                    OnLog(ConnectionStatus.Connected.GetDescription(), DeviceLogEntryType.Disconnected);
                    OnConnectionChanged(ConnectionStatus.Connected);
                }
                catch
                {
                    OnConnectionChanged(ConnectionStatus.ConnectError);
                }
            }

            connectionLocker.Release();
        }

        public async Task Disconnect()
        {
            await connectionLocker.WaitAsync();

            if (IsConnected)
            {
                OnConnectionChanged(ConnectionStatus.Disconnecting);

                await ChangeOutput(false, false);

                string disconnectCommand = Configuration.CommandSet?.DisconnectCommand;
                if (!string.IsNullOrEmpty(disconnectCommand))
                {
                    await QueryAsync(disconnectCommand, false);
                }

                if (testMode)
                {
                    IsConnected = false;
                }
                else
                {
                    ShutDownConnection();
                }

                OnLog(ConnectionStatus.Disconnected.GetDescription(), DeviceLogEntryType.Disconnected);
                OnConnectionChanged(ConnectionStatus.Disconnected);
            }

            connectionLocker.Release();
        }

        private void ShutDownConnection()
        {
            try
            {
                commandStreamReader?.Dispose();
                commandStreamWriter?.Dispose();
                commandStream?.Dispose();
            }
            catch
            {
            }
            finally
            {
                IsConnected = false;
            }
        }

        public async Task<bool> QueryAction(Function function, bool background)
        {
            if (!await ProcessModeAndRange(function, background))
            {
                return false;
            }

            return await QueryAction(function, FunctionCommandType.Value, background);
        }

        public async Task<decimal?> QueryResult(Function function, bool background)
        {
            if (!await ProcessModeAndRange(function, background))
            {
                return null;
            }


            return await QueryResult(function, FunctionCommandType.Value, background);
        }

        private async Task<bool> ProcessModeAndRange(Function function, bool background)
        {
            if (!function.AutoRange && function.RangeInfo == null)
            {
                return false;
            }

            if (lastMode != function.Mode)
            {
                if (IsOutputOn)
                {
                    await ChangeOutput(false, true);
                }

                if (await QueryAction(function, FunctionCommandType.Function, background))
                {
                    lastRange = null;
                    lastMode = function.Mode;
                }
                else
                {
                    return false;
                }
            }

            foreach (var command in function.AttachedCommands)
            {
                if (command.AutoExecute == AutoExecuteType.AfterMode)
                {
                    await command.Process(background);
                }
            }

            if (!function.AutoRange && lastRange != function.RangeInfo)
            {
                if (function.RangeInfo.Output != lastRange?.Output)
                {
                    if (IsOutputOn)
                    {
                        await ChangeOutput(false, true);
                    }
                }

                if (await QueryAction(function, FunctionCommandType.Range, background))
                {
                    lastRange = function.RangeInfo;
                }
                else
                {
                    return false;
                }
            }

            foreach (var command in function.AttachedCommands)
            {
                if (command.AutoExecute == AutoExecuteType.AfterRange)
                {
                    await command.Process(background);
                }
            }

            return true;
        }

        private async Task<bool> QueryAction(Function function, FunctionCommandType commandType, bool background)
        {
            if (Configuration.CommandSet.TryGetCommand(function.Mode, commandType, out string command))
            {
                try
                {
                    string filled = Utils.FillCommand(command, function, Configuration);
                    return await QueryAction(filled, background);
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

        public async Task<bool> QueryAction(string command, bool background)
        {
            string response = await QueryAsync(command, background);
            return Configuration.CommandSet.CheckResponse(response);
        }

        public async Task<bool> ChangeOutput(bool on, bool auto)
        {
            if (on)
            {
                if (string.IsNullOrEmpty(Configuration.CommandSet.OutputOnCommand))
                {
                    return true;
                }

                if (await QueryAction(Configuration.CommandSet.OutputOnCommand, false))
                {
                    IsOutputOn = true;
                    IsOutputAutoOff = false;

                    return true;
                }
                else
                {
                    IsOutputAutoOff = true;
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Configuration.CommandSet.OutputOffCommand))
                {
                    return true;
                }

                if (await QueryAction(Configuration.CommandSet.OutputOffCommand, false))
                {
                    IsOutputOn = false;
                    IsOutputAutoOff = auto;

                    return true;
                }
                else
                {
                    IsOutputOn = false;
                    IsOutputAutoOff = auto;

                    return false;
                }
            }
        }

        private async Task<decimal?> QueryResult(Function function, FunctionCommandType commandType, bool background)
        {
            if (Configuration.CommandSet.TryGetCommand(function.Mode, commandType, out string command))
            {
                try
                {
                    string filled = Utils.FillCommand(command, function, Configuration);
                    string response = await QueryAsync(filled, background);

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

        public Task<string> QueryAsync(string command, bool background)
        {
            return Task.Run(() => Query(command, background));
        }

        private string Query(string command, bool isBackground)
        {
            string result;

            if (!isBackground && IsProcessing)
            {
                return null;
            }

            lock (queryLocker)
            {
                if (!isBackground)
                {
                    IsProcessing = true;
                }

                try
                {
                    if (IsConnected)
                    {
                        OnLog(command, DeviceLogEntryType.DataSend);

                        if (testMode)
                        {
                            result = Configuration.CommandSet.ActionSuccess;
                            OnLog(result, DeviceLogEntryType.DataReceived);
                        }
                        else
                        {
                            commandStreamWriter.Write(command);

                            Thread.Sleep(ConnectionSettings.PauseAfterWrite);

                            if (Configuration.CommandSet.WaitForActionResponse)
                            {
                                result = commandStreamReader.ReadLine();

                                OnLog(result, DeviceLogEntryType.DataReceived);

                                Thread.Sleep(ConnectionSettings.PauseAfterRead);
                            }
                            else
                            {
                                result = null;
                            }
                        }
                    }
                    else
                    {
                        result = null;
                    }
                }
                catch
                {
                    ShutDownConnection();
                    OnConnectionChanged(ConnectionStatus.ConnectionLost);

                    result = null;
                }

                if (!isBackground)
                {
                    IsProcessing = false;
                }
            }

            return result;
        }

        private void OnConnectionChanged(ConnectionStatus status)
        {
            if (status == ConnectionStatus.Disconnected)
            {
                foreach (var function in Functions)
                {
                    foreach(var attached in function.Value.AttachedCommands)
                    {
                        attached.Reset();
                    }
                }
            }

            ConnectionChanged?.Invoke(this, new DeviceConnectionChangedEventArgs(this, IsConnected, status));
        }

        private void OnLog(string text, DeviceLogEntryType entryType)
        {
            Log?.Invoke(this, new DeviceLogEventArgs(this, text, entryType));
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            // TODO: implement disposing
        }
    }
}
