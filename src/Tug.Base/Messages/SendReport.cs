/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Tug.Model;

namespace Tug.Messages
{
    public class SendReportRequest : DscAgentRequest
    {
        public static readonly HttpMethod VERB = HttpMethod.Post;

        public const string ROUTE = "Nodes(AgentID='{AgentId}')/SendReport";
        public const string ROUTE_NAME = nameof(SendReportRequest);

        [FromBody]
        public SendReportRequestBody Body
        { get; set; }

        public override object GetBody() => Body;
    }
}