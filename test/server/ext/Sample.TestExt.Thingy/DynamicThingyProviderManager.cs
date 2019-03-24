// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using TugDSC.Ext.Util;

namespace Sample.TestExt.Thingy
{
    /// <summary>
    /// A test provider manager implementation that is based on MEF
    /// support for dynamic discovery and loading of provider implementations.
    /// </summary>
    public class DynamicThingyProviderManager : ProviderManagerBase<IThingyProvider, IThingy>
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