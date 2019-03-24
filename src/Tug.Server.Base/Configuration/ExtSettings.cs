// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

namespace Tug.Server.Configuration
{
    public class ExtSettings
    {
        public bool ReplaceExtAssemblies
        { get; set; }

        public string[] SearchAssemblies
        { get; set; }

        public bool ReplaceExtPaths
        { get; set; }

        public string[] SearchPaths
        { get; set; }
    }
}