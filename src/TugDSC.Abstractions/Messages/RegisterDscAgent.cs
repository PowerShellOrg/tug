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
    public class RegisterDscAgentRequest : DscAgentRequest
    {
        public static readonly HttpMethod VERB = HttpMethod.Put;

        public const string ROUTE = "Nodes(AgentId='{AgentId}')";
        public const string ROUTE_NAME = nameof(RegisterDscAgentRequest);

        [FromBody]
        [Required]
        public RegisterDscAgentRequestBody Body
        { get; set; }

        public override object GetBody() => Body;
    }

    public class RegisterDscAgentResponse : DscResponse
    {
        /// <summary>
        /// We only need a single instance since there are
        /// no mutable elements in the object graph.
        /// </summary>
        public static readonly RegisterDscAgentResponse INSTANCE =
                new RegisterDscAgentResponse();

        [ToResult]
        public NoContentResult Body
        { get; } = new NoContentResult();
    }
}