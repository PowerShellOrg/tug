/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Tug.Messages;
using Tug.Model;
using Tug.Server.FaaS.AwsLambda.Configuration;
using Tug.Server.Mvc;

namespace Tug.Server.FaaS.AwsLambda
{
    public class DscController : Controller
    {
        #region -- Constants --

        public const string ChecksumAlgorithm = "SHA-256";

        #endregion -- Constants --

        #region -- Fields --

        private ILogger<DscController> _logger;

        private IAmazonS3 _s3;

        private PullServiceSettings _settings;

        #endregion -- Fields --

        #region -- Constructors --

        public DscController(ILogger<DscController> logger, IAmazonS3 s3,
                IOptions<PullServiceSettings> settings)
        {
            _logger = logger;
            _s3 = s3;
            _settings = settings.Value;

            _logger.LogInformation("Using settings:");
            _logger.LogInformation($"  * {nameof(_settings.S3BucketName)} = [{_settings.S3BucketName}]");
            _logger.LogInformation($"  * {nameof(_settings.S3KeyPrefixRegistrations)} = [{_settings.S3KeyPrefixRegistrations}]");
            _logger.LogInformation($"  * {nameof(_settings.S3KeyPrefixConfigurations)} = [{_settings.S3KeyPrefixConfigurations}]");
            _logger.LogInformation($"  * {nameof(_settings.S3KeyPrefixModules)} = [{_settings.S3KeyPrefixModules}]");
        }

        #endregion -- Constructors --

        #region -- Methods --
        
        [HttpPut]
        [Route(RegisterDscAgentRequest.ROUTE,
            Name = RegisterDscAgentRequest.ROUTE_NAME)]
        [ActionName(nameof(RegisterDscAgent))]
        public async Task<IActionResult> RegisterDscAgent(RegisterDscAgentRequest input)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"{nameof(RegisterDscAgent)}:  {RegisterDscAgentRequest.VERB}");

            if (!ModelState.IsValid)
                return base.BadRequest(ModelState);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"AgentId=[{input.AgentId}]");

            var regS3Key = $"{_settings.S3KeyPrefixRegistrations}/{input.AgentId}.json";

            var putRequ = new PutObjectRequest
            {
                BucketName = _settings.S3BucketName,
                Key = regS3Key,
                CannedACL = S3CannedACL.Private,
                ContentBody = JsonConvert.SerializeObject(input.Body),
                ContentType = Model.DscContentTypes.JSON,
                StorageClass = S3StorageClass.ReducedRedundancy,
            };
            
            var putResp = await _s3.PutObjectAsync(putRequ);
            if (putResp.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError($"failed to save registration; unexpected HTTP status code [{putResp.HttpStatusCode}]");
                return base.StatusCode((int)HttpStatusCode.InternalServerError,
                        "unexpected error trying to persist node registration");
            }

            return this.Model(RegisterDscAgentResponse.INSTANCE);
        }

        [HttpPost]
        [Route(GetDscActionRequest.ROUTE,
            Name = GetDscActionRequest.ROUTE_NAME)]
        [ActionName(nameof(GetDscAction))]
        public async Task<IActionResult> GetDscAction(GetDscActionRequest input)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"{nameof(GetDscAction)}:  {GetDscActionRequest.VERB}");

            if (!ModelState.IsValid)
                return base.BadRequest(ModelState);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"AgentId=[{input.AgentId}]");

            RegisterDscAgentRequestBody regInfo;
            
            var regS3Key = $"{_settings.S3KeyPrefixRegistrations}/{input.AgentId}.json";
            _logger.LogDebug($"registration S3 Key resolved as [{regS3Key}]");

            using (var getResp = await _s3.GetObjectAsync(_settings.S3BucketName, regS3Key))
            {
                if (getResp == null || getResp.ContentLength == 0
                        || getResp.HttpStatusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning($"failed to get existing registration:"
                            + " contentLen=[{getResp?.ContentLength}]"
                            + " httpStatus=[{getResp.HttpStatusCode}]");
                    throw new InvalidOperationException("unknown agent id");
                }

                using (var rs = getResp.ResponseStream)
                using (var reader = new System.IO.StreamReader(rs))
                {
                    regInfo = JsonConvert.DeserializeObject<RegisterDscAgentRequestBody>(
                            await reader.ReadToEndAsync());
                }
            }

            var detail = input.Body;
            var nodeStatus = DscActionStatus.OK;
            var configCount = (int)regInfo.ConfigurationNames?.Count();
            _logger.LogDebug($"loaded registration info with config count[{configCount}]");

            string configName;

            if (detail.ClientStatus.Length > 1)
            {
                // TODO:
                return base.StatusCode((int)HttpStatusCode.NotImplemented,
                        "multiple input client statuses not yet implemented");
            }

            var clientStatus = detail.ClientStatus[0];

            if (string.IsNullOrEmpty(clientStatus.ConfigurationName))
            {
                if (regInfo.ConfigurationNames.Length == 0)
                {
                    _logger.LogWarning("no configuration specified in request and no configuration specified in registration");
                    configName = null;
                }
                else if (regInfo.ConfigurationNames.Length > 1)
                {
                    // TODO:
                    return base.StatusCode((int)HttpStatusCode.NotImplemented,
                            "multiple registered configurations not yet implemented");
                }
                else
                {
                    configName = regInfo.ConfigurationNames[0];
                }
            }
            else
            {
                configName = clientStatus.ConfigurationName;
            }

            _logger.LogDebug($"configuration name resolved as [{configName}]");

            if (configName != null)
            {
                string checksum;
                var cfgS3Key = $"{_settings.S3KeyPrefixConfigurations}/{configName}.mof";
                _logger.LogDebug($"configuration S3 Key resolved as [{cfgS3Key}]");

                // If the object key is not found, this seems to manifest as an
                // Amazon.S3.AmazonS3Exception with an underlying message of Access Denied
                using (var getResp = await _s3.GetObjectAsync(_settings.S3BucketName, cfgS3Key))
                {
                    if (getResp == null || getResp.ContentLength == 0
                            || getResp.HttpStatusCode != HttpStatusCode.OK)
                    {
                        _logger.LogWarning($"failed to get configuration by name [{cfgS3Key}]:"
                                + " contentLen=[{getResp?.ContentLength}]"
                                + " httpStatus=[{getResp.HttpStatusCode}]");
                        return NotFound();
                    }

                    using (var rs = getResp.ResponseStream)
                    {
                        checksum = ComputeChecksum(rs);
                    }
                    _logger.LogDebug($"configuration checksum resolved as [{checksum}]");
                }

                if (string.IsNullOrEmpty(clientStatus.Checksum))
                {
                    // Doesn't matter what the real checksum is,
                    // node is asking for it, pby its first time
                    nodeStatus = DscActionStatus.GetConfiguration;
                    _logger.LogDebug("client status checksum is empty; forcing [{nodeStatus}] response");
                }
                else
                {
                    // Make sure we're speaking the same language
                    if (ChecksumAlgorithm != clientStatus.ChecksumAlgorithm)
                    {
                        _logger.LogWarning($"checksum algorithm mismatch [{clientStatus.ChecksumAlgorithm}]");
                        return BadRequest("unsupported checksum algorithm");
                    }

                    // We've resolved the config name
                    nodeStatus = checksum == clientStatus.Checksum
                            ? DscActionStatus.OK
                            : DscActionStatus.GetConfiguration;
                    _logger.LogDebug($"checksum comparison resolved as [{nodeStatus.ToString()}]");
                }
            }
            else
            {
                _logger.LogWarning("no configuration name could be resolved from client status or registration");
                nodeStatus = DscActionStatus.OK;
            }

            var response = new GetDscActionResponse
            {
                Body = new GetDscActionResponseBody
                {
                    NodeStatus = nodeStatus,
                    Details = new [] { new ActionDetailsItem
                    {
                        ConfigurationName = configName,
                        Status = nodeStatus,
                    }},
                }
            };

            return this.Model(response);
        }


        [HttpGet]
        [Route(GetConfigurationRequest.ROUTE,
            Name = GetConfigurationRequest.ROUTE_NAME)]
        [ActionName(nameof(GetConfiguration))]
        public async Task<IActionResult> GetConfiguration(GetConfigurationRequest input)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"{nameof(GetConfiguration)}:  {GetConfigurationRequest.VERB}");

            if (!ModelState.IsValid)
                return base.BadRequest(ModelState);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"AgentId=[{input.AgentId}] Configuration=[{input.ConfigurationName}]");
            
            // TODO:
            // Strictly speaking, this may not be how the DSCPM
            // protocol is supposed to resolve the config name
            var configName = input.ConfigurationName ?? input.ConfigurationNameHeader;
            if (string.IsNullOrEmpty(configName))
                return base.BadRequest("empty or missing configuration name");

            var cfgS3Key = $"{_settings.S3KeyPrefixConfigurations}/{configName}.mof";

            // If the object key is not found, this seems to manifest as an
            // Amazon.S3.AmazonS3Exception with an underlying message of Access Denied
            using (var getResp = await _s3.GetObjectAsync(_settings.S3BucketName, cfgS3Key))
            {
                if (getResp == null || getResp.ContentLength == 0
                        || getResp.HttpStatusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning($"failed to get configuration by object key [{cfgS3Key}]:"
                            + " contentLen=[{getResp?.ContentLength}]"
                            + " httpStatus=[{getResp.HttpStatusCode}]");
                    return NotFound();
                }

                using (var rs = getResp.ResponseStream)
                {
                    // This will be disposed of by the MVC framework
                    var ms = new MemoryStream();
                    await rs.CopyToAsync(ms);

                    // Make sure we're at the start of the stream to compute the checksum
                    ms.Seek(0, SeekOrigin.Begin);
                    var checksum = ComputeChecksum(ms);
                    // Make sure we're at the start of the stream to return the contents
                    ms.Seek(0, SeekOrigin.Begin);

                    return this.Model(new GetConfigurationResponse
                    {
                        ChecksumAlgorithmHeader = ChecksumAlgorithm,
                        ChecksumHeader = checksum,
                        Configuration = ms,
                    });
                }
            }
        }

        [HttpGet]
        [Route(GetModuleRequest.ROUTE,
            Name = GetModuleRequest.ROUTE_NAME)]
        [ActionName(nameof(GetModule))]
        public async Task<IActionResult> GetModule(GetModuleRequest input)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"{nameof(GetModule)}:  {GetModuleRequest.VERB}");

            if (!ModelState.IsValid)
                return base.BadRequest(ModelState);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Module name=[{input.ModuleName}] Version=[{input.ModuleVersion}]");

            var modName = input.ModuleName;
            var modVers = input.ModuleVersion;
            var modS3Key = $"{_settings.S3KeyPrefixModules}/{modName}/{modVers}.zip";

            // If the object key is not found, this seems to manifest as an
            // Amazon.S3.AmazonS3Exception with an underlying message of Access Denied
            using (var getResp = await _s3.GetObjectAsync(_settings.S3BucketName, modS3Key))
            {
                if (getResp == null || getResp.ContentLength == 0
                        || getResp.HttpStatusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning($"failed to get module by object key [{modS3Key}]:"
                            + " contentLen=[{getResp?.ContentLength}]"
                            + " httpStatus=[{getResp.HttpStatusCode}]");
                    return NotFound();
                }

                using (var rs = getResp.ResponseStream)
                {
                    // This will be disposed of by the MVC framework
                    var ms = new MemoryStream();
                    await rs.CopyToAsync(ms);

                    // Make sure we're at the start of the stream to compute the checksum
                    ms.Seek(0, SeekOrigin.Begin);
                    var checksum = ComputeChecksum(ms);
                    // Make sure we're at the start of the stream to return the contents
                    ms.Seek(0, SeekOrigin.Begin);

                    return this.Model(new GetModuleResponse
                    {
                        ChecksumAlgorithmHeader = ChecksumAlgorithm,
                        ChecksumHeader = checksum,
                        Module = ms,
                    });
                }
            }
        }

        private static string ComputeChecksum(byte[] bytes)
        {
            using (var sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
            }

        }
        private static string ComputeChecksum(Stream s)
        {
            using (var sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(s)).Replace("-", "");
            }
        }

        #endregion -- Methods --
    }
}