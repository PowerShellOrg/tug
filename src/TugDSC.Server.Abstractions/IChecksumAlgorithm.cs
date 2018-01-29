/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licensed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.IO;
using TugDSC.Ext;

namespace TugDSC
{
    public interface IChecksumAlgorithm : IProviderProduct
    {
        string AlgorithmName
        { get; }

        string ComputeChecksum(byte[] bytes);

        string ComputeChecksum(Stream stream);
    }
}