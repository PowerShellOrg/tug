using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Tug.Server.Providers
{
    public class Ps5DscHandlerFactory // : IDscHandlerProvider
    {
        private static readonly IEnumerable<string> PARAMS = new[]
        {
            nameof(Ps5DscHandler.BootstrapPath),
            nameof(Ps5DscHandler.BootstrapScript),
        };

        private ILogger<Ps5DscHandlerFactory> _factoryLogger;
        private ILogger<Ps5DscHandler> _handlerLogger;
               
        public Ps5DscHandlerFactory(
                ILogger<Ps5DscHandlerFactory> factoryLogger,
                ILogger<Ps5DscHandler> handlerlogger)
                //IChecksumAlgorithmProvider checksumProvider)
        {
            _factoryLogger = factoryLogger;
            _handlerLogger = handlerlogger;
        }

        public IEnumerable<string> GetParameters()
        {
            return PARAMS;
        }

        public IDisposable GetHandler(IDictionary<string, object> initParams)
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
