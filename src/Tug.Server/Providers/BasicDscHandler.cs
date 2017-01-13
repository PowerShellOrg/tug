/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tug.Model;
using Tug.Server.Util;

namespace Tug.Server.Providers
{
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

        public ChecksumHelper ChecksumHelper
        { get; set; }

        public string RegistrationSavePath
        { get; set; } = $"{DEFAULT_WORK_FOLDER}\\Registrations";
        
        public string ConfigurationPath
        { get; set; } = $"{DEFAULT_WORK_FOLDER}\\Configuration";
        
        public string ModulePath
        { get; set; } = $"{DEFAULT_WORK_FOLDER}\\Modules";

        public bool IsDisposed
        { get; private set; }

        public virtual void Init()
        {
            Assert(Logger != null, "missing logger");
            Assert(ChecksumHelper != null, "missing checksum helper");
            Assert(!string.IsNullOrWhiteSpace(RegistrationSavePath),
                    "registration save path not set");
            Assert(!string.IsNullOrWhiteSpace(ConfigurationPath),
                    "configuration path not set");
            Assert(!string.IsNullOrWhiteSpace(ModulePath),
                    "module path not set");

            Logger.LogInformation("All Assertions Passed!");
            Directory.CreateDirectory(RegistrationSavePath);
            Directory.CreateDirectory(ConfigurationPath);
            Directory.CreateDirectory(ModulePath);
            Logger.LogInformation("All Directories Created/Confirmed");
        }

        protected virtual void Assert(bool value, string failMessage = null)
        {
            if (!value)
                if (string.IsNullOrEmpty(failMessage))
                    throw new Exception("failed assertion");
                else
                    throw new Exception(); // ($"failed assertion: {message}");
        }

        public virtual void RegisterDscAgent(Guid agentId,
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

        public virtual ActionStatus GetDscAction(Guid agentId,
            GetDscActionRequestBody detail)
        {
            var regPath = Path.Combine(RegistrationSavePath, $"{agentId}.json");
            if (!File.Exists(regPath))
                throw new InvalidOperationException("unknown agent ID");

            var regDetail = JsonConvert.DeserializeObject<RegisterDscAgentRequestBody>(
                    File.ReadAllText(regPath));
            var configCount = (regDetail.ConfigurationNames?.Length).GetValueOrDefault();
            Logger.LogDebug($"regDetail[{JsonConvert.SerializeObject(regDetail)}]");

            DscActionStatus nodeStatus = DscActionStatus.OK;
            var list = new List<ActionDetailsItem>();

            if (configCount == 0)
            {
                // Nothing to do since we don't know what config name to provide;
                // the xDscWebService-compatible behavior is to just return an OK
                nodeStatus = DscActionStatus.OK;
                Logger.LogWarning($"No configuration names specified during registration for AgentId=[{agentId}]");
            }
            else if (configCount == 1)
            {
                var cn = regDetail.ConfigurationNames[0];

                // This is the scenario of a single (default)
                // named configuration tied to the node
                if (detail.ClientStatus?.Length == 1
                    && (string.IsNullOrEmpty(detail.ClientStatus[0].ConfigurationName)
                        || detail.ClientStatus[0].ConfigurationName == cn)) 
                {
                    var cs = detail.ClientStatus[0];

                    // Checksum is for the single default configuration of this node
                    var configPath = Path.Combine(ConfigurationPath, $"SHARED/{cn}.mof");
                    if (!File.Exists(configPath))
                    {
                        Logger.LogWarning($"unable to find ConfigurationName=[{cn}] for AgentId=[{agentId}]");
                        return null;
                    }


                    // Assume we have to pull
                    nodeStatus = DscActionStatus.GetConfiguration;
                    var dtlItem = new ActionDetailsItem
                    {
                        ConfigurationName = cn,
                        Status = nodeStatus,
                    };
                    list.Add(dtlItem);

                    if (!string.IsNullOrEmpty(cs.Checksum)) // Empty Checksum on the first pull
                    {
                        using (var csum = ChecksumHelper.GetAlgorithm())
                        {
                            if (csum.AlgorithmName == cs.ChecksumAlgorithm
                                && !string.IsNullOrEmpty(cs.Checksum)) // Make sure we're on the same algor
                            {
                                using (var fs = File.OpenRead(configPath))
                                {
                                    var csumCsum = csum.ComputeChecksum(fs);
                                    if (csumCsum == cs.Checksum)
                                    {
                                        // We've successfully passed all the checks, nothing to do
                                        nodeStatus = DscActionStatus.OK;
                                        dtlItem.Status = nodeStatus;
                                    }
                                    else
                                    {
                                        Logger.LogDebug($"Checksum mismatch "
                                                + "[{csumCsum}]!=[{cs.Checksum}]"
                                                + "for AgentId=[{agentId}]");
                                    }
                                }
                            }
                            else
                            {
                                Logger.LogWarning($"Checksum Algorithm mismatch "
                                        + "[{csum.AlgorithmName}]!=[{cs.ChecksumAlgorithm}] "
                                        + "for AgentId=[{agentId}]");
                            }
                        }
                    }
                    else
                    {
                        Logger.LogDebug($"First time pull check for AgentId=[{agentId}]");
                    }
                }
                else
                {
                    throw new NotImplementedException("only single/default configuration names are implemented");
                }
            }
            else
            {
                Logger.LogWarning($"Found [{regDetail.ConfigurationNames.Length}] config names:  {regDetail.ConfigurationNames}");
                throw new NotImplementedException("multiple configuration names are not implemented");

                // foreach (var cn in regDetail.ConfigurationNames)
                // {
                //     var configPath = Path.Combine(ConfigurationPath, $"SHARED/{cn}.mof");
                //     if (!File.Exists(configPath))
                //         throw new InvalidOperationException($"missing configuration by name [{cn}]");
                    
                //     using (var csum = ChecksumHelper.GetAlgorithm())
                //     {
                //         if (csum.AlgorithmName != cs.ChecksumAlgorithm)
                //         {
                //             Logger.LogError("Checksum Algorithm mismatch!");
                //         }
                //         else
                //         {
                //             using (var fs = File.OpenRead(configPath))
                //             {
                //                 if (csum.ComputeChecksum(fs) == cs.Checksum)
                //                     continue;
                //             }
                //         }
                //     }
                // }
            }
            /*
            foreach (var cs in detail.ClientStatus)
            {
                if (string.IsNullOrEmpty(cs.ConfigurationName))
                {
                    var configPath = Path.Combine(ConfigurationPath, $"{agentId}/{agentId}.mof");
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
                        using (var csum = ChecksumHelper.GetAlgorithm())
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
                            Status = DscActionStatus.GetConfiguration,
                        });
                    }
                }
            }
            */
            return new ActionStatus
            {
                NodeStatus = nodeStatus,
                ConfigurationStatuses = list.ToArray(),
            };
        }

        public virtual FileContent GetConfiguration(Guid agentId, string configName)
        {
            var configPath = Path.Combine(ConfigurationPath, $"SHARED/{configName}.mof");
            if (!File.Exists(configPath))
            {
                Logger.LogWarning($"unable to find ConfigurationName=[{configName}] for AgentId=[{agentId}]");
                return null;
            }

            // TODO:  Clean this up for performance with caching and stuff
            using (var cs = ChecksumHelper.GetAlgorithm())
            using (var fs = File.OpenRead(configPath))
            {
                return new FileContent
                {
                    ChecksumAlgorithm = cs.AlgorithmName,
                    Checksum = cs.ComputeChecksum(fs),
                    Content = (Stream)File.OpenRead(configPath),
                };
            }
        }

        public virtual FileContent GetModule(Guid? agentId, string moduleName, string moduleVersion)
        {
            var modulePath = Path.Combine(ModulePath, $"{moduleName}/{moduleVersion}.zip");
            if (!File.Exists(modulePath))
                return null;

            // TODO:  Clean this up for performance with caching and stuff
            using (var cs = ChecksumHelper.GetAlgorithm())
            using (var fs = File.OpenRead(modulePath))
            {
                return new FileContent
                {
                    ChecksumAlgorithm = cs.AlgorithmName,
                    Checksum = cs.ComputeChecksum(fs),
                    Content = (Stream)File.OpenRead(modulePath),
                };
            }
        }

        public virtual void SendReport(Guid agentId, SendReportRequestBody reserved)
        {
            throw new NotImplementedException();
        }

        public virtual Stream GetReports(Guid agentId)
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