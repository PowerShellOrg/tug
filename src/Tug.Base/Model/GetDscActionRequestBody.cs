/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.ComponentModel.DataAnnotations;

namespace Tug.Model
{
    public class GetDscActionRequestBody : Util.ExtDataIndexerBase
    {
        [Required]
        [MinLengthAttribute(1)]
        public ClientStatusItem[] ClientStatus
        { get; set; }
    }
}