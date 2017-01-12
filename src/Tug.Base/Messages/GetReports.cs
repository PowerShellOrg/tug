/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace Tug.Messages
{
    public class GetReportsRequest : DscAgentRequest
    {
        public static readonly HttpMethod VERB = HttpMethod.Get;

        public const string ROUTE = "Nodes(AgentId='{AgentId}')/Reports(JobId='{JobId}'))";
        public const string ROUTE_NAME = nameof(GetReportsRequest);

        [Required]
        public Guid JobId
        { get; set; }
    }
}