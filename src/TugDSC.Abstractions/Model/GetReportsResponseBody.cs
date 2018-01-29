/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

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