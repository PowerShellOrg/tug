/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licensed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TugDSC.Client.CLIApp.Configuration;
using TugDSC.Configuration.Binder;

namespace TugDSC.Client.CLIApp
{
    public class Program
    {
        public const string DEFAULT_AGENT_VERSION = "2.0";

        /// <summary>
        /// File name of an optional JSON file used to configure the Web Host.
        /// </summary>
        public const string APP_CONFIG_FILENAME = "appsettings.json";

        /// <summary>
        /// Prefix used to identify environment variables that can override Web Host
        /// configuration.
        /// </summary>
        public const string APP_CONFIG_ENV_PREFIX = "TUG_CLIENT_";

        private CommandLine _commandLine;
        private DscPullConfig _config;
        private DscPullClient _client;

        public void Execute(string[] args)
        {
            var run = new List<Action>();

            _commandLine = new CommandLine();
            _commandLine.OnRegisterAgent = () => run.Add(DoRegisterAgent);
            _commandLine.OnGetAction = () => run.Add(DoGetAction);
            _commandLine.OnGetConfiguration = () => run.Add(DoGetConfiguration);
            _commandLine.OnGetActionAndConfiguration = () => run.Add(DoGetActionAndConfiguration);
            _commandLine.OnGetModule = () => run.Add(DoGetModule);
            
            _commandLine.Init().Execute(args);

            _config = ResolveClientConfiguration();

            try
            {
                using (_client = new DscPullClient(_config))
                {
                    foreach (var r in run)
                        r();
                }
            }
            catch (AggregateException ex)
            {
                Console.Error.WriteLine("UNCAUGHT EXCEPTION:");
                Console.Error.WriteLine(ex.InnerException);
                Console.Error.WriteLine("INNER EXCEPTIONS ====================>");
                foreach (var iex in ex.InnerExceptions)
                    Console.Error.WriteLine(iex);
            }
        }

        public void DoRegisterAgent()
        {
            Console.WriteLine("REGISTER-AGENT");
            _client.RegisterDscAgent().Wait();
        }

        public void DoGetAction()
        {
            Console.WriteLine("GET-ACTION");
            _client.GetDscAction().Result?.ToArray();
        }

        public void DoGetConfiguration()
        {
            Console.WriteLine("GET-CONFIGURATION");
            
            Console.WriteLine("Getting configs:");
            foreach (var cn in _config.ConfigurationNames)
            {
                Console.WriteLine($"  * Config [{cn}]");
                var bytes = _client.GetConfiguration(cn).Result?.Content;
                Console.WriteLine($"    Got config file with [{bytes.Length}] bytes");
            }
        }

        public void DoGetActionAndConfiguration()
        {
            Console.WriteLine("GET-ACTION-AND-CONFIGURATION");

            var actions = _client.GetDscAction().Result?.ToArray();

            if (actions?.Length > 0)
            {
                Console.WriteLine("We have actions:");
                foreach(var a in actions)
                {
                    Console.WriteLine($"  * Action [{a.ConfigurationName}] = [{a.Status}]");

                    if (a.Status == Model.DscActionStatus.RETRY)
                        throw new NotSupportedException(
                                /*SR*/"Action RETRY status not supported");

                    if (a.Status == Model.DscActionStatus.UpdateMetaConfiguration)
                        throw new NotSupportedException(
                                /*SR*/"Action UpdateMetaConfiguration status not supported");

                    if (a.Status == Model.DscActionStatus.GetConfiguration)
                    {
                        var bytes = _client.GetConfiguration(a.ConfigurationName).Result?.Content;
                        Console.WriteLine($"    Got config file with [{bytes.Length}] bytes");
                    }
                }
            }
        }

        public void DoGetModule()
        {
            Console.WriteLine("GET-MODULE");
        }

        public void DoSendReport(DscPullClient client)
        {
            Console.WriteLine("SEND-REPORT");

            throw new NotImplementedException();
        }

        public static void Main(string[] args)
        {
            AppLog.Factory.AddConsole(LogLevel.Debug);

            new Program().Execute(args);
        }

        public DscPullConfig ResolveClientConfiguration()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(_commandLine.ConfigFile, optional: true)
                .AddEnvironmentVariables(_commandLine.ConfigEnvPrefix);

            if (_commandLine.ConfigValues != null)
                configBuilder.AddCommandLine(_commandLine.ConfigValues);

            var config = configBuilder.Build();

            // Optionally load additionally config files
            var addJsonFile = nameof(JsonConfigurationExtensions.AddJsonFile);
            var addJsonSingle = config[addJsonFile];
            var addJsonMultiple = config.GetSection(addJsonFile);
            if (!string.IsNullOrEmpty(config[addJsonFile]))
                configBuilder.AddJsonFile(config[addJsonFile], optional: true);
            else if (addJsonMultiple != null)
                foreach (var s in addJsonMultiple.GetChildren())
                    configBuilder.AddJsonFile(s.Value, optional: true);

            // Optionally load User Secrets if specified in the config so far
            // **************************************************************
            // NOTE:  cannot use nameof(ConfigurationExtension.AddUserSecrets)
            // because we have duplicate extension class names across different
            // assemblies and current .NET Core does not allow use to hone in to
            // a specific class within a specific assembly using the extern alias
            // feature as you can with the /r switch of the  .NET Framework CSC CLI
            var addUserSecrets = "AddUserSecrets";
            if (!string.IsNullOrEmpty(config[addUserSecrets]))
                configBuilder.AddUserSecrets(config[addUserSecrets]);

            // Rebuild config with possible additions
            config = configBuilder.Build();

            // Resolve the strongly-typed configuration model
            var clientConfig = config.GetExt<DscPullConfig>();

            // If AgentInformation is not explicitly configured
            // resolve a default instance based on context
            if (clientConfig.AgentInformation == null)
                clientConfig.AgentInformation = ComputeAgentInformation();

            return clientConfig;
        }

        public static Model.AgentInformation ComputeAgentInformation(
                string nodeName = null,
                string ipAddress = null,
                string agentVersion = DEFAULT_AGENT_VERSION)
        {
            if (nodeName == null)
            {
                nodeName = Dns.GetHostName();
            }

            if (ipAddress == null)
            {
                // TODO:  this *might* not be correct or complete -- based on issue #26
                // this is definitely a better approach than what we had before, but need
                // to do some more exploring and experimenting to see if this is all of it
                var ipList = new System.Collections.Generic.List<IPAddress>();
                foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                {
                    // Collect all IPv4 & IPVv6 address
                    foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                        if (ua.Address.AddressFamily == AddressFamily.InterNetwork
                                || ua.Address.AddressFamily == AddressFamily.InterNetworkV6)
                            ipList.Add(ua.Address);
                }
                // Sort by IPv4 first, then v6
                ipAddress = string.Join(";", ipList.OrderBy(x => x.AddressFamily)
                        .Select(x => x.ToString()));
            }

            return new Model.AgentInformation
            {
                LCMVersion = agentVersion,
                NodeName = nodeName,
                IPAddress = ipAddress,
            };
        }
    }
}
