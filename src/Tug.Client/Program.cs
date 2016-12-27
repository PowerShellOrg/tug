/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Tug.Client
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

        public static void Main(string[] args)
        {
            AppLog.Factory.AddConsole(LogLevel.Debug);

            var clientConfig = ResolveClientConfiguration(args);

            try
            {
                using (var client = new DscPullClient(clientConfig))
                {
                    //client.RegisterDscAgentAsync().Wait();

                    var configNames = client.GetDscActionAsync().Result?.ToArray();
                    if (configNames?.Length > 0)
                    {
                        Console.WriteLine("We have configs to get:");
                        foreach(var cn in configNames)
                        {
                            Console.WriteLine($"  * Config [{cn}]");
                            var bytes = client.GetConfiguration(cn).Result;
                            Console.WriteLine($"    Got config file with [{bytes.Length}] bytes");
                        }
                    }
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

        public static DscPullConfig ResolveClientConfiguration(params string[] args)
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(APP_CONFIG_FILENAME, optional: true)
                .AddEnvironmentVariables(APP_CONFIG_ENV_PREFIX)
                .AddCommandLine(args);

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
            var clientConfig = config.Get<DscPullConfig>();

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
                // TODO:  this is not correct, it doesn't return all IP addresses
                // (e.g. loopbacks) and it returns dups, but it's a start
                ipAddress = string.Join(";", NetworkInterface.GetAllNetworkInterfaces()
                        .Select(x => string.Join(";", x.GetIPProperties().DnsAddresses
                                .Select(y => y.ToString()))));
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
