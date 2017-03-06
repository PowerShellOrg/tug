using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tug.Client.Configuration;

namespace Tug.Client
{
    public class ProtocolCompatibilityTestsBase
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

            // Flag that enables some kludges for latest Classic DSC Pull Server on WMF 5.0 platforms
            //    See:  https://github.com/PowerShell/PowerShell/issues/2921
            // We leave the default enabled to support the CI server builds, but disable on
            // local development environments where we're normally testing against Win2016
            public bool adjust_for_wmf_50
            { get; set; } = true;
        }

        // This is the date format that appears to be what the Classic DSC Pull Server
        // is using to parse and store the report dates, so we need to test against this
        public const string CLASSIC_SERVER_REPORT_DATE_FORMAT = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff";
        // On WMF 5.0, DscService formats its dates differently
        public const string CLASSIC_SERVER_REPORT_DATE_FORMAT_ALT = "MM/dd/yyyy HH:mm:ss";

        protected static TestConfig _testConfig = new TestConfig();

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

        protected static DscPullConfig BuildConfig(bool newAgentId = false)
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
    }
}