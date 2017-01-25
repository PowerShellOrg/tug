/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Tug.Ext;
using Tug.Server.Util;

namespace Tug.Server.Providers
{
    public class BasicDscHandlerProvider : IDscHandlerProvider
    {
        protected static ProviderInfo INFO = new ProviderInfo("basic");

        protected static readonly IEnumerable<ProviderParameterInfo> PARAMS = new[]
        {
            new ProviderParameterInfo(nameof(BasicDscHandler.RegistrationSavePath)),
            new ProviderParameterInfo(nameof(BasicDscHandler.ConfigurationPath)),
            new ProviderParameterInfo(nameof(BasicDscHandler.ModulePath)),
        };

        protected ILogger<BasicDscHandlerProvider> _pLogger;
        protected ILogger<BasicDscHandler> _hLogger;
        protected ChecksumHelper _checksumHelper;

        protected IDictionary<string, object> _productParams;

        protected BasicDscHandler _handler;

        public BasicDscHandlerProvider(
                ILogger<BasicDscHandlerProvider> pLogger,
                ILogger<BasicDscHandler> hLogger,
                ChecksumAlgorithmManager checksumManager,
                ChecksumHelper checksumHelper)
        {
            _pLogger = pLogger;
            _hLogger = hLogger;
            _checksumHelper = checksumHelper;

            _pLogger.LogInformation("Provider Created");
        }

        public virtual ProviderInfo Describe() => INFO;

        public virtual IEnumerable<ProviderParameterInfo> DescribeParameters() => PARAMS;

        public virtual void SetParameters(IDictionary<string, object> productParams)
        {
            _productParams = productParams;
        }

        public virtual IDscHandler Produce()
        {
            _pLogger.LogDebug("Resolving Handler");
            if (_handler == null)
            {
                lock (this)
                {
                    if (_handler == null)
                    {
                        _pLogger.LogInformation("Building global Handler instance");

                        _handler = ConstructHandler();
                        _handler.Logger = _hLogger;
                        _handler.ChecksumHelper = _checksumHelper;

                        if (_productParams != null)
                        {
                            foreach (var p in PARAMS)
                            {
                                if (_productParams.ContainsKey(p.Name))
                                {
                                    _pLogger.LogInformation($"  * Setting init param:  [{p.Name}]");
                                    typeof(BasicDscHandler).GetTypeInfo()
                                            .GetProperty(p.Name, BindingFlags.Public
                                                    | BindingFlags.Instance)
                                            .SetValue(_handler, _productParams[p.Name]);
                                }
                            }
                        }

                        _handler.Init();
                    }
                }
            }

            return _handler;
        }

        protected virtual BasicDscHandler ConstructHandler()
        {
            return new BasicDscHandler();
        }
    }
}