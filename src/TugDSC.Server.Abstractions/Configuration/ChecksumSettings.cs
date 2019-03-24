// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

namespace TugDSC.Server.Configuration
{
    public class ChecksumSettings
    {
        public ExtSettings Ext
        { get; set; }
        
        public string Default
        { get; set; } = "SHA-256";
    }
}