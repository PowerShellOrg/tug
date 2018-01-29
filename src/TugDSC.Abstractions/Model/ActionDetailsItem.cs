/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licensed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

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