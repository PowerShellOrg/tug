using System.Collections.Generic;

namespace Tug.Ext
{
    public interface IExtManagerX<TE, TEP>
        where TE : IExtension
        where TEP: IExtProviderX<TE>
    {
        IEnumerable<TEP> FoundProviders
        { get; }

        TEP GetProvider(string name);
    }
}