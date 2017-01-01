using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tug.Client.Configuration;
using Tug.Client.Util;

namespace Tug.Client
{
    [TestClass]
    public class ClassicPullServerProtocolCompatibilityTests
    {
        [TestMethod]
        public void TestRegisterDscAgent() 
        {
            var config = BuildConfig();
            using (var client = new DscPullClient(config))
            {
                client.RegisterDscAgentAsync().Wait();
            }
        }

        [TestMethod]
        public void TestGetDscAction()
        {
            var config = BuildConfig();
            using (var client = new DscPullClient(config))
            {
                client.RegisterDscAgentAsync().Wait();

                var actionResult = client.GetDscActionAsync().Result;
                Assert.IsNotNull(actionResult, "Action result is not null");

                var resultArr = actionResult.ToArray();
                Assert.AreEqual(1, resultArr.Length, "Number of action results");
                Assert.AreEqual(config.ConfigurationNames.First(), resultArr[0]?.ConfigurationName,
                        "Action result config name");
                Assert.AreEqual(Model.DscActionStatus.GetConfiguration, resultArr[0].Status,
                        "Action result status");
            }
        }

        [TestMethod]
        public void TestGetDscAction_NonExistentConfig()
        {
            var config = BuildConfig();
            config.ConfigurationNames = new[] { "NoSuchConfig" };

            using (var client = new DscPullClient(config))
            {
                client.RegisterDscAgentAsync().Wait();

                var actionResult = client.GetDscActionAsync(new[]
                {
                    new Model.ClientStatusItem
                    {
                        ConfigurationName = "NoSuchConfig",
                        ChecksumAlgorithm = "SHA-256",
                        Checksum = "",
                    }
                }).Result;

                Assert.IsNotNull(actionResult, "Action result is not null");

                var resultArr = actionResult.ToArray();
                Assert.AreEqual(1, resultArr.Length, "Number of action results");
                Assert.AreEqual("NoSuchConfig", resultArr[0]?.ConfigurationName,
                        "Action result config name");
                Assert.AreEqual(Model.DscActionStatus.RETRY, resultArr[0].Status,
                        "Action result status");
            }
        }

        [TestMethod]
        public void TestGetConfiguration()
        {
            var config = BuildConfig();
            using (var client = new DscPullClient(config))
            {
                client.RegisterDscAgentAsync().Wait();

                var actionResult = client.GetDscActionAsync(new[]
                {
                    new Model.ClientStatusItem
                    {
                        ConfigurationName = "TestConfig1",
                        ChecksumAlgorithm = "SHA-256",
                        Checksum = "",
                    }
                }).Result;
                Assert.IsNotNull(actionResult, "Action result is not null");

                var resultArr = actionResult.ToArray();
                Assert.AreEqual(1, resultArr.Length, "Number of action results");
                Assert.AreEqual("TestConfig1", resultArr[0]?.ConfigurationName,
                        "Action result config name");
                Assert.AreEqual(Model.DscActionStatus.GetConfiguration, resultArr[0].Status,
                        "Action result status");

                var configResult = client.GetConfiguration(resultArr[0]?.ConfigurationName).Result;
                Assert.IsNotNull(configResult, "Configuration content not null");
                Assert.AreNotEqual(0, configResult, "Configuration content length > 0");
            }
        }

        private static DscPullConfig BuildConfig()
        {
            var config = new DscPullConfig();

            config.AgentId = Guid.NewGuid();
            config.AgentInformation = Program.ComputeAgentInformation();
            config.ConfigurationNames = new[] { "TestConfig1" };

            // This is required for RegKey Authz
            config.CertificateInformation = new Model.CertificateInformation
            {
                FriendlyName = "Tug.Client-Test",
                Issuer = "Tug.Client.Test",
                NotAfter = DateTime.Now.AddYears(1).ToString("O"),
                NotBefore = DateTime.Now.AddMinutes(-10).ToString("O"),
                Subject = "Tug.Client-TestNode",
                PublicKey = "U3lzdGVtLlNlY3VyaXR5LkNyeXB0b2dyYXBoeS5YNTA5Q2VydGlmaWNhdGVzLlB1YmxpY0tleQ==",
                Thumbprint = "8351F16C2B06634279F2C0287B5430452DA1CD94",
                Version = 3,
            };

            config.ConfigurationRepositoryServer = new DscPullConfig.ServerConfig
            {
                ServerUrl = new Uri("http://DSC-SERVER1.tugnet:8080/PSDSCPullServer.svc/"),
                RegistrationKey = "c3ea5066-ce5a-4d12-a42a-850be287b2d8",

                Proxy = new BasicWebProxy("http://localhost:8888")
            };

            return config;
        }
    }
}
