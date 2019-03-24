// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tug.Server.Configuration;

namespace Tug.Server.Util
{
    public class DscHandlerHelper
    {
        private ILogger<DscHandlerHelper> _logger;
        private HandlerSettings _settings;
        private DscHandlerManager _dscManager;

        private IDscHandlerProvider _defaultDscProvider;
        private IDscHandler _defaultDscHandler;

        public DscHandlerHelper(ILogger<DscHandlerHelper> logger,
                IOptions<HandlerSettings> settings,
                DscHandlerManager dscManager)
        {
            _logger = logger;
            _settings = settings.Value;
            _dscManager = dscManager;

            Init();
        }

        public IDscHandler DefaultHandler
        {
            get { return _defaultDscHandler; }
        }

        private void Init()
        {
            // _logger.LogInformation("constructing DSC Handler Provider Manager");
            // var dscManager = new DscHandlerManager();
            _logger.LogInformation("resolved the following DSC Handler Providers:");
            foreach (var fpn in _dscManager.FoundProvidersNames)
                _logger.LogInformation($"  * [{fpn}]");

            _logger.LogInformation("resolving target Provider");
            _defaultDscProvider = _dscManager.GetProvider(_settings.Provider);
            if (_defaultDscProvider == null)
                throw new ArgumentException("invalid, missing or unresolved Provider name");

            _logger.LogInformation("applying optional DSC Handler parameters");
            if (_settings.Params?.Count > 0)
                _defaultDscProvider.SetParameters(_settings.Params);

            _logger.LogInformation("producing DSC Handler");
            _defaultDscHandler = _defaultDscProvider.Produce();
            if (_defaultDscHandler == null)
                throw new InvalidOperationException("failed to construct DSC Handler");

            // services.AddSingleton<IDscHandler>(dscHandler);



            // _logger.LogInformation($"Resolving DSC Handler Provider for [{settings?.Provider}]");
            // var handlerProviderType = Type.GetType(settings?.Provider);
            // if (handlerProviderType == null)
            //     throw new Exception("Unable to resolve DSC Handler Provider type (is the type specifiied fully?)");

            // services.AddSingleton<DscHandlerConfig>(new DscHandlerConfig
            // {
            //     InitParams = settings?.Params,   
            // });

            // services.AddSingleton(
            //         typeof(IDscHandlerProvider), handlerProviderType);
        }
    }
}