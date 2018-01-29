/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TugDSC.Util
{
    /// Model entities that implement this interface support "extension data" in the context of
    /// serialization to/from JSON.  Extension data allows us to add additional pieces of data
    /// to an entity that was not included in the initial entity definition.   However, we use
    /// mostly to catch possible mismatch between client/server representations of data classes.
    public interface IExtData
    {
        IDictionary<string, JToken> GetExtData();
    }
} 