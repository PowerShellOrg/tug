using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Tug.Client.Configuration;
using Tug.UnitTesting;

namespace Tug.Client
{
    [TestClass]
    public class ClassicPullServerProtocolCompatibilityTests
    {
        // This defines configuration settings for running the
        // the tests that can be overridden on the command line
        // by using the format:   /cfg_prop=value
        public class TestConfig
        {
            public string agent_id
            { get; set; } = "12345678-0000-0000-0000-000000000001";

            public string server_url
            { get; set; } = "http://DSC-SERVER1.tugnet:8080/PSDSCPullServer.svc/"; // "http://localhost:5000/"; // "http://DSC-LOCALHOST:5000/"; // 

            public string reg_key
            { get; set; } = "c3ea5066-ce5a-4d12-a42a-850be287b2d8";

            // Only for debugging/testing in DEV (i.e. with Fiddler) -- can't be const because of compile warning
            public string proxy_url
            { get; set; } = null; // "http://localhost:8888"; // 
        }

        // This is the date format that appears to be what the Classic DSC Pull Server
        // is using to parse and store the report dates, so we need to test against this
        public const string CLASSIC_SERVER_REPORT_DATE_FORMAT = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff";

        private static TestConfig _testConfig = new TestConfig();

        [ClassInitialize]
        public static void SetClientLogLevel(TestContext ctx)
        {
            // Set the global logging configuration to log some info
            // which may be useful in debugging and diagnostics
            Tug.Client.AppLog.Factory.AddConsole(LogLevel.Information);

            // Bind optional configuration overrides to our test config
            new ConfigurationBuilder()
                .AddCommandLine(Environment.GetCommandLineArgs()
                        .Where(x => x.StartsWith("/")).ToArray())
                .Build()
                .Bind(_testConfig);
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

                CollectionAssert.AreEqual(modBody, moduleResult.Content, "Expected MOF config content");
            }
        }

        [TestMethod]
        public void TestSendReport()
        {
            var config = BuildConfig(newAgentId: true);
            using (var client = new DscPullClient(config))
            {
                client.RegisterDscAgent().Wait();
                client.SendReport("SimpleInventoryDefaults",
                        overrides: new Model.SendReportBody
                        {
                            NodeName = "MY_NAME",
                            IpAddress = "::1;127.0.01",
                        }).Wait();
                client.SendReport("DetailedStatusDefaults",
                        statusData: new[] { "STATUS" }).Wait();
                client.SendReport("ErrorDefaults",
                        errors: new[] { "ERROR" }).Wait();
            }
        }

        [TestMethod]
        public void TestSendReport_BadEmptyBody()
        {
            var config = BuildConfig();
            using (var client = new DscPullClient(config))
            {
                TugAssert.ThrowsExceptionWhen<AggregateException>(
                        condition: (ex) =>
                            ex.InnerException is HttpRequestException
                            && ex.InnerException.Message.Contains(
                                    "Response status code does not indicate success: 400 (Bad Request)"),
                        action: () =>
                            client.SendReport(null).Wait(),
                        message:
                            "Throws HTTP exception for bad request (400)");
            }
        }

        [TestMethod]
        public void TestSendReport_BadMissingJobId()
        {
            var config = BuildConfig();
            using (var client = new DscPullClient(config))
            {
                TugAssert.ThrowsExceptionWhen<AggregateException>(
                        condition: (ex) =>
                            ex.InnerException is HttpRequestException
                            && ex.InnerException.Message.Contains(
                                    "Response status code does not indicate success: 400 (Bad Request)"),
                        action: () =>
                            client.SendReport(new BadSendReportBody()).Wait(),
                        message:
                            "Throws HTTP exception for bad request (400)");
            }
        }

        [TestMethod]
        public void TestSendReport_BadDateFormat()
        {
            var report = new Model.SendReportBody
            {
                JobId = Guid.NewGuid(),
                StartTime = "NOW",
                EndTime = "THEN",
                OperationType = "FOO",
                ReportFormatVersion = "BAR",
            };
            
            var config = BuildConfig();
            using (var client = new DscPullClient(config))
            {
                TugAssert.ThrowsExceptionWhen<AggregateException>(
                        condition: (ex) =>
                            ex.InnerException is HttpRequestException
                            && ex.InnerException.Message.Contains(
                                    "Response status code does not indicate success: 500 (Internal Server Error)"),
                        action: () =>
                            client.SendReport(report).Wait(),
                        message:
                            "Throws HTTP exception for internal server error (500)");
            }
        }

        [TestMethod]
        public void TestGetReports_Single()
        {
            var config = BuildConfig(newAgentId: true);
            var report = new Model.SendReportBody
            {
                JobId = Guid.NewGuid(),
                OperationType = "FOO",
                RefreshMode = Model.DscRefreshMode.Pull,
                Status = "BAR",
                ReportFormatVersion = "Spooky",
                ConfigurationVersion = "Scary",
              //StartTime = DateTime.Now.ToString(Model.SendReportBody.REPORT_DATE_FORMAT),
                StartTime = DateTime.Now.ToString(CLASSIC_SERVER_REPORT_DATE_FORMAT),
              //EndTime = DateTime.Now.ToString(Model.SendReportBody.REPORT_DATE_FORMAT),
                EndTime = DateTime.Now.ToString(CLASSIC_SERVER_REPORT_DATE_FORMAT),
                RebootRequested = Model.DscTrueFalse.False,
                StatusData = new[] { "STATUS-DATA" },
                Errors = new[] { "ERRORS" },
                AdditionalData = new[]
                {
                    new Model.SendReportBody.AdditionalDataItem { Key = "1", Value = "ONE", },
                    new Model.SendReportBody.AdditionalDataItem { Key = "2", Value = "TWO", },
                },
            };

            using (var client = new DscPullClient(config))
            {
                client.RegisterDscAgent().Wait();
                client.SendReport(report).Wait();

                var sr = client.GetReports().Result;
                Assert.IsNotNull(sr, "Reports not null");
                var srArr = sr.ToArray();
                Assert.AreEqual(1, srArr.Length, "Reports length is exactly 1");

                var ser1 = JsonConvert.SerializeObject(report);
                var ser2 = JsonConvert.SerializeObject(srArr[0]);
                Assert.AreEqual(ser1, ser2, "Submitted and retrieved reports are the same");

                sr = client.GetReports().Result;
                Assert.IsNotNull(sr, "All reports not null");
                srArr = sr.ToArray();
                Assert.AreEqual(1, srArr.Length, "All reports length is exactly 1");
            }
        }

        [TestMethod]
        public void TestGetReports_Multi()
        {
            var strArr1 = new[] { "STATUS-1" };
            var strArr2 = new[] { "STATUS-2" };
            var strArr3 = new[] { "ERROR-1" };
            var strArr4 = new[] { "ERROR-2" };

            var config = BuildConfig(newAgentId: true);
            using (var client = new DscPullClient(config))
            {
                client.RegisterDscAgent().Wait();
                client.SendReport(operationType: "1", statusData: strArr1).Wait();
                client.SendReport(operationType: "2", statusData: strArr2).Wait();
                client.SendReport(operationType: "3", errors: strArr3).Wait();
                client.SendReport(operationType: "4", errors: strArr4).Wait();

                var sr = client.GetReports().Result;
                Assert.IsNotNull(sr, "All reports not null");
                var srArr = sr.ToArray();
                Assert.AreEqual(4, srArr.Length, "All reports length");

                var srArrOrd = srArr.OrderBy(x => x.OperationType).ToArray();
                CollectionAssert.AreEqual(strArr1, srArrOrd[0].StatusData);
                CollectionAssert.AreEqual(strArr2, srArrOrd[1].StatusData);
                CollectionAssert.AreEqual(strArr3, srArrOrd[2].Errors);
                CollectionAssert.AreEqual(strArr4, srArrOrd[3].Errors);
            }
        }

        private static DscPullConfig BuildConfig(bool newAgentId = false)
        {
            var config = new DscPullConfig();

            config.AgentId = newAgentId
                    ? Guid.NewGuid()
                    : Guid.Parse(_testConfig.agent_id);

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
                ServerUrl = new Uri(_testConfig.server_url),
                RegistrationKey = _testConfig.reg_key,
            };

            // Only for debugging/testing in DEV (i.e. with Fiddler)
            if (!string.IsNullOrEmpty(_testConfig.proxy_url))
                config.ConfigurationRepositoryServer.Proxy =
                        new Util.BasicWebProxy(_testConfig.proxy_url);

            // Resource & Reporting Server endpoint URLs same as Config Server endpoint URL
            config.ResourceRepositoryServer = config.ConfigurationRepositoryServer;
            config.ReportServer = config.ConfigurationRepositoryServer;

            config.SendReport = new DscPullConfig.SendReportConfig
            {
                CommonDefaults = new Model.SendReportBody
                {
                    OperationType = "Consistency",
                    ReportFormatVersion = "2.0",
                    StartTime = "%NOW%",
                    AdditionalData = new[]
                    {
                        new Model.SendReportBody.AdditionalDataItem
                        {
                            Key = "OSVersion",
                            Value = "{\"VersionString\":\"Microsoft Windows NT 10.0.14393.0\",\"ServicePack\":\"\",\"Platform\":\"Win32NT\"}"
                        },
                        new Model.SendReportBody.AdditionalDataItem
                        {
                            Key = "PSVersion",
                            Value = "{\"CLRVersion\":\"4.0.30319.42000\",\"PSVersion\":\"5.1.14393.576\",\"BuildVersion\":\"10.0.14393.576\"}"
                        },
                    }
                },
                Profiles = new Dictionary<string, Model.SendReportBody>
                {
                    ["SimpleInventoryDefaults"] = new Model.SendReportBody
                    {
                        NodeName = "HOST_NAME",
                        IpAddress = "127.0.0.1;::1",
                        LCMVersion = "2.0",
                    },
                    ["DetailedStatusDefaults"] = new Model.SendReportBody
                    {
                        RefreshMode = Model.DscRefreshMode.Pull,
                        Status = "Success",
                        EndTime = "%NOW%",
                        ConfigurationVersion = "2.0",
                        RebootRequested = Model.DscTrueFalse.False,
                    },
                    ["ErrorDefaults"] = new Model.SendReportBody
                    { },
                },
            };

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

        public class BadSendReportBody : Model.SendReportBody
        {
            public new string JobId
            { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public new string OperationType
            { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public new string ReportFormatVersion
            { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public new string StartTime
            { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public new string Errors
            { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public new string StatusData
            { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public new string AdditionalData
            { get; set; }
        }
    }
}
