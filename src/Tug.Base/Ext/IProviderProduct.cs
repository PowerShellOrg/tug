using System;

namespace Tug.Ext
{
    // Alternative Names:
    // public interface IProviderYield
    // public interface IProviderResult
    // public interface IProviderOutput
    public interface IProviderProduct : IDisposable
    {
        bool IsDisposed
        { get; }
    }
}