/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using Microsoft.AspNetCore.Mvc;
using Tug.Model;

namespace Tug.Messages
{
    public class SendReportRequest : DscAgentRequest
    {
        [FromBody]
        public SendReportRequestBody Body
        { get; set; }
    }
}