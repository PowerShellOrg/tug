/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.Collections.Generic;
using Tug.Model;

namespace Tug.Server
{
    public class ActionStatus
    {
        public DscActionStatus NodeStatus
        { get; set; }

        public IEnumerable<ActionDetailsItem> ConfigurationStatuses
        { get; set; }
    }
}