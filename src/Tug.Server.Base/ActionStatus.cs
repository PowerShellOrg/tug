// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

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