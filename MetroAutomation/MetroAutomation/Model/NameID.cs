namespace MetroAutomation.Model
{
    public class NameID : IDataObject
    {
        public NameID()
        {
        }

        public NameID(int id, string name)
        {
            ID = id;
            Name = name;
        }

        public NameID(IDataObject dataObject)
        {
            ID = dataObject.ID;
            Name = dataObject.Name;
        }

        public int ID { get; set; }

        public string Name { get; set; }
    }
}
