/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Tug.Server.Providers
{
    public class Ps5DscHandlerProvider : IDscHandlerProvider
    {
        private static readonly IEnumerable<string> PARAMS = new[]
        {
            nameof(Ps5DscHandler.BootstrapPath),
            nameof(Ps5DscHandler.BootstrapScript),
        };

        private ILogger<Ps5DscHandlerProvider> _pLogger;
        private ILogger<Ps5DscHandler> _hLogger;
        private DscHandlerConfig _config;
        private IChecksumAlgorithmProvider _checksumProvider;

        private Ps5DscHandler _handler;
               
        public Ps5DscHandlerProvider(
                ILogger<Ps5DscHandlerProvider> providerLogger,
                ILogger<Ps5DscHandler> handlerlogger,
                DscHandlerConfig config,
                IChecksumAlgorithmProvider checksumProvider)
        {
            _pLogger = providerLogger;
            _hLogger = handlerlogger;
            _config = config;
            _checksumProvider = checksumProvider;
        }

        public IEnumerable<string> GetParameters()
        {
            return PARAMS;
        }

        public IDscHandler GetHandler(IDictionary<string, object> initParams)
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

                        _pLogger.LogInformation("Handler Constructed");

                        if (initParams == null)
                            initParams = _config?.InitParams;

                        if (initParams != null)
                        {
                            foreach (var p in PARAMS)
                            {
                                if (initParams.ContainsKey(p))
                                {
                                    _pLogger.LogDebug("Setting parameter {initParamName}", p);
                                    typeof(Ps5DscHandler).GetTypeInfo()
                                            .GetProperty(p, BindingFlags.Public | BindingFlags.Instance)
                                            .SetValue(h, initParams[p]);
                                }
                            }
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
