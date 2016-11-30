using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace tug.Messages
{
    public class GetConfigurationRequest : DscAgentRequest
    {
        public string ConfigurationName
        { get; set; }

        [Required]
        [FromHeader(Name = "ProtocolVersion")]
        public string ProtocolVersionHeader
        { get; set; }

        /// <summary>
        /// TODO:  Resolve how this relates to the same parameter name in the URI. 
        /// https://msdn.microsoft.com/en-us/library/mt181633.aspx 
        /// </summary>
        [FromHeader(Name = "ConfigurationName")]
        public string ConfigurationNameHeader
        { get; set; }
    }

    public static class GetConfigurationResponse
    {
        public const string ChecksumHeader = "Checksum";
        public const string ChecksumAlgorithmHeader = "ChecksumAlgorithm";
    }
}