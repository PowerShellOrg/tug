/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace Tug
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