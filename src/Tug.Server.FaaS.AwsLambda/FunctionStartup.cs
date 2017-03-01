/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using Tug.Server.Filters;

namespace Tug.Server.FaaS.AwsLambda
{
    public class FunctionStartup
    {
        /// <summary>
        /// Prefix used to identify environment variables that can override server app
        /// configuration.
        /// </summary>
        public const string APP_CONFIG_ENV_PREFIX = "TUG_";

        private IConfigurationRoot _config;
        private ILogger _logger;

        public FunctionStartup(IHostingEnvironment env)
        {
            // _logger = logger;
            // _logger.LogInformation("Startup constructed");

            var builder = new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables(APP_CONFIG_ENV_PREFIX);

            _config = builder.Build();
        }

        // This method gets called by the runtime. Use this
        // method to add services to the container.  For more
        // information on how to configure your application,
        // visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // _logger.LogInformation("Configuring services registry");

            // Enable and bind to strongly-typed configuration
            // consumers should add dependency on one of:
            //    IOptions<AppSettings>
            //    IOptionsMonitor<AppSettings> - supports updates
            //    IOptionsSnapshot<AppSettings> - session/request-scoped
            services.AddOptions();
            services.Configure<Configuration.AppSettings>(_config);
            services.Configure<Configuration.PullServiceSettings>(
                    _config.GetSection(nameof(Configuration.AppSettings.PullService)));
            
            // // Register a single instance of each filter type we'll use down below
            services.AddSingleton<DscRegKeyAuthzFilter.IAuthzStorageHandler,
                    LambdaAuthzStorageHandler>();
            services.AddSingleton<DscRegKeyAuthzFilter>();
            services.AddSingleton<StrictInputFilter>();
            services.AddSingleton<VeryStrictInputFilter>();

            // Add MVC-supporting services
            //_logger.LogInformation("Adding MVC services");
            services.AddMvc(options =>
            {
                // Add the filter by service type reference
                options.Filters.AddService(typeof(DscRegKeyAuthzFilter));
                options.Filters.AddService(typeof(VeryStrictInputFilter));
            }).AddJsonOptions(options =>
            {
                // This enables converting Enums to/from their string names instead
                // of their numerical value, based on:
                //    * https://www.exceptionnotfound.net/serializing-enumerations-in-asp-net-web-api/
                //    * https://siderite.blogspot.com/2016/10/controlling-json-serialization-in-net.html
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });
            
            // // Register the Provider Managers 
            // services.AddSingleton<ChecksumAlgorithmManager>();
            // services.AddSingleton<DscHandlerManager>();

            // // Register the Helpers
            // services.AddSingleton<ChecksumHelper>();
            // services.AddSingleton<DscHandlerHelper>();

            services.AddDefaultAWSOptions(_config.GetAWSOptions());

            services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();
            services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDBStreams>();
            services.AddAWSService<Amazon.S3.IAmazonS3>();
            services.AddAWSService<Amazon.CloudFront.IAmazonCloudFront>();
        }

        // This method gets called by the runtime. Use this
        // method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
                ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLambdaLogger(_config);
            _logger = loggerFactory.CreateLogger<FunctionStartup>();

            _logger.LogInformation("Configuring MVC");
            app.UseMvc(routeBuilder =>
            {
                // Default route welcome message
                _logger.LogInformation("adding default route");
                routeBuilder.MapGet("", context =>
                {
                    return context.Response.WriteAsync("Welcome to FaaS Tug!");
                });

                // Server version info
                _logger.LogInformation("adding version route");
                routeBuilder.MapGet("version", context =>
                {
                    var version = GetType().GetTypeInfo().Assembly.GetName().Version;
                    return context.Response.WriteAsync($@"{{""version"":""{version}""}}");
                });
            });
        }
    }
}