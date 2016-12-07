/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using NLog.Extensions.Logging;

namespace Tug.Server
{
    public class Startup
    {
        private ILoggerFactory _preLoggerFactory;
        private ILogger _logger;

        public Startup()
        {
            // We set this up to log any events that take place before the
            // ultimate logging configuration is finalized and realized
            _preLoggerFactory = new LoggerFactory()
                .AddConsole();
            _logger = _preLoggerFactory.CreateLogger<Startup>();
            _logger.LogInformation("Commencing PRE-logging on startup");
        }

        // This method gets called by the runtime. Use this
        // method to add services to the container.  For more
        // information on how to configure your application,
        // visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            _logger.LogInformation("Adding MVC services");
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    // This enables converting Enums to/from their string names instead
                    // of their numerical value, based on:
                    //    * https://www.exceptionnotfound.net/serializing-enumerations-in-asp-net-web-api/
                    //    * https://siderite.blogspot.com/2016/10/controlling-json-serialization-in-net.html
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                });

            // TODO:  This is temporary, we'll setup logic to optionally
            // support multiple providers of checksum algorithms
            _logger.LogInformation("Registering SHA-256 checksum provider");
            services.AddSingleton<IChecksumAlgorithmProvider,
                    Tug.Providers.Sha256ChecksumAlgorithmProvider>();

            // TODO:  This is where we'll put logic to resolve the
            // selected DSC Handler based on a configured provider
            _logger.LogInformation("Registering BASIC DSC Handler");
            services.AddSingleton<IDscHandlerProvider,
                    Providers.BasicDscHandlerProvider>();
        }

        // This method gets called by the runtime. Use this
        // method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _logger.LogInformation("Preparing to resolve final runtime configuration");

            // get configuration
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json");
            var config = builder.Build();

            // set up console logging
            // TODO it would be nice to also have a text file logger
            _logger.LogInformation("Resolving logging configuration");
            if (config["LogType"] == "console" | config["LogType"] == "both") {
                _logger.LogInformation("  * enabling Console Logging");
                if (config["DebugLog"] == "true") {
                    loggerFactory.AddConsole(LogLevel.Debug);
                } else {
                    loggerFactory.AddConsole(LogLevel.Information);
                }
            }
            if (config["LogType"] == "nlog" | config["LogType"] == "both") {
                var configPath = Path.Combine(Directory.GetCurrentDirectory(), "nlog.config");
                _logger.LogInformation($"  * enabling NLog with config=[{configPath}]");
                loggerFactory.AddNLog();
                env.ConfigureNLog(configPath);
            }

            // Initiate and switch to runtime logging
            _logger.LogInformation("Instantiating runtime logging (PRE-logging will cease)");
            _logger = loggerFactory.CreateLogger<Startup>();
            _logger.LogInformation("Commencing runtime logging");

            // set development option
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // // begin registering routes for incoming requests
            // var routeBuilder = new RouteBuilder(app);

            app.UseMvc(routeBuilder =>
            {

                // Default route welcome message
                routeBuilder.MapGet("", context =>
                {
                    return context.Response.WriteAsync(@"
<h1>Welcome to Tug!</h1>
<li><a href=""/version"">Version Info</a></li>
");
                });

                // Server version info
                routeBuilder.MapGet("version", context =>
                {
                    var version = GetType().GetTypeInfo().Assembly.GetName().Version;
                    return context.Response.WriteAsync($"{{{version}}}");
                });
            });

        }
    }
}