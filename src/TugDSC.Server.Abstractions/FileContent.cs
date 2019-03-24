// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.IO;

namespace TugDSC.Server
{
    public class FileContent
    {
        public string ChecksumAlgorithm
        { get; set; }

        public string Checksum
        { get; set; }

        public Stream Content
        { get; set; }
    }
}