/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.Collections.Generic;

namespace Tug.Server
{
    public interface IDscHandlerProvider
    {
        IEnumerable<string> GetParameters();

        IDscHandler GetHandler(IDictionary<string, object> initParams);
    }
}