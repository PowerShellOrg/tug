using System.Collections.Generic;
using Tug.Model;

namespace Tug.Server
{
    public class ActionStatus
    {
        public DscActionStatus NodeStatus
        { get; set; }

        public IEnumerable<ActionDetailsItem> ConfigurationStatuses
        { get; set; }
    }
}