using MetroAutomation.Calibration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using VisaComLib;

namespace MetroAutomation.Model
{
    public class ConnectedDeviceInfo
    {
        public ConnectedDeviceInfo(string deviceName, string resourceName)
        {
            DeviceName = deviceName;
            ResourceName = resourceName;
        }

        public string DeviceName { get; }

        public string ResourceName { get; }
    }

    public class VisaComWrapper
    {
        public static MessageStream GetStream(ConnectionSettings connectionSettings)
        {
            return new MessageStream(new ResourceManagerClass(), connectionSettings, 2000);
        }

        public static ConnectedDeviceInfo[] GetDevicesList()
        {
            lock (MessageStream.CommonRequestLocker)
            {
                try
                {
                    ResourceManagerClass resourceManagerClass = new ResourceManagerClass();
                    string[] resources = resourceManagerClass.FindRsrc("?*INSTR").Cast<string>().ToArray();
                    List<ConnectedDeviceInfo> devices = new List<ConnectedDeviceInfo>();

                    for (int i = 0; i < resources.Length; i++)
                    {
                        string resource = resources[i];
                        string deviceName;
                        try
                        {
                            var stream = new MessageStream(resourceManagerClass, resource, 100, 100);
                            stream.Write(Encoding.ASCII.GetBytes("*IDN?\n"));

                            byte[] idnBytes = new byte[512];
                            int readCount = stream.Read(idnBytes);

                            deviceName = Encoding.ASCII.GetString(idnBytes, 0, readCount).TrimEnd('\n', '\r');

                            stream.Dispose();
                        }
                        catch
                        {
                            deviceName = resource;
                        }

                        if (resource.StartsWith(ConnectionUtils.Tags[ConnectionType.Serial]) && TryGetPrologixDevices(resource, resourceManagerClass, out var prologix))
                        {
                            devices.AddRange(prologix);
                        }
                        else
                        {
                            devices.Add(new ConnectedDeviceInfo(deviceName, resource));
                        }
                    }

                    return devices.ToArray();
                }
                catch
                {
                    return new ConnectedDeviceInfo[0];
                }
            }
        }

        private static bool TryGetPrologixDevices(string resource, ResourceManagerClass resourceManagerClass, out ConnectedDeviceInfo[] prologixDevices)
        {
            lock (MessageStream.CommonRequestLocker)
            {
                List<ConnectedDeviceInfo> result = new List<ConnectedDeviceInfo>();

                try
                {
                    MessageStream stream;
                    AdvancedConnectionSettings advancedSettings;
                    try
                    {
                        advancedSettings = ConnectionUtils.GetConnectionSettingsByResourceName(resource);
                        var settings = new ConnectionSettings
                        {
                            Timeout = 100,
                            AdvancedConnectionSettings = advancedSettings
                        };

                        stream = new MessageStream(resourceManagerClass, settings, 100);

                        // buffer clear
                        try
                        {
                            stream.MessageSession.Read(512);
                        }
                        catch
                        {
                        }

                        stream.MessageSession.WriteString("++ver\n");
                        var version = stream.MessageSession.ReadString(512);

                        if (!version.StartsWith("Prologix"))
                        {
                            prologixDevices = null;
                            return false;
                        }
                    }
                    catch
                    {
                        prologixDevices = null;
                        return false;
                    }

                    stream.MessageSession.WriteString($"++mode 10\n");
                    stream.MessageSession.WriteString($"++auto 0\n");
                    stream.MessageSession.WriteString($"++eoi 1\n");
                    stream.MessageSession.WriteString($"++eos 3\n");
                    stream.MessageSession.WriteString($"++eot_enable 0\n");
                    stream.MessageSession.WriteString($"++read_tmo_ms 70\n");

                    // buffer clear
                    try
                    {
                        stream.MessageSession.WriteString("++read eoi\n");
                        stream.MessageSession.Read(512);
                    }
                    catch
                    {
                    }

                    for (int i = 0; i <= 30; i++)
                    {
                        try
                        {
                            while (true)
                            {
                                stream.MessageSession.WriteString($"++addr {i}\n");
                                stream.MessageSession.WriteString($"++spoll {i}\n");
                                string sb = stream.MessageSession.ReadString(512).TrimEnd('\n', '\r');

                                if (int.TryParse(sb, out _))
                                {
                                    break;
                                }
                            }

                            string resourceName = $"{ConnectionUtils.Tags[ConnectionType.GpibPrologix]}{advancedSettings.BoardIndex}{ConnectionUtils.Splitter}{i}{ConnectionUtils.Splitter}{ConnectionUtils.InstrumentTag}";
                            string deviceName = resourceName;

                            try
                            {
                                // buffer clear
                                try
                                {
                                    stream.MessageSession.WriteString("++read eoi\n");
                                    stream.MessageSession.Read(512);
                                }
                                catch
                                {
                                }

                                stream.MessageSession.WriteString("*IDN?\n");
                                stream.MessageSession.WriteString("++read eoi\n");

                                deviceName = stream.MessageSession.ReadString(512).TrimEnd('\n', '\r');
                            }
                            catch
                            {
                            }

                            result.Add(new ConnectedDeviceInfo(deviceName, resourceName));
                        }
                        catch
                        {

                        }
                    }
                }
                catch
                {
                }

                if (result.Count > 0)
                {
                    prologixDevices = result.ToArray();
                    return true;
                }
                else
                {
                    prologixDevices = null;
                    return false;
                }
            }
        }
    }

    public class MessageStream : Stream
    {
        private const int ReadTimeoutInternal = 1000;
        private readonly int timeout;

        private readonly GpibPrologixConnectionSettings prologixConnectionSettings;

        public MessageStream(ResourceManagerClass resourceManager, string resourceName, int openTimeout, int timeout)
        {
            this.timeout = timeout;
            IMessage messageSession = (IMessage)resourceManager.Open(resourceName, AccessMode.NO_LOCK, openTimeout);
            messageSession.TerminationCharacterEnabled = false;
            messageSession.Timeout = timeout;
            MessageSession = messageSession;
        }

        public MessageStream(ResourceManagerClass resourceManager, ConnectionSettings connectionSettings, int openTimeout)
            : this(resourceManager, connectionSettings.AdvancedConnectionSettings.ToConnectionString(), openTimeout, connectionSettings.Timeout)
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
                WritePrologixCommand("read_tmo_ms 1100");
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
                if (Prologix)
                {
                    UpdatePrologicAddress();
                    WritePrologixCommand("read eoi");
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

            throw new TimeoutException("Response timeout");
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            // Discarding input buffer
            try
            {
                if (Prologix)
                {
                    UpdatePrologicAddress();
                    WritePrologixCommand("read eoi");
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
