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

        private ILogger<BasicDscHandler> _logger;
        private IChecksumAlgorithmProvider _checksumProvider;
        private BasicDscHandler _handler;

        public BasicDscHandlerProvider(ILogger<BasicDscHandler> logger,
                IChecksumAlgorithmProvider checksumProvider)
        {
            _logger = logger;
            _checksumProvider = checksumProvider;
        }

        public IEnumerable<string> GetParameters()
        {
            return PARAMS;
        }

        public IDscHandler GetHandler(IDictionary<string, object> initParams)
        {
            if (_handler == null)
            {
                lock (this)
                {
                    if (_handler == null)
                    {
                        _handler = new BasicDscHandler
                        {
                            Logger = _logger,
                            ChecksumProvider = _checksumProvider,
                        };

                        if (initParams != null)
                        {
                            foreach (var p in PARAMS)
                            {
                                if (initParams.ContainsKey(p))
                                {
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