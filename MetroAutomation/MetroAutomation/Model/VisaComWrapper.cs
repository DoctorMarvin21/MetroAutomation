using MetroAutomation.Calibration;
using System;
using System.IO;
using System.Linq;
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
        public static MessageStream GetStream(ResourceManagerClass resourceManagerClass, string resourceName, int openTimeout, int timeout)
        {
            IMessage messageSession = (IMessage)resourceManagerClass.Open(resourceName, AccessMode.NO_LOCK, openTimeout);
            messageSession.Clear();
            messageSession.TerminationCharacterEnabled = false;
            messageSession.Timeout = timeout;

            return new MessageStream(messageSession);
        }

        public static MessageStream GetStream(ConnectionSettings connectionSettings)
        {
            var stream = GetStream(new ResourceManagerClass(), connectionSettings.AdvancedConnectionSettings.ToConnectionString(), 2000, connectionSettings.Timeout);

            if (connectionSettings.AdvancedConnectionSettings is SerialConnectionSettings serialSettings && stream is ISerial serialMessage)
            {
                serialMessage.BaudRate = (int)serialSettings.BaudRate;
                serialMessage.DataBits = (short)serialSettings.DataBits;
                serialMessage.FlowControl = (VisaComLib.SerialFlowControl)serialSettings.FlowControl;
                serialMessage.Parity = (VisaComLib.SerialParity)serialSettings.Parity;
                serialMessage.StopBits = (VisaComLib.SerialStopBits)serialSettings.StopBits;
                serialMessage.DataTerminalReadyState = serialSettings.DtrEnable ? LineState.STATE_ASSERTED : LineState.STATE_UNASSERTED;
                serialMessage.RequestToSendState = serialSettings.RtsEnable ? LineState.STATE_ASSERTED : LineState.STATE_UNASSERTED;
            }

            return stream;
        }

        public static ConnectedDeviceInfo[] GetDevicesList()
        {
            try
            {
                ResourceManagerClass resourceManagerClass = new ResourceManagerClass();
                string[] resources = resourceManagerClass.FindRsrc("?*INSTR").Cast<string>().ToArray();
                ConnectedDeviceInfo[] devices = new ConnectedDeviceInfo[resources.Length];

                for (int i = 0; i < resources.Length; i++)
                {
                    string resource = resources[i];
                    string deviceName;
                    try
                    {
                        var stream = GetStream(resourceManagerClass, resource, 100, 100);
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

                    devices[i] = new ConnectedDeviceInfo(deviceName, resource);
                }

                return devices;
            }
            catch
            {
                return new ConnectedDeviceInfo[0];
            }
        }
    }

    public class MessageStream : Stream
    {
        public MessageStream(IMessage messageSession)
        {
            MessageSession = messageSession;
        }

        public IMessage MessageSession { get; }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

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
            byte[] result = (byte[])MessageSession.Read(buffer.Length);
            Array.Copy(result, 0, buffer, offset, result.Length);
            return result.Length;
        }

        public void WriteString(string text)
        {
            MessageSession.WriteString(text);
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            Array bufferArray = buffer;
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
