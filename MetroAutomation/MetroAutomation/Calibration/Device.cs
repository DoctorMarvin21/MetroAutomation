﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MetroAutomation.Calibration
{
    public enum Direction
    {
        Get,
        Set
    }

    public class Device : INotifyPropertyChanged
    {
        private readonly object queryLocker = new object();

        private bool isConnected;
        private bool isProcessing;
        private bool isOutputOn;

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

        public event PropertyChangedEventHandler PropertyChanged;

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
                if (!string.IsNullOrEmpty(connectCommand))
                {
                    await QueryAsync(connectCommand, false);
                }

                await ChangeOutput(false);

                IsConnected = true;
            }
        }

        public async Task Disconnect()
        {
            if (IsConnected)
            {
                await ChangeOutput(false);

                string disconnectCommand = Configuration.CommandSet?.DisconnectCommand;
                if (!string.IsNullOrEmpty(disconnectCommand))
                {
                    await QueryAsync(disconnectCommand, false);
                }

                commandStreamReader.Dispose();
                commandStreamWriter.Dispose();
                await commandStream.DisposeAsync();

                IsConnected = false;
            }
        }

        public async Task<bool> ProcessFunction(Function function, bool background)
        {
            if (function.RangeInfo == null)
            {
                return false;
            }

            if (lastMode != function.Mode)
            {
                if (IsOutputOn)
                {
                    await ChangeOutput(false);
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

            if (lastRange != function.RangeInfo)
            {
                if (function.RangeInfo.Output != lastRange?.Output)
                {
                    if (IsOutputOn)
                    {
                        await ChangeOutput(false);
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

            if (function.Direction == Direction.Set)
            {
                return await QueryAction(function, FunctionCommandType.Value, background);
            }
            else
            {
                var result = await QueryResult(function, FunctionCommandType.Value, background);
                function.ProcessResult(result, UnitModifier.None);

                return result.HasValue;
            }
        }

        public async Task<bool> QueryAction(Function function, FunctionCommandType commandType, bool background)
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

        private async Task<bool> QueryAction(string command, bool background)
        {
            string response = await QueryAsync(command, background);
            return Configuration.CommandSet.CheckResponse(response);
        }

        public async Task<bool> ChangeOutput(bool on)
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
                    return true;
                }
                else
                {
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
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<decimal?> QueryResult(Function function, FunctionCommandType commandType, bool background)
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
                        commandStreamWriter.Write(command);

                        Thread.Sleep(ConnectionSettings.PauseAfterWrite);

                        if (Configuration.CommandSet.WaitForActionResponse)
                        {
                            result = commandStreamReader.ReadLine();

                            Thread.Sleep(ConnectionSettings.PauseAfterRead);
                        }
                        else
                        {
                            result = null;
                        }
                    }
                    else
                    {
                        result = null;
                    }
                }
                catch
                {
                    result = null;
                }

                if (!isBackground)
                {
                    IsProcessing = false;
                }
            }

            return result;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
