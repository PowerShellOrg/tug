/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Tug.Util
{
    public interface IExtData
    {
        IDictionary<string, JToken> GetExtData();
    }
} 