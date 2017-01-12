/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.Collections.Generic;

namespace Tug.Server.Configuration
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