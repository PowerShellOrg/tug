using System.Collections.Generic;

namespace Tug.Ext
{
    public interface IExtManager<TExt, TAtt>
        where TExt : IExtension
        where TAtt : ExtensionAttribute
    {
        IEnumerable<string> FoundExtensionNames
        { get; }

        TExt GetExtension(string name);
    }
}