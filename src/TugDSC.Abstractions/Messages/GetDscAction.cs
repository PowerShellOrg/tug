// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using TugDSC.Messages.ModelBinding;
using TugDSC.Model;

namespace TugDSC.Messages
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