// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations;

namespace TugDSC.Model
{
    public class GetDscActionRequestBody : Util.ExtDataIndexerBase
    {
        [Required]
        [MinLengthAttribute(1)]
        public ClientStatusItem[] ClientStatus
        { get; set; }
    }
}