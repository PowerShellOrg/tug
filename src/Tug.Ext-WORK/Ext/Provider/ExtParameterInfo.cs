namespace Tug.Ext
{
    public class ExtParameterInfo
    {
        public ExtParameterInfo(string name,
            bool isRequired = false,
            string label = null,
            string description = null)
        {
            Name = name;

            IsRequired = false;
            Label = label;
            Description = description;
        }

        public string Name
        { get; private set; }

        public bool IsRequired
        { get; private set; }

        public string Label
        { get; private set; }

        public string Description
        { get; private set; }
    }
}