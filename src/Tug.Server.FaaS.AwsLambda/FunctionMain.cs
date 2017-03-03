/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using Amazon.Lambda.Core;
using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Configuration;
using Tug.Server.FaaS.AwsLambda.Configuration;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Microsoft.Extensions.Logging;

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

        // /// <summary>
        // /// Default constructor that Lambda will invoke.
        // /// </summary>
        // public FunctionMain()
        // { }


        // /// <summary>
        // /// A Lambda function to respond to HTTP Get methods from API Gateway
        // /// </summary>
        // /// <param name="request"></param>
        // /// <returns>The list of blogs</returns>
        // public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
        // {
        //     context.Logger.LogLine("Get Request\n");

        //     var response = new APIGatewayProxyResponse
        //     {
        //         StatusCode = (int)HttpStatusCode.OK,
        //         Body = "Hello AWS Serverless",
        //         Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        //     };

        //     return response;
        // }
    }
}
