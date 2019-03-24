// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Composition.Hosting.Core;
using Microsoft.Extensions.Logging;

namespace Tug.Ext.Util
{
    /// <summary>
    /// Custom MEF provider to resolve dependencies against the native DI framework.
    /// </summary>
    /// <remarks>
    /// This class acts as an adapter that bridges the dependency resolution mechanism
    /// of MEF to be able to resolve against the services provided by the .NET Core
    /// native dependency injection (DI) facility.
    /// </remarks>
    public class ServiceProviderExportDescriptorProvider : ExportDescriptorProvider
    {
        public const string ORIGIN_NAME = "DI-ServiceProvider";

        ILogger _logger;
        private IServiceProvider _serviceProvider;

        public ServiceProviderExportDescriptorProvider(ILogger logger, IServiceProvider sp)
        {
            _logger = logger;
            _serviceProvider = sp;
        }

        public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors(
                CompositionContract contract, DependencyAccessor descriptorAccessor)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Getting Export Descriptors:"
                    + $" contractName=[{contract.ContractName}]"
                    + $" contractType=[{contract.ContractType.FullName}]");

            var svc = _serviceProvider.GetService(contract.ContractType);
            if (svc == null)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"No DI service found for"
                            + $" contractType=[{contract.ContractType.FullName}]");
                yield break;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Resolved DI service for"
                        + $" contractType=[{contract.ContractType.FullName}]"
                        + $" service=[{svc}]");

            CompositeActivator ca = (ctx, op) => svc;
            yield return new ExportDescriptorPromise(contract, ORIGIN_NAME, true,
                    NoDependencies, deps => ExportDescriptor.Create(ca, NoMetadata));
        }
    }
}