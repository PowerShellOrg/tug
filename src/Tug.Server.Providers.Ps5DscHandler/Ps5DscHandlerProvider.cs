/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tug.Ext;
using Tug.Ext.Util;
using Tug.Server.Util;

namespace Tug.Server.Providers
{
    public class Ps5DscHandlerProvider : IDscHandlerProvider
    {
        private static readonly ProviderInfo INFO = new ProviderInfo("ps5");

        private static readonly IEnumerable<ProviderParameterInfo> PARAMS = new[]
        {
            new ProviderParameterInfo(nameof(Ps5DscHandler.BootstrapPath)),
            new ProviderParameterInfo(nameof(Ps5DscHandler.BootstrapScript)),
            new ProviderParameterInfo(nameof(Ps5DscHandler.BootstrapWatch)),
        };

        private ILogger<Ps5DscHandlerProvider> _pLogger;
        private ILogger<Ps5DscHandler> _hLogger;
        private IConfiguration _appConfig;
        private IDictionary<string, object> _productParams;
        private ChecksumHelper _checksumHelper;
        private IChecksumAlgorithmProvider _checksumProvider;

        private Ps5DscHandler _handler;
               
        public Ps5DscHandlerProvider(
                ILogger<Ps5DscHandlerProvider> providerLogger,
                ILogger<Ps5DscHandler> handlerlogger,
                IConfiguration appSettings,
                ChecksumHelper checksumHelper)
        {
            _pLogger = providerLogger;
            _hLogger = handlerlogger;
            _appConfig = appSettings;
            _checksumHelper = checksumHelper;
        }

        public ProviderInfo Describe() => INFO;

        public IEnumerable<ProviderParameterInfo> DescribeParameters() => PARAMS;

        public void SetParameters(IDictionary<string, object> productParams)
        {
            _productParams = productParams;
        }

        public IDscHandler Produce()
        {
            _pLogger.LogDebug("Resolving handler");
            if (_handler == null)
            {
                lock (this)
                {
                    if (_handler == null)
                    {
                        var h = new Ps5DscHandler();
                        h.Logger = _hLogger;
                        h.AppConfig = _appConfig;

                        _pLogger.LogInformation("Handler Constructed");

                        if (_productParams != null)
                        {
                            _pLogger.LogInformation("Applying parameters:");
                            h.ApplyParameters(PARAMS, _productParams,
                                    // Include a filter just to log the params applied
                                    filter: (pInfo, pValue) =>
                                    {
                                        _pLogger.LogInformation("  * Setting parameter:  [{initParamName}]", pInfo.Name);
                                        return Tuple.Create(true, pValue);
                                    });
                        }

                        h.Init();
                        _handler = h;
                    }
                }
            }
            return _handler;
        }
    }
}
