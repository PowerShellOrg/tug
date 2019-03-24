// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

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