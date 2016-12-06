using System;
using System.Collections.Generic;
using System.IO;

namespace tug
{
    public interface IChecksumAlgorithmProvider
    {
        IChecksumAlgorithm GetChecksumAlgorithm(IDictionary<string, object> initParams = null);
    }

    public interface IChecksumAlgorithm : IDisposable
    {
        bool IsDisposed
        { get; }
        
        string AlgorithmName
        { get; }

        string ComputeChecksum(byte[] bytes);

        string ComputeChecksum(Stream stream);
    }
}