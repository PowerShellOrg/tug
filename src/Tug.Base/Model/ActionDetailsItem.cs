/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.ComponentModel.DataAnnotations;

namespace Tug.Model
{
    public class ActionDetailsItem : Util.ExtDataIndexerBase
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