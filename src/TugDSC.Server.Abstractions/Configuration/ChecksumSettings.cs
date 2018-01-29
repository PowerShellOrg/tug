/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licensed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

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