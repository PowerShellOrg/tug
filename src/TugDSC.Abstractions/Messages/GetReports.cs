// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using TugDSC.Messages.ModelBinding;
using TugDSC.Model;

namespace TugDSC.Messages
{
    public class GetReportsRequest : DscAgentRequest
    {
        public static readonly HttpMethod VERB = HttpMethod.Get;

        public const string ROUTE_SINGLE = "Nodes(AgentId='{AgentId}')/Reports(JobId='{JobId}')";
        public const string ROUTE_SINGLE_NAME = nameof(GetReportsRequest) + "Single";

        public const string ROUTE_ALL = "Nodes(AgentId='{AgentId}')/Reports()";
        public const string ROUTE_ALL_NAME = nameof(GetReportsRequest) + "All";

        public const string ROUTE_ALL_ALT = "Nodes(AgentId='{AgentId}')/Reports";
        public const string ROUTE_ALL_ALT_NAME = nameof(GetReportsRequest) + "AllAlt";


        [FromRoute]
        public Guid? JobId
        { get; set; }
    }

    public class GetReportsSingleResponse : DscResponse
    {
        [ToResult]
        public SendReportBody Body
        { get; set; }
    }

    public class GetReportsAllResponse : DscResponse
    {
        [ToResult]
        public GetReportsAllResponseBody Body
        { get; set; }
    }
}