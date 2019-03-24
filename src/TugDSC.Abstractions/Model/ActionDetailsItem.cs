// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations;

namespace TugDSC.Model
{
    public class ActionDetailsItem : Util.ExtDataIndexerBase
    {
        [Required(AllowEmptyStrings = true)]
        public string ConfigurationName
        { get; set; } = string.Empty;

        [Required]
        [EnumDataTypeAttribute(typeof(DscActionStatus))]
        public DscActionStatus Status
        { get; set; }
    }
}