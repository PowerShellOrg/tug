/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

namespace Tug.Server.Configuration
{
    public class AppSettings
    {
        public ChecksumSettings Checksum
        { get; set; }

        public AuthzSettings Authz
        { get; set; }
        
        public HandlerSettings Handler
        { get; set; }
    }
}