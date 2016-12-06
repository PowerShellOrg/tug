using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace tug.Messages
{
    public class SendReportRequest : DscAgentRequest
    {
        [FromBody]
        public SendReportRequestBody Body
        { get; set; }
    }

    /*
    {
        "title": "SendReport request schema",
        "type": "object",
        "properties": {
            "JobId": {
                "type": [
                    "string",
                    "null"
                ],
                "required": "true"
            },
            "OperationType": {
                "type": [
                    "string",
                    "null"
                ]
            },
            "RefreshMode": {
                "enum": [
                    "Push",
                    "Pull"
                ]
            },
            "Status": {
                "type": [
                    "string",
                    "null"
                ]
            },
            "LCMVersion": {
                "type": [
                    "string",
                    "null"
                ]
            },
            "ReportFormatVersion": {
                "type": [
                    "string",
                    "null"
                ]
            },
            "ConfigurationVersion": {
                "type": [
                    "string",
                    "null"
                ]
            },
            "NodeName": {
                "type": [
                    "string",
                    "null"
                ]
            },
            "IpAddress": {
                "type": [
                    "string",
                    "null"
                ]
            },
            "StartTime": {
                "type": [
                    "string",
                    "null"
                ]
            },
            "EndTime": {
                "type": [
                    "string",
                    "null"
                ]
            },
            "RebootRequested": {
                "enum": [
                    "True",
                    "False"
                ]
            },
            "Errors": {
                "type": [
                    "string",
                    "null"
                ]
            },
            "StatusData": {
                "type": [
                    "string",
                    "null"
                ]
            },
            "AdditionalData": {
                "type": "array",
                "required": false,
                "items": [
                    {
                        "type": "object",
                        "required": true,
                        "properties": {
                            "Key": {
                                "type": "string",
                                "required": true
                            },
                            "Value": {
                                "type": "string",
                                "required": true
                            }
                        }
                    }
                ]
            }
        }
    }
    */

    public class SendReportRequestBody
    {
        [Required]
        public Guid JobId
        { get; set; }

        public string OperationType
        { get; set; }

        public DscRefreshMode? RefreshMode
        { get; set; }

        public string Status
        { get; set; }

        public string LCMVersion
        { get; set; }

        public string ReportFormatVersion
        { get; set; }

        public string ConfigurationVersion
        { get; set; }

        public string NodeName
        { get; set; }

        public string IpAddress
        { get; set; }

        public string StartTime
        { get; set; }

        public string EndTime
        { get; set; }

        public DscTrueFalse RebootRequested
        { get; set; }

        public string[] Errors
        { get; set; }

        public string[] StatusData
        { get; set; }

        public AdditionalDataItem[] AdditionalData
        { get; set; }

        public class AdditionalDataItem
        {
            [Required]
            public string Key
            { get; set; }

            [Required]
            public string Value
            { get; set; }
        }
    }
}