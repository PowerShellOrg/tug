using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using tug;
using tug.Messages;

namespace tug.Providers
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

    /// <summary>
    /// Implements a very basic, file-base Pull Server Handler that
    /// aligns closely with the default behavior of the xDscWebService.
    /// </summary>
    public class BasicDscHandler : IDscHandler
    {
        public const string DEFAULT_WORK_FOLDER = "DscService";

        // These parameters, names and default values, are based on
        // the corresponding appSettings defined for the xDscWebService
        // Pull Server out of xPSDesiredStateConfiguration

        public ILogger<BasicDscHandler> Logger
        { get; set; }

        public IChecksumAlgorithmProvider ChecksumProvider
        { get; set; }

        public string RegistrationKeyPath
        { get; set; } = $"{DEFAULT_WORK_FOLDER}";

        public string RegistrationSavePath
        { get; set; } = $"{DEFAULT_WORK_FOLDER}\\Registrations";
        
        public string ConfigurationPath
        { get; set; } = $"{DEFAULT_WORK_FOLDER}\\Configuration";
        
        public string ModulePath
        { get; set; } = $"{DEFAULT_WORK_FOLDER}\\Modules";


        public bool IsDisposed
        { get; private set; }

        public void Init()
        {
            Directory.CreateDirectory(RegistrationKeyPath);
            Directory.CreateDirectory(RegistrationSavePath);
            Directory.CreateDirectory(ConfigurationPath);
            Directory.CreateDirectory(ModulePath);
        }

        public void RegisterDscAgent(Guid agentId,
                RegisterDscAgentRequestBody detail)
        {
            var regPath = Path.Combine(RegistrationSavePath, $"{agentId}.json");
            if (File.Exists(regPath))
            {
                // TODO:  Do nothing?  Does the protocol allow unlimited re-registrations?
                //throw new Exception("agent ID already registered");
            }
            
            File.WriteAllText(regPath, JsonConvert.SerializeObject(detail));
        }

        public Tuple<DscActionStatus, GetDscActionResponseBody.DetailsItem[]> GetDscAction(Guid agentId,
            GetDscActionRequestBody detail)
        {
            DscActionStatus nodeStatus = DscActionStatus.OK;
            var list = new List<GetDscActionResponseBody.DetailsItem>();
            foreach (var cs in detail.ClientStatus)
            {
                if (string.IsNullOrEmpty(cs.ConfigurationName))
                {
                    var configPath = Path.Combine(ConfigurationPath, $"{agentId}.json");
                    if (!File.Exists(configPath))
                    {
                        nodeStatus = DscActionStatus.RETRY;
                        list.Add(new GetDscActionResponseBody.DetailsItem
                        {
                            Status = DscActionStatus.RETRY,
                        });
                    }
                    else
                    {
                        using (var csum = ChecksumProvider.GetChecksumAlgorithm())
                        {
                            if (csum.AlgorithmName != cs.ChecksumAlgorithm)
                            {
                                Logger.LogError("Checksum Algorithm mismatch!");
                            }
                            else
                            {
                                using (var fs = File.OpenRead(configPath))
                                {
                                    if (csum.ComputeChecksum(fs) == cs.Checksum)
                                        continue;
                                }
                            }
                        }

                        nodeStatus = DscActionStatus.GetConfiguration;
                        list.Add(new GetDscActionResponseBody.DetailsItem
                        {
                            ConfigurationName = cs.ConfigurationName,
                            Status = DscActionStatus.GetConfiguration,
                        });
                    }
                }
            }

            return Tuple.Create(nodeStatus, list.ToArray());
        }

        public Tuple<string, string, Stream> GetConfiguration(Guid agentId, string configName)
        {
            var configPath = Path.Combine(ConfigurationPath, $"{agentId}.json");
            if (!File.Exists(configPath))
                return null;

            // TODO:  Clean this up for performance with caching and stuff
            using (var cs = ChecksumProvider.GetChecksumAlgorithm())
            using (var fs = File.OpenRead(configPath))
            {
                return Tuple.Create(cs.AlgorithmName, cs.ComputeChecksum(fs),
                        (Stream)File.OpenRead(configPath));
            }
        }

        public Tuple<string, string, Stream> GetModule(string moduleName, string moduleVersion)
        {
            var modulePath = Path.Combine(ModulePath, $"{moduleName}/{moduleVersion}");
            if (!File.Exists(modulePath))
                return null;
            
            // TODO:  Clean this up for performance with caching and stuff
            using (var cs = ChecksumProvider.GetChecksumAlgorithm())
            using (var fs = File.OpenRead(modulePath))
            {
                return Tuple.Create(cs.AlgorithmName, cs.ComputeChecksum(fs),
                        (Stream)File.OpenRead(modulePath));
            }
        }

        public void SendReport(Guid agentId, Stream reportContent,
                SendReportRequest reserved)
        {
            throw new NotImplementedException();
        }

        public Stream GetReports(Guid agentId,
                GetReportsRequest reserved)
        {
            throw new NotImplementedException();
        }


        #region -- IDisposable Support --

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                IsDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BasicDscHandler() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion -- IDisposable Support --
    }
}