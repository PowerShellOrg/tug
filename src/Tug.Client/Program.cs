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
                    RegistrationKey = "f65e1a0c-46b0-424c-a6a5-c3701aef32e5",
                },

                // Cert Info is a *MUST* when using RegKey authorization
                CertificateInformation = new CertificateInformation
                {
                    FriendlyName = "DSC-OaaS Client Authentication",
                    Issuer = "CN=DSC-OaaS",
                    NotAfter = DateTime.Now.AddYears(1).ToString("O"),
                    NotBefore = DateTime.Now.AddMinutes(-10).ToString("O"),
                    Subject = "CN=DSC-OaaS",
                    PublicKey = "U3lzdGVtLlNlY3VyaXR5LkNyeXB0b2dyYXBoeS5YNTA5Q2VydGlmaWNhdGVzLlB1YmxpY0tleQ==",
                    Thumbprint = "8351F16C2B06634279F2C0287B5430452DA1CD94",
                    Version = 3,
                }
            };

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
