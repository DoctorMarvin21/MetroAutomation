using MetroAutomation.Calibration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VisaComLib;

namespace MetroAutomation
{
    public class VisaComWrapper
    {
        public static MessageStream GetStream(string resourceName, int timout)
        {
            ResourceManagerClass resourceManagerClass = new ResourceManagerClass();
            IMessage messageSession = (IMessage)resourceManagerClass.Open(resourceName);
            messageSession.TerminationCharacterEnabled = false;
            messageSession.Timeout = timout;

            return new MessageStream(messageSession);
        }

        public static MessageStream GetStream(ConnectionSettings connectionSettings)
        {
            ResourceManagerClass resourceManagerClass = new ResourceManagerClass();
            IMessage messageSession = (IMessage)resourceManagerClass.Open(connectionSettings.AdvancedConnectionSettings.GenerateConnectionString());
            messageSession.Clear();
            messageSession.TerminationCharacterEnabled = false;
            messageSession.Timeout = connectionSettings.Timeout;

            // TODO: do other interfaces, not GPIB only

            return new MessageStream(messageSession);
        }

        public void Test()
        {
            try
            {
                ResourceManagerClass resourceManagerClass = new ResourceManagerClass();
                var resources = resourceManagerClass.FindRsrc("GPIB?*INSTR");

                IMessage messageSession = (IMessage)resourceManagerClass.Open("GPIB0::10::INSTR");

                messageSession.TerminationCharacterEnabled = false;
                messageSession.Timeout = 30000;

                MessageStream messageStream = new MessageStream(messageSession);

                StreamWriter writer = new StreamWriter(messageStream);
                writer.NewLine = "\n";
                writer.AutoFlush = true;

                StreamReader reader = new StreamReader(messageStream);
                writer.WriteLine("*IDN?");
                var result = reader.ReadLine();

                messageSession.Close();
            }
            catch (Exception ex)
            {

            }

            //var session = resourceManagerClass.Open()
        }
    }

    public class MessageStream : Stream
    {
        private IMessage messageSession;

        public MessageStream(IMessage messageSession)
        {
            this.messageSession = messageSession;
        }

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
            byte[] result = (byte[])messageSession.Read(buffer.Length);
            Array.Copy(result, 0, buffer, offset, result.Length);
            return result.Length;
        }

        public void WriteString(string text)
        {
            messageSession.WriteString(text);
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            Array bufferArray = buffer;
            messageSession.Write(ref bufferArray, count);
        }

        public string ReadString()
        {
            return messageSession.ReadString(1000);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    messageSession.Close();
                }
                catch
                {
                }
            }

            base.Dispose(disposing);
        }
    }
}
