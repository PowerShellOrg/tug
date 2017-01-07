using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Tug.Util
{
    public interface IExtData
    {
        IDictionary<string, JToken> GetExtData();
    }
} 