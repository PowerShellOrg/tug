// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace TugDSC.Server
{
    public class DscHandlerConfig
    {
        public IDictionary<string, object> InitParams
        { get; set; }
    }
}