using MetroAutomation.Calibration;
using System;
using System.Runtime.InteropServices;
using System.Text;
using VisaComLib;

namespace MetroAutomation.Model
{
    public class MessageStream : IDisposable
    {
        private const int BufferLength = 128;
        private const int ReadTimeoutInternal = 1000;
        private readonly int timeout;
        private readonly int openTimeout;
        private readonly byte terminationCharacter;
        private readonly string terminationString;
        private readonly bool autoTerminate;

        private readonly GpibPrologixConnectionSettings prologixConnectionSettings;

        public MessageStream(ResourceManagerClass resourceManager, string resourceName, int openTimeout, int timeout, string terminationString, bool autoTerminate)
        {
            this.openTimeout = openTimeout;
            this.timeout = timeout;
            this.terminationString = terminationString;
            this.autoTerminate = autoTerminate;

            if (terminationString?.Length > 0)
            {
                terminationCharacter = Encoding.ASCII.GetBytes(terminationString)[0];
            }

            IMessage messageSession = (IMessage)resourceManager.Open(resourceName, AccessMode.NO_LOCK, openTimeout);
            messageSession.Timeout = timeout;
            MessageSession = messageSession;
        }

        public MessageStream(ResourceManagerClass resourceManager, ConnectionSettings connectionSettings, int openTimeout)
            : this(resourceManager, connectionSettings.AdvancedConnectionSettings.ToConnectionString(), openTimeout, connectionSettings.Timeout, connectionSettings.GetNewLineString(), !connectionSettings.WaitForTermination)
        {
            if (connectionSettings.AdvancedConnectionSettings is GpibPrologixConnectionSettings prologix)
            {
                Prologix = true;
                prologixConnectionSettings = prologix;
                ConfigurePrologixDevice();
            }
            else if (connectionSettings.AdvancedConnectionSettings is SerialConnectionSettings serialSettings && MessageSession is ISerial serialMessage)
            {
                serialMessage.BaudRate = (int)serialSettings.BaudRate;
                serialMessage.DataBits = (short)serialSettings.DataBits;
                serialMessage.FlowControl = (VisaComLib.SerialFlowControl)serialSettings.FlowControl;
                serialMessage.Parity = (VisaComLib.SerialParity)serialSettings.Parity;
                serialMessage.StopBits = (VisaComLib.SerialStopBits)serialSettings.StopBits;
                serialMessage.DataTerminalReadyState = serialSettings.DtrEnable ? LineState.STATE_ASSERTED : LineState.STATE_UNASSERTED;
                serialMessage.RequestToSendState = serialSettings.RtsEnable ? LineState.STATE_ASSERTED : LineState.STATE_UNASSERTED;
            }
        }

        public static object CommonRequestLocker { get; } = new object();

        private void ConfigurePrologixDevice()
        {
            lock (CommonRequestLocker)
            {
                WritePrologixCommand("mode 1");
                WritePrologixCommand("auto 0");
                WritePrologixCommand("eoi 1");
                WritePrologixCommand("eos 3");
                WritePrologixCommand("eot_enable 0");

                // Now we need to check if device is connected, this will throw timeout exception if device is not connected
                WritePrologixCommand($"read_tmo_ms {openTimeout / 2}");
                MessageSession.Timeout = openTimeout;

                while (true)
                {
                    UpdatePrologicAddress();
                    WritePrologixCommand($"spoll {prologixConnectionSettings.PrimaryAddress}");
                    string sb = MessageSession.ReadString(BufferLength).TrimEnd('\n', '\r');

                    if (int.TryParse(sb, out _))
                    {
                        break;
                    }
                }

                MessageSession.Timeout = timeout;
            }
        }

        private void UpdatePrologicAddress()
        {
            if (prologixConnectionSettings.SecondaryAddress.HasValue)
            {
                WritePrologixCommand($"addr {prologixConnectionSettings.PrimaryAddress} {prologixConnectionSettings.SecondaryAddress}");
            }
            else
            {
                WritePrologixCommand($"addr {prologixConnectionSettings.PrimaryAddress}");
            }
        }

        private void WritePrologixCommand(string command)
        {
            WriteInternal($"++{command}\n");
        }

        public IMessage MessageSession { get; }

        public bool Prologix { get; }

        public string ReadString()
        {
            int timeout = this.timeout;

            while (timeout >= 0)
            {
                lock (CommonRequestLocker)
                {
                    if (Prologix)
                    {
                        WritePrologixCommand($"read_tmo_ms {ReadTimeoutInternal / 2}");
                        UpdatePrologicAddress();
                        WritePrologixCommand($"read {terminationCharacter}");
                    }

                    try
                    {
                        if (MessageSession.Timeout > ReadTimeoutInternal)
                        {
                            MessageSession.Timeout = ReadTimeoutInternal;
                        }

                        string result = MessageSession.ReadString(BufferLength);

                        if (autoTerminate)
                        {
                            result = result.TrimEnd('\n').TrimEnd('\r');
                        }
                        else if (!(result.EndsWith('\r') || result.EndsWith('\n')))
                        {
                            throw new InvalidOperationException("Text not terminated");
                        }
                        else
                        {
                            result = result.TrimEnd('\n').TrimEnd('\r');
                        }

                        return result;
                    }
                    catch (COMException ex)
                    {
                        // timeout exception
                        if (ex.ErrorCode == -2147221483)
                        {
                            timeout -= ReadTimeoutInternal;
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                    finally
                    {
                        MessageSession.Timeout = this.timeout;
                    }
                }
            }

            throw new TimeoutException("Response timeout");
        }

        public void WriteString(string buffer)
        {
            lock (CommonRequestLocker)
            {
                WriteInternal(buffer + terminationString);
            }
        }

        private void WriteInternal(string buffer)
        {
            MessageSession.WriteString(buffer);
        }

        public void Dispose()
        {
            try
            {
                MessageSession.Close();
            }
            catch
            {

            }
        }
    }
}
