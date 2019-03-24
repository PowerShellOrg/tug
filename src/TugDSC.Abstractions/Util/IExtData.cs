// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

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