/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licensed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TugDSC.Ext;
using TugDSC.Ext.Util;
using TugDSC.Server.Configuration;

namespace TugDSC.Server
{
    public interface IDscHandlerProvider : IProvider<IDscHandler>
    { }

    public class DscHandlerManager
        : ProviderManagerBase<IDscHandlerProvider, IDscHandler>
    {
        public DscHandlerManager(
                ILogger<DscHandlerManager> logger,
                ILogger<ServiceProviderExportDescriptorProvider> spLogger,
                IOptions<HandlerSettings> settings,
                IServiceProvider sp)
            : base(logger, new ServiceProviderExportDescriptorProvider(spLogger, sp))
        {
            var extAssms = settings.Value?.Ext?.SearchAssemblies;
            var extPaths = settings.Value?.Ext?.SearchPaths;

            // Add assemblies to search context
            if ((settings.Value?.Ext?.ReplaceExtAssemblies).GetValueOrDefault())
                ClearSearchAssemblies();
            if (extAssms?.Length > 0)
            {
                logger.LogInformation("Adding Search Assemblies");
                AddSearchAssemblies(
                    extAssms.Select(x =>
                    {
                        var an = GetAssemblyName(x);
                        if (an == null)
                            throw new ArgumentException("invalid assembly name");
                        return an;
                    }).Select(x =>
                    {
                        var asm = GetAssembly(x);
                        if (asm == null)
                            throw new InvalidOperationException("unable to resolve assembly from name");
                        
                        if (logger.IsEnabled(LogLevel.Debug))
                            logger.LogDebug($"  * [{x.FullName}]");

                        return asm;
                    }));
            }

            // Add dir paths to search context
            if ((settings.Value?.Ext?.ReplaceExtPaths).GetValueOrDefault())
                ClearSearchPaths();
            if (extPaths?.Length > 0)
            {
                logger.LogInformation("Adding Search Paths");
                AddSearchPath(extPaths.Select(x =>
                {
                    var y = Path.GetFullPath(x);
                    if (logger.IsEnabled(LogLevel.Debug))
                        logger.LogDebug($"  * [{y}]");
                    return y;
                }));
            }

            base.Init();
        }

        protected override void Init()
        {
            // Skipping the initialization till
            // after constructor parameters are applied
        }
    }
}