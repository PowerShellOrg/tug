using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

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

    public static class GetModuleResponse
    {
        public const string ChecksumHeader = "Checksum";
        public const string ChecksumAlgorithmHeader = "ChecksumAlgorithm";
    }
}