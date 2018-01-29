/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

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