/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

namespace TugDSC.Util
{
    /// Extends the base extension data implementation with support for an
    /// indexer to access extension data properties.
    public abstract class ExtDataIndexerBase : ExtDataBase
    {
        public object this[string key]
        {
            get { return ((IExtData)this).GetExtData(key); }
            set { ((IExtData)this).SetExtData(key, value); }
        }
    }
}