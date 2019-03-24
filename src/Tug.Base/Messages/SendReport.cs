// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Tug.Model;

namespace Tug.Messages
{
    public class SendReportRequest : DscAgentRequest
    {
        public static readonly HttpMethod VERB = HttpMethod.Post;

        public const string ROUTE = "Nodes(AgentId='{AgentId}')/SendReport";
        public const string ROUTE_NAME = nameof(SendReportRequest);

        [FromBody]
        [Required(AllowEmptyStrings = true)]
        public SendReportBody Body
        { get; set; }

        public override bool HasStrictBody() => false;

        public override object GetBody() => Body;
    }
}