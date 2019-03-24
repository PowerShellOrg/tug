// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using TugDSC.Messages.ModelBinding;

namespace TugDSC.Messages
{
    public class GetConfigurationRequest : DscAgentRequest
    {
        public static readonly HttpMethod VERB = HttpMethod.Get;

        public const string ROUTE = "Nodes(AgentId='{AgentId}')/Configurations(ConfigurationName='{ConfigurationName}')/ConfigurationContent";
        public const string ROUTE_NAME = nameof(GetConfigurationRequest);

        [FromRoute]
        public string ConfigurationName
        { get; set; }

        /// <summary>
        /// TODO:  Resolve how this relates to the same parameter name in the URI. 
        /// https://msdn.microsoft.com/en-us/library/mt181633.aspx 
        /// </summary>
        [FromHeader(Name = "ConfigurationName")]
        public string ConfigurationNameHeader
        { get; set; }
    }

    public class GetConfigurationResponse : DscResponse
    {
        [ToHeader(Name = "Checksum")]
        public string ChecksumHeader
        { get; set; }

        [ToHeader(Name = "ChecksumAlgorithm")]
        public string ChecksumAlgorithmHeader
        { get; set; }

        [ToResult]
        public Stream Configuration
        { get; set; }
    }
}