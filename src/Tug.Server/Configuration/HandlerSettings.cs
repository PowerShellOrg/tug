using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tug.Server.Configuration
{
    public class HandlerSettings
    {
        [Required]
        public string Provider
        { get; set; } = typeof(Providers.BasicDscHandlerProvider).FullName;

        // This has to be concrete class, not interface to
        // be able to construct during deserialization
        public Dictionary<string, object> Params
        { get; set; }
    }
}