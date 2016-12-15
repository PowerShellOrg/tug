/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tug.Ext;
using Tug.Ext.Util;
using Tug.Server.Configuration;

namespace Tug
{
    public interface IChecksumAlgorithmProvider : IProvider<IChecksumAlgorithm>
    { }

    public class ChecksumAlgorithmManager
        : ProviderManagerBase<IChecksumAlgorithmProvider, IChecksumAlgorithm>
    {
        public ChecksumAlgorithmManager(
                ILogger<ChecksumAlgorithmManager> logger,
                ILogger<ServiceProviderExportDescriptorProvider> spLogger,
                IOptions<ChecksumSettings> settings,
                IServiceProvider sp)
            : base(logger, new ServiceProviderExportDescriptorProvider(spLogger, sp))
        {
            var extAssms = settings.Value?.Ext?.SearchAssemblies;
            var extPaths = settings.Value?.Ext?.SearchPaths;

            // Add assemblies to search context
            if ((settings.Value?.Ext?.ReplaceExtAssemblies).GetValueOrDefault())
            {
                logger.LogInformation("Resetting default Search Assemblies");
                ClearSearchAssemblies();
            }

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
            {
                logger.LogInformation("Resetting default search paths");
                ClearSearchPaths();
            }

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

        protected override IEnumerable<IChecksumAlgorithmProvider> FindProviders()
        {
            try
            {
                return base.FindProviders();
            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                Console.Error.WriteLine(">>>>>> Load Exceptions:");
                foreach (var lex in ex.LoaderExceptions)
                {
                    Console.Error.WriteLine(">>>>>> >>>>" + lex);
                }
                throw ex;
            }
        }
    }
}