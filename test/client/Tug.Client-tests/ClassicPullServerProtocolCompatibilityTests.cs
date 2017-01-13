using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tug.Client.Configuration;
using Tug.UnitTesting;

namespace Tug.Client
{
    [TestClass]
    public class ClassicPullServerProtocolCompatibilityTests
    {
        public const string DEFAULT_AGENT_ID = "12345678-0000-0000-0000-000000000001";
        public const string DEFAULT_SERVER_URL = "http://DSC-SERVER1.tugnet:8080/PSDSCPullServer.svc/"; // "http://localhost:5000/"; // "http://DSC-LOCALHOST:5000/"; // 
        public const string DEFAULT_REG_KEY = "c3ea5066-ce5a-4d12-a42a-850be287b2d8";

        // Only for debugging/testing in DEV (i.e. with Fiddler) -- can't be const because of compile warning
        public static readonly string PROXY_URL = null; // "http://localhost:8888"; // 
        
        [ClassInitialize]
        public static void SetClientLogLevel(TestContext ctx)
        {
            Tug.Client.AppLog.Factory.AddConsole(LogLevel.Information);
        }


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
        public void TestRegisterDscAgent_BadContent_AgentInfo()
        {
            var config = BuildConfig(newAgentId: true);

            // Add an unexpected property
            config.AgentInformation["foo"] = "bar";

            using (var client = new DscPullClient(config))
            {
                TugAssert.ThrowsExceptionWhen<AggregateException>(
                        condition: (ex) =>
                            ex.InnerException is HttpRequestException
                            && ex.InnerException.Message.Contains(
                                    "Response status code does not indicate success: 400 (Bad Request)"),
                        action: () =>
                            client.RegisterDscAgentAsync().Wait(),
                        message:
                            "Throws HTTP exception for bad request (400)");
            }
        }

        [TestMethod]
        public void TestRegisterDscAgent_BadContent_CertInfo()
        {
            var config = BuildConfig(newAgentId: true);

            // Add an unexpected property
            config.CertificateInformation["foo"] = "bar";

            using (var client = new DscPullClient(config))
            {
                TugAssert.ThrowsExceptionWhen<AggregateException>(
                        condition: (ex) =>
                            ex.InnerException is HttpRequestException
                            && ex.InnerException.Message.Contains(
                                    "Response status code does not indicate success: 400 (Bad Request)"),
                        action: () =>
                            client.RegisterDscAgentAsync().Wait(),
                        message:
                            "Throws HTTP exception for bad request (400)");
            }
        }

        [TestMethod]
        public void TestRegisterDscAgent_NoRegKey()
        {
            var config = BuildConfig(newAgentId: true);

            // Remove the RegKey
            config.ConfigurationRepositoryServer.RegistrationKey = null;

            using (var client = new DscPullClient(config))
            {
                TugAssert.ThrowsExceptionWhen<AggregateException>(
                        condition: (ex) =>
                            ex.InnerException is HttpRequestException
                            && ex.InnerException.Message.Contains(
                                    "Response status code does not indicate success: 401 (Unauthorized)"),
                        action: () =>
                            client.RegisterDscAgentAsync().Wait(),
                        message:
                            "Throws HTTP exception for unauthorized (401)");
            }
        }

        [TestMethod]
        public void TestRegisterDscAgent_BadRegKey()
        {
            var config = BuildConfig(newAgentId: true);

            // Force a bad RegKey
            config.ConfigurationRepositoryServer.RegistrationKey = Guid.NewGuid().ToString();

            using (var client = new DscPullClient(config))
            {
                TugAssert.ThrowsExceptionWhen<AggregateException>(
                        condition: (ex) =>
                            ex.InnerException is HttpRequestException
                            && ex.InnerException.Message.Contains(
                                    "Response status code does not indicate success: 401 (Unauthorized)"),
                        action: () =>
                            client.RegisterDscAgentAsync().Wait(),
                        message:
                            "Throws HTTP exception for unauthorized (401)");
            }
        }

        [TestMethod]
        public void TestRegisterDscAgent_BadCert_FieldType() 
        {
            var config = BuildConfig(newAgentId: true);

            // Force bad/unexpected cert info
            var badCert = new BadVersionTypeCertInfo(config.CertificateInformation)
            {
                Version = config.CertificateInformation.Version.ToString(),
            };
            config.CertificateInformation = badCert;

            using (var client = new DscPullClient(config))
            {
                TugAssert.ThrowsExceptionWhen<AggregateException>(
                        condition: (ex) =>
                            ex.InnerException is HttpRequestException
                            // We test for one of two possible error codes, either
                            // 500 which is returned from Classic DSC Pull Server or
                            // 400 which is returned from Tug Server which could not
                            // easily or practically reproduce the same error condition
                            && (ex.InnerException.Message.Contains(
                                    "Response status code does not indicate success: 500 (Internal Server Error)")
                                || ex.InnerException.Message.Contains(
                                    "Response status code does not indicate success: 400 (Bad Request)")),
                        action: () =>
                            client.RegisterDscAgentAsync().Wait(),
                        message:
                            "Throws HTTP exception for internal server error (500)");
            }
        }

        [TestMethod]
        public void TestRegisterDscAgent_BadCert_NewField()
        {
            var config = BuildConfig(newAgentId: true);

            // Force bad/unexpected cert info
            var badCert = new BadNewFieldCertInfo(config.CertificateInformation);
            config.CertificateInformation = badCert;

            using (var client = new DscPullClient(config))
            {
                TugAssert.ThrowsExceptionWhen<AggregateException>(
                        condition: (ex) =>
                            ex.InnerException is HttpRequestException
                            && ex.InnerException.Message.Contains(
                                    "Response status code does not indicate success: 400 (Bad Request)"),
                        action: () =>
                            client.RegisterDscAgentAsync().Wait(),
                        message:
                            "Throws HTTP exception for unauthorized (401)");
            }
        }

        [TestMethod]
        public void TestRegisterDscAgent_BadCert_FieldOrder()
        {
            var config = BuildConfig(newAgentId: true);

            // Force bad/unexpected cert info
            var badCert = new BadFieldOrderCertInfo(config.CertificateInformation);
            config.CertificateInformation = badCert;

            using (var client = new DscPullClient(config))
            {
                TugAssert.ThrowsExceptionWhen<AggregateException>(
                        condition: (ex) =>
                            ex.InnerException is HttpRequestException
                            // We test for one of two possible error codes, either
                            // 401 which is returned from Classic DSC Pull Server or
                            // 400 which is returned from Tug Server which could not
                            // easily or practically reproduce the same error condition
                            && (ex.InnerException.Message.Contains(
                                    "Response status code does not indicate success: 401 (Unauthorized)")
                                || ex.InnerException.Message.Contains(
                                    "Response status code does not indicate success: 400 (Bad Request)")),
                        action: () =>
                            client.RegisterDscAgentAsync().Wait(),
                        message:
                            "Throws HTTP exception for unauthorized (401)");
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
        public void TestGetDscAction_BadContent_StatusItem()
        {
            var config = BuildConfig();
            using (var client = new DscPullClient(config))
            {
                // Construct our own status item collection
                var statusItems = new[] { new Model.ClientStatusItem() };
                statusItems[0].ChecksumAlgorithm = "SHA-256";
                statusItems[0].Checksum = "";
                statusItems[0].ConfigurationName = config.ConfigurationNames.First();
                // Inject one unexpected property
                statusItems[0]["foo"] = "bar";

                client.RegisterDscAgentAsync().Wait();

                TugAssert.ThrowsExceptionWhen<AggregateException>(
                        condition: (ex) =>
                            ex.InnerException is HttpRequestException
                            && ex.InnerException.Message.Contains(
                                    "Response status code does not indicate success: 400 (Bad Request)"),
                        action: () =>
                            client.GetDscActionAsync(statusItems).Wait(),
                        message:
                            "Throws HTTP exception for bad request (400)");
            }
        }

        [TestMethod]
        public void TestGetDscAction_NonExistentConfig()
        {
            var config = BuildConfig(newAgentId: true);
            config.ConfigurationNames = new[] { "NoSuchConfig" };

            using (var client = new DscPullClient(config))
            {
                try
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
                catch (AggregateException ex)
                        when (ex.InnerException.Message.Contains(
                                "Response status code does not indicate success: 404 (Not Found)"))
                {
                    Assert.IsInstanceOfType(ex.InnerException,
                            typeof(HttpRequestException),
                            "Expected HTTP exception for missing config");
                }
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
                Assert.IsNotNull(configResult?.Content, "Configuration content not null");
                Assert.AreNotEqual(0, configResult.Content.Length, "Configuration content length > 0");
            }
        }

        [TestMethod]
        public void TestGetConfiguration_Content()
        {
            // Get path and content of expected results
            var myPath = typeof(ClassicPullServerProtocolCompatibilityTests).GetTypeInfo().Assembly.Location;
            var myDir = Path.GetDirectoryName(myPath);
            var dscDir = Path.Combine(myDir, "../../../../../../tools/ci/DSC");
            var mofPath = Path.Combine(dscDir, "StaticTestConfig.mof");
            var csumPath = Path.Combine(dscDir, "StaticTestConfig.mof.checksum");
            var mofBody = File.ReadAllText(mofPath);
            var csumBody = File.ReadAllText(csumPath);

            var config = BuildConfig(newAgentId: true);

            config.ConfigurationNames = new[] { "StaticTestConfig" };

            using (var client = new DscPullClient(config))
            {
                client.RegisterDscAgentAsync().Wait();

                var actionResult = client.GetDscActionAsync(new[]
                {
                    new Model.ClientStatusItem
                    {
                        ConfigurationName = "StaticTestConfig",
                        ChecksumAlgorithm = "SHA-256",
                        Checksum = "",
                    }
                }).Result;
                Assert.IsNotNull(actionResult, "Action result is not null");

                var resultArr = actionResult.ToArray();
                Assert.AreEqual(1, resultArr.Length, "Number of action results");
                Assert.AreEqual("StaticTestConfig", resultArr[0]?.ConfigurationName,
                        "Action result config name");
                Assert.AreEqual(Model.DscActionStatus.GetConfiguration, resultArr[0].Status,
                        "Action result status");
                
                var configResult = client.GetConfiguration(resultArr[0]?.ConfigurationName).Result;
                Assert.IsNotNull(configResult?.Content, "Configuration content not null");
                Assert.AreNotEqual(0, configResult.Content.Length, "Configuration content length > 0");

                Assert.AreEqual(csumBody, configResult.Checksum, "Expected MOF config checksum");

                // The fixed content is expected to be in UTF-16 Little Endian (LE)
                var configBody = Encoding.Unicode.GetString(configResult.Content);
                // Skip the BOM
                configBody = configBody.Substring(1);

                Assert.AreEqual(mofBody, configBody, "Expected MOF config content");
            }
        }

        [TestMethod]
        public void TestGetModule_Content()
        {
            var modName = "xPSDesiredStateConfiguration";
            var modVers = "5.1.0.0";

            // Get path and content of expected results
            var myPath = typeof(ClassicPullServerProtocolCompatibilityTests).GetTypeInfo().Assembly.Location;
            var myDir = Path.GetDirectoryName(myPath);
            var dscDir = Path.Combine(myDir, "../../../../../../tools/ci/DSC");
            var modPath = Path.Combine(dscDir, $"{modName}_{modVers}.zip");
            var csumPath = Path.Combine(dscDir, $"{modName}_{modVers}.zip.checksum");
            var modBody = File.ReadAllBytes(modPath);
            var csumBody = File.ReadAllText(csumPath);

            var config = BuildConfig();

            using (var client = new DscPullClient(config))
            {
                client.RegisterDscAgentAsync().Wait();
                
                var moduleResult = client.GetModule(modName, modVers).Result;
                Assert.IsNotNull(moduleResult?.Content, "Module content not null");
                Assert.AreNotEqual(0, moduleResult.Content.Length, "Module content length > 0");

                Assert.AreEqual(csumBody, moduleResult.Checksum, "Expected module checksum");

                CollectionAssert.AreEqual(modBody, moduleResult.Content, "Expected MOF config content");
            }
        }

        private static DscPullConfig BuildConfig(bool newAgentId = false)
        {
            var config = new DscPullConfig();

            config.AgentId = newAgentId
                    ? Guid.NewGuid()
                    : Guid.Parse(DEFAULT_AGENT_ID);

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
                ServerUrl = new Uri(DEFAULT_SERVER_URL),
                RegistrationKey = DEFAULT_REG_KEY,
            };

            
            // Only for debugging/testing in DEV (i.e. with Fiddler)
            if (PROXY_URL != null)
                config.ConfigurationRepositoryServer.Proxy = new Util.BasicWebProxy(PROXY_URL);

            // Resource Server endpoint URL same as Config Server endpoint URL
            config.ResourceRepositoryServer = config.ConfigurationRepositoryServer;

            return config;
        }


        public class BadVersionTypeCertInfo : Model.CertificateInformation
        {
            public BadVersionTypeCertInfo(Model.CertificateInformation copyFrom) : base(copyFrom)
            { }

            public new string FriendlyName { get; set; }
            public new string Issuer { get; set; }
            public new string NotAfter { get; set; }
            public new string NotBefore { get; set; }
            public new string Subject { get; set; }
            public new string PublicKey { get; set; }
            public new string Thumbprint { get; set; }

            // This is expected to be a number not a string
            public new string Version
            { get; set; }
        }

        public class BadNewFieldCertInfo : Model.CertificateInformation
        {
            public BadNewFieldCertInfo(Model.CertificateInformation copyFrom) : base(copyFrom)
            { }

            public new string FriendlyName { get; set; }
            public new string Issuer { get; set; }
            public new string NotAfter { get; set; }
            public new string NotBefore { get; set; }
            public new string Subject { get; set; }
            public new string PublicKey { get; set; }
            public new string Thumbprint { get; set; }
            public new int Version { get; set; }

            // This is not a valid CertInfo field
            public string Foo
            { get; set; } = "BAR";
        }


        public class BadFieldOrderCertInfo : Model.CertificateInformation
        {
            public BadFieldOrderCertInfo(Model.CertificateInformation copyFrom) : base(copyFrom)
            { }

            // Redefining this field forces it to
            // go to the top of serialization order
            public new int Version { get; set; }
        }
    }
}
