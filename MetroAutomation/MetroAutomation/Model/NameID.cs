using System;

namespace MetroAutomation.Model
{
    public class NameID : IDataObject
    {
        public NameID()
        {
        }

        public NameID(Guid id, string name)
        {
            ID = id;
            Name = name;
        }

        public NameID(IDataObject dataObject)
        {
            ID = dataObject.ID;
            Name = dataObject.Name;
        }

        public Guid ID { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
