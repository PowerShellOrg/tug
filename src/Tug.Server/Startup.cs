/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.IO;
using System.Linq;
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
        #region -- Constants --

        /// <summary>
        /// File name of a required JSON file used to configure the server app.
        /// </summary>
        public const string APP_CONFIG_FILENAME = "appsettings.json";

        /// <summary>
        /// File name of an optional JSON file used to override server app configuration.
        /// </summary>
        public const string APP_USER_CONFIG_FILENAME = "appsettings.user.json";

        /// <summary>
        /// Prefix used to identify environment variables that can override server app
        /// configuration.
        /// </summary>
        public const string APP_CONFIG_ENV_PREFIX = "TUG_CFG_";

        #endregion -- Constants --

        ILogger<Startup> _logger;

        protected IConfiguration _config;

        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Start with a pre-logger till the final
            // logging config is finalized down below
            _logger = AppLog.CreatePreLogger<Startup>();

            // This is ugly as hell but unfortunately, we could not find another
            // way to pass this along from Program to other parts of the app via DI
            var args = Program.CommandLineArgs.ToArray();

            _logger.LogInformation("Resolving final runtime configuration");
            _config = ResolveAppConfig(args);
            ConfigureLogging(env, loggerFactory);
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

        protected IConfiguration ResolveAppConfig(string[] args = null)
        {
            // Resolve the runtime configuration settings
            var appConfigBuilder = new ConfigurationBuilder();
            // Base path for any file-based config sources
            appConfigBuilder.SetBasePath(Directory.GetCurrentDirectory());
            // Default location for all configuration settings
            appConfigBuilder.AddJsonFile(APP_CONFIG_FILENAME, optional: false);
            // Optional location for user-specific local overrides
            appConfigBuilder.AddJsonFile(APP_USER_CONFIG_FILENAME, optional: true);
            // Allows overriding any setting using envVars that being with TUG_CFG_
            appConfigBuilder.AddEnvironmentVariables(prefix: APP_CONFIG_ENV_PREFIX);

            if (args != null)
            {
                // Last but not least, allow overriding with CLI arguments
                appConfigBuilder.AddCommandLine(args);
            }

            return appConfigBuilder.Build();
        }

        protected void ConfigureLogging(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var logSettings = _config
                .GetSection(nameof(LogSettings))
                .Get<LogSettings>();

            _logger.LogInformation("Applying logging configuration");

            if (logSettings.LogType.HasFlag(LogType.Console)) {
                _logger.LogInformation("  * enabling Console Logging");
                if (logSettings.DebugLog) {
                    loggerFactory.AddConsole(LogLevel.Debug);
                } else {
                    loggerFactory.AddConsole(LogLevel.Information);
                }
            }

            if (logSettings.LogType.HasFlag(LogType.NLog)) {
                var configPath = Path.Combine(Directory.GetCurrentDirectory(), "nlog.config");
                _logger.LogInformation($"  * enabling NLog with config=[{configPath}]");
                loggerFactory.AddNLog();
                env.ConfigureNLog(configPath);
            }

            // Initiate and switch to runtime logging
            _logger.LogInformation("Instantiating runtime logging");
            _logger.LogInformation("***** PRE-logging will cease *****");
            _logger = loggerFactory.CreateLogger<Startup>();
            _logger.LogInformation("Commencing runtime logging");
        }
    }
}