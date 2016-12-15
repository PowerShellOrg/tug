using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Tug.TestExt
{
    /// <summary>
    /// A test provider manager implementation that is based on MEF
    /// support for dynamica discovery and loading of provider impls.
    /// </summary>
    public class DynamicThingyProviderManager : Ext.Util.ProviderManagerBase<IThingyProvider, IThingy>
    {
        private static readonly ILogger LOG = new LoggerFactory()
                .CreateLogger<DynamicThingyProviderManager>();

        public DynamicThingyProviderManager(
                IEnumerable<Assembly> searchAssemblies = null,
                IEnumerable<string> searchPaths = null,
                bool resetBuiltIns = false,
                bool resetSearchAssemblies = false,
                bool resetSearchPaths = false)
            : base(LOG)
        {
            // Check if any of the default search
            // collections should be cleared
            if (resetBuiltIns)
                base.ClearBuiltIns();
            if (resetSearchAssemblies)
                base.ClearSearchAssemblies();
            if (resetSearchPaths)
                base.ClearSearchPaths();

            // Add to the searchable assembly collection
            if (searchAssemblies != null)
                base.AddSearchAssemblies(searchAssemblies);

            // Add to the searchable path collection
            if (searchPaths != null)
                base.AddSearchPath(searchPaths);
        }
    }
}