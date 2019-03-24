// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

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