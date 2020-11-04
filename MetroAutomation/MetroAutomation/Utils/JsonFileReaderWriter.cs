using System.IO;
using System.Text.Json;

namespace MetroAutomation
{
    public class JsonFileReaderWriter
    {
        public static T ReadData<T>(string fileName) where T : new()
        {
            try
            {
                return JsonSerializer.Deserialize<T>(File.ReadAllText(fileName));
            }
            catch
            {
                return new T();
            }
        }

        public static bool WriteData<T>(string fileName, T data)
        {
            try
            {
                File.WriteAllText(fileName, JsonSerializer.Serialize(data));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
