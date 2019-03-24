// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

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