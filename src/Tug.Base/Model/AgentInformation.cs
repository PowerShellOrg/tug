/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tug.Util;

namespace Tug.Model
{
    public class AgentInformation : IExtData
    {
        [JsonExtensionData]
        protected IDictionary<string, JToken> _extData = new Dictionary<string, JToken>();

        public object this[string key]
        {
            get { return ((IExtData)this).GetExtData(key); }
            set { ((IExtData)this).SetExtData(key, value); }
        }

        // NOTE:  DO NOT CHANGE THE ORDER OF THESE PROPERTIES!!!
        // Apparently the order of these properties is important
        // to successfully fulfill the RegKey authz requirements

        public string LCMVersion
        { get; set; }

        public string NodeName
        { get; set; }

        public string IPAddress
        { get; set; }

        IDictionary<string, JToken> IExtData.GetExtData()
        {
            return _extData;
        }
    }
}