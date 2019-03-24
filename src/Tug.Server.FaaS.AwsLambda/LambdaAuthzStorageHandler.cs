// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
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
        private ILogger _logger;
        private IAmazonS3 _s3;
        private PullServiceSettings _settings;

        /// Points to the filename of the last locally-cached reg keys
        private string _regKeyCache = null;
        /// The time of the last retrieval of the locally-cached reg keys
        private DateTime _regKeyCacheUpdated = DateTime.MinValue;
        /// R/W lock around the refresh of the local reg keys cache file
        private ReaderWriterLockSlim _regKeyCacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public LambdaAuthzStorageHandler(ILogger<LambdaAuthzStorageHandler> logger,
                IAmazonS3 s3, IOptions<PullServiceSettings> settings)
        {
            _logger = logger;
            _s3 = s3;
            _settings = settings.Value;

            _logger.LogInformation("RegKeyAuthz Storage Handler created");
            
            // We used to try to initialize the cache here, but
            // this always caused the process to term in Labmda
            //InitCache();
        }

        public void InitCache()
        {
            _logger.LogInformation("Seeding the initial cache");
            UpdateRegKeyCache();

            var mins = _settings.AuthzRegKeysRefreshMins;
            _logger.LogInformation("Refresh interval [{mins}] minutes requested", mins);
            if (mins > 0)
            {
                try
                {
                    // Schedule to have it updated on a regular interval
                    // Threading in Lambda is a "kinda best effort" affair
                    // in that there is no autonomous execution that takes
                    // place, only execution that runs in response to some
                    // trigger event, such as an HTTP request (trickled in
                    // from the API Gateway, for example).  Presumably the
                    // way this happens is that the underlying container
                    // is normally suspended and only resumed only when an
                    // event triggers execution.  And so any threads that
                    // we spin off actually get suspended when the container
                    // is not running, and will execute in tandem with any
                    // "main" threads that are servicing actual triggering
                    // events.  This behavior is OK as long as we know and
                    // accept the consequences -- in our case this means
                    // that the suggested cache refresh time is really just
                    // a lower bound, as the cache refresh thread will not
                    // run if there is no activity that resumes the container.
                    new Thread(() =>
                    {
                        while (true)
                        {
                            Thread.Sleep(mins * 60 * 1000);
                            try
                            {
                                UpdateRegKeyCache();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(-1, ex, "Error during attempt to update AuthzRegKey cache");
                            }
                        }
                    })
                    {
                        IsBackground = true,
                    }.Start();

                    _logger.LogInformation("Scheduled cache refresh every [{mins}]", mins);
                }
                catch (Exception ex)
                {
                    _logger.LogError(-1, ex, "Failed to schedule recurring cache refresh");
                }
            }
            else
            {
                _logger.LogInformation("cache refresh is DISABLED");
            }
        }

        /// <summary>
        /// Updates the local cache of Authz Reg Keys used to authorize
        /// node registrations.
        /// </summary>
        /// <remarks>
        /// The authz reg keys are defined in a plain text file located
        /// at a user-defined S3 object.  At startup we load this object
        /// from S3 and store it in a local file (local to Lambda execution
        /// context located at /tmp as per the Ephemeral disk defined here:
        /// http://docs.aws.amazon.com/lambda/latest/dg/limits.html).
        ///
        /// Optional, on a regular user-defined cadence (defaults to 15 mins)
        /// we refresh this locally cached copy.  We use a swapping strategy
        /// to minimize the contention between this cache-writer and all the
        /// possible cache-reader threads.  Namely, we pull the S3 object to
        /// a local cache file with a computed random name on each pull, then
        /// "swap" the cache file name that is used in process, and after
        /// swapping, delete the old.
        /// </remarks>
        public void UpdateRegKeyCache()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Updating authz reg key cache");

            // Pull the auhz reg keys from an S3 object and save
            // to a cache file in Lambda-local Ephemeral storage
            var newFileName = $"/tmp/tug-authz-reg-keys-{Path.GetRandomFileName()}";
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("FETCHING keys from [{s3key}]...", _settings.S3KeyAuthzRegKeys);
                var getResp = _s3.GetObjectAsync(_settings.S3Bucket, _settings.S3KeyAuthzRegKeys).Result;
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("    ...FETCHED!", _settings.S3KeyAuthzRegKeys);

                using (getResp)
                using (var rs = getResp.ResponseStream)
                using (var fs = File.OpenWrite(newFileName))
                {
                    rs.CopyTo(fs);
                }
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("    ...and STORED to [{newFileName}]!", newFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(-1, ex, "failed to retrieve and store local cache of authz reg keys");
                return;
            }

            // Swap the current (old) cache filename with the new
            // cache filename during a thread-synchronzed operation
            var oldFileName = _regKeyCache;
            try
            {
                _regKeyCacheLock.EnterWriteLock();

                _regKeyCache = newFileName;
                _regKeyCacheUpdated = DateTime.Now;
                _logger.LogDebug("Authz reg key cache updated to [{newFileName}] at [{updateTime}]",
                        newFileName, _regKeyCacheUpdated);
            }
            finally
            {
                _regKeyCacheLock.ExitWriteLock();
            }

            // If the old cache filename is present, we should clean up
            // the old file so we don't accumulate junk in local storage
            if (oldFileName != null)
            {
                try
                {
                    _logger.LogDebug("Deleting old reg key cache at [{oldFileName}]", oldFileName);
                    if (File.Exists(oldFileName))
                        File.Delete(oldFileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(-1, ex, "failed to delete old cache file at [{oldFileName}]", oldFileName);
                }
            }
            else
            {
                _logger.LogDebug("No prior reg key cache");
            }
        }

        public IEnumerable<string> RegistrationKeys
        {
            get
            {
                string[] cacheLines;
                string cache;
                DateTime cacheUpdated;

                try
                {
                    _regKeyCacheLock.EnterReadLock();

                    cache = _regKeyCache;
                    cacheUpdated = _regKeyCacheUpdated;

                    if (cache == null)
                    {
                        // This condition should only happen to the first few requests and
                        // locking should be minimal for those; after that should not be an issue

                        try
                        {
                            _regKeyCacheLock.ExitReadLock();

                            lock (_regKeyCacheLock)
                            {
                                if (cache == null)
                                {
                                    InitCache();
                                }
                            }
                        }
                        finally
                        {
                            _regKeyCacheLock.EnterReadLock();

                            // Need to refresh these after seeding the cache
                            cache = _regKeyCache;
                            cacheUpdated = _regKeyCacheUpdated;

                            if (cache == null)
                            {
                                // Still null!  Not expected!
                                _logger.LogWarning("unexpected -- local cache file is not populated");
                                throw new InvalidOperationException("authz reg key cache is missing");
                            }
                        }
                    }

                    try
                    {
                        // Always pull from the locally cached File
                        cacheLines = File.ReadAllLines(cache);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(-1, ex, "Failed to read from cache");
                        throw new Exception("failed to read from cache", ex);
                    }
                }
                finally
                {
                    _regKeyCacheLock.ExitReadLock();
                }

                _logger.LogDebug("Using cache stored at [{cacheFilename}] updated at [{cacheUpdated}]",
                        cache, cacheUpdated);

                // Resolve reg keys from file as non-blank lines after optional comments
                // (starting with a '#') and any surround whitespace have been stripped
                // NOTE -- we cannot simply return the result of chained LINQ expressions
                // as it appears to terminate the process in the Lambda environment,
                // presumably there is some nuiance with the iterator state being invalid
                // after the return -- instead we generate our own iterator
                var cacheEntries = cacheLines
                        .Select(x => x.Split(DscRegKeyAuthzFilter
                                .LocalAuthzStorageHandler.REG_KEY_FILE_COMMENT_START
                        )[0].Trim())
                        .Where(x => x.Length > 0);

                foreach (var entry in cacheEntries)
                    yield return entry;
            }
        }

        public void StoreAgentAuthorization(Guid agentId, string regKey, string regDetails)
        {
            var regKeyS3Key = $"{_settings.S3KeyPrefixAuthzRegistrations}/{agentId}.regKey";
            var regDetailsS3Key = $"{_settings.S3KeyPrefixAuthzRegistrations}/{agentId}.json";

            Task.WaitAll(
                _s3.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = _settings.S3Bucket,
                    Key = regKeyS3Key,
                    ContentBody = regKey,
                    ContentType = "plain/text",
                    CannedACL = S3CannedACL.Private,
                    StorageClass = S3StorageClass.ReducedRedundancy,
                }),
                _s3.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = _settings.S3Bucket,
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
                var getResp = _s3.GetObjectAsync(_settings.S3Bucket, regKeyS3Key).Result;
                return getResp?.ContentLength > 0 && getResp.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}