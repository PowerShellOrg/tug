/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

namespace Tug.Util
{
    public class ExtDataIndexerBase : ExtDataBase
    {
        public object this[string key]
        {
            get { return ((IExtData)this).GetExtData(key); }
            set { ((IExtData)this).SetExtData(key, value); }
        }
    }
}