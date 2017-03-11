using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tug.UnitTesting;

namespace Tug.Client
{
    [TestClass]
    public class ClassicPullServerProtocolCompatibilityTests : ProtocolCompatibilityTestsBase
    {
        [ClassInitialize]
        public new static void ClassInit(TestContext ctx)
        {
            ProtocolCompatibilityTestsBase.ClassInit(ctx);
        }

        [TestMethod]
        public void TestRegisterDscAgent()
        {
            var config = BuildConfig();
            using (var client = new DscPullClient(config))
            {
                client.RegisterDscAgent().Wait();
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
                            client.RegisterDscAgent().Wait(),
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
                            client.RegisterDscAgent().Wait(),
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
                            client.RegisterDscAgent().Wait(),
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
                            client.RegisterDscAgent().Wait(),
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
                            client.RegisterDscAgent().Wait(),
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
                            client.RegisterDscAgent().Wait(),
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
                            client.RegisterDscAgent().Wait(),
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
                client.RegisterDscAgent().Wait();

                var actionResult = client.GetDscAction().Result;
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

                client.RegisterDscAgent().Wait();

                TugAssert.ThrowsExceptionWhen<AggregateException>(
                        condition: (ex) =>
                            ex.InnerException is HttpRequestException
                            && ex.InnerException.Message.Contains(
                                    "Response status code does not indicate success: 400 (Bad Request)"),
                        action: () =>
                            client.GetDscAction(statusItems).Wait(),
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
                    client.RegisterDscAgent().Wait();

                    var actionResult = client.GetDscAction(new[]
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
                client.RegisterDscAgent().Wait();

                var actionResult = client.GetDscAction(new[]
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
                client.RegisterDscAgent().Wait();

                var actionResult = client.GetDscAction(new[]
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
                client.RegisterDscAgent().Wait();
                
                var moduleResult = client.GetModule(modName, modVers).Result;
                Assert.IsNotNull(moduleResult?.Content, "Module content not null");
                Assert.AreNotEqual(0, moduleResult.Content.Length, "Module content length > 0");

                Assert.AreEqual(csumBody, moduleResult.Checksum, "Expected module checksum");

                Assert.AreEqual(modBody.Length, moduleResult.Content.Length, "Expected module content size");
                CollectionAssert.AreEqual(modBody, moduleResult.Content, "Expected module content");
            }
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
