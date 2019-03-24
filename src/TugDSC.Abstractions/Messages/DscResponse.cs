// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using TugDSC.Messages.ModelBinding;

namespace TugDSC.Messages
{
    public class DscResponse
    {
        public const string PROTOCOL_VERSION_HEADER = "ProtocolVersion";
        public const string PROTOCOL_VERSION_VALUE = "2.0";


        [ToHeader(Name = PROTOCOL_VERSION_HEADER)]
        public string ProtocolVersionHeader
        { get; set; } = PROTOCOL_VERSION_VALUE;
    }
}