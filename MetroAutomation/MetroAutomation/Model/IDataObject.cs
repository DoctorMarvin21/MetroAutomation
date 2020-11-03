using System;

namespace MetroAutomation.Model
{
    public interface IDataObject
    {
        public Guid ID { get; set; }

        public string Name { get; set; }
    }
}
