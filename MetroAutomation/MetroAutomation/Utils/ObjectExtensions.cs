using MetroAutomation.ViewModel;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MetroAutomation
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString()).FirstOrDefault();
            var description = memberInfo?.GetCustomAttribute<DescriptionAttribute>()?.Description;

            return description ?? value.ToString();
        }

        public static string GetDescription(this Enum value, DescriptionType descriptionType)
        {
            return ExtendedDescriptionAttribute.GetDescription(value, descriptionType);
        }

        public static T[] GetValues<T>()
            where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }
    }

    public static class DecimalExtension
    {
        public static decimal Normalize(this decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }

        public static decimal Round(this decimal value, int decimals)
        {
            return Math.Round(value * 1.000000000000000000000000000000000m, decimals);
        }
    }

    public static class SerializationExtensions
    {
        public static T BinaryDeepClone<T>(this T source)
        {
            if (source == null)
            {
                return default;
            }

            IFormatter formatter = new BinaryFormatter();

            Stream stream = new MemoryStream();

            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        public static bool DeepBinaryEquals<T>(this T source, T item)
        {
            IFormatter formatter = new BinaryFormatter();

            using Stream stream1 = new MemoryStream();
            using Stream stream2 = new MemoryStream();

            formatter.Serialize(stream1, source);
            stream1.Seek(0, SeekOrigin.Begin);

            formatter.Serialize(stream2, item);
            stream2.Seek(0, SeekOrigin.Begin);

            while (true)
            {
                var byte1 = stream1.ReadByte();
                var byte2 = stream2.ReadByte();

                if (byte1 != byte2)
                {
                    return false;
                }
                else if (byte1 == -1 && byte2 == -1)
                {
                    return true;
                }
            }
        }
    }
}
