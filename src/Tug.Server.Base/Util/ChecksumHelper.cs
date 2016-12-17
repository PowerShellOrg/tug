/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tug.Server.Configuration;

namespace Tug.Server.Util
{
    /// <summary>
    /// Helper class to manage usage of Checksum Algorithm Providers and
    /// implementation classes as indicated by user configuration settings.
    /// </summary>
    public class ChecksumHelper
    {
        private ILogger<ChecksumHelper> _logger;
        private ChecksumSettings _settings;
        private ChecksumAlgorithmManager _csumManager;
        private string _defaultName;
        private IChecksumAlgorithmProvider _defaultProvider;

        public ChecksumHelper(ILogger<ChecksumHelper> logger,
                IOptions<ChecksumSettings> settings,
                ChecksumAlgorithmManager csumManager)
        {
            _logger = logger;
            _settings = settings.Value;
            _csumManager = csumManager;

            Init();
        }

        public string DefaultAlgorithmName
        {
            get { return _defaultName; }
        }

        public IChecksumAlgorithm GetAlgorithm(string name = null)
        {
            if (name == null)
                name = _defaultName;
            return _csumManager.GetProvider(name).Produce();
        }

        private void Init()
        {
            // _logger.LogInformation("constructing Checksum Algorithm Provider Manager");
            // var csumManager = new ChecksumAlgorithmManager();
            // services.AddSingleton(csumManager);

            _logger.LogInformation("resolved the following Checksum Providers:");
            foreach (var fpn in _csumManager.FoundProvidersNames)
                _logger.LogInformation($"  * [{fpn}]");

            _logger.LogInformation("resolving default Checksum Algorithm:");
            if (!string.IsNullOrEmpty(_settings?.Default))
            {
                _defaultName = _settings.Default;
                _logger.LogInformation("    resolved as [{defaultProviderName}]", _defaultName);
                _defaultProvider = _csumManager.GetProvider(_settings.Default);
                if (_defaultProvider == null)
                    throw new ArgumentException("invalid, missing or unresolved Provider name");
                // services.AddSingleton<IChecksumAlgorithmProvider>(csumProvider);
            }
            else
            {
                _logger.LogWarning("    no explicit Default Checksum algorithm specified");
                var first = _csumManager.FoundProvidersNames.FirstOrDefault();
                if (string.IsNullOrEmpty(first))
                    throw new InvalidOperationException("unable to resolve first provider");
                _logger.LogInformation("    defaulting to first {firstCsum}", first);
                _defaultName = first;
            }
        }
    }
}