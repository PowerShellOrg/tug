// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TugDSC.Model
{
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

    
    /*
     Response:
        {
            "odata.metadata":"http://10.50.1.5:8080/PSDSCPullServer.svc/$metadata#Edm.String",
            "value":"SavedReport"
        }     
     */

     // NOTE: the naming convention of this class is a bit different
     // because it is not strictly used by the DSC Request class
    public class SendReportBody : Util.ExtDataIndexerBase
    {
        public const string REPORT_DATE_FORMAT = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzzz";

        [Required]
        public Guid JobId
        { get; set; }

        // Appears to be one of these values (maybe turn this into an Enum?):
        //    * Initial
        //    * LocalConfigurationManager
        //    * Consistency
        [Required]
        public string OperationType
        { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DscRefreshMode? RefreshMode
        { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Status
        { get; set; }

        // IN TESTING AND OBSERVATION THIS FIELD DOES
        // NOT ALWAYS GET SENT EVEN BY THE SAME NODE
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NodeName
        { get; set; }

        /// <summary>
        /// This is assigned as a comma-separated list of IPv4 and IPv6
        /// addresses.
        /// </summary>
        // IN TESTING AND OBSERVATION THIS FIELD DOES
        // NOT ALWAYS GET SENT EVEN BY THE SAME NODE
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string IpAddress
        { get; set; }

        // IN TESTING AND OBSERVATION THIS FIELD DOES
        // NOT ALWAYS GET SENT EVEN BY THE SAME NODE
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LCMVersion
        { get; set; }

        [Required]
        public string ReportFormatVersion
        { get; set; }

        // IN TESTING AND OBSERVATION THIS FIELD DOES
        // NOT ALWAYS GET SENT EVEN BY THE SAME NODE
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ConfigurationVersion
        { get; set; }

        // START TIME IS ALWAYS PRESENT (END TIME IS NOT)
        // Example:  2016-08-15T15:21:08.9530000-07:00
        [Required]
        public string StartTime
        { get; set; }

        // END TIME IS SOMETIMES OMITTED (START TIME IS NOT)
        // Example:  2017-01-20T06:20:36.6950000-05:00
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EndTime
        { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DscTrueFalse? RebootRequested
        { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] Errors
        { get; set; } = CommonValues.EMPTY_STRINGS;

        // THIS TYPICALLY HAS A SINGLE STRING ENTRY
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] StatusData
        { get; set; } = CommonValues.EMPTY_STRINGS;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AdditionalDataItem[] AdditionalData
        { get; set; } = AdditionalDataItem.EMPTY_ITEMS;

        public class AdditionalDataItem : Util.ExtDataIndexerBase
        {
            public static readonly AdditionalDataItem[] EMPTY_ITEMS = new AdditionalDataItem[0];

            [Required]
            public string Key
            { get; set; }

            [Required]
            public string Value
            { get; set; }
        }
    }    
}