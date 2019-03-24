// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

namespace TugDSC.Client
{
    // TODO:  maybe we should make this a disposable and the
    // Content should be a stream (that would be cleaned up)
    // and we could provide convenience methods to get bytes,
    // etc.  -- SOMETHING TO CONSIDER...
    
    public class FileResponse
    {
        public string ChecksumAlgorithm
        { get; set; }

        public string Checksum
        { get; set; }

        public byte[] Content
        { get; set; }
    }
}