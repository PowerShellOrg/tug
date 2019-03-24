// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
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

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Using settings:");
                _logger.LogTrace($"  * {nameof(_settings.S3Bucket)} = [{_settings.S3Bucket}]");
                _logger.LogTrace($"  * {nameof(_settings.S3KeyPrefixRegistrations)} = [{_settings.S3KeyPrefixRegistrations}]");
                _logger.LogTrace($"  * {nameof(_settings.S3KeyPrefixConfigurations)} = [{_settings.S3KeyPrefixConfigurations}]");
                _logger.LogTrace($"  * {nameof(_settings.S3KeyPrefixModules)} = [{_settings.S3KeyPrefixModules}]");
            }
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
                BucketName = _settings.S3Bucket,
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

            using (var getResp = await _s3.GetObjectAsync(_settings.S3Bucket, regS3Key))
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
            if(_logger.IsEnabled(LogLevel.Debug))
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

                // We can't simply try to get the S3 object that corresponds to the
                // config name being requested because if it doesn't exist, we see
                // Amazon.S3.AmazonS3Exception with an underlying message of Access Denied;
                // instead we try to list the object first, and if it's not there, we
                // can response to the node appropriately with our own error protocol
                var listResp = await _s3.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = _settings.S3Bucket,
                    Prefix = cfgS3Key,
                    MaxKeys = 1,
                });
                var firstS3Obj = listResp.S3Objects?.FirstOrDefault();
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("Request to list S3 objects matching requested config found"
                            + " [{s3KeyCount}] keys, first is [{s3Key}]", listResp.KeyCount,
                            firstS3Obj?.Key);

                if (listResp.KeyCount < 1 || firstS3Obj.Key != cfgS3Key)
                {
                    _logger.LogWarning($"failed to get configuration by object key [{cfgS3Key}]:");
                    return NotFound();
                }

                using (var getResp = await _s3.GetObjectAsync(_settings.S3Bucket, cfgS3Key))
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

            // We can't simply try to get the S3 object that corresponds to the
            // config name being requested because if it doesn't exist, we see
            // Amazon.S3.AmazonS3Exception with an underlying message of Access Denied;
            // instead we try to list the object first, and if it's not there, we
            // can response to the node appropriately with our own error protocol
            var listResp = await _s3.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = _settings.S3Bucket,
                Prefix = cfgS3Key,
                MaxKeys = 1,
            });
            var firstS3Obj = listResp.S3Objects.FirstOrDefault();
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Request to list S3 objects matching requested config found"
                        + " [{s3KeyCount}] keys, first is [{s3Key}]", listResp.KeyCount,
                        firstS3Obj?.Key);

            if (listResp.KeyCount < 1 || firstS3Obj.Key != cfgS3Key)
            {
                _logger.LogWarning($"failed to get configuration by object key [{cfgS3Key}]:");
                return NotFound();
            }

            using (var getResp = await _s3.GetObjectAsync(_settings.S3Bucket, cfgS3Key))
            {
                // Because of the preceding ListObjects call,
                // this should never happen, but just in case...
                if (getResp == null || getResp.ContentLength == 0
                        || getResp.HttpStatusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning($"failed to get configuration by object key [{cfgS3Key}]:"
                            + $" contentLen=[{getResp?.ContentLength}]"
                            + $" httpStatus=[{getResp.HttpStatusCode}]");
                    return NotFound();
                }

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"got configuration by object key [{cfgS3Key}]:"
                            + $" contentLen=[{getResp?.ContentLength}]"
                            + $" httpStatus=[{getResp.HttpStatusCode}]");

                using (var rs = getResp.ResponseStream)
                using (var rawMs = new MemoryStream())
                {
                    await rs.CopyToAsync(rawMs);

                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug("buffered configuration content of size [{buffLen}]", rawMs.Length);

                    // Make sure we're at the start of the stream to compute the checksum
                    rawMs.Position = 0;
                    var rawBytes = rawMs.ToArray();
                    var checksum = ComputeChecksum(rawBytes);

                    // This will be disposed of by the MVC framework
                    var b64Ms = new MemoryStream(Encoding.UTF8.GetBytes(
                            Convert.ToBase64String(rawBytes)));

                    return this.Model(new GetConfigurationResponse
                    {
                        ChecksumAlgorithmHeader = ChecksumAlgorithm,
                        ChecksumHeader = checksum,
                        Configuration = b64Ms,
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

            // We can't simply try to get the S3 object that corresponds to the
            // config name being requested because if it doesn't exist, we see
            // Amazon.S3.AmazonS3Exception with an underlying message of Access Denied;
            // instead we try to list the object first, and if it's not there, we
            // can response to the node appropriately with our own error protocol
            var listResp = await _s3.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = _settings.S3Bucket,
                Prefix = modS3Key,
                MaxKeys = 1,
            });
            var firstS3Obj = listResp.S3Objects.FirstOrDefault();
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Request to list S3 objects matching requested module found"
                        + " [{s3KeyCount}] keys, first is [{s3Key}]", listResp.KeyCount,
                        firstS3Obj?.Key);

            if (listResp.KeyCount < 1 || firstS3Obj.Key != modS3Key)
            {
                _logger.LogWarning($"failed to get module by object key [{modS3Key}]:");
                return NotFound();
            }

            using (var getResp = await _s3.GetObjectAsync(_settings.S3Bucket, modS3Key))
            {
                // Because of the preceding ListObjects call,
                // this should never happen, but just in case...
                if (getResp == null || getResp.ContentLength == 0
                        || getResp.HttpStatusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning($"failed to get module by object key [{modS3Key}]:"
                            + " contentLen=[{getResp?.ContentLength}]"
                            + " httpStatus=[{getResp.HttpStatusCode}]");
                    return NotFound();
                }

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"got module by object key [{modS3Key}]:"
                            + $" contentLen=[{getResp?.ContentLength}]"
                            + $" httpStatus=[{getResp.HttpStatusCode}]");

                using (var rs = getResp.ResponseStream)
                {
                    using (var rawMs = new MemoryStream())
                    {
                        await rs.CopyToAsync(rawMs);

                        // Make sure we're at the start of the stream to compute the checksum
                        rawMs.Seek(0, SeekOrigin.Begin);
                        var checksum = ComputeChecksum(rawMs);
                        // Make sure we're at the start of the stream to return the contents
                        rawMs.Seek(0, SeekOrigin.Begin);
                        var rawBytes = rawMs.ToArray();

                        var b64 = Encoding.UTF8.GetBytes(Convert.ToBase64String(rawBytes));
                        _logger.LogDebug("Raw bytes length [{rawBytesLen}]", rawBytes.Length);
                        _logger.LogDebug("B64 bytes length [{b64BytesLen}]", b64.Length);

                        // This will be disposed of by the MVC framework
                        var b64Ms = new MemoryStream(b64);
                        return this.Model(new GetModuleResponse
                        {
                            ChecksumAlgorithmHeader = ChecksumAlgorithm,
                            ChecksumHeader = checksum,
                            Module = b64Ms,
                        });
                    }
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