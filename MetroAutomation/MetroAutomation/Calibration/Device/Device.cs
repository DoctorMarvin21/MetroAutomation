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

        private MessageStream commandStream;
        private DeviceConfiguration configuration;

#if DEBUG
        private readonly bool testMode = true;
#else
        private readonly bool testMode = false;
#endif

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

        public Guid ConfigurationID { get; set; }

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

        public Func<RangeInfo, Function, Task<bool>> OnRangeChanged { get; set; }

        public Func<Mode?, Function, Task<bool>> OnModeChanged { get; set; }

        public Func<bool, Task<bool>> OnOutputChanging { get; set; }

        public Func<bool, Task> OnOutputChanged { get; set; }

        public Func<string, Task<bool>> OnManualAction { get; set; }

        public Func<string, Task<decimal?>> OnManualResult { get; set; }

        public Mode? LastMode { get; set; }

        public RangeInfo LastRange { get; set; }

        public async Task Connect()
        {
            await connectionLocker.WaitAsync();

            if (!IsConnected)
            {
                OnConnectionChanged(false, ConnectionStatus.Connecting);

                try
                {
                    if (!testMode)
                    {
                        if (ConnectionSettings.Type != ConnectionType.Manual)
                        {
                            commandStream = await Task.Run(() => VisaComWrapper.GetStream(ConnectionSettings));
                        }
                    }
                    else
                    {
                        await Task.Delay(100);
                    }

                    IsConnected = true;

                    string connectCommand = Configuration.CommandSet.ConnectCommand;

                    if (!string.IsNullOrEmpty(connectCommand))
                    {
                        await QueryAsync(connectCommand, false);
                    }

                    await ChangeOutput(false, false);

                    // Conection might be lost during connection
                    if (IsConnected)
                    {
                        OnConnectionChanged(true, ConnectionStatus.Connected);
                        OnLog(true, ConnectionStatus.Connected.GetDescription(), DeviceLogEntryType.Connected);
                    }
                }
                catch
                {
                    OnConnectionChanged(false, ConnectionStatus.ConnectError);
                    OnLog(false, ConnectionStatus.ConnectError.GetDescription(), DeviceLogEntryType.ConnectError);
                }
            }

            connectionLocker.Release();
        }

        public async Task Disconnect()
        {
            await connectionLocker.WaitAsync();

            if (IsConnected)
            {
                OnConnectionChanged(true, ConnectionStatus.Disconnecting);

                await ChangeOutput(false, false);

                string disconnectCommand = Configuration.CommandSet?.DisconnectCommand;
                if (!string.IsNullOrEmpty(disconnectCommand))
                {
                    await QueryAsync(disconnectCommand, false);
                }

                if (testMode)
                {
                    await Task.Delay(300);
                    IsConnected = false;
                }
                else
                {
                    ShutDownConnection();
                }

                OnConnectionChanged(false, ConnectionStatus.Disconnected);
                OnLog(true, ConnectionStatus.Disconnected.GetDescription(), DeviceLogEntryType.Disconnected);
            }

            connectionLocker.Release();
        }

        private void ShutDownConnection()
        {
            try
            {
                commandStream?.Dispose();
            }
            catch
            {
            }
            finally
            {
                ResetRangeAndMode();
                IsConnected = false;
            }
        }

        public async Task<bool> QueryAction(Function function, bool background)
        {
            if (!await ProcessModeAndRange(function, background))
            {
                return false;
            }

            var result = await QueryAction(function, FunctionCommandType.Value, background);

            if (result && IsOutputOn)
            {
                await ChangeOutput(true, true);
            }

            return result;
        }

        public async Task<decimal?> QueryResult(Function function, bool background)
        {
            if (!await ProcessModeAndRange(function, background))
            {
                return null;
            }

            var result = await QueryResult(function, FunctionCommandType.Value, background);

            if (result.HasValue && IsOutputOn)
            {
                await ChangeOutput(true, true);
            }

            return result;
        }

        public void ResetRangeAndMode()
        {
            LastMode = null;
            LastRange = null;
        }

        public async Task<bool> ProcessModeAndRange(Function function, bool background)
        {
            if (LastMode != function.Mode)
            {
                if (OnModeChanged != null && !await OnModeChanged(LastMode, function))
                {
                    return false;
                }

                if (IsOutputOn)
                {
                    await ChangeOutput(false, true);
                }

                if (await QueryAction(function, FunctionCommandType.Function, background))
                {
                    LastMode = function.Mode;
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

            if (LastRange != function.RangeInfo)
            {
                if (!function.AutoRange && function.RangeInfo == null)
                {
                    return false;
                }

                if (OnRangeChanged != null && !await OnRangeChanged(LastRange, function))
                {
                    return false;
                }

                if (function.RangeInfo.Output != LastRange?.Output)
                {
                    if (IsOutputOn)
                    {
                        await ChangeOutput(false, true);
                    }
                }

                if (await QueryAction(function, FunctionCommandType.Range, background))
                {
                    LastRange = function.RangeInfo;
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
            if (Configuration.CommandSet.TryGetCommand(function.Mode, commandType, out string[] commands))
            {
                try
                {
                    foreach (var command in commands)
                    {
                        string filled = Utils.FillCommand(command, function, Configuration);
                        
                        if (!await QueryAction(filled, background))
                        {
                            return false;
                        }
                    }

                    return true;
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
            if (ConnectionSettings.Type == ConnectionType.Manual)
            {
                if (OnManualAction != null)
                {
                    return await OnManualAction(command);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                string response = await QueryAsync(command, background);
                bool result = Configuration.CommandSet.CheckResponse(response);

                if (!result)
                {
                    OnLog(false, "Запрос не выполнен", DeviceLogEntryType.QueryError);
                }

                return result;
            }
        }

        public async Task<bool> ChangeOutput(bool on, bool auto)
        {
            if (OnOutputChanging != null && !await OnOutputChanging(on))
            {
                return false;
            }

            bool result;

            if (on)
            {
                if (!string.IsNullOrEmpty(Configuration.CommandSet.OutputOnCommand))
                {
                    if (await QueryAction(Configuration.CommandSet.OutputOnCommand, false))
                    {
                        IsOutputOn = true;
                        IsOutputAutoOff = false;

                        result = true;
                    }
                    else
                    {
                        IsOutputAutoOff = true;
                        result = false;
                    }
                }
                else
                {
                    result = true;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Configuration.CommandSet.OutputOffCommand))
                {
                    if (await QueryAction(Configuration.CommandSet.OutputOffCommand, false))
                    {
                        IsOutputOn = false;
                        IsOutputAutoOff = auto;

                        result = true;
                    }
                    else
                    {
                        IsOutputOn = false;
                        IsOutputAutoOff = auto;

                        result = false;
                    }
                }
                else
                {
                    result = true;
                }
            }

            if (result && OnOutputChanged != null)
            {
                await OnOutputChanged(isOutputOn);
            }

            return result;
        }

        private async Task<decimal?> QueryResult(Function function, FunctionCommandType commandType, bool background)
        {
            if (Configuration.CommandSet.TryGetCommand(function.Mode, commandType, out string[] commands))
            {
                try
                {
                    // all commands except for last one considered as action commands
                    for (int i = 0; i < commands.Length - 1; i++)
                    {
                        string filledQuery = Utils.FillCommand(commands[i], function, Configuration);
                        if (!await QueryAction(filledQuery, background))
                        {
                            return null;
                        }
                    }

                    string filled = Utils.FillCommand(commands[commands.Length - 1], function, Configuration);

                    if (ConnectionSettings.Type == ConnectionType.Manual)
                    {
                        if (OnManualResult != null)
                        {
                            return await OnManualResult(filled);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        string response = await QueryAsync(filled, background);

                        if (decimal.TryParse(response, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                        {
                            return result;
                        }
                        else
                        {
                            OnLog(false, "Ошибка обработки ответа", DeviceLogEntryType.QueryError);
                            return null;
                        }
                    }
                }
                catch
                {
                    OnLog(false, "Ошибка обработки ответа", DeviceLogEntryType.QueryError);
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
            if (ConnectionSettings.Type != ConnectionType.Manual)
            {
                return Task.Run(() => Query(command, background));
            }
            else
            {
                return Task.FromResult(Configuration.CommandSet.ActionSuccess);
            }
        }

        private string Query(string command, bool isBackground)
        {
            string result;

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
                        OnLog(true, command, DeviceLogEntryType.DataSend);

                        if (testMode)
                        {
                            Thread.Sleep(100);
                            result = Configuration.CommandSet.ActionSuccess;
                            OnLog(true, result, DeviceLogEntryType.DataReceived);
                        }
                        else
                        {
                            commandStream.WriteString(command);

                            Thread.Sleep(ConnectionSettings.PauseAfterWrite);

                            if (Configuration.CommandSet.WaitForActionResponse)
                            {
                                result = commandStream.ReadString();

                                OnLog(true, result, DeviceLogEntryType.DataReceived);

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
                        OnLog(false, "Прибор не подключен", DeviceLogEntryType.QueryError);
                        result = null;
                    }
                }
                catch
                {
                    ShutDownConnection();
                    OnConnectionChanged(false, ConnectionStatus.ConnectionLost);
                    OnLog(false, "Потеря соединения", DeviceLogEntryType.QueryError);

                    result = null;
                }

                if (!isBackground)
                {
                    IsProcessing = false;
                }
            }

            return result;
        }

        private void OnConnectionChanged(bool isConnected, ConnectionStatus status)
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

            ConnectionChanged?.Invoke(this, new DeviceConnectionChangedEventArgs(this, isConnected, status));
        }

        private void OnLog(bool isSuccess, string text, DeviceLogEntryType entryType)
        {
            Log?.Invoke(this, new DeviceLogEventArgs(this, isSuccess, text, entryType));
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            ShutDownConnection();
        }
    }
}
