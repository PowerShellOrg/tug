/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;

namespace Tug.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppLog.Factory.AddConsole(LogLevel.Debug);

            var clientConfig = new DscPullConfig
            {
                AgentId = Guid.Parse("67217804-cc0b-4836-a396-5cc67eb9672e"),
                AgentInformation = ComputeAgentInformation(),

                ConfigurationNames = new[] { "TestConfig1" },

                ConfigurationRepositoryServer = new DscPullConfig.ServerConfig
                {
                    // We have to use this URL because of the hard-coded "bypass-local"
                    // behavior of HttpClient when using a Proxy, more details here:
                    //    http://stackoverflow.com/questions/12378638/how-to-make-system-net-webproxy-to-not-bypass-local-urls
                    ServerUrl = new Uri($"http://{Dns.GetHostName()}:5000"),
                    Proxy = new Util.BasicWebProxy("http://localhost:8888/"),
                }
            };

            try
            {
                using (var client = new DscPullClient(clientConfig))
                {
                    client.RegisterDscAgentAsync().Wait();
                    client.GetDscActionAsync().Wait();
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

        public static Model.AgentInformation ComputeAgentInformation(
                string nodeName = null,
                string ipAddress = null,
                string agentVersion = "2.0")
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
