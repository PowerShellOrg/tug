/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.IO;

namespace Tug.Server
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