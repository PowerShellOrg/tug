using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using tug.Controllers;

namespace tug.Messages
{
    public class GetConfigurationRequest : DscAgentRequest
    {
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