// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

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