using MetroAutomation.Calibration;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using VisaComLib;

namespace MetroAutomation.Model
{
    public class MessageStream : Stream
    {
        private const int ReadTimeoutInternal = 1000;
        private readonly int timeout;
        private readonly int openTimeout;
        private readonly byte terminationCharacter;

        private readonly GpibPrologixConnectionSettings prologixConnectionSettings;

        public MessageStream(ResourceManagerClass resourceManager, string resourceName, int openTimeout, int timeout)
        {
            this.openTimeout = openTimeout;
            this.timeout = timeout;
            IMessage messageSession = (IMessage)resourceManager.Open(resourceName, AccessMode.NO_LOCK, openTimeout);
            messageSession.TerminationCharacterEnabled = false;
            messageSession.Timeout = timeout;
            MessageSession = messageSession;
        }

        public MessageStream(ResourceManagerClass resourceManager, ConnectionSettings connectionSettings, int openTimeout)
            : this(resourceManager, connectionSettings.AdvancedConnectionSettings.ToConnectionString(), openTimeout, connectionSettings.Timeout)
        {
            if (connectionSettings.Termination == Termination.Lf)
            {
                terminationCharacter = 10;
            }
            else if (connectionSettings.Termination == Termination.Cr)
            {
                terminationCharacter = 13;
            }

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
                    WritePrologixCommand("spoll");
                    string sb = MessageSession.ReadString(512).TrimEnd('\n', '\r');

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
            byte[] commandBytes = Encoding.ASCII.GetBytes($"++{command}\n");
            WriteInternal(commandBytes, 0, commandBytes.Length);
        }

        public IMessage MessageSession { get; }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public bool Prologix { get; }

        public override long Length => throw new NotImplementedException();

        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
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

                        byte[] result = (byte[])MessageSession.Read(count);
                        Array.Copy(result, 0, buffer, offset, result.Length);

                        return result.Length;
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

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (CommonRequestLocker)
            {
                // Discarding input buffer
                try
                {
                    if (Prologix)
                    {
                        UpdatePrologicAddress();
                        
                        WritePrologixCommand($"read {terminationCharacter}");
                    }

                    MessageSession.Timeout = 50;
                    MessageSession.Read(512);
                }
                catch
                {
                }
                finally
                {
                    MessageSession.Timeout = timeout;
                }

                WriteInternal(buffer, offset, count);
            }
        }

        private void WriteInternal(byte[] buffer, int offset, int count)
        {
            byte[] bytesWithOffet = new byte[buffer.Length - offset];
            Array.Copy(buffer, offset, bytesWithOffet, 0, buffer.Length - offset);

            Array bufferArray = bytesWithOffet;
            MessageSession.Write(ref bufferArray, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    MessageSession.Close();
                }
                catch
                {
                }
            }

            base.Dispose(disposing);
        }
    }
}
