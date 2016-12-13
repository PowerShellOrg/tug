namespace Tug.Ext
{
    public class ExtInfo
    {
        public ExtInfo(string name,
            string label = null, string description = null)
        {
            Name = name;
            Label = label;
            Description = description;
        }

        public string Name
        { get; set; }

        public string Label
        { get; set; }

        public string Description
        { get; set; }
    }
}