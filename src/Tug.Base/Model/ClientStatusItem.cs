/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.ComponentModel.DataAnnotations;

namespace Tug.Model
{
    public class ClientStatusItem : Util.ExtDataIndexerBase
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