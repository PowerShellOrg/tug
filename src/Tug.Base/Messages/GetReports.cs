/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace Tug.Messages
{
    public class GetReportsRequest : DscAgentRequest
    {
        [Required]
        public Guid JobId
        { get; set; }
    }
}