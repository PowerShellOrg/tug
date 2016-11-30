using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace tug.Messages
{
    public class GetDscActionRequest : DscRequest
    {
        [FromBody]
        [Required]
        public GetDscActionRequestBody Body
        { get; set; } 
    }

    /*
    {
        "title": "GetDscAction request schema (AgentID)",
        "type": "object",
        "properties": {
            "ClientStatus": {
                "type": "array",
                "minItems": 1,
                "items": [
                    {
                        "type": "object",
                        "properties": {
                            "Checksum": {
                                "type": [
                                    "string",
                                    "null"
                                ]
                            },
                            "ConfigurationName": {
                                "type": [
                                    "string",
                                    "null"
                                ]
                            },
                            "ChecksumAlgorithm": {
                                "enum": [
                                    "SHA-256"
                                ],
                                "description": "Checksum algorithm used to generate checksum"
                            }
                        }
                    }
                ],
                "uniqueItems": true
            }
        }
    }
    */
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

            [CustomValidation(typeof(GetDscActionRequestBody),
            nameof(ValidateChecksumAlgorithm))]
            public string ChecksumAlgorithm
            { get; set; } 

            public string Checksum
            { get; set; }

            public static bool ValidateChecksumAlgorithm(string value)
            {
                return "SHA-256" == value;
            }
        }
    }

    /*
    {
        "title": "GetDscAction response",
        "type": "object",
        "properties": {
            "NodeStatus": {
                "enum": [
                    "OK",
                    "RETRY",
                    "GetConfiguration",
                    "UpdateMetaConfiguration"
                ],
                "required": "true"
            },
            "Details": {
                "type": "array",
                "required": false,
                "items": [
                    {
                        "type": "object",
                        "required": true,
                        "properties": {
                            "ConfigurationName": {
                                "type": "string",
                                "required": true
                            },
                            "Status": {
                                "enum": [
                                    "OK",
                                    "RETRY",
                                    "GetConfiguration",
                                    "UpdateMetaConfiguration"
                                ],
                                "required": true
                            }
                        }
                    }
                ]
            }
        }
    }
    */
    public class GetDscActionResponseBody
    {
        [Required]
        public DscActionStatus NodeStatus
        { get; set; }

        public DetailsItem[] Details
        { get; set; }

        public class DetailsItem
        {
            [Required]
            public string ConfigurationName
            { get; set; }

            [Required]
            public DscActionStatus Status
            { get; set; }
        }
    }
}