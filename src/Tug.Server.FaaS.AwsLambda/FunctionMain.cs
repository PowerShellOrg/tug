// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using Amazon.Lambda.Core;
using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Configuration;
using Tug.Server.FaaS.AwsLambda.Configuration;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.Hosting.Internal;
using Amazon.Lambda.AspNetCoreServer.Internal;
using System.Runtime.Serialization;

// Assembly attribute to enable the Lambda function's
// JSON input to be converted into a .NET class.
[assembly: LambdaSerializerAttribute(
    typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Tug.Server.FaaS.AwsLambda
{
    public class FunctionMain : APIGatewayProxyFunction
    {
        protected static readonly ILoggerFactory _preLoggingFactory = new LoggerFactory();
        protected static ILogger _logger;

        protected static IConfigurationRoot _hostConfig;
        protected static HostSettings _settings;
        protected static AWSOptions _awsOptions;

        public static ILogger<T> CreatePreLogger<T>()
        {
            return _preLoggingFactory.CreateLogger<T>();
        }

        protected override void Init(IWebHostBuilder builder)
        {
            _preLoggingFactory.AddLambdaLogger();
            _logger = CreatePreLogger<FunctionMain>();

            _logger.LogInformation("***** Initializing Lambda Function *****");
            ResolveHostConfig();

            builder
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<FunctionStartup>()
                .UseApiGateway();
        }

        protected void ResolveHostConfig()
        {
            _logger.LogInformation("Resolving Host Configuration");
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables(prefix: HostSettings.ConfigEnvPrefix);

            _hostConfig = configBuilder.Build();
            _settings = _hostConfig.Get<HostSettings>();
            _awsOptions = _hostConfig.GetAWSOptions();

            if (_settings.AppSettingsS3Bucket != null && _settings.AppSettingsS3Key != null)
            {
                _logger.LogInformation($"Resolved AppSettings S3 source as"
                        + $" [{_settings.AppSettingsS3Bucket}][{_settings.AppSettingsS3Key}]");
                var s3 = _awsOptions.CreateServiceClient<IAmazonS3>();
                var getResp = s3.GetObjectAsync(_settings.AppSettingsS3Bucket, _settings.AppSettingsS3Key).Result;

                var localJson = HostSettings.AppSettingsLocalJsonFile;

                using (getResp)
                using (var rs = getResp.ResponseStream)
                using (var fs = File.OpenWrite(localJson))
                {
                    rs.CopyTo(fs);
                }
                _logger.LogInformation($"Copied AppSettings from S3 source to local file at [{localJson}]");
            }
        }

        // We no longer need to do this kludge since this PR was accepted:
        //    https://github.com/aws/aws-lambda-dotnet/pull/75
        //
        // We will delete this bit of old reference code in a forthcoming
        // commit after testing and some usage confirms expected behavior

        /*
        #region -- Temporary Binary Response Content Kludge --

        // We've temporarily re-implemented some of the functionality in the base class so that
        // we can hook into, and alter the processing of the response send back out from Lambda
        // to the API Gateway.  The entry-point handler function is almost all of the same code
        // as in the base class, except for the call to our version of the <see cref="MyProcessRequest"/>
        // routine so that we can do some special processing of the response to accomodate binary
        // response content.
        //
        // This is necessary until https://github.com/aws/aws-lambda-dotnet/pull/75  gets merged
        // in or some other equivalent solution is implemented in the framework. At that  point
        // this section can be removed.


        // Manage the serialization so the raw requests and responses can be logged.
        ILambdaSerializer _serializer = new Amazon.Lambda.Serialization.Json.JsonSerializer();

        /// <summary>
        /// This method is what the Lambda function handler points to.
        /// </summary>
        public override async Task<Stream> FunctionHandlerAsync(Stream requestStream, ILambdaContext lambdaContext)
        {
            if (this.EnableRequestLogging)
            {
                StreamReader reader = new StreamReader(requestStream);
                string json = reader.ReadToEnd();
                lambdaContext.Logger.LogLine(json);
                requestStream.Position = 0;
            }

            var request = this._serializer.Deserialize<APIGatewayProxyRequest>(requestStream);

            lambdaContext.Logger.Log($"Incoming {request.HttpMethod} requests to {request.Path}");
            InvokeFeatures features = new InvokeFeatures();
            MarshallRequest(features, request);
            var context = this.CreateContext(features);

            var response = await this.MyProcessRequest(lambdaContext, context, features);

            var responseStream = new MemoryStream();
            this._serializer.Serialize<APIGatewayProxyResponse>(response, responseStream);
            responseStream.Position = 0;

            if (this.EnableResponseLogging)
            {
                StreamReader reader = new StreamReader(responseStream);
                string json = reader.ReadToEnd();
                lambdaContext.Logger.LogLine(json);
                responseStream.Position = 0;
            }


            return responseStream;
        }

        protected async Task<APIGatewayProxyResponse> MyProcessRequest(ILambdaContext lambdaContext,
                HostingApplication.Context context, InvokeFeatures features,
                bool rethrowUnhandledError = false)
        {
            _logger.LogInformation("MY PROCESS REQUEST!!!");

            var resp = await base.ProcessRequest(lambdaContext, context, features, rethrowUnhandledError);
            if (resp.Body != null && resp.Headers.ContainsKey("Content-Type"))
            {
                if ("application/octet-stream" == resp.Headers["Content-Type"])
                {
                    _logger.LogInformation("SETTING B64!!!");

                    resp = new MyAPIGatewayProxyResponse
                    {
                        Body = resp.Body,
                        Headers = resp.Headers,
                        StatusCode = resp.StatusCode,
                        IsBase64Encoded = true,
                    };
                }
            }

            return resp;
        }

        [DataContract]
        public class MyAPIGatewayProxyResponse : APIGatewayProxyResponse
        {
            /// <summary>
            /// Flag indicating whether the body should be treated as a base64-encoded string
            /// </summary>
            [DataMember(Name = "isBase64Encoded")]
            public bool IsBase64Encoded { get; set; }
        }

        // protected override APIGatewayProxyResponse MarshallResponse(IHttpResponseFeature responseFeatures, int statusCodeIfNotSet = 200)
        // {
        //     if (responseFeatures.Body != null && responseFeatures.Headers.ContainsKey("Content-Type"))
        //     {
        //         string contentType = responseFeatures.Headers["Content-Type"];
        //         if (contentType == "BINARY")
        //         {
        //             using (var rawBody = new MemoryStream())
        //             using (var oldBody = responseFeatures.Body)
        //             {
        //                 oldBody.Position = 0;
        //                 oldBody.CopyTo(rawBody);
        //                 responseFeatures.Body = new MemoryStream(Encoding.UTF8.GetBytes(Convert.ToBase64String(rawBody.ToArray()));
        //             }
        //         }
        //     }

        //     return base.MarshallResponse(responseFeatures, statusCodeIfNotSet);
        // }

        #endregion -- Temporary Binary Response Content Kludge --
        */
    }
}
