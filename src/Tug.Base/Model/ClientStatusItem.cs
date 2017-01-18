/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

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

        [Required]
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