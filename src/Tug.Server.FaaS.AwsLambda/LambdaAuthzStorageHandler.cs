/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tug.Server.FaaS.AwsLambda.Configuration;
using Tug.Server.Filters;

namespace Tug.Server.FaaS.AwsLambda
{
    public class LambdaAuthzStorageHandler : DscRegKeyAuthzFilter.IAuthzStorageHandler
    {
        public const int CACHE_TIME_SECS = 30;

        private ILogger _logger;
        private IAmazonS3 _s3;
        private PullServiceSettings _settings;

        private string _regKeyCache = "/tmp/tug-authz-reg-keys";
        private DateTime _regKeyCacheUpdated = DateTime.MinValue;


        public LambdaAuthzStorageHandler(ILogger<LambdaAuthzStorageHandler> logger,
                IAmazonS3 s3, IOptions<PullServiceSettings> settings)
        {
            _logger = logger;
            _s3 = s3;
            _settings = settings.Value;
        }

        public IEnumerable<string> RegistrationKeys
        {
            get
            {
                var now = DateTime.Now;
                if ((now - _regKeyCacheUpdated).TotalSeconds > CACHE_TIME_SECS
                        || !File.Exists(_regKeyCache))
                {
                    // Need to refresh the local cache of reg keys
                    using (var getResp = _s3.GetObjectAsync(_settings.S3BucketName, _settings.S3KeyAuthzRegKeys).Result)
                    using (var rs = getResp.ResponseStream)
                    using (var fs = File.OpenWrite(_regKeyCache))
                    {
                        rs.CopyTo(fs);
                    }
                }

                // Always pull from the locally cached File
                // Resolve reg keys from file as non-blank lines after optional comments
                // (starting with a '#') and any surround whitespace have been stripped
                return File.ReadAllLines(_regKeyCache)
                        .Select(x => x.Split(DscRegKeyAuthzFilter
                                .LocalAuthzStorageHandler.REG_KEY_FILE_COMMENT_START
                        )[0].Trim())
                        .Where(x => x.Length > 0);
            }
        }

        public void StoreAgentAuthorization(Guid agentId, string regKey, string regDetails)
        {
            var regKeyS3Key = $"{_settings.S3KeyPrefixAuthzRegistrations}/{agentId}.regKey";
            var regDetailsS3Key = $"{_settings.S3KeyPrefixAuthzRegistrations}/{agentId}.json";

            Task.WaitAll(
                _s3.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = _settings.S3BucketName,
                    Key = regKeyS3Key,
                    ContentBody = regKey,
                    ContentType = "plain/text",
                    CannedACL = S3CannedACL.Private,
                    StorageClass = S3StorageClass.ReducedRedundancy,
                }),
                _s3.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = _settings.S3BucketName,
                    Key = regDetailsS3Key,
                    ContentBody = regDetails,
                    ContentType = Model.DscContentTypes.JSON,
                    CannedACL = S3CannedACL.Private,
                    StorageClass = S3StorageClass.ReducedRedundancy,
                }));
        }

        public bool IsAgentAuthorized(Guid agentId)
        {
            var regKeyS3Key = $"{_settings.S3KeyPrefixAuthzRegistrations}/{agentId}.regKey";

            // If the object key is not found, this seems to manifest as an
            // Amazon.S3.AmazonS3Exception with an underlying message of Access Denied
            try
            {
                var getResp = _s3.GetObjectAsync(_settings.S3BucketName, regKeyS3Key).Result;
                return getResp?.ContentLength > 0 && getResp.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}