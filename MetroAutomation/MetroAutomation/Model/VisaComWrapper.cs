using MetroAutomation.Calibration;
using System.Collections.Generic;
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
                    stream.MessageSession.WriteString("++read 10\n");
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
                                stream.MessageSession.WriteString("++read 10\n");
                                stream.MessageSession.Read(512);
                            }
                            catch
                            {
                            }

                            stream.MessageSession.WriteString("*IDN?\n");
                            stream.MessageSession.WriteString("++read 10\n");

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
