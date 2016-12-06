using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using tug.Controllers;

namespace tug.Messages
{
    public class GetModuleRequest : DscRequest
    {
        [FromRoute]
        [Required]
        public string ModuleName
        { get; set; }

        [FromRoute]
        public string ModuleVersion
        { get; set; }
    }

    public class GetModuleResponse : DscResponse
    {
        [ToHeaderAttribute(Name = "Checksum")]
        public string ChecksumHeader
        { get; set; }

        [ToHeader(Name = "ChecksumAlgorithm")]
        public string ChecksumAlgorithmHeader
        { get; set; }

        [ToResult]
        public Stream Module
        { get; set; }
    }
}