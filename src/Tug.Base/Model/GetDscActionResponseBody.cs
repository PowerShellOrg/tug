/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.ComponentModel.DataAnnotations;

namespace Tug.Model
{
    public class GetDscActionResponseBody : Util.ExtDataIndexerBase
    {
        [Required]
        [EnumDataTypeAttribute(typeof(DscActionStatus))]
        public DscActionStatus NodeStatus
        { get; set; }

        public ActionDetailsItem[] Details
        { get; set; }
    }
}