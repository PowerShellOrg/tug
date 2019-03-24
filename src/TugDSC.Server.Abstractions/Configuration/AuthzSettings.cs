// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace TugDSC.Server.Configuration
{
    public class AuthzSettings
    {
        // TODO:  Need to think about the extensibility model for
        // authorization, if we want to use the Provider mechanism

        // public ExtSettings Ext
        // { get; set; }
        
        // [Required]
        // public string Provider
        // { get; set; }

        // This has to be concrete class, not interface to
        // be able to construct during deserialization
        public Dictionary<string, object> Params
        { get; set; }
    }
}