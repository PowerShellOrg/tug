using System;
using System.Linq;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Tug.Client;
using Tug.UnitTesting;

namespace Tug.Client_tests
{
    [TestClass]
    public class ClassicPullServerProtocolCompatibilityReportTests : ProtocolCompatibilityTestsBase
    {
        [ClassInitialize]
        public new static void ClassInit(TestContext ctx)
        {
            ProtocolCompatibilityTestsBase.ClassInit(ctx);
        }

        [TestMethod]
        public void TestSendReport()
        {
            var config = BuildConfig(newAgentId: true);
            using (var client = new DscPullClient(config))
            {
                client.DisableReportAdditionalData = _testConfig.adjust_for_wmf_50;
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
                client.DisableReportAdditionalData = _testConfig.adjust_for_wmf_50;
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
                client.DisableReportAdditionalData = _testConfig.adjust_for_wmf_50;
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
                client.DisableReportAdditionalData = _testConfig.adjust_for_wmf_50;
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
            var reportDateFormat = CLASSIC_SERVER_REPORT_DATE_FORMAT;
            if (_testConfig.adjust_for_wmf_50)
                reportDateFormat = CLASSIC_SERVER_REPORT_DATE_FORMAT_ALT;

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
                StartTime = DateTime.Now.ToString(reportDateFormat),
              //EndTime = DateTime.Now.ToString(Model.SendReportBody.REPORT_DATE_FORMAT),
                EndTime = DateTime.Now.ToString(reportDateFormat),
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
                client.DisableReportAdditionalData = _testConfig.adjust_for_wmf_50;
                client.RegisterDscAgent().Wait();
                client.SendReport(report).Wait();

                var sr = client.GetReports().Result;
                Assert.IsNotNull(sr, "Reports not null");
                var srArr = sr.ToArray();
                Assert.AreEqual(1, srArr.Length, "Reports length is exactly 1");

                // Unfortunate kludge to deal with broken DscService on WMF 5.1
                //    See https://github.com/PowerShell/PowerShell/issues/2921
                if (_testConfig.adjust_for_wmf_50)
                    report.AdditionalData = Model.SendReportBody.AdditionalDataItem.EMPTY_ITEMS;

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
                client.DisableReportAdditionalData = _testConfig.adjust_for_wmf_50;
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