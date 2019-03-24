// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TugDSC.Ext;
using TugDSC.Ext.Util;
using TugDSC.Server.Configuration;

namespace TugDSC
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
                        if (logger.IsEnabled(LogLevel.Debug))
                            logger.LogDebug($"  * Adding [{x}]");

                        var an = GetAssemblyName(x);
                        if (an == null)
                            throw new ArgumentException("invalid assembly name");

                        if (logger.IsEnabled(LogLevel.Debug))
                            logger.LogDebug($"    o Resolved as AsmName [{an}]{Directory.GetCurrentDirectory()}:{an}");
                        return an;
                    }).Select(x =>
                    {
                        var asm = GetAssembly(x);
                        if (asm == null)
                            throw new InvalidOperationException("unable to resolve assembly from name");
                        
                        if (logger.IsEnabled(LogLevel.Debug))
                            logger.LogDebug($"    o [{x.FullName}]");

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