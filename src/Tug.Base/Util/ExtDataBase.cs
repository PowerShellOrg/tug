// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tug.Util
{
    public class ExtDataBase : IExtData
    {
        [JsonExtensionData]
        protected IDictionary<string, JToken> _extData = new Dictionary<string, JToken>();

        IDictionary<string, JToken> IExtData.GetExtData()
        {
            return _extData;
        }
    }
}