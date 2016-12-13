using System.Collections.Generic;

namespace Tug.Ext
{
    public interface IProviderManager<TProv, TProd>
        where TProv : IProvider<TProd>
        where TProd : IProviderProduct
    {
        IEnumerable<string> FoundProvidersNames
        { get; }

        TProv GetProvider(string name);
    }
}