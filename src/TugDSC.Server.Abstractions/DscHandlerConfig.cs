/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licensed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.Collections.Generic;

namespace TugDSC.Server
{
    public class DscHandlerConfig
    {
        public IDictionary<string, object> InitParams
        { get; set; }
    }
}