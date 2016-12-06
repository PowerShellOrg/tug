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

        private ILogger<Ps5DscHandlerProvider> _factoryLogger;
        private ILogger<Ps5DscHandler> _handlerLogger;
        private IChecksumAlgorithmProvider _checksumProvider;
               
        public Ps5DscHandlerProvider(
                ILogger<Ps5DscHandlerProvider> factoryLogger,
                ILogger<Ps5DscHandler> handlerlogger,
                IChecksumAlgorithmProvider checksumProvider)
        {
            _factoryLogger = factoryLogger;
            _handlerLogger = handlerlogger;
            _checksumProvider = checksumProvider;
        }

        public IEnumerable<string> GetParameters()
        {
            return PARAMS;
        }

        public IDscHandler GetHandler(IDictionary<string, object> initParams)
        {
            var h = new Ps5DscHandler();
            h.LOG = _handlerLogger;

            if (initParams != null)
            {
                foreach (var p in PARAMS)
                {
                    if (initParams.ContainsKey(p))
                    {
                        typeof(Ps5DscHandler).GetTypeInfo()
                                .GetProperty(p, BindingFlags.Public | BindingFlags.Instance)
                                .SetValue(h, initParams[p]);
                    }
                }
            }

            h.Init();

            return h;
        }
    }
}
