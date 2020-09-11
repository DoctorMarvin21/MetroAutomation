using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace MetroAutomation.ViewModel
{
    public enum DescriptionType
    {
        Normal,
        Short,
        Full
    }

    public class ExtendedDescriptionAttribute : DescriptionAttribute
    {
        public ExtendedDescriptionAttribute(string shortDescription, string description, string fullDescription)
            : base(description)
        {
            ShortDescription = shortDescription;
            FullDescription = fullDescription;
        }

        public string ShortDescription { get; }

        public string FullDescription { get; }

        public string GetDescription(DescriptionType type)
        {
            switch (type)
            {
                case DescriptionType.Normal:
                    {
                        return Description;
                    }
                case DescriptionType.Short:
                    {
                        return ShortDescription;
                    }
                case DescriptionType.Full:
                    {
                        return FullDescription;
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }

        public static string GetDescription(Enum value, DescriptionType descriptionType)
        {
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString()).FirstOrDefault();
            var description = memberInfo?.GetCustomAttribute<ExtendedDescriptionAttribute>()?.GetDescription(descriptionType);

            return description ?? value.ToString();
        }
    }
}
