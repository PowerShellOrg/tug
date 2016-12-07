using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Tug.Server.Providers
{
    public class BasicDscHandlerProvider : IDscHandlerProvider
    {
        private static readonly IEnumerable<string> PARAMS = new[]
        {
            nameof(BasicDscHandler.RegistrationKeyPath),
            nameof(BasicDscHandler.RegistrationSavePath),
            nameof(BasicDscHandler.ConfigurationPath),
            nameof(BasicDscHandler.ModulePath),
        };

        private ILogger<BasicDscHandlerProvider> _pLogger;
        private ILogger<BasicDscHandler> _hLogger;
        private IChecksumAlgorithmProvider _checksumProvider;
        private DscHandlerConfig _config;
        private BasicDscHandler _handler;

        public BasicDscHandlerProvider(
                ILogger<BasicDscHandlerProvider> pLogger,
                ILogger<BasicDscHandler> hLogger,
                DscHandlerConfig config,
                IChecksumAlgorithmProvider checksumProvider)
        {
            _pLogger = pLogger;
            _hLogger = hLogger;
            _config = config;
            _checksumProvider = checksumProvider;

            _pLogger.LogInformation("Provider Created");
        }

        public IEnumerable<string> GetParameters()
        {
            return PARAMS;
        }

        public IDscHandler GetHandler(IDictionary<string, object> initParams)
        {
            _pLogger.LogDebug("Resolving Handler");
            if (_handler == null)
            {
                lock (this)
                {
                    if (_handler == null)
                    {
                        _pLogger.LogInformation("Building global Handler instance");

                        _handler = new BasicDscHandler
                        {
                            Logger = _hLogger,
                            ChecksumProvider = _checksumProvider,
                        };

                        if (initParams == null)
                            initParams = _config?.InitParams;

                        if (initParams != null)
                        {
                            foreach (var p in PARAMS)
                            {
                                if (initParams.ContainsKey(p))
                                {
                                    _pLogger.LogInformation("  * Setting init param: " + p);
                                    typeof(BasicDscHandler).GetTypeInfo()
                                            .GetProperty(p, BindingFlags.Public | BindingFlags.Instance)
                                            .SetValue(_handler, initParams[p]);
                                }
                            }
                        }

                        _handler.Init();
                    }
                }
            }

            return _handler;
        }
    }
}