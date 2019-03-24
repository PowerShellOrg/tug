// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TugDSC.Util
{
    /// Various extension methods that make working with <see cref="IExtData">extension data</see>
    /// implementations easier and more fluid.
    public static class ExtDataExtensions
    {
        public static int GetExtDataCount(this IExtData extData)
        {
            return extData.GetExtData().Count;
        }

        public static IEnumerable<string> GetExtDataKeys(this IExtData extData)
        {
            return extData.GetExtData().Keys;
        }

        public static bool ContainsExtData(this IExtData extData, string key)
        {
            return extData.GetExtData().ContainsKey(key);
        }

        public static object GetExtData(this IExtData extData, string key, object ifNotFound = null)
        {
            return extData.GetExtData().ContainsKey(key)
                    ? extData.GetExtData()[key]
                    : ifNotFound;
        }

        public static void SetExtData(this IExtData extData, string key, object value)
        {
            extData.GetExtData()[key] = JToken.FromObject(value);
        }

        public static void RemoveExtData(this IExtData extData, string key)
        {
            extData.GetExtData().Remove(key);
        }
    }
}