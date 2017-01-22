/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Tug.Messages;
using Tug.Messages.ModelBinding;
using Tug.Model;
using Tug.Client.Configuration;
using Tug.Util;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace Tug.Client
{
    public class DscPullClient : IDisposable
    {
        private static readonly ILogger LOG = AppLog.Create<DscPullClient>();

        public const string COMPUTE_MS_DATE_HEADER = "%NOW%";

        public const string REGISTRATION_MESSAGE_TYPE_CONFIGURATION_REPOSITORY = "ConfigurationRepository";

        public static readonly SendReportBody SendReportRequestBodyDefault =
                new SendReportBody
                {
                    OperationType = "Consistency",
                    RefreshMode = DscRefreshMode.Pull,
                    Status = "Success",
                    ReportFormatVersion = "2.0",
                    ConfigurationVersion = "2.0",
                    StartTime = DscPullConfig.SendReportConfig.DATETIME_NOW_TOKEN,
                    EndTime = DscPullConfig.SendReportConfig.DATETIME_NOW_TOKEN,
                    RebootRequested = DscTrueFalse.False,
                };


        private JsonSerializerSettings _jsonSerSettings;

        // A function that generates a client given a client handler
        private Func<HttpClientHandler, HttpClient> _clientFactory = x => new HttpClient(x);

        public DscPullClient(DscPullConfig config, Func<HttpClientHandler, HttpClient> clientFactory = null)
        {
            Configuration = config;
            if (clientFactory != null)
                _clientFactory = clientFactory;

            LOG.LogInformation("Initializing DSC Pull Client -- validating state");

            AssertState(Configuration != null,
                    /*SR*/"missing configuration");
            AssertState(Configuration.AgentInformation != null,
                    /*SR*/"missing configuration agent information");
            AssertState(Configuration.ConfigurationNames?.Count() > 0,
                    /*SR*/"at least one configuraiton name must be configured");
            AssertState(Configuration.ConfigurationRepositoryServer != null
                    || Configuration.ResourceRepositoryServer != null
                    || Configuration.ReportServer != null,
                    /*SR*/"at least one target server configuration must be specified");

            // Create JSON ser settings to be used in JSON to/from serializations
            _jsonSerSettings = new JsonSerializerSettings();
            // This enables converting Enums to/from their string names instead
            // of their numerical value, based on:
            //    * https://www.exceptionnotfound.net/serializing-enumerations-in-asp-net-web-api/
            //    * https://siderite.blogspot.com/2016/10/controlling-json-serialization-in-net.html
            _jsonSerSettings.Converters.Add(new StringEnumConverter());

            IsInitialized = true;

            LOG.LogInformation("Initialized DSC Pull Client");
         }

        public DscPullConfig Configuration
        { get; private set; }

        public bool IsInitialized
        { get; private set; }

        public bool IsDisposed
        { get; private set; }

        protected void AssertInit()
        {
            if (!IsInitialized)
                throw new InvalidOperationException(/*SR*/"client not initialized");

            if (IsDisposed)
                throw new InvalidOperationException(/*SR*/"client is disposed");
        }

        protected void AssertState(bool state, string exMessage)
        {
            if (!state)
                throw new InvalidOperationException(exMessage);
        }

        protected void AssertServerConfig(DscPullConfig.ServerConfig config)
        {
            AssertState(config?.ServerUrl != null,
                    /*SR*/"missing server URL configuration");
        }

        public async Task RegisterDscAgent()
        {
            if (LOG.IsEnabled(LogLevel.Trace))
                LOG.LogTrace(nameof(RegisterDscAgent));

            AssertInit();

            var serverConfig = Configuration.ConfigurationRepositoryServer;
            AssertServerConfig(serverConfig);

            var dscRequ = new RegisterDscAgentRequest
            {
                AgentId = Configuration.AgentId,
                ContentTypeHeader = DscContentTypes.JSON,
                MsDateHeader = COMPUTE_MS_DATE_HEADER,
                Body = new RegisterDscAgentRequestBody
                {
                    ConfigurationNames = Configuration.ConfigurationNames.ToArray(),
                    AgentInformation = Configuration.AgentInformation,
                    RegistrationInformation = new RegistrationInformation
                    {
                        RegistrationMessageType = REGISTRATION_MESSAGE_TYPE_CONFIGURATION_REPOSITORY,
                        CertificateInformation = Configuration.CertificateInformation
                    }
                }
            };

            await SendDscAsync(serverConfig, RegisterDscAgentRequest.VERB,
                    RegisterDscAgentRequest.ROUTE, dscRequ);
        }

        public async Task<IEnumerable<ActionDetailsItem>> GetDscAction(IEnumerable<ClientStatusItem> clientStatus = null)
        {
            if (LOG.IsEnabled(LogLevel.Trace))
                LOG.LogTrace(nameof(GetDscAction));
            
            AssertInit();

            var serverConfig = Configuration.ConfigurationRepositoryServer;
            AssertServerConfig(serverConfig);

            if (clientStatus == null)
            {
                if (Configuration.ConfigurationNames != null)
                {
                    clientStatus = Configuration.ConfigurationNames.Select(x =>
                    {
                        return new ClientStatusItem
                        {
                            ConfigurationName = x,
                            ChecksumAlgorithm = "SHA-256", // TODO: figure out this
                            Checksum = string.Empty,
                        };
                    });
                }
                else
                {
                    clientStatus = new[]
                    {
                        new ClientStatusItem
                        {
                            ConfigurationName = string.Empty,
                            ChecksumAlgorithm = "SHA-256", // TODO: figure out this
                            Checksum = string.Empty,
                        }
                    };
                }
            }

            var dscRequ = new GetDscActionRequest
            {
                AgentId = Configuration.AgentId,
                ContentTypeHeader = DscContentTypes.JSON,
                Body = new GetDscActionRequestBody
                {
                    ClientStatus = clientStatus?.ToArray()
                }
            };

            var dscResp = new GetDscActionResponse();
            
            using (var disposable = await SendDscAsync(serverConfig, GetDscActionRequest.VERB,
                    GetDscActionRequest.ROUTE, dscRequ, dscResp))
            {
                LOG.LogDebug("*********************************************************");
                LOG.LogDebug("DSC Action:  " + JsonConvert.SerializeObject(dscResp.Body,
                        _jsonSerSettings));

                if (dscResp?.Body?.Details?.Length == 0)
                {
                    return new[]
                    {
                        new ActionDetailsItem
                        {
                            ConfigurationName = string.Empty,
                            Status = (dscResp?.Body?.NodeStatus).GetValueOrDefault(),  
                        }
                    };
                }

                return dscResp.Body.Details;
            }
        }

        // TODO:  I think returning a Stream would be better here, but coordinating that
        // with the disposable resources that are contained within could be tricky
        public async Task<FileResponse> GetConfiguration(string configName)
        {
            if (LOG.IsEnabled(LogLevel.Trace))
                LOG.LogTrace(nameof(GetConfiguration));
            
            AssertInit();

            var serverConfig = Configuration.ConfigurationRepositoryServer;
            AssertServerConfig(serverConfig);

            var dscRequ = new GetConfigurationRequest
            {
                AgentId = Configuration.AgentId,
                ConfigurationName = configName,
                AcceptHeader = DscContentTypes.OCTET_STREAM,
            };

            var dscResp = new GetConfigurationResponse();

            using (var bs = new MemoryStream())
            using (var disposable = await SendDscAsync(serverConfig, GetConfigurationRequest.VERB,
                    GetConfigurationRequest.ROUTE, dscRequ, dscResp))
            {
                dscResp.Configuration.CopyTo(bs);
                return new FileResponse
                {
                    ChecksumAlgorithm = dscResp.ChecksumAlgorithmHeader,
                    Checksum = dscResp.ChecksumHeader,
                    Content = bs.ToArray(),
                };
            }
        }

        // TODO:  I think returning a Stream would be better here, but coordinating that
        // with the disposable resources that are contained within could be tricky
        public async Task<FileResponse> GetModule(string moduleName, string moduleVersion)
        {
            if (LOG.IsEnabled(LogLevel.Trace))
                LOG.LogTrace(nameof(GetConfiguration));
            
            AssertInit();

            var serverConfig = Configuration.ResourceRepositoryServer;
            AssertServerConfig(serverConfig);

            var dscRequ = new GetModuleRequest
            {
                AgentId = Configuration.AgentId.ToString(),
                ModuleName = moduleName,
                ModuleVersion = moduleVersion,
                AcceptHeader = DscContentTypes.OCTET_STREAM,
            };

            var dscResp = new GetModuleResponse();

            using (var bs = new MemoryStream())
            using (var disposable = await SendDscAsync(serverConfig, GetModuleRequest.VERB,
                    GetModuleRequest.ROUTE, dscRequ, dscResp))
            {
                dscResp.Module.CopyTo(bs);
                return new FileResponse
                {
                    ChecksumAlgorithm = dscResp.ChecksumAlgorithmHeader,
                    Checksum = dscResp.ChecksumHeader,
                    Content = bs.ToArray(),
                };
            }
        }

        /// <summary>
        /// Submits a <c>SendReport</c> message request which the body payload
        /// of the request defined as the JSON-serialized form of the resultant
        /// combination of the argument provided.
        /// </summary>
        /// <param name="defaultsProfile">optionally, the name of a profile of <c>SendReport</c>
        ///    body payload elements; these must be defined in the configuration</param>
        /// <param name="overrides">optionally, a set of element overrides of <c>SendReport</c>
        ///    body payload elements</param>
        /// <param name="statusData">optionally, an array of status data to override
        ///    in the body payload</param>
        /// <param name="errors">optionally, an array of errors to override
        ///    in the body payload</param>
        /// <param name="additionalData">optionally, a collection of additional, named
        ///    data elements to override the body payload</param>
        /// <remarks>
        /// <p>This method starts with a base payload defined by the
        /// <see cref="DscPullConfig.SendReportConfig.CommonDefaults"/> settings.
        /// If specified, a profile of elements is then merged next.
        /// Then if specified, the elements defined by the overrides.
        /// Finally, if any of the additional individual elemnents of <c>statusData</c>
        /// <c>errors</c> or <c>additionalData</c> are specified, they will replace
        /// the current elements respectively to produce a final resultant payload.
        /// </p>
        /// </remarks>
        public async Task SendReport(
                string defaultsProfile = null,
                SendReportBody overrides = null,
                string operationType = null,
                string[] statusData = null,
                string[] errors = null,
                IDictionary<string, string> additionalData = null)
        {
            List<string> jsons = new List<string>();

            var common = Configuration?.SendReport?.CommonDefaults;
            if (common != null)
            {
                jsons.Add(JsonConvert.SerializeObject(common));
            }
            else
            {
                jsons.Add(JsonConvert.SerializeObject(SendReportRequestBodyDefault));
            }

            if (defaultsProfile != null)
            {
                if (!(Configuration?.SendReport?.Profiles?.ContainsKey(
                        defaultsProfile)).GetValueOrDefault())
                {
                    throw new ArgumentException("invalid or missing defaults profile specified")
                            .WithData(nameof(defaultsProfile), defaultsProfile);
                }

                var profile = Configuration.SendReport.Profiles[defaultsProfile];
                jsons.Add(JsonConvert.SerializeObject(profile));
            }

            if (overrides != null)
            {
                jsons.Add(JsonConvert.SerializeObject(overrides));
            }

            JObject merged = null;
            foreach (var j in jsons)
            {
                if (j == null)
                    continue;

                if (merged == null)
                    merged = JObject.Parse(j);
                else
                    merged.Merge(JObject.Parse(j));
            }

            var body = merged.ToObject<SendReportBody>();

            if (operationType != null)
                body.OperationType = operationType;
            if (statusData != null)
                body.StatusData = statusData;
            if (errors != null)
                body.Errors = errors;
            if (additionalData != null)
                body.AdditionalData = additionalData.Select(
                        x => new Model.SendReportBody.AdditionalDataItem
                        {
                            Key = x.Key,
                            Value = x.Value,
                        }).ToArray();

            await SendReport(body);
        }
        
        /// <summary>
        /// Submits a <c>SendReport</c> message request which the body payload
        /// of the request defined as the JSON-serialized form of the argument.
        /// </summary>
        /// <param name="body"></param>
        public async Task SendReport(SendReportBody body)
        {
            if (LOG.IsEnabled(LogLevel.Trace))
                LOG.LogTrace(nameof(SendReport));
            
            AssertInit();

            var serverConfig = Configuration.ReportServer;
            AssertServerConfig(serverConfig);

            if (body != null)
            {
                var now = DateTime.Now.ToString(SendReportBody.REPORT_DATE_FORMAT);
                var jobId = Guid.NewGuid();

                if (Guid.Empty == body.JobId)
                    body.JobId = jobId;

                if (DscPullConfig.SendReportConfig.DATETIME_NOW_TOKEN == body.StartTime)
                    body.StartTime = now;
                
                if (DscPullConfig.SendReportConfig.DATETIME_NOW_TOKEN == body.EndTime)
                    body.EndTime = now;

            }

            var dscRequ = new SendReportRequest
            {
                AgentId = Configuration.AgentId,
                AcceptHeader = DscContentTypes.JSON,
                Body = body,
                ContentTypeHeader = DscContentTypes.JSON,
            };

            await SendDscAsync(serverConfig, SendReportRequest.VERB,
                    SendReportRequest.ROUTE, dscRequ);
        }

        public async Task<IEnumerable<SendReportBody>> GetReports(Guid? jobId = null)
        {
            if (LOG.IsEnabled(LogLevel.Trace))
                LOG.LogTrace(nameof(GetReports));
            
            AssertInit();

            var serverConfig = Configuration.ReportServer;
            AssertServerConfig(serverConfig);

            var dscRequ = new GetReportsRequest
            {
                AgentId = Configuration.AgentId,
                JobId = jobId,
            };

            if (jobId == null)
            {
                var dscResp = new GetReportsAllResponse();

                await SendDscAsync(serverConfig, GetReportsRequest.VERB,
                        GetReportsRequest.ROUTE_ALL, dscRequ, dscResp);
                
                return dscResp.Body.Value;
            }
            else
            {
                var dscResp = new GetReportsSingleResponse();

                await SendDscAsync(serverConfig, GetReportsRequest.VERB,
                        GetReportsRequest.ROUTE_SINGLE, dscRequ, dscResp);
                
                return new[] { dscResp.Body };
            }
        }

        protected async Task SendDscAsync(DscPullConfig.ServerConfig server, HttpMethod verb, string route,
                DscRequest dscRequ)
        {
            await SendDscAsync(server, verb, route, dscRequ, null);
        }

        protected async Task<IDisposable> SendDscAsync(DscPullConfig.ServerConfig server, HttpMethod verb, string route,
                DscRequest dscRequ, DscResponse dscResp)
        {
            if (LOG.IsEnabled(LogLevel.Trace))
                LOG.LogTrace(nameof(SendDscAsync));

            AssertInit();

            dscRequ.ProtocolVersionHeader = "2.0";

            var routeExpanded = route;
            if (dscRequ is DscAgentRequest)
            {
                var dscAgentRequ = (DscAgentRequest)dscRequ;
                routeExpanded = route.Replace("{AgentId}", dscAgentRequ.AgentId.ToString());
            }

            var requUrl = new Uri(server.ServerUrl, routeExpanded);
            if (LOG.IsEnabled(LogLevel.Debug))
                LOG.LogDebug("Computed request URL:  [{url}]", requUrl);

            var httpHandler = new HttpClientHandler();
            if (server.Proxy?.Instance != null)
            {
                if (LOG.IsEnabled(LogLevel.Debug))
                    LOG.LogDebug("Enabling Proxy:  [{proxy}] supported=[{supported}]",
                            server.Proxy, httpHandler.SupportsProxy);
                httpHandler.UseProxy = true;
                httpHandler.Proxy = server.Proxy.Instance;
            }

            var requMessage = new HttpRequestMessage
            {
                Method = verb,
                RequestUri = requUrl,
            };

            // By default, we only send JSON unless something else was specified
            var contentType = dscRequ.ContentTypeHeader;
            if (string.IsNullOrEmpty(contentType))
                contentType = DscContentTypes.JSON;
            requMessage.Headers.Accept.Clear();
            requMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            
            ExtractFromRequestModel(dscRequ, requMessage);

            // See if we need to add RegKey authorization data
            if (!string.IsNullOrEmpty(server.RegistrationKey)
                    && dscRequ.MsDateHeader == COMPUTE_MS_DATE_HEADER
                    && requMessage.Content != null)
            {
                LOG.LogInformation("Computing RegKey Authorization");

                // Shhh!  This is the super-secret formula for computing an
                // Authorization challenge when using Reg Key Authentication
                // Details can be found at /references/regkey-authorization.md
                var msDate = DateTime.UtcNow.ToString(DscRequest.X_MS_DATE_FORMAT);
                requMessage.Headers.Remove(DscRequest.X_MS_DATE_HEADER);
                requMessage.Headers.Add(DscRequest.X_MS_DATE_HEADER, msDate);
                dscRequ.MsDateHeader = null;

                var macKey = Encoding.UTF8.GetBytes(server.RegistrationKey);

                using (var sha = SHA256.Create())
                using (var mac = new HMACSHA256(macKey))
                using (var ms = new MemoryStream())
                {
                    await requMessage.Content.CopyToAsync(ms);

                    var body = ms.ToArray();
                    LOG.LogDebug("Computing hash over body content");
                    LOG.LogDebug("BODY:-----------------------------");
                    LOG.LogDebug($"<{Encoding.UTF8.GetString(body)}>");
                    LOG.LogDebug("-----------------------------:BODY");

                    var digest = sha.ComputeHash(body);
                    var digB64 = Convert.ToBase64String(digest);
                    LOG.LogDebug("  * digB64=" + digB64);
                    var concat = $"{digB64}\n{msDate}";
                    LOG.LogDebug("  * concat=" + concat);
                    var macSig = mac.ComputeHash(Encoding.UTF8.GetBytes(concat));
                    var sigB64 = Convert.ToBase64String(macSig);

                    requMessage.Headers.Authorization = new AuthenticationHeaderValue("Shared", sigB64);
                }
            }

            // TODO:  Eventually we'll address this improper usage pattern as described here:
            //    https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            HttpResponseMessage respMessage = null;
            using (var http = _clientFactory(httpHandler))
            {
                respMessage = await http.SendAsync(requMessage);
            }

            respMessage.EnsureSuccessStatusCode();

            if (dscResp == null)
                return null;
            else
                return await ExtractToResponseModel(respMessage, dscResp);
        }

        protected void ExtractFromRequestModel(DscRequest dscRequ, HttpRequestMessage requMessage)
        {
            // For POST or PUT requests, we search to see if any
            // property is supposed to be sent as the request body
            if (requMessage.Method == HttpMethod.Post || requMessage.Method == HttpMethod.Put)
            {
                requMessage.Content = ExtractBodyFromRequestModel(dscRequ);
            }

            // This will be resolved and populated on-demand
            // if there are any "FromRoute" model properties
            StringBuilder route = null;

            foreach (var pi in dscRequ.GetType().GetProperties())
            {
                var fromHeader = pi.GetCustomAttribute(typeof(FromHeaderAttribute))
                        as FromHeaderAttribute;
                if (fromHeader != null)
                {
                    var name = fromHeader.Name;
                    if (string.IsNullOrEmpty(name))
                        name = pi.Name;
                    
                    var value = pi.GetValue(dscRequ);
                    if (value == null)
                        continue;

                    if (!typeof(string).IsAssignableFrom(value.GetType()))
                        value = ConvertTo<string>(value);

                    if (LOG.IsEnabled(LogLevel.Debug))
                        LOG.LogDebug("Extracting request header [{name}] from property [{property}]",
                                name, pi.Name);

                    if (!(TryAddHeader(requMessage.Headers, name, (string)value, replace: true)))
                        if (!(TryAddHeader(requMessage.Content?.Headers, name, (string)value,
                                replace: true)))
                            throw new Exception(
                                    /*SR*/"unable to add header anywhere to request message")
                                    .WithData(nameof(name), name)
                                    .WithData(nameof(value), value);
                        else if (LOG.IsEnabled(LogLevel.Debug))
                            LOG.LogDebug("    added as CONTENT header");
                    else if (LOG.IsEnabled(LogLevel.Debug))
                        LOG.LogDebug("    added as REQUEST header");

                    continue;
                }

                var fromRoute = pi.GetCustomAttribute(typeof(FromRouteAttribute))
                        as FromRouteAttribute;
                if (fromRoute != null)
                {
                    if (route == null)
                        // This must be the first property that updates the route
                        route = new StringBuilder(requMessage.RequestUri.ToString());

                    var name = fromRoute.Name;
                    if (string.IsNullOrEmpty(name))
                        name = pi.Name;
                    
                    var value = pi.GetValue(dscRequ);
                    if (value == null)
                        // TODO: is this the right assumption???
                        value = string.Empty;
                    
                    if (!typeof(string).IsAssignableFrom(value.GetType()))
                        value = ConvertTo<string>(value);

                    if (LOG.IsEnabled(LogLevel.Debug))
                        LOG.LogDebug("Extracting route element [{name}] from property [{property}]", name, pi.Name);
                    
                    // Replace all occurrences of the route element
                    // name wrapped in curlys with this value
                    route.Replace($"{{{name}}}", (string)value);
                }

                // TODO:  Also need to do FromQuery
            }

            // If the route was modified, update the request's URL
            if (route != null)
                requMessage.RequestUri = new Uri(route.ToString());
        }

        protected bool TryAddHeader(HttpHeaders headers, string name, string value, bool replace = false)
        {
            if (headers == null)
                return false;

            IEnumerable<string> values;
            if (replace && headers.TryGetValues(name, out values) && values.Count() > 0)
                headers.Remove(name);

            return headers.TryAddWithoutValidation(name, value);
        }

        protected HttpContent ExtractBodyFromRequestModel(DscRequest dscRequ)
        {
            HttpContent content = null;
            PropertyInfo fromBodyProperty = null;
            foreach (var pi in dscRequ.GetType().GetProperties())
            {
                var attr = pi.GetCustomAttribute(typeof(FromBodyAttribute))
                        as FromBodyAttribute;
                if (attr != null)
                {
                    fromBodyProperty = pi;
                    break;
                }
            }

            if (fromBodyProperty != null)
            {
                // We test for a few principal property types that we can send directly as body,
                // content and otherwise we assume a custom model object that we serialize via JSON

                var required = fromBodyProperty.GetCustomAttribute(typeof(RequiredAttribute))
                        as RequiredAttribute;

                if (typeof(string).IsAssignableFrom(fromBodyProperty.PropertyType))
                {
                    var body = (string)fromBodyProperty.GetValue(dscRequ);
                    if (body != null)
                        content = new StringContent(body);
                }
                else if (typeof(byte[]).IsAssignableFrom(fromBodyProperty.PropertyType))
                {
                    var body = (byte[])fromBodyProperty .GetValue(dscRequ);
                    if (body != null)
                        content = new ByteArrayContent(body);
                }
                else if (typeof(Stream).IsAssignableFrom(fromBodyProperty.PropertyType))
                {
                    var body = (Stream)fromBodyProperty.GetValue(dscRequ);
                    if (body != null)
                        content = new StreamContent(body);
                }
                else
                {
                    var body = fromBodyProperty.GetValue(dscRequ);
                    if (body != null)
                    {
                        var bodySer = JsonConvert.SerializeObject(body, _jsonSerSettings);
                        content = new StringContent(bodySer);
                    }
                    else if ((bool)required?.AllowEmptyStrings)
                    {
                        content = new StringContent(string.Empty);
                    }
                }
            }

            return content;
        }

        // This routine is the client-side complement of
        //    Tug.Server.Controllers.ModelResultExt#Model()
        //
        // This routine returns an IDisposable that should be cleaned up by the caller
        // after they are done working with the dscResp instance to release any possible
        // temporary resources that were created to satisfy the model binding
        protected async Task<IDisposable> ExtractToResponseModel(HttpResponseMessage respMessage, DscResponse dscResp)
        {
            // We keep track of any disposables that we have to create during
            // the course of binding from the response the model instance and
            // then we'll clean all those up when the disposer is invoked
            var disposables = new List<IDisposable>();
            var disposer = new DisposableAction(
                    x => { foreach (var d in (List<IDisposable>)x) d.Dispose(); },
                    disposables);

            PropertyInfo toResultProperty = null; // Used to detect more than one result property

            foreach (var pi in dscResp.GetType().GetProperties())
            {
                var toHeader = pi.GetCustomAttribute(typeof(ToHeaderAttribute))
                        as ToHeaderAttribute;
                if (toHeader != null)
                {
                    var headerName = toHeader.Name;
                    if (string.IsNullOrEmpty(headerName))
                        headerName = pi.Name;
                    
                    if (LOG.IsEnabled(LogLevel.Debug))
                        LOG.LogDebug("Extracting Header[{headerName}]", headerName);
                    
                    if (respMessage.Headers.Contains(headerName))
                    {
                        object headerValue = string.Join(",",
                                respMessage.Headers.GetValues(headerName));
                        if (pi.PropertyType != typeof(string))
                            headerValue = ConvertTo(pi.PropertyType, headerValue);
                        pi.SetValue(dscResp, headerValue);
                    }
                    continue;
                }
                
                var toResult = pi.GetCustomAttribute(typeof(ToResultAttribute))
                        as ToResultAttribute;
                if (toResult != null && respMessage.Content != null)
                {
                    if (toResultProperty != null)
                        throw new InvalidOperationException("multiple Result-mapping attributes found");
                    toResultProperty = pi;

                    object resultValue = null;
                    var contentType = respMessage.Content.Headers.ContentType;
                    var toResultType = pi.PropertyType;
                    if (toResultType.IsAssignableFrom(typeof(IActionResult)))
                    {
                        resultValue = new FileContentResult(
                            await respMessage.Content.ReadAsByteArrayAsync(),
                            contentType.ToString());
                    }
                    else if (toResultType.IsAssignableFrom(typeof(byte[])))
                    {
                        resultValue = await respMessage.Content.ReadAsByteArrayAsync();
                    }
                    else if (toResultType.IsAssignableFrom(typeof(Stream)))
                    {
                        // We'll remember the stream to properly clean it up
                        var stream = await respMessage.Content.ReadAsStreamAsync();
                        disposables.Add(stream);
                        resultValue = stream;
                    }
                    else if (toResultType.IsAssignableFrom(typeof(FileInfo)))
                    {
                        // If the result type is a FileInfo, we write out the
                        // response body content to a temp file, and then register
                        // an IDisposable that will clean up the temp file
                        var filePath = Path.GetTempFileName();
                        var fileInfo = new FileInfo(filePath);
                        disposables.Add(new DisposableAction(
                            x => { ((FileInfo)x).Delete(); },
                            fileInfo));
                        resultValue = fileInfo;
                    }
                    else if (toResultType.IsAssignableFrom(typeof(string)))
                    {
                        resultValue = await respMessage.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        // Assume the result is model object that we can contruct from
                        // a JSON representation encoded in the result content
                        var json = await respMessage.Content.ReadAsStringAsync();
                        resultValue = JsonConvert.DeserializeObject(json, toResultType, _jsonSerSettings);
                    }

                    if (resultValue != null)
                        pi.SetValue(dscResp, resultValue);
                }
            }

            return disposer;
        }

        // TODO: reconcile these with the similar routines in ModelResult
        public static T ConvertTo<T>(object value)
        {
            return (T)ConvertTo(typeof(T), value);
        }

        public static object ConvertTo(Type type, object value)
        {
            var tc = TypeDescriptor.GetConverter(type);
            if (value != null && !tc.CanConvertFrom(value.GetType()))
                return tc.ConvertFromString(value.ToString());
            else
                return tc.ConvertFrom(value);
        }

        #region -- IDisposable Support --

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                IsDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DscPullClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion -- IDisposable Support --
    }

    public class DisposableAction : IDisposable
    {
        private Action<object> _disposeAction;
        private object _state;

        public DisposableAction(Action<object> disposeAction, object state)
        {
            if (disposeAction == null)
                throw new ArgumentNullException(nameof(disposeAction));

            _disposeAction = disposeAction;
            _state = state;
        }

        public bool IsDisposed
        { get; private set; }

        #region IDisposable Support


        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    _disposeAction(_state);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                IsDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DisposableAction() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}