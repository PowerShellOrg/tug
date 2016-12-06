using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using tug.Controllers;

namespace tug.Messages
{
    public class GetDscActionRequest : DscAgentRequest
    {
        [FromBody]
        [Required]
        public GetDscActionRequestBody Body
        { get; set; } 
    }

    public class GetDscActionResponse : DscResponse
    {
        [ToResult]
        public GetDscActionResponseBody Body
        { get; set; }
    }

    public class GetDscActionRequestBody
    {
        [Required]
        [MinLengthAttribute(1)]
        public ClientStatusItem[] ClientStatus
        { get; set; }
        
        public class ClientStatusItem
        {
            public string ConfigurationName
            { get; set; }

            [CustomValidation(typeof(ClientStatusItem),
            nameof(ValidateChecksumAlgorithm))]
            public string ChecksumAlgorithm
            { get; set; } 

            public string Checksum
            { get; set; }

            public static ValidationResult ValidateChecksumAlgorithm(string value)
            {
                return "SHA-256" == value
                    ? ValidationResult.Success
                    : new ValidationResult("unsupported or unknown checksum algorithm");
            }
        }
    }

    public class GetDscActionResponseBody
    {
        [Required]
        [EnumDataTypeAttribute(typeof(DscActionStatus))]
        public DscActionStatus NodeStatus
        { get; set; }

        public DetailsItem[] Details
        { get; set; }

        public class DetailsItem
        {
            [Required]
            public string ConfigurationName
            { get; set; } = string.Empty;

            [Required]
            [EnumDataTypeAttribute(typeof(DscActionStatus))]
            public DscActionStatus Status
            { get; set; }
        }
    }
}