// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Tug.Model
{
    public class ClientStatusItem : Util.ExtDataIndexerBase
    {
        // NOTE:  DO NOT CHANGE THE ORDER OF THESE PROPERTIES!!!
        // Apparently the order of these properties is important
        // to successfully satisfy the strict input validation

        // Based on testing and observation, this property
        // is completely omitted when it has no value
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ConfigurationName
        { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Checksum
        { get; set; }

        [Required]
        [CustomValidation(typeof(ClientStatusItem),
        nameof(ValidateChecksumAlgorithm))]
        public string ChecksumAlgorithm
        { get; set; } 

        public static ValidationResult ValidateChecksumAlgorithm(string value)
        {
            return "SHA-256" == value
                ? ValidationResult.Success
                : new ValidationResult("unsupported or unknown checksum algorithm");
        }
    }
}