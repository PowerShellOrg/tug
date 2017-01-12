/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Tug.Messages.ModelBinding;
using Tug.Model;

namespace Tug.Messages
{
    public class GetDscActionRequest : DscAgentRequest
    {
        public static readonly HttpMethod VERB = HttpMethod.Post;

        public const string ROUTE = "Nodes(AgentId='{AgentId}')/GetDscAction";
        public const string ROUTE_NAME = nameof(GetDscActionRequest);

        [FromBody]
        [Required]
        public GetDscActionRequestBody Body
        { get; set; }

        public override object GetBody() => Body;
    }

    public class GetDscActionResponse : DscResponse
    {
        [ToResult]
        public GetDscActionResponseBody Body
        { get; set; }
    }
}