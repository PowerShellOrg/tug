/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licensed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.Collections.Generic;
using TugDSC.Model;

namespace TugDSC.Server
{
    public class ActionStatus
    {
        public DscActionStatus NodeStatus
        { get; set; }

        public IEnumerable<ActionDetailsItem> ConfigurationStatuses
        { get; set; }
    }
}