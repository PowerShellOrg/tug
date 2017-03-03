/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using Amazon.Lambda.Core;
using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Hosting;
using System.IO;
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
        public static ILogger<T> CreatePreLogger<T>()
        {
            return _preLoggingFactory.CreateLogger<T>();
        }

        protected override void Init(IWebHostBuilder builder)
        {
            _preLoggingFactory.AddLambdaLogger();
            _logger = CreatePreLogger<FunctionMain>();
            builder
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<FunctionStartup>()
                .UseApiGateway();
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
