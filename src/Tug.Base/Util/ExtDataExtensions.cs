/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Tug.Util
{
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