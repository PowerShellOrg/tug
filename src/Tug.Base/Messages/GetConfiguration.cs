/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Tug.Messages.ModelBinding;

namespace Tug.Messages
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