// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace TugDSC.Model
{
    public class GetReportsAllResponseBody : Util.ExtDataIndexerBase
    {
        [JsonProperty(PropertyName = "value")]
        public IEnumerable<SendReportBody> Value
        { get; set; }
    }
}